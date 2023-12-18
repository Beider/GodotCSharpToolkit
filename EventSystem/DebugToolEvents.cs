using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using GodotCSharpToolkit.DebugMenu;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.EventSystem.Events;

namespace GodotCSharpToolkit.EventSystem
{
    /// <summary>
    /// Simple tool that scans for new RecordableEvents on startup.
    /// If any are found it is added to our JSON file that lists all RecordableEvents.
    /// We do this to ensure each RecordableEvent has a consistent unique ID.
    /// </summary>
    public partial class DebugToolEvents : IDebugTool
    {
        private List<Type> NewEvents = new List<Type>();

        private EventIdJsonFile FileContent;

        /// <summary>
        /// Called by DebugMenu
        /// </summary>
        public void Initialize()
        {
            string absPath = Utils.GetAbsoluteProjectPath();
            string dataPath = Utils.GetRelativeDataPath();
            if (absPath == "" || dataPath == "")
            {
                Logger.Error("Data or absolute path is null, did you load the toolkit addon?");
                return;
            }
            string path = "res://" + dataPath + Constants.EVENT_ID_FILE_NAME;
            string fullPath = absPath + dataPath + Constants.EVENT_ID_FILE_NAME;

            LoadExistingEvents(path);
            CheckEvents();
            SaveEvents(fullPath);
        }

        /// <summary>
        /// Get all existing registered events from JSON
        /// </summary>
        private void LoadExistingEvents(string path)
        {
            FileContent = new EventIdJsonFile();
            string fileContent = FileUtils.LoadTextFile(path);
            if (!String.IsNullOrEmpty(fileContent))
            {
                FileContent = (EventIdJsonFile)Utils.FromJson(fileContent, typeof(EventIdJsonFile));
            }
            else
            {
                FileContent = new EventIdJsonFile();
                FileContent.Values = new List<EventIdJsonDef>();
            }
        }

        /// <summary>
        /// Check all events in assembly to see if they exist in JSON
        /// </summary>
        private void CheckEvents()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && typeof(RecordableEvent).IsAssignableFrom(type))
                {
                    CheckEventExists(type);
                }
            }
        }

        /// <summary>
        /// check if the individual event type exists in JSON, if not add it to NewEvents list
        /// </summary>
        /// <param name="type">The type to check</param>
        private void CheckEventExists(Type type)
        {
            foreach (var value in FileContent.Values)
            {
                if (value.ClassName.Equals(type.Name))
                {
                    return;
                }
            }
            NewEvents.Add(type);
        }

        /// <summary>
        /// Save events if any new events were found
        /// </summary>
        private void SaveEvents(string path)
        {
            Logger.Info($"- {NewEvents.Count} new events found");
            if (NewEvents.Count == 0)
            {
                return;
            }

            byte currentId = GetMaxId();

            foreach (Type evnt in NewEvents)
            {
                currentId++;
                Logger.Info($"- Added Event ({currentId}): {evnt.Name}");
                var eventId = new EventIdJsonDef();
                eventId.Id = currentId;
                eventId.ClassName = evnt.Name;
                FileContent.Values.Add(eventId);
            }

            string json = Utils.ToJson(FileContent);
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Get max ID currently in use
        /// </summary>
        private byte GetMaxId()
        {
            byte id = 0;
            foreach (var value in FileContent.Values)
            {
                if (value.Id > id)
                {
                    id = value.Id;
                }
            }
            return id;
        }

    }
}
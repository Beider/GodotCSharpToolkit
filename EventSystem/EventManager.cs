using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.EventSystem.Events;
using GodotCSharpToolkit.EventSystem.Recorders;
using GodotCSharpToolkit.EventSystem.Providers;
using GodotCSharpToolkit.DataManager;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.EventSystem
{
    public partial class EventManager : Node
    {
        public static EventManager Instance;
        public static readonly int POS_TICK = 0;
        public static readonly int POS_SEQUENCE = 1;
        public static readonly int POS_WIE_ID = 2;

        private List<EventRecorder> Recorders = new List<EventRecorder>();
        private EventProvider Provider = null;

        private Dictionary<byte, Type> IdToTypeList = new Dictionary<byte, Type>();
        private Dictionary<Type, byte> TypeToIdList = new Dictionary<Type, byte>();

        private Dictionary<Type, IEventReciever> EventRecievers = new Dictionary<Type, IEventReciever>();

        // This is not thread safe
        private ulong CurrentSequence = 0;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Instance = this;
            string dataPath = Utils.GetRelativeDataPath();
            if (dataPath == "")
            {
                Logger.Error("Data path is null, did you load the toolkit addon?");
                return;
            }
            string path = "res://" + dataPath;
            try
            {
                JsonDataManager.LoadJsonFile<EventIdJsonFile, EventIdJsonDef>(Constants.EVENT_SYSTEM_JSON_KEY, path, false);
                RegisterEvents();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to register events", ex);
            }
        }

        public static void StopAllRecorders()
        {
            Instance.Recorders.ForEach(r => r.Stop());
        }

        /// <summary>
        /// Starts a new file recording
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="settings">A list of settings to record</param>
        public static void StartFileRecording(string path, List<String> settings)
        {
            Instance.Reload();
            Instance.Recorders.Add(new FileEventRecorder(path, settings));
            Logger.Info("Recording started");
        }

        /// <summary>
        /// Start playback from disk
        /// </summary>
        /// <param name="path">The path to the file to load (godot relative path)</param>
        /// <param name="settingsMethod">A method callback to handle settings, or null if you don't want settings</param>
        /// <returns></returns>
        public static void StartFilePlayback(string path, Func<List<string>, String> settingsMethod)
        {
            Instance.Reload();
            Instance.Provider = new FileEventProvider();
            string value = ((FileEventProvider)Instance.Provider).Load(path, settingsMethod);
            if (value != null)
            {
                Logger.Error($"Playback could not start: {value}");
            }
            else
            {
                Logger.Info("Playback started");
            }
        }

        /// <summary>
        /// Register someone to get events of a given type
        /// </summary>
        /// <param name="eventType">The type of event</param>
        /// <param name="reciever">The reciever to get the event</param>
        /// <param name="overrideExisting">Override any existing recievers</param>
        public static void RegisterEventReciever(Type eventType, IEventReciever reciever, bool overrideExisting = false)
        {
            if (Instance.EventRecievers.ContainsKey(eventType))
            {
                if (overrideExisting)
                {
                    Instance.EventRecievers.Remove(eventType);
                }
                else
                {
                    Logger.Error($"Event reciever already exist for type {eventType.Name}");
                    return;
                }
            }

            Instance.EventRecievers.Add(eventType, reciever);
        }

        /// <summary>
        /// Register all world interaction events we know about
        /// </summary>
        private void RegisterEvents()
        {
            List<Type> typeList = new List<Type>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(RecordableEvent).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    typeList.Add(type);
                }
            }

            // Add types to list
            typeList.ForEach(type => GetTypeId(type));
        }

        public void Reload()
        {
            Instance.CurrentSequence = 0;
            Instance.Recorders.ForEach(r => r.Stop());
            Instance.Recorders.Clear();

            if (Instance.Provider != null)
            {
                Instance.Provider.Stop();
                Instance.Provider = null;
            }
        }

        public static ulong GetNextSequenceNumber()
        {
            Instance.CurrentSequence++;
            return Instance.CurrentSequence;
        }

        public static void SendEvent(RecordableEvent rEvent, bool setSender = true)
        {
            // Add our peer ID if this event originates from our client
            if (setSender)
            {
                rEvent.Sender = Instance.GetPeerId();
            }

            // Send to all recorders (they will figure out if it should be stored or not)
            Instance.Recorders.ForEach(r => r.RecordEvent(rEvent));

            // Propigate event
            bool recieverFound = false;
            foreach (Type type in Instance.EventRecievers.Keys)
            {
                if (type.IsAssignableFrom(rEvent.GetType()))
                {
                    recieverFound = true;
                    Instance.EventRecievers[type].HandleEvent(rEvent);
                    break;
                }
            }

            // Log problems
            if (!recieverFound)
            {
                Logger.Error($"No reciever found for event: {SerializeEvent(rEvent)}");
            }

            // Log
            //Logger.Error($"{rEvent.Tick} - {rEvent.Sequence} - {rEvent.GetType().ToString()} - {rEvent.Serialize()}");
        }

        public static Type GetTypeFromId(byte id)
        {
            if (!Instance.IdToTypeList.ContainsKey(id))
            {
                Logger.Error($"Event with id {id} not found");
                return null;
            }
            return Instance.IdToTypeList[id];
        }

        public static byte GetTypeId(Type type)
        {
            if (!Instance.TypeToIdList.ContainsKey(type))
            {
                var evnt = JsonDataManager.GetByName<EventIdJsonDef>(Constants.EVENT_SYSTEM_JSON_KEY, type.Name);
                if (evnt == null)
                {
                    Logger.Error($"Event not found: {type.Name}");
                    return 0;
                }
                if (Instance.IdToTypeList.ContainsKey(evnt.Id))
                {
                    Logger.Error($"Duplicate event ID registered: {evnt.Id} - {type.Name}");
                    return 0;
                }
                Instance.TypeToIdList.Add(type, evnt.Id);
                Instance.IdToTypeList.Add(evnt.Id, type);
            }

            return Instance.TypeToIdList[type]; ;
        }

        /// <summary>
        /// Serializes the event to a string
        /// </summary>
        /// <param name="evnt">The event to serialize</param>
        /// <returns>A string representing the event</returns>
        public static string SerializeEvent(RecordableEvent evnt)
        {
            return $"{evnt.Tick}{Constants.SEPARATOR}{evnt.Sequence}{Constants.SEPARATOR}{GetTypeId(evnt.GetType())}{Constants.SEPARATOR}{evnt.Serialize()}";
        }

        public static RecordableEvent DeserializeEvent(string line)
        {
            return DeserializeEvent(line.Split(Constants.SEPARATOR));
        }

        public static RecordableEvent DeserializeEvent(string[] splitLine)
        {
            // Extract common data
            ulong tick = ulong.Parse(splitLine[POS_TICK]);
            ulong sequence = ulong.Parse(splitLine[POS_SEQUENCE]);
            Type type = GetTypeFromId(byte.Parse(splitLine[POS_WIE_ID]));

            // Create a new array with only what was serialized from the original event
            string[] wie_parameter_array = new string[splitLine.Length - 3];
            Array.Copy(splitLine, 3, wie_parameter_array, 0, wie_parameter_array.Length);

            // Create the event with reflection
            var evnt = Activator.CreateInstance(type) as RecordableEvent;
            evnt.Tick = tick;
            evnt.Sequence = sequence;
            // TODO: Surround with try/catch and add error handling
            evnt.Deserialize(wie_parameter_array);

            return evnt;
        }
    }
}
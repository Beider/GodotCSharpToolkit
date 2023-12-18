using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorPrefsStorage
    {
        public const string DEFAULT_PATH = "user://gd_toolkit_editor_prefs.json";

        public bool HasChanges { get; set; } = false;

        private string Path3D = DEFAULT_PATH;

        private Dictionary<string, EditorPrefEntry> Entries = new Dictionary<string, EditorPrefEntry>();

        public EditorPrefsStorage(string path = null)
        {
            if (!path.IsNullOrEmpty())
            {
                this.Path3D = path;
            }

            Load();
        }

        /// <summary>
        /// Check if the setting exists
        /// </summary>
        public bool SettingExists(string name)
        {
            return Entries.ContainsKey(name);
        }

        /// <summary>
        /// Return the setting or default value if not found
        /// </summary>
        public string GetValue(string name, string defaultValue = "")
        {
            if (SettingExists(name))
            {
                return Entries[name].Value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets the setting and saves if autosave is on
        /// </summary>
        public void SetValue(string name, string value)
        {
            if (SettingExists(name))
            {
                Entries[name].Value = value;
            }
            else
            {
                var pref = new EditorPrefEntry();
                pref.Name = name;
                pref.Value = value;
                Entries.Add(name, pref);
            }
            HasChanges = true;
        }

        private void Load()
        {
            Entries.Clear();
            var text = FileUtils.LoadTextFile(Path3D);
            if (text != "")
            {
                var prefs = (EditorPrefsJson)Utils.FromJson(text, typeof(EditorPrefsJson));
                foreach (var pref in prefs.Values)
                {
                    Entries.Add(pref.Name, pref);
                }
            }
            else
            {
                Logger.Warning($"Could not load editor preferences, creating new preferences");
            }
        }

        public void Save()
        {
            if (!HasChanges) { return; }
            HasChanges = false;
            try
            {
                EditorPrefsJson file = new EditorPrefsJson();
                file.Values = new List<EditorPrefEntry>(Entries.Values);
                var json = Utils.ToJson(file);
                FileUtils.SaveToFile(json, Path3D);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save editor preferences", ex);
            }
        }
    }
}
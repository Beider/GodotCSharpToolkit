using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.EventSystem.Events
{

    /// <summary>
    /// An event that works on metadata. Good for quickly setting up new events
    /// but not very network efficient or fast.
    /// 
    /// Use the prefixes to decide how metadata is handled.
    /// Godot type: will use str2var and var2str for serialization (Vector2, Vector3, etc...)
    /// Custom type: will call the DeserializeCustom and SerializeCustom methods, implement yourself
    /// Default: We assume it is a string, will use .ToString()
    /// Null values will be serialized to "" for godot types and default
    /// </summary>
    public abstract class MetadataEvent : RecordableEvent
    {
        protected const string GODOT_TYPE_PREFIX = "GD_";
        protected const string CUSTOM_TYPE_PREFIX = "C_";

        private Dictionary<String, object> Metadata = new Dictionary<string, object>();

        public MetadataEvent()
        {

        }

        /// <summary>
        /// Store some metadata
        /// </summary>
        public void AddMetadata(string key, object value)
        {
            SetValue(key, value);
        }

        /// <summary>
        /// Get the value or null if key does not exist
        /// </summary>
        public object GetMetadata(string key)
        {
            if (Metadata.ContainsKey(key))
            {
                return Metadata[key];
            }
            return null;
        }

        /// <summary>
        /// Override with your own implementation
        /// </summary>
        protected virtual object DeserializeCustom(string key, string value)
        {
            return null;
        }

        /// <summary>
        /// Override with your own implementation
        /// </summary>
        protected virtual string SerializeCustom(string key, object value)
        {
            return "";
        }

        private void DeserializeValue(string key, string value)
        {
            // Handle custom
            if (key.StartsWith(CUSTOM_TYPE_PREFIX))
            {
                SetValue(key, DeserializeCustom(key, value));
            }

            // Handle null
            if (value == "")
            {
                SetValue(key, "");
            }

            // Godot type
            if (key.StartsWith(GODOT_TYPE_PREFIX))
            {
                SetValue(key, GD.Str2Var(value));
            }

            // Default
            SetValue(key, value);
        }

        private String SerializeValue(string key)
        {
            // Handle custom
            if (key.StartsWith(CUSTOM_TYPE_PREFIX))
            {
                return SerializeCustom(key, Metadata[key]);
            }

            // Handle null
            if (Metadata[key] == null)
            {
                return "";
            }

            // Godot type
            if (key.StartsWith(GODOT_TYPE_PREFIX))
            {
                return GD.Var2Str(Metadata[key]);
            }

            // Default
            return Metadata[key].ToString();
        }

        private void SetValue(string key, object value)
        {
            if (Metadata.ContainsKey(key))
            {
                Metadata.Remove(key);
            }
            Metadata.Add(key, value);
        }

        public override String Serialize()
        {
            string returnValue = "";
            foreach (string key in Metadata.Keys)
            {
                try
                {
                    returnValue += $"{key}{Constants.SEPARATOR}{SerializeValue(key)}";
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to serialize", ex);
                }
            }
            return returnValue;
        }

        public override int Deserialize(string[] list, int pos = 0)
        {
            // Just consume everything in the list
            for (int i = pos; i < list.Length; i += 2)
            {
                try
                {
                    string key = list[i];
                    string value = list[i + 1];
                    DeserializeValue(key, value);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to deserialize", ex);
                }
            }

            return list.Length;
        }

        protected T MetadataStringToEnum<T>(string metadataKey) where T : struct
        {
            string value = GetMetadata(metadataKey) as String;
            T enumVal;
            Enum.TryParse<T>(value, out enumVal);

            return enumVal;
        }
    }
}
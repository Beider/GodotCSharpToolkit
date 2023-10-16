using Godot;
using System;
using System.Globalization;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorPrefsStorage
    {
        public void SetValue(string name, Color value)
        {
            SetValue(name, GD.Var2Str(value));
        }

        public Color GetValue(string name, Color defaultValue)
        {
            if (SettingExists(name))
            {
                return (Color)GD.Str2Var(GetValue(name));
            }
            return defaultValue;
        }

        public void SetValue(string name, bool value)
        {
            SetValue(name, value.ToString());
        }

        public bool GetValue(string name, bool defaultValue)
        {
            if (SettingExists(name))
            {
                bool value = defaultValue;
                if (Boolean.TryParse(GetValue(name), out value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        public void SetValue(string name, int value)
        {
            SetValue(name, value.ToString());
        }

        public int GetValue(string name, int defaultValue)
        {
            if (SettingExists(name))
            {
                int value = defaultValue;
                if (int.TryParse(GetValue(name), out value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        public void SetValue(string name, float value)
        {
            SetValue(name, value.ToString());
        }

        public float GetValue(string name, float defaultValue)
        {
            if (SettingExists(name))
            {
                float value = defaultValue;
                if (float.TryParse(GetValue(name), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        public void SetValue(string name, Vector2 value)
        {
            SetValue(name, GD.Var2Str(value));
        }

        public Vector2 GetValue(string name, Vector2 defaultValue)
        {
            if (SettingExists(name))
            {
                object var = GD.Str2Var(GetValue(name));
                if (var is Vector2 vec)
                {
                    return vec;
                }
            }
            return defaultValue;
        }
    }
}

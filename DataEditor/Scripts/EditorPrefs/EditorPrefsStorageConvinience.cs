using Godot;
using System;

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
    }
}

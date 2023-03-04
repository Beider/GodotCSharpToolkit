using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorPrefsStorage
    {
        public void SetColorValue(string name, Color value)
        {
            SetValue(name, GD.Var2Str(value));
        }

        public Color GetColorValue(string name, Color defaultValue)
        {
            if (SettingExists(name))
            {
                return (Color)GD.Str2Var(GetValue(name));
            }
            return defaultValue;
        }

        public void SetBoolValue(string name, bool value)
        {
            SetValue(name, value.ToString());
        }

        public bool GetBoolValue(string name, bool defaultValue)
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
    }
}

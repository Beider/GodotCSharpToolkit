using System;
using System.Reflection;
using System.Collections.Concurrent;
using Godot;

namespace GodotCSharpToolkit.DebugMenu
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public partial class OnScreenDebug : Attribute
    {
        private static ConcurrentDictionary<string, Color> ColorLookupDictionary = new ConcurrentDictionary<string, Color>();
        public readonly string DebugCategory;
        public readonly string Name;
        public readonly Color Color;

        public static void Init()
        {
            if (ColorLookupDictionary.Count > 0)
            {
                return;
            }

            foreach (PropertyInfo info in typeof(Colors).GetProperties())
            {
                // Doing try here as this could cause problems when using threading
                ColorLookupDictionary.TryAdd(info.Name.ToLower(), (Color)info.GetValue(null, null));
            }
        }

        public OnScreenDebug(string debugCategory, string name, string color = "White")
        {
            this.DebugCategory = debugCategory;
            this.Name = name;
            this.Color = GetColor(color);
        }

        public static Color GetColor(string color)
        {
            string colorLower = color.ToLower();
            if (ColorLookupDictionary.Count == 0)
            {
                Init();
            }
            if (ColorLookupDictionary.ContainsKey(colorLower))
            {
                return ColorLookupDictionary[colorLower];
            }

            return Colors.White;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public partial class DebugIncludeClass : Attribute
    {
        public DebugIncludeClass() { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public partial class DebugCategoryColumn : Attribute
    {
        public readonly string Category;
        public readonly int Column;

        public DebugCategoryColumn(string category, int column)
        {
            this.Category = category;
            this.Column = Math.Min(4, Math.Max(1, column));
        }
    }

    public abstract class DebugMenuEntry : Attribute
    {
        public readonly string Category;
        public readonly string ButtonText;
        public readonly Color ButtonColor;
        public readonly bool CloseOnClick = false;
        public readonly int DialogId;

        public DebugMenuEntry(string category, string text, string buttonColor, bool closeOnClick = false, int dialogId = 0)
        {
            this.Category = category;
            this.ButtonText = text;
            this.ButtonColor = OnScreenDebug.GetColor(buttonColor);
            this.CloseOnClick = closeOnClick;
            this.DialogId = dialogId;
        }
    }
}
using System;
using DebugMenu;
using System.Reflection;
using System.Collections.Generic;
using Godot;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class OnScreenDebug : Attribute
{
    private static Dictionary<string, Color> ColorLookupDictionary = new Dictionary<string, Color>();
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
            ColorLookupDictionary.Add(info.Name.ToLower(), (Color)info.GetValue(null, null));
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
public class DebugIncludeClass : Attribute
{
    public DebugIncludeClass() { }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class DebugCategoryColumn : Attribute
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
using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Misc
{
    public static class Constants
    {
        //
        // For event system
        //
        public static readonly string SEPARATOR = "#";
        public static readonly string END_OF_SETTINGS = "EOS";
        public static readonly string EVENT_SYSTEM_JSON_KEY = "EVENT_SYSTEM_JSON";
        public const string EVENT_ID_FILE_NAME = "GCSTK_EventIds.json";

        //
        // Project settings
        //

        public static readonly string SETTING_PATH_NAME = "csharptoolkit/absolute_project_path";
        public static readonly string SETTING_DATA_PATH = "csharptoolkit/relative_data_path";
        public static readonly string DEFAULT_SETTING_DATA_PATH = "Data/Toolkit/";

    }

    //
    // Enums
    //

    /// <summary>
    /// Directional enum for 8-way movement. Has related extensions to convert into Vector2 or Angles.
    /// </summary>
    public enum Direction
    {
        None,
        Up,
        UpRight,
        Right, // Forward
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }
}
using Godot;
using System.Collections.Generic;
using System.Globalization;

namespace GodotCSharpToolkit.DebugMenu
{

    /// <summary>
    /// Class taken from MDFramework
    /// https://github.com/DoubleDeez/MDFramework
    /// </summary>
    internal class OnScreenDebugInfo
    {
        public OnScreenInfoFunction InfoFunction;

        public Color Color;

        public OnScreenDebugInfo(OnScreenInfoFunction InfoFunction, Color Color)
        {
            this.InfoFunction = InfoFunction;
            this.Color = Color;
        }

        public OnScreenDebugInfo(OnScreenInfoFunction InfoFunction) : this(InfoFunction, Colors.White)
        {
        }
    }

    public delegate string OnScreenInfoFunction();

    /// <summary>
    /// Class taken from MDFramework
    /// https://github.com/DoubleDeez/MDFramework
    /// </summary>
    public static class OnScreenDebugManager
    {
        private const string BASIC_DEBUG_CAT = "Basic Info";
        internal static List<string> HiddenCategories { get; private set; } = new List<string>();
        internal static Dictionary<string, Dictionary<string, OnScreenDebugInfo>> DebugInfoCategoryMap { get; private set; } = new Dictionary<string, Dictionary<string, OnScreenDebugInfo>>();

        internal static void Initialize()
        {
            AddBasicInfo();
        }

        /// <summary>
        /// Adds some basic information on creation, can be toggled in config
        /// </summary>
        public static void AddBasicInfo()
        {
            AddOnScreenDebugInfo(BASIC_DEBUG_CAT, "FPS", () => Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture), Colors.Cyan);
        }

        /// <summary>Adds some info to print on the screen</summary>
        /// <param name="DebugCategory">The category to display, should be unique.</param>
        /// <param name="Name">The name to display, should be unique to the category.</param>
        /// <param name="InfoFunction">Function that returns a string to display on the screen.</param>
        public static void AddOnScreenDebugInfo(string DebugCategory, string Name, OnScreenInfoFunction InfoFunction)
        {
            AddOnScreenDebugInfo(DebugCategory, Name, InfoFunction, Colors.White);
        }

        /// <summary>Adds some info to print on the screen</summary>
        /// <param name="DebugCategory">The category to display, should be unique.</param>
        /// <param name="Name">The name to display, should be unique to the category.</param>
        /// <param name="InfoFunction">Function that returns a string to display on the screen.</param>
        /// <param name="Color">Function that returns a string to display on the screen.</param>
        public static void AddOnScreenDebugInfo(string DebugCategory, string Name, OnScreenInfoFunction InfoFunction, Color Color)
        {
            Dictionary<string, OnScreenDebugInfo> DebugInfoList = null;
            if (DebugInfoCategoryMap.ContainsKey(DebugCategory))
            {
                DebugInfoList = DebugInfoCategoryMap[DebugCategory];
            }
            else
            {
                DebugInfoList = new Dictionary<string, OnScreenDebugInfo>();
                DebugInfoCategoryMap.Add(DebugCategory, DebugInfoList);
            }

            if (DebugInfoList.ContainsKey(Name))
            {
                DebugInfoList[Name].InfoFunction = InfoFunction;
                DebugInfoList[Name].Color = Color;
            }
            else
            {
                DebugInfoList.Add(Name, new OnScreenDebugInfo(InfoFunction, Color));
            }
        }

        /// <summary>
        /// Removes the information from the on screen debug
        /// </summary>
        /// <param name="DebugCategory">The category to remove</param>
        /// <returns>True if we removed it, false if not</returns>
        public static bool RemoveOnScreenDebugInfo(string DebugCategory)
        {
            return DebugInfoCategoryMap.Remove(DebugCategory);
        }

        /// <summary>
        /// Removes the information from the on screen debug
        /// </summary>
        /// <param name="DebugCategory">The category to remove</param>
        /// <param name="name">The name to remove</param>
        /// <returns>True if we removed it, false if not</returns>
        public static bool RemoveOnScreenDebugInfo(string DebugCategory, string name)
        {
            if (DebugInfoCategoryMap.ContainsKey(DebugCategory))
            {
                return DebugInfoCategoryMap[DebugCategory].Remove(name);
            }

            return false;
        }

        public static void ToggleHideDebugCategory(string DebugCategory)
        {
            if (HiddenCategories.Contains(DebugCategory))
            {
                HiddenCategories.Remove(DebugCategory);
            }
            else
            {
                HiddenCategories.Add(DebugCategory);
            }
        }
    }
}
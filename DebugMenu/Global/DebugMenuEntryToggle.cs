using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.DebugMenu
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DebugMenuEntryToggle : DebugMenuEntry
    {
        public readonly string GetValueMethodName;

        /// <summary>
        /// Simple toggle button, will call the method with a single true/false parameter and expects it to return the new value.
        /// </summary>
        /// <param name="category">The category this belongs in</param>
        /// <param name="buttonText">The text on the button</param>
        /// <param name="getValueMethod">A method to get the current value, should return a bool</param>
        public DebugMenuEntryToggle(string category, string buttonText, string getValueMethod)
        : base(category, buttonText, "Green", false)
        {
            this.GetValueMethodName = getValueMethod;
        }

        /// <summary>
        /// Calls the get value method on the object
        /// </summary>
        public bool CallGetValueMethod(object obj)
        {
            try
            {
                return (bool)obj.GetType().GetMethod(GetValueMethodName).Invoke(obj, null);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get bool value", ex);
            }
            return false;
        }
    }
}
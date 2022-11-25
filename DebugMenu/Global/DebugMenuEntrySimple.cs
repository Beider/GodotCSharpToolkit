using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.DebugMenu
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DebugMenuEntrySimple : DebugMenuEntry
    {
        public readonly object[] Parameters;

        /// <summary>
        /// Simple action used to add something to the debug menu. Will just call the method when clicked
        /// </summary>
        /// <param name="category">The category this belongs in</param>
        /// <param name="buttonText">The text on the button</param>
        /// <param name="buttonColor">The color of the button</param>
        /// <param name="closeOnClick">If true this will close after it has been clicked</param>
        /// <param name="dialogId">If 0 not a dialog, if set this will be a dialog</param>
        /// <param name="parameters">The parameters to pass to the method</param>
        public DebugMenuEntrySimple(string category, string buttonText, string buttonColor,
        bool closeOnClick, int dialogId, params object[] parameters) : base(category, buttonText, buttonColor, closeOnClick, dialogId)
        {
            this.Parameters = parameters;
        }
    }
}
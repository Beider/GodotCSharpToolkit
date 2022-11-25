using Godot;
using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class DebugMenuEntryCallback : DebugMenuEntry
{
    public readonly object[] Parameters;
    public readonly string TextCallback;
    public readonly string ColorCallback;

    /// <summary>
    /// Simple action used to add something to the debug menu. Will just call the method when clicked
    /// </summary>
    /// <param name="category">The category this belongs in</param>
    /// <param name="callbackMethodText">The method to call to get the text</param>
    /// <param name="callbackMethodColor">The method to call to get the value</param>
    /// <param name="closeOnClick">If true this will close after it has been clicked</param>
    /// <param name="dialogId">If 0 not a dialog, if set this will be a dialog</param>
    /// <param name="parameters">The parameters to pass to the method</param>
    public DebugMenuEntryCallback(string category, string callbackMethodText, string callbackMethodColor,
    bool closeOnClick, int dialogId, params object[] parameters) : base(category, "", "Red", closeOnClick, dialogId)
    {
        this.Parameters = parameters;
        this.TextCallback = callbackMethodText;
        this.ColorCallback = callbackMethodColor;
    }
}
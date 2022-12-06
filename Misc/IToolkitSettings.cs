using Godot;
using System;

/// <summary>
/// Used to get configuration from the project without having to change things inside the toolkit.
/// </summary>
namespace GodotCSharpToolkit.Misc
{
    public interface IToolkitSettings
    {
        /// <summary>
        /// Should return the absolute path to your project directory
        /// eg. C:/Coding/<my game folder>/"
        /// </summary>
        string GetProjectAbsolutePath();

        /// <summary>
        /// Path where we should store data files (relative to the absolute path)
        /// eg. Data/Toolkit/
        /// The main thing stored here will be event IDs for the event system
        /// </summary>
        string GetDataPath();
    }
}
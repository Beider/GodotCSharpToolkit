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
        /// </summary>
        string GetProjectAbsolutePath();

        /// <summary>
        /// Path where we should store data files
        /// </summary>
        string GetDataPath();
    }
}
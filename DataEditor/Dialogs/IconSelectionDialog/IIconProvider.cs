using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    public interface IIconProvider
    {
        /// <summary>
        /// Searches for a string in icon names. Can send in blank string to clear
        /// </summary>
        void Search(string searchString);

        /// <summary>
        /// Get the given page
        /// </summary>
        /// <param name="number">The page number</param>
        Dictionary<string, Texture2D> GetPage(int number);

        /// <summary>
        /// Get number of pages
        /// </summary>
        /// <returns>The number of pages</returns>
        int GetPageCount();

        /// <summary>
        /// Get the given icon
        /// </summary>
        Texture2D GetIcon(string key);
    }
}
using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// This must be implemented to make the editor work
    /// Should return root items along with the paths to look for things for that root item.
    /// This can be used to split your project into mods
    /// </summary>
    public abstract class AbstractEditorTreeModFolderProvider
    {

        /// <summary>
        /// Should return a list of root items.
        /// Values are name + a list of paths to look for items for this root item
        /// </summary>
        public abstract Dictionary<string, List<string>> GetModFolders();
    }
}
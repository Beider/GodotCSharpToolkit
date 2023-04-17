using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Root item for a subsection under a mod. These are created for each module
    /// </summary>
    public abstract class AbstractEditorRootItem : AbstractEditorSavableItem
    {
        public AbstractEditorRootItem()
        {

        }

        /// <summary>
        /// Should return the root item
        /// </summary>
        public virtual TreeItem CreateRootItem()
        {
            return Editor.Tree.CreateTreeItem(Parent, this);
        }
    }
}
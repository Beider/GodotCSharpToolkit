using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Savable item, used for mods and delegate items.
    /// This does not mean that save is called just because you inherit from this.
    /// If you instance this yourself you need to make sure to call save as needed.
    /// </summary>
    public abstract class AbstractEditorSavableItem : AbstractEditorTreeItem
    {
        public AbstractEditorSavableItem()
        {

        }

        /// <summary>
        /// Caled when we should reload
        /// </summary>
        public virtual void Reload()
        {

        }

        /// <summary>
        /// Called to check if we have unsaved changes
        /// </summary>
        public virtual bool HasUnsavedChanges()
        {
            return false;
        }

        /// <summary>
        /// Called when we should save our data
        /// A reload will always be done after save so no need to refresh yourself
        /// </summary>
        public virtual void Save()
        {

        }
    }
}
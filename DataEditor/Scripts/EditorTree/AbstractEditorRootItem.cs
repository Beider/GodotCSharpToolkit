using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public abstract class AbstractEditorRootItem : AbstractEditorTreeItem
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
            return true;
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
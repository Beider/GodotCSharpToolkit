using Godot;
using System;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public abstract class AbstractEditorRootItem : AbstractEditorTreeItem
    {
        /// <summary>
        /// If this returns anything but an empty string or null
        /// this node will be put into a parent node with that name.
        /// Useful for grouping your editors into seperate categories
        /// </summary>
        public string Category { get; protected set; } = "";

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

        protected DelegateEditorTreeItem CreateDelegateTreeItem(TreeItem parent, string name, bool collapsed,
                    Color defaultColor, Color defaultBgColor, Action<DelegateEditorTreeItem> onSelection, object relatedData = null)
        {
            var newItem = new DelegateEditorTreeItem();
            newItem.Init(parent, Editor);
            newItem.Name = name;
            newItem.Collapsed = collapsed;
            newItem.Color = defaultColor;
            newItem.ColorBg = defaultBgColor;
            newItem.OnSelection = onSelection;
            newItem.RelatedData = relatedData;
            return newItem;
        }

    }
}
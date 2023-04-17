using Godot;
using System;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Delegate item can be used to implement simple tree items without creating a new class
    /// </summary>
    public class DelegateEditorTreeItem : AbstractEditorSavableItem
    {
        public Action<DelegateEditorTreeItem> OnSelection { get; set; } = null;
        public Action<DelegateEditorTreeItem> OnSave { get; set; } = null;
        public Action<DelegateEditorTreeItem> OnDispose { get; set; } = null;
        public Action<DelegateEditorTreeItem> OnReload { get; set; } = null;

        public Func<DelegateEditorTreeItem, bool> OnContextMenuFill { get; set; } = null;

        public Func<DelegateEditorTreeItem, bool> OnHasUnsavedChangedCheck { get; set; } = null;

        public object RelatedData;

        protected override void OnSelectItemRequest(string uniqueId)
        {
            if (uniqueId.Equals(Key))
            {
                OnItemSelected();
            }
        }

        public override void OnItemSelected()
        {
            if (OnSelection != null)
            {
                OnSelection(this);
            }
            else
            {
                Logger.Warning($"OnSelection delegate not set for tree item {Name}");
            }
        }

        public override bool FillContextMenu()
        {
            if (OnContextMenuFill != null)
            {
                return OnContextMenuFill(this);
            }
            return false;
        }

        public override void Reload()
        {
            if (OnReload != null)
            {
                OnReload(this);
            }
        }

        public override void Dispose()
        {
            if (OnDispose != null)
            {
                OnDispose(this);
            }
        }

        public override bool HasUnsavedChanges()
        {
            if (OnHasUnsavedChangedCheck != null)
            {
                return OnHasUnsavedChangedCheck(this);
            }
            return false;
        }

        public override void Save()
        {
            if (OnSave != null)
            {
                OnSave(this);
            }
        }
    }
}
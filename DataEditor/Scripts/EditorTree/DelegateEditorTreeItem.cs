using Godot;
using System;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Delegate item can be used to implement simple tree items without creating a new class
    /// </summary>
    public partial class DelegateEditorTreeItem : AbstractEditorSavableItem
    {
        public Action<DelegateEditorTreeItem> OnSelection { get; set; } = null;
        public Action<DelegateEditorTreeItem> OnSave { get; set; } = null;
        public Action<DelegateEditorTreeItem> OnDispose { get; set; } = null;
        public Action<DelegateEditorTreeItem> OnReload { get; set; } = null;

        public Func<DelegateEditorTreeItem, EditorPopupMenu, bool> OnContextMenuFill { get; set; } = null;

        public Func<DelegateEditorTreeItem, bool> OnHasUnsavedChangedCheck { get; set; } = null;

        public object RelatedData;
        public string TreeItemName { get; set; } = "";
        public string TreeItemIcon { get; set; } = "";

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

        public override bool FillContextMenu(EditorPopupMenu menu)
        {
            if (OnContextMenuFill != null)
            {
                return OnContextMenuFill(this, menu);
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
            base.Dispose();
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

        public void UpdateSettings(string key, string name, string icon)
        {
            Key = key;
            TreeItemName = name;
            TreeItemIcon = icon;
        }
    }
}
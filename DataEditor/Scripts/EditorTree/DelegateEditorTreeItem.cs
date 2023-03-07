using Godot;
using System;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public class DelegateEditorTreeItem : AbstractEditorTreeItem
    {
        public Action<DelegateEditorTreeItem> OnSelection { get; set; } = null;

        public Func<DelegateEditorTreeItem, bool> OnContextMenuFill { get; set; } = null;

        public object RelatedData;

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
    }
}
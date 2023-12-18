using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class FolderManagerTree : Tree
    {
        public FolderManager FolderManager { get; set; } = null;

        public FolderManagerTree()
        {
            this.Connect("item_collapsed", new Callable(this, nameof(OnCollapsed)));
        }

        private void OnCollapsed(TreeItem treeItem)
        {
            string key = (string)treeItem.GetMetadata(0);
            var item = FolderManager.ItemLookup[key];
            item.Collapsed = treeItem.Collapsed;

        }


        public override Variant _GetDragData(Vector2 position)
        {
            Godot.Collections.Array arry = new Godot.Collections.Array();
            TreeItem current = GetNextSelected(null);
            if (current == null) { return new Variant(); }

            var first = GetData(current);
            if (!first.AllowMove) { return new Variant(); }

            while (current != null)
            {
                arry.Add(current);
                if (!first.Type.Equals(GetData(current).Type))
                {
                    // We cant drag different types at the same time
                    return new Variant();
                }
                current = GetNextSelected(current);
            }
            if (arry.Count == 0) { return new Variant(); }

            DropModeFlags = (int)DropModeFlagsEnum.Inbetween;

            Label preview = new Label();
            preview.Text = $"{arry.Count} items";
            SetDragPreview(preview);

            return arry;
        }

        private FolderManagerTreeItem GetData(TreeItem treeItem)
        {
            string key = (string)treeItem.GetMetadata(0);
            return FolderManager.ItemLookup[key];
        }

        public override bool _CanDropData(Vector2 position, Variant data)
        {

            if (data.VariantType != Variant.Type.Array)
            {
                DropModeFlags = (int)DropModeFlagsEnum.Disabled;
                return false;
            }
            TreeItem treeItem = GetItemAtPosition(position);
            if (treeItem == null)
            {
                DropModeFlags = (int)DropModeFlagsEnum.Disabled;
                return false;
            }

            var arry = (Godot.Collections.Array)data;
            var targetItem = GetData(treeItem);
            var dropItem = GetData((TreeItem)arry[0]);

            var dropMode = targetItem.ShouldAcceptDropRequest(dropItem);
            DropModeFlags = (int)dropMode;
            return dropMode != DropModeFlagsEnum.Disabled;
        }

        public override void _DropData(Vector2 position, Variant data)
        {
            if (data.VariantType != Variant.Type.Array) { return; }
            TreeItem treeItem = GetItemAtPosition(position);
            if (treeItem == null) { return; }

            var targetItem = GetData(treeItem);

            var arry = (Godot.Collections.Array)data;
            var dropItem = GetData((TreeItem)arry[0]);

            targetItem = targetItem.GetDropTarget(dropItem);
            if (targetItem == null) { return; }

            foreach (var value in arry)
            {
                dropItem = GetData((TreeItem)value);
                dropItem.RemoveFromParent();
                targetItem.AddChild(dropItem);
            }

            FolderManager.Refresh();
        }
    }
}
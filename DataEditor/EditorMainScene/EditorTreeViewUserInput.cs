using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorTreeView : Tree
    {
        private void OnTreeItemSelected()
        {
            var item = GetAbstractTreeItem(GetSelected());
            if (item != null)
            {
                item.OnItemSelected();
            }
        }

        private void OnTreeItemCollapsed(TreeItem item)
        {
            if (InCode) { return; }
            var aItem = GetAbstractTreeItem(item);
            string text = aItem == null ? item.GetText(0) : aItem.Name;
            Editor.Preferences.SetTreeItemCollapsedState(item);
        }

        private void OnItemRmbSelected(Vector2 pos)
        {
            if (!Editor.Preferences.ShouldUseLocalPath()) { return; }
            if (GetSelected() == null) { return; }
            var data = GetSelected().GetMetadata(0);
            if (data == null) { return; }
            var item = TreeItemLookup.ContainsKey(data.ToString()) ? TreeItemLookup[data.ToString()] : null;
            if (item == null) { return; }

            var menu = Editor.PopupMenu;
            Editor.ClearPopupMenu();
            menu.RectSize = menu.RectMinSize;
            if (item.FillContextMenu())
            {
                menu.RectPosition = GetViewport().GetMousePosition();
                menu.Popup_();
            }
        }
    }
}
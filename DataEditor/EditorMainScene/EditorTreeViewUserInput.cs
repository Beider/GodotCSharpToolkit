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
            item.OnItemSelected();
        }

        private void OnTreeItemCollapsed(TreeItem item)
        {
            if (InCode) { return; }
            var aItem = GetAbstractTreeItem(item);
            string text = aItem == null ? item.GetText(0) : aItem.Name;
            Editor.Preferences.SetTreeItemCollapsedState(text, item.Collapsed);
        }
    }
}
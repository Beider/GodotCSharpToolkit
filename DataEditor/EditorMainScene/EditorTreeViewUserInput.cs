using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;

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

        private bool FillModContextMenu(string name)
        {
            Editor.AddPopupMenuSeparator(name);
            Editor.AddPopupMenuEntry($"Delete {name}", () => { DeleteMod(name); }, DataEditorConstants.ICON_DELETE);
            return true;
        }

        private void DeleteMod(string name)
        {
            if (Editor.Tree.HasUnsavedChanges())
            {
                Editor.ShowConfirmDialog($"Deleting a mod will cause you to lose any unsaved changes. Do you wish to save first?",
                                        shouldSave => { _DeleteMod(name, shouldSave); });
            }
            else
            {
                _DeleteMod(name, false);
            }

        }

        private void _DeleteMod(string name, bool save)
        {
            if (save)
            {
                Editor.Save();
            }
            Editor.ShowConfirmDialog($"Are you sure you wish to delte all local changes to '{name}'? This can not be undone.",
             shouldDelete => { if (shouldDelete) { __DeleteMod(name); } });
        }

        private void __DeleteMod(string name)
        {
            FileUtils.Delete($"{Editor.Preferences.SettingLocalSavePath}{name}", true);
            Editor.Refresh(false);
        }
    }
}
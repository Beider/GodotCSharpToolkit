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
            string text = item.GetText(0);
            Editor.Preferences.SetTreeItemCollapsedState(item);
        }

        private void OnItemRmbSelected(Vector2 pos, int buttonIndex)
        {
            if (!Editor.Preferences.ShouldUseLocalPath()) { return; }
            if (buttonIndex != (int)MouseButton.Right) { return; }
            if (GetSelected() == null) { return; }
            var item = GetAbstractTreeItem(GetSelected());
            if (item == null) { return; }

            var menu = DataEditorConstants.CreatePopupMenu(this);
            menu.Size = menu.MinSize;
            if (item.FillContextMenu(menu))
            {
                menu.PositionInParent(GetViewport().GetMousePosition());
                menu.Popup();
            }
        }

        private bool FillModContextMenu(string name, EditorPopupMenu menu)
        {
            menu.AddPopupMenuSeparator(name);
            menu.AddPopupMenuEntry($"Delete {name}", () => { DeleteMod(name); }, DataEditorConstants.ICON_DELETE);
            menu.AddPopupMenuEntry($"Add Feature", () => { AddModFeature(name); }, DataEditorConstants.ICON_NEW);
            return true;
        }

        private void AddModFeature(string modName)
        {
            Func<string, bool> nameValidator = (name) =>
                {
                    if (name.IsNullOrEmpty()) { return false; }
                    var normPath = FileUtils.NormalizePath($"{Editor.Preferences.SettingLocalSavePath}{modName}/{name}");
                    return !FileUtils.DirectoryExists(normPath);
                };
            Editor.ShowTextEntryDialog("Please enter the feature name", "Name", (s1, s2) => AddNewModuleFeature(s1, modName), nameValidator);
        }

        private void AddNewModuleFeature(string name, string module)
        {
            var path = FileUtils.NormalizePath($"{Editor.Preferences.SettingLocalSavePath}{module}/");
            FileUtils.CreateDirectory(path, name);
            Editor.Tree.RefreshTree(false);
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
             shouldDelete => { if (shouldDelete) { _DeleteMod(name); } else if (save) { Editor.Refresh(false); } });
        }

        private void _DeleteMod(string name)
        {
            FileUtils.Delete($"{Editor.Preferences.SettingLocalSavePath}{name}", true);
            Editor.Refresh(false);
        }

        private bool FillModFeatureContextMenu(string modname, string featureName, EditorPopupMenu menu)
        {
            menu.AddPopupMenuSeparator(featureName);
            menu.AddPopupMenuEntry($"Delete {featureName}", () => { DeleteModFeature(modname, featureName); }, DataEditorConstants.ICON_DELETE);
            return true;
        }

        private void DeleteModFeature(string modName, string featureName)
        {
            if (Editor.Tree.HasUnsavedChanges())
            {
                Editor.ShowConfirmDialog($"Deleting a mod feature will cause you to lose any unsaved changes. Do you wish to save first?",
                                        shouldSave => { _DeleteModFeature(modName, featureName, shouldSave); });
            }
            else
            {
                _DeleteModFeature(modName, featureName, false);
            }
        }

        private void _DeleteModFeature(string modName, string featureName, bool save)
        {
            if (save)
            {
                Editor.Save();
            }
            Editor.ShowConfirmDialog($"Are you sure you wish to delte all local changes to '{modName}/{featureName}'? This can not be undone.",
             shouldDelete => { if (shouldDelete) { _DeleteModFeature(modName, featureName); } else if (save) { Editor.Refresh(false); } });
        }

        private void _DeleteModFeature(string modName, string featureName)
        {
            FileUtils.Delete(FileUtils.NormalizeDirectory($"{Editor.Preferences.SettingLocalSavePath}{modName}/{featureName}"), true);
            Editor.Refresh(false);
        }
    }
}
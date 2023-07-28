using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorMainScene : Control, IDataEditor
    {
        public void ShowEditor(Control editorContent)
        {
            if (ActiveEditor != null && IsInstanceValid((Control)ActiveEditor))
            {
                ActiveEditor.Save();
                ActiveEditor.QueueFree();
                ActiveEditor = null;
            }

            if (editorContent == null) { return; }

            if (editorContent is IDataEditorContent iContent)
            {
                ActiveEditor = iContent;
                iContent.Init(this);
                EditorArea.AddChild(editorContent);
            }
            else
            {
                Logger.Warning($"Could not show editor since the root of the scene does not implement iContent: {editorContent.GetType()}");
            }
        }

        public void ShowConfirmDialog(string message, Action<bool> callback)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_CONFIRM.Instance() as DataEditorConfirmDialog;
            dialog.SetupBeforeAddChild(message, callback);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowTextEntryDialog(string title, string textName,
                    Action<string, string> callback, Func<string, bool> nameValidator,
                    string listName = "", Func<Dictionary<object, string>> getItemList = null)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_TEXT_ENTRY.Instance() as DataEditorTextEntryDialog;
            dialog.SetupBeforeAddChild(title, textName, callback, nameValidator, listName, getItemList);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowAdvancedListDialog(DataEditorAdvancedListDialogInput input)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_ADVANCED_LIST.Instance() as DataEditorAdvancedListDialog;
            dialog.Init(input, this);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowFolderManagerDialog()
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_FOLDER_MANAGER.Instance() as FolderManager;
            dialog.Init(this);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowGenericEditorDialog(GenericEditorDialogInput input)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_GENERIC_EDITOR.Instance() as GenericEditorDialog;
            dialog.Init(input, this);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ClearPopupMenu()
        {
            PopupMenu.Clear();
            PopupMenu.ClearChildren();
            PopupMenuDelegates.Clear();
        }

        public void AddPopupMenuSeparator(string name)
        {
            PopupMenu.AddSeparator($" {name} ");
        }

        public void AddPopupMenuEntry(string name, Action action, Texture icon = null, string subMenuName = "")
        {
            var menu = PopupMenu;
            if (!subMenuName.IsNullOrEmpty())
            {
                foreach (Control child in PopupMenu.GetChildren())
                {
                    if (child.Name.Equals(subMenuName))
                    {
                        menu = (PopupMenu)child;
                        break;
                    }
                }
            }
            if (icon != null)
            {
                menu.AddIconItem(icon, name);
            }
            else
            {
                menu.AddItem(name);
            }
            PopupMenuDelegates.Add(name, action);
        }

        public void CreatePopupSubMenu(string name)
        {
            PopupMenu subMenu = new PopupMenu();
            subMenu.Name = name;
            PopupMenu.AddChild(subMenu);
            PopupMenu.AddSubmenuItem(name, name);
        }

        private void OnPopupMenuPressed(int index)
        {
            string text = PopupMenu.GetItemText(index);
            if (PopupMenuDelegates.ContainsKey(text))
            {
                // Invoke the menu item
                PopupMenuDelegates[text]();
            }
        }
    }
}
using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

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
            AddChild(dialog);
        }

        public void ShowTextEntryDialog(string title, string textName,
                    Action<string, string> callback, Func<string, bool> nameValidator,
                    string listName = "", Func<Dictionary<object, string>> getItemList = null)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_TEXT_ENTRY.Instance() as DataEditorTextEntryDialog;
            dialog.SetupBeforeAddChild(title, textName, callback, nameValidator, listName, getItemList);
            AddChild(dialog);
        }

        public void ClearPopupMenu()
        {
            PopupMenu.Clear();
            PopupMenuDelegates.Clear();
        }

        public void AddPopupMenuSeparator(string name)
        {
            PopupMenu.AddSeparator($" {name} ");
        }

        public void AddPopupMenuEntry(string name, Action action, Texture icon = null)
        {
            if (icon != null)
            {
                PopupMenu.AddIconItem(icon, name);
            }
            else
            {
                PopupMenu.AddItem(name);
            }
            PopupMenuDelegates.Add(name, action);
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
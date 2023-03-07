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
                    string listName = "", List<string> itemList = null)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_TEXT_ENTRY.Instance() as DataEditorTextEntryDialog;
            dialog.SetupBeforeAddChild(title, textName, callback, nameValidator, listName, itemList);
            AddChild(dialog);
        }
    }
}
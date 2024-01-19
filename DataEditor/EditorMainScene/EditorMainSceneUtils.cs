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
                OnEditorOpened(iContent.GetTypeId(), iContent.GetContentName(), iContent.GetContentID(), iContent.GetColor());
            }
            else
            {
                Logger.Warning($"Could not show editor since the root of the scene does not implement iContent: {editorContent.GetType()}");
            }
        }

        public void ShowConfirmDialog(string message, Action<bool> callback)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_CONFIRM.Instantiate() as DataEditorConfirmDialog;
            dialog.SetupBeforeAddChild(message, callback);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowTextEntryDialog(string title, string textName,
                    Action<string, string> callback, Func<string, bool> nameValidator,
                    string listName = "", Func<Dictionary<object, string>> getItemList = null)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_TEXT_ENTRY.Instantiate() as DataEditorTextEntryDialog;
            dialog.SetupBeforeAddChild(title, textName, callback, nameValidator, listName, getItemList);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowAdvancedListDialog(DataEditorAdvancedListDialogInput input)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_ADVANCED_LIST.Instantiate() as DataEditorAdvancedListDialog;
            dialog.Init(input, this);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowFolderManagerDialog()
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_FOLDER_MANAGER.Instantiate() as FolderManager;
            dialog.Init(this);
            CurrentDialog = dialog;
            AddChild(dialog);
        }

        public void ShowGenericEditorDialog(GenericEditorDialogInput input)
        {
            var dialog = DataEditorConstants.SCENE_DIALOG_GENERIC_EDITOR.Instantiate() as GenericEditorDialog;
            dialog.Init(input, this);
            CurrentDialog = dialog;
            AddChild(dialog);
        }
    }
}
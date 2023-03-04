using Godot;
using System;
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
    }
}
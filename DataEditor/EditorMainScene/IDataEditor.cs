using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Interface used by EditorMainScene so other components can access it
    /// </summary>
    public interface IDataEditor
    {
        EditorPrefsExtended Preferences { get; }
        EditorTreeView Tree { get; }
        EditorToolbar Toolbar { get; }

        /// <summary>
        /// Show the editor, must implement IDataEditorContent
        /// </summary>
        void ShowEditor(Control editorContent);

        /// <summary>
        /// Can be called to refresh all data, will ask to confirm if you want to save first if you got unsaved changes
        /// </summary>
        void Refresh();

        /// <summary>
        /// Will save the editor content
        /// </summary>
        void Save();

        /// <summary>
        /// Shows a confirm dialog where the user can press confirm or cancel.
        /// The corresponding callback will be called with the result (true for ok, false for not ok)
        /// </summary>
        void ShowConfirmDialog(string message, Action<bool> callback);
    }
}
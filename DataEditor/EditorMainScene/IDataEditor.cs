using Godot;
using ScriptSystem.Data;
using System;
using System.Collections.Generic;

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

        SearchWindow SearchWindow { get; }

        /// <summary>
        /// Show the editor, must implement IDataEditorContent
        /// </summary>
        void ShowEditor(Control editorContent);

        /// <summary>
        /// Can be called to refresh all data, will ask to confirm if you want to save first if you got unsaved changes
        /// </summary>
        void Refresh(bool askForSave = true);

        /// <summary>
        /// Will save the editor content
        /// </summary>
        void Save();

        /// <summary>
        /// Close the editor, ask to save if there are changes
        /// </summary>
        void Close();

        /// <summary>
        /// Shows a confirm dialog where the user can press confirm or cancel.
        /// The corresponding callback will be called with the result (true for ok, false for not ok)
        /// </summary>
        void ShowConfirmDialog(string message, Action<bool> callback);

        /// <summary>
        /// Shows a dialog with a text entry field and an optional list of values
        /// </summary>
        void ShowTextEntryDialog(string title, string textName,
                    Action<string, string> callback, Func<string, bool> nameValidator,
                    string listName = "", Func<Dictionary<object, string>> getItemList = null);

        /// <summary>
        /// Shows the advanced list dialog
        /// </summary>
        void ShowAdvancedListDialog(DataEditorAdvancedListDialogInput input);

        /// <summary>
        /// Shows the generic editor dialog
        /// </summary>
        void ShowGenericEditorDialog(GenericEditorDialogInput input);

        /// <summary>
        /// Editor that lets you organize your mods
        /// </summary>
        void ShowFolderManagerDialog();

        /// <summary>
        /// Sent when a module tree item is pressed
        /// </summary>
        void NotifyOnModuleTreeItemPressed(string name, DelegateEditorTreeItem item);

        /// <summary>
        /// Shows an icon selection dialog and sets icon to editor data
        /// </summary>
        void ShowIconSelectionDialog(IEditorDataParent data, IIconProvider provider);

        /// <summary>
        /// Search for json defs
        /// </summary>
        /// <param name="query"></param>
        /// <param name="exactMatch"></param>
        /// <returns></returns>
        List<JsonDefWithName> Search(string query, bool exactMatch);

        /// <summary>
        /// Refresh the active editor if there is one
        /// </summary>
        void RefreshActiveEditor();
    }
}
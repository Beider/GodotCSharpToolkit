using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Advanced list dialog that let's you define columns for the data to list.
    /// Also allows the user to search
    /// </summary>
    public class DataEditorAdvancedListDialog : ColorRect
    {
        private const string PREF_SIZE = "list_dia_size_";
        private const string PREF_POS = "list_dia_pos_";

        private WindowDialog Dialog;
        private Label LblDescription;
        private Label LblSearch;
        private LineEdit TxtSearchField;
        private Tree Tree;
        private Button BtnOk;
        private Button BtnCancel;
        private TreeItem Root;
        private IDataEditor Editor;

        private DataEditorAdvancedListDialogInput Input;

        private int SortedColumn = -1;
        private bool AscendingSort = true;

        private object SelectedTreeItem = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Dialog
            Dialog = FindNode("Dialog") as WindowDialog;
            Dialog.Connect("popup_hide", this, nameof(OnCancelPressed));

            // Misc
            LblDescription = FindNode("Description") as Label;
            LblSearch = FindNode("SearchLabel") as Label;

            // Search field
            TxtSearchField = FindNode("SearchField") as LineEdit;
            TxtSearchField.Connect("text_changed", this, nameof(OnSearchTextChanged));

            // Tree
            Tree = FindNode("Tree") as Tree;
            Tree.Connect("item_selected", this, nameof(OnItemSelected));
            Tree.Connect("multi_selected", this, nameof(OnMultiSelected));
            Tree.Connect("column_title_pressed", this, nameof(OnColumnTitlePressed));
            Tree.Connect("item_double_clicked", this, nameof(OnOkPressed));
            Tree.Connect("item_activated", this, nameof(OnOkPressed));

            // Buttons
            BtnOk = FindNode("BtnOk") as Button;
            BtnCancel = FindNode("BtnCancel") as Button;
            BtnOk.Connect("pressed", this, nameof(OnOkPressed));
            BtnCancel.Connect("pressed", this, nameof(OnCancelPressed));
            BtnOk.Disabled = true;

            // Refresh & show dialog
            LoadPrefs();
            RefreshDialog();
            Dialog.Popup_();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key && key.Pressed)
            {
                if (key.Scancode == (int)KeyList.Escape)
                {
                    OnCancelPressed();
                    GetTree().SetInputAsHandled();
                }
                else if ((key.Scancode == (int)KeyList.Enter || key.Scancode == (int)KeyList.KpEnter) && !BtnOk.Disabled)
                {
                    OnOkPressed();
                    GetTree().SetInputAsHandled();
                }
            }
        }

        private void OnSearchTextChanged(string newText)
        {
            RefreshTree();
            CheckOkEnabled();
        }

        private void OnItemSelected()
        {
            CheckOkEnabled();
        }

        private void OnMultiSelected(TreeItem item, int column, bool selected)
        {
            CheckOkEnabled();
        }

        private void OnColumnTitlePressed(int col)
        {
            if (col != SortedColumn)
            {
                SortedColumn = col;
                AscendingSort = true;
            }
            else
            {
                AscendingSort = !AscendingSort;
            }

            var colData = Input.Columns[col];
            Input.ObjectList.Sort((o1, o2) =>
            {
                var val1 = colData.GetValue(o1);
                var val2 = colData.GetValue(o2);
                if (val1.IsNullOrEmpty() && val2.IsNullOrEmpty()) { return 0; }
                if (val1.IsNullOrEmpty()) { return 1; }
                if (val2.IsNullOrEmpty()) { return -1; }
                if (AscendingSort)
                {
                    return val1.CompareTo(val2);
                }
                return val2.CompareTo(val1);
            });
            RefreshTree();
        }

        public void Init(DataEditorAdvancedListDialogInput input, IDataEditor editor)
        {
            Input = input;
            Editor = editor;
        }

        private void CheckOkEnabled()
        {
            UpdateTreeSelection();
            if (Input.IsOkEnabled != null)
            {
                BtnOk.Disabled = !Input.IsOkEnabled(TxtSearchField.Text, SelectedTreeItem);
                return;
            }

            // Default is disable with no selection
            UpdateTreeSelection();
            BtnOk.Disabled = SelectedTreeItem == null;
        }

        private void UpdateTreeSelection()
        {
            SelectedTreeItem = null;
            if (Tree.SelectMode == Tree.SelectModeEnum.Row)
            {
                // Single mode get single item
                var item = Tree.GetSelected();
                if (item != null)
                {
                    int index = int.Parse(item.GetMetadata(0).ToString());
                    SelectedTreeItem = Input.ObjectList[index];
                }
            }
            else if (Tree.SelectMode == Tree.SelectModeEnum.Multi)
            {
                // Multi mode we get a list
                var selectedItem = Tree.GetNextSelected(null);
                var objList = new List<object>();
                while (selectedItem != null)
                {
                    int index = int.Parse(selectedItem.GetMetadata(0).ToString());
                    if (!objList.Contains(Input.ObjectList[index]))
                    {
                        objList.Add(Input.ObjectList[index]);
                    }
                    selectedItem = Tree.GetNextSelected(selectedItem);
                }
                if (objList.Count > 0)
                {
                    SelectedTreeItem = objList;
                }
            }
        }

        private void RefreshDialog()
        {
            // Misc
            Dialog.WindowTitle = Input.Name;
            LblSearch.Text = Input.SearchLabelText;

            // Description
            if (Input.Description.IsNullOrEmpty())
            {
                LblDescription.Visible = false;
            }
            else
            {
                LblDescription.Text = Input.Description;
            }

            // We do not allow single select
            Tree.SelectMode = Input.SelectionMode == Tree.SelectModeEnum.Single ?
                                Tree.SelectModeEnum.Row : Input.SelectionMode;

            BuildTreeColumns();

            var col = Input.SortedColumn;
            if (col < 0) { col = 0; }
            if (col >= Tree.Columns) { col = Tree.Columns - 1; }
            OnColumnTitlePressed(col);
            RefreshTree();
        }

        private void RefreshTree()
        {
            Tree.Clear();
            Root = Tree.CreateItem(null);
            for (int i = 0; i < Input.ObjectList.Count; i++)
            {
                var obj = Input.ObjectList[i];
                if (FilterItem(obj)) { continue; }

                TreeItem item = Tree.CreateItem(Root);
                // Store index in MD so we can find it again
                item.SetMetadata(0, i);
                for (int c = 0; c < Input.Columns.Count; c++)
                {
                    FillTreeItemColumn(item, c, obj);
                }
            }
        }

        /// <summary>
        /// Filters out items
        /// </summary>
        private bool FilterItem(object item)
        {
            var text = TxtSearchField.Text.ToLower();
            if (text.IsNullOrEmpty()) { return false; }

            // Built in column filter
            if (Input.FilterByColumnValues)
            {
                var splitText = text.Split(";");
                var matches = new bool[splitText.Length];
                for (int i = 0; i < splitText.Length; i++)
                {
                    var value = splitText[i];
                    matches[i] = false;
                    var splitVal = value.Split(":");
                    var col = splitVal.Length > 1 ? splitVal[0] : "";
                    var val = splitVal.Length > 1 ? splitVal[1] : splitVal[0];
                    if (val.IsNullOrEmpty())
                    {
                        // Empty we just match
                        matches[i] = true;
                        continue;
                    }

                    foreach (var colData in Input.Columns)
                    {
                        // If we got a column skip any non matching column
                        if (!col.IsNullOrEmpty() && !colData.Name.Equals(col, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (colData.GetValue(item).ToLower().Contains(val))
                        {
                            matches[i] = true;
                        }
                    }
                }

                // Check that all filters match
                if (!matches.Any((v) => !v))
                {
                    return false;
                }
            }

            // Go through user defined filters
            foreach (var filter in Input.FilterList)
            {
                if (filter(text, item))
                {
                    return false;
                }
            }
            return true;
        }

        private void FillTreeItemColumn(TreeItem item, int colIndex, object obj)
        {
            var colData = Input.Columns[colIndex];
            if (colData.GetValue != null)
            {
                item.SetText(colIndex, colData.GetValue(obj));
            }
            if (colData.GetForegroundColor != null)
            {
                item.SetCustomColor(colIndex, colData.GetForegroundColor(obj));
            }
            if (colData.GetBackgroundColor != null)
            {
                item.SetCustomBgColor(colIndex, colData.GetBackgroundColor(obj));
            }
        }

        private void BuildTreeColumns()
        {
            Tree.Columns = Input.Columns.Count;
            for (int i = 0; i < Input.Columns.Count; i++)
            {
                var colData = Input.Columns[i];
                Tree.SetColumnTitle(i, colData.Name);
                Tree.SetColumnExpand(i, colData.Expand);
                Tree.SetColumnMinWidth(i, colData.MinWidth);
            }
        }

        private void OnCancelPressed()
        {
            if (Input.OnClose != null)
            {
                Input.OnClose();
            }
            SavePrefs();
            QueueFree();
        }

        private void OnOkPressed()
        {
            if (BtnOk.Disabled) { return; }
            if (Input.OnDialogOk != null)
            {
                Input.OnDialogOk(TxtSearchField.Text, SelectedTreeItem);
            }
            SavePrefs();
            QueueFree();
        }

        private void SavePrefs()
        {
            Editor.Preferences.SetValue($"{PREF_SIZE}{Input.UserPrefsKey}", GD.Var2Str(Dialog.RectSize));
            Editor.Preferences.SetValue($"{PREF_POS}{Input.UserPrefsKey}", GD.Var2Str(Dialog.RectPosition));
        }

        private void LoadPrefs()
        {
            var size = Editor.Preferences.GetValue($"{PREF_SIZE}{Input.UserPrefsKey}", GD.Var2Str(Dialog.RectSize));
            var pos = Editor.Preferences.GetValue($"{PREF_POS}{Input.UserPrefsKey}", GD.Var2Str(Dialog.RectPosition));

            Dialog.RectSize = (Vector2)GD.Str2Var(size);
            Dialog.RectPosition = (Vector2)GD.Str2Var(pos);
        }
    }
}

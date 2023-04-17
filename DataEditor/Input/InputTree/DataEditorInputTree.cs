using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{

    public class DataEditorInputTree : DataEditorInput
    {
        private Tree Tree;
        private TreeItem Root;
        private JsonGenericEditorInputRowTree Input;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            Tree = FindNode("Tree") as Tree;
            Tree.Connect("item_activated", this, nameof(OnItemActivated));
            Tree.Connect("item_rmb_selected", this, nameof(OnRmbClicked));
            Tree.Connect("empty_rmb", this, nameof(OnRmbClicked));
            Tree.Connect("empty_tree_rmb_selected", this, nameof(OnRmbClicked));
        }

        protected override void Init()
        {
            Input = InputData as JsonGenericEditorInputRowTree;
            Tree.RectMinSize = new Vector2(Input.EditorWidth, Input.EditorHeight);
            if (TextLabel != null)
            {
                TextLabel.HintTooltip = InputData.ToolTip;
            }
            Tree.HintTooltip = InputData.ToolTip;
            BuildTreeColumns();
            RefreshTree();
        }

        private void OnRmbClicked(Vector2 pos)
        {
            var item = Tree.GetSelected();
            if (item != null)
            {
                ShowPopupMenu(item, pos);
            }
            else
            {
                ShowPopupMenu(null, pos);
            }
        }

        private void OnItemActivated(int index)
        {
            Input.OnDoubleClick(GetSelectedObject());
        }

        private object GetSelectedObject()
        {
            var item = Tree.GetSelected();
            if (item != null)
            {
                int index = (int)item.GetMetadata(0);
                return Input.GetObjectList()[index];
            }
            return null;
        }

        private void ShowPopupMenu(TreeItem item, Vector2 pos)
        {
            if (Input.OnAdd == null && Input.OnRemove == null)
            {
                return;
            }
            var menu = Editor.PopupMenu;
            Editor.ClearPopupMenu();
            menu.RectSize = menu.RectMinSize;
            FillPopupMenu();
            menu.RectPosition = GetViewport().GetMousePosition();
            menu.Popup_();
        }

        private void FillPopupMenu()
        {
            var item = Tree.GetSelected();
            var selectedItem = GetSelectedObject();
            Editor.AddPopupMenuSeparator(InputData.Name);
            Editor.AddPopupMenuEntry($"Add new ", () =>
            {
                Input.OnAdd(this);
                RefreshTree();
            }, DataEditorConstants.ICON_NEW);
            if (selectedItem == null) { return; }
            Editor.AddPopupMenuEntry($"Remove {item.GetText(0)} ", () =>
            {
                Input.OnRemove(selectedItem, this);
                RefreshTree();
            }, DataEditorConstants.ICON_DELETE);
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

        private void RefreshTree()
        {
            Tree.Clear();
            Root = Tree.CreateItem(null);
            var list = Input.GetObjectList();
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i];

                TreeItem item = Tree.CreateItem(Root);
                // Store index in MD so we can find it again
                item.SetMetadata(0, i);
                for (int c = 0; c < Input.Columns.Count; c++)
                {
                    FillTreeItemColumn(item, c, obj);
                }
            }
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

    }
}
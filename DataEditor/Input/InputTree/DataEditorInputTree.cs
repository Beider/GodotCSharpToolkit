using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{

    public partial class DataEditorInputTree : DataEditorInput
    {
        private Tree Tree;
        private TreeItem Root;
        private JsonGenericEditorInputRowTree Input;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            Tree = FindChild("Tree") as Tree;
            Tree.Connect("item_activated", new Callable(this, nameof(OnItemActivated)));
            Tree.Connect("item_mouse_selected", new Callable(this, nameof(OnRmbClicked)));
            Tree.Connect("empty_clicked", new Callable(this, nameof(OnRmbClicked)));
        }

        protected override void Init()
        {
            Input = InputData as JsonGenericEditorInputRowTree;
            Tree.CustomMinimumSize = new Vector2(Input.EditorWidth, Input.EditorHeight);
            Tree.TooltipText = InputData.ToolTip;
            BuildTreeColumns();
            Refresh();
        }

        private void OnRmbClicked(Vector2 pos, int buttonIndex)
        {
            if (buttonIndex != (int)MouseButton.Right) { return; }
            var item = Tree.GetSelected();
            if (item != null && item != Root)
            {
                ShowPopupMenu(item, pos);
            }
            else
            {
                ShowPopupMenu(null, pos);
            }
        }

        private void OnItemActivated()
        {
            var selection = GetSelectedObject();
            if (selection != null && Input.OnDoubleClick != null)
            {
                Input.OnDoubleClick(selection, this);
            }
        }

        private object GetSelectedObject()
        {
            var item = Tree.GetSelected();
            if (item != null && item != Root)
            {
                int index = (int)item.GetMetadata(0);
                return Input.GetObjectList()[index];
            }
            return null;
        }

        private void ShowPopupMenu(TreeItem item, Vector2 pos)
        {
            if (Input.OnAdd == null &&
                Input.OnRemove == null &&
                Input.OnEdit == null)
            {
                return;
            }
            var menu = DataEditorConstants.CreatePopupMenu(this);
            menu.Size = menu.MinSize;
            FillPopupMenu(menu);
            menu.PositionInParent(GetViewport().GetMousePosition());
            menu.Popup();
        }

        private void FillPopupMenu(EditorPopupMenu menu)
        {
            var item = Tree.GetSelected();
            var selectedItem = GetSelectedObject();
            menu.AddPopupMenuSeparator(InputData.Name);
            if (Input.OnAdd != null)
            {
                menu.AddPopupMenuEntry($"Add new ", () =>
                {
                    Input.OnAdd(this);
                }, DataEditorConstants.ICON_NEW);
            }
            if (selectedItem == null) { return; }
            if (Input.OnEdit != null)
            {
                menu.AddPopupMenuEntry($"Edit {item.GetText(0)} ", () =>
                {
                    Input.OnEdit(selectedItem, this);
                }, DataEditorConstants.ICON_EDIT);
            }
            if (Input.OnRemove != null)
            {
                menu.AddPopupMenuEntry($"Remove {item.GetText(0)} ", () =>
                {
                    Input.OnRemove(selectedItem, this);
                }, DataEditorConstants.ICON_DELETE);
            }
            if (Input.MenuItems != null)
            {
                foreach (var mItem in Input.MenuItems)
                {
                    menu.AddPopupMenuEntry(mItem.Name, () =>
                    {
                        mItem.Action(selectedItem, this);
                    }, mItem.Icon);
                }
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
                Tree.SetColumnCustomMinimumWidth(i, colData.MinWidth);
            }

        }

        public override void Refresh()
        {
            Tree.Clear();
            Root = Tree.CreateItem(null);
            if (Root == null) { return; }
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
            Tree.HideRoot = list.Count != 0;
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
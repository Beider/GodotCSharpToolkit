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

        private GridContainer ParentGrid;
        private GridContainer MovementGrid;

        private Button BtnTop;
        private Button BtnUp;
        private Button BtnDown;
        private Button BtnBottom;

        private List<Button> Buttons = new List<Button>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            Tree = FindChild("Tree") as Tree;
            Tree.Connect("item_selected", new Callable(this, nameof(ToggleButtonStates)));
            Tree.Connect("item_activated", new Callable(this, nameof(OnItemActivated)));
            Tree.Connect("item_mouse_selected", new Callable(this, nameof(OnRmbClicked)));
            Tree.Connect("empty_clicked", new Callable(this, nameof(OnRmbClicked)));

            ParentGrid = FindChild("ParentGrid") as GridContainer;
            MovementGrid = FindChild("MovementGrid") as GridContainer;

            BtnTop = FindChild("BtnTop") as Button;
            BtnTop.Connect("pressed", Callable.From(() => OnMove(ItemMovements.Top)));

            BtnUp = FindChild("BtnUp") as Button;
            BtnUp.Connect("pressed", Callable.From(() => OnMove(ItemMovements.Up)));

            BtnDown = FindChild("BtnDown") as Button;
            BtnDown.Connect("pressed", Callable.From(() => OnMove(ItemMovements.Down)));

            BtnBottom = FindChild("BtnBottom") as Button;
            BtnBottom.Connect("pressed", Callable.From(() => OnMove(ItemMovements.Bottom)));
        }

        protected override void Init()
        {
            Input = InputData as JsonGenericEditorInputRowTree;
            Tree.CustomMinimumSize = new Vector2(Input.EditorWidth, Input.EditorHeight);
            Tree.TooltipText = InputData.ToolTip;

            if (Input.OnMovement == null)
            {
                ParentGrid.Columns = 1;
                MovementGrid.Visible = false;
            }
            BuildTreeColumns();
            Refresh();
        }

        private void OnMove(ItemMovements direction)
        {
            if (Input.OnMovement == null) { return; }

            var obj = GetSelectedObject();
            if (obj == null) { return; }

            var index = GetSelectedIndex();

            var newIndex = Input.OnMovement(index, obj, direction);
            Refresh(newIndex);
        }

        private void ToggleButtonStates()
        {
            // Default disabled
            BtnTop.Disabled = true;
            BtnUp.Disabled = true;
            BtnDown.Disabled = true;
            BtnBottom.Disabled = true;

            var item = Tree.GetSelected();
            if (item == null) { return; }

            var parent = item.GetParent();
            if (parent == null) { return; }

            int index = item.GetIndex();
            int total = parent.GetChildCount() - 1;

            BtnTop.Disabled = index == 0;
            BtnUp.Disabled = index == 0;
            BtnDown.Disabled = index == total;
            BtnBottom.Disabled = index == total;
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
                var index = GetSelectedIndex();
                Input.OnDoubleClick(index, selection, this);
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

        private int GetSelectedIndex()
        {
            var item = Tree.GetSelected();
            if (item != null && item != Root)
            {
                int index = (int)item.GetMetadata(0);
                return index;
            }
            return -1;
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
            var index = GetSelectedIndex();
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
                    Input.OnEdit(index, selectedItem, this);
                }, DataEditorConstants.ICON_EDIT);
            }
            if (Input.OnRemove != null)
            {
                menu.AddPopupMenuEntry($"Remove {item.GetText(0)} ", () =>
                {
                    Input.OnRemove(index, selectedItem, this);
                }, DataEditorConstants.ICON_DELETE);
            }
            if (Input.MenuItems != null)
            {
                foreach (var mItem in Input.MenuItems)
                {
                    menu.AddPopupMenuEntry(mItem.Name, () =>
                    {
                        mItem.Action(index, selectedItem, this);
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
            Refresh();
        }

        private void Refresh(int selectIndex = -1)
        {
            if (Tree == null || !IsInstanceValid(Tree)) { return; }
            Tree.Clear();
            Root = Tree.CreateItem(null);
            if (Root == null) { return; }
            var list = Input.GetObjectList();
            TreeItem selectAfterRefresh = null;
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
                if (i == selectIndex)
                {
                    selectAfterRefresh = item;
                }
            }
            Tree.HideRoot = list.Count != 0;
            if (selectAfterRefresh != null)
            {
                selectAfterRefresh.Select(0);
            }
            ToggleButtonStates();
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
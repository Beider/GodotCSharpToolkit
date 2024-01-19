using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class DataEditorInputList : DataEditorInput
    {
        private ItemList ListField;
        private Dictionary<int, object> ReturnValueLookup = new Dictionary<int, object>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            ListField = FindChild("ItemList") as ItemList;
            ListField.Connect("item_clicked", new Callable(this, nameof(ShowPopupMenu)));
            ListField.Connect("item_activated", new Callable(this, nameof(OnItemActivated)));
            ListField.Connect("empty_clicked", Callable.From((Vector2 pos, int buttonIndex) => ShowPopupMenu(-1, pos, buttonIndex)));
        }

        private void OnItemActivated(int index)
        {
            if (InputData is JsonGenericEditorInputRowList iData)
            {
                iData.OnDoubleClick(ReturnValueLookup[index]);
            }
        }

        public override void TakeFocus()
        {
            CallDeferred(nameof(_TakeFocus));
        }

        private void _TakeFocus()
        {
            ListField?.GrabFocus();
        }

        private void ShowPopupMenu(int index, Vector2 pos, int buttonIndex)
        {
            if (buttonIndex != (int)MouseButton.Right) { return; }
            if (InputData is JsonGenericEditorInputRowList iData)
            {
                if (iData.OnAdd == null && iData.OnRemove == null)
                {
                    return;
                }
            }
            var menu = DataEditorConstants.CreatePopupMenu(this);
            menu.Size = menu.MinSize;
            FillPopupMenu(index, menu);
            menu.PositionInParent(GetViewport().GetMousePosition());
            menu.Popup();
        }

        private void FillPopupMenu(int index, EditorPopupMenu menu)
        {
            if (InputData is JsonGenericEditorInputRowList iData)
            {
                menu.AddPopupMenuSeparator(InputData.Name);
                menu.AddPopupMenuEntry($"Add new ", () =>
                {
                    iData.OnAdd(this);
                }, DataEditorConstants.ICON_NEW);
                if (index < 0) { return; }
                menu.AddPopupMenuEntry($"Remove {ListField.GetItemText(index)} ", () =>
                {
                    iData.OnRemove(ReturnValueLookup[index], this);
                }, DataEditorConstants.ICON_DELETE);
            }
        }

        protected override void Init()
        {
            if (InputData.Name.IsNullOrEmpty())
            {
                TextLabel.QueueFree();
            }
            var input = (JsonGenericEditorInputRowList)InputData;
            ListField.CustomMinimumSize = new Vector2(input.EditorWidth, input.EditorHeight);
            ListField.TooltipText = InputData.ToolTip;
            Refresh();
        }

        public override void Refresh()
        {
            ReturnValueLookup.Clear();
            ListField.Clear();

            if (InputData is JsonGenericEditorInputRowList iData)
            {
                var valueList = iData.GetListValues().ToList();
                if (iData.Sort)
                {
                    valueList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                }
                foreach (var item in valueList)
                {
                    ListField.AddItem(item.Value);
                    ReturnValueLookup.Add(ListField.ItemCount - 1, item.Key);
                }
            }
        }

        private object GetSelectedItemValue()
        {
            if (ListField.IsAnythingSelected())
            {
                return ReturnValueLookup[ListField.GetSelectedItems()[0]];
            }
            return "";
        }


    }
}

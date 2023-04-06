using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorInputList : DataEditorInput
    {
        private ItemList ListField;
        private Dictionary<int, object> ReturnValueLookup = new Dictionary<int, object>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            ListField = FindNode("ItemList") as ItemList;
            ListField.Connect("rmb_clicked", this, nameof(OnRmbClicked));
            ListField.Connect("item_activated", this, nameof(OnItemActivated));
            ListField.Connect("item_rmb_selected", this, nameof(ShowPopupMenu));
        }

        private void OnRmbClicked(Vector2 pos)
        {
            if (ListField.IsAnythingSelected())
            {
                ShowPopupMenu(ListField.GetSelectedItems()[0], pos);
            }
            else
            {
                ShowPopupMenu(-1, pos);
            }
        }

        private void OnItemActivated(int index)
        {
            if (InputData is JsonGenericEditorInputRowList iData)
            {
                iData.OnDoubleClick(ReturnValueLookup[index]);
            }
        }

        private void ShowPopupMenu(int index, Vector2 pos)
        {
            if (InputData is JsonGenericEditorInputRowList iData)
            {
                if (iData.OnAdd == null && iData.OnRemove == null)
                {
                    return;
                }
            }
            var menu = Editor.PopupMenu;
            Editor.ClearPopupMenu();
            menu.RectSize = menu.RectMinSize;
            FillPopupMenu(index);
            menu.RectPosition = GetViewport().GetMousePosition();
            menu.Popup_();
        }

        private void FillPopupMenu(int index)
        {
            if (InputData is JsonGenericEditorInputRowList iData)
            {
                Editor.AddPopupMenuSeparator(InputData.Name);
                Editor.AddPopupMenuEntry($"Add new ", () =>
                {
                    iData.OnAdd(this);
                }, DataEditorConstants.ICON_NEW);
                if (index < 0) { return; }
                Editor.AddPopupMenuEntry($"Remove {ListField.GetItemText(index)} ", () =>
                {
                    iData.OnRemove(ReturnValueLookup[index], this);
                }, DataEditorConstants.ICON_DELETE);
            }
        }

        protected override void Init()
        {
            var input = (JsonGenericEditorInputRowList)InputData;
            ListField.RectMinSize = new Vector2(input.EditorWidth, input.EditorHeight);
            TextLabel.HintTooltip = InputData.ToolTip;
            ListField.HintTooltip = InputData.ToolTip;
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
                    ReturnValueLookup.Add(ListField.GetItemCount() - 1, item.Key);
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

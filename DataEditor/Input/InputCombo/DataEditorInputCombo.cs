using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Used to show a list. It expects a dictionary of string, string
    /// where first string is key and second is value to show
    /// </summary>
    public class DataEditorInputCombo : DataEditorInput
    {
        private OptionButton OptButton;

        private Dictionary<string, int> ItemIndexLookup = new Dictionary<string, int>();
        private Dictionary<int, object> ReturnValueLookup = new Dictionary<int, object>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            OptButton = FindNode("OptionButton") as OptionButton;
            OptButton.Connect("item_selected", this, nameof(_OnItemChanged));
        }

        private void _OnItemChanged(int index)
        {
            var value = OptButton.GetItemText(index);
            OnValueChanged(ReturnValueLookup[index]);
        }

        public void RefreshItemList()
        {
            OptButton.Clear();
            ItemIndexLookup.Clear();
            ReturnValueLookup.Clear();
            if (InputData is JsonGenericEditorInputRowCombo iData)
            {
                var valueList = iData.GetListValues().ToList();
                if (iData.Sort)
                {
                    valueList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                }
                foreach (var item in valueList)
                {
                    OptButton.AddItem(item.Value);
                    ItemIndexLookup.Add(item.Value, OptButton.GetItemCount() - 1);
                    ReturnValueLookup.Add(OptButton.GetItemCount() - 1, item.Key);
                }
            }
        }

        protected override void Init()
        {
            OptButton.RectMinSize = new Vector2(InputData.EditorWidth, 0f);
            OptButton.HintTooltip = InputData.ToolTip;
            TextLabel.HintTooltip = InputData.ToolTip;
            RefreshItemList();
            Refresh();
        }

        public override void Disable(bool disabled)
        {
            base.Disable(disabled);
            OptButton.Disabled = disabled;
        }

        public override void Refresh()
        {
            object value = InputData.GetValue(Data);

            foreach (var key in ReturnValueLookup.Keys)
            {
                if (ReturnValueLookup[key].Equals(value))
                {
                    OptButton.Select(key);
                    OnValueChanged(ReturnValueLookup[key], true, false);
                    return;
                }
            }


            OnValueChanged(ReturnValueLookup[0], true, false);
        }
    }
}
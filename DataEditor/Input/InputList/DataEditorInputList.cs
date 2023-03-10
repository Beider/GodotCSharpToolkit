using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorInputList : DataEditorInput
    {
        private OptionButton OptButton;

        private Dictionary<string, int> ItemIndexLookup = new Dictionary<string, int>();

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
            OnValueChanged(value);
        }

        public void RefreshItemList()
        {
            OptButton.Clear();
            ItemIndexLookup.Clear();
            if (InputData is JsonGenericEditorInputRowList iData)
            {
                if (iData.Sort) { iData.Values.Sort(); }
                foreach (var item in iData.Values)
                {
                    OptButton.AddItem(item);
                    ItemIndexLookup.Add(item, OptButton.GetItemCount() - 1);
                }
            }
        }

        protected override void Init()
        {
            OptButton.RectMinSize = new Vector2(InputData.EditorWidth, 0f);
            RefreshItemList();
            Refresh();
        }

        public override void Refresh()
        {
            object value = InputData.GetValue(Data);
            string val = "";
            if (value != null)
            {
                val = value.ToString();
            }

            if (ItemIndexLookup.ContainsKey(val))
            {
                OptButton.Select(ItemIndexLookup[val]);
                OnValueChanged(val, false);
            }
        }
    }
}
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

        public void SetItemList(List<string> itemList, bool sort = true)
        {
            OptButton.Clear();
            ItemIndexLookup.Clear();
            if (sort) { itemList.Sort(); }
            foreach (var item in itemList)
            {
                OptButton.AddItem(item);
                ItemIndexLookup.Add(item, OptButton.GetItemCount() - 1);
            }
        }

        public override void SetValue(object value)
        {
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

        public void SetInputWidth(float width)
        {
            OptButton.RectMinSize = new Vector2(width, 0f);
        }
    }
}
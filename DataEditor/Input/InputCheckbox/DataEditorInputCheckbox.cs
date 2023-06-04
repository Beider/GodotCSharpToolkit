using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorInputCheckbox : DataEditorInput
    {
        private CheckBox CheckBox;
        private bool InCode = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            CheckBox = FindNode("CheckBox") as CheckBox;
            CheckBox.Connect("toggled", this, nameof(_OnCheckboxToggled));
        }

        private void _OnCheckboxToggled(bool newValue)
        {
            if (InCode) { return; }
            OnValueChanged(newValue);
        }

        protected override void Init()
        {
            CheckBox.Text = InputData.Name;
            CheckBox.RectMinSize = new Vector2(InputData.EditorWidth, 0f);
            CheckBox.HintTooltip = InputData.ToolTip;
            InCode = true;
            Disable(InputData.Disabled);
            Refresh();
            InCode = false;
        }

        public override void Disable(bool disabled)
        {
            base.Disable(disabled);
            CheckBox.Disabled = disabled;
        }

        public override void Refresh()
        {
            object value = InputData.GetValue(Data);
            if (value == null) { return; }
            bool curVal;
            if (bool.TryParse(value.ToString(), out curVal))
            {
                CheckBox.Pressed = curVal;
            }

            OnValueChanged(CheckBox.Pressed, false, false);
        }


    }
}
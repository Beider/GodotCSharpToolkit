using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public partial class DataEditorInputText : DataEditorInput
    {
        private LineEdit TextField;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            TextField = FindChild("LineEdit") as LineEdit;
            TextField.Connect("text_changed", new Callable(this, nameof(_OnTextChanged)));
        }

        public override void Disable(bool disabled)
        {
            base.Disable(disabled);
            TextField.Editable = !disabled;
        }

        private void _OnTextChanged(string newText)
        {
            OnValueChanged(newText);
        }

        protected override void Init()
        {
            TextField.CustomMinimumSize = new Vector2(InputData.EditorWidth, 0f);
            TextField.TooltipText = InputData.ToolTip;
            TextLabel.TooltipText = InputData.ToolTip;
            Disable(InputData.Disabled);
            Refresh();
        }

        public override void TakeFocus()
        {
            CallDeferred(nameof(_TakeFocus));
        }

        private void _TakeFocus()
        {
            if (TextField == null) { return; }
            TextField.GrabFocus();
            TextField.CaretColumn = TextField.Text.Length;
        }

        public override void Refresh()
        {
            object value = InputData.GetValue(Data);
            if (value == null)
            {
                TextField.Text = "";
            }
            else
            {
                TextField.Text = value.ToString();
            }

            OnValueChanged(TextField.Text, false, false);
        }


    }
}
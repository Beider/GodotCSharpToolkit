using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// An input button, calls the OnSave method when pressed.
    /// Sets the text to the getValue string
    /// </summary>
    public class DataEditorInputButton : DataEditorInput
    {
        private Button BtnInput;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            BtnInput = FindNode("Button") as Button;
            BtnInput.Connect("pressed", this, nameof(_OnButtonPressed));
        }

        public override void Disable(bool disabled)
        {
            base.Disable(disabled);
            BtnInput.Disabled = disabled;
        }

        private void _OnButtonPressed()
        {
            if (InputData is JsonGenericEditorInputRowButton btnInput &&
                btnInput.OnPressed != null)
            {
                btnInput.OnPressed(Data, this);
            }
        }

        protected override void Init()
        {
            BtnInput.RectMinSize = new Vector2(InputData.EditorWidth, 0f);
            BtnInput.HintTooltip = InputData.ToolTip;
            BtnInput.Text = (string)InputData.GetValue(Data);
            Disable(InputData.Disabled);
            Refresh();
        }

        public override void Refresh()
        {
            BtnInput.Text = (string)InputData.GetValue(Data);
        }


    }
}
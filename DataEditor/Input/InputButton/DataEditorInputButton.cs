using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// An input button, calls the OnSave method when pressed.
    /// Sets the text to the getValue string
    /// </summary>
    public partial class DataEditorInputButton : DataEditorInput
    {
        private Button BtnInput;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            BtnInput = FindChild("Button") as Button;
            BtnInput.Connect("pressed", new Callable(this, nameof(_OnButtonPressed)));
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
            BtnInput.CustomMinimumSize = new Vector2(InputData.EditorWidth, 0f);
            BtnInput.TooltipText = InputData.ToolTip;
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
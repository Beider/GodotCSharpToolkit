using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorInputText : DataEditorInput
    {
        private LineEdit TextField;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            TextField = FindNode("LineEdit") as LineEdit;
            TextField.Connect("text_changed", this, nameof(_OnTextChanged));
        }

        private void _OnTextChanged(string newText)
        {
            OnValueChanged(newText);
        }

        protected override void Init()
        {
            TextField.RectMinSize = new Vector2(InputData.EditorWidth, 0f);
            Refresh();
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

            OnValueChanged(TextField.Text, false);
        }


    }
}
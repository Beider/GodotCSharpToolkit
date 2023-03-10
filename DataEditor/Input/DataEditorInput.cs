using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorInput : GridContainer, IDataEditorInput
    {
        protected Label TextLabel;
        protected JsonDefWithName Data;
        protected JsonGenericEditorInputRow InputData;
        protected IDataEditor Editor;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            TextLabel = FindNode("Label") as Label;
        }

        public void SetInputData(JsonDefWithName data, JsonGenericEditorInputRow input, IDataEditor editor)
        {
            this.Data = data;
            this.InputData = input;
            this.Editor = editor;
            if (TextLabel != null)
            {
                TextLabel.Text = $"{InputData.Name}: ";
                TextLabel.RectMinSize = new Vector2(InputData.LabelWidth, 0f);
            }
            Init();
        }

        protected virtual void Init()
        {

        }

        public virtual void Refresh()
        {

        }

        protected virtual void OnValueChanged(object newValue, bool notifyAndSave = true, bool markModified = true)
        {
            if (InputData.OnValidate == null)
            {
                if (notifyAndSave)
                {
                    InputData.OnSave(InputData.Name, Data, newValue);
                    if (Data != null && markModified) { Data.IsModified = true; }
                }
            }
            else
            {
                if (InputData.OnValidate(InputData.Name, Data, newValue))
                {
                    if (notifyAndSave)
                    {
                        InputData.OnSave(InputData.Name, Data, newValue);
                        if (Data != null && markModified) { Data.IsModified = true; }
                    }
                    TextLabel.AddColorOverride("font_color", GetColor("font_color", "Label"));
                }
                else
                {
                    TextLabel.AddColorOverride("font_color", Colors.Red);
                }
            }
        }
    }
}
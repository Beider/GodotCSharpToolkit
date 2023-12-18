using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public partial class DataEditorInput : GridContainer, IDataEditorInput
    {
        protected Label TextLabel;
        protected JsonDefWithName Data;
        protected JsonGenericEditorInputRow InputData;
        protected IDataEditor Editor;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            TextLabel = FindChild("Label") as Label;
        }

        public void SetInputData(JsonDefWithName data, JsonGenericEditorInputRow input, IDataEditor editor)
        {
            this.Data = data;
            this.InputData = input;
            this.Editor = editor;
            if (TextLabel != null)
            {
                TextLabel.Text = $"{InputData.Name}: ";
                TextLabel.CustomMinimumSize = new Vector2(InputData.LabelWidth, 0f);
            }
            Init();
            DataUpdated();
        }

        public virtual void Disable(bool disabled)
        {
            if (TextLabel != null)
            {
                SetTextLabelDisabled(disabled);
            }
        }

        protected void SetTextLabelDisabled(bool disabled)
        {
            if (disabled)
            {
                TextLabel.AddThemeColorOverride("font_color", DataEditorConstants.COLOR_DISABLED);
            }
            else
            {
                TextLabel.AddThemeColorOverride("font_color", DataEditorConstants.COLOR_DEFAULT);
            }
        }

        protected virtual void Init()
        {

        }

        public virtual void Refresh()
        {

        }

        public virtual void TakeFocus()
        {

        }

        public virtual void DataUpdated()
        {
            if (InputData.OnDataUpdated != null)
            {
                InputData.OnDataUpdated(this);
            }
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
                    if (TextLabel != null)
                    {
                        TextLabel.AddThemeColorOverride("font_color", GetThemeColor("font_color", "Label"));
                    }
                }
                else if (TextLabel != null)
                {
                    TextLabel.AddThemeColorOverride("font_color", Colors.Red);
                }
            }
        }
    }
}
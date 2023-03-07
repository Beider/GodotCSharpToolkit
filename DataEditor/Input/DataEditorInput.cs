using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorInput : GridContainer
    {
        protected Label TextLabel;
        protected Func<object, string, bool> Validator = null;
        protected Action<object, string> SaveAction = null;
        protected string Key = "";

        public event Action<object> OnChange = delegate { };

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            TextLabel = FindNode("Label") as Label;
        }

        public void SetText(string text)
        {
            if (TextLabel != null)
            {
                TextLabel.Text = $"{text}: ";
            }
        }

        public virtual void SetValue(object value)
        {

        }

        protected virtual void OnValueChanged(string newText, bool notifyAndSave = true)
        {
            if (Validator == null)
            {
                if (notifyAndSave) { SaveAction.Invoke(newText, Key); }
            }
            else
            {
                if (Validator.Invoke(newText, Key))
                {
                    if (notifyAndSave) { SaveAction.Invoke(newText, Key); }
                    TextLabel.AddColorOverride("font_color", GetColor("font_color", "Label"));
                }
                else
                {
                    TextLabel.AddColorOverride("font_color", Colors.Red);
                }
            }
            if (notifyAndSave) { NotifyOnChange(newText); }
        }

        /// <summary>
        /// Used for save / validate actions
        /// </summary>
        public void SetKey(string key)
        {
            Key = key;
        }

        public void SetValidator(Func<object, string, bool> function)
        {
            Validator = function;
        }

        public void SetSaveAction(Action<object, string> function)
        {
            SaveAction = function;
        }

        protected void NotifyOnChange(object newValue)
        {
            OnChange(newValue);
        }
    }
}
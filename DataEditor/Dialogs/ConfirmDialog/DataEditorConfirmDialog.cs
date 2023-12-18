using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public partial class DataEditorConfirmDialog : ColorRect
    {
        private Label LblMessage;
        private Button BtnConfirm;
        private Button BtnCancel;

        private Action<bool> Callback;
        private string Message;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            LblMessage = FindChild("Message") as Label;
            BtnConfirm = FindChild("BtnConfirm") as Button;
            BtnCancel = FindChild("BtnCancel") as Button;

            LblMessage.Text = Message;
            BtnConfirm.Connect("pressed", new Callable(this, nameof(OnConfirmPressed)));
            BtnCancel.Connect("pressed", new Callable(this, nameof(OnCancelPressed)));

        }

        private void OnConfirmPressed()
        {
            Callback.Invoke(true);
            QueueFree();
        }

        private void OnCancelPressed()
        {
            Callback.Invoke(false);
            QueueFree();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key && key.Pressed)
            {
                if (key.Keycode == Key.Escape)
                {
                    OnCancelPressed();
                    GetViewport().SetInputAsHandled();
                }
                else if (key.Keycode == Key.Enter || key.Keycode == Key.KpEnter)
                {
                    OnConfirmPressed();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        public void SetupBeforeAddChild(string message, Action<bool> callback)
        {
            Callback = callback;
            Message = message;
        }


    }
}

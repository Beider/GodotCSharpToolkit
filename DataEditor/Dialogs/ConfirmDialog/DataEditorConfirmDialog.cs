using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorConfirmDialog : ColorRect
    {
        private Label LblMessage;
        private Button BtnConfirm;
        private Button BtnCancel;

        private Action<bool> Callback;
        private string Message;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            LblMessage = FindNode("Message") as Label;
            BtnConfirm = FindNode("BtnConfirm") as Button;
            BtnCancel = FindNode("BtnCancel") as Button;

            LblMessage.Text = Message;
            BtnConfirm.Connect("pressed", this, nameof(OnConfirmPressed));
            BtnCancel.Connect("pressed", this, nameof(OnCancelPressed));

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
                if (key.Scancode == (int)KeyList.Escape)
                {
                    OnCancelPressed();
                    GetTree().SetInputAsHandled();
                }
                else if (key.Scancode == (int)KeyList.Enter || key.Scancode == (int)KeyList.KpEnter)
                {
                    OnConfirmPressed();
                    GetTree().SetInputAsHandled();
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
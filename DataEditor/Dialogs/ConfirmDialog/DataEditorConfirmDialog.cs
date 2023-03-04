using Godot;
using System;

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
        AudioManager.PlaySound(Sounds.UiClick, null);
        Callback.Invoke(true);
        QueueFree();
    }

    private void OnCancelPressed()
    {
        AudioManager.PlaySound(Sounds.UiClick, null);
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
            }
            else if (key.Scancode == (int)KeyList.Enter)
            {
                OnConfirmPressed();
            }
        }
    }

    public void SetupBeforeAddChild(string message, Action<bool> callback)
    {
        Callback = callback;
        Message = message;
    }


}

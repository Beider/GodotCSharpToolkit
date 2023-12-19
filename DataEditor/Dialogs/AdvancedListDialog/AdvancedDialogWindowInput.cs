using Godot;
using System;

public partial class AdvancedDialogWindowInput : Window
{
	public event Action OnCancelPressed = delegate { };
	public event Action OnOkPressed = delegate { };
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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
				OnOkPressed();
				GetViewport().SetInputAsHandled();
			}
		}
	}
}

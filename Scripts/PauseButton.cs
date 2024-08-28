using Godot;
using System;

public class PauseButton : Button
{
	public override void _Ready()
	{
		Connect("pressed", this, "ButtonPressed");
	}

	private void ButtonPressed()
	{
   		EventManager.Instance.EmitSignal("LevelChange", "PauseMenu");
	}
}

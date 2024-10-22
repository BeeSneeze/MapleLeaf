using Godot;
using System;

public class DiscordButton : Control
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	private void _on_Control_gui_input(object @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			switch (mouseEvent.ButtonIndex)
			{
				case 1:
					GD.Print($"Left button was clicked at {mouseEvent.Position}");
					OS.ShellOpen("https://discord.gg/EyxfdRCYQQ");
				break;
			}
		}
	}
}




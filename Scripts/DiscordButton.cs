using Godot;
using System;

public class DiscordButton : Control
{
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




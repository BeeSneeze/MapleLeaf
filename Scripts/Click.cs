using Godot;
using System;





public class Click : Control
{
	public override void _GuiInput(InputEvent @event) // On mouse clicks.
	{
		
		var Parent = GetParent();
		
		if (@event is InputEventMouseButton mb)
		{
			// Left mouse button
			if (mb.ButtonIndex == 1 && mb.Pressed)
				Parent.CallDeferred("LeftClick");
			
			// Right mouse button
			if (mb.ButtonIndex == 2 && mb.Pressed)
				Parent.CallDeferred("RightClick");
			
		}
	}

	void OnMouseEntered()
	{
		var Parent = GetParent();
		Parent.CallDeferred("MouseEnter");
	}

	void OnMouseExit()
	{
		var Parent = GetParent();
		Parent.CallDeferred("MouseExit");
	}
}

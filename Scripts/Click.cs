using Godot;
using System;

public class Click : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//CardParent = GetParent();
	}

	public override void _GuiInput(InputEvent @event) // On mouse clicks.
	{
		var CardParent = GetParent();

		if (@event is InputEventMouseButton mb)
		{
			// Left mouse button
			if (mb.ButtonIndex == 1 && mb.Pressed)
				CardParent.CallDeferred("LeftClick");
			
			// Right mouse button
			if (mb.ButtonIndex == 2 && mb.Pressed)
				CardParent.CallDeferred("RightClick");
			
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

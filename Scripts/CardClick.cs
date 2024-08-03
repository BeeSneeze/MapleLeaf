using Godot;
using System;

public class CardClick : Control
{

	Card CardParent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CardParent = (Card)GetParent();
	}

	public override void _GuiInput(InputEvent @event) // On mouse clicks.
	{
		if (@event is InputEventMouseButton mb)
		{
			// Left mouse button
			if (mb.ButtonIndex == 1 && mb.Pressed)
				CardParent.LeftClick();
			
			// Right mouse button
			if (mb.ButtonIndex == 2 && mb.Pressed)
				CardParent.RightClick();
			
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

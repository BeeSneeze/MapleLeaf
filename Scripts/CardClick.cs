using Godot;
using System;

public class CardClick : Control
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public override void _GuiInput(InputEvent @event) // On mouse clicks.
	{
		Card CardParent = (Card)GetParent();

		if (@event is InputEventMouseButton mb)
		{
			// Left mouse button
			if (mb.ButtonIndex == 1 && mb.Pressed)
			{
				CardParent.BigMode(false);
			}
			
			// Right mouse button
			if (mb.ButtonIndex == 2 && mb.Pressed)
			{
				CardParent.BigMode(true);
			}
			
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

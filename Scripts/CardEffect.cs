using Godot;
using System;

public class CardEffect : AnimatedSprite
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Play();
	}

	private void _on_Node2D_animation_finished()
	{
		QueueFree();
	}
}




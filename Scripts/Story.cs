using Godot;
using System;

public class Story : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private AnimatedSprite Image;
	private Label BottomText;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Image = (AnimatedSprite)GetNode("Slides");
		BottomText = (Label)GetNode("Label");
	}

	public void StartStory(string StoryName)
	{
		GD.Print("ATTEMPTED TO START STORY: " + StoryName);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

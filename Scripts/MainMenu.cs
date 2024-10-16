using Godot;
using System;

public class MainMenu : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private float CountDownTime = 0.0f;
	private bool CountingDown = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Node2D FirstTimeCover = GetNode<Node2D>("Blackness");
		FirstTimeCover.Show();
	}

	public void StartButton()
	{
		LevelManager LM = GetParent<LevelManager>();
		LM.ChangeLevel("Game");
	}

	public void StartCountDown()
	{
		CountDownTime = 1.4f;
		CountingDown = true;
	}

	public override void _Process(float delta)
	{
		if(CountingDown)
		{
			CountDownTime-=delta;
		}

		if(CountDownTime < 0)
		{
			Node2D FirstTimeCover = GetNode<Node2D>("Blackness");
			FirstTimeCover.Hide();
			CountingDown = false;
			CountDownTime = 1.0f;
		}	
	}
}




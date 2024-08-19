using Godot;
using System;

public class LevelManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public MusicManager MM;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MM = (MusicManager)GetParent().GetNode("MusicManager");
	}

	public void ChangeLevel(string InString)
	{
		GD.Print("LEVEL CHANGED:" + InString);
		foreach(Node2D child in GetChildren())
		{
			child.Hide();
		}
		Node2D NewLevel = (Node2D)GetNode(InString);
		NewLevel.Show();

		MM.PlayMusic(InString);
	}


	// Debug buttons for changing between levels
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Y)
			{
				ChangeLevel("WorldMap");
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.U)
			{
				ChangeLevel("PauseMenu");
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.I)
			{
				ChangeLevel("MainMenu");
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.O)
			{
				ChangeLevel("Game");
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
			{
				GetTree().Quit();
			}
			
		}	
	}

	

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

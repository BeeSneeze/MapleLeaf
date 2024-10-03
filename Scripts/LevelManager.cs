using Godot;
using System;

public class LevelManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public MusicManager MM;

	private string CurrentLevel = "Story";
	private string PreviousLevel = "MainMenu";

	private Story SM;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SM = GetNode<Story>("Story");
		MM = (MusicManager)GetParent().GetNode("MusicManager");
		EventManager.Instance.Connect("LevelChange", this, "ChangeLevel");
		EventManager.Instance.Connect("BackTrack", this, "BackTrack");
	}

	public void ChangeLevel(string InString)
	{
		PreviousLevel = CurrentLevel;
		CurrentLevel = InString;
		GD.Print("LEVEL CHANGED:" + InString);
		foreach(Node2D child in GetChildren())
		{
			child.Hide();
		}
		Node2D NewLevel = GetNode<Node2D>(InString);
		NewLevel.Show();

		MM.PlayMusic(InString);

		PauseMenu PauseMenu = GetNode<PauseMenu>("PauseMenu");
		if(InString != "PauseMenu")
		{
			PauseMenu.ToggleMainMenuButton(InString != "MainMenu");
		}
	}

	public void BackTrack()
	{
		ChangeLevel(PreviousLevel);
	}


	// Debug buttons for changing between levels
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.Y)
			{
				ChangeLevel("MainMenu");
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.U)
			{
				SM.StartStory("Start");
				ChangeLevel("Story");
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.P)
			{
				SM.StartStory("Ending");
				ChangeLevel("Story");
			}
		}	
	}

}

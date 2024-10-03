using Godot;
using System;

public class PauseMenu : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	
	CanvasItem MainMenuButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MainMenuButton = GetNode<CanvasItem>("MainMenuButton");
	}

	public void BackToMain()
	{
		LevelManager LM = GetParent<LevelManager>();
		LM.ChangeLevel("MainMenu");
		
	}

	public void ToggleMainMenuButton(bool InBool)
	{
		if(InBool)
		{
			GD.Print("SHOWING BUTTON");
			MainMenuButton.Show();
		}
		else
		{
			GD.Print("HIDING BUTTON");
			MainMenuButton.Hide();
		}
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

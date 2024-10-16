using Godot;
using System;

public class GameOver : Sprite
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void GoBackToMain()
	{
		LevelManager LM = GetParent<LevelManager>();

		LM.ChangeLevel("MainMenu");
	}
}



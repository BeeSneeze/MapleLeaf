using Godot;
using System;

public class MusicManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	string ActiveSong;

	AudioStreamPlayer PauseMenu, MainMenu, WorldMap, Game;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PauseMenu = (AudioStreamPlayer)GetNode("PauseMenu");
		MainMenu = (AudioStreamPlayer)GetNode("MainMenu");
		WorldMap = (AudioStreamPlayer)GetNode("WorldMap");
		Game = (AudioStreamPlayer)GetNode("Game");
	}

	public void PlayMusic(string InString)
	{
		if(ActiveSong==InString)
		{
			return;
		}

		PauseMenu.Stop();
		MainMenu.Stop();
		WorldMap.Stop();
		Game.Stop();

		switch(InString)
		{
			case "PauseMenu":
				PauseMenu.Play();
			break;
			case "MainMenu":
				MainMenu.Play();
			break;
			case "WorldMap":
				WorldMap.Play();
			break;
			case "Game":
				Game.Play();
			break;
		}

		ActiveSong = InString;
	}




//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

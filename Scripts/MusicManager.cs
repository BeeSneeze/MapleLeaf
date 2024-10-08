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
		PauseMenu = GetNode<AudioStreamPlayer>("PauseMenu");
		MainMenu = GetNode<AudioStreamPlayer>("MainMenu");
		WorldMap = GetNode<AudioStreamPlayer>("WorldMap");
		Game = GetNode<AudioStreamPlayer>("Game");
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

	public void UpdateVolume(float Volume)
	{
		PauseMenu.VolumeDb = Volume;
		MainMenu.VolumeDb = Volume;
		WorldMap.VolumeDb = Volume;
		Game.VolumeDb = Volume;
	}
	
}

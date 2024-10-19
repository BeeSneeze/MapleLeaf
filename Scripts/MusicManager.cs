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
		UpdateVolume(0.0f);
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

	private float[] HardCodedModifiers = {-1.0f,-3.0f,-3.0f,-3.0f};

	public void UpdateVolume(float Volume)
	{
		PauseMenu.VolumeDb = Volume + HardCodedModifiers[0];
		MainMenu.VolumeDb = Volume + HardCodedModifiers[1];
		WorldMap.VolumeDb = Volume + HardCodedModifiers[2];
		Game.VolumeDb = Volume + HardCodedModifiers[3];
	}
	
}

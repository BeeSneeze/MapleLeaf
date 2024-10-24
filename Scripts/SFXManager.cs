using Godot;
using System;

public class SFXManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	AudioStreamPlayer Sound;

	static float SFXVolume = 10.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateVolume(0.0f);
	}

	public void PlaySFX(string SFXName)
	{
		Sound = GetNode<AudioStreamPlayer>(SFXName);
		Sound.Play();
	}

	public void UpdateVolume(float Volume)
	{
		for(int i = 1; i < 9; i++)
		{
			AudioStreamPlayer SoundByte = GetChild<AudioStreamPlayer>(i);

			SoundByte.VolumeDb = Volume - 3.0f;
		}
	}
}

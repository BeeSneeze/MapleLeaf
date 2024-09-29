using Godot;
using System;

public class SFXManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	AudioStreamPlayer Sound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void PlaySFX(string SFXName)
	{
		Sound = GetNode<AudioStreamPlayer>(SFXName);
		Sound.Play();
	}
}

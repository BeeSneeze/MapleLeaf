using Godot;
using System;
using System.Collections.Generic;

public class Options : OptionButton
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	[Export] public List<string> AllOptions;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach(string opt in  AllOptions)
		{
			AddItem(opt);
		}

		Selected = 2;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

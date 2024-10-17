using Godot;
using System;
using System.Collections.Generic;

public class Options : OptionButton
{
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
	
}

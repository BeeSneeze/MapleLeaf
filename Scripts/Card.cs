using Godot;
using System;

public class Card : Sprite
{

	public string Name;
	public int OwnerID;

	// Card specifics
	private string MatrixName;
	private int Range;
	private string TargetType;
	private string FlavorText;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load JSON stuff from the name
	}

	public void PlayCard()
	{
		GD.Print("Card Played!");
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

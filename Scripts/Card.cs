using Godot;
using System;

public class Card : Sprite
{

	public string CardName;
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
		GD.Print("Card Played: " + CardName);
	}

	public void BigMode(bool InBool)
	{
		if(InBool)
		{
			GD.Print("Card Went BigMode");
			Scale = new Vector2(1.0f, 1.0f);
			ZIndex = 101;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(1.0f, 1.0f);
		}
		else
		{
			Scale = new Vector2(0.5f, 0.5f);
			ZIndex = 100;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(0.0f, 0.0f);
		}
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
using Godot;
using System;

public class Tile : Node2D
{	
	public int X = 0;
	public int Y = 0;

	private string Terrain;
	public string Character;
	public int CharID {get; private set;}

	public int MaxHP;
	public int HP;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		Terrain = "Grass";
		Translate(new Vector2(X,Y));
	}

	public void SetTerrain(string InString)
	{
		Terrain = InString;
		Sprite spr = (Sprite)GetNode("Terrain");
		spr.Texture = (Texture)GD.Load("res://Assets/Visuals/Terrain/" + InString + ".png");
		if(InString == "Mountains")
		{
			AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Character");
			AnimSpr.Animation = "Mountain";
		}

	}

	public void SetCharacter(int InInt)
	{
		CharID = InInt;
		// Change the animated sprite

		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Character");

		// Character IDs: 0 = Nothing, 1-9 = friendly characters, 10-49 = enemy characters, 50+ = misc.
		switch(InInt % 100)
		{
			case 0:
				AnimSpr.Animation = "None";
				AnimSpr.ZIndex = 0;
			break;
			case 1:
				AnimSpr.Animation = "Soldier";
				AnimSpr.ZIndex = 20;
			break;
			case 2:
				AnimSpr.Animation = "Sniper";
				AnimSpr.ZIndex = 20;
			break;
			case 3:
				AnimSpr.Animation = "Support";
				AnimSpr.ZIndex = 20;
			break;
			case 4:
				AnimSpr.Animation = "City";
				AnimSpr.ZIndex = 0;
			break;
			case 10:
				AnimSpr.Animation = "RatTutorial";
				AnimSpr.ZIndex = 0;
			break;
		}
	}

	public void SetMarker(string InString)
	{
		// Change the animated sprite
		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Marker");
		AnimSpr.Animation = InString;
	}

	public void RightClick()
	{
		GD.Print("Tile right clicked!");
	}

	public void LeftClick()
	{
		GD.Print("Tile left clicked!");
	}
	
}

using Godot;
using System;

public class Tile : Node2D
{	
	public int X = 0;
	public int Y = 0;

	private string Terrain;
	public string Character;
	public int CharID {get; private set;}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		Terrain = "Grass";
		Translate(new Vector2(X,Y));
	}


	public void SetTerrain(string InString)
	{
		Terrain = InString;
		var img = (Texture)GD.Load("res://Assets/Visuals/Terrain/" + InString + ".png");
		Sprite spr = (Sprite)GetNode("Terrain");
		spr.SetTexture(img);
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
			break;
			case 1:
				AnimSpr.Animation = "Soldier";
			break;
			case 2:
				AnimSpr.Animation = "Sniper";
			break;
			case 3:
				AnimSpr.Animation = "Support";
			break;
			case 4:
				AnimSpr.Animation = "City";
			break;
			case 10:
				AnimSpr.Animation = "RatTutorial";
			break;
		}
	}

	public void SetMarker(string InString)
	{
		// Change the animated sprite
		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Marker");
		AnimSpr.Animation = InString;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

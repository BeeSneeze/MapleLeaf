using Godot;
using System;

public class Tile : Node2D
{	
	public int X = 0;
	public int Y = 0;

	private string Terrain;
	private string Character;
	private int CharID;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		Terrain = "Grass";
		Translate(new Vector2(X,Y));
	}


	public void SetTerrain(string InString)
	{
		Terrain = InString;
		var img = (Texture)GD.Load("res://Assets/Terrain/" + InString + ".png");
		Sprite spr = (Sprite)GetNode("Terrain");
		spr.SetTexture(img);
	}

	public void SetCharacter(int InInt)
	{
		CharID = InInt;
		// Change the animated sprite

		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Character");

		if(InInt % 100 == 10)
		{
			AnimSpr.Animation = "RatTutorial";
		}

		


	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

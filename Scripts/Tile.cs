using Godot;
using System;

public class Tile : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	
	public int X = 0;
	public int Y = 0;

	private string Terrain;

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

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

using Godot;
using System;



public class Board : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	const int MaxSize = 8;

	public override void _Ready()
	{
		
		//Translate(new Vector2(GetWindow().size.x/2,GetWindow().size.y/2));

		Translate(new Vector2(800,450));

		// Spawn all the tile objects
		var scene = GD.Load<PackedScene>("res://Scenes/Tile.tscn");

		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				Tile tile = (Tile)scene.Instance();
				tile.X = x*106-371;
				tile.Y = y*106-371;

				AddChild(tile);
			}
		}
		

		
		
		//_sprite2D = new Sprite2D(); // Create a new Sprite2D.
		//AddChild(_sprite2D); // Add it as a child of this node.
	}
	
	//Tile[] TileMatrix = new Tile[8];

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

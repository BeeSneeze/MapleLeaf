using Godot;
using System;


public class Board : Node2D
{
	const int MaxSize = 8;
	Tile[,] TileMatrix = new Tile[8,8];

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
				tile.X = x*94-94*4+47; // 90x90 pixel boxes + 4 pixel margin between
				tile.Y = y*94-94*4+47;

				TileMatrix[x,y] = tile;

				AddChild(tile);
			}
		}

		TileMatrix[1,2].SetTerrain("Mountains");
		TileMatrix[1,3].SetTerrain("Mountains");
		TileMatrix[6,2].SetTerrain("Mountains");
		TileMatrix[6,3].SetTerrain("Mountains");
		
	}

	// Swaps characters between two tiles, useful for movement
	public void SwapTiles(int X1, int Y1, int X2, int Y2)
	{
		int CharID1 = TileMatrix[X1,Y1].CharID;
		int CharID2 = TileMatrix[X2,Y2].CharID;
		TileMatrix[X1,Y1].SetCharacter(CharID2);
		TileMatrix[X2,Y2].SetCharacter(CharID1);
	}



	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.W)
			{
				TileMatrix[2,2].SetCharacter(103);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.A)
			{
				SwapTiles(2,2,5,5);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.S)
			{
				TileMatrix[2,3].SetCharacter(102);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.D)
			{
				TileMatrix[2,4].SetCharacter(101);
			}
		}
	}
	
	//Tile[] TileMatrix = new Tile[8];

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

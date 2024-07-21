using Godot;
using System;


public class Board : Node2D
{
	const int MaxSize = 8;
	Tile[,] Cell = new Tile[8,8]; // A matrix containing all of the tiles on the board
	bool[,] ActionMatrix = new bool[8,8]; // Matrix saying which tiles are affected by an action

	public override void _Ready()
	{
		//Translate(new Vector2(GetWindow().size.x/2,GetWindow().size.y/2));
		//Translate(new Vector2(800,450));

		// Spawn all the tile objects
		var scene = GD.Load<PackedScene>("res://Scenes/Tile.tscn");

		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				Tile tile = (Tile)scene.Instance();
				tile.X = x*74-74*4+35+2; // 70x70 pixel boxes + 4 pixel margin between
				tile.Y = y*74-74*4+35+2;

				Cell[x,y] = tile;

				AddChild(tile);
			}
		}

		Cell[2,0].SetTerrain("Mountains");
		Cell[2,1].SetTerrain("Mountains");
		Cell[2,2].SetTerrain("Mountains");
		Cell[2,3].SetTerrain("Mountains");

		Cell[7,1].SetCharacter(104);
		Cell[1,0].SetCharacter(204);
		Cell[3,5].SetCharacter(304);

		Cell[3,6].SetCharacter(110);
		Cell[6,1].SetCharacter(210);
		Cell[1,1].SetCharacter(310);
		Cell[3,4].SetCharacter(410);
		Cell[2,7].SetCharacter(510);

		GameManager GM = (GameManager)GetParent();

		bool[,] PatchTest = GM.LoadMatrix("Full", 1);

		Cell[2,4].SetCharacter(103);

		RotCounter(PatchTest);
		//RotCounter(PatchTest);

		Patch(PatchTest, ActionMatrix, new Vector2(2,4));

		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				if(ActionMatrix[x,y])
				{
					Cell[x,y].SetMarker("Attack");
				}
			}
		}
		
	}

	// Remove all of the markers
	public void ClearMarkers()
	{
		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				Cell[x,y].SetMarker("None");
			}
		}
	}

	// Places a matrix inside of a bigger matrix
	// Both are assumed to be square, the input matrix is assumed to have odd dimensions
	// By default, the input matrix is only additive. Turn on replace to allow for removal via input matrix
	public void Patch(bool[,] InMat, bool[,] OutMat, Vector2 CPos, bool Replace = false)
	{
		int CIndex = (InMat.GetLength(0) - 1)/2;
		
		int OffsetX = (int)CPos.x-CIndex;
		int OffsetY = (int)CPos.y-CIndex;

		for(int x = 0; x < InMat.GetLength(0); x++)
		{
			for(int y = 0; y < InMat.GetLength(0); y++)
			{
				bool BoxTestX = x+OffsetX >= 0 && x+OffsetX < OutMat.GetLength(0);
				bool BoxTestY = y+OffsetY >= 0 && y+OffsetY < OutMat.GetLength(1);

				if(BoxTestX && BoxTestY)
				{
					if(InMat[x,y] || Replace)
					{
						OutMat[x+OffsetX, y+OffsetY] = InMat[x,y];
					}
				}
			}
		}
	}

	// Rotate a matrix counter clock-wise
	public void RotCounter(bool[,] InMat)
	{
		bool[,] OldMat = (bool[,])InMat.Clone();
		int MatSize = InMat.GetLength(0);

		for(int x = 0; x < MatSize; x++)
		{
			for(int y = 0; y < MatSize; y++)
			{
				InMat[x,y] = OldMat[MatSize-1-y,x];
			}
		}
	}

	// Rotate a matrix clockwise
	public void RotClock(bool[,] InMat)
	{
		bool[,] OldMat = (bool[,])InMat.Clone();
		int MatSize = InMat.GetLength(0);

		for(int x = 0; x < MatSize; x++)
		{
			for(int y = 0; y < MatSize; y++)
			{
				InMat[x,y] = OldMat[y,MatSize-1-x];
			}
		}
	}

	// Swaps characters between two tiles, useful for movement
	public void SwapTiles(int X1, int Y1, int X2, int Y2)
	{
		int CharID1 = Cell[X1,Y1].CharID;
		int CharID2 = Cell[X2,Y2].CharID;
		Cell[X1,Y1].SetCharacter(CharID2);
		Cell[X2,Y2].SetCharacter(CharID1);
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.W)
			{
				Cell[2,2].SetCharacter(103);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.A)
			{
				SwapTiles(2,2,5,5);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.S)
			{
				Cell[2,3].SetCharacter(102);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.D)
			{
				Cell[2,4].SetCharacter(101);
			}
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Board : Node2D
{
	const int MaxSize = 8;
	Tile[,] Cell = new Tile[8,8]; // A matrix containing all of the tiles on the board
	bool[,] ActionMatrix = new bool[8,8]; // Matrix saying which tiles are affected by an action

	public Dictionary<string,CharacterInfo> AllCharacters; // Contains relevant info on characters, loaded via Characters.JSON
	// RAT MATRICES?

	public List<Vector2> TargetList = new List<Vector2>(); // List of all targets selected for a given card action, to be used in ExecutePlay

	public override void _Ready()
	{
		// Load Character Info
		File Reader = new File();
		Reader.Open("res://Assets/Characters.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		AllCharacters = JsonConvert.DeserializeObject<Dictionary<string,CharacterInfo>>(Contents);
		Reader.Close();

		// Spawn all the tile objects
		var scene = GD.Load<PackedScene>("res://Scenes/Tile.tscn");

		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				Tile tile = (Tile)scene.Instance();
				tile.X = x;
				tile.Y = y;


				Cell[x,y] = tile;

				AddChild(tile);
			}
		}

		Cell[2,0].SetTerrain("Mountains");
		Cell[2,1].SetTerrain("Mountains");
		Cell[2,2].SetTerrain("Mountains");
		Cell[2,3].SetTerrain("Mountains");

		Cell[4,3].CreateCharacter("Soldier");
		Cell[5,4].CreateCharacter("Sniper");
		Cell[2,4].CreateCharacter("Support");

		Cell[7,1].CreateCharacter("City");
		Cell[1,0].CreateCharacter("City");
		Cell[3,5].CreateCharacter("City");

		Cell[3,6].CreateCharacter("RatTutorial");
		Cell[6,1].CreateCharacter("RatTutorial");
		Cell[1,1].CreateCharacter("RatTutorial");
		Cell[3,4].CreateCharacter("RatTutorial");
		Cell[2,7].CreateCharacter("RatTutorial");
		
		Swap(new Vector2(1,1), new Vector2(2,6));
	}

	// Get the position of a specific character
	public Vector2 GetCharPos(int InID)
	{
		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				if(Cell[x,y].Char.ID == InID)
				{
					return new Vector2(x,y);
				}
			}
		}
		return new Vector2(0,0);
	}

	public void AddTarget(Vector2 Vec)
	{
		if(!TargetList.Contains(Vec))
		{
			TargetList.Add(Vec);
		}
	}

	// Removes parts of the matrix depending on certain keywords
	public void Remove(bool[,] InMat, string Type)
	{
		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				if(Cell[x,y].Char.ID > 0 && Type == "Occupied")
				{
					InMat[x,y] = false;
				}
				if(Cell[x,y].Char.ID == 50 && Type == "Mountain")
				{
					InMat[x,y] = false;
				}
			}
		}
	}

	// Visualize what a card does
	public void ShowMatrix(bool[,] InMat, Card Card)
	{
		Vector2 Center = GetCharPos(Card.PlayerID);

		ClearMarkers();
		Patch(InMat, ActionMatrix, Center);

		Cell[(int)Center.x,(int)Center.y].SetMarker("Select");
		
		Cell[(int)Center.x,(int)Center.y].Clickable = false;

		if(Card.AbilityList.Count == 0)
			return;

		// Remove parts of the matrix according to who the card targets
		switch(Card.TargetCell)
		{
			case "Empty":
				Remove(ActionMatrix, "Occupied");
			break;
			case "Enemy":
				Remove(ActionMatrix, "Mountain");
			break;
		}
		
		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				if(ActionMatrix[x,y])
				{
					switch(Card.AbilityList[0].Name)
					{
						case "Move":
							Cell[x,y].SetMarker("Move");
						break;
						case "Damage":
							Cell[x,y].SetMarker("Attack");
						break;
					}
					
				}
			}
		}
	}

	// Remove all of the markers
	public void ClearMarkers()
	{
		TargetList = new List<Vector2>();
		ActionMatrix = new bool[8,8];
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


	// ABILITY FUNCTIONS

	// Applies an amount of damage to a given tile
	public void Damage(Vector2 InVec, int Amount)
	{
		Cell[(int)InVec.x, (int)InVec.y].TakeDamage(Amount);
	}

	// Swaps characters between two tiles, useful for movement
	public void Swap(Vector2 Vec1, Vector2 Vec2)
	{
		Character Char1 = Cell[(int)Vec1.x,(int)Vec1.y].Char;
		Character Char2 = Cell[(int)Vec2.x,(int)Vec2.y].Char;

		Cell[(int)Vec1.x,(int)Vec1.y].SetCharacter(Char2);
		Cell[(int)Vec2.x,(int)Vec2.y].SetCharacter(Char1);
	}

}

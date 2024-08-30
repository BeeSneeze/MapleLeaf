using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Board : Node2D
{
	private const int MaxSize = 8;
	public Tile[,] Cell = new Tile[8,8]; // A matrix containing all of the tiles on the board
	public bool[,] ActionMatrix = new bool[8,8]; // Matrix saying which tiles are affected by an action

	public Dictionary<string,CharacterInfo> AllCharacters; // Contains relevant info on characters, loaded via Characters.JSON
	// RAT MATRICES?

	public List<Vector2> TargetList = new List<Vector2>(); // List of all targets selected for a given card action, to be used in ExecutePlay
	public List<Arrow> QueuedMoves = new List<Arrow>();

	private int[,] TheoreticalCellID = new int[8,8];

	private GameManager GM;

	public override void _Ready()
	{
		GM = (GameManager)GetParent();

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

		LoadStage("Calgary");

		LoadTheoretical();
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
	private void Remove(bool[,] InMat, bool[,] PossibleMat, string Type)
	{
		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				int CID = Cell[x,y].Char.ID % 100;

				if(InMat[x,y]) // Only activate if there is anything to remove. Needed for PossibleMat
				{
					if(CID > 0 && Type == "Occupied")
					{
						InMat[x,y] = false;
						PossibleMat[x,y] = true;
					}
					if(CID == 50 && Type == "Mountain")
					{
						InMat[x,y] = false;
						PossibleMat[x,y] = true;
					}
					if(CID == 0 && Type == "Empty")
					{
						InMat[x,y] = false;
						PossibleMat[x,y] = true;
					}
					if(CID > 9 && CID < 49 && Type == "Rat")
					{
						InMat[x,y] = false;
						PossibleMat[x,y] = true;
					}
					if(CID < 9 && Type == "Friendly")
					{
						InMat[x,y] = false;
						PossibleMat[x,y] = true;
					}
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

		if(Card.MatrixName != "Global")
		{
			if(Card.TargetType == "Area")
			{
				Cell[(int)Center.x,(int)Center.y].SetMarker("SelectClickable");
			}
			else
			{
				Cell[(int)Center.x,(int)Center.y].SetMarker("Select");
			}
		}
		

		if(Card.AbilityList.Count == 0)
			return;

		bool[,] PossibleMat = new bool[8,8]; // Used to show a move as possible, if  other characters were involved
		bool[,] Impossible = new bool[8,8]; // Dummy matrix used to *not* show a move as possible

		// Remove parts of the matrix according to who the card targets
		switch(Card.TargetCell)
		{
			case "Empty":
				Remove(ActionMatrix, Impossible, "Occupied");
			break;
			case "Enemy":
				Remove(ActionMatrix, PossibleMat, "Mountain");
				Remove(ActionMatrix, PossibleMat, "Empty");
				if(Card.PlayerID % 100 < 10)
				{
					Remove(ActionMatrix, PossibleMat, "Friendly");
				}
				else
				{
					Remove(ActionMatrix, PossibleMat, "Rat");
				}
			break;
			case "Friendly":
				Remove(ActionMatrix, Impossible, "Mountain");
				Remove(ActionMatrix, PossibleMat, "Empty");
				if(Card.PlayerID % 100 < 10)
				{
					Remove(ActionMatrix, PossibleMat, "Rat");
				}
				else
				{
					Remove(ActionMatrix, PossibleMat, "Friendly");
				}
			break;
			case "Character":
				Remove(ActionMatrix, PossibleMat, "Mountain");
				Remove(ActionMatrix, PossibleMat, "Empty");
			break;
		}
		
		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{

				// Show moves that would be possible, if only the right characters were present on those tiles
				if(PossibleMat[x,y])
				{
					switch(Card.AbilityList[0].Name)
					{
						case "Move":
							Cell[x,y].SetMarker("PossibleMove");
						break;
						case "Damage":
							Cell[x,y].SetMarker("PossibleAttack");
						break;
						case "Stun":
							Cell[x,y].SetMarker("PossibleHarm");
						break;
						case "Spawn":
							Cell[x,y].SetMarker("PossibleSpawn");
						break;
					}
					if(Card.TargetCell == "Friendly")
					{
						Cell[x,y].SetMarker("PossibleSupport");
					}
				}


				if(ActionMatrix[x,y])
				{
					
					// If it's an areal attack, add every single tile to the target list
					if(Card.TargetType == "Area")
					{
						AddTarget(new Vector2(x,y));
					}


					switch(Card.AbilityList[0].Name)
					{
						case "Move":
							Cell[x,y].SetMarker("Move");
						break;
						case "Damage":
							Cell[x,y].SetMarker("Attack");
						break;
						case "Stun":
							Cell[x,y].SetMarker("Harm");
						break;
						case "Spawn":
							Cell[x,y].SetMarker("Spawn");
						break;
					}

					if(Card.TargetCell == "Friendly")
					{
						Cell[x,y].SetMarker("Support");
					}
					
				}
				
			}
		}
	}

	// Remove all of the markers, and resets the target list
	public void ClearMarkers()
	{
		TargetList = new List<Vector2>();
		ActionMatrix = new bool[8,8];
		QueuedMoves = new List<Arrow>();
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

	// Preps the theoretical ID matrix
	public void LoadTheoretical()
	{
		for(int x = 0; x < MaxSize; x++)
		{
			for(int y = 0; y < MaxSize; y++)
			{
				TheoreticalCellID[x,y] = Cell[x,y].Char.ID;
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

		if(Char1.ID % 100 == 4 || Char2.ID % 100 == 4)
		{
			return; // Cities are immovable
		}

		Cell[(int)Vec1.x,(int)Vec1.y].SetCharacter(Char2);
		Cell[(int)Vec2.x,(int)Vec2.y].SetCharacter(Char1);
	}


	// Push a character a given amount of squares
	public bool Push(Vector2 Tile, Vector2 OrignalTile, string Direction, int Amount)
	{
		int TargetX = (int)Tile.x;
		int TargetY = (int)Tile.y;

		switch(Direction)
		{
			case "N":
				TargetY-=1;
			break;
			case "NE":
				TargetX+=1;
				TargetY-=1;
			break;
			case "E":
				TargetX+=1;
			break;
			case "SE":
				TargetX+=1;
				TargetY+=1;
			break;
			case "S":
				TargetY+=1;
			break;
			case "SW":
				TargetX-=1;
				TargetY+=1;
			break;
			case "W":
				TargetX-=1;
			break;
			case "NW":
				TargetX-=1;
				TargetY-=1;
			break;
		}

		if(TargetX < 0 || TargetX > 7 || TargetY < 0 || TargetY > 7)
		{
			GD.Print("OOB");
			GD.Print(OrignalTile);
			GD.Print(new Vector2(TargetX,TargetY));
			// Out of bounds
			return false; 
		}

		if(TheoreticalCellID[TargetX,TargetY] % 100 > 49 || TheoreticalCellID[TargetX,TargetY] % 100 == 4)
		{
			GD.Print("SOLID OBJECT");
			GD.Print(OrignalTile);
			GD.Print(new Vector2(TargetX,TargetY));
			return false; // Hit an immovable object
		}


		if(TheoreticalCellID[TargetX,TargetY] != 0)
		{
			GD.Print("CHARACTER PUSH");
			GD.Print(OrignalTile);
			GD.Print(new Vector2(TargetX,TargetY));
			// Recurse over the found character
			bool Result = false;
			Result = Push(new Vector2(TargetX, TargetY), new Vector2(TargetX, TargetY), Direction, Amount);

			if(!Result)
			{
				return false; // If blocked by the character, return false
			}
			
		}


		if(Amount > 1)
		{
			GD.Print("CONTINUED PUSH");
			GD.Print(OrignalTile);
			GD.Print(new Vector2(TargetX,TargetY));
			// Attempt another push from the next square
			bool Result = false;
			Result = Push(new Vector2(TargetX, TargetY), OrignalTile, Direction, Amount-1);
			
			if(!Result) // Hit a dead end, stop recursing and assign the queued move
			{
				/*Arrow NewArr;
				NewArr.From = OrignalTile;
				NewArr.To = new Vector2(TargetX,TargetY);
				QueuedMoves.Add(NewArr);*/
			}
		}
		else
		{
			GD.Print("ENDED PUSH");
			GD.Print(OrignalTile);
			GD.Print(new Vector2(TargetX,TargetY));
			// Just push the character one step, and then it's done
			Arrow NewArr;
			NewArr.From = OrignalTile;
			NewArr.To = new Vector2(TargetX,TargetY);
			QueuedMoves.Add(NewArr);

			TheoreticalCellID[TargetX, TargetY] = TheoreticalCellID[(int)Tile.x, (int)Tile.y];
			TheoreticalCellID[(int)Tile.x, (int)Tile.y] = 0;
		}

		return true; // Successful push!
		
	}

	public void Stun(Vector2 Position)
	{

	}

	public void Spawn(string SpawnName, Vector2 Pos)
	{
		Cell[(int)Pos.x,(int)Pos.y].CreateCharacter(SpawnName);
	}

	// Moves all the queued moves at once
	public void MoveQueue()
	{
		List<Character> MoveChars = new List<Character>();

		// First save all the characters in a list
		foreach(Arrow A in QueuedMoves)
		{
			GD.Print("QUEUED MOVE:");
			GD.Print(A.From);
			GD.Print(A.To);
			MoveChars.Add(Cell[(int)A.From.x,(int)A.From.y].Char);
			Cell[(int)A.From.x,(int)A.From.y].CreateCharacter("None");
		}

		// Then set all the characters from the list to the corresponding spot
		int index = 0;
		foreach(Arrow A in QueuedMoves)
		{
			Cell[(int)A.To.x,(int)A.To.y].SetCharacter(MoveChars[index++]);
		}
	}



	public void LoadStage(string StageName)
	{
		File Reader = new File();
		Reader.Open("res://Assets/Matrices/Stages/" + StageName + ".txt", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		Reader.Close();

		int x = 0;
		int y = 0;

		foreach(char C in Contents)
		{
			// We explicitly check for 1's and 0's, as line ends are two or just one character depending on OS
			switch(C)
			{
				case '0':
					//Cell[x,y].SetTerrain("Mountains");
					x++;
				break;
				case '1':
					Cell[x,y].CreateCharacter("Soldier");
					x++;
				break;
				case '2':
					Cell[x,y].CreateCharacter("Sniper");
					x++;
				break;
				case '3':
					Cell[x,y].CreateCharacter("Support");
					x++;
				break;
				case '4':
					Cell[x,y].CreateCharacter("City");
					x++;
				break;
				case '5':
					Cell[x,y].CreateCharacter("RatNormal");
					x++;
				break;
				case '6':
					Cell[x,y].SetTerrain("Mountains");
					x++;
				break;
				case '\n':
					// Line break, new row to the matrix
					x = 0;
					y++;
				break;
			}

		}
	}

	

}

using Godot;
using System;
using System.Collections.Generic;

public class AI : Node2D
{

	private int[,] MoveRat = new int[8,8];

	private GameManager GM;
	private Board Board;
	private CardManager CM;

	private List<int> ExaminedCards = new List<int>();
	private int[,] RatPatch = new int[8,8];
	

	private Card ActiveCard;
	private int CardFlag = 0;
	private int ClickNum = 0;
	private bool MoveMode = false;
	private bool SuccessfulAction = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = (GameManager)GetParent();
		CM = (CardManager)GM.GetNode("CardsRat");
		Board = (Board)GM.GetNode("Board");

		EvaluateBoard();
	}

	float Timer = 0;
	static float TurnTime = 0.3f; // How long inbetween AI Clicks

	// Called every frame
	public override void _Process(float delta)
	{
		Timer += delta;

		if(Timer > TurnTime)
		{
			Timer = 0;

			if(MoveMode)
			{
				MoveClick();
			}
		}
	}
	
	// Prepares to start the move phase
	public void StartMoveMode()
	{
		ExaminedCards = new List<int>();
		CardFlag = 1;
		MoveMode = true;
		ClickNum = 1;
	}

	// The action loop for the move phase. This includes playing move cards, and spawn cards
	public void MoveClick()
	{
		
		if(ClickNum == 1)
		{
			// CLICK ONE
			SuccessfulAction = true;
			CardFlag = ClickMoveCard();
			ClickNum = 2;
		}
		else if(ClickNum == 2)
		{
			// CLICK TWO
			if(CardFlag == 1)
			{
				SuccessfulAction = MoveBest();
			}
			else if (CardFlag == 2)
			{
				SuccessfulAction = SpawnBest();
			}

			if(!SuccessfulAction)
			{
				ActiveCard.LeftClick();
				ActiveCard.Skip(true);
				// UNCLICK CARD BEFORE MOVING ON
			}
			else
			{
				if(ActiveCard.Uses > 0)
				{
					ExaminedCards.Remove(ActiveCard.CardID);
				}
				
			}
			SuccessfulAction = false;
			ClickNum = 1;
			if(CardFlag == 0)
			{
				MoveMode = false; // Ran out of cards
				GM.SetMode("Player");
			}
		}
	}

	// Reevaluates all positions to provide information to the AI about best moves
	public void EvaluateBoard()
	{
		List<Vector2> PatchPosList = new List<Vector2>();
		MoveRat = new int[8,8];
		for(int x = 0; x < 8; x++)
		{
			for(int y = 0; y < 8; y++)
			{
				MoveRat[x,y] = 1000;
				if(Board.Cell[x,y].Char.ID % 100 < 10 && Board.Cell[x,y].Char.ID != 0)
				{
					PatchPosList.Add(new Vector2(x,y));
				}
				
			}
		}

		LoadPatch("Move");

		foreach(Vector2 Pos in PatchPosList)
		{
			PatchRatMove(RatPatch, MoveRat, Pos);
		}

	}

	// This clicks a card in the move phase
	public int ClickMoveCard()
	{
		// RETURN: 0 = Failed to find card, 1 = Found move card, 2 = Found Spawn card
		foreach(Card C in CM.HandCards)
		{
			if(ExaminedCards.Contains(C.CardID))
			{
				continue;
			}
			ExaminedCards.Add(C.CardID);
			foreach(Ability A in C.AbilityList)
			{
				if(A.Name == "Move")
				{
					C.LeftClick();
					ActiveCard = C;
					return 1;
					
				}
				if(A.Name == "Spawn")
				{
					GD.Print("FOUND SPAWN CARD!");
					C.LeftClick();
					ActiveCard = C;
					return 2;
				}
			}
			
		}
		return 0;
	}

	public void ClickTile(Vector2 TilePos)
	{
		Board.Cell[(int)TilePos.x,(int)TilePos.y].LeftClick();
	}


	// Move the closest possible to friendlies
	public bool MoveBest()
	{
		bool SuccessfulMove = false;
		Vector2 BestMove = new Vector2(0,0);
		int BestValue = 1000;

		for(int x = 0; x < 8; x++)
		{
			for(int y = 0; y < 8; y++)
			{
				if(Board.ActionMatrix[x,y])
				{
					if(BestValue > MoveRat[x,y])
					{
						BestValue = MoveRat[x,y];
						BestMove = new Vector2(x,y);
						SuccessfulMove = true;
					}
				}
			}
		}

		ClickTile(BestMove);
		return SuccessfulMove;
	}

	// Spawn the furthest away from friendlies
	public bool SpawnBest()
	{
		bool SuccessfulSpawn = false;
		Vector2 BestMove = new Vector2(0,0);
		int BestValue = -1000;

		for(int x = 0; x < 8; x++)
		{
			for(int y = 0; y < 8; y++)
			{
				if(Board.ActionMatrix[x,y])
				{
					if(BestValue < MoveRat[x,y])
					{
						BestValue = MoveRat[x,y];
						BestMove = new Vector2(x,y);
						SuccessfulSpawn = true;
					}
				}
			}
		}

		ClickTile(BestMove);
		return SuccessfulSpawn;
	}


	public void PatchRatMove(int[,] InMat, int[,] OutMat, Vector2 CPos)
	{
		int Penalty = 0;
		if(Board.Cell[(int)CPos.x,(int)CPos.y].Char.ID % 100 < 4)
		{
			Penalty = 1;
		}

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
					if(InMat[x,y] + Penalty < OutMat[x+OffsetX, y+OffsetY])
					{
						OutMat[x+OffsetX, y+OffsetY] = InMat[x,y] + Penalty;
					}
				}
			}
		}
	}

	// Loads the appropriate patch to determine move/attack priorities
	public void LoadPatch(string MatrixName)
	{
		RatPatch = new int[15,15];

		File Reader = new File();

		Reader.Open("res://Assets/Matrices/AI/" + MatrixName + ".txt", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		Reader.Close();

		int x = 0;
		int y = 0;

		foreach(int C in Contents)
		{
			if(C == 10)
			{
				// Line break, new row to the matrix
				x = 0;
				y++;
			}
			else
			{
				RatPatch[x,y] = C-64;
				x++;
			}

		}
	}
}
using Godot;
using System;
using System.Collections.Generic;

public class AI : Node2D
{

	private int[,] MoveRat = new int[8,8];
	private int[,] SpawnRat = new int[8,8];
	private int[,,] AttackRat = new int[7,8,8];

	private float Timer = 0;
	public float TurnTime = 0.6f; // How long inbetween AI Clicks
	public bool Paused = false;
	static float RandomVariation = 0.1f;

	private GameManager GM;
	private Board Board;
	private CardManager CM;

	private List<int> ExaminedCards = new List<int>();
	private int[,] RatPatch = new int[15,15];

	
	

	private Card ActiveCard;
	private int CardFlag = 0;
	private int ClickNum = 0;
	private bool MoveMode = false;
	private bool AttackMode = false;
	private bool SuccessfulAction = false;
	private Random rnd;

	private List<Card> QueuedActions = new List<Card>();
	private List<string> QueuedRotations = new List<string>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rnd = new Random();
		GM = (GameManager)GetParent();
		CM = (CardManager)GM.GetNode("CardsRat");
		Board = (Board)GM.GetNode("Board");
	}

	// Called every frame
	public override void _Process(float delta)
	{
		if(Paused)
			return;

		Timer += delta;

		if(Timer > TurnTime)
		{
			Timer = 0;

			if(MoveMode)
			{
				MoveClick();
			}
			if(AttackMode)
			{
				AttackClick();
			}
		}
	}
	
	// Prepares to start the move phase
	public void StartMoveMode()
	{
		ExaminedCards = new List<int>();
		QueuedActions = new List<Card>();
		QueuedRotations = new List<string>();
		CardFlag = 1;
		MoveMode = true;
		ClickNum = 1;
		EvaluateBoard();
	}

	// Prepares to start the attack phase
	public void StartAttackMode()
	{
		AttackMode = true;
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
			if(CardFlag == 1) // Movement card
			{
				SuccessfulAction = MoveBest(MoveRat);
			}
			else if (CardFlag == 2) // Attack card
			{
				SuccessfulAction = MoveBest(SpawnRat);
			}
			else if (CardFlag == 3) // Early Action card
			{
				GM.ExecutePlay();
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
				ExamineAllActions();
				MoveMode = false; // Ran out of cards
				GM.SetMode("Player");
			}
		}
	}


	// The action loop for the attack phase. This means clicking all remaining cards in hand
	public void AttackClick()
	{

		if(ClickNum == 1)
		{
			// CLICK ONE

			// Remove skipped cards from the list
			if(QueuedActions.Count > 0)
			{
				if(QueuedActions[0].Skipped)
				{
					QueuedActions[0].Skip(true);
					QueuedActions.RemoveAt(0);
					QueuedRotations.RemoveAt(0);
					ClickNum = 1;
					return;
				}
			}
			else
			{
				// Ran out of cards early due to skips, terminate attack phase
				AttackMode = false;
				GM.SetMode("Draw");
				return;
			}
			
			

			
			ClickNum = 2;
			QueuedActions[0].LeftClick();
			QueuedActions.RemoveAt(0);
			GM.Rotate(QueuedRotations[0]);
			QueuedRotations.RemoveAt(0);
		}
		else if(ClickNum == 2)
		{
			// CLICK TWO
			ClickNum = 1;
			GM.ExecutePlay();
			if(QueuedActions.Count == 0)
			{
				AttackMode = false; // Ran out of cards
				GM.SetMode("Draw");
			}
		}
	}

	// Reevaluates all positions to provide information to the AI about best moves
	public void EvaluateBoard()
	{
		List<Vector2> PatchPosList = new List<Vector2>();
		MoveRat = new int[8,8];
		SpawnRat = new int[8,8];
		AttackRat = new int[7,8,8];

		for(int x = 0; x < 8; x++)
		{
			for(int y = 0; y < 8; y++)
			{
				MoveRat[x,y] = 1000;
				if(Board.Cell[x,y].Char.ID % 100 < 10 && Board.Cell[x,y].Char.ID != 0)
				{
					PatchPosList.Add(new Vector2(x,y));
				}

				for(int Range = 0; Range < 7; Range++)
				{
					AttackRat[Range,x,y] = 1; // Since it's based on prime multiplication, set this to one
				}


			}
		}
		
		foreach(Vector2 Pos in PatchPosList)
		{
			LoadPatch("Move/SpawnNormal");
			PatchRatMove(RatPatch, SpawnRat, Pos);
			LoadPatch("Move/MoveNormal");
			PatchRatMove(RatPatch, MoveRat, Pos);
			
			// Attack patching
			LoadPatch("Attack/D1");
			PatchRatAttack(RatPatch, 0, Pos);
			LoadPatch("Attack/D2");
			PatchRatAttack(RatPatch, 1, Pos);
			LoadPatch("Attack/D3");
			PatchRatAttack(RatPatch, 2, Pos);
			LoadPatch("Attack/D4");
			PatchRatAttack(RatPatch, 3, Pos);
			LoadPatch("Attack/D5");
			PatchRatAttack(RatPatch, 4, Pos);
			LoadPatch("Attack/D6");
			PatchRatAttack(RatPatch, 5, Pos);
			LoadPatch("Attack/D7");
			PatchRatAttack(RatPatch, 6, Pos);
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
				else if(A.Name == "Spawn")
				{
					C.LeftClick();
					ActiveCard = C;
					return 2;
				}
			}

			// Specific card exceptions

			if(C.CardName == "Coffee")
			{
				C.LeftClick();
				ActiveCard = C;
				return 3;
			}



			// Card contains neither move, nor spawn
			QueuedActions.Add(C);
		}
		return 0;
	}

	public void ExamineAllActions()
	{
		foreach(Card C in QueuedActions)
		{
			ExamineActionCard(C);
		}
	}

	// This examines (but does not click!) an action card in the move phase
	// The examination determines which direction the rat should face when using the card
	public void ExamineActionCard(Card C)
	{
		GD.Print("EXAMINED AN ACTION CARD");

		int x = (int)Board.GetCharPos(C.PlayerID).x;
		int y = (int)Board.GetCharPos(C.PlayerID).y;


		GD.Print(AttackRat[0,x,y]);
		GD.Print(AttackRat[1,x,y]);
		GD.Print(AttackRat[2,x,y]);
		GD.Print(AttackRat[3,x,y]);
		GD.Print(AttackRat[4,x,y]);
		GD.Print(AttackRat[5,x,y]);
		GD.Print(AttackRat[6,x,y]);

		if(AttackRat[1,x,y] % 2 == 0)
		{
			GD.Print("ABOVE");
		}

		QueuedRotations.Add("Left");
	}

	public void ClickTile(Vector2 TilePos)
	{
		Board.Cell[(int)TilePos.x,(int)TilePos.y].LeftClick();
	}


	// Move the closest possible to friendlies
	public bool MoveBest(int[,] InMat)
	{
		bool SuccessfulMove = false;
		Vector2 BestMove = new Vector2(0,0);
		float BestValue = 1000.0f;

		for(int x = 0; x < 8; x++)
		{
			for(int y = 0; y < 8; y++)
			{
				if(Board.ActionMatrix[x,y])
				{
					float RandomVal = RandomVariation/2.0f - ((float)rnd.NextDouble() * RandomVariation);
					
					if(BestValue >= (float)InMat[x,y] + RandomVal)
					{
						BestValue = (float)InMat[x,y] + RandomVal;
						BestMove = new Vector2(x,y);
						SuccessfulMove = true;
					}
				}
			}
		}

		ClickTile(BestMove);
		return SuccessfulMove;
	}

	// MATRIX PATCHING

	// Patches movement priorities around cities and player characters
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

	// Patches an attack matrix around a city/player character, by multiplying the values
	public void PatchRatAttack(int[,] InMat, int Range, Vector2 CPos)
	{
		int CIndex = (InMat.GetLength(0) - 1)/2;
		
		int OffsetX = (int)CPos.x-CIndex;
		int OffsetY = (int)CPos.y-CIndex;

		for(int x = 0; x < InMat.GetLength(0); x++)
		{
			for(int y = 0; y < InMat.GetLength(0); y++)
			{
				bool BoxTestX = x+OffsetX >= 0 && x+OffsetX < AttackRat.GetLength(0);
				bool BoxTestY = y+OffsetY >= 0 && y+OffsetY < AttackRat.GetLength(1);

				if(BoxTestX && BoxTestY)
				{
					if(InMat[x,y] == 0)
					{
						GD.Print("WTF");
					}

					AttackRat[Range, x+OffsetX, y+OffsetY] *= InMat[x,y];
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

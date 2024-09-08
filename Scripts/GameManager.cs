using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class GameManager : Node2D
{
	private Board Board;
	private AI AI;
	
	private CardManager CMSoldier, CMSniper, CMSupport, CMRat;
	private SFXManager SFX;
	private int CardIDCounter = 0;
	private int CharacterIDCounter = 0;
	private Dictionary<string,List<string>> Decks;

	private bool CurrentLoaded = false;
	private bool[,] UnRotated;
	private bool[,] CurrentMatrix;

	public string Rot = "Up";
	public Card CurrentCard;
	public bool PrepMode;

	public string Turn = "Player"; // Whose turn is it? RatMove, Player, RatAttack, None

	public List<int> RatIDList = new List<int>();

	public Dictionary<int, string> RatIDToName = new Dictionary<int,string>(); // Used to convert from a rat id to a rat name

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SFX = (SFXManager)GetNode("SFX");
		Board = (Board)GetNode("Board");
		CMSoldier = (CardManager)GetNode("CardsSoldier");
		CMSniper = (CardManager)GetNode("CardsSniper");
		CMSupport = (CardManager)GetNode("CardsSupport");
		CMRat = (CardManager)GetNode("CardsRat");
		AI = (AI)GetNode("AI");

		// Load starting decks
		File Reader = new File();
		Reader.Open("res://Assets/Decks.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		Decks = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(Contents);
		Reader.Close();

		string[] CMnames = {"Soldier", "Sniper", "Support", "Rat"};

		foreach(string CMName in CMnames)
		{
			foreach(string Cstring in Decks[CMName])
			{
				switch(CMName)
				{
					case "Soldier":
						CMSoldier.AddCard(Cstring);
					break;
					case "Sniper":
						CMSniper.AddCard(Cstring);
					break;
					case "Support":
						CMSupport.AddCard(Cstring);
					break;
					case "Rat":
						CMRat.AddCard(Cstring);
					break;
				}
			}
		}
		
		// Draw a full hand for each player
		for(int x = 0; x < 4; x++)
		{
			CMSoldier.DrawCard();
			CMSniper.DrawCard();
			CMSupport.DrawCard();
			CMRat.DrawCard();
			CMRat.DrawCard();
			CMRat.DrawCard();
		}
	}

	public void PlaySFX(string SFXName)
	{
		SFX.PlaySFX(SFXName);
		GD.Print("ATTEMPTED TO PLAY SFX: " + SFXName);
	}

	//  RatMove->Player->RatAttack->Draw->RatMove etc.
	public void SetMode(string ModeName)
	{
		GD.Print("SET MODE: " + ModeName);
		Turn = ModeName;
		switch(ModeName)
		{
			case "RatMove":
				CMSoldier.UnClick();
				CMSniper.UnClick();
				CMSupport.UnClick();
				CMRat.ReClick();
				AI.StartMoveMode();
			break;
			case "Player":
				CMSoldier.ReClick();
				CMSniper.ReClick();
				CMSupport.ReClick();
				CMRat.UnClick();
			break;
			case "RatAttack":
				CMSoldier.UnClick();
				CMSniper.UnClick();
				CMSupport.UnClick();
				CMRat.ReClick();
			break;
			case "Draw":
				CMSoldier.NewTurn();
				CMSniper.NewTurn();
				CMSupport.NewTurn();
				CMRat.NewTurn();
				CMRat.UnClick();
			break;
		}
	}


	// Rotating matrices using WASD
	public override void _UnhandledInput(InputEvent @event)
	{

		if (@event is InputEventKey eventKey2)
		{
			if(eventKey2.Pressed)
			{
				switch(eventKey2.Scancode)
				{
					// DEBUG SWITCH GAME MODES
					case (int)KeyList.G:
						SetMode("RatMove");
					break;
					case (int)KeyList.H:
						SetMode("Player");
					break;
					case (int)KeyList.J:
						SetMode("RatAttack");
					break;
					case (int)KeyList.K:
						SetMode("Draw");
					break;
					case (int)KeyList.L:
						SetMode("None");
					break;

				}
			}
		}

		if(!CurrentLoaded)
			return;

		if (@event is InputEventKey eventKey)
		{
			if(eventKey.Pressed)
			{
				switch(eventKey.Scancode)
				{
					case (int)KeyList.W:
						Rot = "Up";
						CurrentMatrix = (bool[,])UnRotated.Clone();
						Board.ShowMatrix(CurrentMatrix, CurrentCard);
					break;
					case (int)KeyList.A:
						Rot = "Left";
						CurrentMatrix = (bool[,])UnRotated.Clone();
						RotCounter(CurrentMatrix);
						Board.ShowMatrix(CurrentMatrix, CurrentCard);
					break;
					case (int)KeyList.S:
						Rot = "Down";
						CurrentMatrix = (bool[,])UnRotated.Clone();
						RotCounter(CurrentMatrix);
						RotCounter(CurrentMatrix);
						Board.ShowMatrix(CurrentMatrix, CurrentCard);
					break;
					case (int)KeyList.D:
						Rot = "Right";
						CurrentMatrix = (bool[,])UnRotated.Clone();
						RotClock(CurrentMatrix);
						Board.ShowMatrix(CurrentMatrix, CurrentCard);
					break;

					
				}
			}
		}	
	}

	// Prepares a card for play
	public void PrepPlay(Card Card)
	{
		if(Turn == "Player")
		{
			CMSoldier.UnClick();
			CMSniper.UnClick();
			CMSupport.UnClick();
			
		}
		else
		{
			CMRat.UnClick();
		}
		

		PrepMode = true;
		Rot = "Up";
		ShowPlay(Card);
	}

	// Enables clicking for all cards again, and clears the board
	public void UnPrep()
	{
		if(Turn == "Player")
		{
			CMSoldier.ReClick();
			CMSniper.ReClick();
			CMSupport.ReClick();
			
		}
		else
		{
			CMRat.ReClick();
		}

		Board.ClearMarkers();
		CurrentLoaded = false;
		PrepMode = false;
	}

	// Plays a card and its abilities
	public void ExecutePlay()
	{
		GD.Print(Rot);
		foreach(Ability A in CurrentCard.AbilityList)
		{
			Board.LoadTheoretical();
			switch(A.Name)
			{
				case "Damage":
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.Damage(Target, int.Parse(A.Effect));
					}
				break;
				case "Move":
					if(Board.TargetList.Count > 0)
					{
						Board.Swap(Board.TargetList[0], Board.GetCharPos(CurrentCard.PlayerID));
					}
				break;
				case "Swap":
					if(Board.TargetList.Count > 0)
					{
						Board.Swap(Board.TargetList[0], Board.GetCharPos(CurrentCard.PlayerID));
					}
				break;
				case "Spawn":
					if(Board.TargetList.Count > 0)
					{
						Board.Spawn(A.Effect, Board.TargetList[0]);
					}
				break;
				case "Stun":
					if(Board.TargetList.Count > 0)
					{
						Board.Stun(Board.TargetList[0]);
					}
				break;
				case "Push":
					string PushDirection = "N";
					switch(Rot)
					{
						case "Up":
							PushDirection = "N";
						break;
						case "Left":
							PushDirection = "W";
						break;
						case "Down":
							PushDirection = "S";
						break;
						case "Right":
							PushDirection = "E";
						break;
					}
					foreach(Vector2 Target in Board.TargetList)
					{	
						// GOTTA FIGURE OUT HOW TO RESOLVE MULTIPLE PUSHES AT ONCE
						Board.Push(Target, Target, PushDirection);
						
					}

					Board.MoveQueue();
					PlaySFX("Punch");
					
				break;
			}
		}

		foreach(Ability B in CurrentCard.SecondaryList)
		{
			switch(B.Name)
			{
				case "Shuffle":
					string[] Effect = (B.Effect).Split(":");
					switch(Effect[0])
					{
						case "Soldier":
							CMSoldier.AddCard(Effect[1]);
						break;
						case "Sniper":
							CMSniper.AddCard(Effect[1]);
						break;
						case "Support":
							CMSupport.AddCard(Effect[1]);
						break;
						case "Rat":
							CMRat.AddCard(Effect[1], RatIDList.LastOrDefault());
						break;
					}
					
				break;
			}
		}


		CurrentCard.Discard();
		UnPrep();
	}

	// Visualizes what a card does, but without playing it
	public void ShowPlay(Card Card)
	{
		if(Card.MatrixName == "Global")
		{
			CurrentMatrix = new bool[15,15];
			for(int x = 0; x < 15; x++)
			{
				for(int y = 0; y < 15; y++)
				{
					CurrentMatrix[x,y] = true;
				}
			}
		}
		else
		{
			CurrentMatrix = LoadMatrix(Card.MatrixName, Card.Range);
		}
		
		UnRotated = (bool[,])CurrentMatrix.Clone();
		CurrentLoaded = true;
		CurrentCard = Card;

		Board.ShowMatrix(CurrentMatrix, CurrentCard);
	}

	// Creates a new ID for a card
	public int NewCardID(int InInt)
	{
		CardIDCounter++;
		return CardIDCounter * 1000 + InInt;
	}

	// Creates a new ID for a character
	public int NewCharacterID(int InInt)
	{
		CharacterIDCounter++;
		return CharacterIDCounter * 100 + InInt;
	}



	public bool[,] LoadMatrix(string MatrixName, int range)
	{
		bool[,] OutMat = new bool[range*2 + 1, range*2 + 1];

		File Reader = new File();

		Reader.Open("res://Assets/Matrices/" + range.ToString() + "/" + MatrixName + ".txt", File.ModeFlags.Read);
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
					OutMat[x,y] = false;
					x++;
				break;
				case '1':
					OutMat[x,y] = true;
					x++;
				break;
				case '8':
					// This is just to mark the center character, purely for readability
					OutMat[x,y] = false;
					x++;
				break;
				case '\n':
					// Line break, new row to the matrix
					x = 0;
					y++;
				break;
			}

		}

		return OutMat;
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


	public void UnBig()
	{
		CMSoldier.UnBig();
		CMSniper.UnBig();
		CMSupport.UnBig();
		CMRat.UnBig();
	}

	// Temporarily disables a card due to a dying character
	public void SkipCard(int PID)
	{
		CMSoldier.SkipCard(PID);
		CMSniper.SkipCard(PID);
		CMSupport.SkipCard(PID);
		CMRat.SkipCard(PID);
	}
}

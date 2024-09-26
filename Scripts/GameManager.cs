using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class GameManager : Node2D
{
	public Board Board;
	
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

	public string Turn = "None"; // Whose turn is it? RatMove, Player, RatAttack, None

	public List<int> RatIDList = new List<int>();

	private bool AreYouWinningSon = false;

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

		LevelStart();
	}

	public void PlaySFX(string SFXName)
	{
		SFX.PlaySFX(SFXName);
		GD.Print("ATTEMPTED TO PLAY SFX: " + SFXName);
	}

	//  RatMove->Player->RatAttack->Draw->RatMove etc.
	public void SetMode(string ModeName)
	{
		if(ModeName == "RatAttack" && Turn != "Player")
		{
			return;
		}
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
				AI.StartAttackMode();
			break;
			case "Draw":
				Board.NewTurn();
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
					case (int)KeyList.Z:
						SetMode("RatMove");
					break;
					case (int)KeyList.V:
						SetMode("Draw");
					break;
					case (int)KeyList.N:
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
		PrepMode = true;
		Rot = "Up";
		ShowPlay(Card);
	}

	// Enables clicking for all cards again, and clears the board
	public void UnPrep(bool ForceUnprep = false)
	{
		if(ForceUnprep)
		{
			CMSoldier.UnPrep();
			CMSniper.UnPrep();
			CMSupport.UnPrep();
			CMRat.UnPrep();
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
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.Spawn(A.Effect, Target);
					}
				break;
				case "Draw":
					foreach(Vector2 Target in Board.TargetList)
					{
						GD.Print(Board.Cell[(int)Target.x,(int)Target.y].Char.ID);
						// Draw card for one of the player characters
						switch(Board.Cell[(int)Target.x,(int)Target.y].Char.ID)
						{
							case 101:
								CMSoldier.DrawCard();
							break;
							case 202:
								CMSniper.DrawCard();
							break;
							case 303:
								CMSupport.DrawCard();
							break;
						}
						// Draw card for the rats
						if(Board.Cell[(int)Target.x,(int)Target.y].Char.ID % 100 > 10)
						{
							CMRat.DrawCard();
						}
					}
				break;
				case "Create":
					foreach(Vector2 Target in Board.TargetList)
					{
						// Draw card for one of the player characters
						switch(Board.Cell[(int)Target.x,(int)Target.y].Char.ID)
						{
							case 101:
								CMSoldier.AddCard(A.Effect);
								CMSoldier.DrawCard(A.Effect);
							break;
							case 202:
								CMSniper.AddCard(A.Effect);
								CMSniper.DrawCard(A.Effect);
							break;
							case 303:
								CMSupport.AddCard(A.Effect);
								CMSupport.DrawCard(A.Effect);
							break;
						}
						// Draw card for the rats
						if(Board.Cell[(int)Target.x,(int)Target.y].Char.ID % 100 > 10)
						{
							CMRat.DrawCard();
						}
					}
				break;
				case "Stun":
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.AddModifier(Target, "Stun", int.Parse(A.Effect));
					}
				break;
				case "Push":
					string PushDirection = "N";
					switch(Rot)
					{
						case "Up":
							PushDirection = A.Effect;
							
						break;
						case "Left":
							switch(A.Effect)
							{
								case "N":
									PushDirection = "W";
								break;
								case "W":
									PushDirection = "S";
								break;
								case "S":
									PushDirection = "E";
								break;
								case "E":
									PushDirection = "N";
								break;
							}
							
						break;
						case "Down":
							switch(A.Effect)
							{
								case "N":
									PushDirection = "S";
								break;
								case "W":
									PushDirection = "E";
								break;
								case "S":
									PushDirection = "N";
								break;
								case "E":
									PushDirection = "W";
								break;
							}
						break;
						case "Right":
							switch(A.Effect)
							{
								case "N":
									PushDirection = "E";
								break;
								case "W":
									PushDirection = "N";
								break;
								case "S":
									PushDirection = "W";
								break;
								case "E":
									PushDirection = "S";
								break;
							}
						break;
					}
					foreach(Vector2 Target in Board.TargetList)
					{	
						// Make sure the thing you're trying to push is not immovable
						if(!Board.Cell[(int)Target.x,(int)Target.y].Char.ContainsModifier("Immovable"))
						{
							Board.Push(Target, Target, PushDirection);
						}
						
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
				case "Create":
					string[] CEffect = (B.Effect).Split(":");
					switch(CEffect[0])
					{
						case "Soldier":
							CMSoldier.AddCard(CEffect[1]);
							CMSoldier.DrawCard(CEffect[1]);
						break;
						case "Sniper":
							CMSniper.AddCard(CEffect[1]);
							CMSniper.DrawCard(CEffect[1]);
						break;
						case "Support":
							CMSupport.AddCard(CEffect[1]);
							CMSupport.DrawCard(CEffect[1]);
						break;
						case "Rat":
							CMRat.AddCard(CEffect[1], RatIDList.LastOrDefault());
							CMRat.DrawCard(CEffect[1]);
						break;
					}
				break;
			}
		}


		CurrentCard.Discard();
		UnPrep();

		// Only end the level after finishing the turn
		if(AreYouWinningSon)
		{
			LevelEnd();
		}
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


	// Loads an action matrix centered around a character
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

	// Reset BigMode for all cards
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

	// Everything needed to set up the new round
	public void LevelStart()
	{
		SetMode("Draw");
	}
	
	// Everything done after the round is over
	public void LevelEnd()
	{
		GD.Print("LEVEL ENDED!");
		SetMode("None");
		AreYouWinningSon = false;
	}

	// Checks if there are no more rats, and no more spawn cards left
	// if there is neither, the player wins!
	public void CheckWin()
	{
		if(RatIDList.Count > 0)
		{
			return;
		}
		
		foreach(int CID in CMRat.Deck)
		{
			if(CID % 1000 >= 600)
			{
				return;
			}
		}
		foreach(int CID in CMRat.Deck)
		{
			if(CID % 1000 >= 600)
			{
				return;
			}
		}
		foreach(int CID in CMRat.Deck)
		{
			if(CID % 1000 >= 600)
			{
				return;
			}
		}


		AreYouWinningSon = true;

	}
}

using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class GameManager : Node2D
{
	private Board Board;
	
	private CardManager CMSoldier, CMSniper, CMSupport, CMRat;
	private int CardIDCounter = 0;
	private int CharacterIDCounter = 0;
	private Dictionary<string,List<string>> Decks;

	private bool CurrentLoaded = false;
	private bool[,] UnRotated;
	private bool[,] CurrentMatrix;

	public string Rotation = "Up";
	public Card CurrentCard;
	public bool PrepMode;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Board = (Board)GetNode("Board");
		CMSoldier = (CardManager)GetNode("CardsSoldier");
		CMSniper = (CardManager)GetNode("CardsSniper");
		CMSupport = (CardManager)GetNode("CardsSupport");
		CMRat = (CardManager)GetNode("CardsRat");

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


	// Rotating matrices using WASD
	public override void _UnhandledInput(InputEvent @event)
	{
		if(!CurrentLoaded)
			return;

		if (@event is InputEventKey eventKey)
		{
			if(eventKey.Pressed)
			{
				switch(eventKey.Scancode)
				{
					case (int)KeyList.W:
						Rotation = "Up";
						CurrentMatrix = (bool[,])UnRotated.Clone();
						Board.ShowMatrix(CurrentMatrix, CurrentCard);
					break;
					case (int)KeyList.A:
						Rotation = "Left";
						CurrentMatrix = (bool[,])UnRotated.Clone();
						RotCounter(CurrentMatrix);
						Board.ShowMatrix(CurrentMatrix, CurrentCard);
					break;
					case (int)KeyList.S:
						Rotation = "Down";
						CurrentMatrix = (bool[,])UnRotated.Clone();
						RotCounter(CurrentMatrix);
						RotCounter(CurrentMatrix);
						Board.ShowMatrix(CurrentMatrix, CurrentCard);
					break;
					case (int)KeyList.D:
						Rotation = "Right";
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
		CMSoldier.UnClick();
		CMSniper.UnClick();
		CMSupport.UnClick();
		CMRat.UnClick();

		PrepMode = true;
		Rotation = "Up";
		ShowPlay(Card);
	}

	// Enables clicking for all cards again, and clears the board
	public void UnPrep()
	{
		CMSoldier.ReClick();
		CMSniper.ReClick();
		CMSupport.ReClick();
		CMRat.ReClick();

		Board.ClearMarkers();
		CurrentLoaded = false;
		PrepMode = false;
	}

	// Plays a card and its abilities
	public void ExecutePlay()
	{
		GD.Print(Rotation);
		foreach(Ability A in CurrentCard.AbilityList)
		{
			switch(A.Name)
			{
				case "Damage":
					foreach(Vector2 Target in Board.TargetList)
					{
						GD.Print(Target);
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
				case "Push":
					string PushDirection = "N";
					switch(Rotation)
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

					if(Board.TargetList.Count > 0)
					{
						Board.Push(Board.TargetList[0], PushDirection, 1);
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
		CurrentMatrix = LoadMatrix(Card.MatrixName, Card.Range);
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
		CardManager CMnew = (CardManager)GetNode("CardsSoldier");
		CMnew.UnBig();
		CMnew = (CardManager)GetNode("CardsSniper");
		CMnew.UnBig();
		CMnew = (CardManager)GetNode("CardsSupport");
		CMnew.UnBig();
		CMnew = (CardManager)GetNode("CardsRat");
		CMnew.UnBig();
	}
}

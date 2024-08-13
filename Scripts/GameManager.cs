using Godot;
using System;
using System.Collections.Generic;

public class GameManager : Node2D
{
	private Board Board;
	
	private CardManager CMSoldier, CMSniper, CMSupport, CMRat;
	private int CardIDCounter = 0;
	private int CharacterIDCounter = 0;

	private bool CurrentLoaded = false;
	private bool[,] UnRotated;
	public bool[,] CurrentMatrix;
	public Card CurrentCard;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Board = (Board)GetNode("Board");
		CMSoldier = (CardManager)GetNode("CardsSoldier");
		CMSniper = (CardManager)GetNode("CardsSniper");
		CMSupport = (CardManager)GetNode("CardsSupport");
		CMRat = (CardManager)GetNode("CardsRat");

		CMSoldier.AddCard("Duck!");
		CMSoldier.AddCard("Duck!");
		CMSoldier.AddCard("Duck!");
		CMSoldier.AddCard("Duck!");

		CMSniper.AddCard("Duck!");
		CMSniper.AddCard("Duck!");
		CMSniper.AddCard("Duck!");
		CMSniper.AddCard("Duck!");

		CMSupport.AddCard("Duck!");
		CMSupport.AddCard("Duck!");
		CMSupport.AddCard("Duck!");
		CMSupport.AddCard("Duck!");

		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		
		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		
		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		CMRat.AddCard("Duck!");
		CMRat.AddCard("GOOSE!");
		


		for(int x = 0; x < 4; x++)
		{
			CMSoldier.DrawCard();
			CMSniper.DrawCard();
			CMSupport.DrawCard();
			CMRat.DrawCard();
		}

		for(int x = 0; x < 8; x++)
		{
			CMRat.DrawCard();
		}
	}


	// Rotating matrices using the A and D buttons
	public override void _UnhandledInput(InputEvent @event)
	{
		if(!CurrentLoaded)
			return;

		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.W)
			{
				CurrentMatrix = (bool[,])UnRotated.Clone();
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.A)
			{
				CurrentMatrix = (bool[,])UnRotated.Clone();
				RotCounter(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.S)
			{
				CurrentMatrix = (bool[,])UnRotated.Clone();
				RotCounter(CurrentMatrix);
				RotCounter(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.D)
			{
				CurrentMatrix = (bool[,])UnRotated.Clone();
				RotClock(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.Space)
			{
				// FINISH PLAYING HERE!
			}
		}	
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

	// Plays a card and its abilities
	public void ExecutePlay(Card Card)
	{
		CurrentMatrix = LoadMatrix(Card.MatrixName, Card.Range);
		UnRotated = (bool[,])CurrentMatrix.Clone();
		CurrentLoaded = true;
		CurrentCard = Card;

		Board.ShowMatrix(CurrentMatrix, CurrentCard);

		Card.Discard();
	}

	// Creates a new ID for a card
	public int NewCardID(int InInt)
	{
		CardIDCounter++;
		return CardIDCounter * 100 + InInt;
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

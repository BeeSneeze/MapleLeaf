using Godot;
using System;
using System.Collections.Generic;

public class GameManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Board Board;
	public bool[,] CurrentMatrix;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Board = (Board)GetNode("Board");
		CurrentMatrix = LoadMatrix("Cone", 2);

		Board.ShowMatrix(CurrentMatrix, new Vector2(2,4));
	}


	// Rotating matrices using the A and D buttons
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.A)
			{
				RotCounter(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, new Vector2(2,4));
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.D)
			{
				RotClock(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, new Vector2(2,4));
			}
			else if(eventKey.Pressed && eventKey.Scancode == (int)KeyList.Space)
			{
				// FINISH PLAYING HERE!
			}
		}	
	}

	// Plays a card and its abilities
	public void ExecutePlay(Card Card)
	{
		GD.Print(Card.CardName);
		CurrentMatrix = LoadMatrix(Card.MatrixName, Card.Range);

		Board.ShowMatrix(CurrentMatrix, new Vector2(2,4));

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
}

using Godot;
using System;

public class GameManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
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
}

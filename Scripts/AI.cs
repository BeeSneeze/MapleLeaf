using Godot;
using System;
using System.Collections.Generic;

public class AI : Node2D
{

	private GameManager GM;
	private Board Board;
	private CardManager CM;


	private int[,] RatPatch = new int[8,8];
	public int[,] MoveRat = new int[8,8];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = (GameManager)GetParent();
		CM = (CardManager)GM.GetNode("CardsRat");
		Board = (Board)GM.GetNode("Board");

		LoadPatch("Move");
		EvaluateBoard();

	}

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
			GD.Print(C);

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

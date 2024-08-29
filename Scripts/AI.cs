using Godot;
using System;

public class AI : Node2D
{

	private GameManager GM;
	private Board Board;
	private CardManager CM;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = (GameManager)GetParent();
		CM = (CardManager)GM.GetNode("CardsRat");
		Board = (Board)GM.GetNode("Board");

	}
}

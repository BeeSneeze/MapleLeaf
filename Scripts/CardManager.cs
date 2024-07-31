using Godot;
using System;
using System.Collections.Generic;

public class CardManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public List<Card> Cards = new List<Card>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var scene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

		for(int x = 0; x < 4; x++)
		{
			Card NewCard = (Card)scene.Instance();
			Cards.Add(NewCard);
			NewCard.Translate(new Vector2(x*100,0));

			NewCard.BigMode(x==0);
			
			AddChild(NewCard);
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

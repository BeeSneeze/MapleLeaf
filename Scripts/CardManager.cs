using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CardManager : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public List<Card> Cards = new List<Card>();

	private bool Big = false;

	Dictionary<string,CardType> AllCardsDict; // Dict containing info about all the cards in a Cardtype struct

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		File Reader = new File();
		Reader.Open("res://Assets/Cards.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		AllCardsDict = JsonConvert.DeserializeObject<Dictionary<string,CardType>>(Contents);
		Reader.Close();

		var scene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

		for(int x = 0; x < 4; x++)
		{
			Card NewCard = (Card)scene.Instance();

			NewCard.CardName = "The Swarm";

			if(x==0)
				NewCard.CardName = "The Swarm";
			if(x==1)
				NewCard.CardName = "GOOSE!";
			if(x==2)
				NewCard.CardName = "Binoculars";
			if(x==3)
				NewCard.CardName = "Duck!";

			CardType CardInfo =  AllCardsDict[NewCard.CardName];

			NewCard.LoadInfo(CardInfo);

			// INITIATE STUFF HERE

			Cards.Add(NewCard);
			NewCard.Translate(new Vector2(x*100,0));
			AddChild(NewCard);
			NewCard.BigMode(false);
		}
	}

	public void BigMode(bool InBool)
	{
		GD.Print("MANAGER BIGMODE");
		Big = InBool;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

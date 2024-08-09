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

	[Export] public string OwnerName;

	private bool Big = false;

	private GameManager GM;

	Dictionary<string,CardType> AllCardsDict; // Dict containing info about all the cards in a Cardtype struct

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = (GameManager)GetParent();
		Sprite SprOwner = (Sprite)GetNode("Owner");
		Sprite BG = (Sprite)GetNode("CardManagerBG");

		switch(OwnerName)
		{
			case "Soldier":
				BG.Modulate = new Color(0.0f, 0.8f, 0.0f, 0.6f);
			break;
			case "Sniper":
				BG.Modulate = new Color(1.0f, 0.6f, 0.3f, 0.7f);
			break;
			case "Support":
				BG.Texture = (Texture)GD.Load("res://Assets/Visuals/CardManagerBGSupport.png");
				BG.Modulate = new Color(0.1f, 0.1f, 0.9f, 0.5f);
			break;
			case "RatNormal":
				BG.Texture = (Texture)GD.Load("res://Assets/Visuals/CardManagerBGRat.png");
				Node2D SOwner = (Node2D)SprOwner;
				SOwner.Translate(new Vector2(390,-280));
				SOwner.Scale = new Vector2(0.7f,0.7f);
				BG.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			break;
		}

		SprOwner.Texture = (Texture)GD.Load("res://Assets/Visuals/Characters/" + OwnerName + ".png");
		
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
			NewCard.Translate(new Vector2(x*100+10,-12));
			AddChild(NewCard);
			NewCard.BigMode(false);
		}
	}

	public void BigMode(bool InBool)
	{
		GD.Print("MANAGER BIGMODE");
		Big = InBool;
		
	}

	public void UnBig()
	{
		foreach(Card C in Cards)
		{
			C.BigMode(false);
		}
	}

}

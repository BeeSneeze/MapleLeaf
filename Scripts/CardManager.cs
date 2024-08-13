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

	private GameManager GM;

	// Cards as card IDs
	public List<int> Deck = new List<int>();
	public List<int> Hand = new List<int>();
	public List<int> Discard = new List<int>();

	Dictionary<string,CardType> AllCardsDict; // Dict containing info about all the cards in a Cardtype struct

	Dictionary<int, string> IdToNameConvert = new Dictionary<int, string>(); 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = (GameManager)GetParent();
		
		// Manager Visuals
		Sprite SprOwner = (Sprite)GetNode("Owner");
		Sprite BG = (Sprite)GetNode("CardManagerBG");

		SprOwner.Texture = (Texture)GD.Load("res://Assets/Visuals/Characters/" + OwnerName + ".png");

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
			case "Rat":
				BG.Texture = (Texture)GD.Load("res://Assets/Visuals/CardManagerBGRat.png");
				Node2D SOwner = (Node2D)SprOwner;
				SOwner.Translate(new Vector2(390,-205));
				SOwner.Scale = new Vector2(0.7f,0.7f);
				BG.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.6f);
			break;
		}

		// Load AllCardsDict
		File Reader = new File();
		Reader.Open("res://Assets/Cards.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		AllCardsDict = JsonConvert.DeserializeObject<Dictionary<string,CardType>>(Contents);
		Reader.Close();

		foreach(KeyValuePair<string, CardType> Entry in AllCardsDict)
		{
			IdToNameConvert.Add(int.Parse(Entry.Value.CardNum), Entry.Key);
		}

		
	}

	private void CreateCardObject(int InInt)
	{
		var scene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
			
		Card NewCard = (Card)scene.Instance();

		switch(OwnerName)
		{
			case "Soldier":
				NewCard.OwnerID = 101;
				NewCard.PlayerID = 101;
			break;
			case "Sniper":
				NewCard.OwnerID = 202;
				NewCard.PlayerID = 202;
			break;
			case "Support":
				NewCard.OwnerID = 303;
				NewCard.PlayerID = 303;
			break;
			case "Rat":
				NewCard.OwnerID = 101; // TEMP ID
				NewCard.PlayerID = 101; // TEMP ID
				// Gotta think about how this one works
			break;
		}

		NewCard.CardName = IdToNameConvert[InInt%100];

		NewCard.LoadInfo(AllCardsDict[NewCard.CardName]);

		Cards.Add(NewCard);
		if(OwnerName == "Rat")
		{
			int Column = ((Hand.Count-1)%4);
			int Row = ((Hand.Count-1) - ((Hand.Count-1)%4)) / 4;
			// Rat uses several rows for their cards
			NewCard.Translate(new Vector2(Column*100+5,-220 + Row*200+7));
		}
		else
		{
			// Everyone else, sandwich on one row
			NewCard.Translate(new Vector2((Hand.Count-1)*100+10,-12));
		}
		
		AddChild(NewCard);
		NewCard.BigMode(false);
	}

	// Adds a new card to the deck, by name
	public void AddCard(string CardName)
	{
		Deck.Add(GM.NewCardID(int.Parse(AllCardsDict[CardName].CardNum)));
	}


	// Draws the top card from the deck, and puts it in hand
	public void DrawCard()
	{
		int TopCard = Deck[0];
		Deck.Remove(TopCard);
		Hand.Add(TopCard);

		CreateCardObject(TopCard);
	}

	// Discards a specific card
	public void DiscardCard(Card InCard)
	{
		Hand.Remove(InCard.CardID);
		Discard.Add(InCard.CardID);
		Cards.Remove(InCard);

		InCard.QueueFree();
	}

	// BIG MODE BIG MODE BIG MODE BIG MODE BIG MODE
	public void BigMode(Card Card)
	{
		GD.Print("MANAGER BIGMODE");
	}

	// Turns all cards in hand to the small visual
	public void UnBig()
	{
		foreach(Card C in Cards)
		{
			C.BigMode(false);
		}
	}

}

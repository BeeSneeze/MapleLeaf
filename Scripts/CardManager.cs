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
			case "RatNormal":
				BG.Texture = (Texture)GD.Load("res://Assets/Visuals/CardManagerBGRat.png");
				Node2D SOwner = (Node2D)SprOwner;
				SOwner.Translate(new Vector2(390,-280));
				SOwner.Scale = new Vector2(0.7f,0.7f);
				BG.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
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

		Deck.Add(GM.NewCardID(2));
		Deck.Add(GM.NewCardID(1));
		Deck.Add(GM.NewCardID(3));
		Deck.Add(GM.NewCardID(4));

		for(int x = 0; x < 4; x++)
		{
			DrawCard();
		}
	}

	private void CreateCardObject(int InInt)
	{
		var scene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
			
		Card NewCard = (Card)scene.Instance();

		NewCard.CardName = IdToNameConvert[InInt%100];

		CardType CardInfo =  AllCardsDict[NewCard.CardName];

		NewCard.LoadInfo(CardInfo);

		Cards.Add(NewCard);
		NewCard.Translate(new Vector2((Hand.Count-1)*100+10,-12));
		AddChild(NewCard);
		NewCard.BigMode(false);
	}

	public void DrawCard()
	{
		int TopCard = Deck[0];
		Deck.Remove(TopCard);
		Hand.Add(TopCard);
		GD.Print(TopCard);
		CreateCardObject(TopCard);

	}




	public void BigMode(bool InBool)
	{
		GD.Print("MANAGER BIGMODE");
		Big = InBool;
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

using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CardManager : Node2D
{
	[Export] public string OwnerName;

	// Cards as card IDs
	public List<Card> HandCards = new List<Card>();

	public List<int> Deck = new List<int>();
	public List<int> Hand = new List<int>();
	public List<int> Discard = new List<int>();
	

	private GameManager GM;
	private Label DrawLabel;
	private Label DiscardLabel;

	private Dictionary<string,CardType> AllCardsDict; // Dict containing info about all the cards in a Cardtype struct. Keys by card name
	private Dictionary<int, string> IdToNameConvert = new Dictionary<int, string>(); // Used to convert from ID to name

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = (GameManager)GetParent();
		
		// Manager Visuals
		Sprite SprOwner = (Sprite)GetNode("Owner");
		Sprite BG = (Sprite)GetNode("CardManagerBG");
		Sprite Discard = (Sprite)GetNode("Discard");
		Sprite Draw = (Sprite)GetNode("Draw");
		DiscardLabel = (Label)GetNode("Discard").GetNode("Label");
		DrawLabel = (Label)GetNode("Draw").GetNode("Label");

		SprOwner.Texture = (Texture)GD.Load("res://Assets/Visuals/Characters/" + OwnerName + ".png");

		switch(OwnerName)
		{
			case "Soldier":
				BG.Modulate = new Color(0.0f, 0.8f, 0.0f, 0.6f);
				Discard.Modulate = new Color(0.0f, 0.8f, 0.0f, 1.0f);
				Draw.Modulate = new Color(0.0f, 0.8f, 0.0f, 1.0f);
			break;
			case "Sniper":
				BG.Modulate = new Color(1.0f, 0.6f, 0.3f, 0.7f);
				Discard.Modulate = new Color(1.0f, 0.6f, 0.3f, 1.0f);
				Draw.Modulate = new Color(1.0f, 0.6f, 0.3f, 1.0f);
			break;
			case "Support":
				BG.Texture = (Texture)GD.Load("res://Assets/Visuals/CardManagerBGSupport.png");
				BG.Modulate = new Color(0.1f, 0.1f, 0.9f, 0.5f);
				Discard.Modulate = new Color(0.4f, 0.4f, 1.0f, 1.0f);
				Draw.Modulate = new Color(0.4f, 0.4f, 1.0f, 1.0f);
			break;
			case "Rat":
				BG.Texture = (Texture)GD.Load("res://Assets/Visuals/CardManagerBGRat.png");
				Node2D SOwner = (Node2D)SprOwner;
				SOwner.Translate(new Vector2(450,-250));
				SOwner.Scale = new Vector2(0.7f,0.7f);
				BG.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.6f);

				Discard.Modulate = new Color(0.8f, 0.8f, 0.8f, 1.0f);
				Draw.Modulate = new Color(0.8f, 0.8f, 0.8f, 1.0f);
				Discard.Translate(new Vector2(-350,-245));
				Draw.Translate(new Vector2(-350,-245));
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

	private void UpdateLabels()
	{
		if(DrawLabel != null)
		{
			DrawLabel.Text = (Deck.Count).ToString();
			DiscardLabel.Text = (Discard.Count).ToString();
		}
		
	}


	// Creates a new card from a CardID
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
				// Player ID assigned further down
			break;
		}

		NewCard.CardID = InInt;
		NewCard.CardName = IdToNameConvert[InInt % 1000];

		NewCard.LoadInfo(AllCardsDict[NewCard.CardName]);

		HandCards.Add(NewCard);
		if(OwnerName == "Rat")
		{
			// RAT ID ASSIGNMENT

			Random rnd = new Random();
			NewCard.PlayerID = GM.RatIDList[rnd.Next(0,GM.RatIDList.Count)];


			// CARD VISUALS
			int Column = ((Hand.Count-1)%4);
			int Row = ((Hand.Count-1) - ((Hand.Count-1)%4)) / 4;
			// Rat uses several rows for their cards
			NewCard.Translate(new Vector2(Column*120-25,-220 + Row*240-30));
		}
		else
		{
			// Everyone else, sandwich on one row
			NewCard.Translate(new Vector2((Hand.Count-1)*120-25,-12));
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
		if(Deck.Count == 0)
		{
			GD.Print("OUT OF CARDS!");
			return;
		}

		int TopCard = Deck[0];
		Deck.Remove(TopCard);
		Hand.Add(TopCard);

		CreateCardObject(TopCard);
		UpdateLabels();
	}

	// Discards a specific card
	public void DiscardCard(Card InCard)
	{
		Hand.Remove(InCard.CardID);
		Discard.Add(InCard.CardID);
		HandCards.Remove(InCard);

		InCard.QueueFree();
		UpdateLabels();
	}

	// BIG MODE BIG MODE BIG MODE BIG MODE BIG MODE
	public void BigMode(Card Card)
	{
		GD.Print("MANAGER BIGMODE");
	}

	// Turns all cards in hand to the small visual
	public void UnBig()
	{
		foreach(Card C in HandCards)
		{
			C.BigMode(false);
		}
	}


	// Makes all non-prepped cards unclickable
	public void UnClick()
	{
		foreach(Card C in HandCards)
		{
			if(!C.Prepped)
			{
				C.Clickable = false;
				((Control)(C.GetNode("CardClick"))).MouseFilter = (Godot.Control.MouseFilterEnum)2;
			}
			
		}
	}

	// Makes all cards clickable again
	public void ReClick()
	{
		foreach(Card C in HandCards)
		{
			C.Clickable = true;
			((Control)(C.GetNode("CardClick"))).MouseFilter = (Godot.Control.MouseFilterEnum)1;
		}
	}

	// Turn a card into "SKIP" making it unplayable for this turn
	public void SkipCard(int PID)
	{
		foreach(Card C in HandCards)
		{
			if(C.PlayerID == PID)
			{
				C.Skip();
			}
		}
	}

	// Discard all remaining cards, and draw a new full hand
	public void NewTurn()
	{	
		// Discard all cards
		while(HandCards.Count > 0)
		{
			DiscardCard(HandCards[0]);
		}

		// Draw new cards
		for(int x = 0; x < 4; x++)
		{
			DrawCard();
			if(OwnerName == "Rat")
			{
				DrawCard();
				DrawCard();
				DrawCard();
			}
		}
		
	}

}

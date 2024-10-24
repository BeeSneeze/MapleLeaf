using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CardShop : Node2D
{
	private string OwnerName = "Shop";

	public CardShop OtherShop;

	public bool Shuffle = true;

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
	private Dictionary<int, CompactCard> ActiveCards = new Dictionary<int, CompactCard>(); // Contains supplementary info about all the cards managed by this class

	private Dictionary<string,List<string>> Decks;

	private Random rnd;
	private SceneTreeTween CTween;
	private CheckBox CBox;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		CBox = GetNode<CheckBox>("CheckBox");

		if(Name == "ShopB")
		{
			Control CBoxNode = (Control)CBox;
			CBoxNode.RectPosition = new Vector2(-470.0f,-52.0f);
		}


		rnd = new Random();

		GM = (GameManager)((GetParent().GetParent()).GetNode("Game"));
		
		// Manager Visuals
		Sprite SprOwner = GetNode<Sprite>("Owner");
		Sprite BG = GetNode<Sprite>("CardManagerBG");
		Sprite Discard = GetNode<Sprite>("Discard");
		Sprite Draw = GetNode<Sprite>("Draw");

		//BG.Modulate = new Color(0.1f, 0.1f, 0.9f, 0.5f);

		// Load AllCardsDict
		File Reader = new File();
		Reader.Open("res://Assets/Cards.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		AllCardsDict = JsonConvert.DeserializeObject<Dictionary<string,CardType>>(Contents);
		Reader.Close();

		// Load starting decks
		Reader = new File();
		Reader.Open("res://Assets/Decks.JSON", File.ModeFlags.Read);
		Contents = Reader.GetAsText();
		Decks = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(Contents);
		Reader.Close();

		foreach(KeyValuePair<string, CardType> Entry in AllCardsDict)
		{
			IdToNameConvert.Add(int.Parse(Entry.Value.ID), Entry.Key);
		}

		LoadShop();

		foreach(Card C in HandCards)
		{
			C.Clickable = false;
		}
	}

	// Loads new cards from the card pool
	public void LoadShop()
	{
		AddCard(Decks["PoolSoldier"][rnd.Next(0,Decks["PoolSoldier"].Count)]);
		AddCard(Decks["PoolSniper"][rnd.Next(0,Decks["PoolSniper"].Count)]);
		AddCard(Decks["PoolSupport"][rnd.Next(0,Decks["PoolSupport"].Count)]);
		AddCard(Decks["PoolRatActionA"][rnd.Next(0,Decks["PoolRatActionA"].Count)]);
		AddCard(Decks["PoolRatActionB"][rnd.Next(0,Decks["PoolRatActionB"].Count)]);
		WorldMap WM = GetParent<WorldMap>();
		AddCard(Decks["PoolRatSpawn"][WM.CurrentIndex-1]); // Move through the rats in order
		DrawCard();
		DrawCard();
		DrawCard();
		DrawCard();
		DrawCard();
		DrawCard();
	}

	public void LoadCardEffect(string EffectName, Card InCard)
	{
		var scene = GD.Load<PackedScene>("res://Scenes/CardEffect.tscn");
		Node2D NewCardEffect = (Node2D)scene.Instance();
		NewCardEffect.Position = ((Node2D)InCard).Position;
		AddChild(NewCardEffect);

		AnimatedSprite AnimSpr = (AnimatedSprite)NewCardEffect;

		switch(EffectName)
		{
			case "Exhaust":
				AnimSpr.Animation = "Poof";
			break;
			case "Unplayable":
				AnimSpr.Animation = "Shrug";
			break;

		}
	}


	// Creates a new card from a CardID
	private void CreateCardObject(int InID)
	{
		var scene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
			
		Card NewCard = (Card)scene.Instance();

		NewCard.OwnerName = OwnerName;

		NewCard.OwnerID = ActiveCards[InID].OwnerID;
		
		string ShopCardOwner = "";

		if(100 < InID % 1000 && InID % 1000 <= 200)
		{
			ShopCardOwner = "Soldier";
		}
		else if(200 < InID % 1000 && InID % 1000 <= 300)
		{
			ShopCardOwner = "Sniper";
		}
		else if(300 < InID % 1000 && InID % 1000 <= 400)
		{
			ShopCardOwner = "Support";
		}
		else if(500 < InID % 1000 && InID % 1000 <= 700)
		{
			ShopCardOwner = "Rat";
		}



		switch(ShopCardOwner)
		{
			case "Soldier":
				NewCard.CornerSymbol = "Soldier";
				NewCard.PlayerID = 1;
			break;
			case "Sniper":
				NewCard.CornerSymbol = "Sniper";
				NewCard.PlayerID = 2;
			break;
			case "Support":
				NewCard.CornerSymbol = "Support";
				NewCard.PlayerID = 3;
			break;
			case "Rat":
				NewCard.CornerSymbol = "RatNormal";
				NewCard.PlayerID = 4;
			break;
		}

		NewCard.CardID = InID;
		NewCard.CardName = IdToNameConvert[InID % 1000];
		NewCard.Draws = int.Parse(AllCardsDict[NewCard.CardName].Draws) - ActiveCards[InID].DrawCount; 

		NewCard.LoadInfo(AllCardsDict[NewCard.CardName]);
		NewCard.GM = GM;
		HandCards.Add(NewCard);

		NewCard.Translate(new Vector2(365,-202));
		UpdateCardPositions();
		AddChild(NewCard);
	}

	public string GetOwnerName()
	{
		return OwnerName;
	}

	private static float Space = 450;
	private static float Margin = 265;

	// Moves the cards around to suitable positions
	public void UpdateCardPositions()
	{
		int index = 0;
		float BetweenPadding = 0.0f;
		
		foreach(Card C in HandCards)
		{
			if(index > 2)
			{
				BetweenPadding = 80.0f;
			}

			Node2D CNode = (Node2D)C;
			CTween = GetTree().CreateTween();
			CTween.TweenProperty(CNode, "position", new Vector2(BetweenPadding + (index)* Space /((float)Hand.Count-1) - Margin, 12), 0.20f);
			index++;
		}
	}

	// Adds a new card to the deck, by name
	public void AddCard(string CardName, int RatID = 0)
	{
		int NewID = GM.NewCardID(int.Parse(AllCardsDict[CardName].ID));

		// Compact card notation
		CompactCard CC = new CompactCard();
		CC.DrawCount = 0;
		CC.ID = NewID;
		switch(OwnerName)
		{
			case "Soldier":
				CC.OwnerID = 101;
			break;
			case "Sniper":
				CC.OwnerID = 202;
			break;
			case "Support":
				CC.OwnerID = 303;
			break;
			case "Rat":
				if(RatID != 0)
				{
					CC.OwnerID = RatID;
				}
				else
				{
					CC.OwnerID = 20; // TEMP ID
				}
				
			break;
		}
		
		ActiveCards[NewID] = CC;
		Deck.Add(NewID);
	}

	private List<int> ShufflePile(List<int> PileToShuffle)
	{
		List<int> OutPile = new List<int>();
		List<int> ShufflePile = new List<int>(PileToShuffle);

		int N = ShufflePile.Count;
		int i = 0;

		while(i < N)
		{
			Random rnd = new Random();
			int RandomIndex = rnd.Next(0,N-i);
			
			OutPile.Add(ShufflePile[RandomIndex]);
			ShufflePile.RemoveAt(RandomIndex);
			
			i++;
		}
		return OutPile;
	}

	// Draws a specific card to the hand. Defaults to the top card.
	public void DrawCard(string CardName = "")
	{
		// Reshuffle the deck if out of cards
		if(Deck.Count == 0)
		{
			if(Discard.Count == 0)
			{
				return;
			}

			if(Shuffle)
			{
				Deck = new List<int>(ShufflePile(Discard));
				Discard = new List<int>();
			}
			else
			{
				Deck = new List<int>(Discard);
				Discard = new List<int>();
			}

			
		}

		int TopCard = Deck[0];

		if(CardName != "")
		{
			int index = 0;
			foreach(int CID in Deck)
			{
				if(CID % 1000 == int.Parse(AllCardsDict[CardName].ID))
				{
					TopCard = Deck[index];
					break;
				}
				index++;
			}
		}
		
		Deck.Remove(TopCard);
		Hand.Add(TopCard);

		CreateCardObject(TopCard);
		UpdateCardPositions();
	}

	// Discards a specific card
	public void DiscardCard(Card InCard)
	{
		CompactCard CC = ActiveCards[InCard.CardID];
		CC.DrawCount += 1;
		ActiveCards[InCard.CardID] = CC;

		Hand.Remove(InCard.CardID);
		Discard.Add(InCard.CardID);
		HandCards.Remove(InCard);

		InCard.QueueFree();
		UpdateCardPositions();
	}

	// Completely remove a card from play
	public void ExhaustCard(Card InCard)
	{
		Hand.Remove(InCard.CardID);
		HandCards.Remove(InCard);

		LoadCardEffect("Exhaust", InCard);

		InCard.QueueFree();
		UpdateCardPositions();
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
				C.ToggleClickable(false);
			}
			
		}
	}

	// Makes all cards clickable again
	public void ReClick()
	{
		foreach(Card C in HandCards)
		{
			C.ToggleClickable(true);
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

	// Puts cards into their respective true decks
	public void Ratify()
	{
		foreach(Card C in HandCards)
		{
			switch(C.PlayerID)
			{
				case 1:
					GM.CMSoldier.AddCard(C.CardName);
				break;
				case 2:
					GM.CMSniper.AddCard(C.CardName);
				break;
				case 3:
					GM.CMSupport.AddCard(C.CardName);
				break;
				case 4:
					GM.CMRat.AddCard(C.CardName);
				break;
			}
		}
	}
	
	// Removes all the cards from the shop
	public void ResetShop()
	{
		int failsafe = 10;

		while(HandCards.Count > 0)
		{
			if(failsafe-- <= 0)
				break;

			ExhaustCard(HandCards[0]);
		}

		LoadShop();
	}


	public void CheckPressed()
	{
		OtherShop.ToggleCheck(!CBox.Pressed);
	}

	public void ToggleCheck(bool InBool)
	{
		CheckBox CBox = GetNode<CheckBox>("CheckBox");
		CBox.Pressed = InBool;
	}

}




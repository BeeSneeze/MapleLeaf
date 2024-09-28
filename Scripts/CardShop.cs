using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CardShop : Node2D
{
	private string OwnerName = "Shop";

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

	private SceneTreeTween CTween;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = (GameManager)((GetParent().GetParent()).GetNode("Game"));
		
		// Manager Visuals
		Sprite SprOwner = (Sprite)GetNode("Owner");
		Sprite BG = (Sprite)GetNode("CardManagerBG");
		Sprite Discard = (Sprite)GetNode("Discard");
		Sprite Draw = (Sprite)GetNode("Draw");
		DiscardLabel = (Label)GetNode("Discard").GetNode("Label");
		DrawLabel = (Label)GetNode("Draw").GetNode("Label");

		BG.Texture = (Texture)GD.Load("res://Assets/Visuals/CardManagerBGSupport.png");
		BG.Modulate = new Color(0.1f, 0.1f, 0.9f, 0.5f);
		Discard.Modulate = new Color(0.4f, 0.4f, 1.0f, 1.0f);
		Draw.Modulate = new Color(0.4f, 0.4f, 1.0f, 1.0f);

		// Load AllCardsDict
		File Reader = new File();
		Reader.Open("res://Assets/Cards.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		AllCardsDict = JsonConvert.DeserializeObject<Dictionary<string,CardType>>(Contents);
		Reader.Close();

		foreach(KeyValuePair<string, CardType> Entry in AllCardsDict)
		{
			IdToNameConvert.Add(int.Parse(Entry.Value.ID), Entry.Key);
		}

		AddCard("Duck!");
		AddCard("Duck!");
		AddCard("Duck!");
		AddCard("Duck!");
		DrawCard();
		DrawCard();
		DrawCard();
		DrawCard();

		foreach(Card C in HandCards)
		{
			C.Clickable = false;
		}
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

		NewCard.OwnerID = ActiveCards[InID].OwnerID;
		

		switch(OwnerName)
		{
			case "Soldier":
				NewCard.PlayerID = 101;
			break;
			case "Sniper":
				NewCard.PlayerID = 202;
			break;
			case "Support":
				NewCard.PlayerID = 303;
			break;
			case "Rat":
				// RAT ID ASSIGNMENT
				Random rnd = new Random();
				NewCard.PlayerID = GM.RatIDList[rnd.Next(0,GM.RatIDList.Count)];
				if(GM.RatIDList.Contains(NewCard.OwnerID))
				{
					NewCard.RatName = GM.RatIDToName[NewCard.OwnerID];
				}
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

	private static float Space = 360;
	private static float Margin = 25;

	// Moves the cards around to suitable positions
	public void UpdateCardPositions()
	{
		int index = 0;
		
		foreach(Card C in HandCards)
		{
			Node2D CNode = (Node2D)C;
			CTween = GetTree().CreateTween();
			if(Hand.Count > 1)
			{
				CTween.TweenProperty(CNode, "position", new Vector2((index)* Space /((float)Hand.Count-1) - Margin,-12), 0.20f);
			}
			else
			{
				CTween.TweenProperty(CNode, "position", new Vector2(Space / 2.0f - Margin,-12), 0.20f);
			}
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
					CC.OwnerID = 520; // TEMP ID
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
		GD.Print("ATTEMPTED TO EXHAUST");

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

	// UnPreps all cards
	public void UnPrep()
	{
		foreach(Card C in HandCards)
		{
			C.Prep(false);
		}
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
				//((Control)(C.GetNode("CardClick"))).MouseFilter = (Godot.Control.MouseFilterEnum)2;
			}
			
		}
	}

	// Makes all cards clickable again
	public void ReClick()
	{
		foreach(Card C in HandCards)
		{
			C.ToggleClickable(true);
			//((Control)(C.GetNode("CardClick"))).MouseFilter = (Godot.Control.MouseFilterEnum)1;
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

}

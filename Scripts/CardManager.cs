using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CardManager : Node2D
{
	[Export] public string OwnerName;
	public bool Shuffle = false;

	// Cards as card IDs
	public List<Card> HandCards = new List<Card>();
	public List<int> Deck = new List<int>();
	public List<int> TrueDeck = new List<int>(); // What should the deck reset to inbetween rounds?
	public List<int> Hand = new List<int>();
	public List<int> Discard = new List<int>();

	public bool SettingUp = true; // When true, cards added to the deck are also added to the true deck
	
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
		GM = (GameManager)GetParent();
		
		// Manager Visuals
		Sprite SprOwner = GetNode<Sprite>("Owner");
		Sprite BG = GetNode<Sprite>("CardManagerBG");
		Sprite Discard = GetNode<Sprite>("Discard");
		Sprite Draw = GetNode<Sprite>("Draw");
		DiscardLabel = GetNode("Discard").GetNode<Label>("Label");
		DrawLabel = GetNode("Draw").GetNode<Label>("Label");

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
				SOwner.Scale = new Vector2(0.9f,0.9f);
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
			IdToNameConvert.Add(int.Parse(Entry.Value.ID), Entry.Key);
		}
	}

	// Update the labels at the top of the Card Manager
	private void UpdateLabels()
	{
		if(DrawLabel != null)
		{
			DrawLabel.Text = (Deck.Count).ToString();
			DiscardLabel.Text = (Discard.Count).ToString();
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

	public string GetOwnerName()
	{
		return OwnerName;
	}


	// Creates a new card from a CardID
	private void CreateCardObject(int InID)
	{
		var scene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
			
		Card NewCard = (Card)scene.Instance();

		NewCard.OwnerName = OwnerName;

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
				NewCard.PlayerID = 999;

				if(GM.RatIDList.Count != 0)
				{
					NewCard.PlayerID = GM.RatIDList[rnd.Next(0,GM.RatIDList.Count)];
				}
				
				if(GM.RatIDList.Contains(NewCard.OwnerID))
				{
					NewCard.RatName = GM.RatIDToName[NewCard.OwnerID];
				}
				//NewCard.PlayerID = 999;
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
			if(OwnerName == "Rat")
			{
				int Column = ((index)%4);
				int Row = ((index) - ((index)%4)) / 4;
				// Rat uses several rows for their cards
				CTween = GetTree().CreateTween();
				CTween.TweenProperty(CNode, "position", new Vector2(Column*120-25,-220 + Row*240-30), 0.20f);
			}
			else
			{
				CTween = GetTree().CreateTween();
				if(Hand.Count > 1)
				{
					CTween.TweenProperty(CNode, "position", new Vector2((index)* Space /((float)Hand.Count-1) - Margin,-12), 0.20f);
				}
				else
				{
					CTween.TweenProperty(CNode, "position", new Vector2(Space / 2.0f - Margin,-12), 0.20f);
				}
				
			}
			CNode.ZIndex = index + 100;
			C.Layer = index;
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

		if(SettingUp)
		{
			TrueDeck.Add(NewID);
		}
		else
		{
			Deck = ShufflePile(Deck); // Give it a good shuffle after adding the cards
		}

		

		UpdateLabels();
	}

	public List<int> ShufflePile(List<int> PileToShuffle)
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
		UpdateLabels();
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
		UpdateLabels();
		UpdateCardPositions();
	}

	// Completely remove a card from play
	public void ExhaustCard(Card InCard)
	{
		Hand.Remove(InCard.CardID);
		HandCards.Remove(InCard);

		LoadCardEffect("Exhaust", InCard);

		InCard.QueueFree();
		UpdateLabels();
		UpdateCardPositions();
	}

	// BIG MODE BIG MODE BIG MODE BIG MODE BIG MODE
	public void BigMode(Card Card)
	{
		GD.Print("MANAGER BIGMODE");
	}

	public void PreventPlayerClicks(bool Prevent)
	{
		foreach(Card C in HandCards)
		{
			C.PreventPlayerClicks = Prevent;
		}
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
		Modulate = new Color(0.75f, 0.75f, 0.75f, 1.0f);
		
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
		Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);

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

	public void ToggleQueueMode(bool InBool)
	{
		foreach(Card C in HandCards)
		{
			C.CanQueue = InBool;
		}
	}

	public void ReRoll()
	{
		int index = HandCards.Count-1;
		while(index>=0)
		{
			if(HandCards[index].ReRolled)
			{
				DiscardCard(HandCards[index]);
				DrawCard();
				index = HandCards.Count-1;
			}
			else
			{
				index--;
			}
		}
	}

	// Discard all remaining cards, and draw a new full hand
	public void NewTurn()
	{	
		Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);

		// Discard all cards
		int failsafe = 50; // How many times to run this loop before we stop

		while(HandCards.Count > 0)
		{
			if(failsafe-- <= 0)
				break;
			bool ForceExhaust = false;

			foreach(Ability A in HandCards[0].SecondaryList)
			{
				if(A.Name == "Exhaust" && A.Effect == "Forced")
				{
					ForceExhaust = true;
				}
			}

			if(ForceExhaust)
			{
				ExhaustCard(HandCards[0]);
			}
			else
			{
				DiscardCard(HandCards[0]);
			}
		}

		// Draw new cards
		for(int x = 0; x < 4; x++)
		{
			DrawCard();
			if(OwnerName == "Rat")
			{
				DrawCard();
				DrawCard();
			}
		}
		
	}

	// Resets the deck back to its original state, and clears the hand + the discard
	// Used inbetween levels, so it also turns on shuffle (as the tutorial is *not* shuffled)
	public void ResetDeck()
	{
		Shuffle = true;
		SettingUp = true;
		Deck = new List<int>(TrueDeck);

		if(Shuffle)
		{
			Deck = ShufflePile(Deck);
		}

		Hand = new List<int>();
		Discard = new List<int>();

		int failsafe = 50; // How many times to run this loop before we stop

		while(HandCards.Count > 0)
		{
			if(failsafe-- <= 0)
				break;

			ExhaustCard(HandCards[0]);
		}

		foreach(int DeckCardID in Deck)
		{
			CompactCard CC = ActiveCards[DeckCardID];
			CC.DrawCount = 0;
			ActiveCards[DeckCardID] = CC;
		}


	}

}

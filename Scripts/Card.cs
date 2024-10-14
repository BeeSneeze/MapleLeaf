using Godot;
using System;
using System.Collections.Generic;

public class Card : Sprite
{
	public string CardName;

	// Different card visuals
	public bool Clickable = true;

	public bool CanQueue = false;

	public int Layer = 0;

	public string PreppedRotation = "";
	
	
	public bool Prepped = false;	// When the player is preparing a play for a card
	private bool Big = false;		// When the player wants extra info about a card
	private bool Preview = false; 	// When the player is hovering over a card

	public bool Skipped = false; // Card is completely unclickable until its game object is destroyed
	public bool ReRolled = false; // Going to ReRoll this card upon draw end
	public GameManager GM;

	public string CornerSymbol = "None";

	public AnimatedSprite CornerSprite;

	public string OwnerName;

	// Card specifics
	public int CardID; // Unique identifier. Do % 1000 to get the specific card type
	public int OwnerID; // If this person dies, remove the card
	public int PlayerID; // Who is currently playing this card?
	public string MatrixName {get; private set;}
	public int Range {get; private set;}
	public int Uses {get; private set;}
	public int Draws;
	public string RatName;
	public string TargetType {get; private set;}
	public string TargetCell {get; private set;}
	public string FlavorText {get; private set;}
	public string AbilityText {get; private set;}
	public List<Ability> AbilityList {get; private set;}
	public List<Ability> SecondaryList {get; private set;}

	public string CardFlavor = "Negative";

	// Loads all the information about the card. This is the only way to edit from outside
	public void LoadInfo(CardType CardInfo)
	{
		MatrixName = CardInfo.MatrixName;
		Range = int.Parse(CardInfo.Range);
		Uses = int.Parse(CardInfo.Uses);
		TargetType = CardInfo.TargetType;
		TargetCell = CardInfo.TargetCell;
		FlavorText = CardInfo.FlavorText;
		AbilityText = CardInfo.AbilityText;
		AbilityList = CardInfo.AbilityList;
		SecondaryList = CardInfo.SecondaryList;
	}

	private List<Node2D> Keywords = new List<Node2D>();

	// Called when the node enters the scene tree for the first time. Executed after LoadInfo
	public override void _Ready()
	{
		CornerSprite = GetNode<AnimatedSprite>("CornerSprite");
		CornerSprite.Animation = CornerSymbol;

		((Sprite)this).Scale = new Vector2(0.0f,0.0f);

		SceneTreeTween tween = GetTree().CreateTween();
		tween.TweenProperty((Sprite)this, "scale", new Vector2(0.5f, 0.5f), 0.20f);
		ZIndex = 100 + Layer;
		Control N2D = GetNode<Control>("FlavorText");
		N2D.RectScale = new Vector2(0.0f, 0.0f);

		// Load the card picture
		Sprite Image = GetNode<Sprite>("Picture");
		Image.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/" + CardName + ".png");

		// Set the flavortext
		RichTextLabel RichLabel = (RichTextLabel)GetNode("FlavorText");
		if(AbilityText != "")
		{
			RichLabel.AppendBbcode("[center]" + FlavorText + "\n [i](" + AbilityText + ")");
		}
		else
		{
			RichLabel.AppendBbcode("[center]" + FlavorText);
		}
		

		if(Draws == 1)
		{
			Ability A = new Ability();
			A.Name = "Exhaust";
			A.Effect = "Forced";
			bool ShouldAdd = true;
			foreach(Ability AExist in SecondaryList)
			{
				if(AExist.Name == "Exhaust")
				{
					ShouldAdd = false;
				}
			}
			if(ShouldAdd)
			{
				SecondaryList.Add(A);
			}
			
		}

		// Set the background corresponding to the ability
		
		foreach(Ability A in AbilityList)
		{
			switch(A.Name)
			{
				case "Move":
					CardFlavor = "Move";

					if(OwnerName == "Rat")
					{
						CardFlavor = "MoveRat";
					}
				break;
				case "Stun":
					if(CardFlavor!="Damage")
					{
						CardFlavor = "Harm";
					}
				break;
				case "Damage":
					CardFlavor = "Damage";
				break;
				case "Swap":
					if(CardFlavor!="Damage")
					{
						CardFlavor = "Harm";
					}
				break;
				
				case "Spawn":
					CardFlavor = "Rat";
				break;
			}
		}

		if(TargetCell == "Friendly")
		{
			CardFlavor = "Support";
		}

		Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/Card" + CardFlavor + ".png");

		SpawnKeywords();

		UpdateLabels();
	}

	// Shows the appropriate visuals for abilities
	private void SpawnKeywords()
	{
		List<Ability> AllAbilities = new List<Ability>();
		foreach(Ability A in AbilityList)
		{
			if(A.Name != "Spawn")
			{
				AllAbilities.Add(A);
			}
			
		}
		foreach(Ability A in SecondaryList)
		{
			AllAbilities.Add(A);
		}

		if(TargetType == "Area")
		{
			Ability AreaA = new Ability();
			AreaA.Name = "Area";
			AllAbilities.Add(AreaA);
		}

		// The positionings of keywords depending on the amount of abilities
		Vector2[] OnePos = {new Vector2(0,80)};
		Vector2[] OneScale = {new Vector2(1,1)};

		Vector2[] TwoPos = {new Vector2(-55,80), new Vector2(55,80)};
		Vector2[] TwoScale = {new Vector2(1,1), new Vector2(1,1)};

		Vector2[] ThreePos = {new Vector2(-80,60), new Vector2(0,100), new Vector2(80,60)};
		Vector2[] ThreeScale = {new Vector2(0.9f,0.9f), new Vector2(0.9f,0.9f), new Vector2(0.9f,0.9f)};

		Vector2[] FourPos = {new Vector2(-80,50), new Vector2(-25,110), new Vector2(25,50), new Vector2(80,110)};
		Vector2[] FourScale = {new Vector2(0.8f,0.8f), new Vector2(0.8f,0.8f), new Vector2(0.8f,0.8f), new Vector2(0.8f,0.8f)};

		var scene = GD.Load<PackedScene>("res://Scenes/Keyword.tscn");

		int index = 0;

		foreach(Ability A in AllAbilities)
		{
			Node2D NewKey = (Node2D)scene.Instance();

			switch(AllAbilities.Count)
			{
				case 1:
					NewKey.Translate(OnePos[index]);
					NewKey.Scale = OneScale[index];
				break;
				case 2:
					NewKey.Translate(TwoPos[index]);
					NewKey.Scale = TwoScale[index];
				break;
				case 3:
					NewKey.Translate(ThreePos[index]);
					NewKey.Scale = ThreeScale[index];
				break;
				case 4:
					NewKey.Translate(FourPos[index]);
					NewKey.Scale = FourScale[index];
				break;
			}


			Sprite KeySprite = (Sprite)NewKey;
			if(A.Name == "Exhaust")
			{
				KeySprite.Texture = (Texture)GD.Load("res://Assets/Visuals/CardKeywords/" + A.Name + A.Effect + ".png");
			}
			else
			{
				KeySprite.Texture = (Texture)GD.Load("res://Assets/Visuals/CardKeywords/" + A.Name + ".png");
			}
			

			Label KeyText = (Label)NewKey.GetNode("Label");
			KeyText.Text = "";

			switch(A.Name)
			{
				case "Move":
					KeyText.Text = Range.ToString();
				break;
				case "Damage":
					KeyText.Text = A.Effect;
				break;
				case "Push":
					KeyText.Text = "";
				break;
				case "Area":
					KeyText.Text = "!";
				break;
				case "Spawn":
					KeyText.Text = "";
				break;
				case "Exhaust":
					KeyText.Text = "";
				break;

				case "Shuffle":
					//string[] ShuffleStrings = (A.Effect).Split(":");
					//KeyText.Text = ShuffleStrings[1];
					KeyText.Text = "";
				break;
				case "Create":
					//string[] ShuffleStrings = (A.Effect).Split(":");
					//KeyText.Text = ShuffleStrings[1];
					KeyText.Text = "";
				break;
				
			}
			
			AddChild(NewKey);
			Keywords.Add(NewKey);

			index++;
		}
	}

	

	// Update all the labels on the card
	public void UpdateLabels()
	{
		// Set the title of the card to the name
		Label Title = GetNode<Label>("Title");
		Title.Text = CardName;

		if(CardID % 2 == 0 && CardID % 1000 < 400 && CardID % 1000 > 99)
		{
			CanvasItem CTitle = (CanvasItem)Title;
			CTitle.Modulate =  new Color(1,0.95f,0.0f,1);
			Node2D UpgradeOverlay = GetNode<Node2D>("UpgradeOverlay");
			UpgradeOverlay.Show();
		}

		// Set the range of the card
		Label RLabel = GetNode<Label>("Range");
		RLabel.Text = Range.ToString();

		// Set the amount of uses of the card
		Label ULabel = GetNode<Label>("Uses");
		ULabel.Text = Uses.ToString();

		Label MLabel = GetNode<Label>("MiddleLabel");
		if(Draws > 0) // If it has limited draws, show the draws
		{
			MLabel.Text = Draws.ToString();
		}
		else if(RatName != "") // If it's a rat, show the name corresponding to the ID
		{
			MLabel.Text = RatName;
		}
		else
		{
			MLabel.Text = "";
		}

		CoverLabels();

	}

	// Covers the bottom labels on the card that are not in use
	private void CoverLabels()
	{
		string AmmoOn = "1";
		string MiddleOn = "1";
		string RangeOn = "1";

		AnimatedSprite Cover = GetNode<AnimatedSprite>("LabelCover");


		if(Uses == 1)
		{
			AmmoOn = "0";
		}

		if(OwnerID < 400)
		{
			MiddleOn = "0";
		}

		if(MatrixName == "Global" || MatrixName == "None")
		{
			RangeOn = "0";
		}

		Cover.Animation = AmmoOn + MiddleOn + RangeOn;

		CanvasItem CoverNode = (CanvasItem)Cover;

		switch(CardFlavor)
		{
			case "Damage":
				CoverNode.Modulate = new Color(0.8353f,0.3451f,0.3451f,1);
			break;
			case "Move":
				CoverNode.Modulate = new Color(0.4627f,0.6706f,0.8510f,1);
			break;
			case "MoveRat":
				CoverNode.Modulate = new Color(0.4627f,0.6706f,0.8510f,1);
			break;
			case "Harm":
				CoverNode.Modulate = new Color(0.9490f,0.6196f,0.2f,1);
			break;
			case "Support":
				CoverNode.Modulate = new Color(0.4902f,0.7490f,0.2431f,1);
			break;
			case "Negative":
				CoverNode.Modulate = new Color(0.3804f,0.3804f,0.3804f,1);
			break;
			case "Rat":
				CoverNode.Modulate = new Color(0.7059f,0.7059f,0.7059f,1);
			break;
		}
		
	}

	// Discard card
	public void Discard()
	{
		if(--Uses < 1) // Ran out of uses
		{
			bool ShouldExhaust = false;
			foreach(Ability A in SecondaryList)
			{
				if(A.Name == "Exhaust")
				{
					ShouldExhaust = true;
				}
			}

			var CM = GetParent();

			if(ShouldExhaust)
			{
				CM.CallDeferred("ExhaustCard", this);
			}
			else
			{
				CM.CallDeferred("DiscardCard", this);
			}
		}
		else // Still have some amount of uses left
		{
			PlayEffect("Use");
			UpdateLabels();
			Prep(false);
			Prepped = false;
		}
	}

	// Show a visual effect of some kind
	private void PlayEffect(string InString)
	{
		AnimatedSprite AnimSpr = GetNode<AnimatedSprite>("Effect");
		AnimSpr.Animation = "None";
		AnimSpr.Animation = InString;
	}

	// CARD VISUALIZER MODES

	// Go back to normal
	public void NormalMode()
	{
		SceneTreeTween tween = GetTree().CreateTween();
		tween.TweenProperty((Sprite)this, "scale", new Vector2(0.5f, 0.5f), 0.07f);
		ZIndex = 100 + Layer;
			
		Control N2D = GetNode<Control>("FlavorText");
		N2D.RectScale = new Vector2(0.0f, 0.0f);

		foreach(Node2D Key in Keywords)
		{
			Key.Show();
		}
	}

	// Visualizes what a card does, and prepares it for play
	public void Prep(bool InBool)
	{
		
		if(InBool)
		{
			GM.UnPrep(true);
			Node2D PrepHalo = GetNode<Node2D>("PrepHalo");
			PrepHalo.Show();
			GM.PrepPlay(this);
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(0.6f, 0.6f), 0.07f);
			ZIndex = 110 + Layer;

			// Make sure the keywords are visible
			Control N2D = GetNode<Control>("FlavorText");
			N2D.RectScale = new Vector2(0.0f, 0.0f);

			foreach(Node2D Key in Keywords)
			{
				Key.Show();
			}
		}
		else
		{
			GM.UnPrep();
			Node2D PrepHalo = GetNode<Node2D>("PrepHalo");
			PrepHalo.Hide();
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(0.5f, 0.5f), 0.07f);
			ZIndex = 100 + Layer;
		}

		Prepped = InBool;
	}

	// Visualizes what a card does, but does *not* prepare it for play
	public void PreviewMode(bool InBool)
	{
		if(InBool)
		{
			GM.UnBig(); // Prioritise manager bigmode first
			GM.ShowPlay(this);
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(0.55f, 0.55f), 0.07f);
			ZIndex = 110 + Layer;
		}
		else
		{
			if(!Prepped && !Big)
			{
				NormalMode();
			}
		}
	}

	// Makes a card big and displays helper information
	public void BigMode(bool InBool)
	{
		if(InBool)
		{
			GM.UnBig(); // Prioritise manager bigmode first
			var CM = GetParent();
			CM.CallDeferred("BigMode", this);

			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(1.0f, 1.0f), 0.07f);
			ZIndex = 110 + Layer;
			Control N2D = GetNode<Control>("FlavorText");
			N2D.RectScale = new Vector2(1.0f, 1.0f);
			foreach(Node2D Key in Keywords)
			{
				Key.Hide();
			}

		}
		else
		{
			if(!Prepped && !Preview)
			{
				NormalMode();
			}
		}

		Big = InBool;
	}

	// Skip a card for this turn, making it unplayable
	public void Skip(bool Unplayable = false)
	{
		Sprite Overlay = GetNode<Sprite>("Overlay");
		Overlay.Show();
		Overlay.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/Skipped.png");
		Skipped = true;

		if(Unplayable)
		{
			var CM = GetParent();
			CM.CallDeferred("LoadCardEffect", "Unplayable", this);
		}
	}

	// Toggle reroll of card
	public void ToggleReroll()
	{
		ReRolled = !ReRolled;
		Sprite Overlay = GetNode<Sprite>("Overlay");
		
		if(ReRolled)
		{
			Overlay.Show();
			Overlay.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/ReRoll.png");
		}
		else
		{
			Overlay.Hide();
		}
	}

	public void ToggleClickable(bool InBool)
	{
		Clickable = InBool;
		if(Clickable && !Skipped)
		{
			Sprite Overlay = GetNode<Sprite>("Overlay");
			Overlay.Hide();
		}
		if(!Clickable && !Skipped)
		{
			Sprite Overlay = GetNode<Sprite>("Overlay");
			Overlay.Show();
			Overlay.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/UnClickable.png");
		}
	}


	// MOUSE ACTIONS
	public void LeftClick()
	{
		if(Clickable && !Skipped)
		{
			if(CanQueue)
			{
				GD.Print("TRYING TO QUEUE");
				ToggleReroll();
				return;
			}


			if(Prepped) // Click the second time to execute the play
			{
				//GM.ExecutePlay();
				Prepped = false;
				Prep(false);
			}
			else // The first click preps the play
			{
				Big = false;
				GM.UnBig();
				BigMode(Big);
				Prepped = true;
				Prep(Prepped);
			}
			
		}
	}

	public void RightClick()
	{

		if(Prepped) // Right click to abort play
		{
			Prepped = false;
			Prep(false);
		}
		else if(!GM.PrepMode)
		{
			Big = true;
			BigMode(true);
		}
	}

	public void MouseEnter()
	{
		if(!GM.PrepMode)
		{
			Preview = true;
			PreviewMode(true);
		}
		
	}

	public void MouseExit()
	{
		if(!GM.PrepMode)
		{
			Preview = false;
			PreviewMode(false);
			GM.UnPrep();
			BigMode(false);
		}
	}
}

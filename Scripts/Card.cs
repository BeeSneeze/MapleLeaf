using Godot;
using System;
using System.Collections.Generic;

public class Card : Sprite
{
	public string CardName;

	// Different card visuals
	public bool Prepped = false;
	private bool Clickable = true;
	private bool Big = false;

	private bool Skipped = false; // Card is completely unclickable until its game object is destroyed
	private GameManager GM;

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

	private string ChosenAbility = "Negative";

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

		Vector2[] FourPos = {new Vector2(-80,60), new Vector2(-25,100), new Vector2(25,60), new Vector2(80,100)};
		Vector2[] FourScale = {new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(1,1)};

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
			KeyText.Text = A.Effect;

			switch(A.Name)
			{
				case "Move":
					KeyText.Text = Range.ToString();
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
				case "Swap":
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
				
			}
			
			AddChild(NewKey);
			Keywords.Add(NewKey);

			index++;
		}
	}

	// Called when the node enters the scene tree for the first time. Executed after LoadInfo
	public override void _Ready()
	{
		((Sprite)this).Scale = new Vector2(0.0f,0.0f);

		SceneTreeTween tween = GetTree().CreateTween();
		tween.TweenProperty((Sprite)this, "scale", new Vector2(0.5f, 0.5f), 0.20f);
		ZIndex = 100;
		Control N2D = (Control)GetNode("FlavorText");
		N2D.RectScale = new Vector2(0.0f, 0.0f);

		GM = (GameManager)GetParent().GetParent();

		// Load the card picture
		Sprite Image = (Sprite)GetNode("Picture");
		Image.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/" + CardName + ".png");

		// Set the flavortext
		RichTextLabel RichLabel = (RichTextLabel)GetNode("FlavorText");
		RichLabel.AppendBbcode("[center]" + FlavorText + "\n [u]" + AbilityText);

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

		UpdateLabels();

		// Set the background corresponding to the ability
		
		foreach(Ability A in AbilityList)
		{
			switch(A.Name)
			{
				case "Move":
					ChosenAbility = "Move";
					if(((CardManager)GetParent()).OwnerName == "Rat")
					{
						ChosenAbility = "MoveRat";
					}
				break;
				case "Stun":
					ChosenAbility = "Harm";
				break;
				case "Swap":
					if(ChosenAbility!="Damage")
					{
						ChosenAbility = "Harm";
					}
				break;
				case "Damage":
					ChosenAbility = "Damage";
				break;
				case "Spawn":
					ChosenAbility = "Rat";
				break;
			}
		}

		if(TargetCell == "Friendly")
		{
			ChosenAbility = "Support";
		}

		Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/Card" + ChosenAbility + ".png");

		SpawnKeywords();

	}

	// Update all the labels on the card
	public void UpdateLabels()
	{
		// Set the title of the card to the name
		Label Title = (Label)GetNode("Title");
		Title.Text = CardName;

		// Set the range of the card
		Label RLabel = (Label)GetNode("Range");
		RLabel.Text = Range.ToString();

		// Set the amount of uses of the card
		Label ULabel = (Label)GetNode("Uses");
		ULabel.Text = Uses.ToString();

		Label MLabel = (Label)GetNode("MiddleLabel");
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

	}

	// Visualizes what a card does, without playing it
	public void Prep(bool InBool)
	{
		if(InBool)
		{
			Node2D PrepHalo = (Node2D)GetNode("PrepHalo");
			PrepHalo.Show();
			GM.PrepPlay(this);
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(0.6f, 0.6f), 0.07f);
			ZIndex = 101;
		}
		else
		{
			Node2D PrepHalo = (Node2D)GetNode("PrepHalo");
			PrepHalo.Hide();
			GM.UnPrep();
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(0.5f, 0.5f), 0.07f);
			ZIndex = 100;
		}
	}

	// Discard card
	public void Discard()
	{
		if(--Uses < 1) // Ran out of uses
		{
			bool ShouldExhaust = false;

			CardManager CM = (CardManager)GetParent();
			foreach(Ability A in SecondaryList)
			{
				if(A.Name == "Exhaust")
				{
					ShouldExhaust = true;
				}
			}

			if(ShouldExhaust)
			{
				CM.ExhaustCard(this);
			}
			else
			{
				CM.DiscardCard(this);
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
		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Effect");
		AnimSpr.Animation = "None";
		AnimSpr.Animation = InString;
	}


	// BIG MODE BIG MODE BIG MODE BIG MODE BIG MODE
	public void BigMode(bool InBool)
	{
		if(InBool)
		{
			GM.UnBig(); // Prioritise manager bigmode first
			GM.ShowPlay(this);
			CardManager CM = (CardManager)GetParent();
			CM.BigMode(this);
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(1.0f, 1.0f), 0.07f);
			ZIndex = 101;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(1.0f, 1.0f);
			foreach(Node2D Key in Keywords)
			{
				Key.Hide();
			}

		}
		else
		{
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(0.5f, 0.5f), 0.07f);
			ZIndex = 100;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(0.0f, 0.0f);

			foreach(Node2D Key in Keywords)
			{
				Key.Show();
			}
		}

		Big = InBool;
	}

	// Skip a card for this turn, making it unplayable
	public void Skip(bool Unplayable = false)
	{
		Sprite Overlay = (Sprite)GetNode("Overlay");
		Overlay.Show();
		Overlay.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/Skipped.png");
		Skipped = true;

		if(Unplayable)
		{
			((CardManager)GetParent()).LoadCardEffect("Unplayable", this);
		}
	}


	public void ToggleClickable(bool InBool)
	{
		Clickable = InBool;
		if(Clickable && !Skipped)
		{
			Sprite Overlay = (Sprite)GetNode("Overlay");
			Overlay.Hide();
		}
		if(!Clickable && !Skipped)
		{
			Sprite Overlay = (Sprite)GetNode("Overlay");
			Overlay.Show();
			Overlay.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/UnClickable.png");
		}
	}


	// MOUSE ACTIONS
	public void LeftClick()
	{
		if(Clickable && !Skipped)
		{
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
		if(Clickable && !Skipped)
		{
			if(Prepped) // Right click to abort play
			{
				Prepped = false;
				Prep(false);
			}
			else
			{
				Big = !Big;
				BigMode(Big);
				if(Big == false)
				{
					GM.UnPrep();
				}
			}
			
		}
	}

	public void MouseEnter()
	{
		if(!Skipped)
		{
			//Prep(true);
		}
	}

	public void MouseExit()
	{
		if(!Skipped)
		{
			//Prep(false);
		}
	}
}

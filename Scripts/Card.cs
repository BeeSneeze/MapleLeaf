using Godot;
using System;
using System.Collections.Generic;

public class Card : Sprite
{
	public string CardName;

	// Different card visuals
	public bool Prepped = false;
	public bool Clickable = true;
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

	// Called when the node enters the scene tree for the first time. Executed after LoadInfo
	public override void _Ready()
	{
		((Sprite)this).Scale = new Vector2(0.6f,0.6f);

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
			A.Effect = "Self";
			SecondaryList.Add(A);
		}

		UpdateLabels();

		// Set the background corresponding to the ability
		string ChosenAbility = "Negative";
		foreach(Ability A in AbilityList)
		{
			switch(A.Name)
			{
				case "Move":
					ChosenAbility = "Move";
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
				if(A.Name == "Exhaust" && A.Effect == "Self")
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
		}
		else
		{
			SceneTreeTween tween = GetTree().CreateTween();
			tween.TweenProperty((Sprite)this, "scale", new Vector2(0.5f, 0.5f), 0.07f);
			ZIndex = 100;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(0.0f, 0.0f);
		}

		Big = InBool;
	}

	// Skip a card for this turn, making it unplayable
	public void Skip(bool Unplayable = false)
	{
		Sprite Overlay = (Sprite)GetNode("Overlay");
		Overlay.Show();
		Skipped = true;

		if(Unplayable)
		{
			((CardManager)GetParent()).LoadCardEffect("Unplayable", this);
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

	}

	public void MouseExit()
	{

	}
}

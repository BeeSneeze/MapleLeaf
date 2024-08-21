using Godot;
using System;
using System.Collections.Generic;

public class Card : Sprite
{

	public string CardName;
	

	// Different card visuals
	private bool Big = false;
	public bool Prepped = false;
	public bool Clickable = true;
	private GameManager GM;

	// Card specifics
	public int CardID;
	public int OwnerID; // If this person dies, remove the card
	public int PlayerID; // Who is currently playing this card?
	public string MatrixName {get; private set;}
	public int Range {get; private set;}
	public int Uses {get; private set;}
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
		GM = (GameManager)GetParent().GetParent();

		// Load the card picture
		Sprite Image = (Sprite)GetNode("Picture");
		Image.Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/" + CardName + ".png");

		// Set the title of the card to the name
		Label Title = (Label)GetNode("Title");
		Title.Text = CardName;

		// Set the range of the card
		Label RLabel = (Label)GetNode("Range");
		RLabel.Text = Range.ToString();

		// Set the amount of uses of the card
		Label ULabel = (Label)GetNode("Uses");
		ULabel.Text = Uses.ToString();

		// Set the flavortext
		RichTextLabel RichLabel = (RichTextLabel)GetNode("FlavorText");
		RichLabel.AppendBbcode("[center]" + FlavorText + "\n [u]" + AbilityText);

		// Set the background corresponding to the ability
		string ChosenAbility = "Rat";
		foreach(Ability A in AbilityList)
		{
			if(A.Name == "Move")
			{
				ChosenAbility = "Move";
			}
			if(A.Name == "Damage")
			{
				ChosenAbility = "Damage";
			}
		}

		Texture = (Texture)GD.Load("res://Assets/Visuals/Cards/Card" + ChosenAbility + ".png");

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
		CardManager CM = (CardManager)GetParent();
		CM.DiscardCard(this);
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

	public void LeftClick()
	{
		if(Clickable)
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
		if(Clickable)
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
			}
			
		}
	}
}

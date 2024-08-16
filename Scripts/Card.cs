using Godot;
using System;
using System.Collections.Generic;

public class Card : Sprite
{

	public string CardName;
	public int CardID;
	public int OwnerID; // If this person dies, remove the card
	public int PlayerID; // Who is currently playing this card?

	// Different card visuals
	private bool Big = false;
	private bool Prepped = false;

	private GameManager GM;

	// Card specifics
	public string MatrixName {get; private set;}
	public int Range {get; private set;}
	public string TargetType {get; private set;}
	public string FlavorText {get; private set;}
	public List<Ability> AbilityList {get; private set;}
	public List<Ability> SecondaryList {get; private set;}

	// Loads all the information about the card. This is the only way to edit from outside
	public void LoadInfo(CardType CardInfo)
	{
		MatrixName = CardInfo.MatrixName;
		Range = int.Parse(CardInfo.Range);
		TargetType = CardInfo.TargetType;
		FlavorText = CardInfo.FlavorText;
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

		// Set the flavortext
		RichTextLabel RichLabel = (RichTextLabel)GetNode("FlavorText");
		RichLabel.AppendBbcode("[center][i]" + FlavorText);


	}

	// Visualizes what a card does, without playing it
	public void Prep()
	{
		Prepped = true;
		GM.ShowPlay(this);
	}
	
	// Play card
	public void Play()
	{
		GM.ExecutePlay(this);
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
		Prep();
	}

	public void RightClick()
	{
		Big = !Big;
		BigMode(Big);
	}
}

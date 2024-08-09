using Godot;
using System;
using System.Collections.Generic;

public class Card : Sprite
{

	public string CardName;
	public int OwnerID;

	private bool Big = false;

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
	}

	

	public void PlayCard()
	{
		GM.ExecutePlay(this);
	}

	public void BigMode(bool InBool)
	{
		CardManager CM = (CardManager)GetParent();

		// Prioritise manager bigmode first

		if(InBool)
		{
			Big = true;
			GM.UnBig();
			Scale = new Vector2(1.0f, 1.0f);
			ZIndex = 101;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(1.0f, 1.0f);
		}
		else
		{
			Big = false;
			Scale = new Vector2(0.5f, 0.5f);
			ZIndex = 100;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(0.0f, 0.0f);
		}
		
	}

	public void RightClick()
	{
		Big = !Big;
		BigMode(Big);
	}

	public void LeftClick()
	{
		PlayCard();
	}
}

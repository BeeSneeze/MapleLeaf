using Godot;
using System;
using System.Collections.Generic;

public class Card : Sprite
{

	public string CardName;
	public int OwnerID;

	private bool Big = false;

	// Card specifics
	public string MatrixName;
	public int Range;
	public string TargetType;
	public string FlavorText;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load the card picture
		Sprite Image = (Sprite)GetNode("Picture");
		var img = (Texture)GD.Load("res://Assets/Visuals/Cards/" + CardName + ".png");
		Image.SetTexture(img);

		// Set the title of the card to the name
		Label Title = (Label)GetNode("Title");
		Title.Text = CardName;

		// Set the range of the card
		Label RLabel = (Label)GetNode("Range");
		RLabel.Text = Range.ToString();
	}

	public void PlayCard()
	{
		GD.Print("Card Played: " + CardName);
	}

	public void BigMode(bool InBool)
	{
		CardManager CM = (CardManager)GetParent();

		CM.BigMode(InBool);

		if(InBool)
		{
			Scale = new Vector2(1.0f, 1.0f);
			ZIndex = 101;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(1.0f, 1.0f);

			//this.GetParent().MoveChild(this, 3); // Trying to order the cards so you can't click a card behind the focused one
		}
		else
		{
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

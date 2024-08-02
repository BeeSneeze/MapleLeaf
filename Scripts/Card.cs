using Godot;
using System;

public class Card : Sprite
{

	public string CardName;
	public int OwnerID;

	// Card specifics
	private string MatrixName;
	private int Range;
	private string TargetType;
	private string FlavorText;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Load JSON stuff from the name
		Range = 2;
		TargetType = "Multi"; // Single, Multi, Area
		FlavorText = "With the power of friendship, we can accomplish anything![u]Spawn three rats";

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
		if(InBool)
		{
			GD.Print("Card Went BigMode");
			Scale = new Vector2(1.0f, 1.0f);
			ZIndex = 101;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(1.0f, 1.0f);
		}
		else
		{
			Scale = new Vector2(0.5f, 0.5f);
			ZIndex = 100;
			Control N2D = (Control)GetNode("FlavorText");
			N2D.RectScale = new Vector2(0.0f, 0.0f);
		}
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

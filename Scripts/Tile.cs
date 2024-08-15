using Godot;
using System;

public class Tile : Node2D
{	
	public int X, Y;

	private string Terrain;

	// Character information
	public int MaxHP;
	public int HP;

	Label NameLabel;
	ColorRect LabelBox;

	public Character Char = new Character(); // The character currently on the tile

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		LabelBox = (ColorRect)GetNode("LabelBox");
		NameLabel = (Label)GetNode("LabelBox").GetNode("Label");

		LabelBox.Hide();
		Terrain = "Grass";
		Translate(new Vector2(X,Y));
	}

	public void SetTerrain(string InString)
	{
		Terrain = InString;
		Sprite spr = (Sprite)GetNode("Terrain");
		spr.Texture = (Texture)GD.Load("res://Assets/Visuals/Terrain/" + InString + ".png");
		if(InString == "Mountains")
		{
			Char.ID = 50;
			SetCharacter(Char);
		}

	}


	// Creates a new character on a tile, using the character ID
	public void CreateCharacter(string CharName)
	{
		Board Brd = (Board)GetParent();
		CharacterInfo CI = Brd.AllCharacters[CharName];
		GameManager GM = (GameManager)GetParent().GetParent();


		Char.ID = GM.NewCharacterID(int.Parse(CI.ID));
		Char.MaxHP = int.Parse(CI.MaxHP);
		Char.HP = Char.MaxHP;


		Char.Name = CI.Names[0];

		if(CI.Names.Count > 1)
		{
			Random rnd = new Random();
			Char.Name = CI.Names[rnd.Next(1,CI.Names.Count)];
		}
		


		switch(Char.ID % 100)
		{
			case 0: // Empty tile

			break;
			case 1: // Soldier

			break;
			case 2: // Sniper

			break;
			case 3: // Support

			break;
			case 4: // City

			break;
			case 10: // Tutorial Rat
				
			break;
		}

		

		SetCharacter(Char);
	}

	public void SetCharacter(Character InChar)
	{
		Char = InChar;

		NameLabel.Text = Char.Name;

		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Character");

		LabelBox.Show();

		// Character IDs: 0 = Nothing, 1-9 = friendly characters, 10-49 = enemy characters, 50+ = misc.
		switch(Char.ID % 100)
		{
			case 0: // Empty tile
				LabelBox.Hide();
				AnimSpr.Animation = "None";
			break;
			case 1:
				AnimSpr.Animation = "Soldier";
			break;
			case 2:
				AnimSpr.Animation = "Sniper";
			break;
			case 3:
				AnimSpr.Animation = "Support";
			break;
			case 4:
				LabelBox.Hide();
				AnimSpr.Animation = "City";
			break;
			case 10:
				AnimSpr.Animation = "RatTutorial";
			break;
			case 50:
				LabelBox.Hide();
				AnimSpr.Animation = "Mountain";
			break;
		}
	}

	public void SetMarker(string InString)
	{
		// Change the animated sprite
		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Marker");
		AnimSpr.Animation = InString;
	}

	public void RightClick()
	{
		GD.Print("Tile right clicked!");
	}

	public void LeftClick()
	{
		GD.Print("Tile left clicked!");
	}
	
}

using Godot;
using System;

public class Tile : Node2D
{	
	
	private string Terrain;
	private string Marker;

	public int X, Y;
	public bool Clickable = false;

	public Character Char = new Character(); // The character currently on the tile

	private Label NameLabel;
	private ColorRect LabelBox;

	private Node2D HPNode;
	private bool HPToggle = false;
	
	private GameManager GM;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		HPNode = (Node2D)GetNode("HealthBar");
		GM = (GameManager)GetParent().GetParent();
		LabelBox = (ColorRect)GetNode("LabelBox");
		NameLabel = (Label)GetNode("LabelBox").GetNode("Label");

		LabelBox.Hide();
		Terrain = "Grass";
		Translate(new Vector2(X*88-88*4+42+2,Y*88-88*4+42+2)); // 84x84 pixel boxes + 4 pixel margin between
		UpdateHealthBar();
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

	// Take the numbered amount of damage
	public void TakeDamage(int Dmg)
	{
		if(Char.MaxHP == 0) // Character cannot be damaged
		{
			return;
		}
		
		Char.HP -= Dmg;
		if(Char.HP < 1) // Character dies
		{
			CreateCharacter("None");
		}

		UpdateHealthBar();
		
	}

	// Updates the health bar visual
	public void UpdateHealthBar()
	{
		Sprite HPBar = (Sprite)HPNode;
		if(Char.MaxHP == 0)
		{
			return;
		}

		float TruePercentage = ((float)Char.HP)/((float)Char.MaxHP) * 100f;
		int Percentage = ( ((int)TruePercentage/10)) * 10;

		HPBar.Texture = (Texture)GD.Load("res://Assets/Visuals/HP/HP" + Percentage.ToString() + ".png");

		Label HPLabel = (Label)HPNode.GetNode("Label");
		HPLabel.Text = (Char.HP).ToString();
	}


	// Creates a new character on a tile, using the character ID
	public void CreateCharacter(string CharName)
	{
		Board Brd = (Board)GetParent();
		CharacterInfo CI = Brd.AllCharacters[CharName];
		GameManager GM = (GameManager)GetParent().GetParent();

		if(CharName != "None")
		{
			Char.ID = GM.NewCharacterID(int.Parse(CI.ID));
		}
		else
		{
			Char.ID = 0;
		}
		
		Char.MaxHP = int.Parse(CI.MaxHP);
		Char.HP = Char.MaxHP;

		Random rnd = new Random();
		Char.Name = CI.Names[rnd.Next(0,CI.Names.Count)];

		SetCharacter(Char);
	}

	public void SetCharacter(Character InChar)
	{
		Char = InChar;

		UpdateHealthBar();

		NameLabel.Text = Char.Name;

		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Character");

		AnimSpr.Scale = new Vector2(0.7f,0.7f);

		SceneTreeTween tween = GetTree().CreateTween();
		tween.TweenProperty(AnimSpr, "scale", new Vector2(1.0f, 1.0f), 0.07f);

		// Character IDs: 0 = Nothing, 1-9 = friendly characters, 10-49 = enemy characters, 50+ = misc.
		switch(Char.ID % 100)
		{
			case 0: // Empty tile
				LabelBox.Hide();
				HPNode.Hide();
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


	// Shows the action marker, move, attack etc.
	public void SetMarker(string InString)
	{
		Marker = InString;
		// Change the animated sprite
		AnimatedSprite AnimSpr = (AnimatedSprite)GetNode("Marker");
		AnimSpr.Animation = InString;

		if(((GameManager)GetParent().GetParent()).PrepMode)
		{
			Clickable = !(InString == "None");
		}

		if(InString == "Select")
		{
			Clickable = false;
		}

		if(GM.CurrentCard.TargetType == "Area" && InString != "SelectClickable")
		{
			Clickable = false; // Only allow the select marker to be clicked if it's an area attack
		}
		
	}

	// Adds this cell to the list of cells affected by a specific card ability
	public void AddTarget()
	{
		Board Brd = (Board)GetParent();
		Brd.AddTarget(new Vector2(X,Y));
	}



	// MOUSE INTERACTIONS

	public void LeftClick()
	{
		if(Clickable)
		{
			GD.Print("Tile left clicked!");
			if(Marker != "SelectClickable")
			{
				AddTarget();
			}
			if(GM.CurrentCard.TargetType == "Single" || Marker == "SelectClickable")
			{
				GM.ExecutePlay();
			}
		}
		else
		{

		}
		
	}
	
	public void RightClick()
	{
		if(Char.MaxHP != 0)
		{
			HPToggle = !HPToggle;
			if(HPToggle)
			{
				LabelBox.Show();
				HPNode.Show();
			}
			else
			{
				LabelBox.Hide();
				HPNode.Hide();
			}
		}
		
		if(Clickable)
		{
			
		}
		else
		{

		}
	}

	public void MouseEnter()
	{

	}

	public void MouseExit()
	{

	}
	
}

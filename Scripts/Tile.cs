using Godot;
using System;
using System.Collections.Generic;

public class Tile : Node2D
{	

	public int X, Y;
	public bool Clickable = false;
	public Character Char = new Character(); // The character currently on the tile

	private string Terrain;
	private string Marker;
	private Label NameLabel;
	private ColorRect LabelBox;
	private Node2D HPNode;
	private bool HPToggle = false;
	private GameManager GM;
	Node2D WARNING;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		WARNING = GetNode<Node2D>("Warning");
		HPNode = GetNode<Node2D>("HealthBar");
		GM = GetParent().GetParent<GameManager>();
		LabelBox = (ColorRect)GetNode("LabelBox");
		NameLabel = GetNode("LabelBox").GetNode<Label>("Label");

		LabelBox.Hide();
		Terrain = "Grass";
		Translate(new Vector2(X*88-88*4+42+2,Y*88-88*4+42+2)); // 84x84 pixel boxes + 4 pixel margin between
		UpdateHealthBar();
	}

	public void ShowHelp(string InString)
	{
		AnimatedSprite HelpArrows = GetNode<AnimatedSprite>("HelpArrows");

		HelpArrows.Animation = InString;
	}

	// Creates a specific terrain for a tile
	public void SetTerrain(string InString)
	{
		Terrain = InString;
		Sprite spr = GetNode<Sprite>("Terrain");
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
			if(Char.ID % 100 < 10) // Player characters just get stunned
			{
				PlayEffect("Explosion"); 
				AddModifier("Stun", 1); // Only stun this turn
				Char.HP = Char.MaxHP;
			}
			else if(Char.ID % 100 == 51) // Cities spawn some rubble
			{
				PlayEffect("Explosion");
				GM.CityAttacked(1);
				CreateCharacter("Rubble");
			}
			else // Everyone else actually dies
			{
				if(Char.ID % 100 > 9 && Char.ID % 100 < 50)
				{
					GM.RatIDList.Remove(Char.ID);
					GM.CheckWin();
				}

				GM.SkipCard(Char.ID);

				PlayEffect("Death");
				CreateCharacter("None");
			}


		}
		else
		{
			PlayEffect("Explosion");
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

		Char.ModifierData = new List<Modifier>();

		if(CharName != "None")
		{
			Char.ID = GM.NewCharacterID(int.Parse(CI.ID));
			switch(CharName)
			{
				case "Soldier":
					Char.ID = 101;
				break;
				case "Sniper":
					Char.ID = 202;
				break;
				case "Support":
					Char.ID = 303;
				break;
				case "Mountain":
					Char.AddModifier("Immovable");
				break;
				case "City":
					Char.AddModifier("Immovable");
				break;
				case "Rubble":
					Char.AddModifier("Immovable");
				break;
			}
		}
		else
		{
			Char.ID = 0;
		}
		
		Char.MaxHP = int.Parse(CI.MaxHP);
		Char.HP = Char.MaxHP;

		Random rnd = new Random();
		Char.Name = CI.Names[rnd.Next(0,CI.Names.Count)];


		if(Char.ID % 100 > 9 && Char.ID % 100 < 50)
		{
			GM.RatIDToName.Add(Char.ID, Char.Name);
			GM.RatIDList.Add(Char.ID);
		}

		SetCharacter(Char);
	}

	public void SetCharacter(Character InChar)
	{
		Char = InChar;

		UpdateHealthBar();

		NameLabel.Text = Char.Name;

		AnimatedSprite ModAnim = GetNode<AnimatedSprite>("Modifier");
		ModAnim.Animation = "None";

		foreach(Modifier M in Char.ModifierData)
		{
			switch(M.Name)
			{
				case "Stun":
					ModAnim.Animation = "Stun";
				break;
				case "Strong":
					ModAnim.Animation = "Strong";
				break;
			}
		}

		AnimatedSprite AnimSpr = GetNode<AnimatedSprite>("Character");

		AnimSpr.Scale = new Vector2(0.7f,0.7f);

		SceneTreeTween tween = GetTree().CreateTween();
		tween.TweenProperty(AnimSpr, "scale", new Vector2(1.0f, 1.0f), 0.07f);

		if(Char.ID != 0 && Char.ID % 100 != 52)
			PlayEffect("Step");

		AnimSpr.FlipH = X < 4 && Char.ID % 100 < 50;

		AnimatedSprite WAnim = (AnimatedSprite)WARNING;

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
				WAnim.Animation = "Player";
			break;
			case 2:
				AnimSpr.Animation = "Sniper";
				WAnim.Animation = "Player";
			break;
			case 3:
				AnimSpr.Animation = "Support";
				WAnim.Animation = "Player";
			break;
			case 10:
				AnimSpr.Animation = "RatTutorial";
			break;
			case 11:
				AnimSpr.Animation = "RatNormal";
			break;
			case 12:
				AnimSpr.Animation = "RatLawyer";
			break;
			case 13:
				AnimSpr.Animation = "RatSparky";
			break;
			case 14:
				AnimSpr.Animation = "RatChef";
			break;
			case 15:
				AnimSpr.Animation = "RatNinja";
			break;
			case 16:
				AnimSpr.Animation = "RatMenace";
			break;
			case 17:
				AnimSpr.Animation = "RatBodyGuard";
			break;
			case 18:
				AnimSpr.Animation = "RatPresident";
			break;
			case 50:
				LabelBox.Hide();
				AnimSpr.Animation = "Mountain";
			break;
			case 51:
				LabelBox.Hide();
				AnimSpr.Animation = "City";
				WAnim.Animation = "City";
			break;
			case 52:
				LabelBox.Hide();
				HPNode.Hide();
				AnimSpr.Animation = "Rubble";
			break;
		}
	}


	// Shows the action marker, move, attack etc.
	public void SetMarker(string InString)
	{
		WARNING.Hide();

		Marker = InString;
		// Change the animated sprite
		AnimatedSprite AnimSpr = GetNode<AnimatedSprite>("Marker");
		AnimSpr.Animation = InString;

		if(((GameManager)GetParent().GetParent()).PrepMode)
		{
			Clickable = !(InString == "None");
		}

		if(InString == "Select" || InString.Contains("Possible"))
		{
			Clickable = false;
		}

		if(InString == "Support" || InString == "Harm")
		{
			((Node2D)AnimSpr).ZIndex = 10;
		}
		else
		{
			((Node2D)AnimSpr).ZIndex = -10;
		}

		if(InString == "Attack" && (Char.ID % 100 == 51 || Char.ID % 100 < 10) && GM.CurrentCard.TargetType == "Area")
		{
			WARNING.Show();
		}

	}

	// Plays a visual effect to coincide with stuff like dying, taking damage, etc.
	public void PlayEffect(string InString)
	{
		AnimatedSprite AnimSpr = GetNode<AnimatedSprite>("Effect");
		AnimSpr.Animation = "None";
		AnimSpr.Animation = InString;
		
		if(InString == "Step")
		{
			((Node2D)AnimSpr).ZIndex = -10;
		}
		else
		{
			((Node2D)AnimSpr).ZIndex = 0;
		}
	}

	public void AddModifier(string ModName, int ModTime)
	{
		if(!Char.ContainsModifier(ModName) && Char.ID % 100 != 0)
		{
			Char.AddModifier(ModName, ModTime);
			ShowModifier(ModName);

			if(ModName == "Immovable")
			{
				AnimatedSprite AnimSpr = GetNode<AnimatedSprite>("Character");
				AnimSpr.Playing = false;
			}

			if(ModName == "Stun")
			{
				GM.SkipCard(Char.ID);
			}
		}
	}

	// ShowModifier
	public void ShowModifier(string ModName)
	{
		AnimatedSprite AnimSpr = GetNode<AnimatedSprite>("Modifier");
		AnimSpr.Animation = ModName;
	}

	// Adds this cell to the list of cells affected by a specific card ability
	public void AddTarget()
	{
		Board Brd = (Board)GetParent();
		Brd.AddTarget(new Vector2(X,Y));
	}

	// Check if characters are stunned, and skip their cards
	public void StunCheck()
	{
		foreach(Modifier M in Char.ModifierData)
		{
			if(M.Name == "Stun")
			{
				// Do something to prevent this rat from getting a card assigned
				//GM.RatIDList.Remove(Char.ID);
			}
		}
	}


	public void NewTurn()
	{
		Char.AdvanceModifiers();

		AnimatedSprite AnimSpr = GetNode<AnimatedSprite>("Character");
		if(!Char.ContainsModifier("Immovable"))
		{
			AnimSpr.Playing = true;
		}
		

		bool NoModifiers = true;
		
		foreach(Modifier M in Char.ModifierData)
		{
			if(M.Name != "Immovable")
			{
				ShowModifier(M.Name);
				NoModifiers = false;
			}
		}

		if(NoModifiers)
		{
			ShowModifier("None");
		}
	}



	// MOUSE INTERACTIONS

	public void LeftClick()
	{
		if(GM.PrepMode)
		{
			if(GM.CurrentCard.TargetType == "Area")
			{
				GM.ExecutePlay();
			}
		}
		if(Clickable)
		{
			if(GM.CurrentCard.TargetType == "Single")
			{
				AddTarget();
				GM.ExecutePlay();
			}
		}
	}
	
	public void RightClick()
	{	

	}

	public void MouseEnter()
	{
		if(Marker == "Attack" && (Char.ID % 100 == 51 || Char.ID % 100 < 10)  && GM.CurrentCard.TargetType == "Single")
		{
			WARNING.Show();
		}

		if(Char.MaxHP != 0 && Char.ID % 100 != 51)
		{
			LabelBox.Show();
			HPNode.Show();
		}
		if(Char.ID % 100 == 51)
		{
			HPNode.Show();
		}
	}

	public void MouseExit()
	{

		if(Marker == "Attack" && (Char.ID % 100 == 51 || Char.ID % 100 < 10) && GM.CurrentCard.TargetType == "Single")
		{
			WARNING.Hide();
		}

		if(Char.MaxHP != 0)
		{
			LabelBox.Hide();
			HPNode.Hide();
		}
	}
	
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class GameManager : Node2D
{
	public Board Board;
	public string Rot = "Up";
	public Card CurrentCard;
	public bool PrepMode;
	public string Turn = "None"; // Whose turn is it? RatMove, Player, RatAttack, None
	public List<int> RatIDList = new List<int>();
	public Dictionary<int, string> RatIDToName = new Dictionary<int,string>(); // Used to convert from a rat id to a rat name
	public AI AI;
	public CardManager CMSoldier, CMSniper, CMSupport, CMRat;


	private SFXManager SFX;
	private int CardIDCounter = 0;
	private int CharacterIDCounter = 0;
	private Dictionary<string,List<string>> Decks;
	private bool CurrentLoaded = false;
	private bool[,] UnRotated, CurrentMatrix;
	private CanvasItem EndTurnButton, EndDrawButton;

	private Label TopLabel;
	private bool RatShown = false;
	private bool AreYouWinningSon = false;
	private Node2D WinScreen;
	private int LevelHP = 5;
	private int MaxLevelHP;
	private AnimatedSprite TutorialOverlay;
	private bool FinalLevel = false;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TutorialOverlay = GetNode<AnimatedSprite>("TutorialOverlay");
		EndTurnButton = GetNode<CanvasItem>("EndTurn");
		EndDrawButton = GetNode<CanvasItem>("EndDraw");
		WinScreen = GetNode<Node2D>("WinScreen");
		SFX = GetNode<SFXManager>("SFX");
		Board = GetNode<Board>("Board");
		CMSoldier = GetNode<CardManager>("CardsSoldier");
		CMSniper = GetNode<CardManager>("CardsSniper");
		CMSupport = GetNode<CardManager>("CardsSupport");
		CMRat = GetNode<CardManager>("CardsRat");
		AI = GetNode<AI>("AI");

		TopLabel = GetNode<Label>("TopLabel");

		// Load starting decks
		File Reader = new File();
		Reader.Open("res://Assets/Decks.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		Decks = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(Contents);
		Reader.Close();

		string[] CMnames = {"Soldier", "Sniper", "Support", "Tutorial"};

		foreach(string CMName in CMnames)
		{
			foreach(string Cstring in Decks[CMName])
			{
				switch(CMName)
				{
					case "Soldier":
						CMSoldier.AddCard(Cstring);
					break;
					case "Sniper":
						CMSniper.AddCard(Cstring);
					break;
					case "Support":
						CMSupport.AddCard(Cstring);
					break;
					case "Tutorial":
						CMRat.AddCard(Cstring);
					break;
				}
			}
		}

		if(CMSoldier.Shuffle)
			CMSoldier.Deck = CMSoldier.ShufflePile(CMSoldier.Deck);
		if(CMSniper.Shuffle)
			CMSniper.Deck = CMSniper.ShufflePile(CMSniper.Deck);
		if(CMSupport.Shuffle)
			CMSupport.Deck = CMSupport.ShufflePile(CMSupport.Deck);
		if(CMRat.Shuffle)
			CMRat.Deck = CMRat.ShufflePile(CMRat.Deck);

		LevelStart();
	}

	// Performs all the setup for the first level after the tutorial
	public void LoadFirstLevel()
	{
		CMRat.Deck = new List<int>();
		CMRat.TrueDeck = new List<int>();
		CMRat.Hand = new List<int>();
		CMRat.Discard = new List<int>();

		foreach(string Cstring in Decks["Rat"])
		{
			CMRat.AddCard(Cstring);
		}
	}

	// Performs all the setup for the final level of the game
	public void LoadBossFight()
	{
		FinalLevel = true;
		GD.Print("BOSS FIGHT LOADED!!!");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.G)
			{
				CMSupport.AddCard("DEBUG AREA DMG");
				CMSupport.AddCard("DEBUG SINGLE DMG");
				CMSupport.AddCard("DEBUG WIN");
				CMSupport.DrawCard("DEBUG AREA DMG");
				CMSupport.DrawCard("DEBUG SINGLE DMG");
				CMSupport.DrawCard("DEBUG WIN");

			}
			
		}	
	}

	public void PlaySFX(string SFXName)
	{
		SFX.PlaySFX(SFXName);
	}

	//  RatMove->Player->RatAttack->Draw->RatMove etc.
	public void SetMode(string ModeName)
	{
		// Only start AI turns when appropriate
		if(ModeName == "RatAttack" && Turn != "Player")
			return;
		if(ModeName == "RatMove" && Turn != "Draw")
			return;

		UnPrep();
		
		GD.Print("SET MODE: " + ModeName);
		Turn = ModeName;
		switch(ModeName)
		{
			case "RatMove":
				TutorialOverlay.Animation = "None";
				TopLabel.Text = "Rats are on the move...";
				CMSoldier.ToggleQueueMode(false);
				CMSniper.ToggleQueueMode(false);
				CMSupport.ToggleQueueMode(false);
				CMSoldier.ReRoll();
				CMSniper.ReRoll();
				CMSupport.ReRoll();
				CMSoldier.UnClick();
				CMSniper.UnClick();
				CMSupport.UnClick();
				CMRat.ReClick();
				AI.StartMoveMode();
			break;
			case "Player":
				TopLabel.Text = "Play phase: Play cards";
				CMSoldier.ReClick();
				CMSniper.ReClick();
				CMSupport.ReClick();
				CMRat.UnClick();
			break;
			case "RatAttack":
				TopLabel.Text = "Rats are attacking!!";
				CMSoldier.UnClick();
				CMSniper.UnClick();
				CMSupport.UnClick();
				CMRat.ReClick();
				AI.StartAttackMode();
			break;
			case "Draw":
				TopLabel.Text = "Draw Phase: Redraw cards";
				Board.NewTurn();
				CMSoldier.NewTurn();
				CMSniper.NewTurn();
				CMSupport.NewTurn();
				CMRat.NewTurn();
				CMRat.UnClick();
				CMSoldier.ToggleQueueMode(true);
				CMSniper.ToggleQueueMode(true);
				CMSupport.ToggleQueueMode(true);
			break;
		}
	}

	// Rotating attacks around the mouse pointer
	public override void _Process(float delta)
	{
		if(CurrentLoaded && (Turn == "Player" || Turn == "Draw") && !RatShown)
		{
			Vector2 CPos = Board.GetCharPos(CurrentCard.PlayerID);

			Vector2 TilePos = new Vector2(CPos.x*88-88*4+42+2 + 960 ,CPos.y*88-88*4+42+2 + 540);

			Vector2 Diff = GetGlobalMousePosition() - TilePos;

			if(CurrentCard.MatrixName == "Diagonal" || CurrentCard.MatrixName == "Fox")
			{
				if(Diff.x > 0)
				{
					if(Diff.y > 0)
					{
						if(Rot == "Down")
							return; // Avoid uneccesary rotations
						Rotate("Down");
					}
					else
					{
						if(Rot == "Right") 
							return; // Avoid uneccesary rotations
						Rotate("Right");
					}
				}
				else
				{
					if(Diff.y > 0)
					{
						if(Rot == "Left")
							return; // Avoid uneccesary rotations
						Rotate("Left");
					}
					else
					{
						if(Rot == "Up")
							return; // Avoid uneccesary rotations
						Rotate("Up");
					}
				}
			}
			else
			{
				if(Diff.x > Diff.y)
				{
					if(Diff.x * (-1) < Diff.y)
					{
						if(Rot == "Right") 
							return; // Avoid uneccesary rotations
						Rotate("Right");
					}
					else
					{
						if(Rot == "Up")
							return; // Avoid uneccesary rotations
						Rotate("Up");
					}
				}
				else
				{
					if(Diff.x * (-1) < Diff.y)
					{
						if(Rot == "Down")
							return; // Avoid uneccesary rotations
						Rotate("Down");
					}
					else
					{
						if(Rot == "Left")
							return; // Avoid uneccesary rotations
						Rotate("Left");
					}
				}
			}
		}
	}

	// Rotate the currently active matrix in a particular direction
	public void Rotate(string RotDir)
	{
		Rot = RotDir;
		switch(RotDir)
		{
			case "Up":
				CurrentMatrix = (bool[,])UnRotated.Clone();
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			break;
			case "Left":
				CurrentMatrix = (bool[,])UnRotated.Clone();
				RotCounter(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			break;
			case "Down":
				CurrentMatrix = (bool[,])UnRotated.Clone();
				RotCounter(CurrentMatrix);
				RotCounter(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			break;
			case "Right":
				CurrentMatrix = (bool[,])UnRotated.Clone();
				RotClock(CurrentMatrix);
				Board.ShowMatrix(CurrentMatrix, CurrentCard);
			break;
		}
	}

	// Prepares a card for play
	public void PrepPlay(Card Card)
	{
		PrepMode = true;
		Rot = "Up";
		ShowPlay(Card);
	}

	// Enables clicking for all cards again, and clears the board
	public void UnPrep(bool ForceUnprep = false)
	{
		if(ForceUnprep)
		{
			CMSoldier.UnPrep();
			CMSniper.UnPrep();
			CMSupport.UnPrep();
			CMRat.UnPrep();
		}
		

		Board.ClearMarkers();
		CurrentLoaded = false;
		PrepMode = false;
	}

	// Plays a card and its abilities
	public void ExecutePlay()
	{
		switch(CurrentCard.CardFlavor)
		{
			case "Support":	
				PlaySFX("Friendly");
			break;
			case "Move":	
				PlaySFX("Move");
			break;
			case "MoveRat":	
				PlaySFX("MoveRat");
			break;
			case "Harm":	
				PlaySFX("Harm");
			break;
			case "Damage":
				bool NoPunch = true;
				foreach(Ability C in CurrentCard.AbilityList)
				{
					if(C.Name == "Push")
					{
						NoPunch = false;
					}
				}

				if(CurrentCard.TargetType == "Area")
				{
					PlaySFX("Gunshots");
				}
				else if(NoPunch)
				{
					PlaySFX("SingleShot");
				}
			break;
		}


		foreach(Ability A in CurrentCard.AbilityList)
		{
			Board.LoadTheoretical();
			switch(A.Name)
			{
				case "Damage":
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.Damage(Target, int.Parse(A.Effect));
					}
				break;
				case "Move":
					if(Board.TargetList.Count > 0)
					{
						Board.Swap(Board.TargetList[0], Board.GetCharPos(CurrentCard.PlayerID));
					}
				break;
				case "Spawn":
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.Spawn(A.Effect, Target);
					}
				break;
				case "Draw":
					foreach(Vector2 Target in Board.TargetList)
					{
						GD.Print(Board.Cell[(int)Target.x,(int)Target.y].Char.ID);
						// Draw card for one of the player characters
						switch(Board.Cell[(int)Target.x,(int)Target.y].Char.ID)
						{
							case 101:
								CMSoldier.DrawCard();
							break;
							case 202:
								CMSniper.DrawCard();
							break;
							case 303:
								CMSupport.DrawCard();
							break;
						}
						// Draw card for the rats
						if(Board.Cell[(int)Target.x,(int)Target.y].Char.ID % 100 > 10)
						{
							CMRat.DrawCard();
						}
					}
				break;
				case "Create":
					foreach(Vector2 Target in Board.TargetList)
					{
						// Draw card for one of the player characters
						switch(Board.Cell[(int)Target.x,(int)Target.y].Char.ID)
						{
							case 101:
								CMSoldier.AddCard(A.Effect);
								CMSoldier.DrawCard(A.Effect);
							break;
							case 202:
								CMSniper.AddCard(A.Effect);
								CMSniper.DrawCard(A.Effect);
							break;
							case 303:
								CMSupport.AddCard(A.Effect);
								CMSupport.DrawCard(A.Effect);
							break;
						}
						// Draw card for the rats
						if(Board.Cell[(int)Target.x,(int)Target.y].Char.ID % 100 > 9 && Board.Cell[(int)Target.x,(int)Target.y].Char.ID % 100 < 50)
						{
							CMRat.AddCard(A.Effect);
							CMRat.DrawCard(A.Effect);
						}
					}
				break;
				case "Shuffle":
					foreach(Vector2 Target in Board.TargetList)
					{
						// Draw card for one of the player characters
						switch(Board.Cell[(int)Target.x,(int)Target.y].Char.ID)
						{
							case 101:
								CMSoldier.AddCard(A.Effect);
							break;
							case 202:
								CMSniper.AddCard(A.Effect);
							break;
							case 303:
								CMSupport.AddCard(A.Effect);
							break;
						}
						// Draw card for the rats
						if(Board.Cell[(int)Target.x,(int)Target.y].Char.ID % 100 > 9 && Board.Cell[(int)Target.x,(int)Target.y].Char.ID % 100 < 50)
						{
							CMRat.AddCard(A.Effect);
						}
					}
				break;
				case "Stun":
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.AddModifier(Target, "Stun", int.Parse(A.Effect));
					}
				break;
				case "Strong":
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.AddModifier(Target, "Strong", int.Parse(A.Effect));
					}
				break;
				case "Immovable":
					foreach(Vector2 Target in Board.TargetList)
					{
						Board.AddModifier(Target, "Immovable", int.Parse(A.Effect));
					}
				break;
				case "Push":
					string PushDirection = "N";
					switch(Rot)
					{
						case "Up":
							PushDirection = A.Effect;
							
						break;
						case "Left":
							switch(A.Effect)
							{
								case "N":
									PushDirection = "W";
								break;
								case "W":
									PushDirection = "S";
								break;
								case "S":
									PushDirection = "E";
								break;
								case "E":
									PushDirection = "N";
								break;
							}
							
						break;
						case "Down":
							switch(A.Effect)
							{
								case "N":
									PushDirection = "S";
								break;
								case "W":
									PushDirection = "E";
								break;
								case "S":
									PushDirection = "N";
								break;
								case "E":
									PushDirection = "W";
								break;
							}
						break;
						case "Right":
							switch(A.Effect)
							{
								case "N":
									PushDirection = "E";
								break;
								case "W":
									PushDirection = "N";
								break;
								case "S":
									PushDirection = "W";
								break;
								case "E":
									PushDirection = "S";
								break;
							}
						break;
					}
					foreach(Vector2 Target in Board.TargetList)
					{	
						// Make sure the thing you're trying to push is not immovable
						if(!Board.Cell[(int)Target.x,(int)Target.y].Char.ContainsModifier("Immovable"))
						{
							Board.Push(Target, Target, PushDirection);
						}
						
					}

					Board.MoveQueue();
					PlaySFX("Punch");
					
				break;
				case "Swap":
					if(Board.TargetList.Count > 0)
					{
						Board.Swap(Board.TargetList[0], Board.GetCharPos(CurrentCard.PlayerID));
					}
				break;
			}
		}

		foreach(Ability B in CurrentCard.SecondaryList)
		{
			switch(B.Name)
			{
				case "Win":
					AreYouWinningSon = true;
				break;
				case "Shuffle":
					string[] Effect = (B.Effect).Split(":");
					switch(Effect[0])
					{
						case "Soldier":
							CMSoldier.AddCard(Effect[1]);
						break;
						case "Sniper":
							CMSniper.AddCard(Effect[1]);
						break;
						case "Support":
							CMSupport.AddCard(Effect[1]);
						break;
						case "Rat":
							CMRat.AddCard(Effect[1], RatIDList.LastOrDefault());
						break;
					}
				break;
				case "Create":
					string[] CEffect = (B.Effect).Split(":");
					switch(CEffect[0])
					{
						case "Soldier":
							CMSoldier.AddCard(CEffect[1]);
							CMSoldier.DrawCard(CEffect[1]);
						break;
						case "Sniper":
							CMSniper.AddCard(CEffect[1]);
							CMSniper.DrawCard(CEffect[1]);
						break;
						case "Support":
							CMSupport.AddCard(CEffect[1]);
							CMSupport.DrawCard(CEffect[1]);
						break;
						case "Rat":
							CMRat.AddCard(CEffect[1], RatIDList.LastOrDefault());
							CMRat.DrawCard(CEffect[1]);
						break;
					}
				break;
				case "Draw":
					// Draw card for one of the player characters
					switch(B.Effect)
					{
						case "Soldier":
							CMSoldier.DrawCard();
						break;
						case "Sniper":
							CMSniper.DrawCard();
						break;
						case "Support":
							CMSupport.DrawCard();
						break;
						case "Rat":
							CMRat.DrawCard();
						break;
					}
				break;
			}
		}


		Vector2 PPos = Board.GetCharPos(CurrentCard.PlayerID);
		Tile PTile = Board.Cell[(int)PPos.x,(int)PPos.y];
		
		if(PTile.Char.ContainsModifier("Strong"))
		{
			PTile.Char.AdvanceModifier("Strong");
			if(!PTile.Char.ContainsModifier("Strong"))
			{
				PTile.ShowModifier("None");
			}
		}

		CurrentCard.Discard();
		UnPrep();

		// Only end the level after finishing the turn
		if(AreYouWinningSon)
		{
			if(FinalLevel)
			{
				GameWon();
			}
			else
			{
				LevelEnd();
			}
			
		}
	}

	public void GameOver()
	{
		LevelManager LM = GetParent<LevelManager>();
		LM.ChangeLevel("GameOver");
	}


	public void CityAttacked(int Damage)
	{
		
		SetHP(LevelHP-1);

		Random rnd = new Random();

		for(int i = 0; i < 2; i++)
		{
			switch(rnd.Next(0,3))
			{
				case 0:
					GD.Print("CITY DAMAGE: Soldier got a paramedic");
					CMSoldier.AddCard("Mourn");
					CMSoldier.DrawCard("Mourn");
				break;
				case 1:
					GD.Print("CITY DAMAGE: Sniper got a paramedic");
					CMSniper.AddCard("Mourn");
					CMSniper.DrawCard("Mourn");
				break;
				case 2:
					GD.Print("CITY DAMAGE: Support got a paramedic");
					CMSupport.AddCard("Mourn");
					CMSupport.DrawCard("Mourn");
				break;
			}
		}
	}

	// Visualizes what a card does, but without playing it
	public void ShowPlay(Card Card)
	{
		if(Card.MatrixName == "Global")
		{
			CurrentMatrix = new bool[15,15];
			for(int x = 0; x < 15; x++)
			{
				for(int y = 0; y < 15; y++)
				{
					CurrentMatrix[x,y] = true;
				}
			}
		}
		else
		{
			Vector2 PPos = Board.GetCharPos(Card.PlayerID);
			Tile PTile = Board.Cell[(int)PPos.x,(int)PPos.y];
		
			int MatRange = Card.Range;
			if(PTile.Char.ContainsModifier("Strong"))
			{
				MatRange += 1;
			}

			if(MatRange > 7)
			{
				MatRange = 7;
			}

			CurrentMatrix = LoadMatrix(Card.MatrixName, MatRange);
		}
		
		UnRotated = (bool[,])CurrentMatrix.Clone();
		CurrentLoaded = true;
		CurrentCard = Card;

		if(Card.PreppedRotation != "")
		{
			RatShown = true;
			GD.Print(Card.PreppedRotation);
			Rotate(Card.PreppedRotation);
		}
		else
		{
			RatShown = false;
			Board.ShowMatrix(CurrentMatrix, CurrentCard);
		}
	}

	// Creates a new ID for a card
	public int NewCardID(int InInt)
	{
		CardIDCounter++;
		return CardIDCounter * 1000 + InInt;
	}

	// Creates a new ID for a character
	public int NewCharacterID(int InInt)
	{
		CharacterIDCounter++;
		return CharacterIDCounter * 100 + InInt;
	}


	// Loads an action matrix centered around a character
	public bool[,] LoadMatrix(string MatrixName, int range)
	{
		bool[,] OutMat = new bool[range*2 + 1, range*2 + 1];

		File Reader = new File();

		Reader.Open("res://Assets/Matrices/" + range.ToString() + "/" + MatrixName + ".txt", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		Reader.Close();

		int x = 0;
		int y = 0;

		foreach(char C in Contents)
		{
			// We explicitly check for 1's and 0's, as line ends are two or just one character depending on OS
			switch(C)
			{
				case '0':
					OutMat[x,y] = false;
					x++;
				break;
				case '1':
					OutMat[x,y] = true;
					x++;
				break;
				case '8':
					// This is just to mark the center character, purely for readability
					OutMat[x,y] = false;
					x++;
				break;
				case '\n':
					// Line break, new row to the matrix
					x = 0;
					y++;
				break;
			}

		}

		return OutMat;
	}


	// Rotate a matrix counter clock-wise
	public void RotCounter(bool[,] InMat)
	{
		bool[,] OldMat = (bool[,])InMat.Clone();
		int MatSize = InMat.GetLength(0);

		for(int x = 0; x < MatSize; x++)
		{
			for(int y = 0; y < MatSize; y++)
			{
				InMat[x,y] = OldMat[MatSize-1-y,x];
			}
		}
	}

	// Rotate a matrix clockwise
	public void RotClock(bool[,] InMat)
	{
		bool[,] OldMat = (bool[,])InMat.Clone();
		int MatSize = InMat.GetLength(0);

		for(int x = 0; x < MatSize; x++)
		{
			for(int y = 0; y < MatSize; y++)
			{
				InMat[x,y] = OldMat[y,MatSize-1-x];
			}
		}
	}

	// Reset BigMode for all cards
	public void UnBig()
	{
		CMSoldier.UnBig();
		CMSniper.UnBig();
		CMSupport.UnBig();
		CMRat.UnBig();
	}

	// Temporarily disables a card due to a dying character
	public void SkipCard(int PID)
	{
		CMSoldier.SkipCard(PID);
		CMSniper.SkipCard(PID);
		CMSupport.SkipCard(PID);
		CMRat.SkipCard(PID);
	}

	public void SetHP(int InHP)
	{
		LevelHP = InHP;
		Label HPLabel = GetNode<Label>("HP");
		HPLabel.Text = "HP: " + LevelHP.ToString();

		if(Turn == "None")
		{
			MaxLevelHP = InHP;
		}

		if(LevelHP <= 0)
		{
			GameOver();
		}
	}

	// Everything needed to set up the new round
	public void LevelStart()
	{
		CMSoldier.SettingUp = false;
		CMSniper.SettingUp = false;
		CMSupport.SettingUp = false;
		CMRat.SettingUp = false;

		GD.Print("LEVEL STARTED!");
		
		SetMode("Draw");
		EndTurnButton.Hide();
		EndDrawButton.Show();
	}
	
	// Everything done after the round is over
	public void LevelEnd()
	{
		GD.Print("LEVEL ENDED!");
		GD.Print(MaxLevelHP - LevelHP);
		SetMode("None");
		AreYouWinningSon = false;
		WinScreen.Show();
	}

	public void GameWon()
	{
		GD.Print("GAME WIN SEQUENCE");

		LevelManager LM = (LevelManager)GetParent();
		Story SM = LM.GetNode<Story>("Story");
		SM.StartStory("Ending");
		LM.ChangeLevel("Story");
	}

	// When changing to the world map from the game
	public void MoveToWorldMap()
	{
		LevelManager LM = (LevelManager)GetParent();
		WorldMap WM = (WorldMap)LM.GetNode("WorldMap");

		WM.NextCity();
		LM.ChangeLevel("WorldMap");
		WinScreen.Hide();

		CMSoldier.ResetDeck();
		CMSniper.ResetDeck();
		CMSupport.ResetDeck();
		CMRat.ResetDeck();

		if(WM.CurrentIndex == 2)
		{
			LoadFirstLevel();
		}
	}

	// Checks if there are no more rats, and no more spawn cards left
	// if there is neither, the player wins!
	public void CheckWin()
	{
		if(RatIDList.Count > 0)
		{
			return;
		}
		
		foreach(int CID in CMRat.Deck)
		{
			if(CID % 1000 >= 600)
			{
				return;
			}
		}
		foreach(int CID in CMRat.Deck)
		{
			if(CID % 1000 >= 600)
			{
				return;
			}
		}
		foreach(int CID in CMRat.Deck)
		{
			if(CID % 1000 >= 600)
			{
				return;
			}
		}
		
		// Level does not immediately change, but is rather resolved at the end of the turn
		AreYouWinningSon = true;

	}

	public void TurnButtonClicked()
	{
		if(Turn == "Draw")
		{
			EndTurnButton.Show();
			EndDrawButton.Hide();
			SetMode("RatMove");
		}
		else if(Turn == "Player")
		{
			EndTurnButton.Hide();
			EndDrawButton.Show();
			SetMode("RatAttack");
		}
	}
}

using Godot;
using System;

public class PauseMenu : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	
	CanvasItem MainMenuButton;

	private GameManager GM;
	private LevelManager LM;
	private MusicManager MM;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LM = GetParent<LevelManager>();
		GM = LM.GetNode<GameManager>("Game");
		MainMenuButton = GetNode<CanvasItem>("MainMenuButton");
		
		MM = LM.GetParent().GetNode<MusicManager>("MusicManager");
	}

	public void BackToMain()
	{
		LM = GetParent<LevelManager>();
		LM.ChangeLevel("MainMenu");
		
	}

	public void ToggleMainMenuButton(bool InBool)
	{
		if(InBool)
		{
			MainMenuButton.Show();
		}
		else
		{
			MainMenuButton.Hide();
		}
		
	}

	// Makes the AI play cards faster or slower
	public void UpdateAnimationSpeed(int InInt)
	{
		switch(InInt)
		{
			case 0:
				GM.AI.TurnTime = 1.5f;
			break;
			case 1:
				GM.AI.TurnTime = 1.0f;
			break;
			case 2:
				GM.AI.TurnTime = 0.75f;
			break;
			case 3:
				GM.AI.TurnTime = 0.25f;
			break;
			case 4:
				GM.AI.TurnTime = 0.05f;
			break;
		}
	}

	public void UpdateVolume(float InFloat)
	{
		GD.Print((InFloat-100)/5.0f);
		MM.UpdateVolume((InFloat-100)/5.0f);

		// Maybe not the cleanest way to do this, but personally tweaking the audio scale is kinda important to me
		// The switch statement is here in case I want to tweak things even further
		switch(InFloat)
		{
			case 0.0f:
				MM.UpdateVolume(-80.0f);
			break;
			case 10.0f:
				MM.UpdateVolume(-50.0f);
			break;
			case 20.0f:
				MM.UpdateVolume(-30.0f);
			break;
			case 30.0f:
				MM.UpdateVolume(-20.0f);
			break;
			case 40.0f:
				MM.UpdateVolume(-10.0f);
			break;
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

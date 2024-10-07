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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LM = GetParent<LevelManager>();
		GM = LM.GetNode<GameManager>("Game");
		MainMenuButton = GetNode<CanvasItem>("MainMenuButton");

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

	public void UpdateAnimationSpeed(int InInt)
	{
		switch(InInt)
		{
			case 0:
				GM.AI.TurnTime = 1.5f;
			break;
			case 1:
				GM.AI.TurnTime = 0.9f;
			break;
			case 2:
				GM.AI.TurnTime = 0.6f;
			break;
			case 3:
				GM.AI.TurnTime = 0.2f;
			break;
			case 4:
				GM.AI.TurnTime = 0.05f;
			break;
		}

		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

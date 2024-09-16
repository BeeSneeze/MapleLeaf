using Godot;
using System;

public class CityMarker : AnimatedSprite
{

	[Export] public string CityName;

	private GameManager GM;
	private LevelManager LM;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LM = (LevelManager)GetParent().GetParent();
		GM = (GameManager)LM.GetNode("Game");
	}

	public void LeftClick()
	{
		GD.Print(CityName);
		GM.Board.LoadStage(CityName);
	}

	public void RightClick()
	{
		
	}

	public void MouseEnter()
	{
		
	}

	public void MouseExit()
	{
		
	}
}

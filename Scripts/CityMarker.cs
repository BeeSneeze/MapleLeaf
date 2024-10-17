using Godot;
using System;

public class CityMarker : AnimatedSprite
{

	[Export] public string CityName;

	private GameManager GM;
	private LevelManager LM;
	private WorldMap WM;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		WM = (WorldMap)GetParent();
		LM = (LevelManager)GetParent().GetParent();
		GM = (GameManager)LM.GetNode("Game");
	}

	public void ActivateCity()
	{
		Show();
		Node2D Flag = GetNode<Node2D>("Flag");
		Flag.Hide();
	}

	public void DeactivateCity(){
		Animation = "Cross";
		Node2D Flag = GetNode<Node2D>("Flag");
		Flag.Show();
	}
}

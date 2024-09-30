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
		CanvasItem C = (CanvasItem)GetNode("Control");
		C.Hide();
		Node2D Flag = GetNode<Node2D>("Flag");
		Flag.Show();
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

using Godot;
using System;

public class CityMarker : AnimatedSprite
{

	[Export] public string CityName;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void LeftClick()
	{
		GD.Print(CityName);
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

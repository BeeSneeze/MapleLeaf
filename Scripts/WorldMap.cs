using Godot;
using System;
using System.Collections.Generic;

public class WorldMap : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	List<CityMarker> CityList;
	int CurrentIndex = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CityList = new List<CityMarker>();
		int index = 0;
		for(int i = 1; i < 12; i++)
		{
			if(i > 2)
			{
				Node2D CChild = (Node2D)GetChild(i);
				CChild.Hide();
			}
			CityList.Add((CityMarker)GetChild(i));
		}

		CityList[0].DeactivateCity();
	}

	public void NextCity()
	{
		CityList[CurrentIndex++].DeactivateCity();
		CityList[CurrentIndex].ActivateCity();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

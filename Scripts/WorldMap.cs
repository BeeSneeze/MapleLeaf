using Godot;
using System;
using System.Collections.Generic;

public class WorldMap : Node2D
{
	public int CurrentIndex = 1;
	public bool FinalCity = false;
	
	private List<CityMarker> CityList;
	private GameManager GM;
	private CardShop ShopA, ShopB;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GM = GetParent().GetNode<GameManager>("Game");
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

		ShopA = GetNode<CardShop>("ShopA");
		ShopB = GetNode<CardShop>("ShopB");

		ShopA.OtherShop = ShopB;
		ShopB.OtherShop = ShopA;

		CheckBox CBoxA = GetNode("ShopA").GetNode<CheckBox>("CheckBox");
		CBoxA.Pressed = true;
	}

	public void NextCity()
	{
		if(!FinalCity)
		{
			CityList[CurrentIndex++].DeactivateCity();
			CityList[CurrentIndex].ActivateCity();
			FinalCity = CurrentIndex == 10;

			if(FinalCity)
			{
				GM.LoadBossFight();
			}
		}
	}

	public void StartLevel()
	{
		LevelManager LM = GetParent<LevelManager>();

		CheckBox CBoxA = GetNode("ShopA").GetNode<CheckBox>("CheckBox");

		Node2D PickOne = GetNode<Node2D>("PickOne");
		PickOne.Hide();


		if(CBoxA.Pressed)
		{
			ShopA.Ratify();
		}
		else
		{
			ShopB.Ratify();
		}

		ShopA.ResetShop();
		ShopB.ResetShop();

		GM.Board.LoadStage(CityList[CurrentIndex].CityName);

		GM.LevelStart();
		LM.ChangeLevel("Game");
	}
}

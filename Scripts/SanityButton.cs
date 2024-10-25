using Godot;
using System;

public class SanityButton : Button
{

	[Export] public bool Yes;
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	private void _on_Button_pressed()
	{
		if(Yes)
		{
			GameManager GM = GetParent().GetParent<GameManager>();
			GM.EndTurnButton.Hide();
			GM.EndDrawButton.Show();
			GM.SetMode("RatAttack");
			Node2D Sanity = GetParent<Node2D>();
			Sanity.Hide();
		}
		else
		{
			Node2D Sanity = GetParent<Node2D>();
			Sanity.Hide();
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}




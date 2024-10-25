using Godot;
using System;

public class SanityButton : Button
{

	[Export] public bool Yes;

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
}




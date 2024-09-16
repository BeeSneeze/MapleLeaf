using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Story : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private AnimatedSprite Image;
	private Label BottomText;
	private Dictionary<string, StorySegment> StoryDict;

	private string ActiveStory;
	private int CurrentSlide;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// Load AllCardsDict
		File Reader = new File();
		Reader.Open("res://Assets/Story.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		StoryDict = JsonConvert.DeserializeObject<Dictionary<string,StorySegment>>(Contents);
		Reader.Close();


		Image = (AnimatedSprite)GetNode("Slides");
		BottomText = (Label)GetNode("Label");

		StartStory("Start");
	}

	public void StartStory(string StoryName)
	{
		ActiveStory = StoryName;
		CurrentSlide = 0;
		GD.Print("ATTEMPTED TO START STORY: " + StoryName);
		AdvanceStory();
	}

	public void AdvanceStory()
	{
		if(StoryDict[ActiveStory].Slides.Count > CurrentSlide)
		{
			Image.Animation = StoryDict[ActiveStory].Slides[CurrentSlide];
			BottomText.Text = StoryDict[ActiveStory].Text[CurrentSlide];
			CurrentSlide++;
		}
		else
		{
			switch(ActiveStory)
			{
				case "Start":
					LevelManager LM = (LevelManager)GetParent();
					LM.ChangeLevel("MainMenu");
				break;
			}
		}
	}

	public void LeftClick()
	{
		AdvanceStory();
	}
	public void RightClick()
	{
		
	}
	

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

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

	private string CurrentText;
	private int CurrentChar;

	private const float CharTime = 0.03f;
	private float CurrTime = 0.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// Load AllCardsDict
		File Reader = new File();
		Reader.Open("res://Assets/Story.JSON", File.ModeFlags.Read);
		string Contents = Reader.GetAsText();
		StoryDict = JsonConvert.DeserializeObject<Dictionary<string,StorySegment>>(Contents);
		Reader.Close();

		CurrentChar = 10000000;

		Image = GetNode<AnimatedSprite>("Slides");
		BottomText = GetNode<Label>("Label");

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
			BottomText.Text = "";
			Image.Animation = StoryDict[ActiveStory].Slides[CurrentSlide];
			CurrentText = StoryDict[ActiveStory].Text[CurrentSlide];
			CurrentChar = 0;
			CurrentSlide++;
			AdvanceText();
		}
		else
		{
			LevelManager LM = (LevelManager)GetParent();
			switch(ActiveStory)
			{
				case "Start":
					AudioStreamPlayer SMusic = GetNode<AudioStreamPlayer>("StartMusic");
					SMusic.Stop();
					MainMenu MM = LM.GetNode<MainMenu>("MainMenu");
					MM.StartCountDown();
					LM.ChangeLevel("MainMenu");

				break;
				case "Ending":
					LM.ChangeLevel("MainMenu");
				break;
			}
		}
	}

	public void AdvanceText()
	{
		BottomText.Text += CurrentText[CurrentChar];
	}

	public void LeftClick()
	{
		AdvanceStory();
	}

	public override void _Process(float delta)
	{
		CurrTime += delta;

		if(CurrTime > CharTime)
		{
			CurrTime = 0;
			if(++CurrentChar < CurrentText.Length)
			{
				AdvanceText();
			}
		}
		
		
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

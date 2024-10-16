using Godot;
using System;
using System.Collections.Generic;


// Contains all the information needed about a specific card
public struct CardType
{
	public string ID;		// ID end for the card
	public string MatrixName;	// What shape of matrix should be loaded
	public string Range; 		// Stored as a string for the sake of the JSON interpreter
	public string Uses; 		// How many times can this card be used before being discarded?
	public string Draws; 		// How many times can this card be drawn before getting exhausted?
	public string TargetType; 	// Single target, Area effects or a specific amount (2, 3, 4 etc.)
	public string TargetCell; 	// Empty, occupied, friendly, enemy etc.
	public string FlavorText; 	// Funny text
	public string AbilityText;  // Useful text
	public List<Ability> AbilityList; 	// List of abilities that target cells
	public List<Ability> SecondaryList; // List of abilities that work independently
};

public struct StorySegment
{
	public List<string> Slides;
	public List<string> Text;
}

// Used to store a unique card in the card manager
public struct CompactCard
{
	public int ID;		// ID for the card
	public int OwnerID; 	// Who owns this card?
	public int DrawCount; 	// How many times can this card be drawn before it is exhausted?
};

// A specific ability, stored in a card, to be triggered by the game manager and board
public struct Ability
{
	public string Name;
	public string Effect;
}

// Information about a specific character. Each tile has one.
public struct Character
{
	public int HP, MaxHP;				// How much damage can this character take before it dies?
	public int ID; 						// % 100 on the ID to get the character type. For instance 101 % 100 = 1, which is the soldier
	public string Name; 				// The name associated with a given ID. Only used for labeling purposes
	public Vector2 QueuedMove; 			// Where is this character about to move?
	public List<Modifier> ModifierData;	// Contains a list of modifiers and how long they last

	public void AddModifier(string MName, int MTime = 10000)
	{
		Modifier M = new Modifier();
		M.Name = MName;
		M.Time = MTime;
		ModifierData.Add(M);
	}

	public void AdvanceModifiers()
	{
		List<int> DoneMods = new List<int>();
		for(int i = 0; i < ModifierData.Count; i++)
		{
			Modifier M = ModifierData[i];

			M.Time = ModifierData[i].Time - 1;

			ModifierData[i] = M;
			if(ModifierData[i].Time < 1)
			{
				DoneMods.Add(i);
			}
		}

		DoneMods.Reverse(); // Check in reverse order, since elements are being removed
		foreach(int index in DoneMods)
		{
			ModifierData.RemoveAt(index);
		}
	}


	// Advances time for a specific modifier
	public void AdvanceModifier(string ModName)
	{
		if(!ContainsModifier(ModName))
		{
			return;
		}

		List<int> DoneMods = new List<int>();
		
		for(int i = 0; i < ModifierData.Count; i++)
		{
			Modifier M = ModifierData[i];

			if(M.Name != ModName)
			{
				continue;
			}

			M.Time = ModifierData[i].Time - 1;

			ModifierData[i] = M;
			
			if(ModifierData[i].Time < 1)
			{
				DoneMods.Add(i);
			}
		}

		DoneMods.Reverse(); // Check in reverse order, since elements are being removed
		foreach(int index in DoneMods)
		{
			ModifierData.RemoveAt(index);
		}
	}

	public bool ContainsModifier(string InString)
	{
		foreach(Modifier M in ModifierData)
		{
			if(M.Name == InString)
			{
				return true;
			}
		}
		return false;
	}
}

public struct Modifier
{
	public string Name;
	public int Time;
}

// Character info as saved in the JSON files
public struct CharacterInfo
{
	public string ID;
	public string MaxHP;
	public List<string> Names;
}

public struct Arrow
{
	public Vector2 From;
	public Vector2 To;
}

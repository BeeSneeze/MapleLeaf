using Godot;
using System;
using System.Collections.Generic;


// Contains all the information needed about a specific card
public struct CardType
{
	public string CardNum;		// ID end for the card
	public string MatrixName;	// What shape of matrix should be loaded
	public string Range; 		// Stored as a string for the sake of the JSON interpreter
	public string Uses; 		// How many times can this card be used before being discarded?
	public string TargetType; 	// Single target, Area effects or a specific amount (2, 3, 4 etc.)
	public string TargetCell; 	// Empty, occupied, friendly, enemy etc.
	public string FlavorText; 	// Funny text
	public string AbilityText;  // Useful text
	public List<Ability> AbilityList; 	// List of abilities that target cells
	public List<Ability> SecondaryList; // List of abilities that work independently
};

// A specific ability, stored in a card, to be triggered by the game manager and board
public struct Ability
{
	public string Name;
	public string Effect;
}

// Information about a specific character. Each tile has one. (HAVE EMPTY CHARACTERS OR NOT?)
public struct Character
{
	public int HP, MaxHP;
	public int ID; // % 100 on the ID to get the character type. For instance 101 % 100 = 1, which is the soldier
	public string Name; // The name associated with a given ID. Only used for labeling purposes
	public Vector2 QueuedMove; // Where is this character about to move?
}

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
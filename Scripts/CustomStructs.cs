using Godot;
using System;
using System.Collections.Generic;


// Contains all the information needed about a specific card
public struct CardType
{
	public string CardNum;
	public string MatrixName;
	public string Range; // Stored as a string for the sake of the JSON interpreter
	public string TargetType;
	public string AbilityText;
	public string FlavorText;
	public List<Ability> AbilityList;
	public List<Ability> SecondaryList;
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

}

public struct CharacterInfo
{
	public string ID;
	public string MaxHP;
	public List<string> Names;
}
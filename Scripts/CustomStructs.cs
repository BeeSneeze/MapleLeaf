using Godot;
using System;
using System.Collections.Generic;


// Contains all the information needed about a specific card
public struct CardType{
	public string MatrixName;
	public string Range; // Stored as a string for the sake of the JSON interpreter
	public string TargetType;
	public string AbilityText;
	public string FlavorText;
	public List<Ability> AbilityList;
	public List<Ability> SecondaryList;
};


// A specific ability, stored in a card, to be triggered by the game manager and board
public struct Ability{
	public string Name;
	public string Effect;
}
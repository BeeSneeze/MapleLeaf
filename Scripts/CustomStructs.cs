using Godot;
using System;
using System.Collections.Generic;

public struct CardType{
	public string MatrixName;
	public string Range;
	public string TargetType;
	public string AbilityText;
	public string FlavorText;
	public List<Ability> AbilityList;
	public List<Ability> SecondaryList;
};

public struct Ability{
	public string Name;
	public string Effect;
}
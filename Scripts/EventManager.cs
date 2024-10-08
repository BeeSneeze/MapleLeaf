using Godot;
using System;

public class EventManager : Node
{
    [Signal] public delegate void LevelChange(string LevelName); // Change between levels/menus
    [Signal] public delegate void BackTrack(); // Change back to whichever menu you last visited

    public static EventManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
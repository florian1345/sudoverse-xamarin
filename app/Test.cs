using Godot;
using System;

public class Test : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var engine = GetNode("Engine");
		var label = GetNode("CenterContainer").GetNode("VBoxContainer").GetNode<Label>("Label");
		label.Text = engine.Call("test").ToString();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

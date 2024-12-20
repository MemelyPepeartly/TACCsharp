using Godot;
using System;

public partial class Stem : Node
{
	public override void _Ready()
	{
		GD.Print("Stem is ready!");

		AddLeaf("res://Leaves/CutsceneLeaf.tscn");
	}

	public void AddLeaf(string leafPath)
	{
		// Load a leaf (modular component)
		PackedScene leafScene = GD.Load<PackedScene>(leafPath);
		if (leafScene != null)
		{
			Node leafInstance = leafScene.Instantiate();
			AddChild(leafInstance);
			GD.Print($"Leaf {leafInstance.Name} added to Stem.");
		}
		else
		{
			GD.PrintErr($"Failed to load leaf: {leafPath}");
		}
	}
}

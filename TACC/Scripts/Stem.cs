using Godot;
using System;

public partial class Stem : Node
{
	public override void _Ready()
	{
		GD.Print("Stem is ready!");

		AddLeaf("res://TACC/Leaves/CutsceneLeaf.tscn");
		AddLeaf("res://TACC/Leaves/MapLeaf.tscn");
		AddLeafAsUI("res://TACC/Leaves/MenuFactoryLeaf.tscn");
	}

	public void AddLeaf(string leafPath)
	{
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

	// Method to add UI leaves inside a CanvasLayer
	  public void AddLeafAsUI(string leafPath)
	{
		PackedScene leafScene = GD.Load<PackedScene>(leafPath);
		if (leafScene != null)
		{
			Node leafInstance = leafScene.Instantiate();
			leafInstance.Name = "MenuFactoryLeaf";

			var canvasLayer = new CanvasLayer
			{
				Name = "CanvasLayer",
				Layer = 1
			};

			canvasLayer.AddChild(leafInstance);
			AddChild(canvasLayer);

			GD.Print($"UI Leaf {leafInstance.Name} added to Stem inside CanvasLayer.");
		}
		else
		{
			GD.PrintErr($"Failed to load leaf: {leafPath}");
		}
	}
}

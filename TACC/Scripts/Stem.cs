using Godot;
using System;

public partial class Stem : Node
{
	public override void _Ready()
	{
		GD.Print("Stem is ready!");

		AddLeaf("res://TACC/Leaves/CutsceneLeaf.tscn");
		AddLeaf("res://TACC/Leaves/MapLeaf.tscn");
		AddLeafAsUI("res://TACC/Leaves/HudOverlayLeaf.tscn");
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
	public void AddLeafAsUI(string leafPath, string canvasLayerName = "CanvasLayer", int layer = 1)
	{
		PackedScene leafScene = GD.Load<PackedScene>(leafPath);
		if (leafScene != null)
		{
			Node leafInstance = leafScene.Instantiate();
			var canvasLayer = GetNodeOrNull<CanvasLayer>(canvasLayerName);
			if (canvasLayer == null)
			{
				canvasLayer = new CanvasLayer
				{
					Name = canvasLayerName,
					Layer = layer
				};
				AddChild(canvasLayer);
			}

			canvasLayer.AddChild(leafInstance);

			GD.Print($"UI Leaf {leafInstance.Name} added to Stem inside {canvasLayer.Name}.");
		}
		else
		{
			GD.PrintErr($"Failed to load leaf: {leafPath}");
		}
	}
}

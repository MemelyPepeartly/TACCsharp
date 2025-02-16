using Godot;
using System;

public partial class MenuDemo : Node
{
	private Stem _stem;
	private MenuFactoryLeaf _menuFactory;

	public override void _Ready()
	{
		GD.Print("Initializing MenuDemo...");

		// Retrieve the Stem node
		_stem = GetNodeOrNull<Stem>("Stem");

		if (_stem == null)
		{
			GD.PrintErr("ERROR: Stem not found.");
			return;
		}

		// Connect and retrieve the MenuFactoryLeaf
		ConnectMenuLeaf();
	}

	private void ConnectMenuLeaf()
	{
		if (!IsInstanceValid(_stem))
		{
			GD.PrintErr("ERROR: Stem is invalid.");
			return;
		}

		// Retrieve MenuFactoryLeaf
		_menuFactory = _stem.GetNodeOrNull<MenuFactoryLeaf>("CanvasLayer/MenuFactoryLeaf");

		if (_menuFactory == null)
		{
			GD.PrintErr("ERROR: MenuFactoryLeaf not found in Stem.");
			return;
		}

		// Load the menu
		GD.Print("Loading menu from JSON...");
		_menuFactory.LoadMenu("res://Demos/Data/Menus/Start.json");

		// Register actions
		_menuFactory.RegisterAction("start_map_demo", StartMapDemo);
		_menuFactory.RegisterAction("start_cutscene_demo", StartCutsceneDemo);
		_menuFactory.RegisterAction("exit_game", ExitGame);
	}

	private void StartMapDemo()
	{
		GD.Print("Map Demo started.");
	}

	private void StartCutsceneDemo()
	{
		GD.Print("Cutscene Demo started.");
	}

	private void ExitGame()
	{
		GD.Print("Exiting game...");
		GetTree().Quit();
	}
}

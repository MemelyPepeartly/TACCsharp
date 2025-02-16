using Godot;
using System;
using TACCsharp.Demos.Menu_Demo.Scripts;

public partial class MenuDemo : Node
{
	private Stem _stem;
	private MenuFactoryLeaf _menuFactory;
	private MapHelper _mapHelper;
	private CutsceneHelper _cutsceneHelper;

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

		// Retrieve and configure the MenuFactoryLeaf
		ConfigureMenu();
	}

	private void ConfigureMenu()
	{
		_menuFactory = _stem.GetNodeOrNull<MenuFactoryLeaf>("CanvasLayer/MenuFactoryLeaf");

		if (_menuFactory == null)
		{
			GD.PrintErr("ERROR: MenuFactoryLeaf not found in Stem.");
			return;
		}

		// Load the menu
		_menuFactory.LoadMenu("res://Demos/Data/Menus/Start.json");

		// Register actions
		_menuFactory.RegisterAction("start_map_demo", StartMapDemo);
		_menuFactory.RegisterAction("start_cutscene_demo", StartCutsceneDemo);
		_menuFactory.RegisterAction("exit_game", ExitGame);
	}

	private void StartMapDemo()
	{
		GD.Print("Initializing Map Demo...");

		// Hide the menu
		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		// Ensure only one instance of MapHelper is created
		if (_mapHelper == null)
		{
			_mapHelper = new MapHelper(_stem);
			AddChild(_mapHelper);
		}
	}

	private void StartCutsceneDemo()
	{
		GD.Print("Initializing Cutscene Demo...");

		// Hide the menu
		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		// Ensure only one instance of CutsceneHelper is created
		if (_cutsceneHelper == null)
		{
			_cutsceneHelper = new CutsceneHelper(_stem);
			AddChild(_cutsceneHelper);
		}
	}

	private void ExitGame()
	{
		GD.Print("Exiting game...");
		GetTree().Quit();
	}

	public override void _Input(InputEvent @event)
	{
		// Check if the Escape key is pressed
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			ToggleMenu();
		}
	}

	private void ToggleMenu()
	{
		if (_menuFactory != null)
		{
			_menuFactory.Visible = !_menuFactory.Visible;
		}
	}
}

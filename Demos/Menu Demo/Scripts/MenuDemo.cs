using Godot;
using System;
using TACCsharp.Demos.Menu_Demo.Scripts;

public partial class MenuDemo : Node
{
	private const string MainMenuPath = "res://Demos/Data/Menus/Start.json";
	private const string CutsceneMenuPath = "res://Demos/Data/Menus/CutsceneSelect.json";
	private const string CutsceneProloguePath = "res://Demos/Data/Cutscenes/Prologue.json";
	private const string CutsceneInterludePath = "res://Demos/Data/Cutscenes/Interlude.json";
	private const string CutsceneFinalePath = "res://Demos/Data/Cutscenes/Finale.json";

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
		ConfigureMenuFactory();
		ShowMainMenu();
	}

	private void ConfigureMenuFactory()
	{
		_menuFactory = _stem.GetNodeOrNull<MenuFactoryLeaf>("CanvasLayer/MenuFactoryLeaf");

		if (_menuFactory == null)
		{
			GD.PrintErr("ERROR: MenuFactoryLeaf not found in Stem.");
			return;
		}
	}

	private void ShowMainMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		_menuFactory.LoadMenu(MainMenuPath);
		_menuFactory.RegisterAction("start_map_demo", StartMapDemo);
		_menuFactory.RegisterAction("start_cutscene_demo", ShowCutsceneMenu);
		_menuFactory.RegisterAction("exit_game", ExitGame);
		_menuFactory.Visible = true;
	}

	private void ShowCutsceneMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		_menuFactory.LoadMenu(CutsceneMenuPath);
		_menuFactory.RegisterAction("start_cutscene_prologue", () => StartCutscene(CutsceneProloguePath));
		_menuFactory.RegisterAction("start_cutscene_interlude", () => StartCutscene(CutsceneInterludePath));
		_menuFactory.RegisterAction("start_cutscene_finale", () => StartCutscene(CutsceneFinalePath));
		_menuFactory.RegisterAction("back_to_main_menu", ShowMainMenu);
		_menuFactory.Visible = true;
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

		_mapHelper.SetMapActive(true);
	}

	private void StartCutscene(string cutscenePath)
	{
		GD.Print("Initializing Cutscene Demo...");

		// Hide the menu
		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		_mapHelper?.SetMapActive(false);

		// Ensure only one instance of CutsceneHelper is created
		if (_cutsceneHelper == null)
		{
			_cutsceneHelper = new CutsceneHelper(_stem);
			AddChild(_cutsceneHelper);
		}

		_cutsceneHelper.StartCutscene(cutscenePath);
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

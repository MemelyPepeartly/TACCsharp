using Godot;
using System;

public partial class MenuDemo : Node
{
	private Stem _stem;
	private MenuFactoryLeaf _menuFactory;

	public override void _Ready()
	{
		// Create and add the Stem node
		_stem = new Stem();
		AddChild(_stem);

		// Create and add the MenuFactoryLeaf to the Stem
		_menuFactory = new MenuFactoryLeaf
		{
			JsonPath = "res://Demos/Data/Menus/Start.json"
		};
		_stem.AddChild(_menuFactory);

		// Register actions
		_menuFactory.RegisterAction("start_game", StartGame);
		_menuFactory.RegisterAction("open_options", OpenOptions);
		_menuFactory.RegisterAction("exit_game", ExitGame);
	}

	private void StartGame()
	{
		GD.Print("Starting game...");
		// Implement game start logic
	}

	private void OpenOptions()
	{
		GD.Print("Opening options...");
		// Implement options menu logic
	}

	private void ExitGame()
	{
		GetTree().Quit();
	}
}

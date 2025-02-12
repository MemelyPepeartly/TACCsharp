using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public partial class MenuFactoryLeaf : Node2D
{
	[Export] public string JsonPath { get; set; }

	private Dictionary<string, Action> actions = new Dictionary<string, Action>();

	public override void _Ready()
	{
		if (!string.IsNullOrEmpty(JsonPath))
		{
			LoadMenu(JsonPath);
		}
	}

	public void LoadMenu(string path)
	{
		// Check if the file exists
		if (!FileAccess.FileExists(path))
		{
			GD.PrintErr($"Menu JSON file not found: {path}");
			return;
		}

		try
		{
			// Read and deserialize the JSON
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			string jsonContent = file.GetAsText();

			var menuData = JsonConvert.DeserializeObject<MenuData>(jsonContent);

			// Build the menu
			BuildMenu(menuData);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to load menu: {ex.Message}");
		}
	}

	private void BuildMenu(MenuData menuData)
	{
		// Create a vertical container for buttons
		var vbox = new VBoxContainer
		{
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			AnchorTop = 0.5f,
			AnchorBottom = 0.5f,
			PivotOffset = new Vector2(0.5f, 0.5f),
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
		};
		AddChild(vbox);

		foreach (var buttonData in menuData.Buttons)
		{
			var button = new Button
			{
				Text = buttonData.Text,
				SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill
			};

			button.FocusMode = Control.FocusModeEnum.All;

			button.Pressed += () => OnButtonPressed(buttonData.Action);

			vbox.AddChild(button);
		}
	}

	private void OnButtonPressed(string actionName)
	{
		if (actions.TryGetValue(actionName, out var action))
		{
			action?.Invoke();
		}
		else
		{
			GD.PrintErr($"No action found for: {actionName}");
		}
	}

	// Method to register actions
	public void RegisterAction(string actionName, Action action)
	{
		actions[actionName] = action;
	}
}

// Data classes for deserialization
public class MenuData
{
	[JsonProperty("buttons")]
	public List<ButtonData> Buttons { get; set; }
}

public class ButtonData
{
	[JsonProperty("text")]
	public string Text { get; set; }
	[JsonProperty("action")]
	public string Action { get; set; }
}

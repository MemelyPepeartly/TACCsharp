using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public partial class MenuFactoryLeaf : Control
{
	private VBoxContainer _vbox;
	private Dictionary<string, Action> _actions = new Dictionary<string, Action>();

	public override void _Ready()
	{
		_vbox = GetNodeOrNull<VBoxContainer>("CenterContainer/VBoxContainer");

		if (_vbox == null)
		{
			GD.PrintErr("ERROR: VBoxContainer not found.");
			return;
		}

		// Ensure VBoxContainer expands properly
		_vbox.SizeFlagsVertical = SizeFlags.ExpandFill;
		_vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		_vbox.Visible = true;
	}

	// Load and build the menu dynamically from JSON
	public void LoadMenu(string jsonPath)
	{
		if (!FileAccess.FileExists(jsonPath))
		{
			GD.PrintErr($"ERROR: Menu JSON file not found: {jsonPath}");
			return;
		}

		try
		{
			using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
			string jsonContent = file.GetAsText();

			var menuData = JsonConvert.DeserializeObject<MenuData>(jsonContent);

			if (menuData?.Buttons == null || menuData.Buttons.Count == 0)
			{
				GD.PrintErr("ERROR: No valid buttons found in JSON.");
				return;
			}

			BuildMenu(menuData);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"ERROR: Failed to load menu: {ex.Message}");
		}
	}

	private void BuildMenu(MenuData menuData)
	{
		if (_vbox == null)
		{
			GD.PrintErr("ERROR: VBoxContainer is NULL, cannot add buttons.");
			return;
		}

		// Clear existing buttons before adding new ones
		foreach (Node child in _vbox.GetChildren())
		{
			child.QueueFree();
		}

		foreach (var buttonData in menuData.Buttons)
		{
			var button = new Button
			{
				Text = buttonData.Text,
				CustomMinimumSize = new Vector2(200, 50),
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};

			button.Pressed += () => OnButtonPressed(buttonData.Action);
			_vbox.AddChild(button);
		}
	}

	private void OnButtonPressed(string actionName)
	{
		if (_actions.TryGetValue(actionName, out var action))
		{
			action?.Invoke();
		}
		else
		{
			GD.PrintErr($"ERROR: No action found for '{actionName}'.");
		}
	}

	public void RegisterAction(string actionName, Action action)
	{
		_actions[actionName] = action;
	}
}

// Data Classes
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

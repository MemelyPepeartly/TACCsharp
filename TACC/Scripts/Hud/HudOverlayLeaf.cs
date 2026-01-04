using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TACCsharp.TACC.Models;
using FileAccess = Godot.FileAccess;

public partial class HudOverlayLeaf : Control
{
	[Export] public string JsonPath { get; set; }

	private const int DefaultMargin = 12;
	private const int DefaultSeparation = 6;

	private readonly Dictionary<string, Control> _elements = new Dictionary<string, Control>();
	private readonly Dictionary<string, Label> _labels = new Dictionary<string, Label>();
	private readonly Dictionary<string, ProgressBar> _progressBars = new Dictionary<string, ProgressBar>();
	private readonly Dictionary<string, TextureRect> _icons = new Dictionary<string, TextureRect>();
	private readonly Dictionary<string, VBoxContainer> _anchorSlots = new Dictionary<string, VBoxContainer>();

	public override void _Ready()
	{
		MouseFilter = Control.MouseFilterEnum.Ignore;
		EnsureLayout();

		if (!string.IsNullOrWhiteSpace(JsonPath))
		{
			LoadHud(JsonPath);
		}
	}

	public void LoadHud(string jsonPath)
	{
		if (!FileAccess.FileExists(jsonPath))
		{
			GD.PrintErr($"HUD JSON file not found: {jsonPath}");
			return;
		}

		try
		{
			using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
			string jsonContent = file.GetAsText();

			var hudData = JsonConvert.DeserializeObject<HudData>(jsonContent);

			if (hudData?.Elements == null || hudData.Elements.Count == 0)
			{
				GD.PrintErr("ERROR: No valid HUD elements found in JSON.");
				return;
			}

			BuildHud(hudData);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"ERROR: Failed to load HUD: {ex.Message}");
		}
	}

	public void ClearHud()
	{
		foreach (var element in _elements.Values)
		{
			element.GetParent()?.RemoveChild(element);
			element.QueueFree();
		}

		_elements.Clear();
		_labels.Clear();
		_progressBars.Clear();
		_icons.Clear();
	}

	public void SetText(string elementId, string text)
	{
		if (_labels.TryGetValue(elementId, out var label))
		{
			label.Text = text ?? string.Empty;
		}
		else
		{
			GD.PrintErr($"HUD element '{elementId}' is not a label.");
		}
	}

	public void SetValue(string elementId, double value)
	{
		if (_progressBars.TryGetValue(elementId, out var bar))
		{
			bar.Value = value;
		}
		else
		{
			GD.PrintErr($"HUD element '{elementId}' is not a progress bar.");
		}
	}

	public void SetIcon(string elementId, string texturePath)
	{
		if (!_icons.TryGetValue(elementId, out var icon))
		{
			GD.PrintErr($"HUD element '{elementId}' is not an icon.");
			return;
		}

		var texture = LoadTexture(texturePath);
		if (texture != null)
		{
			icon.Texture = texture;
		}
	}

	public void SetVisible(string elementId, bool visible)
	{
		if (_elements.TryGetValue(elementId, out var element))
		{
			element.Visible = visible;
		}
		else
		{
			GD.PrintErr($"HUD element '{elementId}' not found.");
		}
	}

	private void BuildHud(HudData hudData)
	{
		EnsureLayout();
		ClearHud();

		foreach (var elementData in hudData.Elements)
		{
			if (elementData == null || string.IsNullOrWhiteSpace(elementData.Id))
			{
				GD.PrintErr("HUD element is missing an id.");
				continue;
			}

			if (_elements.ContainsKey(elementData.Id))
			{
				GD.PrintErr($"Duplicate HUD element id '{elementData.Id}'.");
				continue;
			}

			bool usedDefaultAnchor = false;
			var slot = GetAnchorSlot(elementData.Anchor, out usedDefaultAnchor);
			if (usedDefaultAnchor && !string.IsNullOrWhiteSpace(elementData.Anchor))
			{
				GD.PrintErr($"HUD element '{elementData.Id}' has invalid anchor '{elementData.Anchor}', defaulting to top_left.");
			}

			var control = CreateElementControl(elementData);
			if (control == null)
			{
				continue;
			}

			control.Name = elementData.Id;
			control.Visible = elementData.Visible ?? true;
			ApplyMinimumSize(control, elementData);

			slot.AddChild(control);
			_elements[elementData.Id] = control;
		}
	}

	private Control CreateElementControl(HudElementData elementData)
	{
		string type = NormalizeType(elementData.Type);

		switch (type)
		{
			case "label":
				var label = new Label { Text = elementData.Text ?? string.Empty };
				label.MouseFilter = Control.MouseFilterEnum.Ignore;
				_labels[elementData.Id] = label;
				return label;
			case "icon":
				var icon = new TextureRect
				{
					Texture = LoadTexture(elementData.IconPath),
					ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
				};
				icon.MouseFilter = Control.MouseFilterEnum.Ignore;
				_icons[elementData.Id] = icon;
				return icon;
			case "progress":
				var container = new HBoxContainer();
				container.MouseFilter = Control.MouseFilterEnum.Ignore;
				container.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				if (!string.IsNullOrEmpty(elementData.Text))
				{
					var progressLabel = new Label { Text = elementData.Text };
					progressLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
					container.AddChild(progressLabel);
					_labels[elementData.Id] = progressLabel;
				}

				var bar = new ProgressBar
				{
					MinValue = elementData.Min ?? 0,
					MaxValue = elementData.Max ?? 100,
					Value = elementData.Value ?? (elementData.Min ?? 0)
				};
				bar.MouseFilter = Control.MouseFilterEnum.Ignore;
				bar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				container.AddChild(bar);
				_progressBars[elementData.Id] = bar;
				return container;
			default:
				GD.PrintErr($"Unknown HUD element type '{elementData.Type}' for '{elementData.Id}'.");
				return null;
		}
	}

	private void ApplyMinimumSize(Control control, HudElementData elementData)
	{
		float width = elementData.MinWidth ?? 0;
		float height = elementData.MinHeight ?? 0;

		if (width > 0 || height > 0)
		{
			control.CustomMinimumSize = new Vector2(Math.Max(width, 0), Math.Max(height, 0));
		}
	}

	private Texture2D LoadTexture(string texturePath)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			return null;
		}

		var texture = GD.Load<Texture2D>(texturePath);
		if (texture == null)
		{
			GD.PrintErr($"HUD texture not found: {texturePath}");
		}

		return texture;
	}

	private VBoxContainer GetAnchorSlot(string anchor, out bool usedDefault)
	{
		EnsureLayout();

		string normalized = NormalizeAnchor(anchor);
		if (normalized != null && _anchorSlots.TryGetValue(normalized, out var slot))
		{
			usedDefault = false;
			return slot;
		}

		usedDefault = true;
		return _anchorSlots["top_left"];
	}

	private string NormalizeAnchor(string anchor)
	{
		if (string.IsNullOrWhiteSpace(anchor))
		{
			return "top_left";
		}

		string normalized = anchor.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
		return _anchorSlots.ContainsKey(normalized) ? normalized : null;
	}

	private string NormalizeType(string type)
	{
		return string.IsNullOrWhiteSpace(type) ? "label" : type.Trim().ToLowerInvariant();
	}

	private void EnsureLayout()
	{
		if (_anchorSlots.Count > 0)
		{
			return;
		}

		var margins = GetNodeOrNull<MarginContainer>("Margins");
		if (margins == null)
		{
			margins = new MarginContainer
			{
				Name = "Margins",
				AnchorRight = 1,
				AnchorBottom = 1
			};
			margins.MouseFilter = Control.MouseFilterEnum.Ignore;
			margins.AddThemeConstantOverride("margin_left", DefaultMargin);
			margins.AddThemeConstantOverride("margin_top", DefaultMargin);
			margins.AddThemeConstantOverride("margin_right", DefaultMargin);
			margins.AddThemeConstantOverride("margin_bottom", DefaultMargin);
			AddChild(margins);
		}
		else
		{
			margins.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		var rows = margins.GetNodeOrNull<VBoxContainer>("Rows");
		if (rows == null)
		{
			rows = new VBoxContainer
			{
				Name = "Rows",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			rows.MouseFilter = Control.MouseFilterEnum.Ignore;
			margins.AddChild(rows);
		}
		else
		{
			rows.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		var topRow = rows.GetNodeOrNull<HBoxContainer>("TopRow");
		if (topRow == null)
		{
			topRow = CreateRow("TopRow");
			rows.AddChild(topRow);
		}
		else
		{
			topRow.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		var spacer = rows.GetNodeOrNull<Control>("Spacer");
		if (spacer == null)
		{
			spacer = new Control
			{
				Name = "Spacer",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			spacer.MouseFilter = Control.MouseFilterEnum.Ignore;
			rows.AddChild(spacer);
		}
		else
		{
			spacer.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		var bottomRow = rows.GetNodeOrNull<HBoxContainer>("BottomRow");
		if (bottomRow == null)
		{
			bottomRow = CreateRow("BottomRow");
			rows.AddChild(bottomRow);
		}
		else
		{
			bottomRow.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		RegisterSlot(topRow, "TopLeft", "top_left", BoxContainer.AlignMode.Begin);
		RegisterSlot(topRow, "TopCenter", "top_center", BoxContainer.AlignMode.Center);
		RegisterSlot(topRow, "TopRight", "top_right", BoxContainer.AlignMode.End);

		RegisterSlot(bottomRow, "BottomLeft", "bottom_left", BoxContainer.AlignMode.Begin);
		RegisterSlot(bottomRow, "BottomCenter", "bottom_center", BoxContainer.AlignMode.Center);
		RegisterSlot(bottomRow, "BottomRight", "bottom_right", BoxContainer.AlignMode.End);
	}

	private HBoxContainer CreateRow(string name)
	{
		return new HBoxContainer
		{
			Name = name,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.Fill,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
	}

	private void RegisterSlot(HBoxContainer row, string name, string key, BoxContainer.AlignMode align)
	{
		var slot = row.GetNodeOrNull<VBoxContainer>(name);
		if (slot == null)
		{
			slot = new VBoxContainer
			{
				Name = name,
				Alignment = align,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.Fill,
				Separation = DefaultSeparation,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			row.AddChild(slot);
		}
		else
		{
			slot.Alignment = align;
			slot.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		_anchorSlots[key] = slot;
	}
}

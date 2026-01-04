using Godot;
using System;
using System.Text;
using TACCsharp.TACC.State;

namespace TACCsharp.Demos.Menu_Demo.Scripts
{
public partial class StateMonitorVisualizerPanel : PanelContainer
{
	private const float PanelWidth = 420f;
	private const float PanelHeight = 520f;
	private const float PanelMargin = 16f;

	private TextEdit _report;
	private Label _status;
	private StateMonitorLeaf _monitor;

	public override void _Ready()
	{
		BuildLayout();
		ApplyDefaultLayout();
		RefreshReport();
	}

	public override void _ExitTree()
	{
		DetachMonitor();
	}

	public void AttachMonitor(StateMonitorLeaf monitor)
	{
		if (_monitor == monitor)
		{
			return;
		}

		DetachMonitor();
		_monitor = monitor;

		if (_monitor != null)
		{
			_monitor.StateUpdated += OnStateUpdated;
		}

		RefreshReport();
	}

	private void DetachMonitor()
	{
		if (_monitor != null)
		{
			_monitor.StateUpdated -= OnStateUpdated;
			_monitor = null;
		}
	}

	private void BuildLayout()
	{
		if (GetNodeOrNull<VBoxContainer>("Layout") != null)
		{
			_report = GetNodeOrNull<TextEdit>("Layout/Report");
			_status = GetNodeOrNull<Label>("Layout/Header/Status");
			return;
		}

		var layout = new VBoxContainer
		{
			Name = "Layout",
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		AddChild(layout);

		var header = new HBoxContainer
		{
			Name = "Header",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		layout.AddChild(header);

		var title = new Label
		{
			Text = "State Monitor Visualizer",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		header.AddChild(title);

		_status = new Label
		{
			Name = "Status",
			HorizontalAlignment = HorizontalAlignment.Right,
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		header.AddChild(_status);

		var closeButton = new Button
		{
			Text = "Close"
		};
		closeButton.Pressed += OnClosePressed;
		header.AddChild(closeButton);

		_report = new TextEdit
		{
			Name = "Report",
			Editable = false,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		layout.AddChild(_report);
	}

	private void ApplyDefaultLayout()
	{
		AnchorLeft = 1f;
		AnchorRight = 1f;
		AnchorTop = 0f;
		AnchorBottom = 0f;

		OffsetLeft = -PanelWidth - PanelMargin;
		OffsetRight = -PanelMargin;
		OffsetTop = PanelMargin;
		OffsetBottom = PanelHeight + PanelMargin;
	}

	private void OnClosePressed()
	{
		Visible = false;
	}

	private void OnStateUpdated(LeafStateSnapshot snapshot)
	{
		RefreshReport();
	}

	private void RefreshReport()
	{
		if (_report == null || _status == null)
		{
			return;
		}

		_status.Text = DateTime.Now.ToString("HH:mm:ss");

		var sb = new StringBuilder();
		if (_monitor == null)
		{
			sb.AppendLine("StateMonitorLeaf not found.");
			SetReportText(sb.ToString());
			return;
		}

		if (_monitor.TryGetState<HudStateSnapshot>(LeafStateKeys.Hud, out var hudState))
		{
			AppendHudState(sb, hudState);
		}
		else
		{
			sb.AppendLine("HUD:");
			sb.AppendLine("  (no data)");
		}

		sb.AppendLine();

		if (_monitor.TryGetState<CutsceneStateSnapshot>(LeafStateKeys.Cutscene, out var cutsceneState))
		{
			AppendCutsceneState(sb, cutsceneState);
		}
		else
		{
			sb.AppendLine("Cutscene:");
			sb.AppendLine("  (no data)");
		}

		sb.AppendLine();

		if (_monitor.TryGetState<MapStateSnapshot>(LeafStateKeys.Map, out var mapState))
		{
			AppendMapState(sb, mapState);
		}
		else
		{
			sb.AppendLine("Map:");
			sb.AppendLine("  (no data)");
		}

		SetReportText(sb.ToString());
	}

	private void SetReportText(string text)
	{
		if (_report == null)
		{
			return;
		}

		double scroll = _report.ScrollVertical;
		_report.Text = text;

		double maxScroll = Math.Max(0, _report.GetLineCount() - 1);
		_report.ScrollVertical = Math.Min(scroll, maxScroll);
	}

	private void AppendHudState(StringBuilder sb, HudStateSnapshot state)
	{
		sb.AppendLine("HUD:");
		sb.AppendLine($"  HudPath: {state.HudPath ?? "(none)"}");
		sb.AppendLine($"  Elements: {state.Elements.Count}");

		foreach (var element in state.Elements.Values)
		{
			sb.AppendLine($"  - {element.Id} ({element.Type}) anchor={element.Anchor} visible={element.Visible}");

			switch (element.Type)
			{
				case "label":
					sb.AppendLine($"      text: {element.Text ?? string.Empty}");
					break;
				case "icon":
					sb.AppendLine($"      icon: {element.IconPath ?? "(none)"}");
					break;
				case "progress":
					sb.AppendLine($"      value: {element.Value} min={element.Min} max={element.Max}");
					break;
			}
		}
	}

	private void AppendCutsceneState(StringBuilder sb, CutsceneStateSnapshot state)
	{
		sb.AppendLine("Cutscene:");
		sb.AppendLine($"  Name: {state.CutsceneName ?? "(none)"}");
		sb.AppendLine($"  Scene: {state.CurrentSceneIndex + 1}/{state.TotalScenes}");
		sb.AppendLine($"  Completed: {state.IsComplete}");

		if (state.CurrentScene.HasValue)
		{
			var scene = state.CurrentScene.Value;
			sb.AppendLine($"  Character: {scene.Character ?? "(none)"}");
			sb.AppendLine($"  Dialogue: {scene.Dialogue ?? "(none)"}");
			sb.AppendLine($"  Background: {scene.Background ?? "(none)"}");
		}
		else
		{
			sb.AppendLine("  CurrentScene: (none)");
		}
	}

	private void AppendMapState(StringBuilder sb, MapStateSnapshot state)
	{
		sb.AppendLine("Map:");
		sb.AppendLine($"  MapPath: {state.MapPath ?? "(none)"}");
		sb.AppendLine($"  Waypoints: {state.WaypointCount}");
		sb.AppendLine($"  LastWaypoint: {state.LastWaypointId ?? "(none)"}");
		sb.AppendLine($"  Zoom: {state.ZoomLevel:0.00}");
		sb.AppendLine($"  Position: {state.MapPosition}");
	}
}
}

using Godot;
using System;
using System.Text;
using TACCsharp.TACC.State;

namespace TACCsharp.Demos.Menu_Demo.Scripts
{
public partial class StateMonitorVisualizerPanel : PanelContainer
{
	private const float PanelWidth = 360f;
	private const float PanelHeight = 480f;
	private const float PanelMargin = 12f;
	private const int TabPadding = 8;

	private TabContainer _tabs;
	private TextEdit _hudReport;
	private TextEdit _cutsceneReport;
	private TextEdit _backgroundReport;
	private TextEdit _musicReport;
	private TextEdit _mapReport;
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
			_status = GetNodeOrNull<Label>("Layout/Header/Status");
			_tabs = GetNodeOrNull<TabContainer>("Layout/Tabs");
			_hudReport = GetNodeOrNull<TextEdit>("Layout/Tabs/Hud/Report");
			_cutsceneReport = GetNodeOrNull<TextEdit>("Layout/Tabs/Cutscene/Report");
			_backgroundReport = GetNodeOrNull<TextEdit>("Layout/Tabs/Background/Report");
			_musicReport = GetNodeOrNull<TextEdit>("Layout/Tabs/Music/Report");
			_mapReport = GetNodeOrNull<TextEdit>("Layout/Tabs/Map/Report");
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

		_tabs = new TabContainer
		{
			Name = "Tabs",
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		layout.AddChild(_tabs);

		_hudReport = CreateTab(_tabs, "HUD", "Hud");
		_cutsceneReport = CreateTab(_tabs, "Cutscene", "Cutscene");
		_backgroundReport = CreateTab(_tabs, "Background", "Background");
		_musicReport = CreateTab(_tabs, "Music", "Music");
		_mapReport = CreateTab(_tabs, "Map", "Map");
	}

	private TextEdit CreateTab(TabContainer tabs, string title, string name)
	{
		var container = new MarginContainer
		{
			Name = name,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		container.AddThemeConstantOverride("margin_left", TabPadding);
		container.AddThemeConstantOverride("margin_top", TabPadding);
		container.AddThemeConstantOverride("margin_right", TabPadding);
		container.AddThemeConstantOverride("margin_bottom", TabPadding);

		var report = new TextEdit
		{
			Name = "Report",
			Editable = false,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		container.AddChild(report);

		tabs.AddChild(container);
		tabs.SetTabTitle(tabs.GetChildCount() - 1, title);

		return report;
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
		if (_status == null)
		{
			return;
		}

		_status.Text = DateTime.Now.ToString("HH:mm:ss");

		if (_monitor == null)
		{
			const string missing = "StateMonitorLeaf not found.";
			SetReportText(_hudReport, missing);
			SetReportText(_cutsceneReport, missing);
			SetReportText(_backgroundReport, missing);
			SetReportText(_musicReport, missing);
			SetReportText(_mapReport, missing);
			return;
		}

		SetReportText(_hudReport, BuildSectionReport<HudStateSnapshot>(LeafStateKeys.Hud, "HUD", AppendHudState));
		SetReportText(_cutsceneReport, BuildSectionReport<CutsceneStateSnapshot>(LeafStateKeys.Cutscene, "Cutscene", AppendCutsceneState));
		SetReportText(_backgroundReport, BuildSectionReport<BackgroundStateSnapshot>(LeafStateKeys.Background, "Background", AppendBackgroundState));
		SetReportText(_musicReport, BuildSectionReport<MusicStateSnapshot>(LeafStateKeys.Music, "Music", AppendMusicState));
		SetReportText(_mapReport, BuildSectionReport<MapStateSnapshot>(LeafStateKeys.Map, "Map", AppendMapState));
	}

	private string BuildSectionReport<T>(string leafKey, string title, Action<StringBuilder, T> append)
		where T : LeafStateSnapshot
	{
		var sb = new StringBuilder();
		if (_monitor != null && _monitor.TryGetState<T>(leafKey, out var state))
		{
			append(sb, state);
		}
		else
		{
			sb.AppendLine($"{title}:");
			sb.AppendLine("  (no data)");
		}

		return sb.ToString();
	}

	private void SetReportText(TextEdit report, string text)
	{
		if (report == null)
		{
			return;
		}

		double scroll = report.ScrollVertical;
		report.Text = text;

		double maxScroll = Math.Max(0, report.GetLineCount() - 1);
		report.ScrollVertical = Math.Min(scroll, maxScroll);
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

	private void AppendBackgroundState(StringBuilder sb, BackgroundStateSnapshot state)
	{
		sb.AppendLine("Background:");
		sb.AppendLine($"  Mode: {state.Mode ?? "(none)"}");
		sb.AppendLine($"  Source: {state.BackgroundPath ?? "(none)"}");
		sb.AppendLine($"  StaticImage: {state.StaticImagePath ?? "(none)"}");
		sb.AppendLine($"  ScrollOffset: {state.ScrollOffset}");
		sb.AppendLine($"  ScrollBaseOffset: {state.ScrollBaseOffset}");
		sb.AppendLine($"  ScrollBaseScale: {state.ScrollBaseScale}");
		sb.AppendLine($"  IgnoreCameraZoom: {state.IgnoreCameraZoom}");
		sb.AppendLine($"  Layers: {state.Layers.Count}");

		foreach (var layer in state.Layers)
		{
			sb.AppendLine($"  - {layer.Index} {layer.Name ?? "Layer"}");
			sb.AppendLine($"      image: {layer.ImagePath ?? "(none)"}");
			sb.AppendLine($"      motionScale: {layer.MotionScale}");
			sb.AppendLine($"      motionOffset: {layer.MotionOffset}");
			sb.AppendLine($"      motionMirroring: {layer.MotionMirroring}");
			sb.AppendLine($"      scale: {layer.Scale}");
			sb.AppendLine($"      zIndex: {layer.ZIndex}");
		}
	}

	private void AppendMusicState(StringBuilder sb, MusicStateSnapshot state)
	{
		sb.AppendLine("Music:");
		sb.AppendLine($"  Config: {state.MusicPath ?? "(none)"}");
		sb.AppendLine($"  Track: {state.TrackPath ?? "(none)"}");
		sb.AppendLine($"  Playing: {state.IsPlaying}");
		sb.AppendLine($"  Paused: {state.IsPaused}");
		sb.AppendLine($"  Loop: {state.Loop}");
		sb.AppendLine($"  VolumeDb: {state.VolumeDb:0.00}");
		sb.AppendLine($"  PitchScale: {state.PitchScale:0.00}");
		sb.AppendLine($"  Bus: {state.Bus ?? "(none)"}");
		sb.AppendLine($"  Playback: {state.PlaybackPosition:0.00}s");
	}
}
}

using Godot;
using System;

public partial class MapDemo : Node
{
	private Stem stem;
	private MapLeaf _mapLeaf;
	private HudOverlayLeaf _hudLeaf;
	private const string MousePositionHudId = "map_mouse_position";
	private const string MousePositionHudAnchor = "top_left";

	public override void _Ready()
	{
		stem = GetNode<Stem>("Stem");
		ConnectMapLeafSignals();
	}

	public override void _Process(double delta)
	{
		if (_hudLeaf == null || _mapLeaf == null)
		{
			return;
		}

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector2 mapPosition = _mapLeaf.ToLocal(mousePosition);

		if (_hudLeaf.EnsureLabel(MousePositionHudId, MousePositionHudAnchor))
		{
			_hudLeaf.SetText(MousePositionHudId, $"Mouse Position: {mapPosition}");
		}
	}

	private void ConnectMapLeafSignals()
	{
		// Wait until MapLeaf is added dynamically
		foreach (Node child in stem.GetChildren())
		{
			if (child is MapLeaf mapLeaf)
			{
				_mapLeaf = mapLeaf;
				mapLeaf.Connect(nameof(MapLeaf.MapLoadedEventHandler), new Callable(this, nameof(OnMapLoaded)));
				mapLeaf.Connect(nameof(MapLeaf.WaypointClickedEventHandler), new Callable(this, nameof(OnWaypointClicked)));

				// Load a demo map
				GD.Print("Loading Demo Map");
				mapLeaf.LoadMap("res://Demos/Data/Maps/Overworld.json");
				InitializeHud();
				GD.Print("MapLeaf signals connected.");
			}
		}
	}

	private void InitializeHud()
	{
		_hudLeaf = stem.GetNodeOrNull<HudOverlayLeaf>("CanvasLayer/HudOverlayLeaf");
		if (_hudLeaf == null)
		{
			GD.PrintErr("HudOverlayLeaf not found in Stem.");
			return;
		}

		if (_hudLeaf.EnsureLabel(MousePositionHudId, MousePositionHudAnchor, "Mouse Position:"))
		{
			_hudLeaf.SetVisible(MousePositionHudId, true);
		}
	}

	private void OnMapLoaded(int waypointCount)
	{
		GD.Print($"Map loaded with {waypointCount} waypoints.");
	}

	private void OnWaypointClicked(string waypointId)
	{
		GD.Print($"Waypoint clicked: {waypointId}");
	}
}

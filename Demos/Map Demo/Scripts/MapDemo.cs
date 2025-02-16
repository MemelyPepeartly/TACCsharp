using Godot;
using System;

public partial class MapDemo : Node
{
	private Stem stem;

	public override void _Ready()
	{
		stem = GetNode<Stem>("Stem");
		ConnectMapLeafSignals();
	}

	private void ConnectMapLeafSignals()
	{
		// Wait until MapLeaf is added dynamically
		foreach (Node child in stem.GetChildren())
		{
			if (child is MapLeaf mapLeaf)
			{
				mapLeaf.Connect(nameof(MapLeaf.MapLoadedEventHandler), new Callable(this, nameof(OnMapLoaded)));
				mapLeaf.Connect(nameof(MapLeaf.WaypointClickedEventHandler), new Callable(this, nameof(OnWaypointClicked)));

				// Load a demo map
				GD.Print("Loading Demo Map");
				mapLeaf.LoadMap("res://Demos/Data/Maps/Overworld.json");
				GD.Print("MapLeaf signals connected.");
			}
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

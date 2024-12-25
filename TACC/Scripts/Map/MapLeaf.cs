using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TACCsharp.TACC.Models;

public partial class MapLeaf : Node2D
{
    [Export] public string JsonPath { get; set; }

    private Texture2D mapTexture;
    private MapData mapData;

    [Signal]
    public delegate void MapLoadedEventHandler(int waypointCount);

    [Signal]
    public delegate void WaypointClickedEventHandler(string waypointId);

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(JsonPath))
        {
            LoadMap(JsonPath);
        }
    }

    public void LoadMap(string path)
    {
        if (!File.Exists(path))
        {
            GD.PrintErr($"Map JSON file not found: {path}");
            return;
        }

        // Read and deserialize the JSON
        string jsonContent = File.ReadAllText(path);
        mapData = JsonConvert.DeserializeObject<MapData>(jsonContent);

        // Load map texture
        mapTexture = GD.Load<Texture2D>(mapData.ImagePath);
        if (mapTexture == null)
        {
            GD.PrintErr($"Map texture not found: {mapData.ImagePath}");
            return;
        }

        // Emit a signal that the map has been loaded
        EmitSignal(nameof(MapLoadedEventHandler), mapData.Waypoints.Count);

        // Display waypoints
        DisplayWaypoints();
    }

    private void DisplayWaypoints()
    {
        foreach (var waypoint in mapData.Waypoints)
        {
            // Create a node to represent the waypoint visually
            var waypointNode = new Sprite2D
            {
                Texture = GD.Load<Texture2D>(waypoint.IconPath),
                Position = waypoint.Position
            };

            // Add metadata to identify the waypoint (thing for signals)
            waypointNode.SetMeta("waypoint_id", waypoint.Id);

            // Handle clicking on the waypoint
            waypointNode.Connect("gui_input", new Callable(this, nameof(OnWaypointClicked)));

            AddChild(waypointNode);
        }
    }

    private void OnWaypointClicked(InputEvent inputEvent, Node viewport)
    {
        if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            // Retrieve the waypoint ID from the node's metadata
            string waypointId = viewport.GetMeta("waypoint_id").AsString();
            if (!string.IsNullOrEmpty(waypointId))
            {
                EmitSignal(nameof(WaypointClickedEventHandler), waypointId);
                GD.Print($"Waypoint clicked: {waypointId}");
            }
        }
    }

    // Accessor for game logic
    public List<Waypoint> GetWaypoints() => mapData?.Waypoints;

    public Texture2D GetMapTexture() => mapTexture;
}

using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TACCsharp.TACC.Models;
using FileAccess = Godot.FileAccess;

public partial class MapLeaf : Node2D
{
	[Export] public string JsonPath { get; set; }

	private Texture2D mapTexture;
	private MapData mapData;
	private Sprite2D mapSprite;

	private bool isDragging = false;
	private Vector2 dragStartPosition;
	private Vector2 mapStartPosition;

	private float zoomLevel = 1.0f;
	private const float zoomStep = 0.1f;
	private const float minZoom = 0.5f;
	private const float maxZoom = 2.0f;

	private Label mousePositionLabel;

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

		// Create a CanvasLayer for UI elements
		var uiLayer = new CanvasLayer();
		AddChild(uiLayer);

		// Create and add the mouse position label to the UI layer
		mousePositionLabel = new Label
		{
			Position = new Vector2(10, 10)
		};
		uiLayer.AddChild(mousePositionLabel);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.Pressed)
				{
					isDragging = true;
					dragStartPosition = mouseEvent.Position;
					mapStartPosition = Position;
				}
				else
				{
					isDragging = false;
				}
			}
			else if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
			{
				ZoomIn(mouseEvent.Position);
			}
			else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
			{
				ZoomOut(mouseEvent.Position);
			}
		}
		else if (@event is InputEventMouseMotion mouseMotionEvent && isDragging)
		{
			Vector2 dragOffset = mouseMotionEvent.Position - dragStartPosition;
			Position = mapStartPosition + dragOffset;
		}

		// Update mouse position label
		if (@event is InputEventMouse mouseInputEvent)
		{
			Vector2 mousePosition = mouseInputEvent.Position;
			Vector2 mapPosition = ToLocal(mousePosition);
			mousePositionLabel.Text = $"Mouse Position: {mapPosition}";
		}
	}

	private void ZoomIn(Vector2 zoomCenter)
	{
		float newZoomLevel = Math.Min(zoomLevel + zoomStep, maxZoom);
		AdjustZoom(zoomCenter, newZoomLevel);
	}

	private void ZoomOut(Vector2 zoomCenter)
	{
		float newZoomLevel = Math.Max(zoomLevel - zoomStep, minZoom);
		AdjustZoom(zoomCenter, newZoomLevel);
	}

	private void AdjustZoom(Vector2 zoomCenter, float newZoomLevel)
	{
		Vector2 offset = (zoomCenter - Position) / zoomLevel;
		zoomLevel = newZoomLevel;
		Scale = new Vector2(zoomLevel, zoomLevel);
		Position = zoomCenter - offset * zoomLevel;
	}

	public void LoadMap(string path)
	{
		// Check file existence using Godot's FileAccess API
		if (!FileAccess.FileExists(path))
		{
			GD.PrintErr($"Map JSON file not found: {path}");
			return;
		}

		try
		{
			// Read and deserialize the JSON
			string jsonContent = FileAccess.Open(path, FileAccess.ModeFlags.Read).GetAsText();
			mapData = JsonConvert.DeserializeObject<MapData>(jsonContent);

			// Load map texture
			mapTexture = GD.Load<Texture2D>(mapData.ImagePath);
			if (mapTexture == null)
			{
				GD.PrintErr($"Map texture not found: {mapData.ImagePath}");
				return;
			}

			// Create and add the map sprite
			mapSprite = new Sprite2D
			{
				Texture = mapTexture
			};
			AddChild(mapSprite);

			// Emit a signal that the map has been loaded
			EmitSignal(nameof(MapLoadedEventHandler), mapData.Waypoints.Count);

			// Display waypoints
			DisplayWaypoints();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to load map: {ex.Message}");
		}
	}

	private void DisplayWaypoints()
	{
		foreach (var waypoint in mapData.Waypoints)
		{
			// Load the waypoint icon texture
			var iconTexture = GD.Load<Texture2D>(waypoint.IconPath);

			// Create a node to represent the waypoint visually
			var waypointNode = new Node2D
			{
				Position = waypoint.Position
			};

			// Create the waypoint icon
			var icon = new Sprite2D
			{
				Texture = iconTexture,
				Scale = new Vector2(0.1f, 0.1f), // Adjust the scale to make the waypoints uniform
				Position = Vector2.Zero
			};
			waypointNode.AddChild(icon);

			// Calculate the scaled size of the icon
			Vector2 iconSize = iconTexture.GetSize() * icon.Scale;

			// Create a white square background matching the scaled icon size
			var background = new ColorRect
			{
				Color = new Color(1, 1, 1, 1),
				Size = iconSize,
				Position = -iconSize / 2
			};
			waypointNode.AddChild(background);

			// Ensure the background is drawn behind the icon
			background.ZIndex = -1;

			// Add metadata to identify the waypoint
			waypointNode.SetMeta("waypoint_id", waypoint.Id);

			// Handle clicking on the waypoint
			icon.Connect("gui_input", new Callable(this, nameof(OnWaypointClicked)));

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
	public override void _Process(double delta)
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector2 mapPosition = ToLocal(mousePosition);
		mousePositionLabel.Text = $"Mouse Position: {mapPosition}";
	}
}

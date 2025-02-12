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
	private Label descriptionLabel;

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

		// Create and add the description label to the UI layer
		descriptionLabel = new Label
		{
			Visible = false
		};
		uiLayer.AddChild(descriptionLabel);
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
	}

	public override void _Process(double delta)
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector2 mapPosition = ToLocal(mousePosition);
		mousePositionLabel.Text = $"Mouse Position: {mapPosition}";

		// Update the position of the description label to follow the mouse
		if (descriptionLabel.Visible)
		{
			descriptionLabel.Position = mousePosition + new Vector2(15, 15);
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
			var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			string jsonContent = file.GetAsText();
			file.Close();

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

			// Create an Area2D node to represent the waypoint
			var waypointArea = new Area2D
			{
				Position = waypoint.Position
			};

			// Create a CollisionShape2D for mouse detection
			var collisionShape = new CollisionShape2D
			{
				Shape = new RectangleShape2D
				{
					Size = iconTexture.GetSize() * 0.1f
				}
			};
			waypointArea.AddChild(collisionShape);

			// Create the waypoint icon
			var icon = new Sprite2D
			{
				Texture = iconTexture,
				Scale = new Vector2(0.1f, 0.1f),
				Position = Vector2.Zero
			};
			waypointArea.AddChild(icon);

			// Add metadata to identify the waypoint and store description
			waypointArea.SetMeta("waypoint_id", waypoint.Id);
			waypointArea.SetMeta("description", waypoint.Description);

			// Connect signals for mouse enter and exit
			waypointArea.MouseEntered += () => OnWaypointMouseEntered(waypointArea);
			waypointArea.MouseExited += OnWaypointMouseExited;

			// Handle clicking on the waypoint using a lambda expression
			waypointArea.InputEvent += (Node viewport, InputEvent @event, long shapeIdx) =>
			{
				if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
				{
					string waypointId = (string)waypointArea.GetMeta("waypoint_id");
					if (!string.IsNullOrEmpty(waypointId))
					{
						EmitSignal(nameof(WaypointClickedEventHandler), waypointId);
						GD.Print($"Waypoint clicked: {waypointId}");
					}
				}
			};

			AddChild(waypointArea);
		}
	}

	private void OnWaypointMouseEntered(Area2D waypointArea)
	{
		string description = (string)waypointArea.GetMeta("description");
		if (!string.IsNullOrEmpty(description))
		{
			descriptionLabel.Text = description;
			descriptionLabel.Visible = true;

			// Position the label near the mouse position
			Vector2 mousePosition = GetViewport().GetMousePosition();
			descriptionLabel.Position = mousePosition + new Vector2(15, 15);
		}
	}

	private void OnWaypointMouseExited()
	{
		descriptionLabel.Visible = false;
	}

	private void OnWaypointClicked(Node viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			var waypointArea = viewport as Area2D;
			string waypointId = (string)waypointArea.GetMeta("waypoint_id");
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

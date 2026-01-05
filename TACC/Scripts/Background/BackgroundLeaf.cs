using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TACCsharp.TACC.Models;
using TACCsharp.TACC.State;
using FileAccess = Godot.FileAccess;

public partial class BackgroundLeaf : Node2D, ILeafStateSource
{
	[Export] public string JsonPath { get; set; }

	public string StateKey => LeafStateKeys.Background;
	public event Action<LeafStateSnapshot> StateChanged;

	private const int BackgroundZIndex = -100;

	private enum BackgroundMode
	{
		None,
		Static,
		Parallax
	}

	private enum BackgroundScaleMode
	{
		Cover,
		Stretch,
		Contain
	}

	private sealed class ParallaxLayerEntry
	{
		public ParallaxLayerEntry(ParallaxLayer layer, Sprite2D sprite, BackgroundLayerData data)
		{
			Layer = layer;
			Sprite = sprite;
			Data = data;
		}

		public ParallaxLayer Layer { get; }
		public Sprite2D Sprite { get; }
		public BackgroundLayerData Data { get; }
	}

	private Sprite2D _staticSprite;
	private ParallaxBackground _parallaxBackground;
	private readonly List<ParallaxLayerEntry> _parallaxLayers = new();
	private BackgroundMode _activeMode = BackgroundMode.None;
	private BackgroundScaleMode _staticScaleMode = BackgroundScaleMode.Cover;
	private string _loadedBackgroundPath;
	private string _staticImagePath;
	private bool _isVisible = true;

	public override void _Ready()
	{
		AddToGroup(LeafStateGroups.LeafStateSourceGroup);

		var viewport = GetViewport();
		if (viewport != null)
		{
			viewport.SizeChanged += OnViewportSizeChanged;
		}

		if (!string.IsNullOrWhiteSpace(JsonPath))
		{
			LoadBackground(JsonPath);
		}
	}

	public override void _ExitTree()
	{
		var viewport = GetViewport();
		if (viewport != null)
		{
			viewport.SizeChanged -= OnViewportSizeChanged;
		}
	}

	public void LoadBackground(string jsonPath)
	{
		if (!FileAccess.FileExists(jsonPath))
		{
			GD.PrintErr($"Background JSON file not found: {jsonPath}");
			return;
		}

		_loadedBackgroundPath = jsonPath;

		try
		{
			using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
			string jsonContent = file.GetAsText();
			var data = JsonConvert.DeserializeObject<BackgroundData>(jsonContent);

			if (data == null)
			{
				GD.PrintErr("ERROR: Background JSON deserialized to null.");
				return;
			}

			ApplyBackground(data);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"ERROR: Failed to load background: {ex.Message}");
		}
	}

	public void SetStaticBackground(string texturePath, string scaleMode = null)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			GD.PrintErr("Background texture path is empty.");
			return;
		}

		var texture = GD.Load<Texture2D>(texturePath);
		if (texture == null)
		{
			GD.PrintErr($"Background texture not found: {texturePath}");
			return;
		}

		EnsureStaticSprite();
		ClearParallaxLayers();

		_staticImagePath = texturePath;
		_activeMode = BackgroundMode.Static;
		_staticScaleMode = ParseScaleMode(scaleMode);

		_staticSprite.Texture = texture;
		UpdateStaticLayout();
		ApplyVisibility();
		EmitStateChanged();
	}

	public void SetParallaxBackground(IReadOnlyList<BackgroundLayerData> layers, Vector2? baseOffset = null, Vector2? baseScale = null, bool ignoreCameraZoom = false, Vector2? scrollOffset = null)
	{
		if (layers == null || layers.Count == 0)
		{
			GD.PrintErr("Parallax background requires at least one layer.");
			return;
		}

		EnsureParallaxBackground();
		ClearStaticSprite();
		ClearParallaxLayers();

		if (baseOffset.HasValue)
		{
			_parallaxBackground.ScrollBaseOffset = baseOffset.Value;
		}

		if (baseScale.HasValue)
		{
			_parallaxBackground.ScrollBaseScale = baseScale.Value;
		}

		_parallaxBackground.ScrollIgnoreCameraZoom = ignoreCameraZoom;

		if (scrollOffset.HasValue)
		{
			_parallaxBackground.ScrollOffset = scrollOffset.Value;
		}

		for (int i = 0; i < layers.Count; i++)
		{
			var layerData = layers[i];
			if (layerData == null)
			{
				continue;
			}

			if (string.IsNullOrWhiteSpace(layerData.ImagePath))
			{
				GD.PrintErr("Parallax layer imagePath is empty.");
				continue;
			}

			var texture = GD.Load<Texture2D>(layerData.ImagePath);
			if (texture == null)
			{
				GD.PrintErr($"Parallax layer texture not found: {layerData.ImagePath}");
				continue;
			}

			var layer = new ParallaxLayer
			{
				Name = string.IsNullOrWhiteSpace(layerData.Name) ? $"Layer{i}" : layerData.Name
			};

			layer.MotionScale = layerData.MotionScale?.ToVector2() ?? Vector2.One;

			if (layerData.MotionOffset != null)
			{
				layer.MotionOffset = layerData.MotionOffset.ToVector2();
			}

			if (layerData.MotionMirroring != null)
			{
				layer.MotionMirroring = layerData.MotionMirroring.ToVector2();
			}

			var sprite = new Sprite2D
			{
				Texture = texture,
				Centered = true
			};

			if (layerData.ZIndex.HasValue)
			{
				sprite.ZIndex = layerData.ZIndex.Value;
			}

			layer.AddChild(sprite);
			_parallaxBackground.AddChild(layer);
			_parallaxLayers.Add(new ParallaxLayerEntry(layer, sprite, layerData));
		}

		_activeMode = BackgroundMode.Parallax;
		_staticImagePath = null;

		UpdateParallaxLayout();
		ApplyVisibility();
		EmitStateChanged();
	}

	public void ClearBackground()
	{
		_activeMode = BackgroundMode.None;
		_staticImagePath = null;
		_loadedBackgroundPath = null;

		ClearStaticSprite();
		ClearParallaxLayers();
		EmitStateChanged();
	}

	public void SetBackgroundVisible(bool isVisible)
	{
		_isVisible = isVisible;
		ApplyVisibility();
	}

	public void SetParallaxScrollOffset(Vector2 offset)
	{
		if (_parallaxBackground == null)
		{
			return;
		}

		_parallaxBackground.ScrollOffset = offset;
		EmitStateChanged();
	}

	public LeafStateSnapshot GetStateSnapshot()
	{
		return BuildSnapshot();
	}

	private void ApplyBackground(BackgroundData data)
	{
		if (data == null)
		{
			return;
		}

		string mode = NormalizeMode(data.Mode);

		if (mode == "parallax" || (string.IsNullOrWhiteSpace(mode) && data.Layers != null && data.Layers.Count > 0))
		{
			var baseOffset = data.ScrollBaseOffset?.ToVector2();
			var baseScale = data.ScrollBaseScale?.ToVector2();
			var scrollOffset = data.ScrollOffset?.ToVector2();
			bool ignoreCameraZoom = data.IgnoreCameraZoom ?? false;

			SetParallaxBackground(data.Layers, baseOffset, baseScale, ignoreCameraZoom, scrollOffset);
			return;
		}

		SetStaticBackground(data.ImagePath, data.ScaleMode);
	}

	private void EnsureStaticSprite()
	{
		if (_staticSprite != null)
		{
			return;
		}

		_staticSprite = new Sprite2D
		{
			Name = "StaticBackground",
			Centered = true,
			ZIndex = BackgroundZIndex
		};

		AddChild(_staticSprite);
	}

	private void EnsureParallaxBackground()
	{
		if (_parallaxBackground != null)
		{
			return;
		}

		_parallaxBackground = new ParallaxBackground
		{
			Name = "ParallaxBackground",
			Layer = -100
		};

		AddChild(_parallaxBackground);
	}

	private void ClearStaticSprite()
	{
		if (_staticSprite == null)
		{
			return;
		}

		_staticSprite.Texture = null;
		_staticSprite.Visible = false;
	}

	private void ClearParallaxLayers()
	{
		if (_parallaxLayers.Count == 0)
		{
			return;
		}

		foreach (var entry in _parallaxLayers)
		{
			if (entry.Layer == null)
			{
				continue;
			}

			entry.Layer.GetParent()?.RemoveChild(entry.Layer);
			entry.Layer.QueueFree();
		}

		_parallaxLayers.Clear();
	}

	private void ApplyVisibility()
	{
		if (_staticSprite != null)
		{
			_staticSprite.Visible = _isVisible && _activeMode == BackgroundMode.Static;
		}

		foreach (var entry in _parallaxLayers)
		{
			if (entry.Layer == null)
			{
				continue;
			}

			entry.Layer.Visible = _isVisible && _activeMode == BackgroundMode.Parallax;
		}
	}

	private void OnViewportSizeChanged()
	{
		UpdateStaticLayout();
		UpdateParallaxLayout();
	}

	private void UpdateStaticLayout()
	{
		if (_staticSprite == null || _staticSprite.Texture == null)
		{
			return;
		}

		var viewport = GetViewport();
		if (viewport == null)
		{
			return;
		}

		Vector2 viewportSize = viewport.GetVisibleRect().Size;
		Vector2 textureSize = _staticSprite.Texture.GetSize();
		if (textureSize == Vector2.Zero)
		{
			return;
		}

		Vector2 scale = GetScaleForMode(viewportSize, textureSize, _staticScaleMode);
		_staticSprite.Scale = scale;
		_staticSprite.Position = viewportSize / 2.0f;
	}

	private void UpdateParallaxLayout()
	{
		if (_parallaxLayers.Count == 0)
		{
			return;
		}

		var viewport = GetViewport();
		if (viewport == null)
		{
			return;
		}

		Vector2 viewportSize = viewport.GetVisibleRect().Size;

		foreach (var entry in _parallaxLayers)
		{
			if (entry.Sprite == null || entry.Sprite.Texture == null)
			{
				continue;
			}

			Vector2 textureSize = entry.Sprite.Texture.GetSize();
			if (textureSize == Vector2.Zero)
			{
				continue;
			}

			Vector2 scale = GetScaleForMode(viewportSize, textureSize, BackgroundScaleMode.Cover);
			var scaleOverride = entry.Data?.Scale?.ToVector2() ?? Vector2.One;
			entry.Sprite.Scale = new Vector2(scale.X * scaleOverride.X, scale.Y * scaleOverride.Y);
			entry.Sprite.Position = viewportSize / 2.0f;
		}
	}

	private BackgroundScaleMode ParseScaleMode(string scaleMode)
	{
		if (string.IsNullOrWhiteSpace(scaleMode))
		{
			return BackgroundScaleMode.Cover;
		}

		switch (scaleMode.Trim().ToLowerInvariant())
		{
			case "stretch":
				return BackgroundScaleMode.Stretch;
			case "contain":
				return BackgroundScaleMode.Contain;
			default:
				return BackgroundScaleMode.Cover;
		}
	}

	private Vector2 GetScaleForMode(Vector2 viewportSize, Vector2 textureSize, BackgroundScaleMode mode)
	{
		if (textureSize == Vector2.Zero)
		{
			return Vector2.One;
		}

		float scaleX = viewportSize.X / textureSize.X;
		float scaleY = viewportSize.Y / textureSize.Y;

		switch (mode)
		{
			case BackgroundScaleMode.Stretch:
				return new Vector2(scaleX, scaleY);
			case BackgroundScaleMode.Contain:
				float minScale = Mathf.Min(scaleX, scaleY);
				return new Vector2(minScale, minScale);
			default:
				float maxScale = Mathf.Max(scaleX, scaleY);
				return new Vector2(maxScale, maxScale);
		}
	}

	private LeafStateSnapshot BuildSnapshot()
	{
		var snapshot = new BackgroundStateSnapshot
		{
			BackgroundPath = _loadedBackgroundPath ?? JsonPath,
			Mode = _activeMode.ToString().ToLowerInvariant(),
			StaticImagePath = _activeMode == BackgroundMode.Static ? _staticImagePath : null,
			ScrollOffset = _parallaxBackground != null ? _parallaxBackground.ScrollOffset : Vector2.Zero,
			ScrollBaseOffset = _parallaxBackground != null ? _parallaxBackground.ScrollBaseOffset : Vector2.Zero,
			ScrollBaseScale = _parallaxBackground != null ? _parallaxBackground.ScrollBaseScale : Vector2.One,
			IgnoreCameraZoom = _parallaxBackground != null && _parallaxBackground.ScrollIgnoreCameraZoom
		};

		if (_activeMode == BackgroundMode.Parallax)
		{
			for (int i = 0; i < _parallaxLayers.Count; i++)
			{
				var entry = _parallaxLayers[i];
				if (entry.Layer == null || entry.Sprite == null)
				{
					continue;
				}

				var layerSnapshot = new BackgroundLayerSnapshot
				{
					Index = i,
					Name = entry.Layer.Name,
					ImagePath = entry.Sprite.Texture?.ResourcePath,
					MotionScale = entry.Layer.MotionScale,
					MotionOffset = entry.Layer.MotionOffset,
					MotionMirroring = entry.Layer.MotionMirroring,
					Scale = entry.Sprite.Scale,
					ZIndex = entry.Sprite.ZIndex
				};

				snapshot.Layers.Add(layerSnapshot);
			}
		}

		return snapshot;
	}

	private string NormalizeMode(string mode)
	{
		return string.IsNullOrWhiteSpace(mode) ? null : mode.Trim().ToLowerInvariant();
	}

	private void EmitStateChanged()
	{
		StateChanged?.Invoke(BuildSnapshot());
	}
}

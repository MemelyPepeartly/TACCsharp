using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TACCsharp.TACC.Models;
using TACCsharp.TACC.State;
using FileAccess = Godot.FileAccess;

public partial class SpriteLeaf : Node2D, ILeafStateSource
{
	[Export] public string JsonPath { get; set; }

	public string StateKey => LeafStateKeys.Sprite;
	public event Action<LeafStateSnapshot> StateChanged;

	private readonly Dictionary<string, Node2D> _spritesById = new();
	private readonly Dictionary<string, Texture2D> _textureCache = new();
	private readonly Dictionary<string, SpriteFrames> _framesCache = new();

	private string _loadedSpritePath;

	public override void _Ready()
	{
		AddToGroup(LeafStateGroups.LeafStateSourceGroup);

		if (!string.IsNullOrWhiteSpace(JsonPath))
		{
			LoadSprites(JsonPath);
		}
	}

	public void LoadSprites(string jsonPath)
	{
		if (!FileAccess.FileExists(jsonPath))
		{
			GD.PrintErr($"Sprite JSON file not found: {jsonPath}");
			return;
		}

		_loadedSpritePath = jsonPath;

		try
		{
			using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
			string jsonContent = file.GetAsText();
			var data = JsonConvert.DeserializeObject<SpriteSetData>(jsonContent);

			if (data?.Sprites == null || data.Sprites.Count == 0)
			{
				GD.PrintErr("ERROR: No valid sprites found in JSON.");
				return;
			}

			ApplySprites(data);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"ERROR: Failed to load sprites: {ex.Message}");
		}
	}

	public bool UpsertSprite(SpriteData data)
	{
		if (!ValidateSpriteData(data))
		{
			return false;
		}

		if (_spritesById.TryGetValue(data.Id, out var existing))
		{
			if (!UpdateSpriteNode(data, existing))
			{
				return false;
			}
		}
		else
		{
			var node = CreateSpriteNode(data);
			if (node == null)
			{
				return false;
			}

			_spritesById[data.Id] = node;
			AddChild(node);
		}

		EmitStateChanged();
		return true;
	}

	public bool RemoveSprite(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			GD.PrintErr("Sprite id is required.");
			return false;
		}

		if (!_spritesById.TryGetValue(id, out var node))
		{
			GD.PrintErr($"Sprite '{id}' not found.");
			return false;
		}

		RemoveSpriteNode(id, node);
		EmitStateChanged();
		return true;
	}

	public void ClearSprites()
	{
		_loadedSpritePath = null;
		ClearSpritesInternal();
		EmitStateChanged();
	}

	public Node2D GetSpriteNode(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return null;
		}

		_spritesById.TryGetValue(id, out var node);
		return node;
	}

	public LeafStateSnapshot GetStateSnapshot()
	{
		return BuildSnapshot();
	}

	private void ApplySprites(SpriteSetData data)
	{
		ClearSpritesInternal();

		foreach (var spriteData in data.Sprites)
		{
			if (!ValidateSpriteData(spriteData))
			{
				continue;
			}

			if (_spritesById.ContainsKey(spriteData.Id))
			{
				GD.PrintErr($"Duplicate sprite id '{spriteData.Id}'.");
				continue;
			}

			var node = CreateSpriteNode(spriteData);
			if (node == null)
			{
				continue;
			}

			_spritesById[spriteData.Id] = node;
			AddChild(node);
		}

		EmitStateChanged();
	}

	private void ClearSpritesInternal()
	{
		foreach (var node in _spritesById.Values)
		{
			if (node == null)
			{
				continue;
			}

			node.GetParent()?.RemoveChild(node);
			node.QueueFree();
		}

		_spritesById.Clear();
	}

	private bool ValidateSpriteData(SpriteData data)
	{
		if (data == null)
		{
			GD.PrintErr("Sprite data is null.");
			return false;
		}

		if (string.IsNullOrWhiteSpace(data.Id))
		{
			GD.PrintErr("Sprite is missing an id.");
			return false;
		}

		if (string.IsNullOrWhiteSpace(data.FramesPath) && string.IsNullOrWhiteSpace(data.TexturePath))
		{
			GD.PrintErr($"Sprite '{data.Id}' needs a texturePath or framesPath.");
			return false;
		}

		return true;
	}

	private Node2D CreateSpriteNode(SpriteData data)
	{
		bool isAnimated = !string.IsNullOrWhiteSpace(data.FramesPath);

		if (isAnimated)
		{
			var frames = LoadFrames(data.FramesPath);
			if (frames == null)
			{
				return null;
			}

			var animated = new AnimatedSprite2D
			{
				Name = data.Id,
				SpriteFrames = frames
			};

			ApplySpriteTransforms(animated, data);
			ApplySpriteVisuals(animated, data);
			ApplyAnimationSettings(animated, data);
			return animated;
		}

		var texture = LoadTexture(data.TexturePath);
		if (texture == null)
		{
			return null;
		}

		var sprite = new Sprite2D
		{
			Name = data.Id,
			Texture = texture
		};

		ApplySpriteTransforms(sprite, data);
		ApplySpriteVisuals(sprite, data);
		return sprite;
	}

	private bool UpdateSpriteNode(SpriteData data, Node2D existing)
	{
		bool wantsAnimated = !string.IsNullOrWhiteSpace(data.FramesPath);
		bool isAnimated = existing is AnimatedSprite2D;

		if (wantsAnimated != isAnimated)
		{
			var replacement = CreateSpriteNode(data);
			if (replacement == null)
			{
				return false;
			}

			RemoveSpriteNode(data.Id, existing);
			_spritesById[data.Id] = replacement;
			AddChild(replacement);
			return true;
		}

		if (existing is AnimatedSprite2D animated)
		{
			UpdateAnimatedSprite(animated, data);
		}
		else if (existing is Sprite2D sprite)
		{
			UpdateStaticSprite(sprite, data);
		}

		ApplySpriteTransforms(existing, data);
		ApplySpriteVisuals(existing, data);
		return true;
	}

	private void UpdateStaticSprite(Sprite2D sprite, SpriteData data)
	{
		if (!string.IsNullOrWhiteSpace(data.TexturePath))
		{
			var texture = LoadTexture(data.TexturePath);
			if (texture != null)
			{
				sprite.Texture = texture;
			}
		}
	}

	private void UpdateAnimatedSprite(AnimatedSprite2D animated, SpriteData data)
	{
		if (!string.IsNullOrWhiteSpace(data.FramesPath))
		{
			var frames = LoadFrames(data.FramesPath);
			if (frames != null)
			{
				animated.SpriteFrames = frames;
			}
		}

		ApplyAnimationSettings(animated, data);
	}

	private void ApplySpriteTransforms(Node2D node, SpriteData data)
	{
		if (data.Position != null)
		{
			node.Position = data.Position.ToVector2();
		}

		if (data.RotationDegrees.HasValue)
		{
			node.RotationDegrees = data.RotationDegrees.Value;
		}

		if (data.Scale != null)
		{
			node.Scale = data.Scale.ToVector2();
		}
	}

	private void ApplySpriteVisuals(Node2D node, SpriteData data)
	{
		if (node is CanvasItem item)
		{
			if (data.Visible.HasValue)
			{
				item.Visible = data.Visible.Value;
			}

			if (data.ZIndex.HasValue)
			{
				item.ZIndex = data.ZIndex.Value;
			}

			if (data.ZAsRelative.HasValue)
			{
				item.ZAsRelative = data.ZAsRelative.Value;
			}

			if (data.Modulate != null)
			{
				item.Modulate = data.Modulate.ToColor();
			}
		}

		if (node is Sprite2D sprite)
		{
			ApplySpriteDetails(sprite, data);
		}
		else if (node is AnimatedSprite2D animated)
		{
			ApplySpriteDetails(animated, data);
		}
	}

	private void ApplySpriteDetails(Sprite2D sprite, SpriteData data)
	{
		if (data.Centered.HasValue)
		{
			sprite.Centered = data.Centered.Value;
		}

		if (data.FlipH.HasValue)
		{
			sprite.FlipH = data.FlipH.Value;
		}

		if (data.FlipV.HasValue)
		{
			sprite.FlipV = data.FlipV.Value;
		}

		if (data.Offset != null)
		{
			sprite.Offset = data.Offset.ToVector2();
		}
	}

	private void ApplySpriteDetails(AnimatedSprite2D sprite, SpriteData data)
	{
		if (data.Centered.HasValue)
		{
			sprite.Centered = data.Centered.Value;
		}

		if (data.FlipH.HasValue)
		{
			sprite.FlipH = data.FlipH.Value;
		}

		if (data.FlipV.HasValue)
		{
			sprite.FlipV = data.FlipV.Value;
		}

		if (data.Offset != null)
		{
			sprite.Offset = data.Offset.ToVector2();
		}
	}

	private void ApplyAnimationSettings(AnimatedSprite2D sprite, SpriteData data)
	{
		if (!string.IsNullOrWhiteSpace(data.Animation))
		{
			if (sprite.SpriteFrames != null && sprite.SpriteFrames.HasAnimation(data.Animation))
			{
				sprite.Animation = data.Animation;
			}
			else
			{
				GD.PrintErr($"Animated sprite '{data.Id}' does not have animation '{data.Animation}'.");
			}
		}

		if (data.SpeedScale.HasValue)
		{
			if (data.SpeedScale.Value > 0f)
			{
				sprite.SpeedScale = data.SpeedScale.Value;
			}
			else
			{
				GD.PrintErr($"Animated sprite '{data.Id}' speedScale must be greater than zero.");
			}
		}

		if (data.Frame.HasValue && data.Frame.Value >= 0)
		{
			sprite.Frame = data.Frame.Value;
		}

		if (data.Playing.HasValue)
		{
			if (data.Playing.Value)
			{
				if (!string.IsNullOrWhiteSpace(sprite.Animation))
				{
					sprite.Play(sprite.Animation);
				}
				else
				{
					sprite.Play();
				}
			}
			else
			{
				sprite.Stop();
			}
		}
	}

	private Texture2D LoadTexture(string texturePath)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			return null;
		}

		if (_textureCache.TryGetValue(texturePath, out var cached))
		{
			return cached;
		}

		var texture = GD.Load<Texture2D>(texturePath);
		if (texture == null)
		{
			GD.PrintErr($"Sprite texture not found: {texturePath}");
			return null;
		}

		_textureCache[texturePath] = texture;
		return texture;
	}

	private SpriteFrames LoadFrames(string framesPath)
	{
		if (string.IsNullOrWhiteSpace(framesPath))
		{
			return null;
		}

		if (_framesCache.TryGetValue(framesPath, out var cached))
		{
			return cached;
		}

		var frames = GD.Load<SpriteFrames>(framesPath);
		if (frames == null)
		{
			GD.PrintErr($"Sprite frames not found: {framesPath}");
			return null;
		}

		_framesCache[framesPath] = frames;
		return frames;
	}

	private void RemoveSpriteNode(string id, Node2D node)
	{
		if (node == null)
		{
			return;
		}

		node.GetParent()?.RemoveChild(node);
		node.QueueFree();
		_spritesById.Remove(id);
	}

	private SpriteStateSnapshot BuildSnapshot()
	{
		var snapshot = new SpriteStateSnapshot
		{
			SpritePath = _loadedSpritePath ?? JsonPath
		};

		foreach (var pair in _spritesById)
		{
			if (pair.Value == null)
			{
				continue;
			}

			snapshot.Sprites.Add(BuildSpriteSnapshot(pair.Key, pair.Value));
		}

		return snapshot;
	}

	private SpriteSnapshot BuildSpriteSnapshot(string id, Node2D node)
	{
		var snapshot = new SpriteSnapshot
		{
			Id = id,
			Position = node.Position,
			Scale = node.Scale,
			RotationDegrees = node.RotationDegrees
		};

		if (node is CanvasItem item)
		{
			snapshot.Visible = item.Visible;
			snapshot.ZIndex = item.ZIndex;
			snapshot.ZAsRelative = item.ZAsRelative;
			snapshot.Modulate = item.Modulate;
		}

		if (node is Sprite2D sprite)
		{
			snapshot.IsAnimated = false;
			snapshot.TexturePath = sprite.Texture?.ResourcePath;
			snapshot.Centered = sprite.Centered;
			snapshot.FlipH = sprite.FlipH;
			snapshot.FlipV = sprite.FlipV;
			snapshot.Offset = sprite.Offset;
			snapshot.IsPlaying = false;
			snapshot.Frame = 0;
			snapshot.SpeedScale = 1f;
		}
		else if (node is AnimatedSprite2D animated)
		{
			snapshot.IsAnimated = true;
			snapshot.FramesPath = animated.SpriteFrames?.ResourcePath;
			snapshot.Animation = animated.Animation;
			snapshot.IsPlaying = animated.Playing;
			snapshot.Frame = animated.Frame;
			snapshot.SpeedScale = animated.SpeedScale;
			snapshot.Centered = animated.Centered;
			snapshot.FlipH = animated.FlipH;
			snapshot.FlipV = animated.FlipV;
			snapshot.Offset = animated.Offset;
		}

		return snapshot;
	}

	private void EmitStateChanged()
	{
		StateChanged?.Invoke(BuildSnapshot());
	}
}

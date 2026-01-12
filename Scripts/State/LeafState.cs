using System;
using System.Collections.Generic;
using Godot;
using TACCsharp.TACC.Models;

namespace TACCsharp.TACC.State
{
	public static class LeafStateGroups
	{
		public const string LeafStateSourceGroup = "leaf_state_sources";
	}

	public static class LeafStateKeys
	{
		public const string Hud = "hud";
		public const string Cutscene = "cutscene";
		public const string Map = "map";
		public const string Background = "background";
		public const string Music = "music";
		public const string Sprite = "sprite";
	}

	public abstract class LeafStateSnapshot
	{
		protected LeafStateSnapshot(string leafKey)
		{
			LeafKey = leafKey;
		}

		public string LeafKey { get; }
	}

	public interface ILeafStateSource
	{
		string StateKey { get; }
		event Action<LeafStateSnapshot> StateChanged;
		LeafStateSnapshot GetStateSnapshot();
	}

	public sealed class HudElementState
	{
		public string Id { get; set; }
		public string Type { get; set; }
		public string Anchor { get; set; }
		public string Text { get; set; }
		public string IconPath { get; set; }
		public double? Value { get; set; }
		public double? Min { get; set; }
		public double? Max { get; set; }
		public bool Visible { get; set; } = true;

		public HudElementState Clone()
		{
			return new HudElementState
			{
				Id = Id,
				Type = Type,
				Anchor = Anchor,
				Text = Text,
				IconPath = IconPath,
				Value = Value,
				Min = Min,
				Max = Max,
				Visible = Visible
			};
		}
	}

	public sealed class HudStateSnapshot : LeafStateSnapshot
	{
		public HudStateSnapshot() : base(LeafStateKeys.Hud) { }

		public string HudPath { get; set; }
		public Dictionary<string, HudElementState> Elements { get; } = new Dictionary<string, HudElementState>();
	}

	public sealed class CutsceneStateSnapshot : LeafStateSnapshot
	{
		public CutsceneStateSnapshot() : base(LeafStateKeys.Cutscene) { }

		public string CutsceneName { get; set; }
		public int CurrentSceneIndex { get; set; }
		public int TotalScenes { get; set; }
		public SceneData? CurrentScene { get; set; }
		public bool IsComplete { get; set; }
	}

	public sealed class MapStateSnapshot : LeafStateSnapshot
	{
		public MapStateSnapshot() : base(LeafStateKeys.Map) { }

		public string MapPath { get; set; }
		public int WaypointCount { get; set; }
		public string LastWaypointId { get; set; }
		public float ZoomLevel { get; set; }
		public Vector2 MapPosition { get; set; }
	}

	public sealed class BackgroundLayerSnapshot
	{
		public int Index { get; set; }
		public string Name { get; set; }
		public string ImagePath { get; set; }
		public Vector2 MotionScale { get; set; }
		public Vector2 MotionOffset { get; set; }
		public Vector2 MotionMirroring { get; set; }
		public Vector2 Scale { get; set; }
		public int ZIndex { get; set; }
	}

	public sealed class BackgroundStateSnapshot : LeafStateSnapshot
	{
		public BackgroundStateSnapshot() : base(LeafStateKeys.Background) { }

		public string BackgroundPath { get; set; }
		public string Mode { get; set; }
		public string StaticImagePath { get; set; }
		public Vector2 ScrollOffset { get; set; }
		public Vector2 ScrollBaseOffset { get; set; }
		public Vector2 ScrollBaseScale { get; set; } = Vector2.One;
		public bool IgnoreCameraZoom { get; set; }
		public List<BackgroundLayerSnapshot> Layers { get; } = new List<BackgroundLayerSnapshot>();
	}

	public sealed class MusicStateSnapshot : LeafStateSnapshot
	{
		public MusicStateSnapshot() : base(LeafStateKeys.Music) { }

		public string MusicPath { get; set; }
		public string TrackPath { get; set; }
		public bool IsPlaying { get; set; }
		public bool IsPaused { get; set; }
		public bool Loop { get; set; }
		public float VolumeDb { get; set; }
		public float PitchScale { get; set; }
		public string Bus { get; set; }
		public double PlaybackPosition { get; set; }
	}

	public sealed class SpriteSnapshot
	{
		public string Id { get; set; }
		public string TexturePath { get; set; }
		public string FramesPath { get; set; }
		public string Animation { get; set; }
		public bool IsAnimated { get; set; }
		public bool IsPlaying { get; set; }
		public int Frame { get; set; }
		public float SpeedScale { get; set; }
		public Vector2 Position { get; set; }
		public Vector2 Scale { get; set; }
		public float RotationDegrees { get; set; }
		public Vector2 Offset { get; set; }
		public bool Centered { get; set; }
		public bool FlipH { get; set; }
		public bool FlipV { get; set; }
		public bool Visible { get; set; }
		public int ZIndex { get; set; }
		public bool ZAsRelative { get; set; }
		public Color Modulate { get; set; }
	}

	public sealed class SpriteStateSnapshot : LeafStateSnapshot
	{
		public SpriteStateSnapshot() : base(LeafStateKeys.Sprite) { }

		public string SpritePath { get; set; }
		public List<SpriteSnapshot> Sprites { get; } = new List<SpriteSnapshot>();
	}
}

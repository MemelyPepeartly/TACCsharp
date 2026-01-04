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
}

using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TACCsharp.TACC.Models
{
	public class BackgroundData
	{
		[JsonProperty("mode")]
		public string Mode { get; set; }

		[JsonProperty("imagePath")]
		public string ImagePath { get; set; }

		[JsonProperty("scaleMode")]
		public string ScaleMode { get; set; }

		[JsonProperty("layers")]
		public List<BackgroundLayerData> Layers { get; set; }

		[JsonProperty("scrollOffset")]
		public Vector2Data ScrollOffset { get; set; }

		[JsonProperty("scrollBaseOffset")]
		public Vector2Data ScrollBaseOffset { get; set; }

		[JsonProperty("scrollBaseScale")]
		public Vector2Data ScrollBaseScale { get; set; }

		[JsonProperty("ignoreCameraZoom")]
		public bool? IgnoreCameraZoom { get; set; }
	}

	public class BackgroundLayerData
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("imagePath")]
		public string ImagePath { get; set; }

		[JsonProperty("motionScale")]
		public Vector2Data MotionScale { get; set; }

		[JsonProperty("motionOffset")]
		public Vector2Data MotionOffset { get; set; }

		[JsonProperty("motionMirroring")]
		public Vector2Data MotionMirroring { get; set; }

		[JsonProperty("repeatX")]
		public bool? RepeatX { get; set; }

		[JsonProperty("repeatY")]
		public bool? RepeatY { get; set; }

		[JsonProperty("scale")]
		public Vector2Data Scale { get; set; }

		[JsonProperty("zIndex")]
		public int? ZIndex { get; set; }
	}

	public class Vector2Data
	{
		[JsonProperty("x")]
		public float X { get; set; }

		[JsonProperty("y")]
		public float Y { get; set; }

		public Vector2 ToVector2()
		{
			return new Vector2(X, Y);
		}
	}
}

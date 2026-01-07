using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TACCsharp.TACC.Models
{
	public class SpriteSetData
	{
		[JsonProperty("sprites")]
		public List<SpriteData> Sprites { get; set; }
	}

	public class SpriteData
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("texturePath")]
		public string TexturePath { get; set; }

		[JsonProperty("framesPath")]
		public string FramesPath { get; set; }

		[JsonProperty("animation")]
		public string Animation { get; set; }

		[JsonProperty("playing")]
		public bool? Playing { get; set; }

		[JsonProperty("speedScale")]
		public float? SpeedScale { get; set; }

		[JsonProperty("frame")]
		public int? Frame { get; set; }

		[JsonProperty("position")]
		public Vector2Data Position { get; set; }

		[JsonProperty("rotationDegrees")]
		public float? RotationDegrees { get; set; }

		[JsonProperty("scale")]
		public Vector2Data Scale { get; set; }

		[JsonProperty("offset")]
		public Vector2Data Offset { get; set; }

		[JsonProperty("centered")]
		public bool? Centered { get; set; }

		[JsonProperty("flipH")]
		public bool? FlipH { get; set; }

		[JsonProperty("flipV")]
		public bool? FlipV { get; set; }

		[JsonProperty("visible")]
		public bool? Visible { get; set; }

		[JsonProperty("zIndex")]
		public int? ZIndex { get; set; }

		[JsonProperty("zAsRelative")]
		public bool? ZAsRelative { get; set; }

		[JsonProperty("modulate")]
		public ColorData Modulate { get; set; }
	}

	public class ColorData
	{
		[JsonProperty("r")]
		public float R { get; set; } = 1f;

		[JsonProperty("g")]
		public float G { get; set; } = 1f;

		[JsonProperty("b")]
		public float B { get; set; } = 1f;

		[JsonProperty("a")]
		public float A { get; set; } = 1f;

		public Color ToColor()
		{
			return new Color(R, G, B, A);
		}
	}
}

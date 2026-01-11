using Godot;
using System.Collections.Generic;
using TACCsharp.TACC.Serialization;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TACCsharp.TACC.Models
{
	public class SpriteSetData
	{
		public List<SpriteData> Sprites { get; set; }

		public static SpriteSetData FromDictionary(GodotDictionary dictionary)
		{
			if (dictionary == null)
			{
				return null;
			}

			var data = new SpriteSetData();
			if (TaccJson.TryGetArray(dictionary, "sprites", out var spritesArray))
			{
				var sprites = new List<SpriteData>();
				foreach (Variant entry in spritesArray)
				{
					if (entry.VariantType != Variant.Type.Dictionary)
					{
						continue;
					}

					sprites.Add(SpriteData.FromDictionary(entry.AsGodotDictionary()));
				}

				data.Sprites = sprites;
			}

			return data;
		}
	}

	public class SpriteData
	{
		public string Id { get; set; }

		public string TexturePath { get; set; }

		public string FramesPath { get; set; }

		public SpriteSheetData Sheet { get; set; }

		public string Animation { get; set; }

		public bool? Playing { get; set; }

		public float? SpeedScale { get; set; }

		public int? Frame { get; set; }

		public Vector2Data Position { get; set; }

		public float? RotationDegrees { get; set; }

		public Vector2Data Scale { get; set; }

		public Vector2Data Offset { get; set; }

		public bool? Centered { get; set; }

		public bool? FlipH { get; set; }

		public bool? FlipV { get; set; }

		public bool? Visible { get; set; }

		public int? ZIndex { get; set; }

		public bool? ZAsRelative { get; set; }

		public ColorData Modulate { get; set; }

		public static SpriteData FromDictionary(GodotDictionary dictionary)
		{
			var data = new SpriteData();
			if (dictionary == null)
			{
				return data;
			}

			data.Id = TaccJson.GetString(dictionary, "id");
			data.TexturePath = TaccJson.GetString(dictionary, "texturePath");
			data.FramesPath = TaccJson.GetString(dictionary, "framesPath");
			data.Animation = TaccJson.GetString(dictionary, "animation");
			data.Playing = TaccJson.GetBool(dictionary, "playing");
			data.SpeedScale = TaccJson.GetFloat(dictionary, "speedScale");
			data.Frame = TaccJson.GetInt(dictionary, "frame");
			data.RotationDegrees = TaccJson.GetFloat(dictionary, "rotationDegrees");
			data.Centered = TaccJson.GetBool(dictionary, "centered");
			data.FlipH = TaccJson.GetBool(dictionary, "flipH");
			data.FlipV = TaccJson.GetBool(dictionary, "flipV");
			data.Visible = TaccJson.GetBool(dictionary, "visible");
			data.ZIndex = TaccJson.GetInt(dictionary, "zIndex");
			data.ZAsRelative = TaccJson.GetBool(dictionary, "zAsRelative");

			if (TaccJson.TryGetDictionary(dictionary, "position", out var position))
			{
				data.Position = Vector2Data.FromDictionary(position);
			}

			if (TaccJson.TryGetDictionary(dictionary, "scale", out var scale))
			{
				data.Scale = Vector2Data.FromDictionary(scale);
			}

			if (TaccJson.TryGetDictionary(dictionary, "offset", out var offset))
			{
				data.Offset = Vector2Data.FromDictionary(offset);
			}

			if (TaccJson.TryGetDictionary(dictionary, "sheet", out var sheet))
			{
				data.Sheet = SpriteSheetData.FromDictionary(sheet);
			}

			if (TaccJson.TryGetDictionary(dictionary, "modulate", out var modulate))
			{
				data.Modulate = ColorData.FromDictionary(modulate);
			}

			return data;
		}
	}

	public class SpriteSheetData
	{
		public string Path { get; set; }

		public Vector2Data FrameSize { get; set; }

		public int? Row { get; set; }

		public int? Start { get; set; }

		public int? Count { get; set; }

		public bool? Loop { get; set; }

		public float? Fps { get; set; }

		public static SpriteSheetData FromDictionary(GodotDictionary dictionary)
		{
			var data = new SpriteSheetData();
			if (dictionary == null)
			{
				return data;
			}

			data.Path = TaccJson.GetString(dictionary, "path");
			data.Row = TaccJson.GetInt(dictionary, "row");
			data.Start = TaccJson.GetInt(dictionary, "start");
			data.Count = TaccJson.GetInt(dictionary, "count");
			data.Loop = TaccJson.GetBool(dictionary, "loop");
			data.Fps = TaccJson.GetFloat(dictionary, "fps");

			if (TaccJson.TryGetDictionary(dictionary, "frameSize", out var frameSize))
			{
				data.FrameSize = Vector2Data.FromDictionary(frameSize);
			}

			return data;
		}
	}

	public class ColorData
	{
		public float R { get; set; } = 1f;

		public float G { get; set; } = 1f;

		public float B { get; set; } = 1f;

		public float A { get; set; } = 1f;

		public static ColorData FromDictionary(GodotDictionary dictionary)
		{
			var data = new ColorData();
			if (dictionary == null)
			{
				return data;
			}

			if (TaccJson.TryGetFloat(dictionary, "r", out var r))
			{
				data.R = r;
			}

			if (TaccJson.TryGetFloat(dictionary, "g", out var g))
			{
				data.G = g;
			}

			if (TaccJson.TryGetFloat(dictionary, "b", out var b))
			{
				data.B = b;
			}

			if (TaccJson.TryGetFloat(dictionary, "a", out var a))
			{
				data.A = a;
			}

			return data;
		}

		public Color ToColor()
		{
			return new Color(R, G, B, A);
		}
	}
}

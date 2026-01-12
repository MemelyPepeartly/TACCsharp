using Godot;
using System.Collections.Generic;
using TACCsharp.TACC.Serialization;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TACCsharp.TACC.Models
{
	public class BackgroundData
	{
		public string Mode { get; set; }

		public string ImagePath { get; set; }

		public string ScaleMode { get; set; }

		public List<BackgroundLayerData> Layers { get; set; }

		public Vector2Data ScrollOffset { get; set; }

		public Vector2Data ScrollBaseOffset { get; set; }

		public Vector2Data ScrollBaseScale { get; set; }

		public bool? IgnoreCameraZoom { get; set; }

		public static BackgroundData FromDictionary(GodotDictionary dictionary)
		{
			if (dictionary == null)
			{
				return null;
			}

			var data = new BackgroundData
			{
				Mode = TaccJson.GetString(dictionary, "mode"),
				ImagePath = TaccJson.GetString(dictionary, "imagePath"),
				ScaleMode = TaccJson.GetString(dictionary, "scaleMode"),
				IgnoreCameraZoom = TaccJson.GetBool(dictionary, "ignoreCameraZoom")
			};

			if (TaccJson.TryGetDictionary(dictionary, "scrollOffset", out var scrollOffset))
			{
				data.ScrollOffset = Vector2Data.FromDictionary(scrollOffset);
			}

			if (TaccJson.TryGetDictionary(dictionary, "scrollBaseOffset", out var scrollBaseOffset))
			{
				data.ScrollBaseOffset = Vector2Data.FromDictionary(scrollBaseOffset);
			}

			if (TaccJson.TryGetDictionary(dictionary, "scrollBaseScale", out var scrollBaseScale))
			{
				data.ScrollBaseScale = Vector2Data.FromDictionary(scrollBaseScale);
			}

			if (TaccJson.TryGetArray(dictionary, "layers", out var layers))
			{
				var parsedLayers = new List<BackgroundLayerData>();
				foreach (Variant entry in layers)
				{
					if (entry.VariantType != Variant.Type.Dictionary)
					{
						continue;
					}

					var layer = BackgroundLayerData.FromDictionary(entry.AsGodotDictionary());
					if (layer != null)
					{
						parsedLayers.Add(layer);
					}
				}

				data.Layers = parsedLayers;
			}

			return data;
		}
	}

	public class BackgroundLayerData
	{
		public string Name { get; set; }

		public string ImagePath { get; set; }

		public Vector2Data MotionScale { get; set; }

		public Vector2Data MotionOffset { get; set; }

		public Vector2Data MotionMirroring { get; set; }

		public bool? RepeatX { get; set; }

		public bool? RepeatY { get; set; }

		public Vector2Data Scale { get; set; }

		public int? ZIndex { get; set; }

		public static BackgroundLayerData FromDictionary(GodotDictionary dictionary)
		{
			if (dictionary == null)
			{
				return null;
			}

			var data = new BackgroundLayerData
			{
				Name = TaccJson.GetString(dictionary, "name"),
				ImagePath = TaccJson.GetString(dictionary, "imagePath"),
				RepeatX = TaccJson.GetBool(dictionary, "repeatX"),
				RepeatY = TaccJson.GetBool(dictionary, "repeatY"),
				ZIndex = TaccJson.GetInt(dictionary, "zIndex")
			};

			if (TaccJson.TryGetDictionary(dictionary, "motionScale", out var motionScale))
			{
				data.MotionScale = Vector2Data.FromDictionary(motionScale);
			}

			if (TaccJson.TryGetDictionary(dictionary, "motionOffset", out var motionOffset))
			{
				data.MotionOffset = Vector2Data.FromDictionary(motionOffset);
			}

			if (TaccJson.TryGetDictionary(dictionary, "motionMirroring", out var motionMirroring))
			{
				data.MotionMirroring = Vector2Data.FromDictionary(motionMirroring);
			}

			if (TaccJson.TryGetDictionary(dictionary, "scale", out var scale))
			{
				data.Scale = Vector2Data.FromDictionary(scale);
			}

			return data;
		}
	}

	public class Vector2Data
	{
		public float X { get; set; }

		public float Y { get; set; }

		public static Vector2Data FromDictionary(GodotDictionary dictionary)
		{
			if (dictionary == null)
			{
				return null;
			}

			var data = new Vector2Data();
			if (TaccJson.TryGetFloat(dictionary, "x", out var x))
			{
				data.X = x;
			}

			if (TaccJson.TryGetFloat(dictionary, "y", out var y))
			{
				data.Y = y;
			}

			return data;
		}

		public Vector2 ToVector2()
		{
			return new Vector2(X, Y);
		}
	}
}

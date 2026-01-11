using Godot;
using System.Collections.Generic;
using TACCsharp.TACC.Serialization;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TACCsharp.TACC.Models
{
	public struct SceneData
	{
		public string Character { get; set; }
		public string Dialogue { get; set; }
		public string Portrait { get; set; }
		public float PortraitWidth { get; set; }
		public float PortraitHeight { get; set; }
		public string Background { get; set; }
		public float Duration { get; set; }

		public static SceneData FromDictionary(GodotDictionary dictionary)
		{
			var data = new SceneData();
			if (dictionary == null)
			{
				return data;
			}

			data.Character = TaccJson.GetString(dictionary, "character");
			data.Dialogue = TaccJson.GetString(dictionary, "dialogue");
			data.Portrait = TaccJson.GetString(dictionary, "portrait");
			data.Background = TaccJson.GetString(dictionary, "background");

			if (TaccJson.TryGetFloat(dictionary, "portrait_width", out var portraitWidth))
			{
				data.PortraitWidth = portraitWidth;
			}

			if (TaccJson.TryGetFloat(dictionary, "portrait_height", out var portraitHeight))
			{
				data.PortraitHeight = portraitHeight;
			}

			if (TaccJson.TryGetFloat(dictionary, "duration", out var duration))
			{
				data.Duration = duration;
			}

			return data;
		}
	}

	public struct CutsceneData
	{
		public string CutsceneName { get; set; }
		public SceneData[] Scenes { get; set; }

		public static CutsceneData FromDictionary(GodotDictionary dictionary)
		{
			var data = new CutsceneData();
			if (dictionary == null)
			{
				return data;
			}

			string cutsceneName = TaccJson.GetString(dictionary, "cutscene_name");
			if (string.IsNullOrWhiteSpace(cutsceneName))
			{
				cutsceneName = TaccJson.GetString(dictionary, "cutsceneName");
			}

			data.CutsceneName = cutsceneName;

			if (TaccJson.TryGetArray(dictionary, "scenes", out var scenesArray))
			{
				var scenes = new List<SceneData>();
				foreach (Variant entry in scenesArray)
				{
					if (entry.VariantType != Variant.Type.Dictionary)
					{
						continue;
					}

					scenes.Add(SceneData.FromDictionary(entry.AsGodotDictionary()));
				}

				data.Scenes = scenes.ToArray();
			}

			return data;
		}
	}
}

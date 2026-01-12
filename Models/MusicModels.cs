using TACCsharp.TACC.Serialization;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TACCsharp.TACC.Models
{
	public class MusicData
	{
		public string TrackPath { get; set; }

		public float? VolumeDb { get; set; }

		public float? PitchScale { get; set; }

		public string Bus { get; set; }

		public bool? Loop { get; set; }

		public bool? Autoplay { get; set; }

		public static MusicData FromDictionary(GodotDictionary dictionary)
		{
			if (dictionary == null)
			{
				return null;
			}

			return new MusicData
			{
				TrackPath = TaccJson.GetString(dictionary, "trackPath"),
				VolumeDb = TaccJson.GetFloat(dictionary, "volumeDb"),
				PitchScale = TaccJson.GetFloat(dictionary, "pitchScale"),
				Bus = TaccJson.GetString(dictionary, "bus"),
				Loop = TaccJson.GetBool(dictionary, "loop"),
				Autoplay = TaccJson.GetBool(dictionary, "autoplay")
			};
		}
	}
}

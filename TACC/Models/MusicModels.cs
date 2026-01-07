using Newtonsoft.Json;

namespace TACCsharp.TACC.Models
{
	public class MusicData
	{
		[JsonProperty("trackPath")]
		public string TrackPath { get; set; }

		[JsonProperty("volumeDb")]
		public float? VolumeDb { get; set; }

		[JsonProperty("pitchScale")]
		public float? PitchScale { get; set; }

		[JsonProperty("bus")]
		public string Bus { get; set; }

		[JsonProperty("loop")]
		public bool? Loop { get; set; }

		[JsonProperty("autoplay")]
		public bool? Autoplay { get; set; }
	}
}

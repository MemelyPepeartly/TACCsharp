using System.Collections.Generic;
using Newtonsoft.Json;

namespace TACCsharp.TACC.Models
{
    public class HudData
    {
        [JsonProperty("elements")]
        public List<HudElementData> Elements { get; set; }
    }

    public class HudElementData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        [JsonProperty("min")]
        public float? Min { get; set; }

        [JsonProperty("max")]
        public float? Max { get; set; }

        [JsonProperty("value")]
        public float? Value { get; set; }

        [JsonProperty("minWidth")]
        public float? MinWidth { get; set; }

        [JsonProperty("minHeight")]
        public float? MinHeight { get; set; }

        [JsonProperty("visible")]
        public bool? Visible { get; set; }
    }
}

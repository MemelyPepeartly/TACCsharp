using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TACCsharp.TACC.Models
{
    // Data classes for deserialization
    public class MapData
    {
        [JsonProperty("imagePath")]
        public string ImagePath { get; set; }

        [JsonProperty("waypoints")]
        public List<Waypoint> Waypoints { get; set; }
    }
    public class Waypoint
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        public Vector2 Position => new Vector2(X, Y);
    }
}

using Godot;
using System.Collections.Generic;
using TACCsharp.TACC.Serialization;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TACCsharp.TACC.Models
{
    // Data classes for deserialization
    public class MapData
    {
        public string ImagePath { get; set; }

        public List<Waypoint> Waypoints { get; set; }

        public static MapData FromDictionary(GodotDictionary dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            var data = new MapData
            {
                ImagePath = TaccJson.GetString(dictionary, "imagePath")
            };

            if (TaccJson.TryGetArray(dictionary, "waypoints", out var waypointsArray))
            {
                var waypoints = new List<Waypoint>();
                foreach (Variant entry in waypointsArray)
                {
                    if (entry.VariantType != Variant.Type.Dictionary)
                    {
                        continue;
                    }

                    waypoints.Add(Waypoint.FromDictionary(entry.AsGodotDictionary()));
                }

                data.Waypoints = waypoints;
            }

            return data;
        }
    }
    public class Waypoint
    {
        public string Id { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public string Description { get; set; }

        public string IconPath { get; set; }

        public Vector2 Position => new Vector2(X, Y);

        public static Waypoint FromDictionary(GodotDictionary dictionary)
        {
            var data = new Waypoint();
            if (dictionary == null)
            {
                return data;
            }

            data.Id = TaccJson.GetString(dictionary, "id");
            data.Description = TaccJson.GetString(dictionary, "description");
            data.IconPath = TaccJson.GetString(dictionary, "iconPath");

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
    }
}

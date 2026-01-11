using Godot;
using System.Collections.Generic;
using TACCsharp.TACC.Serialization;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TACCsharp.TACC.Models
{
    public class HudData
    {
        public List<HudElementData> Elements { get; set; }

        public static HudData FromDictionary(GodotDictionary dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            var data = new HudData();
            if (TaccJson.TryGetArray(dictionary, "elements", out var elementsArray))
            {
                var elements = new List<HudElementData>();
                foreach (Variant entry in elementsArray)
                {
                    if (entry.VariantType != Variant.Type.Dictionary)
                    {
                        continue;
                    }

                    elements.Add(HudElementData.FromDictionary(entry.AsGodotDictionary()));
                }

                data.Elements = elements;
            }

            return data;
        }
    }

    public class HudElementData
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Anchor { get; set; }

        public string Text { get; set; }

        public string IconPath { get; set; }

        public float? Min { get; set; }

        public float? Max { get; set; }

        public float? Value { get; set; }

        public float? MinWidth { get; set; }

        public float? MinHeight { get; set; }

        public bool? Visible { get; set; }

        public static HudElementData FromDictionary(GodotDictionary dictionary)
        {
            var data = new HudElementData();
            if (dictionary == null)
            {
                return data;
            }

            data.Id = TaccJson.GetString(dictionary, "id");
            data.Type = TaccJson.GetString(dictionary, "type");
            data.Anchor = TaccJson.GetString(dictionary, "anchor");
            data.Text = TaccJson.GetString(dictionary, "text");
            data.IconPath = TaccJson.GetString(dictionary, "iconPath");
            data.Min = TaccJson.GetFloat(dictionary, "min");
            data.Max = TaccJson.GetFloat(dictionary, "max");
            data.Value = TaccJson.GetFloat(dictionary, "value");
            data.MinWidth = TaccJson.GetFloat(dictionary, "minWidth");
            data.MinHeight = TaccJson.GetFloat(dictionary, "minHeight");
            data.Visible = TaccJson.GetBool(dictionary, "visible");

            return data;
        }
    }
}

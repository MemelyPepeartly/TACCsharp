using Godot;
using System;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TACCsharp.TACC.Serialization
{
	public static class TaccJson
	{
		public static bool TryParseDictionary(string json, out GodotDictionary dictionary, out string error)
		{
			dictionary = null;
			error = null;

			Variant parsed;
			try
			{
				parsed = Json.ParseString(json);
			}
			catch (Exception ex)
			{
				error = $"Invalid JSON: {ex.Message}";
				return false;
			}

			if (parsed.VariantType == Variant.Type.Nil)
			{
				error = "Invalid JSON content.";
				return false;
			}

			if (parsed.VariantType != Variant.Type.Dictionary)
			{
				error = "Expected a JSON object.";
				return false;
			}

			dictionary = parsed.AsGodotDictionary();
			return true;
		}

		public static bool TryGetDictionary(GodotDictionary dictionary, string key, out GodotDictionary value)
		{
			value = null;
			if (dictionary == null || !dictionary.TryGetValue(key, out var raw))
			{
				return false;
			}

			if (raw.VariantType != Variant.Type.Dictionary)
			{
				return false;
			}

			value = raw.AsGodotDictionary();
			return true;
		}

		public static bool TryGetArray(GodotDictionary dictionary, string key, out GodotArray value)
		{
			value = null;
			if (dictionary == null || !dictionary.TryGetValue(key, out var raw))
			{
				return false;
			}

			if (raw.VariantType != Variant.Type.Array)
			{
				return false;
			}

			value = raw.AsGodotArray();
			return true;
		}

		public static bool TryGetString(GodotDictionary dictionary, string key, out string value)
		{
			value = null;
			if (dictionary == null || !dictionary.TryGetValue(key, out var raw))
			{
				return false;
			}

			if (raw.VariantType != Variant.Type.String)
			{
				return false;
			}

			value = raw.AsString();
			return true;
		}

		public static bool TryGetBool(GodotDictionary dictionary, string key, out bool value)
		{
			value = false;
			if (dictionary == null || !dictionary.TryGetValue(key, out var raw))
			{
				return false;
			}

			if (raw.VariantType != Variant.Type.Bool)
			{
				return false;
			}

			value = raw.AsBool();
			return true;
		}

		public static bool TryGetFloat(GodotDictionary dictionary, string key, out float value)
		{
			value = 0f;
			if (dictionary == null || !dictionary.TryGetValue(key, out var raw))
			{
				return false;
			}

			switch (raw.VariantType)
			{
				case Variant.Type.Float:
					value = (float)raw.AsDouble();
					return true;
				case Variant.Type.Int:
					value = raw.AsInt64();
					return true;
				default:
					return false;
			}
		}

		public static bool TryGetInt(GodotDictionary dictionary, string key, out int value)
		{
			value = 0;
			if (dictionary == null || !dictionary.TryGetValue(key, out var raw))
			{
				return false;
			}

			switch (raw.VariantType)
			{
				case Variant.Type.Int:
					value = (int)raw.AsInt64();
					return true;
				case Variant.Type.Float:
					value = (int)raw.AsDouble();
					return true;
				default:
					return false;
			}
		}

		public static string GetString(GodotDictionary dictionary, string key)
		{
			return TryGetString(dictionary, key, out var value) ? value : null;
		}

		public static bool? GetBool(GodotDictionary dictionary, string key)
		{
			return TryGetBool(dictionary, key, out var value) ? value : null;
		}

		public static float? GetFloat(GodotDictionary dictionary, string key)
		{
			return TryGetFloat(dictionary, key, out var value) ? value : null;
		}

		public static int? GetInt(GodotDictionary dictionary, string key)
		{
			return TryGetInt(dictionary, key, out var value) ? value : null;
		}
	}
}

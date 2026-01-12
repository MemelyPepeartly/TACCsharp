using Godot;
using Json.Schema;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TACCsharp.TACC.Serialization
{
	public static class TaccSchemaValidator
	{
		private static readonly Dictionary<string, JsonSchema> SchemaCache = new(StringComparer.OrdinalIgnoreCase);

		public static bool TryValidate(string schemaPath, string jsonContent, out string error)
		{
			error = null;
			if (string.IsNullOrWhiteSpace(schemaPath))
			{
				error = "Schema path is empty.";
				return false;
			}

			if (!FileAccess.FileExists(schemaPath))
			{
				error = $"Schema file not found: {schemaPath}";
				return false;
			}

			if (!TryGetSchema(schemaPath, out var schema, out error))
			{
				return false;
			}

			JsonDocument instance;
			try
			{
				instance = JsonDocument.Parse(jsonContent);
			}
			catch (Exception ex)
			{
				error = $"Invalid JSON: {ex.Message}";
				return false;
			}

			EvaluationResults results;
			try
			{
				var options = new EvaluationOptions
				{
					OutputFormat = OutputFormat.List
				};
				results = schema.Evaluate(instance.RootElement, options);
			}
			catch (Exception ex)
			{
				error = $"Schema evaluation failed: {ex.Message}";
				return false;
			}
			finally
			{
				instance.Dispose();
			}

			if (results.IsValid)
			{
				return true;
			}

			error = FormatErrors(results);
			return false;
		}

		private static bool TryGetSchema(string schemaPath, out JsonSchema schema, out string error)
		{
			error = null;
			if (SchemaCache.TryGetValue(schemaPath, out schema))
			{
				return true;
			}

			try
			{
				using var file = FileAccess.Open(schemaPath, FileAccess.ModeFlags.Read);
				string schemaText = file.GetAsText();
				schema = JsonSchema.FromText(schemaText);
				SchemaCache[schemaPath] = schema;
				return true;
			}
			catch (Exception ex)
			{
				error = $"Failed to load schema '{schemaPath}': {ex.Message}";
				schema = null;
				return false;
			}
		}

		private static string FormatErrors(EvaluationResults results)
		{
			if (results == null)
			{
				return "JSON does not match schema.";
			}

			try
			{
				results.ToList();
				return JsonSerializer.Serialize(results);
			}
			catch
			{
				return "JSON does not match schema.";
			}
		}
	}
}

using Godot;
using System;
using Newtonsoft.Json;

public partial class CutsceneLeaf : Node
{
	private struct SceneData
	{
		public string Character { get; set; }
		public string Dialogue { get; set; }
		public string Portrait { get; set; }
		public string Background { get; set; }
		public float Duration { get; set; }
	}

	private struct CutsceneData
	{
		public string CutsceneName { get; set; }
		public SceneData[] Scenes { get; set; }
	}

	public void LoadCutscene(string jsonPath)
	{
		string jsonText = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read).GetAsText();
		CutsceneData cutscene = JsonConvert.DeserializeObject<CutsceneData>(jsonText);

		GD.Print($"Loaded cutscene: {cutscene.CutsceneName}");
		foreach (var scene in cutscene.Scenes)
		{
			GD.Print($"Scene - Character: {scene.Character}, Dialogue: {scene.Dialogue}");
		}
	}
}

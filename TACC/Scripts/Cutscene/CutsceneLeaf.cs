using Godot;
using System;
using Newtonsoft.Json;
using TACCsharp.TACC.Models;

public partial class CutsceneLeaf : Node
{
	public Action<string, SceneData> OnSceneChanged; // Event for when a scene changes
	public Action OnCutsceneEnded; // Event for when the cutscene ends

	private SceneData[] _scenes;
	private int _currentSceneIndex = -1;

	public void LoadCutscene(string jsonPath)
	{
		string jsonText = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read).GetAsText();
		CutsceneData cutscene = JsonConvert.DeserializeObject<CutsceneData>(jsonText);

		GD.Print($"Loaded cutscene: {cutscene.CutsceneName}");
		_scenes = cutscene.Scenes;
		_currentSceneIndex = -1;

		if (_scenes == null || _scenes.Length == 0)
		{
			GD.PrintErr("Cutscene has no scenes to play.");
			OnCutsceneEnded?.Invoke();
			return;
		}

		AdvanceScene(); // Start the cutscene
	}

	public void AdvanceScene()
	{
		GD.Print("AdvanceScene called!");

		_currentSceneIndex++;

		if (_currentSceneIndex < _scenes.Length)
		{
			var currentScene = _scenes[_currentSceneIndex];
			OnSceneChanged?.Invoke($"Scene {_currentSceneIndex + 1}", currentScene); // Notify listeners
		}
		else
		{
			GD.Print("Cutscene ended.");
			OnCutsceneEnded?.Invoke(); // Notify listeners
		}
	}
}

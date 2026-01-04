using Godot;
using System;
using Newtonsoft.Json;
using TACCsharp.TACC.Models;
using TACCsharp.TACC.State;

public partial class CutsceneLeaf : Node, ILeafStateSource
{
	public string StateKey => LeafStateKeys.Cutscene;
	public event Action<LeafStateSnapshot> StateChanged;

	public Action<string, SceneData> OnSceneChanged; // Event for when a scene changes
	public Action OnCutsceneEnded; // Event for when the cutscene ends

	private SceneData[] _scenes;
	private int _currentSceneIndex = -1;
	private string _cutsceneName;
	private SceneData? _currentScene;
	private bool _cutsceneEnded;

	public override void _Ready()
	{
		AddToGroup(LeafStateGroups.LeafStateSourceGroup);
	}

	public void LoadCutscene(string jsonPath)
	{
		string jsonText = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read).GetAsText();
		CutsceneData cutscene = JsonConvert.DeserializeObject<CutsceneData>(jsonText);

		_cutsceneName = cutscene.CutsceneName;
		_cutsceneEnded = false;
		GD.Print($"Loaded cutscene: {cutscene.CutsceneName}");
		_scenes = cutscene.Scenes;
		_currentSceneIndex = -1;
		_currentScene = null;

		if (_scenes == null || _scenes.Length == 0)
		{
			GD.PrintErr("Cutscene has no scenes to play.");
			_cutsceneEnded = true;
			EmitStateChanged();
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
			_currentScene = currentScene;
			OnSceneChanged?.Invoke($"Scene {_currentSceneIndex + 1}", currentScene); // Notify listeners
		}
		else
		{
			GD.Print("Cutscene ended.");
			_currentScene = null;
			_cutsceneEnded = true;
			OnCutsceneEnded?.Invoke(); // Notify listeners
		}

		EmitStateChanged();
	}

	public LeafStateSnapshot GetStateSnapshot()
	{
		return new CutsceneStateSnapshot
		{
			CutsceneName = _cutsceneName,
			CurrentSceneIndex = _currentSceneIndex,
			TotalScenes = _scenes?.Length ?? 0,
			CurrentScene = _currentScene,
			IsComplete = _cutsceneEnded
		};
	}

	private void EmitStateChanged()
	{
		StateChanged?.Invoke(GetStateSnapshot());
	}
}

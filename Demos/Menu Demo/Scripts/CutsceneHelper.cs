using Godot;
using System;
using TACCsharp.TACC.Models;

public partial class CutsceneHelper : Node
{
	private const string DialogBoxScenePath = "res://Demos/Cutscene Demo/UI/DialogBox.tscn";
	private const string DefaultBackgroundPath = "res://Demos/Assets/Backgrounds/astillon.jpg";

	private Stem _stem;
	private CutsceneLeaf _cutsceneLeaf;
	private DialogBox _dialogBox;
	private BackgroundLeaf _backgroundLeaf;

	public event Action CutsceneFinished;

	public CutsceneHelper(Stem stem)
	{
		_stem = stem;
		InitializeCutscene();
	}

	private void InitializeCutscene()
	{
		// Find the CutsceneLeaf
		_cutsceneLeaf = _stem.GetNode<CutsceneLeaf>("CutsceneLeaf");

		if (_cutsceneLeaf != null)
		{
			// Hook into CutsceneLeaf events
			_cutsceneLeaf.OnSceneChanged += OnSceneChanged;
			_cutsceneLeaf.OnCutsceneEnded += OnCutsceneEnded;

			EnsureBackgroundLeaf();
			EnsureDialogBox();
			SetCutsceneVisible(false);
		}
		else
		{
			GD.PrintErr("CutsceneLeaf not found in Stem.");
		}
	}

	private void EnsureBackgroundLeaf()
	{
		if (_backgroundLeaf != null)
		{
			return;
		}

		_backgroundLeaf = _stem.GetNodeOrNull<BackgroundLeaf>("BackgroundLeaf");
		if (_backgroundLeaf == null)
		{
			GD.PrintErr("BackgroundLeaf not found in Stem.");
			return;
		}

		_backgroundLeaf.SetStaticBackground(DefaultBackgroundPath);
		_backgroundLeaf.SetBackgroundVisible(false);
	}

	private void EnsureDialogBox()
	{
		if (_dialogBox != null)
		{
			return;
		}

		var dialogScene = GD.Load<PackedScene>(DialogBoxScenePath);
		if (dialogScene == null)
		{
			GD.PrintErr($"DialogBox scene not found: {DialogBoxScenePath}");
			return;
		}

		_dialogBox = dialogScene.Instantiate<DialogBox>();
		_dialogBox.Name = "DialogBox";
		_stem.AddChild(_dialogBox);
	}

	private void SetCutsceneVisible(bool isVisible)
	{
		_backgroundLeaf?.SetBackgroundVisible(isVisible);

		if (_dialogBox != null)
		{
			_dialogBox.Visible = isVisible;
		}
	}

	public void SetCutsceneActive(bool isActive)
	{
		if (isActive)
		{
			EnsureBackgroundLeaf();
			EnsureDialogBox();
		}
		else
		{
			_cutsceneLeaf?.ClearCutsceneState();
		}

		ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
		SetCutsceneVisible(isActive);
	}

	private void UpdateBackground(string backgroundPath)
	{
		if (_backgroundLeaf == null)
		{
			return;
		}

		if (string.IsNullOrEmpty(backgroundPath))
		{
			return;
		}

		_backgroundLeaf.SetStaticBackground(backgroundPath);
	}

	public void StartCutscene(string cutscenePath)
	{
		if (_cutsceneLeaf == null)
		{
			GD.PrintErr("CutsceneLeaf is not initialized.");
			return;
		}

		if (string.IsNullOrEmpty(cutscenePath))
		{
			GD.PrintErr("Cutscene path is empty.");
			return;
		}

		SetCutsceneActive(true);

		_cutsceneLeaf.LoadCutscene(cutscenePath);
	}

	private void OnSceneChanged(string sceneName, SceneData sceneData)
	{
		GD.Print($"Scene changed: {sceneName}");
		GD.Print($"Character: {sceneData.Character}, Dialogue: {sceneData.Dialogue}");

		if (_dialogBox == null)
		{
			GD.PrintErr("DialogBox is not initialized.");
			return;
		}

		UpdateBackground(sceneData.Background);

		// Update the dialog box
		Texture2D portrait = null;
		if (!string.IsNullOrEmpty(sceneData.Portrait))
		{
			portrait = GD.Load<Texture2D>(sceneData.Portrait);
		}

		_dialogBox.UpdateDialogue(
			sceneData.Character,
			sceneData.Dialogue,
			portrait,
			sceneData.PortraitWidth,
			sceneData.PortraitHeight);
	}

	private void OnCutsceneEnded()
	{
		GD.Print("Cutscene finished.");
		SetCutsceneActive(false);
		CutsceneFinished?.Invoke();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Enter)
		{
			GD.Print("Enter key pressed!");
			_cutsceneLeaf?.AdvanceScene();
		}
	}
}

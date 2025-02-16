using Godot;
using TACCsharp.TACC.Models;

public partial class CutsceneHelper : Node
{
	private Stem _stem;
	private CutsceneLeaf _cutsceneLeaf;
	private DialogBox _dialogBox;

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

			// Start the cutscene
			_cutsceneLeaf.LoadCutscene("res://Demos/Data/Cutscenes/Prologue.json");

			// Create and add the DialogBox
			_dialogBox = new DialogBox();
			_stem.AddChild(_dialogBox);
		}
		else
		{
			GD.PrintErr("CutsceneLeaf not found in Stem.");
		}
	}

	private void OnSceneChanged(string sceneName, SceneData sceneData)
	{
		GD.Print($"Scene changed: {sceneName}");
		GD.Print($"Character: {sceneData.Character}, Dialogue: {sceneData.Dialogue}");

		// Update the dialog box
		Texture2D portrait = null;
		if (!string.IsNullOrEmpty(sceneData.Portrait))
		{
			portrait = GD.Load<Texture2D>(sceneData.Portrait);
		}

		_dialogBox.UpdateDialogue(sceneData.Character, sceneData.Dialogue, portrait);
	}

	private void OnCutsceneEnded()
	{
		GD.Print("Cutscene finished.");
		_dialogBox.Visible = false; // Hide dialog box
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Enter)
		{
			GD.Print("Enter key pressed!");
			_cutsceneLeaf.AdvanceScene();
		}
	}
}

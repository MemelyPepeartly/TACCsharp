using Godot;

public partial class CutsceneDemo : Node
{
	private CutsceneLeaf _cutsceneLeaf;
	private DialogBox _dialogBox;
	private TextureRect _background;

	public override void _Ready()
	{
		GD.Print($"_Ready() called in CutsceneDemo");
		
		// Reference CutsceneLeaf and DialogBox
		_cutsceneLeaf = GetNode<CutsceneLeaf>("CutsceneLeaf");
		_dialogBox = GetNode<DialogBox>("DialogBox");
		_background = GetNode<TextureRect>("Background");

		// Hook into CutsceneLeaf events
		_cutsceneLeaf.OnSceneChanged += OnSceneChanged;
		_cutsceneLeaf.OnCutsceneEnded += OnCutsceneEnded;;

		// Start the cutscene
		_cutsceneLeaf.LoadCutscene("res://Game/Data/Cutscenes/Prologue.json");
	}

	private void OnSceneChanged(string sceneName, CutsceneLeaf.SceneData sceneData)
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

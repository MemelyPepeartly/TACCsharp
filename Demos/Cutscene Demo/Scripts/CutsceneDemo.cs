using Godot;
using TACCsharp.TACC.Models;

public partial class CutsceneDemo : Node
{
	private CutsceneLeaf _cutsceneLeaf;
	private DialogBox _dialogBox;
	private Sprite2D _background;

	public override void _Ready()
	{
		GD.Print($"_Ready() called in CutsceneDemo");

		// Reference CutsceneLeaf and DialogBox
		_cutsceneLeaf = GetNode<CutsceneLeaf>("CutsceneLeaf");
		_dialogBox = GetNode<DialogBox>("DialogBox");
		_background = GetNode<Sprite2D>("Background");

		// Hook into CutsceneLeaf events
		_cutsceneLeaf.OnSceneChanged += OnSceneChanged;
		_cutsceneLeaf.OnCutsceneEnded += OnCutsceneEnded;

		if (_background != null)
		{
			_background.Centered = true;
			_background.ZIndex = -10;
		}

		var viewport = GetViewport();
		if (viewport != null)
		{
			viewport.SizeChanged += OnViewportSizeChanged;
		}

		// Start the cutscene
		_cutsceneLeaf.LoadCutscene("res://Demos/Data/Cutscenes/Prologue.json");

		UpdateBackgroundLayout();
	}

	private void OnSceneChanged(string sceneName, SceneData sceneData)
	{
		GD.Print($"Scene changed: {sceneName}");
		GD.Print($"Character: {sceneData.Character}, Dialogue: {sceneData.Dialogue}");

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

	private void UpdateBackground(string backgroundPath)
	{
		if (_background == null)
		{
			return;
		}

		if (string.IsNullOrEmpty(backgroundPath))
		{
			return;
		}

		var backgroundTexture = GD.Load<Texture2D>(backgroundPath);
		if (backgroundTexture == null)
		{
			GD.PrintErr($"Background texture not found: {backgroundPath}");
			return;
		}

		_background.Texture = backgroundTexture;
		UpdateBackgroundLayout();
	}

	private void UpdateBackgroundLayout()
	{
		if (_background == null || _background.Texture == null)
		{
			return;
		}

		var viewport = GetViewport();
		if (viewport == null)
		{
			return;
		}

		Vector2 viewportSize = viewport.GetVisibleRect().Size;
		Vector2 textureSize = _background.Texture.GetSize();
		if (textureSize == Vector2.Zero)
		{
			return;
		}

		float scale = Mathf.Max(viewportSize.X / textureSize.X, viewportSize.Y / textureSize.Y);
		_background.Scale = new Vector2(scale, scale);
		_background.Position = viewportSize / 2.0f;
	}

	private void OnViewportSizeChanged()
	{
		UpdateBackgroundLayout();
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

	public override void _ExitTree()
	{
		var viewport = GetViewport();
		if (viewport != null)
		{
			viewport.SizeChanged -= OnViewportSizeChanged;
		}
	}
}

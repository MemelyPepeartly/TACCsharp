using Godot;
using TACCsharp.TACC.Models;

public partial class CutsceneHelper : Node
{
	private const string DialogBoxScenePath = "res://Demos/Cutscene Demo/UI/DialogBox.tscn";
	private const string BackgroundTexturePath = "res://Demos/Assets/Backgrounds/astillon.jpg";

	private Stem _stem;
	private CutsceneLeaf _cutsceneLeaf;
	private DialogBox _dialogBox;
	private Sprite2D _background;

	public CutsceneHelper(Stem stem)
	{
		_stem = stem;
		InitializeCutscene();
	}

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport != null)
		{
			viewport.SizeChanged += OnViewportSizeChanged;
		}

		UpdateBackgroundLayout();
	}

	public override void _ExitTree()
	{
		var viewport = GetViewport();
		if (viewport != null)
		{
			viewport.SizeChanged -= OnViewportSizeChanged;
		}
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

			EnsureBackground();
			EnsureDialogBox();
			SetCutsceneVisible(false);
		}
		else
		{
			GD.PrintErr("CutsceneLeaf not found in Stem.");
		}
	}

	private void EnsureBackground()
	{
		if (_background != null)
		{
			return;
		}

		var backgroundTexture = GD.Load<Texture2D>(BackgroundTexturePath);
		if (backgroundTexture == null)
		{
			GD.PrintErr($"Background texture not found: {BackgroundTexturePath}");
		}

		_background = new Sprite2D
		{
			Name = "Background",
			Texture = backgroundTexture,
			Centered = true,
			ZIndex = -10
		};

		_stem.AddChild(_background);
		UpdateBackgroundLayout();
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
		if (_background != null)
		{
			_background.Visible = isVisible;
		}

		if (_dialogBox != null)
		{
			_dialogBox.Visible = isVisible;
		}
	}

	public void SetCutsceneActive(bool isActive)
	{
		if (isActive)
		{
			EnsureBackground();
			EnsureDialogBox();
		}

		ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
		SetCutsceneVisible(isActive);
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
		SetCutsceneVisible(false);
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

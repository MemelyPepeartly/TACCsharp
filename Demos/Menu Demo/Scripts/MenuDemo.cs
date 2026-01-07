using Godot;
using System;
using TACCsharp.Demos.Menu_Demo.Scripts;

public partial class MenuDemo : Node
{
	private const string MainMenuPath = "res://Demos/Data/Menus/Start.json";
	private const string CutsceneMenuPath = "res://Demos/Data/Menus/CutsceneSelect.json";
	private const string StateMonitorMenuPath = "res://Demos/Data/Menus/StateMonitorDemos.json";
	private const string BackgroundMenuPath = "res://Demos/Data/Menus/BackgroundTests.json";
	private const string MusicMenuPath = "res://Demos/Data/Menus/MusicTests.json";
	private const string SpriteMenuPath = "res://Demos/Data/Menus/SpriteTests.json";
	private const string InDemoMenuPath = "res://Demos/Data/Menus/InDemo.json";
	private const string CutsceneProloguePath = "res://Demos/Data/Cutscenes/Prologue.json";
	private const string CutsceneInterludePath = "res://Demos/Data/Cutscenes/Interlude.json";
	private const string CutsceneFinalePath = "res://Demos/Data/Cutscenes/Finale.json";
	private const string StaticBackgroundPath = "res://Demos/Data/Backgrounds/StaticBackground.json";
	private const string ParallaxClouds1Path = "res://Demos/Data/Backgrounds/ParallaxClouds1.json";
	private const string ParallaxClouds2Path = "res://Demos/Data/Backgrounds/ParallaxClouds2.json";
	private const string ParallaxClouds3Path = "res://Demos/Data/Backgrounds/ParallaxClouds3.json";
	private const string MusicDemo1Path = "res://Demos/Data/Music/DemoTrack1.json";
	private const string MusicDemo2Path = "res://Demos/Data/Music/DemoTrack2.json";
	private const string SpriteDemoStaticPath = "res://Demos/Data/Sprites/SpriteDemo_Static.json";
	private const string SpriteDemoLayeredPath = "res://Demos/Data/Sprites/SpriteDemo_Layered.json";
	private const string SpriteDemoWalkPath = "res://Demos/Data/Sprites/SpriteDemo_Walkcycle.json";
	private const string SpriteDemoBowPath = "res://Demos/Data/Sprites/SpriteDemo_Bow.json";
	private const string SpriteDemoThrustPath = "res://Demos/Data/Sprites/SpriteDemo_Thrust.json";
	private const string SpriteDemoTintPath = "res://Demos/Data/Sprites/SpriteDemo_TintFlip.json";
	private const float ParallaxMouseMaxOffsetX = 200f;
	private const float ParallaxMouseMaxOffsetY = 0f;
	private const float ParallaxMouseSmoothing = 8f;
	private const float ParallaxTrackerSize = 8f;

	private enum DemoState
	{
		None,
		Map,
		Cutscene,
		Hud,
		Background,
		Music,
		Sprite
	}

	private Stem _stem;
	private MenuFactoryLeaf _menuFactory;
	private MapHelper _mapHelper;
	private CutsceneHelper _cutsceneHelper;
	private HudHelper _hudHelper;
	private BackgroundLeaf _backgroundLeaf;
	private MusicLeaf _musicLeaf;
	private SpriteLeaf _spriteLeaf;
	private ColorRect _parallaxTracker;
	private Vector2 _parallaxOffset = Vector2.Zero;
	private StateMonitorVisualizerPanel _stateMonitorVisualizer;
	private DemoState _activeDemo = DemoState.None;

	public override void _Ready()
	{
		GD.Print("Initializing MenuDemo...");

		// Retrieve the Stem node
		_stem = GetNodeOrNull<Stem>("Stem");

		if (_stem == null)
		{
			GD.PrintErr("ERROR: Stem not found.");
			return;
		}

		// Retrieve and configure the MenuFactoryLeaf
		ConfigureMenuFactory();
		ConfigureBackgroundLeaf();
		ConfigureMusicLeaf();
		ShowMainMenu();
	}

	private void ConfigureMenuFactory()
	{
		_menuFactory = _stem.GetNodeOrNull<MenuFactoryLeaf>("CanvasLayer/MenuFactoryLeaf");

		if (_menuFactory == null)
		{
			GD.PrintErr("ERROR: MenuFactoryLeaf not found in Stem.");
			return;
		}
	}

	private void ConfigureBackgroundLeaf()
	{
		_backgroundLeaf = _stem.GetNodeOrNull<BackgroundLeaf>("BackgroundLeaf");

		if (_backgroundLeaf == null)
		{
			GD.PrintErr("ERROR: BackgroundLeaf not found in Stem.");
		}
	}

	private void ConfigureMusicLeaf()
	{
		_musicLeaf = _stem.GetNodeOrNull<MusicLeaf>("MusicLeaf");

		if (_musicLeaf == null)
		{
			GD.PrintErr("ERROR: MusicLeaf not found in Stem.");
		}
	}

	private void ConfigureSpriteLeaf()
	{
		_spriteLeaf = _stem.GetNodeOrNull<SpriteLeaf>("SpriteLeaf");

		if (_spriteLeaf == null)
		{
			GD.PrintErr("ERROR: SpriteLeaf not found in Stem.");
		}
	}

	private void ShowMainMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		_menuFactory.LoadMenu(MainMenuPath);
		_menuFactory.RegisterAction("start_map_demo", StartMapDemo);
		_menuFactory.RegisterAction("start_cutscene_demo", ShowCutsceneMenu);
		_menuFactory.RegisterAction("start_hud_demo", StartHudDemo);
		_menuFactory.RegisterAction("start_background_demo", ShowBackgroundMenu);
		_menuFactory.RegisterAction("start_music_demo", StartMusicDemo);
		_menuFactory.RegisterAction("start_sprite_demo", ShowSpriteMenu);
		_menuFactory.RegisterAction("start_state_monitor_demo", ShowStateMonitorMenu);
		_menuFactory.RegisterAction("exit_game", ExitGame);
		_menuFactory.Visible = true;
	}

	private void ShowBackgroundMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		_menuFactory.LoadMenu(BackgroundMenuPath);
		_menuFactory.RegisterAction("test_background_static", StartStaticBackgroundTest);
		_menuFactory.RegisterAction("test_parallax_clouds_1", StartParallaxClouds1Test);
		_menuFactory.RegisterAction("test_parallax_clouds_2", StartParallaxClouds2Test);
		_menuFactory.RegisterAction("test_parallax_clouds_3", StartParallaxClouds3Test);
		_menuFactory.RegisterAction("back_to_main_menu", ShowMainMenu);
		_menuFactory.Visible = true;
	}

	private void ShowCutsceneMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		_menuFactory.LoadMenu(CutsceneMenuPath);
		_menuFactory.RegisterAction("start_cutscene_prologue", () => StartCutscene(CutsceneProloguePath));
		_menuFactory.RegisterAction("start_cutscene_interlude", () => StartCutscene(CutsceneInterludePath));
		_menuFactory.RegisterAction("start_cutscene_finale", () => StartCutscene(CutsceneFinalePath));
		_menuFactory.RegisterAction("back_to_main_menu", ShowMainMenu);
		_menuFactory.Visible = true;
	}

	private void ShowStateMonitorMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		_menuFactory.LoadMenu(StateMonitorMenuPath);
		_menuFactory.RegisterAction("open_state_visualizer", OpenStateMonitorVisualizer);
		_menuFactory.RegisterAction("back_to_main_menu", ShowMainMenu);
		_menuFactory.Visible = true;
	}

	private void ShowMusicMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		if (_musicLeaf == null)
		{
			ConfigureMusicLeaf();
		}

		_menuFactory.LoadMenu(MusicMenuPath);
		_menuFactory.RegisterAction("music_load_demo_1", () => LoadMusicConfig(MusicDemo1Path));
		_menuFactory.RegisterAction("music_load_demo_2", () => LoadMusicConfig(MusicDemo2Path));
		_menuFactory.RegisterAction("music_play", () => _musicLeaf?.PlayMusic());
		_menuFactory.RegisterAction("music_pause", () => _musicLeaf?.PauseMusic());
		_menuFactory.RegisterAction("music_stop", () => _musicLeaf?.StopMusic());
		_menuFactory.RegisterAction("music_loop_on", () => _musicLeaf?.SetLoop(true));
		_menuFactory.RegisterAction("music_loop_off", () => _musicLeaf?.SetLoop(false));
		_menuFactory.RegisterAction("music_volume_low", () => _musicLeaf?.SetVolumeDb(-6f));
		_menuFactory.RegisterAction("music_volume_normal", () => _musicLeaf?.SetVolumeDb(0f));
		_menuFactory.RegisterAction("back_to_main_menu", ReturnToMainMenu);
		_menuFactory.Visible = true;
	}

	private void ShowSpriteMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		if (_spriteLeaf == null)
		{
			ConfigureSpriteLeaf();
		}

		_menuFactory.LoadMenu(SpriteMenuPath);
		_menuFactory.RegisterAction("sprite_demo_static", () => StartSpriteDemo(SpriteDemoStaticPath));
		_menuFactory.RegisterAction("sprite_demo_layered", () => StartSpriteDemo(SpriteDemoLayeredPath));
		_menuFactory.RegisterAction("sprite_demo_walk", () => StartSpriteDemo(SpriteDemoWalkPath));
		_menuFactory.RegisterAction("sprite_demo_bow", () => StartSpriteDemo(SpriteDemoBowPath));
		_menuFactory.RegisterAction("sprite_demo_thrust", () => StartSpriteDemo(SpriteDemoThrustPath));
		_menuFactory.RegisterAction("sprite_demo_tint", () => StartSpriteDemo(SpriteDemoTintPath));
		_menuFactory.RegisterAction("back_to_main_menu", ReturnToMainMenu);
		_menuFactory.Visible = true;
	}

	private void OpenStateMonitorVisualizer()
	{
		if (_stem == null)
		{
			return;
		}

		if (_stateMonitorVisualizer == null || !_stateMonitorVisualizer.IsInsideTree())
		{
			_stateMonitorVisualizer = new StateMonitorVisualizerPanel();

			var canvasLayer = _stem.GetNodeOrNull<CanvasLayer>("CanvasLayer");
			if (canvasLayer != null)
			{
				canvasLayer.AddChild(_stateMonitorVisualizer);
			}
			else
			{
				_stem.AddChild(_stateMonitorVisualizer);
			}

			var monitor = _stem.GetNodeOrNull<StateMonitorLeaf>("StateMonitorLeaf");
			_stateMonitorVisualizer.AttachMonitor(monitor);
		}

		_stateMonitorVisualizer.Visible = true;
		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}
	}

	private void StartMapDemo()
	{
		GD.Print("Initializing Map Demo...");

		// Hide the menu
		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		// Ensure only one instance of MapHelper is created
		if (_mapHelper == null)
		{
			_mapHelper = new MapHelper(_stem);
			AddChild(_mapHelper);
		}

		_activeDemo = DemoState.Map;
		_hudHelper?.SetHudActive(false);
		_backgroundLeaf?.SetBackgroundVisible(false);
		_backgroundLeaf?.ClearBackground();
		_cutsceneHelper?.SetCutsceneActive(false);
		_mapHelper.SetMapActive(true);
	}

	private void StartCutscene(string cutscenePath)
	{
		GD.Print("Initializing Cutscene Demo...");

		// Hide the menu
		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		_mapHelper?.SetMapActive(false);
		_hudHelper?.SetHudActive(false);
		_backgroundLeaf?.SetBackgroundVisible(false);
		_backgroundLeaf?.ClearBackground();

		// Ensure only one instance of CutsceneHelper is created
		if (_cutsceneHelper == null)
		{
			_cutsceneHelper = new CutsceneHelper(_stem);
			_cutsceneHelper.CutsceneFinished += OnCutsceneDemoEnded;
			AddChild(_cutsceneHelper);
		}

		_activeDemo = DemoState.Cutscene;
		_cutsceneHelper.StartCutscene(cutscenePath);
	}

	private void StartHudDemo()
	{
		GD.Print("Initializing HUD Demo...");

		// Hide the menu
		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		_mapHelper?.SetMapActive(false);
		_cutsceneHelper?.SetCutsceneActive(false);
		_backgroundLeaf?.SetBackgroundVisible(false);
		_backgroundLeaf?.ClearBackground();

		if (_hudHelper == null)
		{
			_hudHelper = new HudHelper(_stem);
			AddChild(_hudHelper);
		}

		_activeDemo = DemoState.Hud;
		_hudHelper.SetHudActive(true);
	}

	private void StartMusicDemo()
	{
		GD.Print("Initializing Music Demo...");

		_mapHelper?.SetMapActive(false);
		_cutsceneHelper?.SetCutsceneActive(false);
		_hudHelper?.SetHudActive(false);
		_backgroundLeaf?.SetBackgroundVisible(false);
		_backgroundLeaf?.ClearBackground();
		ResetParallaxOffset();
		HideParallaxTracker();

		_activeDemo = DemoState.Music;
		ShowMusicMenu();
	}

	private void StartSpriteDemo(string spritePath)
	{
		GD.Print("Initializing Sprite Demo...");

		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		_mapHelper?.SetMapActive(false);
		_cutsceneHelper?.SetCutsceneActive(false);
		_hudHelper?.SetHudActive(false);
		_backgroundLeaf?.SetBackgroundVisible(false);
		_backgroundLeaf?.ClearBackground();
		ResetParallaxOffset();
		HideParallaxTracker();

		SetSpriteActive(true, spritePath);
		_activeDemo = DemoState.Sprite;
	}

	private void StartStaticBackgroundTest()
	{
		StartBackgroundTest(StaticBackgroundPath);
	}

	private void StartParallaxClouds1Test()
	{
		StartBackgroundTest(ParallaxClouds1Path);
	}

	private void StartParallaxClouds2Test()
	{
		StartBackgroundTest(ParallaxClouds2Path);
	}

	private void StartParallaxClouds3Test()
	{
		StartBackgroundTest(ParallaxClouds3Path);
	}

	private void StartBackgroundTest(string backgroundPath)
	{
		GD.Print("Initializing Background Test...");

		if (_menuFactory != null)
		{
			_menuFactory.Visible = false;
		}

		_mapHelper?.SetMapActive(false);
		_cutsceneHelper?.SetCutsceneActive(false);
		_hudHelper?.SetHudActive(false);

		if (_backgroundLeaf == null)
		{
			ConfigureBackgroundLeaf();
		}

		if (_backgroundLeaf == null)
		{
			return;
		}

		_backgroundLeaf.LoadBackground(backgroundPath);
		_backgroundLeaf.SetBackgroundVisible(true);
		ResetParallaxOffset();
		HideParallaxTracker();
		_activeDemo = DemoState.Background;
	}

	private void LoadMusicConfig(string musicPath)
	{
		if (_musicLeaf == null)
		{
			ConfigureMusicLeaf();
		}

		_musicLeaf?.LoadMusic(musicPath);
	}

	private void OnCutsceneDemoEnded()
	{
		StopActiveDemo();
		ShowCutsceneMenu();
	}

	private void ExitGame()
	{
		GD.Print("Exiting game...");
		GetTree().Quit();
	}

	public override void _Input(InputEvent @event)
	{
		// Check if the Escape key is pressed
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			if (_activeDemo == DemoState.None)
			{
				ToggleMenu();
			}
			else
			{
				ToggleDemoMenu();
			}
		}
	}

	public override void _Process(double delta)
	{
		UpdateParallaxMouse((float)delta);
	}

	private void ShowInDemoMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		_menuFactory.LoadMenu(InDemoMenuPath);
		_menuFactory.RegisterAction("back_to_main_menu", ReturnToMainMenu);
		_menuFactory.RegisterAction("exit_game", ExitGame);
		_menuFactory.Visible = true;
	}

	private void ReturnToMainMenu()
	{
		StopActiveDemo();
		ShowMainMenu();
	}

	private void StopActiveDemo()
	{
		bool wasMusicDemo = _activeDemo == DemoState.Music;

		_mapHelper?.SetMapActive(false);
		_cutsceneHelper?.SetCutsceneActive(false);
		_hudHelper?.SetHudActive(false);
		SetSpriteActive(false);
		_backgroundLeaf?.SetBackgroundVisible(false);
		_backgroundLeaf?.ClearBackground();
		if (wasMusicDemo)
		{
			_musicLeaf?.ClearMusic();
		}
		else
		{
			_musicLeaf?.StopMusic();
		}
		ResetParallaxOffset();
		HideParallaxTracker();
		_activeDemo = DemoState.None;
	}

	private void ToggleDemoMenu()
	{
		if (_menuFactory == null)
		{
			return;
		}

		if (_menuFactory.Visible)
		{
			_menuFactory.Visible = false;
			return;
		}

		if (_activeDemo == DemoState.Music)
		{
			ShowMusicMenu();
		}
		else if (_activeDemo == DemoState.Sprite)
		{
			ShowSpriteMenu();
		}
		else
		{
			ShowInDemoMenu();
		}
	}

	private void SetSpriteActive(bool isActive, string spritePath = null)
	{
		if (_spriteLeaf == null)
		{
			if (!isActive)
			{
				return;
			}

			ConfigureSpriteLeaf();
		}

		if (_spriteLeaf == null)
		{
			return;
		}

		if (isActive && !string.IsNullOrWhiteSpace(spritePath))
		{
			_spriteLeaf.LoadSprites(spritePath);
		}
		else if (!isActive)
		{
			_spriteLeaf.ClearSprites();
		}

		_spriteLeaf.Visible = isActive;
		_spriteLeaf.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}

	private void ToggleMenu()
	{
		if (_menuFactory != null)
		{
			_menuFactory.Visible = !_menuFactory.Visible;
		}
	}

	private void UpdateParallaxMouse(float delta)
	{
		if (_activeDemo != DemoState.Background)
		{
			ResetParallaxOffset();
			HideParallaxTracker();
			return;
		}

		if (_backgroundLeaf == null)
		{
			ConfigureBackgroundLeaf();
		}

		if (_backgroundLeaf == null || !_backgroundLeaf.IsParallaxActive)
		{
			ResetParallaxOffset();
			HideParallaxTracker();
			return;
		}

		var viewport = GetViewport();
		if (viewport == null)
		{
			ResetParallaxOffset();
			HideParallaxTracker();
			return;
		}

		Vector2 viewportSize = viewport.GetVisibleRect().Size;
		if (viewportSize.X <= 0 || viewportSize.Y <= 0)
		{
			ResetParallaxOffset();
			HideParallaxTracker();
			return;
		}

		Vector2 mousePosition = viewport.GetMousePosition();
		float normalizedX = (mousePosition.X / viewportSize.X - 0.5f) * 2f;
		float normalizedY = (mousePosition.Y / viewportSize.Y - 0.5f) * 2f;

		var targetOffset = new Vector2(
			normalizedX * ParallaxMouseMaxOffsetX,
			normalizedY * ParallaxMouseMaxOffsetY);

		float smoothing = 1f - Mathf.Exp(-ParallaxMouseSmoothing * delta);
		_parallaxOffset = _parallaxOffset.Lerp(targetOffset, smoothing);
		_backgroundLeaf.SetParallaxScrollOffset(_parallaxOffset);

		EnsureParallaxTracker();
		UpdateParallaxTracker(mousePosition);
	}

	private void ResetParallaxOffset()
	{
		if (_parallaxOffset == Vector2.Zero)
		{
			return;
		}

		_parallaxOffset = Vector2.Zero;
		_backgroundLeaf?.SetParallaxScrollOffset(Vector2.Zero);
	}

	private void EnsureParallaxTracker()
	{
		if (_parallaxTracker != null || _stem == null)
		{
			return;
		}

		_parallaxTracker = _stem.GetNodeOrNull<ColorRect>("CanvasLayer/ParallaxTracker");
		if (_parallaxTracker != null)
		{
			return;
		}

		var canvasLayer = _stem.GetNodeOrNull<CanvasLayer>("CanvasLayer");
		if (canvasLayer == null)
		{
			return;
		}

		_parallaxTracker = new ColorRect
		{
			Name = "ParallaxTracker",
			Color = new Color(1f, 0.95f, 0.2f, 0.9f),
			Size = new Vector2(ParallaxTrackerSize, ParallaxTrackerSize),
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Visible = false,
			ZIndex = 100
		};
		canvasLayer.AddChild(_parallaxTracker);
	}

	private void UpdateParallaxTracker(Vector2 mousePosition)
	{
		if (_parallaxTracker == null)
		{
			return;
		}

		_parallaxTracker.Visible = true;
		_parallaxTracker.Position = mousePosition - new Vector2(ParallaxTrackerSize * 0.5f, ParallaxTrackerSize * 0.5f);
	}

	private void HideParallaxTracker()
	{
		if (_parallaxTracker == null)
		{
			return;
		}

		_parallaxTracker.Visible = false;
	}
}

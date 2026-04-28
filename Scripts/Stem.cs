using Godot;
using System;
using System.Collections.Generic;
using TACCsharp.TACC.Grafting;

public partial class Stem : Node
{
	private const string CoreGraftId = "tacc";

	private readonly TaccLeafRegistry _leafRegistry = new TaccLeafRegistry();
	private readonly HashSet<string> _registeredGraftIds = new HashSet<string>();
	private readonly Dictionary<string, Node> _leavesByKey = new Dictionary<string, Node>();

	private bool _ready;

	[Export]
	public bool LoadDefaultLeaves { get; set; } = true;

	[Export]
	public Godot.Collections.Array<string> GraftScenePaths { get; set; } = new Godot.Collections.Array<string>();

	public IReadOnlyList<TaccLeafRegistration> LeafRegistrations => _leafRegistry.Registrations;
	public IReadOnlyDictionary<string, Node> LeavesByKey => _leavesByKey;

	public override void _Ready()
	{
		GD.Print("Stem is ready!");

		if (LoadDefaultLeaves)
		{
			RegisterDefaultLeaves();
		}

		RegisterChildGrafts();
		LoadConfiguredGraftScenes();
		InstantiateRegisteredLeaves(0);

		_ready = true;
	}

	public bool RegisterGraft(ITaccGraft graft)
	{
		if (graft == null)
		{
			GD.PrintErr("Cannot register null TACC graft.");
			return false;
		}

		var graftId = string.IsNullOrWhiteSpace(graft.GraftId)
			? graft.GetType().Name
			: graft.GraftId;

		if (!_registeredGraftIds.Add(graftId))
		{
			GD.PrintErr($"TACC graft '{graftId}' is already registered.");
			return false;
		}

		var startIndex = _leafRegistry.Registrations.Count;

		try
		{
			graft.RegisterLeaves(_leafRegistry);
		}
		catch (Exception ex)
		{
			_leafRegistry.RemoveFrom(startIndex);
			_registeredGraftIds.Remove(graftId);
			GD.PrintErr($"TACC graft '{graftId}' failed to register leaves: {ex.Message}");
			return false;
		}

		if (_ready)
		{
			InstantiateRegisteredLeaves(startIndex);
		}

		GD.Print($"TACC graft '{graftId}' registered.");
		return true;
	}

	public Node AddGraftScene(string graftScenePath)
	{
		if (string.IsNullOrWhiteSpace(graftScenePath))
		{
			GD.PrintErr("Cannot add TACC graft scene from an empty path.");
			return null;
		}

		var graftScene = GD.Load<PackedScene>(graftScenePath);
		if (graftScene == null)
		{
			GD.PrintErr($"Failed to load TACC graft scene: {graftScenePath}");
			return null;
		}

		var graftNode = graftScene.Instantiate();
		AddChild(graftNode);

		if (!TryRegisterGraftNode(graftNode))
		{
			GD.PrintErr($"TACC graft scene '{graftScenePath}' does not expose an ITaccGraft node.");
		}

		return graftNode;
	}

	public bool TryGetLeaf(string key, out Node leaf)
	{
		return _leavesByKey.TryGetValue(key, out leaf);
	}

	public T GetLeafOrNull<T>(string key) where T : Node
	{
		return _leavesByKey.TryGetValue(key, out var leaf) ? leaf as T : null;
	}

	public void AddLeaf(string leafPath)
	{
		InstantiateLeaf(leafPath, null);
	}

	// Method to add UI leaves inside a CanvasLayer
	public void AddLeafAsUI(string leafPath, string canvasLayerName = "CanvasLayer", int layer = 1)
	{
		InstantiateUiLeaf(leafPath, canvasLayerName, layer, null);
	}

	private void RegisterDefaultLeaves()
	{
		if (_registeredGraftIds.Contains(CoreGraftId))
		{
			return;
		}

		_registeredGraftIds.Add(CoreGraftId);
		_leafRegistry.RegisterLeaf(TaccLeafKeys.Background, "res://addons/tacc/Leaves/BackgroundLeaf.tscn", CoreGraftId);
		_leafRegistry.RegisterLeaf(TaccLeafKeys.Music, "res://addons/tacc/Leaves/MusicLeaf.tscn", CoreGraftId);
		_leafRegistry.RegisterLeaf(TaccLeafKeys.Cutscene, "res://addons/tacc/Leaves/CutsceneLeaf.tscn", CoreGraftId);
		_leafRegistry.RegisterLeaf(TaccLeafKeys.Map, "res://addons/tacc/Leaves/MapLeaf.tscn", CoreGraftId);
		_leafRegistry.RegisterLeaf(TaccLeafKeys.Sprite, "res://addons/tacc/Leaves/SpriteLeaf.tscn", CoreGraftId);
		_leafRegistry.RegisterUiLeaf(TaccLeafKeys.Hud, "res://addons/tacc/Leaves/HudOverlayLeaf.tscn", "CanvasLayer", 1, CoreGraftId);
		_leafRegistry.RegisterUiLeaf(TaccLeafKeys.Menu, "res://addons/tacc/Leaves/MenuFactoryLeaf.tscn", "CanvasLayer", 1, CoreGraftId);
		_leafRegistry.RegisterLeaf(TaccLeafKeys.StateMonitor, "res://addons/tacc/Leaves/StateMonitorLeaf.tscn", CoreGraftId);
	}

	private void RegisterChildGrafts()
	{
		foreach (Node child in GetChildren())
		{
			TryRegisterGraftNode(child);
		}
	}

	private void LoadConfiguredGraftScenes()
	{
		foreach (var graftScenePath in GraftScenePaths)
		{
			AddGraftScene(graftScenePath);
		}
	}

	private bool TryRegisterGraftNode(Node node)
	{
		if (node is ITaccGraft graft)
		{
			return RegisterGraft(graft);
		}

		var registered = false;
		foreach (Node child in node.GetChildren())
		{
			registered |= TryRegisterGraftNode(child);
		}

		return registered;
	}

	private void InstantiateRegisteredLeaves(int startIndex)
	{
		for (var i = startIndex; i < _leafRegistry.Registrations.Count; i++)
		{
			var registration = _leafRegistry.Registrations[i];
			var leaf = registration.IsUi
				? InstantiateUiLeaf(registration.LeafPath, registration.CanvasLayerName, registration.CanvasLayerLayer, registration.Key)
				: InstantiateLeaf(registration.LeafPath, registration.Key);

			if (leaf != null && !string.IsNullOrEmpty(registration.Key))
			{
				_leavesByKey[registration.Key] = leaf;
			}
		}
	}

	private Node InstantiateLeaf(string leafPath, string key)
	{
		var leafScene = GD.Load<PackedScene>(leafPath);
		if (leafScene == null)
		{
			GD.PrintErr($"Failed to load leaf: {leafPath}");
			return null;
		}

		var leafInstance = leafScene.Instantiate();
		AddChild(leafInstance);
		GD.Print(string.IsNullOrEmpty(key)
			? $"Leaf {leafInstance.Name} added to Stem."
			: $"Leaf {leafInstance.Name} ({key}) added to Stem.");
		return leafInstance;
	}

	private Node InstantiateUiLeaf(string leafPath, string canvasLayerName, int layer, string key)
	{
		var leafScene = GD.Load<PackedScene>(leafPath);
		if (leafScene == null)
		{
			GD.PrintErr($"Failed to load leaf: {leafPath}");
			return null;
		}

		var leafInstance = leafScene.Instantiate();
		var canvasLayer = GetNodeOrNull<CanvasLayer>(canvasLayerName);
		if (canvasLayer == null)
		{
			canvasLayer = new CanvasLayer
			{
				Name = canvasLayerName,
				Layer = layer
			};
			AddChild(canvasLayer);
		}

		canvasLayer.AddChild(leafInstance);
		GD.Print(string.IsNullOrEmpty(key)
			? $"UI Leaf {leafInstance.Name} added to Stem inside {canvasLayer.Name}."
			: $"UI Leaf {leafInstance.Name} ({key}) added to Stem inside {canvasLayer.Name}.");
		return leafInstance;
	}
}

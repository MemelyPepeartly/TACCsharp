using Godot;
using System;
using System.Collections.Generic;
using TACCsharp.TACC.State;

public partial class StateMonitorLeaf : Node
{
	public event Action<LeafStateSnapshot> StateUpdated;

	private readonly Dictionary<string, LeafStateSnapshot> _stateByLeaf = new Dictionary<string, LeafStateSnapshot>();
	private readonly HashSet<ILeafStateSource> _sources = new HashSet<ILeafStateSource>();

	public override void _Ready()
	{
		RegisterExistingSources();

		var tree = GetTree();
		if (tree != null)
		{
			tree.NodeAdded += OnNodeAdded;
			tree.NodeRemoved += OnNodeRemoved;
		}
	}

	public override void _ExitTree()
	{
		var tree = GetTree();
		if (tree != null)
		{
			tree.NodeAdded -= OnNodeAdded;
			tree.NodeRemoved -= OnNodeRemoved;
		}

		foreach (var source in _sources)
		{
			source.StateChanged -= OnSourceStateChanged;
		}

		_sources.Clear();
		_stateByLeaf.Clear();
	}

	public LeafStateSnapshot GetState(string leafKey)
	{
		_stateByLeaf.TryGetValue(leafKey, out var snapshot);
		return snapshot;
	}

	public bool TryGetState<T>(string leafKey, out T snapshot) where T : LeafStateSnapshot
	{
		if (_stateByLeaf.TryGetValue(leafKey, out var found) && found is T typed)
		{
			snapshot = typed;
			return true;
		}

		snapshot = null;
		return false;
	}

	public IReadOnlyDictionary<string, LeafStateSnapshot> GetAllStates()
	{
		return _stateByLeaf;
	}

	private void RegisterExistingSources()
	{
		var tree = GetTree();
		if (tree == null)
		{
			return;
		}

		foreach (Node node in tree.GetNodesInGroup(LeafStateGroups.LeafStateSourceGroup))
		{
			TryRegisterSource(node);
		}
	}

	private void OnNodeAdded(Node node)
	{
		TryRegisterSource(node);
	}

	private void OnNodeRemoved(Node node)
	{
		TryUnregisterSource(node);
	}

	private void TryRegisterSource(Node node)
	{
		if (node is not ILeafStateSource source)
		{
			return;
		}

		if (!_sources.Add(source))
		{
			return;
		}

		source.StateChanged += OnSourceStateChanged;

		var snapshot = source.GetStateSnapshot();
		if (snapshot != null)
		{
			_stateByLeaf[snapshot.LeafKey] = snapshot;
			StateUpdated?.Invoke(snapshot);
		}
	}

	private void TryUnregisterSource(Node node)
	{
		if (node is not ILeafStateSource source)
		{
			return;
		}

		if (!_sources.Remove(source))
		{
			return;
		}

		source.StateChanged -= OnSourceStateChanged;
		_stateByLeaf.Remove(source.StateKey);
	}

	private void OnSourceStateChanged(LeafStateSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return;
		}

		_stateByLeaf[snapshot.LeafKey] = snapshot;
		StateUpdated?.Invoke(snapshot);
	}
}

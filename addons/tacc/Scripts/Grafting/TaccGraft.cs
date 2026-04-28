using System;
using System.Collections.Generic;

namespace TACCsharp.TACC.Grafting
{
	public static class TaccLeafKeys
	{
		public const string Background = "tacc:background";
		public const string Music = "tacc:music";
		public const string Cutscene = "tacc:cutscene";
		public const string Map = "tacc:map";
		public const string Sprite = "tacc:sprite";
		public const string Hud = "tacc:hud";
		public const string Menu = "tacc:menu";
		public const string StateMonitor = "tacc:state-monitor";
	}

	public interface ITaccGraft
	{
		string GraftId { get; }
		void RegisterLeaves(TaccLeafRegistry registry);
	}

	public sealed class TaccLeafRegistration
	{
		public TaccLeafRegistration(
			string key,
			string leafPath,
			bool isUi,
			string canvasLayerName,
			int canvasLayerLayer,
			string sourceGraftId
		)
		{
			Key = key;
			LeafPath = leafPath;
			IsUi = isUi;
			CanvasLayerName = canvasLayerName;
			CanvasLayerLayer = canvasLayerLayer;
			SourceGraftId = sourceGraftId;
		}

		public string Key { get; }
		public string LeafPath { get; }
		public bool IsUi { get; }
		public string CanvasLayerName { get; }
		public int CanvasLayerLayer { get; }
		public string SourceGraftId { get; }
	}

	public sealed class TaccLeafRegistry
	{
		private readonly Dictionary<string, TaccLeafRegistration> _registrationsByKey =
			new Dictionary<string, TaccLeafRegistration>();

		private readonly List<TaccLeafRegistration> _registrations = new List<TaccLeafRegistration>();

		public IReadOnlyList<TaccLeafRegistration> Registrations => _registrations;

		public void RegisterLeaf(string key, string leafPath, string sourceGraftId = "game")
		{
			Register(new TaccLeafRegistration(key, leafPath, false, string.Empty, 0, sourceGraftId));
		}

		public void RegisterUiLeaf(
			string key,
			string leafPath,
			string canvasLayerName = "CanvasLayer",
			int canvasLayerLayer = 1,
			string sourceGraftId = "game"
		)
		{
			Register(new TaccLeafRegistration(key, leafPath, true, canvasLayerName, canvasLayerLayer, sourceGraftId));
		}

		public bool Contains(string key)
		{
			return _registrationsByKey.ContainsKey(key);
		}

		internal void Register(TaccLeafRegistration registration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException(nameof(registration));
			}

			if (string.IsNullOrWhiteSpace(registration.Key))
			{
				throw new ArgumentException("Leaf registration key cannot be empty.", nameof(registration));
			}

			if (string.IsNullOrWhiteSpace(registration.LeafPath))
			{
				throw new ArgumentException("Leaf registration path cannot be empty.", nameof(registration));
			}

			if (_registrationsByKey.ContainsKey(registration.Key))
			{
				throw new InvalidOperationException($"A leaf is already registered with key '{registration.Key}'.");
			}

			_registrationsByKey.Add(registration.Key, registration);
			_registrations.Add(registration);
		}

		internal void RemoveFrom(int startIndex)
		{
			for (var i = _registrations.Count - 1; i >= startIndex; i--)
			{
				_registrationsByKey.Remove(_registrations[i].Key);
				_registrations.RemoveAt(i);
			}
		}
	}
}

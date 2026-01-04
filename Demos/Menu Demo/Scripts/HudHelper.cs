using Godot;

namespace TACCsharp.Demos.Menu_Demo.Scripts
{
	public partial class HudHelper : Node
	{
		private const string HudPath = "res://Demos/Data/Hud/HudDemo.json";

		private Stem _stem;
		private HudOverlayLeaf _hudLeaf;
		private float _health = 100.0f;
		private int _gold = 120;
		private float _tickTimer = 0.0f;
		private bool _showPrompt = false;

		public HudHelper(Stem stem)
		{
			_stem = stem;
			InitializeHud();
			SetHudActive(false);
		}

		public override void _Process(double delta)
		{
			if (_hudLeaf == null || ProcessMode == Node.ProcessModeEnum.Disabled)
			{
				return;
			}

			_tickTimer += (float)delta;
			if (_tickTimer < 0.35f)
			{
				return;
			}

			_tickTimer = 0.0f;
			_health -= 2.0f;
			if (_health <= 0.0f)
			{
				_health = 100.0f;
			}

			_gold += 1;
			_showPrompt = !_showPrompt;

			UpdateHud();
		}

		public void SetHudActive(bool isActive)
		{
			if (_hudLeaf == null)
			{
				return;
			}

			if (isActive)
			{
				ResetDemoState();
				_hudLeaf.LoadHud(HudPath);
				UpdateHud();
			}
			else
			{
				_hudLeaf.ClearHud();
			}

			_hudLeaf.Visible = isActive;
			_hudLeaf.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
			ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
		}

		private void InitializeHud()
		{
			_hudLeaf = _stem.GetNodeOrNull<HudOverlayLeaf>("CanvasLayer/HudOverlayLeaf");
			if (_hudLeaf == null)
			{
				GD.PrintErr("HudOverlayLeaf not found in Stem.");
				return;
			}

			_hudLeaf.LoadHud(HudPath);
		}

		private void ResetDemoState()
		{
			_health = 100.0f;
			_gold = 120;
			_tickTimer = 0.0f;
			_showPrompt = false;
		}

		private void UpdateHud()
		{
			if (_hudLeaf == null)
			{
				return;
			}

			_hudLeaf.SetValue("health", _health);
			_hudLeaf.SetText("gold", $"Gold: {_gold}");
			_hudLeaf.SetVisible("prompt", _showPrompt);
		}
	}
}

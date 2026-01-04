using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TACCsharp.Demos.Menu_Demo.Scripts
{
    public partial class MapHelper : Node
    {
        private Stem _stem;
        private MapLeaf _mapLeaf;
        private HudOverlayLeaf _hudLeaf;
        private const string MousePositionHudId = "map_mouse_position";
        private const string MousePositionHudAnchor = "top_left";
        private bool _mapHudActive = false;

        public MapHelper(Stem stem)
        {
            _stem = stem;
            _hudLeaf = _stem.GetNodeOrNull<HudOverlayLeaf>("CanvasLayer/HudOverlayLeaf");
            if (_hudLeaf == null)
            {
                GD.PrintErr("HudOverlayLeaf not found in Stem.");
            }
            ConnectMapLeafSignals();
        }

        public override void _Process(double delta)
        {
            if (!_mapHudActive || _hudLeaf == null || _mapLeaf == null)
            {
                return;
            }

            Vector2 mousePosition = GetViewport().GetMousePosition();
            Vector2 mapPosition = _mapLeaf.ToLocal(mousePosition);

            if (_hudLeaf.EnsureLabel(MousePositionHudId, MousePositionHudAnchor))
            {
                _hudLeaf.SetText(MousePositionHudId, $"Mouse Position: {mapPosition}");
            }
        }

        private void ConnectMapLeafSignals()
        {
            // Find the MapLeaf added to the Stem
            _mapLeaf = _stem.GetNode<MapLeaf>("MapLeaf");

            if (_mapLeaf != null)
            {
                _mapLeaf.Connect(nameof(MapLeaf.MapLoaded), new Callable(this, nameof(OnMapLoaded)));
                _mapLeaf.Connect(nameof(MapLeaf.WaypointClicked), new Callable(this, nameof(OnWaypointClicked)));

                // Load a demo map
                GD.Print("Loading Demo Map");
                _mapLeaf.LoadMap("res://Demos/Data/Maps/Overworld.json");
                GD.Print("MapLeaf signals connected.");
            }
            else
            {
                GD.PrintErr("MapLeaf not found in Stem.");
            }
        }

        private void OnMapLoaded(int waypointCount)
        {
            GD.Print($"Map loaded with {waypointCount} waypoints.");
        }

        private void OnWaypointClicked(string waypointId)
        {
            GD.Print($"Waypoint clicked: {waypointId}");
        }

        public void SetMapActive(bool isActive)
        {
            if (_mapLeaf == null)
            {
                return;
            }

            _mapHudActive = isActive;
            if (_hudLeaf != null)
            {
                if (isActive)
                {
                    _hudLeaf.Visible = true;
                    if (_hudLeaf.EnsureLabel(MousePositionHudId, MousePositionHudAnchor, "Mouse Position:"))
                    {
                        _hudLeaf.SetVisible(MousePositionHudId, true);
                    }
                }
                else
                {
                    _hudLeaf.SetVisible(MousePositionHudId, false);
                }
            }

            _mapLeaf.Visible = isActive;
            _mapLeaf.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
            ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
        }
    }
}

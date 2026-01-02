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

        public MapHelper(Stem stem)
        {
            _stem = stem;
            ConnectMapLeafSignals();
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

            _mapLeaf.Visible = isActive;
            _mapLeaf.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
        }
    }
}

# TACCsharp
Toolkit for Assembling Custom Content (TACC) for Godot 4.5.1 (C#).

## What this is
- Data-driven toolkit that composes content modules ("Leaves") under a runtime root ("Stem").
- Leaves are Godot scenes with C# scripts that load JSON and emit events.
- Grafts let a game register its own project-specific leaves without editing TACC core.
- Demos show menu-driven map and cutscene flows.

## Quick start
1. Open the folder in Godot 4.5.1 with C# support.
2. Run the project. The main scene is `Demos/Menu Demo/Scenes/MenuDemo.tscn`.
3. Use the menu to launch map or cutscene demos.

## Folder map
- `addons/tacc/` plugin root and core toolkit (`Core/`, `Leaves/`, `Models/`, `Scripts/`, `Serialization/`, `Docs/`)
- `Demos/` example scenes, data, assets
- `docs/Demos.md` demo-only paths and assets
- `docs/distribution.md` addon distribution workflow
- `addons/tacc/Docs/architecture.md` architecture and wiring
- `addons/tacc/Docs/schemas/README.md` JSON formats and examples
- `addons/tacc/Docs/schemas/*.schema.json` machine-readable schemas

## Addon
TACC is packaged as a Godot addon under `addons/tacc/`. Copy that folder into another project to reuse it.

## Leaves
- `MenuFactoryLeaf` builds a menu UI from JSON and maps button actions to registered callbacks.
- `HudOverlayLeaf` builds a persistent HUD from JSON and exposes setters for updating elements by id.
- `MapLeaf` loads a map image and waypoints, supports pan/zoom, hover tooltips, and click signals.
- `CutsceneLeaf` loads a sequence of scenes and emits scene-changed and end events.
- `BackgroundLeaf` loads static or parallax backgrounds from JSON.
- `MusicLeaf` plays background music tracks and exposes playback controls.

## Grafts
A graft is a game-specific extension bundle that registers extra leaves with the Stem:

```csharp
using Godot;
using TACCsharp.TACC.Grafting;

public partial class GameGraft : Node, ITaccGraft
{
	public string GraftId => "my-game";

	public void RegisterLeaves(TaccLeafRegistry registry)
	{
		registry.RegisterUiLeaf("my-game:inventory", "res://grafts/InventoryLeaf.tscn", sourceGraftId: GraftId);
	}
}
```

Register grafts by calling `Stem.RegisterGraft`, `Stem.AddGraftScene`, adding a graft node under the Stem scene, or filling `Stem.GraftScenePaths` in the inspector.

## Data entry points
- `Demos/Data/Menus/Start.json`
- `Demos/Data/Maps/Overworld.json`
- `Demos/Data/Cutscenes/Prologue.json`

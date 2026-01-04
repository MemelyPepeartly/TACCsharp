# TACCsharp
Toolkit for Assembling Custom Content (TACC) for Godot 4.3 (C#).

## What this is
- Data-driven toolkit that composes content modules ("Leaves") under a runtime root ("Stem").
- Leaves are Godot scenes with C# scripts that load JSON and emit events.
- Demos show menu-driven map and cutscene flows.

## Quick start
1. Open the folder in Godot 4.3 with C# support.
2. Run the project. The main scene is `Demos/Menu Demo/Scenes/MenuDemo.tscn`.
3. Use the menu to launch map or cutscene demos.

## Folder map
- `TACC/` core toolkit (Stem, Leaves, models)
- `Demos/` example scenes, data, assets
- `docs/architecture.md` architecture and wiring
- `docs/schemas/README.md` JSON formats and examples
- `docs/schemas/*.schema.json` machine-readable schemas

## Leaves
- `MenuFactoryLeaf` builds a menu UI from JSON and maps button actions to registered callbacks.
- `HudOverlayLeaf` builds a persistent HUD from JSON and exposes setters for updating elements by id.
- `MapLeaf` loads a map image and waypoints, supports pan/zoom, hover tooltips, and click signals.
- `CutsceneLeaf` loads a sequence of scenes and emits scene-changed and end events.

## Data entry points
- `Demos/Data/Menus/Start.json`
- `Demos/Data/Maps/Overworld.json`
- `Demos/Data/Cutscenes/Prologue.json`

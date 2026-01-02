# Architecture

## Core pattern
TACCsharp uses a "Stem + Leaves" composition model.
- Stem is the runtime root that instantiates leaf scenes.
- Leaves are feature modules implemented as scenes with C# scripts.
- Content is data-driven via JSON, which the leaves deserialize into models.

## Runtime flow
1. A demo scene loads (for example, `Demos/Menu Demo/Scenes/MenuDemo.tscn`).
2. The scene instances `TACC/Core/Stem.tscn`.
3. `Stem._Ready` loads leaf scenes (cutscene, map, menu UI).
4. Demo scripts find leaves and wire signals/events.
5. Leaves load JSON and emit signals as the user interacts.

## Stem
`TACC/Scripts/Stem.cs`:
- `AddLeaf` instantiates a leaf scene and adds it as a child.
- `AddLeafAsUI` wraps a leaf inside a `CanvasLayer` for UI.

Typical node tree in the menu demo:
```text
MenuDemo (Node)
- Stem (Node)
  - CutsceneLeaf (Node2D)
  - MapLeaf (Node2D)
  - CanvasLayer
    - MenuFactoryLeaf (Control)
```

## Leaves
MenuFactoryLeaf (`TACC/Leaves/MenuFactoryLeaf.tscn`, `TACC/Scripts/Menu/MenuFactoryLeaf.cs`):
- `LoadMenu(jsonPath)` builds buttons from JSON.
- `RegisterAction(actionName, Action)` binds button actions.
- Expects `CenterContainer/VBoxContainer` in the leaf scene.

MapLeaf (`TACC/Leaves/MapLeaf.tscn`, `TACC/Scripts/Map/MapLeaf.cs`):
- `LoadMap(jsonPath)` loads a background image and waypoints.
- Emits `MapLoaded` and `WaypointClicked` signals.
- Supports drag-to-pan, mouse wheel zoom, and hover tooltips.

CutsceneLeaf (`TACC/Leaves/CutsceneLeaf.tscn`, `TACC/Scripts/Cutscene/CutsceneLeaf.cs`):
- `LoadCutscene(jsonPath)` loads a scene list.
- `AdvanceScene()` progresses and fires `OnSceneChanged`.
- `OnCutsceneEnded` fires when scenes are exhausted.

## Demos and wiring
Menu demo (`Demos/Menu Demo/Scenes/MenuDemo.tscn`, `Demos/Menu Demo/Scripts/MenuDemo.cs`):
- Loads menu JSON and registers actions.
- Uses helpers to start map or cutscene demos.
- ESC toggles the menu.

Map demo (`Demos/Map Demo/Scenes/MapDemo.tscn`, `Demos/Map Demo/Scripts/MapDemo.cs`):
- Connects MapLeaf signals and loads `Overworld.json`.

Cutscene demo (`Demos/Cutscene Demo/Scenes/CutsceneDemo.tscn`, `Demos/Cutscene Demo/Scripts/CutsceneDemo.cs`):
- Uses `DialogBox` UI to display dialogue.
- Enter advances the cutscene.

## Data flow
JSON -> `TACC/Models/*` -> Leaf logic -> signals/events -> demo or game code -> UI updates.

## Notes
- The demo cutscene JSON uses `cutscene_name`. `CutsceneData.CutsceneName` is not annotated with `JsonProperty`, so the default deserializer expects `cutsceneName` (or `CutsceneName`). Either update the JSON or add a naming strategy/attribute if you want snake_case.
- Asset paths are `res://` and must exist in the project.

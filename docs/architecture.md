# Architecture

## Core pattern
TACCsharp uses a "Stem + Leaves" composition model.
- Stem is the runtime root that instantiates leaf scenes.
- Leaves are feature modules implemented as scenes with C# scripts.
- Content is data-driven via JSON, which the leaves deserialize into models.

## Runtime flow
1. A demo scene loads (for example, `Demos/Menu Demo/Scenes/MenuDemo.tscn`).
2. The scene instances `addons/tacc/TACC/Core/Stem.tscn`.
3. `Stem._Ready` loads leaf scenes (background, music, cutscene, map, HUD, menu UI, state monitor).
4. Demo scripts find leaves and wire signals/events.
5. Leaves load JSON and emit signals as the user interacts.

## Stem
`addons/tacc/TACC/Scripts/Stem.cs`:
- `AddLeaf` instantiates a leaf scene and adds it as a child.
- `AddLeafAsUI` wraps a leaf inside a `CanvasLayer` for UI.

Typical node tree in the menu demo:
```text
MenuDemo (Node)
- Stem (Node)
  - BackgroundLeaf (Node2D)
  - MusicLeaf (AudioStreamPlayer)
  - CutsceneLeaf (Node2D)
  - MapLeaf (Node2D)
  - StateMonitorLeaf (Node)
  - CanvasLayer
    - HudOverlayLeaf (Control)
    - MenuFactoryLeaf (Control)
```

## Leaves
MenuFactoryLeaf (`addons/tacc/TACC/Leaves/MenuFactoryLeaf.tscn`, `addons/tacc/TACC/Scripts/Menu/MenuFactoryLeaf.cs`):
- `LoadMenu(jsonPath)` builds buttons from JSON.
- `RegisterAction(actionName, Action)` binds button actions.
- Expects `CenterContainer/VBoxContainer` in the leaf scene.

HudOverlayLeaf (`addons/tacc/TACC/Leaves/HudOverlayLeaf.tscn`, `addons/tacc/TACC/Scripts/Hud/HudOverlayLeaf.cs`):
- `LoadHud(jsonPath)` builds a persistent HUD from JSON.
- `SetText`, `SetValue`, and `SetIcon` update elements by id.
- Anchors elements to top/bottom slots (left, center, right).

BackgroundLeaf (`addons/tacc/TACC/Leaves/BackgroundLeaf.tscn`, `addons/tacc/TACC/Scripts/Background/BackgroundLeaf.cs`):
- `LoadBackground(jsonPath)` loads static or parallax backgrounds from JSON.
- `SetStaticBackground` and `SetParallaxBackground` switch modes programmatically.

MusicLeaf (`addons/tacc/TACC/Leaves/MusicLeaf.tscn`, `addons/tacc/TACC/Scripts/Music/MusicLeaf.cs`):
- `LoadMusic(jsonPath)` loads a track definition from JSON.
- `PlayMusic`, `PauseMusic`, and `StopMusic` control playback.

MapLeaf (`addons/tacc/TACC/Leaves/MapLeaf.tscn`, `addons/tacc/TACC/Scripts/Map/MapLeaf.cs`):
- `LoadMap(jsonPath)` loads a background image and waypoints.
- Emits `MapLoaded` and `WaypointClicked` signals.
- Supports drag-to-pan, mouse wheel zoom, and hover tooltips.

CutsceneLeaf (`addons/tacc/TACC/Leaves/CutsceneLeaf.tscn`, `addons/tacc/TACC/Scripts/Cutscene/CutsceneLeaf.cs`):
- `LoadCutscene(jsonPath)` loads a scene list.
- `AdvanceScene()` progresses and fires `OnSceneChanged`.
- `OnCutsceneEnded` fires when scenes are exhausted.

StateMonitorLeaf (`addons/tacc/TACC/Leaves/StateMonitorLeaf.tscn`, `addons/tacc/TACC/Scripts/State/StateMonitorLeaf.cs`):
- Aggregates state snapshots from leaves implementing `ILeafStateSource`.
- Emits `StateUpdated` when any leaf state changes.

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
JSON -> `addons/tacc/TACC/Models/*` -> Leaf logic -> signals/events -> demo or game code -> UI updates.

## Notes
- The demo cutscene JSON uses `cutscene_name` and the model maps it via `JsonProperty`.
- Asset paths are `res://` and must exist in the project.

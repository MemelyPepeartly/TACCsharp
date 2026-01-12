# Architecture

## Core pattern
TACCsharp uses a "Stem + Leaves" composition model.
- Stem is the runtime root that instantiates leaf scenes.
- Leaves are feature modules implemented as scenes with C# scripts.
- Content is data-driven via JSON, which the leaves deserialize into models.

## Runtime flow
1. A game scene loads (for example, your main scene).
2. The scene instances `addons/tacc/Core/Stem.tscn`.
3. `Stem._Ready` loads leaf scenes (background, music, cutscene, map, HUD, menu UI, state monitor).
4. Game scripts find leaves and wire signals/events.
5. Leaves validate JSON against schemas, load it into models, and emit signals as the user interacts.

## Stem
`addons/tacc/Scripts/Stem.cs`:
- `AddLeaf` instantiates a leaf scene and adds it as a child.
- `AddLeafAsUI` wraps a leaf inside a `CanvasLayer` for UI.

Typical node tree (example):
```text
MainScene (Node)
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
MenuFactoryLeaf (`addons/tacc/Leaves/MenuFactoryLeaf.tscn`, `addons/tacc/Scripts/Menu/MenuFactoryLeaf.cs`):
- `LoadMenu(jsonPath)` builds buttons from JSON.
- `RegisterAction(actionName, Action)` binds button actions.
- Expects `CenterContainer/VBoxContainer` in the leaf scene.

HudOverlayLeaf (`addons/tacc/Leaves/HudOverlayLeaf.tscn`, `addons/tacc/Scripts/Hud/HudOverlayLeaf.cs`):
- `LoadHud(jsonPath)` builds a persistent HUD from JSON.
- `SetText`, `SetValue`, and `SetIcon` update elements by id.
- Anchors elements to top/bottom slots (left, center, right).

BackgroundLeaf (`addons/tacc/Leaves/BackgroundLeaf.tscn`, `addons/tacc/Scripts/Background/BackgroundLeaf.cs`):
- `LoadBackground(jsonPath)` loads static or parallax backgrounds from JSON.
- `SetStaticBackground` and `SetParallaxBackground` switch modes programmatically.

MusicLeaf (`addons/tacc/Leaves/MusicLeaf.tscn`, `addons/tacc/Scripts/Music/MusicLeaf.cs`):
- `LoadMusic(jsonPath)` loads a track definition from JSON.
- `PlayMusic`, `PauseMusic`, and `StopMusic` control playback.

MapLeaf (`addons/tacc/Leaves/MapLeaf.tscn`, `addons/tacc/Scripts/Map/MapLeaf.cs`):
- `LoadMap(jsonPath)` loads a background image and waypoints.
- Emits `MapLoaded` and `WaypointClicked` signals.
- Supports drag-to-pan, mouse wheel zoom, and hover tooltips.

CutsceneLeaf (`addons/tacc/Leaves/CutsceneLeaf.tscn`, `addons/tacc/Scripts/Cutscene/CutsceneLeaf.cs`):
- `LoadCutscene(jsonPath)` loads a scene list.
- `AdvanceScene()` progresses and fires `OnSceneChanged`.
- `OnCutsceneEnded` fires when scenes are exhausted.

StateMonitorLeaf (`addons/tacc/Leaves/StateMonitorLeaf.tscn`, `addons/tacc/Scripts/State/StateMonitorLeaf.cs`):
- Aggregates state snapshots from leaves implementing `ILeafStateSource`.
- Emits `StateUpdated` when any leaf state changes.

## Example wiring
- A menu scene loads menu JSON and registers actions for map/cutscene flows.
- A map scene connects `MapLeaf` signals and loads map data.
- A cutscene scene displays dialogue UI and advances on input.

## Data flow
JSON -> `addons/tacc/Models/*` -> Leaf logic -> signals/events -> demo or game code -> UI updates.

## Notes
- The demo cutscene JSON uses `cutscene_name`, which maps to `CutsceneName` in the parsed data.
- Asset paths are `res://` and must exist in the project.

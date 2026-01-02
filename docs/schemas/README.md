# JSON formats
TACCsharp uses JSON files to drive menus, maps, and cutscenes. Schemas live in `docs/schemas/*.schema.json` and are written for JSON Schema draft 2020-12.

## Menu (MenuFactoryLeaf)
Schema: `docs/schemas/menu.schema.json`  
Used by: `MenuFactoryLeaf.LoadMenu(jsonPath)`

Example:
```json
{
  "buttons": [
    { "text": "Start Map Demo", "action": "start_map_demo" },
    { "text": "Exit", "action": "exit_game" }
  ]
}
```
Full example: `Demos/Data/Menus/Start.json`

## Map (MapLeaf)
Schema: `docs/schemas/map.schema.json`  
Used by: `MapLeaf.LoadMap(path)`

Example:
```json
{
  "imagePath": "res://Demos/Assets/Backgrounds/astillon.jpg",
  "waypoints": [
    {
      "id": "snippstone",
      "x": -361,
      "y": 1200,
      "description": "All the holiday deer live here!",
      "iconPath": "res://Demos/Assets/Icons/Snippstone.png"
    }
  ]
}
```
Full example: `Demos/Data/Maps/Overworld.json`

Notes:
- `imagePath` and `iconPath` are Godot resource paths (typically `res://`).
- Waypoint coordinates are in the map node's local space.

## Cutscene (CutsceneLeaf)
Schema: `docs/schemas/cutscene.schema.json`  
Used by: `CutsceneLeaf.LoadCutscene(jsonPath)`

Example:
```json
{
  "cutscene_name": "Prologue",
  "scenes": [
    {
      "character": "Swift Sail",
      "dialogue": "Fortuna...",
      "portrait": "res://Demos/Assets/Portraits/sleepy-swift.png",
      "background": "res://Demos/Assets/Backgrounds/astillon.jpg",
      "duration": 3.5
    }
  ]
}
```
Full example: `Demos/Data/Cutscenes/Prologue.json`

Notes:
- The demo JSON uses `cutscene_name` and the model maps it via `JsonProperty`.
- `background` updates the cutscene background texture in the demo scripts.

# JSON formats
TACCsharp uses JSON files to drive menus, maps, and cutscenes. Schemas live in `addons/tacc/Docs/schemas/*.schema.json` and are written for JSON Schema draft 2020-12.
Leaf loaders validate JSON against these schemas at load time; invalid data logs errors and aborts the load.

## Menu (MenuFactoryLeaf)
Schema: `addons/tacc/Docs/schemas/menu.schema.json`  
Used by: `MenuFactoryLeaf.LoadMenu(jsonPath)`

Example:
```json
{
  "buttons": [
    { "text": "Start Map", "action": "start_map" },
    { "text": "Exit", "action": "exit_game" }
  ]
}
```

## HUD (HudOverlayLeaf)
Schema: `addons/tacc/Docs/schemas/hud.schema.json`  
Used by: `HudOverlayLeaf.LoadHud(jsonPath)`

Example:
```json
{
  "elements": [
    { "id": "player_name", "type": "label", "anchor": "top_left", "text": "Swift Sail" },
    { "id": "health", "type": "progress", "anchor": "top_left", "text": "HP", "min": 0, "max": 100, "value": 75, "minWidth": 220 },
    { "id": "gold", "type": "label", "anchor": "top_right", "text": "Gold: 120" },
    { "id": "prompt", "type": "label", "anchor": "bottom_center", "text": "Press E to interact", "visible": false }
  ]
}
```
Notes:
- Anchors supported: `top_left`, `top_center`, `top_right`, `bottom_left`, `bottom_center`, `bottom_right`.

## Map (MapLeaf)
Schema: `addons/tacc/Docs/schemas/map.schema.json`  
Used by: `MapLeaf.LoadMap(path)`

Example:
```json
{
  "imagePath": "res://Assets/Backgrounds/world-map.png",
  "waypoints": [
    {
      "id": "snippstone",
      "x": -361,
      "y": 1200,
      "description": "All the holiday deer live here!",
      "iconPath": "res://Assets/Icons/waypoint.png"
    }
  ]
}
```

Notes:
- `imagePath` and `iconPath` are Godot resource paths (typically `res://`).
- Waypoint coordinates are in the map node's local space.

## Cutscene (CutsceneLeaf)
Schema: `addons/tacc/Docs/schemas/cutscene.schema.json`  
Used by: `CutsceneLeaf.LoadCutscene(jsonPath)`

Example:
```json
{
  "cutscene_name": "Prologue",
  "scenes": [
    {
      "character": "Swift Sail",
      "dialogue": "Fortuna...",
      "portrait": "res://Assets/Portraits/character.png",
      "portrait_width": 256,
      "portrait_height": 256,
      "background": "res://Assets/Backgrounds/scene.png",
      "duration": 3.5
    }
  ]
}
```

Notes:
- `cutscene_name` maps to `CutsceneName` in the parsed data.
- `background` can be used by your UI script to update the background texture.
- `portrait_width` and `portrait_height` set an explicit render box for the portrait; if omitted, the dialog uses its default size.

## Background (BackgroundLeaf)
Schema: `addons/tacc/Docs/schemas/background.schema.json`  
Used by: `BackgroundLeaf.LoadBackground(jsonPath)`

Static example:
```json
{
  "mode": "static",
  "imagePath": "res://Assets/Backgrounds/scene.png",
  "scaleMode": "cover"
}
```

Parallax example:
```json
{
  "mode": "parallax",
  "scrollBaseScale": { "x": 1, "y": 1 },
  "layers": [
    {
      "name": "Sky",
      "imagePath": "res://Assets/Backgrounds/sky.png",
      "motionScale": { "x": 0.2, "y": 0.2 }
    },
    {
      "name": "Midground",
      "imagePath": "res://Assets/Backgrounds/midground.png",
      "motionScale": { "x": 0.5, "y": 0.5 }
    }
  ]
}
```

Notes:
- `mode` defaults to static unless `layers` is present.
- `motionScale` controls how fast each layer scrolls relative to the camera or `ScrollOffset`.
- `motionMirroring` repeats a layer when set to the texture size in pixels.
- `repeatX` and `repeatY` auto-set `motionMirroring` to the (scaled) texture size so layers tile as they scroll.

## Music (MusicLeaf)
Schema: `addons/tacc/Docs/schemas/music.schema.json`  
Used by: `MusicLeaf.LoadMusic(jsonPath)`

Example:
```json
{
  "trackPath": "res://Assets/Audio/theme.ogg",
  "volumeDb": -6,
  "loop": true,
  "bus": "Music",
  "autoplay": true
}
```

## Sprites (SpriteLeaf)
Schema: `addons/tacc/Docs/schemas/sprite.schema.json`  
Used by: `SpriteLeaf.LoadSprites(jsonPath)`

Example:
```json
{
  "sprites": [
    {
      "id": "hero_idle",
      "sheet": {
        "path": "res://Assets/Sprites/hero.png",
        "frameSize": { "x": 64, "y": 64 },
        "row": 0,
        "start": 0,
        "count": 4,
        "loop": true,
        "fps": 8
      },
      "animation": "idle",
      "playing": true,
      "position": { "x": 320, "y": 180 },
      "scale": { "x": 2, "y": 2 }
    }
  ]
}
```

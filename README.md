# Tank 90 (坦克大战 / Battle City Clone)

A faithful recreation of the classic **Battle City / Tank 1990 (坦克大战)** built in Unity.
The entire game is constructed from code at runtime — the scene only holds a `GameManager`
object and the main camera.

## Requirements

- **Unity `6000.4.10f1`** (Unity 6.4) — open with this exact editor version for guaranteed compatibility.
- Render pipeline: **2D + URP (Universal Render Pipeline)**
- Input: **New Input System only** (`Keyboard.current`); the old Input Manager is disabled.

## How to Run

1. Open the project in Unity `6000.4.10f1`.
2. Open the scene `Assets/Scenes/SampleScene.unity`.
3. Press **Play**, then press **Enter** at the title screen.

## Controls

| Action | Keys |
| ------ | ---- |
| Move   | Arrow keys / `WASD` |
| Fire   | `Space` / `J` (hold to fire continuously) |
| Start / Restart | `Enter` |

## Features

- Player tank with 4 upgrade levels (faster bullets, double fire, steel-piercing shots).
- 4 enemy types — Basic, Fast, Power, Armor — each with distinct stats and AI.
- 3 stages with destructible brick, solid steel, water, trees and ice terrain.
- The eagle base: lose it and it's game over.
- 7 power-ups: Helmet (shield), Timer (freeze enemies), Shovel (fortify base),
  Star (upgrade), Grenade (clear screen), Life (+1), Gun (max upgrade).
- HUD with score, stage, lives icons and a remaining-enemy icon grid.
- Explosions, spawn-stars, score popups and shield effects.

## Project Layout

```
Assets/
  Scenes/SampleScene.unity     # holds only GameManager + Main Camera
  Resources/GeneralSprites.png # spritesheet, loaded by name via SpriteLibrary
  Resources/Miscellaneous.png  # title-screen art
  Scripts/
    Core/      # SpriteLibrary, GameConfig
    Tanks/     # Tank, EnemyTank, EnemyAI, PlayerController, TankMotor, TankFactory
    Combat/    # Bullet
    Terrain/   # WaterTile and tile markers
    Effects/   # SpriteAnim, Effects (explosions, popups)
    Level/     # LevelData, LevelBuilder
    Managers/  # GameManager, EnemySpawner
    Items/     # PowerUp
    UI/        # Hud
docs/
  SPRITE_MAP.md                # spritesheet sub-sprite naming & coordinates
```

## Conventions

- World scale: 1 cell = 1 unit (PPU 16), sub-tile = 0.5 unit. Field is 13×13, centred at (6, 6).
- Layers: `Wall` = 8, `Tank` = 9, `Bullet` = 10.
- Namespace: `Tank90`.
- Sprites live in `Assets/Resources/GeneralSprites.png` and are loaded by name through `SpriteLibrary`
  (full mapping in `docs/SPRITE_MAP.md`).

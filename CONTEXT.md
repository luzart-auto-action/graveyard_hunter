# CONTEXT: Graveyard Hunter - Unity 2021.3.45f2

**Project path:** `/Users/luzart/Documents/graveyard_hunter/`

## Game Overview (from GDD)
- Top-down stealth maze game, player enters dark map, avoids ghosts, collects treasures, finds exit gate
- Ghosts patrol with light cone, detect player then chase. Player hit by ghost light loses HP + gets slowed
- Collect enough treasures to unlock exit gate and win level
- Traps: Spike, Noise, LightBurst. Boosters: SmokeBomb, SpeedBoots, ShadowCloak, GhostVision

## Architecture
- **Namespace:** `GraveyardHunter.*` (Core, Data, Enemy, Player, Level, UI, Input, CameraSystem, Booster, FX)
- **Event-driven:** `EventBus` publish/subscribe pattern
- **ServiceLocator** for global access
- **Odin Inspector** (Sirenix) for editor UI
- **DOTween** for animations
- **NavMesh** runtime baking for ghost AI pathfinding
- **Joystick Pack** (FloatingJoystick) for mobile input

## Key Files

### Core
- `Assets/_Game/Scripts/Core/Enums.cs` — GameState, GhostState, **EnemyType** (Ghost/Werewolf/Monster/Robot), TrapType, BoosterType, CellType, Direction
- `Assets/_Game/Scripts/Core/GameEvents.cs` — All events (PlayerInLightEvent, GhostStateChangedEvent, NoiseTriggeredEvent, LevelLoadedEvent, EscapePhaseStartedEvent, GameStateChangedEvent, etc.)
- `Assets/_Game/Scripts/Core/EventBus.cs` — Pub/sub event system
- `Assets/_Game/Scripts/Core/ServiceLocator.cs` — Global service registry

### Data
- `Assets/_Game/Scripts/Data/GameConfig.cs` (ScriptableObject at `Assets/_Game/ScriptableObjects/Configs/GameConfig.asset`) — All game settings, prefab refs, `List<EnemyTypeData> EnemyTypes`. Methods: `GetEnemyData(EnemyType)`, `GetEnemyPrefab(EnemyType)`.
- `Assets/_Game/Scripts/Data/EnemyTypeData.cs` — Per-enemy-type config: ScanSpeed, ChaseSpeed, LightConeAngle, LightRange, PatrolRadius, ChaseTimeout, EyeColor, ChaseLightColor, ConeLightColor, Prefab ref. Static factory `GetDefault(EnemyType)`.
- `Assets/_Game/Scripts/Data/SkinData.cs` — Player skin data

### Enemy
- `Assets/_Game/Scripts/Enemy/LightGhost.cs` — Main enemy AI controller. NavMeshAgent pathfinding, Scan/Chase states. `Initialize(GameConfig, EnemyTypeData, Transform player, int id)` accepts per-type data. Uses `_patrolRadius`, `_chaseTimeout` from EnemyTypeData. Animator params: "Speed" (float), "IsChasing" (bool). Subscribes to NoiseTriggeredEvent, EscapePhaseStartedEvent, GameStateChangedEvent. Has `_needsFirstDestination` fallback for NavMesh timing.
- `Assets/_Game/Scripts/Enemy/GhostLightCone.cs` — Cone-based player detection (angle + range + wall raycast LOS). `IsPlayerInCone()`, `PlayerDetected` property, `IgnoreWalls` property, `WallLayer` mask.
- `Assets/_Game/Scripts/Enemy/GhostEyes.cs` — Visual eye feedback (yellow=scan, red=chase), DOTween pulse animation.

### Player
- `Assets/_Game/Scripts/Player/PlayerController.cs` — Movement via InputManager joystick. Animator params: "Speed".
- `Assets/_Game/Scripts/Player/PlayerHealth.cs` — HP system, light damage per second
- `Assets/_Game/Scripts/Player/PlayerInventory.cs` — Treasure collection tracking, RequiredTreasures count
- `Assets/_Game/Scripts/Player/PlayerLightSystem.cs` — Player flashlight cone + circle ambient light

### Level
- `Assets/_Game/Scripts/Level/LevelManager.cs` — Maze generation, NavMesh runtime baking (walls marked Not Walkable via NavMeshBuildMarkup), object spawning. `SpawnEnemy(pos, id, EnemyType)` picks prefab + data from GameConfig. Tracks `_wallTransforms` for NavMesh exclusion. `_ghostInstances` list for all enemies.
- `Assets/_Game/Scripts/Level/LevelData.cs` (ScriptableObject) — LevelIndex, LevelName, GridWidth/Height, GhostCount, RequiredTreasures, **AllowedEnemyTypes** (progressive unlock per level), Placements list, MazeSeed, MazeLayout. Method: `GetRandomEnemyType()`.
- `Assets/_Game/Scripts/Level/MazeGenerator.cs` — Procedural maze generation, returns `CellType[,]` grid. Static `GetEmptyCells()`.
- `Assets/_Game/Scripts/Level/TreasurePickup.cs` — OnTriggerEnter collection logic, publishes TreasureCollectedEvent
- `Assets/_Game/Scripts/Level/ExitGate.cs` — Level exit trigger, checks if enough treasures collected

### UI
- `Assets/_Game/Scripts/UI/GameplayUI.cs` — In-game HUD (contains FloatingJoystick child, HP display, treasure count)
- `Assets/_Game/Scripts/UI/MainMenuUI.cs` — Main menu with Play button
- `Assets/_Game/Scripts/UI/UIManager.cs` — Manages UI panel switching (MainMenu, Gameplay, Win, Fail)

### Input
- `Assets/_Game/Scripts/Input/InputManager.cs` — Reads FloatingJoystick input, provides movement Vector2. Has `[SerializeField] _joystick` field.

### Editor Tools
- `Assets/_Game/Scripts/Editor/PrefabCreator.cs` — Menu: GraveyardHunter > Prefab Creator. Creates ALL prefabs (Player, Ghost, **Werewolf, Monster, Robot**, Treasure, ExitGate, Wall, Floor, Traps, Boosters). `AutoAssignGameConfig()` links prefabs to GameConfig. `AutoPopulateEnemyTypes()` fills EnemyTypes list with defaults + prefab refs.
- `Assets/_Game/Scripts/Editor/BugFixer.cs` — Menu: GraveyardHunter > Bug Fixer (1-Click). Single button fixes everything:
  1. Floor: adds BoxCollider for NavMesh baking
  2. Wall: removes NavMeshObstacle (handled by bake markups now)
  3. ALL enemy prefabs: fixes NavMeshAgent areaMask (excludes Not Walkable)
  4. Treasure: adds TreasurePickup script + wires refs
  5. ExitGate: adds ExitGate script + wires refs
  6. Player animation: creates PlayerAnimator.controller (Idle/Run/Win/Sad)
  7. ALL enemy animations: creates shared EnemyAnimator.controller (Idle/Walk/Run with Speed/IsChasing)
  8. Joystick: instantiates FloatingJoystick under GameplayUI, wires to InputManager
  9. EnemyTypes in GameConfig: auto-populates with 4 types + prefab refs
  10. LevelData: auto-sets AllowedEnemyTypes with progressive unlock
- `Assets/_Game/Scripts/Editor/MaterialFactory.cs` — Creates placeholder materials for prefabs

## 4 Enemy Types

| Type     | Scan | Chase | Cone  | Range | Patrol | Timeout | Eye Color | Cone Light Color    |
|----------|------|-------|-------|-------|--------|---------|-----------|---------------------|
| Ghost    | 2.5  | 4.0   | 80d   | 10m   | 15     | 3s      | Yellow    | White-blue          |
| Werewolf | 3.5  | 5.5   | 100d  | 8m    | 20     | 5s      | Orange    | Warm orange         |
| Monster  | 1.8  | 3.0   | 50d   | 14m   | 10     | 6s      | Green     | Green               |
| Robot    | 2.0  | 3.5   | 120d  | 6m    | 12     | 2s      | Blue      | Blue                |

**Progressive unlock:** Level 0 = Ghost only, Level 1 = +Werewolf, Level 2 = +Monster, Level 3+ = all 4.

## Prefab Structure

### Enemy Prefabs (Ghost/Werewolf/Monster/Robot — all same structure)
```
[EnemyName] (root)
  - NavMeshAgent (speed, radius 0.4, height 1.8)
  - LightGhost script
  - VisualRoot/ (Capsule placeholder or character model)
  - Eyes/
    - LeftEye (Point Light, custom color per type)
    - RightEye (Point Light, custom color per type)
    - GhostEyes script
  - LightCone/
    - ConeSpotLight (Spot Light, custom color per type)
    - GhostLightCone script
  - FX_Center (FXSpawnPoint)
```

### Player Prefab
```
Player (root)
  - PlayerController, PlayerHealth, PlayerInventory, PlayerLightSystem
  - VisualRoot/ (character model from characters_5_02)
  - Flashlight/ (Spot Light)
  - AmbientLight/ (Point Light)
  - FX_Center (FXSpawnPoint)
```

## NavMesh Setup
- Runtime baked per level via `NavMeshBuilder.BuildNavMeshData()`
- Sources collected from `_levelRoot` children using `PhysicsColliders`
- Floor = Walkable (area 0), Walls = Not Walkable (area 1) via `NavMeshBuildMarkup` per wall transform
- Agent settings: radius 0.4, height 1.8, climb 0.3
- Enemy NavMeshAgent areaMask = `~(1 << 1)` (excludes Not Walkable)
- Ghost spawn uses `NavMesh.SamplePosition(pos, hit, 5f, walkableAreaMask)` to find valid position

## Bugs Fixed
1. **Walls as NavMesh:** Removed NavMeshObstacle (conflicted with baking). Now uses NavMeshBuildMarkup area=1 (Not Walkable).
2. **Ghost not moving:** Default `_currentState = Scan` + `SetState(Scan)` early-returned. Fixed: call `PickRandomDestination()` directly in Initialize + `_needsFirstDestination` fallback in Update.
3. **Ghost spawn "not close to NavMesh":** Added `NavMesh.SamplePosition()` with walkable-only mask before instantiation.
4. **NavMeshAgent areaMask:** Set to exclude Not Walkable area so ghosts don't path onto walls.
5. **Treasure not collectible:** Added TreasurePickup script with trigger collider.
6. **Exit gate not working:** Added ExitGate script.
7. **Animator controllers lost at runtime:** Added `EditorUtility.SetDirty()` on Animator + GameObject, plus assign to source model prefab.
8. **Joystick not wired:** Instantiated FloatingJoystick under GameplayUI, wired to InputManager via SerializedObject.
9. **`NavMeshObstacle.carve` error:** Changed to `.carving` (correct API for Unity 2021.3).

## Available Assets
- `Assets/characters_5_02/` — Humanoid models (m_1-m_13, f_1-f_12) with Idle/Walk/Run/Win/Sad animations in `Animations/` folder
- `Assets/KayKit - Mystery Monthly/` — Werewolf_Wolf.fbx, Monster.fbx, Robot_One.fbx and more
- `Assets/Joystick Pack/` — FloatingJoystick prefab at `Prefabs/Floating Joystick.prefab`
- `Assets/Dungeon Cute Series/` — Environment props and tiles

## How to Setup in Unity
1. **GraveyardHunter > Prefab Creator > "Create All"** — creates all prefabs + auto-assigns to GameConfig
2. **GraveyardHunter > Bug Fixer > "Fix All Bugs"** — fixes NavMesh, animations, joystick, enemy types, level data — everything in 1 click

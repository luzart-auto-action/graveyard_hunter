# Graveyard Hunter - Context File
## Cho session AI tiep theo

### Game Overview
- **Ten**: Graveyard Hunter
- **The loai**: Stealth / Survival / Maze Escape
- **Goc nhin**: Top-down 3D
- **Phong cach**: Low-poly, tong mau toi, light-based gameplay
- **Platform**: Mobile (touch joystick)

### Architecture
- **Patterns**: Service Locator, EventBus, State Machine, Object Pool, ScriptableObject, Factory
- **QUY TAC VANG**: Chi GameManager subscribe user-action events. Workers chi expose public methods.
- **UI Bug-proof**: GameplayUI dung CanvasGroup, WinPanel subscribe Awake + cache data, ForceHideAllPanels truoc transitions

### Folder Structure
```
Assets/_Game/Scripts/
├── Core/          (6 files) GameManager, EventBus, ServiceLocator, GameEvents, Enums, ObjectPool
├── GameState/     (9 files) GameStateManager, IGameState, 7 state classes
├── Level/         (5 files) LevelManager, LevelData, MazeGenerator, ExitGate, TreasurePickup
├── Player/        (4 files) PlayerController, PlayerHealth, PlayerInventory, PlayerLightSystem
├── Enemy/         (3 files) LightGhost, GhostLightCone, GhostEyes
├── Input/         (2 files) InputManager, VirtualJoystick
├── UI/            (9 files) UIPanel, UIManager, UIMainMenu, GameplayUI, WinPanel, FailPanel, PopupPause, PopupSettings, ShopPanel
├── Data/          (3 files) GameConfig, PlayerProgressData, SkinData
├── Audio/         (1 file)  AudioManager
├── CameraSystem/  (1 file)  CameraController
├── FX/            (2 files) FXManager, FXSpawnPoint
├── Animation/     (1 file)  DOTweenAnimator
├── Command/       (2 files) ICommand, CommandManager
├── Trap/          (4 files) TrapBase, SpikeTrap, NoiseTrap, LightBurstTrap
├── Booster/       (5 files) BoosterBase, SmokeBomb, SpeedBoots, ShadowCloak, GhostVision
├── Shop/          (1 file)  ShopManager
└── Editor/        (5 files) SceneSetupWizard, MaterialFactory, PrefabCreator, LevelBatchCreator, ProjectValidator
```

### Key GameStates
MainMenu -> Loading -> Playing -> EscapePhase -> Win/Fail -> (Next/Retry/Home)

### Scoring
+100/level, +10/treasure, +1/second alive, +50 no-damage bonus
Stars: 5HP=3star, 3HP=2star, 1HP=1star

### 10 Levels
- L1-5: 12x12 maze, 1-2 ghosts, gradual trap/booster introduction
- L6-9: 16x16 maze, 3-4 ghosts, all mechanics
- L10: 20x20 maze, 5 ghosts, grand finale

### Dependencies
- DOTween, Odin Inspector, TextMeshPro, Unity AI Navigation (NavMesh)

### Them tinh nang moi
1. Tao script trong namespace tuong ung
2. Neu can event moi -> them vao GameEvents.cs
3. Neu la worker -> KHONG subscribe events, chi expose methods
4. Neu GameManager can xu ly -> subscribe trong GameManager
5. Neu la UI -> ke thua UIPanel, hoac dung CanvasGroup neu dac biet
6. Chay Project Validator sau khi thay doi

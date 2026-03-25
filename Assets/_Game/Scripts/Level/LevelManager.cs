using System.Collections.Generic;
using DG.Tweening;
using GraveyardHunter.Core;
using GraveyardHunter.Data;
using GraveyardHunter.CameraSystem;
using GraveyardHunter.Enemy;
using GraveyardHunter.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace GraveyardHunter.Level
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField, Required]
        private GameConfig _gameConfig;

        [SerializeField]
        private List<LevelData> _levels;

        [ShowInInspector, ReadOnly]
        private int _currentLevelIndex;

        private LevelData _currentLevelData;
        private Transform _levelRoot;
        private List<GameObject> _spawnedObjects = new();
        private List<Transform> _wallTransforms = new();
        private PlayerController _playerInstance;
        private List<LightGhost> _ghostInstances = new();
        private float _levelStartTime;
        private float _cellSize = 2f;
        private NavMeshData _navMeshData;
        private NavMeshDataInstance _navMeshInstance;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<LevelManager>();
        }

        public void LoadLevel(int index)
        {
            ClearLevel();

            _currentLevelIndex = index;
            _currentLevelData = _levels[index];

            _levelRoot = new GameObject($"Level_{index}_{_currentLevelData.LevelName}").transform;

            // Generate the maze
            CellType[,] mazeGrid = MazeGenerator.Generate(
                _currentLevelData.GridWidth,
                _currentLevelData.GridHeight,
                _currentLevelData.MazeSeed
            );

            // Build the maze geometry
            BuildMazeGeometry(mazeGrid);

            // Bake NavMesh at runtime for ghost AI
            BakeNavMesh();

            // Spawn objects from placements
            SpawnObjects(mazeGrid);

            _levelStartTime = Time.time;

            EventBus.Publish(new LevelLoadedEvent
            {
                LevelIndex = index
            });

            // Camera follow player
            if (_playerInstance != null)
            {
                var cam = Object.FindObjectOfType<CameraController>();
                if (cam != null)
                    cam.SetTarget(_playerInstance.transform);
            }
        }

        public void ClearLevel()
        {
            DOTween.KillAll(false);

            // Remove runtime NavMesh
            if (_navMeshInstance.valid)
                _navMeshInstance.Remove();
            _navMeshData = null;

            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                    DestroyImmediate(obj);
            }

            _spawnedObjects.Clear();
            _wallTransforms.Clear();
            _ghostInstances.Clear();
            _playerInstance = null;

            if (_levelRoot != null)
            {
                DestroyImmediate(_levelRoot.gameObject);
                _levelRoot = null;
            }
        }

        private void BakeNavMesh()
        {
            // Remove old NavMesh
            if (_navMeshInstance.valid)
                _navMeshInstance.Remove();
            if (_navMeshData != null)
                _navMeshData = null;

            // Collect all mesh sources from level geometry
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            // Mark all walls as Not Walkable (area 1) so ghosts can't path through them
            foreach (var wallTransform in _wallTransforms)
            {
                markups.Add(new NavMeshBuildMarkup
                {
                    root = wallTransform,
                    overrideArea = true,
                    area = 1 // Not Walkable
                });
            }

            NavMeshBuilder.CollectSources(
                _levelRoot, // root transform
                NavMesh.AllAreas,
                NavMeshCollectGeometry.PhysicsColliders,
                0,
                markups,
                sources
            );

            // Calculate bounds
            var bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
            if (_currentLevelData != null)
            {
                float halfW = _currentLevelData.GridWidth * _cellSize * 0.5f;
                float halfH = _currentLevelData.GridHeight * _cellSize * 0.5f;
                bounds = new Bounds(
                    new Vector3(halfW, 0f, halfH),
                    new Vector3(halfW * 2f + 10f, 20f, halfH * 2f + 10f)
                );
            }

            // Build settings
            var settings = NavMesh.GetSettingsByID(0);
            settings.agentRadius = 0.4f;
            settings.agentHeight = 1.8f;
            settings.agentClimb = 0.3f;

            // Bake
            _navMeshData = NavMeshBuilder.BuildNavMeshData(
                settings,
                sources,
                bounds,
                _levelRoot.position,
                _levelRoot.rotation
            );

            if (_navMeshData != null)
            {
                _navMeshInstance = NavMesh.AddNavMeshData(_navMeshData);
                Debug.Log($"[LevelManager] NavMesh baked: {sources.Count} sources");
            }
            else
            {
                Debug.LogWarning("[LevelManager] NavMesh bake failed!");
            }
        }

        private void BuildMazeGeometry(CellType[,] grid)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPos = GridToWorld(x, y);

                    // Always place floor
                    var floor = Instantiate(_gameConfig.FloorPrefab, worldPos, Quaternion.identity, _levelRoot);
                    _spawnedObjects.Add(floor);

                    if (grid[x, y] == CellType.Wall)
                    {
                        var wall = Instantiate(_gameConfig.WallPrefab, worldPos, Quaternion.identity, _levelRoot);
                        _spawnedObjects.Add(wall);
                        _wallTransforms.Add(wall.transform);
                    }
                    else if (grid[x, y] == CellType.ExitGate)
                    {
                        var exitGate = Instantiate(_gameConfig.ExitGatePrefab, worldPos, Quaternion.identity, _levelRoot);
                        _spawnedObjects.Add(exitGate);
                    }
                }
            }
        }

        private void SpawnObjects(CellType[,] mazeGrid)
        {
            if (_currentLevelData.Placements == null)
                return;

            int ghostId = 0;

            foreach (var placement in _currentLevelData.Placements)
            {
                Vector3 worldPos = GridToWorld(placement.GridX, placement.GridY);

                switch (placement.Type)
                {
                    case CellType.PlayerSpawn:
                        SpawnPlayer(worldPos);
                        break;

                    case CellType.EnemySpawn:
                        SpawnEnemy(worldPos, ghostId++, _currentLevelData.GetRandomEnemyType());
                        break;

                    case CellType.Treasure:
                        var treasure = Instantiate(_gameConfig.TreasurePrefab, worldPos, Quaternion.identity, _levelRoot);
                        _spawnedObjects.Add(treasure);
                        break;

                    case CellType.Trap:
                        SpawnTrap(placement.TrapType, worldPos);
                        break;

                    case CellType.Booster:
                        SpawnBooster(placement.BoosterType, worldPos);
                        break;
                }
            }

            // If no player spawn was placed via placements, spawn at grid (1,1)
            if (_playerInstance == null)
            {
                SpawnPlayer(GridToWorld(1, 1));
            }

            // If no ghosts were placed via placements, spawn from GhostCount
            if (_ghostInstances.Count == 0 && _currentLevelData.GhostCount > 0)
            {
                var emptyCells = MazeGenerator.GetEmptyCells(mazeGrid);

                for (int i = 0; i < _currentLevelData.GhostCount && emptyCells.Count > 0; i++)
                {
                    int randomIndex = Random.Range(0, emptyCells.Count);
                    Vector2Int cell = emptyCells[randomIndex];
                    emptyCells.RemoveAt(randomIndex);

                    var enemyType = _currentLevelData.GetRandomEnemyType();
                    SpawnEnemy(GridToWorld(cell.x, cell.y), ghostId++, enemyType);
                }
            }
        }

        private void SpawnPlayer(Vector3 position)
        {
            var playerObj = Instantiate(_gameConfig.PlayerPrefab, position, Quaternion.identity, _levelRoot);
            _spawnedObjects.Add(playerObj);

            _playerInstance = playerObj.GetComponent<PlayerController>();
            _playerInstance.Initialize(_gameConfig);

            var health = playerObj.GetComponent<PlayerHealth>();
            if (health != null)
                health.Initialize(_gameConfig);

            var inventory = playerObj.GetComponent<PlayerInventory>();
            if (inventory != null)
                inventory.Initialize(_currentLevelData.RequiredTreasures);

            var lightSystem = playerObj.GetComponent<PlayerLightSystem>();
            if (lightSystem != null)
                lightSystem.Initialize(_gameConfig);
        }

        private void SpawnEnemy(Vector3 position, int id, Core.EnemyType enemyType)
        {
            // Find nearest valid WALKABLE NavMesh position so the agent can be placed
            int walkableAreaMask = 1 << 0; // Area 0 = Walkable
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, walkableAreaMask))
            {
                position = hit.position;
            }
            else
            {
                Debug.LogWarning($"[LevelManager] No NavMesh near enemy spawn {position}, skipping {enemyType} {id}");
                return;
            }

            var enemyData = _gameConfig.GetEnemyData(enemyType);
            var prefab = _gameConfig.GetEnemyPrefab(enemyType);

            var enemyObj = Instantiate(prefab, position, Quaternion.identity, _levelRoot);
            _spawnedObjects.Add(enemyObj);

            var ghost = enemyObj.GetComponent<LightGhost>();
            ghost.Initialize(_gameConfig, enemyData, _playerInstance.transform, id);
            _ghostInstances.Add(ghost);

            Debug.Log($"[LevelManager] Spawned {enemyType} (id={id}) at {position}");
        }

        /// <summary>Legacy method - spawns default Ghost type.</summary>
        private void SpawnGhost(Vector3 position, int id)
        {
            SpawnEnemy(position, id, Core.EnemyType.Ghost);
        }

        private void SpawnTrap(TrapType trapType, Vector3 position)
        {
            GameObject prefab = trapType switch
            {
                TrapType.Spike => _gameConfig.SpikeTrapPrefab,
                TrapType.Noise => _gameConfig.NoiseTrapPrefab,
                TrapType.LightBurst => _gameConfig.LightBurstTrapPrefab,
                _ => _gameConfig.SpikeTrapPrefab
            };

            var trap = Instantiate(prefab, position, Quaternion.identity, _levelRoot);
            _spawnedObjects.Add(trap);
        }

        private void SpawnBooster(BoosterType boosterType, Vector3 position)
        {
            GameObject prefab = boosterType switch
            {
                BoosterType.SmokeBomb => _gameConfig.SmokeBombPrefab,
                BoosterType.SpeedBoots => _gameConfig.SpeedBootsPrefab,
                BoosterType.ShadowCloak => _gameConfig.ShadowCloakPrefab,
                BoosterType.GhostVision => _gameConfig.GhostVisionPrefab,
                _ => _gameConfig.SmokeBombPrefab
            };

            var booster = Instantiate(prefab, position, Quaternion.identity, _levelRoot);
            _spawnedObjects.Add(booster);
        }

        public Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(x * _cellSize, 0f, y * _cellSize);
        }

        public int GetCurrentLevelIndex()
        {
            return _currentLevelIndex;
        }

        public LevelData GetCurrentLevelData()
        {
            return _currentLevelData;
        }

        public int CalculateScore()
        {
            float survivalTime = Time.time - _levelStartTime;
            int collectedTreasures = 0;

            if (_playerInstance != null)
            {
                var inventory = _playerInstance.GetComponent<PlayerInventory>();
                if (inventory != null)
                    collectedTreasures = inventory.GetCollectedCount();
            }

            int score = _gameConfig.ScorePerLevel
                      + _gameConfig.ScorePerTreasure * collectedTreasures
                      + _gameConfig.ScorePerSecond * Mathf.FloorToInt(survivalTime);

            // No damage bonus
            if (_playerInstance != null)
            {
                var health = _playerInstance.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    if (health.CurrentHP >= _gameConfig.PlayerMaxHP)
                    {
                        score += _gameConfig.NoDamageBonus;
                    }
                }
            }

            return score;
        }

        public int CalculateStars()
        {
            if (_playerInstance == null || _currentLevelData == null)
                return 0;

            var health = _playerInstance.GetComponent<PlayerHealth>();
            if (health == null)
                return 0;

            int currentHP = health.CurrentHP;

            if (currentHP >= _currentLevelData.ThreeStarHP)
                return 3;
            if (currentHP >= _currentLevelData.TwoStarHP)
                return 2;
            if (currentHP >= _currentLevelData.OneStarHP)
                return 1;

            return 0;
        }

        public float GetLevelElapsedTime()
        {
            return Time.time - _levelStartTime;
        }

        public Transform GetPlayerTransform()
        {
            return _playerInstance != null ? _playerInstance.transform : null;
        }

        public void StopAllActiveGameplay()
        {
            foreach (var ghost in _ghostInstances)
            {
                if (ghost != null)
                    ghost.enabled = false;
            }

            StopAllCoroutines();
        }
    }
}

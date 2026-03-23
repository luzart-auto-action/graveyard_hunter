using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraveyardHunter.Level;

namespace GraveyardHunter.Editor
{
    public class ProjectValidator : EditorWindow
    {
        private enum CheckStatus { Pass, Warning, Error }

        private struct ValidationResult
        {
            public string Name;
            public CheckStatus Status;
            public string Message;
        }

        private List<ValidationResult> _results = new List<ValidationResult>();
        private Vector2 _scrollPos;

        private static readonly string ConfigPath = "Assets/_Game/ScriptableObjects/Configs/GameConfig.asset";
        private static readonly string LevelsPath = "Assets/_Game/ScriptableObjects/Levels";
        private static readonly string PrefabsPath = "Assets/_Game/Prefabs";
        private static readonly string MaterialsPath = "Assets/_Game/Art/Materials";

        [MenuItem("GraveyardHunter/Project Validator")]
        public static void ShowWindow()
        {
            GetWindow<ProjectValidator>("Project Validator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Graveyard Hunter - Project Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Run Validation", GUILayout.Height(35)))
            {
                RunValidation();
            }

            EditorGUILayout.Space(10);

            if (_results.Count > 0)
            {
                // Summary
                int pass = 0, warn = 0, err = 0;
                foreach (var r in _results)
                {
                    switch (r.Status)
                    {
                        case CheckStatus.Pass: pass++; break;
                        case CheckStatus.Warning: warn++; break;
                        case CheckStatus.Error: err++; break;
                    }
                }
                EditorGUILayout.LabelField($"Results: {pass} Pass, {warn} Warning, {err} Error", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                foreach (var r in _results)
                {
                    Color color;
                    string icon;
                    switch (r.Status)
                    {
                        case CheckStatus.Pass:
                            color = new Color(0.2f, 0.8f, 0.2f);
                            icon = "[PASS]";
                            break;
                        case CheckStatus.Warning:
                            color = new Color(0.9f, 0.8f, 0.1f);
                            icon = "[WARN]";
                            break;
                        default:
                            color = new Color(0.9f, 0.2f, 0.2f);
                            icon = "[ERROR]";
                            break;
                    }

                    var style = new GUIStyle(EditorStyles.label) { normal = { textColor = color }, wordWrap = true };
                    EditorGUILayout.LabelField($"{icon} {r.Name}: {r.Message}", style);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void RunValidation()
        {
            _results.Clear();

            CheckGameConfig();
            CheckLevelAssets();
            CheckPrefabs();
            CheckMaterials();
            CheckSceneManagers();
            CheckUIManager();
            CheckAudioManager();
            CheckLevelDataPlacements();

            Repaint();
        }

        // 1. GameConfig exists and all prefab references assigned
        private void CheckGameConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>(ConfigPath);
            if (config == null)
            {
                AddResult("GameConfig", CheckStatus.Error, "GameConfig.asset not found at " + ConfigPath);
                return;
            }

            AddResult("GameConfig Exists", CheckStatus.Pass, "Found at " + ConfigPath);

            string[] prefabFields = {
                "PlayerPrefab", "GhostPrefab", "TreasurePrefab", "ExitGatePrefab",
                "WallPrefab", "FloorPrefab", "SpikeTrapPrefab", "NoiseTrapPrefab",
                "LightBurstTrapPrefab", "SmokeBombPrefab", "SpeedBootsPrefab",
                "ShadowCloakPrefab", "GhostVisionPrefab"
            };

            var so = new SerializedObject(config);
            int missing = 0;
            foreach (var field in prefabFields)
            {
                var prop = so.FindProperty(field);
                if (prop == null || prop.objectReferenceValue == null)
                {
                    AddResult($"GameConfig.{field}", CheckStatus.Error, "Prefab reference is not assigned.");
                    missing++;
                }
            }

            if (missing == 0)
                AddResult("GameConfig Prefab Refs", CheckStatus.Pass, "All 13 prefab references assigned.");
        }

        // 2. All 10 level data assets exist
        private void CheckLevelAssets()
        {
            int found = 0;
            for (int i = 1; i <= 10; i++)
            {
                string path = $"{LevelsPath}/Level_{i:D2}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (asset == null)
                {
                    AddResult($"Level {i}", CheckStatus.Error, $"Level_{i:D2}.asset not found.");
                }
                else
                {
                    found++;
                }
            }

            if (found == 10)
                AddResult("Level Assets", CheckStatus.Pass, "All 10 level assets found.");
            else if (found > 0)
                AddResult("Level Assets", CheckStatus.Warning, $"Only {found}/10 level assets found.");
        }

        // 3. All prefabs exist in correct folders
        private void CheckPrefabs()
        {
            var prefabChecks = new Dictionary<string, string>
            {
                { "Player", "Characters/Player.prefab" },
                { "Ghost", "Characters/Ghost.prefab" },
                { "Treasure", "Environment/Treasure.prefab" },
                { "ExitGate", "Environment/ExitGate.prefab" },
                { "Wall", "Environment/Wall.prefab" },
                { "Floor", "Environment/Floor.prefab" },
                { "SpikeTrap", "Traps/SpikeTrap.prefab" },
                { "NoiseTrap", "Traps/NoiseTrap.prefab" },
                { "LightBurstTrap", "Traps/LightBurstTrap.prefab" },
                { "SmokeBomb", "Boosters/SmokeBomb.prefab" },
                { "SpeedBoots", "Boosters/SpeedBoots.prefab" },
                { "ShadowCloak", "Boosters/ShadowCloak.prefab" },
                { "GhostVision", "Boosters/GhostVision.prefab" },
            };

            int found = 0;
            foreach (var kv in prefabChecks)
            {
                string path = $"{PrefabsPath}/{kv.Value}";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                {
                    found++;
                }
                else
                {
                    AddResult($"Prefab: {kv.Key}", CheckStatus.Error, $"Not found at {path}");
                }
            }

            if (found == prefabChecks.Count)
                AddResult("Prefabs", CheckStatus.Pass, $"All {prefabChecks.Count} prefabs found.");
        }

        // 4. All materials exist
        private void CheckMaterials()
        {
            string[] materialNames = {
                "Floor_Dark", "Wall_Stone", "Player_Body", "Ghost_Body", "Ghost_Eyes",
                "Ghost_Chase_Eyes", "Treasure_Gold", "Treasure_Silver", "ExitGate_Closed",
                "ExitGate_Open", "Trap_Spike", "Trap_Noise", "Trap_LightBurst",
                "Booster_Smoke", "Booster_Speed", "Booster_Shadow", "Booster_Vision",
                "Light_Warm", "Fog_Dark"
            };

            int found = 0;
            foreach (var name in materialNames)
            {
                string path = $"{MaterialsPath}/{name}.mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                    found++;
                else
                    AddResult($"Material: {name}", CheckStatus.Warning, $"Not found at {path}");
            }

            if (found == materialNames.Length)
                AddResult("Materials", CheckStatus.Pass, $"All {materialNames.Length} materials found.");
        }

        // 5. Scene has all required managers
        private void CheckSceneManagers()
        {
            var managers = GameObject.Find("Managers");
            if (managers == null)
            {
                AddResult("Scene Managers", CheckStatus.Error, "Managers root GameObject not found in scene.");
                return;
            }

            string[] requiredManagers = {
                "GameManager", "GameStateManager", "LevelManager", "UIManager",
                "InputManager", "AudioManager", "FXManager", "ObjectPool",
                "CommandManager", "ShopManager"
            };

            int found = 0;
            foreach (var name in requiredManagers)
            {
                var child = managers.transform.Find(name);
                if (child == null)
                    AddResult($"Manager: {name}", CheckStatus.Error, $"{name} not found under Managers.");
                else
                    found++;
            }

            if (found == requiredManagers.Length)
                AddResult("Scene Managers", CheckStatus.Pass, $"All {requiredManagers.Length} managers found.");
        }

        // 6. UIManager has all panels assigned
        private void CheckUIManager()
        {
            var managers = GameObject.Find("Managers");
            if (managers == null) return;

            var uiManagerTrans = managers.transform.Find("UIManager");
            if (uiManagerTrans == null)
            {
                AddResult("UIManager", CheckStatus.Error, "UIManager not found.");
                return;
            }

            var uiManager = uiManagerTrans.GetComponent<UI.UIManager>();
            if (uiManager == null)
            {
                AddResult("UIManager Component", CheckStatus.Error, "UIManager component not found.");
                return;
            }

            var so = new SerializedObject(uiManager);
            var panelsProp = so.FindProperty("_panels");
            if (panelsProp == null || panelsProp.arraySize == 0)
            {
                AddResult("UIManager Panels", CheckStatus.Error, "No panels assigned to UIManager._panels.");
                return;
            }

            int nullCount = 0;
            for (int i = 0; i < panelsProp.arraySize; i++)
            {
                if (panelsProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    nullCount++;
            }

            if (nullCount > 0)
                AddResult("UIManager Panels", CheckStatus.Warning, $"{nullCount} null entries in panels list.");
            else
                AddResult("UIManager Panels", CheckStatus.Pass, $"{panelsProp.arraySize} panels assigned.");

            var gameplayProp = so.FindProperty("_gameplayUI");
            if (gameplayProp == null || gameplayProp.objectReferenceValue == null)
                AddResult("UIManager GameplayUI", CheckStatus.Error, "_gameplayUI not assigned.");
            else
                AddResult("UIManager GameplayUI", CheckStatus.Pass, "GameplayUI assigned.");
        }

        // 7. AudioManager has music source
        private void CheckAudioManager()
        {
            var managers = GameObject.Find("Managers");
            if (managers == null) return;

            var audioTrans = managers.transform.Find("AudioManager");
            if (audioTrans == null) return;

            var audioManager = audioTrans.GetComponent<Audio.AudioManager>();
            if (audioManager == null)
            {
                AddResult("AudioManager", CheckStatus.Error, "AudioManager component not found.");
                return;
            }

            var so = new SerializedObject(audioManager);
            var musicProp = so.FindProperty("_musicSource");
            if (musicProp == null || musicProp.objectReferenceValue == null)
                AddResult("AudioManager MusicSource", CheckStatus.Error, "_musicSource not assigned.");
            else
                AddResult("AudioManager MusicSource", CheckStatus.Pass, "Music AudioSource assigned.");
        }

        // 8. Level data placements are valid (within grid bounds)
        private void CheckLevelDataPlacements()
        {
            bool allValid = true;
            for (int i = 1; i <= 10; i++)
            {
                string path = $"{LevelsPath}/Level_{i:D2}.asset";
                var level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level == null) continue;

                if (level.Placements == null || level.Placements.Count == 0)
                {
                    AddResult($"Level {i} Placements", CheckStatus.Warning, "No placements defined.");
                    allValid = false;
                    continue;
                }

                bool hasPlayer = false;
                bool hasExit = false;
                int outOfBounds = 0;

                foreach (var p in level.Placements)
                {
                    if (p.Type == Core.CellType.PlayerSpawn) hasPlayer = true;
                    if (p.Type == Core.CellType.ExitGate) hasExit = true;

                    if (p.GridX < 0 || p.GridX >= level.GridWidth || p.GridY < 0 || p.GridY >= level.GridHeight)
                        outOfBounds++;
                }

                if (!hasPlayer)
                {
                    AddResult($"Level {i} PlayerSpawn", CheckStatus.Error, "No PlayerSpawn placement.");
                    allValid = false;
                }

                if (!hasExit)
                {
                    AddResult($"Level {i} ExitGate", CheckStatus.Error, "No ExitGate placement.");
                    allValid = false;
                }

                if (outOfBounds > 0)
                {
                    AddResult($"Level {i} Bounds", CheckStatus.Error, $"{outOfBounds} placements out of grid bounds.");
                    allValid = false;
                }
            }

            if (allValid)
                AddResult("Level Placements", CheckStatus.Pass, "All level placements are valid.");
        }

        private void AddResult(string name, CheckStatus status, string message)
        {
            _results.Add(new ValidationResult { Name = name, Status = status, Message = message });
        }
    }
}

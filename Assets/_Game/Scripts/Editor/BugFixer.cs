using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

namespace GraveyardHunter.Editor
{
    public class BugFixer : EditorWindow
    {
        private Vector2 _scrollPos;
        private System.Collections.Generic.List<string> _log = new();

        [MenuItem("GraveyardHunter/Bug Fixer (1-Click)")]
        public static void ShowWindow()
        {
            GetWindow<BugFixer>("Bug Fixer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Graveyard Hunter - Bug Fixer", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Fixes:\n" +
                "1. Floor prefab: adds BoxCollider (required for NavMesh baking)\n" +
                "2. Wall prefab: removes NavMeshObstacle\n" +
                "3. All enemy prefabs (Ghost/Werewolf/Monster/Robot): fixes NavMeshAgent walkable mask\n" +
                "4. Treasure prefab: adds TreasurePickup script\n" +
                "5. ExitGate prefab: adds ExitGate script\n" +
                "6. Player animation: creates AnimatorController + assigns to model\n" +
                "7. All enemy animations: creates AnimatorController + assigns to models\n" +
                "8. Joystick: wires FloatingJoystick from Joystick Pack to InputManager\n" +
                "9. Auto-populates EnemyTypes in GameConfig with prefab refs\n" +
                "10. Auto-sets AllowedEnemyTypes in LevelData assets\n" +
                "11. Obstacle prefab: trigger collider + ObstacleShelter + GameConfig ref\n" +
                "12. LevelManager: auto-populates _levels list from LevelData assets\n" +
                "13. TutorialUI: creates tutorial overlay panel in Canvas",
                MessageType.Info);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Fix All Bugs", GUILayout.Height(40)))
            {
                _log.Clear();
                FixFloorPrefab();
                FixWallPrefab();
                FixAllEnemyPrefabs();
                FixTreasurePrefab();
                FixExitGatePrefab();
                FixPlayerAnimation();
                FixAllEnemyAnimations();
                FixJoystickSetup();
                FixEnemyTypesInGameConfig();
                FixLevelDataEnemyTypes();
                FixObstaclePrefab();
                FixLevelManagerLevelsList();
                CreateTutorialUI();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Log("--- All fixes applied! ---");
            }

            EditorGUILayout.Space(10);

            if (_log.Count > 0)
            {
                EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
                foreach (var entry in _log)
                    EditorGUILayout.LabelField(entry, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndScrollView();
            }
        }

        private void Log(string msg)
        {
            _log.Add(msg);
            Debug.Log($"[BugFixer] {msg}");
        }

        private void FixFloorPrefab()
        {
            string path = "Assets/_Game/Prefabs/Environment/Floor.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Log("Floor prefab not found at: " + path);
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);

            // Add BoxCollider if missing (required for NavMesh baking with PhysicsColliders)
            if (root.GetComponent<BoxCollider>() == null)
            {
                var col = root.AddComponent<BoxCollider>();
                col.size = new Vector3(2f, 0.1f, 2f);
                col.center = new Vector3(0f, -0.05f, 0f);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Log("Fixed Floor prefab: added BoxCollider (2x0.1x2) for NavMesh baking.");
            }
            else
            {
                Log("Floor prefab OK: already has BoxCollider.");
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private static readonly string[] EnemyPrefabPaths = {
            "Assets/_Game/Prefabs/Characters/Ghost.prefab",
            "Assets/_Game/Prefabs/Characters/Werewolf.prefab",
            "Assets/_Game/Prefabs/Characters/Monster.prefab",
            "Assets/_Game/Prefabs/Characters/Robot.prefab"
        };

        private void FixAllEnemyPrefabs()
        {
            foreach (var path in EnemyPrefabPaths)
            {
                FixEnemyPrefabNavMesh(path);
            }
        }

        private void FixEnemyPrefabNavMesh(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return; // Prefab not yet created, skip silently

            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            var root = PrefabUtility.LoadPrefabContents(path);

            var agent = root.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                int notWalkableBit = 1 << 1;
                int newMask = ~notWalkableBit;
                if (agent.areaMask != newMask)
                {
                    agent.areaMask = newMask;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    Log($"Fixed {name} prefab: NavMeshAgent areaMask excludes Not Walkable area.");
                }
                else
                {
                    Log($"{name} prefab OK: areaMask already correct.");
                }
            }
            else
            {
                Log($"{name} prefab: no NavMeshAgent found!");
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private void FixWallPrefab()
        {
            string path = "Assets/_Game/Prefabs/Environment/Wall.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Log("Wall prefab not found at: " + path);
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);

            // Remove NavMeshObstacle if present (walls are now excluded via NavMesh bake markups)
            var obstacle = root.GetComponent<NavMeshObstacle>();
            if (obstacle != null)
            {
                Object.DestroyImmediate(obstacle);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Log("Fixed Wall prefab: removed NavMeshObstacle (now handled by LevelManager NavMesh markups).");
            }
            else
            {
                Log("Wall prefab OK: no NavMeshObstacle to remove.");
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private void FixTreasurePrefab()
        {
            // Fix all treasure prefabs (legacy + typed)
            string[] treasurePrefabs = {
                "Assets/_Game/Prefabs/Environment/Treasure.prefab",
                "Assets/_Game/Prefabs/Environment/Treasure_Gold.prefab",
                "Assets/_Game/Prefabs/Environment/Treasure_Silver.prefab",
                "Assets/_Game/Prefabs/Environment/Treasure_Coin.prefab",
                "Assets/_Game/Prefabs/Environment/Treasure_Artifact.prefab",
            };

            foreach (var path in treasurePrefabs)
            {
                FixSingleTreasurePrefab(path);
            }

            // Auto-assign typed prefabs to GameConfig
            string configPath = "Assets/_Game/ScriptableObjects/Configs/GameConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>(configPath);
            if (config != null)
            {
                var so = new SerializedObject(config);

                TryAssignPrefabRef(so, "GoldTreasurePrefab", "Assets/_Game/Prefabs/Environment/Treasure_Gold.prefab");
                TryAssignPrefabRef(so, "SilverTreasurePrefab", "Assets/_Game/Prefabs/Environment/Treasure_Silver.prefab");
                TryAssignPrefabRef(so, "CoinTreasurePrefab", "Assets/_Game/Prefabs/Environment/Treasure_Coin.prefab");
                TryAssignPrefabRef(so, "ArtifactTreasurePrefab", "Assets/_Game/Prefabs/Environment/Treasure_Artifact.prefab");

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
                Log("Assigned typed treasure prefabs to GameConfig.");
            }
        }

        private void FixSingleTreasurePrefab(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var root = PrefabUtility.LoadPrefabContents(path);

            // Ensure TreasurePickup component
            var pickup = root.GetComponent<Level.TreasurePickup>();
            if (pickup == null)
                pickup = root.AddComponent<Level.TreasurePickup>();

            // Assign serialized references
            var so = new SerializedObject(pickup);

            var visualRoot = root.transform.Find("VisualRoot");
            if (visualRoot != null)
            {
                var prop = so.FindProperty("_visualRoot");
                if (prop != null) prop.objectReferenceValue = visualRoot;
            }

            var glowLightGO = root.transform.Find("GlowLight");
            if (glowLightGO != null)
            {
                var light = glowLightGO.GetComponent<Light>();
                if (light != null)
                {
                    var prop = so.FindProperty("_glowLight");
                    if (prop != null) prop.objectReferenceValue = light;
                }
            }

            // Ensure tag
            try { root.tag = "Treasure"; } catch { }

            // Ensure trigger collider
            var col = root.GetComponent<BoxCollider>();
            if (col == null) col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);

            Log($"Fixed treasure prefab: {System.IO.Path.GetFileNameWithoutExtension(path)}");
        }

        private void TryAssignPrefabRef(SerializedObject so, string fieldName, string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;
            var prop = so.FindProperty(fieldName);
            if (prop != null)
                prop.objectReferenceValue = prefab;
        }

        private void FixExitGatePrefab()
        {
            string path = "Assets/_Game/Prefabs/Environment/ExitGate.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Log("ExitGate prefab not found at: " + path);
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);

            if (root.GetComponent<Level.ExitGate>() != null)
            {
                Log("ExitGate prefab already has ExitGate script. Skipped.");
                PrefabUtility.UnloadPrefabContents(root);
                return;
            }

            var gate = root.AddComponent<Level.ExitGate>();
            var so = new SerializedObject(gate);

            // Assign VisualRoot
            var visualRoot = root.transform.Find("VisualRoot");
            if (visualRoot != null)
            {
                var prop = so.FindProperty("_visualRoot");
                if (prop != null) prop.objectReferenceValue = visualRoot;

                // Assign renderer from VisualRoot
                var renderer = visualRoot.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var rendProp = so.FindProperty("_gateRenderer");
                    if (rendProp != null) rendProp.objectReferenceValue = renderer;
                }
                Log("  -> Assigned _visualRoot and _gateRenderer");
            }

            // Assign GateLight
            var gateLightGO = root.transform.Find("GateLight");
            if (gateLightGO != null)
            {
                var light = gateLightGO.GetComponent<Light>();
                if (light != null)
                {
                    var prop = so.FindProperty("_gateLight");
                    if (prop != null) prop.objectReferenceValue = light;
                    Log("  -> Assigned _gateLight");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);

            Log("Fixed ExitGate prefab: added ExitGate script with references.");
        }

        // ======================== PLAYER ANIMATION ========================

        private void FixPlayerAnimation()
        {
            string controllerPath = "Assets/_Game/Animations/PlayerAnimator.controller";
            EnsureFolder("Assets/_Game/Animations");

            // Load animation clips
            var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/characters_5_02/Animations/Idle.anim");
            var runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/characters_5_02/Animations/Run.anim");
            var winClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/characters_5_02/Animations/Win.anim");
            var sadClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/characters_5_02/Animations/Sad.anim");

            if (idleClip == null || runClip == null)
            {
                Log("Player animation clips not found! Need Idle.anim and Run.anim in characters_5_02/Animations/");
                return;
            }

            // Delete existing controller to recreate cleanly
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
                AssetDatabase.DeleteAsset(controllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            // Add parameters
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Win", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            var rootSM = controller.layers[0].stateMachine;

            // Create states
            var idleState = rootSM.AddState("Idle", new Vector3(300, 0, 0));
            idleState.motion = idleClip;

            var runState = rootSM.AddState("Run", new Vector3(300, 80, 0));
            runState.motion = runClip;

            var winState = rootSM.AddState("Win", new Vector3(550, 0, 0));
            if (winClip != null) winState.motion = winClip;

            var sadState = rootSM.AddState("Sad", new Vector3(550, 80, 0));
            if (sadClip != null) sadState.motion = sadClip;

            rootSM.defaultState = idleState;

            // Idle → Run (Speed > 0.1)
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.15f;

            // Run → Idle (Speed < 0.1)
            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.15f;

            // Any → Win
            var anyToWin = rootSM.AddAnyStateTransition(winState);
            anyToWin.AddCondition(AnimatorConditionMode.If, 0, "Win");
            anyToWin.hasExitTime = false;
            anyToWin.duration = 0.2f;

            // Any → Sad (Die)
            var anyToSad = rootSM.AddAnyStateTransition(sadState);
            anyToSad.AddCondition(AnimatorConditionMode.If, 0, "Die");
            anyToSad.hasExitTime = false;
            anyToSad.duration = 0.2f;

            AssetDatabase.SaveAssets();
            Log("Created PlayerAnimator.controller (Idle/Run/Win/Sad with Speed/Win/Die params).");

            // Assign to Player prefab model
            AssignAnimatorToModel("Assets/_Game/Prefabs/Characters/Player.prefab", controller, "Player");
        }

        // ======================== ENEMY ANIMATIONS (ALL TYPES) ========================

        private void FixAllEnemyAnimations()
        {
            string controllerPath = "Assets/_Game/Animations/EnemyAnimator.controller";
            EnsureFolder("Assets/_Game/Animations");

            var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/characters_5_02/Animations/Idle.anim");
            var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/characters_5_02/Animations/Walk.anim");
            var runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/characters_5_02/Animations/Run.anim");

            if (idleClip == null || walkClip == null || runClip == null)
            {
                Log("Enemy animation clips not found! Need Idle/Walk/Run.anim in characters_5_02/Animations/");
                return;
            }

            // Delete old controllers
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
                AssetDatabase.DeleteAsset(controllerPath);
            // Also clean up legacy GhostAnimator
            string legacyPath = "Assets/_Game/Animations/GhostAnimator.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(legacyPath) != null)
                AssetDatabase.DeleteAsset(legacyPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsChasing", AnimatorControllerParameterType.Bool);

            var rootSM = controller.layers[0].stateMachine;

            var idleState = rootSM.AddState("Idle", new Vector3(300, 0, 0));
            idleState.motion = idleClip;

            var walkState = rootSM.AddState("Walk", new Vector3(300, 80, 0));
            walkState.motion = walkClip;

            var runState = rootSM.AddState("Run", new Vector3(300, 160, 0));
            runState.motion = runClip;

            rootSM.defaultState = idleState;

            // Idle → Walk (Speed > 0.1, not chasing)
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsChasing");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;

            // Idle → Run (Speed > 0.1, chasing)
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToRun.AddCondition(AnimatorConditionMode.If, 0, "IsChasing");
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.15f;

            // Walk → Idle (Speed < 0.1)
            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;

            // Walk → Run (IsChasing)
            var walkToRun = walkState.AddTransition(runState);
            walkToRun.AddCondition(AnimatorConditionMode.If, 0, "IsChasing");
            walkToRun.hasExitTime = false;
            walkToRun.duration = 0.15f;

            // Run → Walk (!IsChasing, still moving)
            var runToWalk = runState.AddTransition(walkState);
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsChasing");
            runToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            runToWalk.hasExitTime = false;
            runToWalk.duration = 0.15f;

            // Run → Idle (!IsChasing, stopped)
            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.15f;

            AssetDatabase.SaveAssets();
            Log("Created EnemyAnimator.controller (Idle/Walk/Run with Speed/IsChasing params).");

            // Assign to ALL enemy prefabs
            foreach (var path in EnemyPrefabPaths)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                {
                    AssignAnimatorToModel(path, controller, name);
                }
            }
        }

        // ======================== HELPER: Assign Animator ========================

        private void AssignAnimatorToModel(string prefabPath, AnimatorController controller, string label)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Log($"{label} prefab not found at: {prefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);

            // Find the character model's Animator (should be on a child under VisualRoot)
            var animator = root.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                var visualRoot = root.transform.Find("VisualRoot");
                if (visualRoot != null && visualRoot.childCount > 0)
                {
                    var modelChild = visualRoot.GetChild(0);
                    animator = modelChild.gameObject.AddComponent<Animator>();
                    Log($"  -> Added Animator to {modelChild.name}");
                }
                else
                {
                    Log($"{label}: No model found under VisualRoot!");
                    PrefabUtility.UnloadPrefabContents(root);
                    return;
                }
            }

            // Use SerializedObject to properly record the override on nested prefab instances
            var so = new SerializedObject(animator);
            var controllerProp = so.FindProperty("m_Controller");
            controllerProp.objectReferenceValue = controller;

            var rootMotionProp = so.FindProperty("m_ApplyRootMotion");
            rootMotionProp.boolValue = false;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(animator.gameObject);

            // Get model name BEFORE unloading
            string modelName = null;
            var vr = root.transform.Find("VisualRoot");
            if (vr != null && vr.childCount > 0)
                modelName = vr.GetChild(0).name;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);

            Log($"Fixed {label} prefab: assigned {controller.name} to model Animator (via SerializedObject).");

            // Also assign to the SOURCE model prefab so it persists through nested prefab resolution
            if (!string.IsNullOrEmpty(modelName))
                AssignControllerToSourceModel(modelName, controller, label);
        }

        private void AssignControllerToSourceModel(string modelName, AnimatorController controller, string label)
        {

            // Try to find the source model prefab
            string[] searchPaths = {
                $"Assets/characters_5_02/Prefabs/{modelName}.prefab",
                $"Assets/characters_5_02/Prefabs/{modelName.Replace("(Clone)", "").Trim()}.prefab"
            };

            foreach (var path in searchPaths)
            {
                var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (sourcePrefab == null) continue;

                var sourceRoot = PrefabUtility.LoadPrefabContents(path);
                var sourceAnimator = sourceRoot.GetComponentInChildren<Animator>();

                if (sourceAnimator == null)
                {
                    sourceAnimator = sourceRoot.AddComponent<Animator>();
                }

                var so = new SerializedObject(sourceAnimator);
                var controllerProp = so.FindProperty("m_Controller");
                controllerProp.objectReferenceValue = controller;

                var rootMotionProp = so.FindProperty("m_ApplyRootMotion");
                rootMotionProp.boolValue = false;

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(sourceAnimator);
                EditorUtility.SetDirty(sourceAnimator.gameObject);

                PrefabUtility.SaveAsPrefabAsset(sourceRoot, path);
                PrefabUtility.UnloadPrefabContents(sourceRoot);

                Log($"  -> Also assigned {controller.name} to source model: {path}");
                return;
            }
        }

        // ======================== ENEMY TYPES IN GAMECONFIG ========================

        private void FixEnemyTypesInGameConfig()
        {
            string configPath = "Assets/_Game/ScriptableObjects/Configs/GameConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>(configPath);
            if (config == null)
            {
                Log("GameConfig not found, skipping EnemyTypes setup.");
                return;
            }

            var types = new (Core.EnemyType type, string path)[]
            {
                (Core.EnemyType.Ghost, "Assets/_Game/Prefabs/Characters/Ghost.prefab"),
                (Core.EnemyType.Werewolf, "Assets/_Game/Prefabs/Characters/Werewolf.prefab"),
                (Core.EnemyType.Monster, "Assets/_Game/Prefabs/Characters/Monster.prefab"),
                (Core.EnemyType.Robot, "Assets/_Game/Prefabs/Characters/Robot.prefab")
            };

            foreach (var (type, path) in types)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                // Check if already exists
                bool found = false;
                foreach (var data in config.EnemyTypes)
                {
                    if (data.Type == type)
                    {
                        data.Prefab = prefab;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var newData = Data.EnemyTypeData.GetDefault(type);
                    newData.Prefab = prefab;
                    config.EnemyTypes.Add(newData);
                    Log($"  -> Added {type} to EnemyTypes with default stats.");
                }
            }

            EditorUtility.SetDirty(config);
            Log($"Fixed GameConfig: {config.EnemyTypes.Count} enemy types configured.");
        }

        // ======================== LEVEL DATA ENEMY TYPES ========================

        private void FixLevelDataEnemyTypes()
        {
            // Find all LevelData assets
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/_Game" });
            if (guids.Length == 0)
            {
                Log("No LevelData assets found.");
                return;
            }

            // Progressive enemy type unlocking per level
            var allTypes = new Core.EnemyType[] {
                Core.EnemyType.Ghost,
                Core.EnemyType.Werewolf,
                Core.EnemyType.Monster,
                Core.EnemyType.Robot
            };

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var levelData = AssetDatabase.LoadAssetAtPath<Level.LevelData>(path);
                if (levelData == null) continue;

                // Skip if already configured
                if (levelData.AllowedEnemyTypes != null && levelData.AllowedEnemyTypes.Count > 0) continue;

                // Progressive unlock: level 0 = Ghost only, level 1 = Ghost+Werewolf, etc.
                int typesToUnlock = Mathf.Clamp(levelData.LevelIndex + 1, 1, allTypes.Length);
                levelData.AllowedEnemyTypes = new System.Collections.Generic.List<Core.EnemyType>();
                for (int i = 0; i < typesToUnlock; i++)
                {
                    levelData.AllowedEnemyTypes.Add(allTypes[i]);
                }

                EditorUtility.SetDirty(levelData);
                Log($"  -> Level {levelData.LevelIndex} ({levelData.LevelName}): {typesToUnlock} enemy types.");
            }

            Log("Fixed LevelData: progressive enemy type unlocking.");
        }

        // ======================== JOYSTICK SETUP ========================

        private void FixJoystickSetup()
        {
            // Find InputManager in scene
            var inputManager = Object.FindObjectOfType<GraveyardHunter.Input.InputManager>();
            if (inputManager == null)
            {
                Log("InputManager not found in scene!");
                return;
            }

            // Check if already has a Joystick assigned
            var so = new SerializedObject(inputManager);
            var joystickProp = so.FindProperty("_joystick");
            if (joystickProp != null && joystickProp.objectReferenceValue != null)
            {
                Log("InputManager already has a Joystick assigned. Skipped.");
                return;
            }

            // Find GameplayUI in scene to parent the joystick under it
            var gameplayUI = Object.FindObjectOfType<GraveyardHunter.UI.GameplayUI>(true);
            if (gameplayUI == null)
            {
                Log("GameplayUI not found in scene! Cannot place joystick.");
                return;
            }

            // Load FloatingJoystick prefab
            string prefabPath = "Assets/Joystick Pack/Prefabs/Floating Joystick.prefab";
            var joystickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (joystickPrefab == null)
            {
                Log("FloatingJoystick prefab not found at: " + prefabPath);
                return;
            }

            // Check if a FloatingJoystick already exists under GameplayUI
            var existingJoystick = gameplayUI.GetComponentInChildren<FloatingJoystick>(true);
            if (existingJoystick != null)
            {
                // Just wire it up
                joystickProp.objectReferenceValue = existingJoystick;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(inputManager);
                Log("Wired existing FloatingJoystick to InputManager.");
                return;
            }

            // Instantiate FloatingJoystick as child of GameplayUI
            var joystickGO = (GameObject)PrefabUtility.InstantiatePrefab(joystickPrefab, gameplayUI.transform);
            joystickGO.name = "FloatingJoystick";

            // Set RectTransform to stretch full screen (touch area)
            var rt = joystickGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Wire to InputManager
            var floatingJoystick = joystickGO.GetComponent<FloatingJoystick>();
            joystickProp.objectReferenceValue = floatingJoystick;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(inputManager);
            EditorUtility.SetDirty(gameplayUI.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Log("Fixed Joystick: instantiated FloatingJoystick under GameplayUI, wired to InputManager.");
        }

        // ======================== OBSTACLE PREFAB ========================

        private void FixObstaclePrefab()
        {
            string prefabPath = "Assets/_Game/Prefabs/Environment/Obstacle.prefab";

            // Delete and recreate if exists (to apply new larger design)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
                AssetDatabase.Refresh();
                Log("Deleted old small Obstacle prefab.");
            }

            // Recreate with large shelter design
            var root = new GameObject("Obstacle");

            // Large trigger collider — bigger than 1 maze cell so player enters easily
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2.5f, 3f, 2.5f);
            col.center = new Vector3(0f, 1.5f, 0f);

            root.AddComponent<Level.ObstacleShelter>();

            // Back wall
            var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "BackWall";
            backWall.transform.SetParent(root.transform);
            backWall.transform.localScale = new Vector3(1.8f, 2.5f, 0.3f);
            backWall.transform.localPosition = new Vector3(0f, 1.25f, 0.8f);
            Object.DestroyImmediate(backWall.GetComponent<Collider>());
            ApplyObstacleMaterial(backWall);

            // Left wall
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWall";
            leftWall.transform.SetParent(root.transform);
            leftWall.transform.localScale = new Vector3(0.3f, 2.0f, 1.4f);
            leftWall.transform.localPosition = new Vector3(-0.8f, 1.0f, 0.1f);
            Object.DestroyImmediate(leftWall.GetComponent<Collider>());
            ApplyObstacleMaterial(leftWall);

            // Right wall
            var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWall";
            rightWall.transform.SetParent(root.transform);
            rightWall.transform.localScale = new Vector3(0.3f, 2.0f, 1.4f);
            rightWall.transform.localPosition = new Vector3(0.8f, 1.0f, 0.1f);
            Object.DestroyImmediate(rightWall.GetComponent<Collider>());
            ApplyObstacleMaterial(rightWall);

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(root.transform);
            roof.transform.localScale = new Vector3(1.8f, 0.2f, 1.6f);
            roof.transform.localPosition = new Vector3(0f, 2.5f, 0.1f);
            Object.DestroyImmediate(roof.GetComponent<Collider>());
            ApplyObstacleMaterial(roof);

            // Green glow light visible from far away
            var glowGO = new GameObject("ShelterGlow");
            glowGO.transform.SetParent(root.transform);
            glowGO.transform.localPosition = new Vector3(0f, 1.2f, 0.3f);
            var glow = glowGO.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 5f;
            glow.intensity = 1.2f;
            glow.color = new Color(0.3f, 0.9f, 0.5f, 1f);

            // Ensure folder exists
            EnsureFolder("Assets/_Game/Prefabs/Environment");
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            // Assign to GameConfig
            string configPath = "Assets/_Game/ScriptableObjects/Configs/GameConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>(configPath);
            if (config != null)
            {
                config.ObstaclePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                EditorUtility.SetDirty(config);
            }

            Log("Rebuilt Obstacle prefab: large shelter (3 walls + roof + glow) + GameConfig ref.");
        }

        private void ApplyObstacleMaterial(GameObject go)
        {
            var mat = MaterialFactory.GetOrCreateMaterial("Obstacle_Stone");
            if (mat != null)
            {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.sharedMaterial = mat;
            }
        }

        // ======================== TUTORIAL UI ========================

        private void CreateTutorialUI()
        {
            // Skip if TutorialUI already exists
            if (Object.FindObjectOfType<UI.TutorialUI>(true) != null)
            {
                Log("TutorialUI already exists in scene. Skipped.");
                return;
            }

            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Log("No Canvas found. Cannot create TutorialUI.");
                return;
            }

            // Root panel
            var root = new GameObject("TutorialUI");
            root.transform.SetParent(canvas.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Dark overlay
            var overlay = root.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.7f);
            overlay.raycastTarget = true;

            // Message box (center)
            var msgBox = new GameObject("MessageBox");
            msgBox.transform.SetParent(root.transform, false);
            var msgBoxRect = msgBox.AddComponent<RectTransform>();
            msgBoxRect.anchorMin = new Vector2(0.15f, 0.2f);
            msgBoxRect.anchorMax = new Vector2(0.85f, 0.8f);
            msgBoxRect.offsetMin = Vector2.zero;
            msgBoxRect.offsetMax = Vector2.zero;
            var msgBoxImg = msgBox.AddComponent<Image>();
            msgBoxImg.color = new Color(0.1f, 0.08f, 0.18f, 0.95f);

            // Message text
            var msgTextGO = new GameObject("MessageText");
            msgTextGO.transform.SetParent(msgBox.transform, false);
            var msgTextRect = msgTextGO.AddComponent<RectTransform>();
            msgTextRect.anchorMin = new Vector2(0.05f, 0.25f);
            msgTextRect.anchorMax = new Vector2(0.95f, 0.95f);
            msgTextRect.offsetMin = Vector2.zero;
            msgTextRect.offsetMax = Vector2.zero;
            var msgTMP = msgTextGO.AddComponent<TextMeshProUGUI>();
            msgTMP.text = "Tutorial";
            msgTMP.fontSize = 24;
            msgTMP.alignment = TextAlignmentOptions.Center;
            msgTMP.color = Color.white;
            msgTMP.enableWordWrapping = true;
            msgTMP.richText = true;
            msgTMP.raycastTarget = false;

            // Step counter text (top-right)
            var counterGO = new GameObject("StepCounter");
            counterGO.transform.SetParent(msgBox.transform, false);
            var counterRect = counterGO.AddComponent<RectTransform>();
            counterRect.anchorMin = new Vector2(0.7f, 0.9f);
            counterRect.anchorMax = new Vector2(0.95f, 1f);
            counterRect.offsetMin = Vector2.zero;
            counterRect.offsetMax = Vector2.zero;
            var counterTMP = counterGO.AddComponent<TextMeshProUGUI>();
            counterTMP.text = "1/6";
            counterTMP.fontSize = 18;
            counterTMP.alignment = TextAlignmentOptions.Right;
            counterTMP.color = new Color(0.6f, 0.6f, 0.7f, 1f);
            counterTMP.raycastTarget = false;

            // OK button
            var okBtnGO = new GameObject("OKButton");
            okBtnGO.transform.SetParent(msgBox.transform, false);
            var okRect = okBtnGO.AddComponent<RectTransform>();
            okRect.anchorMin = new Vector2(0.3f, 0.03f);
            okRect.anchorMax = new Vector2(0.7f, 0.18f);
            okRect.offsetMin = Vector2.zero;
            okRect.offsetMax = Vector2.zero;
            var okImg = okBtnGO.AddComponent<Image>();
            okImg.color = new Color(0.2f, 0.75f, 0.3f, 1f);
            var okBtn = okBtnGO.AddComponent<Button>();
            okBtn.targetGraphic = okImg;

            var okTextGO = new GameObject("Text");
            okTextGO.transform.SetParent(okBtnGO.transform, false);
            var okTextRect = okTextGO.AddComponent<RectTransform>();
            okTextRect.anchorMin = Vector2.zero;
            okTextRect.anchorMax = Vector2.one;
            okTextRect.offsetMin = Vector2.zero;
            okTextRect.offsetMax = Vector2.zero;
            var okTMP = okTextGO.AddComponent<TextMeshProUGUI>();
            okTMP.text = "OK";
            okTMP.fontSize = 24;
            okTMP.fontStyle = FontStyles.Bold;
            okTMP.alignment = TextAlignmentOptions.Center;
            okTMP.color = Color.white;
            okTMP.raycastTarget = false;

            // Skip button (bottom-right, smaller)
            var skipBtnGO = new GameObject("SkipButton");
            skipBtnGO.transform.SetParent(msgBox.transform, false);
            var skipRect = skipBtnGO.AddComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(0.75f, 0.03f);
            skipRect.anchorMax = new Vector2(0.97f, 0.18f);
            skipRect.offsetMin = Vector2.zero;
            skipRect.offsetMax = Vector2.zero;
            var skipImg = skipBtnGO.AddComponent<Image>();
            skipImg.color = new Color(0.4f, 0.35f, 0.45f, 1f);
            var skipBtn = skipBtnGO.AddComponent<Button>();
            skipBtn.targetGraphic = skipImg;

            var skipTextGO = new GameObject("Text");
            skipTextGO.transform.SetParent(skipBtnGO.transform, false);
            var skipTextRect = skipTextGO.AddComponent<RectTransform>();
            skipTextRect.anchorMin = Vector2.zero;
            skipTextRect.anchorMax = Vector2.one;
            skipTextRect.offsetMin = Vector2.zero;
            skipTextRect.offsetMax = Vector2.zero;
            var skipTMP = skipTextGO.AddComponent<TextMeshProUGUI>();
            skipTMP.text = "Skip";
            skipTMP.fontSize = 18;
            skipTMP.alignment = TextAlignmentOptions.Center;
            skipTMP.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            skipTMP.raycastTarget = false;

            // Add TutorialUI component & wire refs
            var tutComp = root.AddComponent<UI.TutorialUI>();
            var so = new SerializedObject(tutComp);
            var panelProp = so.FindProperty("_panel");
            if (panelProp != null) panelProp.objectReferenceValue = root;

            var overlayProp = so.FindProperty("_overlay");
            if (overlayProp != null) overlayProp.objectReferenceValue = overlay;

            var msgProp = so.FindProperty("_messageText");
            if (msgProp != null) msgProp.objectReferenceValue = msgTMP;

            var counterProp = so.FindProperty("_stepCounterText");
            if (counterProp != null) counterProp.objectReferenceValue = counterTMP;

            var okProp = so.FindProperty("_okButton");
            if (okProp != null) okProp.objectReferenceValue = okBtn;

            var skipProp = so.FindProperty("_skipButton");
            if (skipProp != null) skipProp.objectReferenceValue = skipBtn;

            so.ApplyModifiedPropertiesWithoutUndo();

            // Start hidden
            root.SetActive(true);
            // The panel hides itself in Awake

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(canvas.gameObject);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Log("Created TutorialUI panel under Canvas.");
        }

        // ======================== LEVEL MANAGER LEVELS LIST ========================

        private void FixLevelManagerLevelsList()
        {
            var levelManager = Object.FindObjectOfType<Level.LevelManager>(true);
            if (levelManager == null)
            {
                Log("LevelManager not found in scene.");
                return;
            }

            // Find all LevelData assets sorted by index
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/_Game/ScriptableObjects/Levels" });
            var allLevels = new System.Collections.Generic.List<Level.LevelData>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var ld = AssetDatabase.LoadAssetAtPath<Level.LevelData>(path);
                if (ld != null) allLevels.Add(ld);
            }

            allLevels.Sort((a, b) => a.LevelIndex.CompareTo(b.LevelIndex));

            var so = new SerializedObject(levelManager);
            var levelsProp = so.FindProperty("_levels");
            if (levelsProp == null)
            {
                Log("LevelManager._levels field not found.");
                return;
            }

            levelsProp.arraySize = allLevels.Count;
            for (int i = 0; i < allLevels.Count; i++)
            {
                levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = allLevels[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(levelManager);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Log($"Fixed LevelManager: populated _levels with {allLevels.Count} LevelData assets.");
        }

        // ======================== HELPER: Ensure Folder ========================

        private void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = System.IO.Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}

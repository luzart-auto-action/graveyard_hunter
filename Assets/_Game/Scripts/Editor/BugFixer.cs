using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.AI;

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
                "10. Auto-sets AllowedEnemyTypes in LevelData assets",
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
            string path = "Assets/_Game/Prefabs/Environment/Treasure.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Log("Treasure prefab not found at: " + path);
                return;
            }

            // Open prefab for editing
            var root = PrefabUtility.LoadPrefabContents(path);

            // Check if TreasurePickup already exists
            if (root.GetComponent<Level.TreasurePickup>() != null)
            {
                Log("Treasure prefab already has TreasurePickup. Skipped.");
                PrefabUtility.UnloadPrefabContents(root);
                return;
            }

            var pickup = root.AddComponent<Level.TreasurePickup>();

            // Assign serialized references
            var so = new SerializedObject(pickup);

            // Find VisualRoot child
            var visualRoot = root.transform.Find("VisualRoot");
            if (visualRoot != null)
            {
                var prop = so.FindProperty("_visualRoot");
                if (prop != null) prop.objectReferenceValue = visualRoot;
                Log("  -> Assigned _visualRoot");
            }

            // Find GlowLight child
            var glowLightGO = root.transform.Find("GlowLight");
            if (glowLightGO != null)
            {
                var light = glowLightGO.GetComponent<Light>();
                if (light != null)
                {
                    var prop = so.FindProperty("_glowLight");
                    if (prop != null) prop.objectReferenceValue = light;
                    Log("  -> Assigned _glowLight");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);

            Log("Fixed Treasure prefab: added TreasurePickup script with references.");
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

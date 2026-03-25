using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace GraveyardHunter.Editor
{
    public class PrefabCreator : EditorWindow
    {
        private static readonly string PrefabsPath = "Assets/_Game/Prefabs";
        private Vector2 _scrollPos;
        private List<string> _log = new List<string>();

        private bool _createPlayer = true;
        private bool _createGhost = true;
        private bool _createTreasure = true;
        private bool _createExitGate = true;
        private bool _createWall = true;
        private bool _createFloor = true;
        private bool _createSpikeTrap = true;
        private bool _createNoiseTrap = true;
        private bool _createLightBurstTrap = true;
        private bool _createSmokeBomb = true;
        private bool _createSpeedBoots = true;
        private bool _createShadowCloak = true;
        private bool _createGhostVision = true;
        private bool _createObstacle = true;
        private bool _createWerewolf = true;
        private bool _createMonster = true;
        private bool _createRobot = true;

        [MenuItem("GraveyardHunter/Prefab Creator")]
        public static void ShowWindow()
        {
            GetWindow<PrefabCreator>("Prefab Creator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Graveyard Hunter - Prefab Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Select Prefabs to Create:", EditorStyles.boldLabel);

            _createPlayer = EditorGUILayout.Toggle("Player", _createPlayer);
            _createGhost = EditorGUILayout.Toggle("Ghost", _createGhost);
            _createTreasure = EditorGUILayout.Toggle("Treasure", _createTreasure);
            _createExitGate = EditorGUILayout.Toggle("ExitGate", _createExitGate);
            _createWall = EditorGUILayout.Toggle("Wall", _createWall);
            _createFloor = EditorGUILayout.Toggle("Floor", _createFloor);
            _createSpikeTrap = EditorGUILayout.Toggle("SpikeTrap", _createSpikeTrap);
            _createNoiseTrap = EditorGUILayout.Toggle("NoiseTrap", _createNoiseTrap);
            _createLightBurstTrap = EditorGUILayout.Toggle("LightBurstTrap", _createLightBurstTrap);
            _createSmokeBomb = EditorGUILayout.Toggle("SmokeBomb", _createSmokeBomb);
            _createSpeedBoots = EditorGUILayout.Toggle("SpeedBoots", _createSpeedBoots);
            _createShadowCloak = EditorGUILayout.Toggle("ShadowCloak", _createShadowCloak);
            _createGhostVision = EditorGUILayout.Toggle("GhostVision", _createGhostVision);
            _createObstacle = EditorGUILayout.Toggle("Obstacle (Shelter)", _createObstacle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Enemy Variants:", EditorStyles.boldLabel);
            _createWerewolf = EditorGUILayout.Toggle("Werewolf", _createWerewolf);
            _createMonster = EditorGUILayout.Toggle("Monster", _createMonster);
            _createRobot = EditorGUILayout.Toggle("Robot", _createRobot);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Create All", GUILayout.Height(35)))
            {
                _log.Clear();
                CreateAllPrefabs();
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
            Debug.Log($"[PrefabCreator] {msg}");
        }

        /// <summary>Public static entry point for SetupAll to call.</summary>
        public static void CreateAllPrefabsStatic()
        {
            var instance = CreateInstance<PrefabCreator>();
            instance.CreateAllPrefabs();
            DestroyImmediate(instance);
        }

        private void CreateAllPrefabs()
        {
            MaterialFactory.CreateAllMaterials();

            if (_createPlayer) CreatePlayerPrefab();
            if (_createGhost) CreateGhostPrefab();
            if (_createTreasure) CreateTreasurePrefab();
            if (_createExitGate) CreateExitGatePrefab();
            if (_createWall) CreateWallPrefab();
            if (_createFloor) CreateFloorPrefab();
            if (_createSpikeTrap) CreateSpikeTrapPrefab();
            if (_createNoiseTrap) CreateNoiseTrapPrefab();
            if (_createLightBurstTrap) CreateLightBurstTrapPrefab();
            if (_createSmokeBomb) CreateBoosterPrefab<Booster.SmokeBombBooster>("SmokeBomb", "Booster_Smoke", Core.BoosterType.SmokeBomb, PrimitiveType.Sphere);
            if (_createSpeedBoots) CreateBoosterPrefab<Booster.SpeedBootsBooster>("SpeedBoots", "Booster_Speed", Core.BoosterType.SpeedBoots, PrimitiveType.Cube);
            if (_createShadowCloak) CreateBoosterPrefab<Booster.ShadowCloakBooster>("ShadowCloak", "Booster_Shadow", Core.BoosterType.ShadowCloak, PrimitiveType.Capsule);
            if (_createGhostVision) CreateBoosterPrefab<Booster.GhostVisionBooster>("GhostVision", "Booster_Vision", Core.BoosterType.GhostVision, PrimitiveType.Sphere);

            if (_createObstacle) CreateObstaclePrefab();

            if (_createWerewolf) CreateEnemyVariantPrefab("Werewolf", new Color(1f, 0.3f, 0f), new Color(1f, 0.6f, 0.4f));
            if (_createMonster) CreateEnemyVariantPrefab("Monster", new Color(0.4f, 1f, 0.4f), new Color(0.5f, 1f, 0.5f));
            if (_createRobot) CreateEnemyVariantPrefab("Robot", new Color(0.3f, 0.7f, 1f), new Color(0.4f, 0.7f, 1f));

            AutoAssignGameConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Log("--- Prefab creation complete! ---");
        }

        // ======================== PLAYER ========================

        private void CreatePlayerPrefab()
        {
            string path = $"{PrefabsPath}/Characters/Player.prefab";
            if (PrefabExists(path)) { Log("Player prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Characters");

            var root = new GameObject("Player");
            SafeSetTag(root, "Player");
            // CharacterController is auto-added by [RequireComponent] on PlayerController
            root.AddComponent<Player.PlayerController>();
            root.AddComponent<Player.PlayerHealth>();
            root.AddComponent<Player.PlayerInventory>();
            var cc = root.GetComponent<CharacterController>();
            if (cc == null) cc = root.AddComponent<CharacterController>();
            cc.radius = 0.4f;
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // VisualRoot
            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Capsule, "Player_Body");
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            // Assign _visualRoot on PlayerController
            var pcSO = new SerializedObject(root.GetComponent<Player.PlayerController>());
            AssignRef(pcSO, "_visualRoot", visual.transform);
            pcSO.ApplyModifiedPropertiesWithoutUndo();

            // PlayerLight
            var playerLightGO = new GameObject("PlayerLight");
            playerLightGO.transform.SetParent(root.transform);
            playerLightGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var plsComp = playerLightGO.AddComponent<Player.PlayerLightSystem>();

            var pointLight = new GameObject("PointLight");
            pointLight.transform.SetParent(playerLightGO.transform);
            pointLight.transform.localPosition = Vector3.zero;
            var pl = pointLight.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.range = 3f;
            pl.intensity = 1.5f;
            pl.color = new Color(1f, 0.92f, 0.7f, 1f);

            var spotLight = new GameObject("SpotLight");
            spotLight.transform.SetParent(playerLightGO.transform);
            spotLight.transform.localPosition = Vector3.zero;
            var sl = spotLight.AddComponent<Light>();
            sl.type = LightType.Spot;
            sl.range = 8f;
            sl.spotAngle = 30f;
            sl.intensity = 2f;
            sl.color = new Color(1f, 0.92f, 0.7f, 1f);

            var plsSO = new SerializedObject(plsComp);
            AssignRef(plsSO, "_pointLight", pl);
            AssignRef(plsSO, "_spotLight", sl);
            plsSO.ApplyModifiedPropertiesWithoutUndo();

            // FX Points
            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.9f, 0f));
            CreateFXPoint("FX_Top", root.transform, new Vector3(0f, 1.8f, 0f));
            CreateFXPoint("FX_Bottom", root.transform, Vector3.zero);

            SavePrefab(root, path);
            Log("Created Player prefab.");
        }

        // ======================== GHOST ========================

        private void CreateGhostPrefab()
        {
            string path = $"{PrefabsPath}/Characters/Ghost.prefab";
            if (PrefabExists(path)) { Log("Ghost prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Characters");

            var root = new GameObject("Ghost");
            var ghostComp = root.AddComponent<Enemy.LightGhost>();
            var agent = root.AddComponent<NavMeshAgent>();
            agent.speed = 2.5f;
            agent.angularSpeed = 180f;
            agent.radius = 0.4f;
            agent.height = 1.8f;

            // VisualRoot
            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Capsule, "Ghost_Body");
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            // Assign _visualRoot
            var ghostSO = new SerializedObject(ghostComp);
            AssignRef(ghostSO, "_visualRoot", visual.transform);

            // Eyes
            var eyesGO = new GameObject("Eyes");
            eyesGO.transform.SetParent(root.transform);
            eyesGO.transform.localPosition = new Vector3(0f, 1.5f, 0.3f);
            var eyesComp = eyesGO.AddComponent<Enemy.GhostEyes>();

            var leftEye = new GameObject("LeftEye");
            leftEye.transform.SetParent(eyesGO.transform);
            leftEye.transform.localPosition = new Vector3(-0.15f, 0f, 0f);
            var leftLight = leftEye.AddComponent<Light>();
            leftLight.type = LightType.Point;
            leftLight.range = 1f;
            leftLight.intensity = 1f;
            leftLight.color = new Color(1f, 0.84f, 0f, 1f);

            var rightEye = new GameObject("RightEye");
            rightEye.transform.SetParent(eyesGO.transform);
            rightEye.transform.localPosition = new Vector3(0.15f, 0f, 0f);
            var rightLight = rightEye.AddComponent<Light>();
            rightLight.type = LightType.Point;
            rightLight.range = 1f;
            rightLight.intensity = 1f;
            rightLight.color = new Color(1f, 0.84f, 0f, 1f);

            var eyesSO = new SerializedObject(eyesComp);
            AssignRef(eyesSO, "_leftEye", leftLight);
            AssignRef(eyesSO, "_rightEye", rightLight);
            eyesSO.ApplyModifiedPropertiesWithoutUndo();

            // LightCone
            var lightConeGO = new GameObject("LightCone");
            lightConeGO.transform.SetParent(root.transform);
            lightConeGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var lightConeComp = lightConeGO.AddComponent<Enemy.GhostLightCone>();

            var coneSpotGO = new GameObject("ConeSpotLight");
            coneSpotGO.transform.SetParent(lightConeGO.transform);
            coneSpotGO.transform.localPosition = Vector3.zero;
            var coneSpot = coneSpotGO.AddComponent<Light>();
            coneSpot.type = LightType.Spot;
            coneSpot.spotAngle = 80f;
            coneSpot.range = 10f;
            coneSpot.intensity = 2f;
            coneSpot.color = new Color(0.9f, 0.9f, 1f, 1f);

            var coneSO = new SerializedObject(lightConeComp);
            AssignRef(coneSO, "_spotLight", coneSpot);
            coneSO.ApplyModifiedPropertiesWithoutUndo();

            AssignRef(ghostSO, "_lightCone", lightConeComp);
            ghostSO.ApplyModifiedPropertiesWithoutUndo();

            // FX Points
            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.9f, 0f));

            SavePrefab(root, path);
            Log("Created Ghost prefab.");
        }

        // ======================== ENEMY VARIANTS ========================

        private void CreateEnemyVariantPrefab(string enemyName, Color eyeColor, Color coneLightColor)
        {
            string path = $"{PrefabsPath}/Characters/{enemyName}.prefab";
            if (PrefabExists(path)) { Log($"{enemyName} prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Characters");

            var root = new GameObject(enemyName);
            var ghostComp = root.AddComponent<Enemy.LightGhost>();
            var agent = root.AddComponent<NavMeshAgent>();
            agent.speed = 2.5f;
            agent.angularSpeed = 180f;
            agent.radius = 0.4f;
            agent.height = 1.8f;

            // VisualRoot - use capsule as placeholder, model will be added via BugFixer
            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Capsule, $"{enemyName}_Body");
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var ghostSO = new SerializedObject(ghostComp);
            AssignRef(ghostSO, "_visualRoot", visual.transform);

            // Eyes with custom color
            var eyesGO = new GameObject("Eyes");
            eyesGO.transform.SetParent(root.transform);
            eyesGO.transform.localPosition = new Vector3(0f, 1.5f, 0.3f);
            var eyesComp = eyesGO.AddComponent<Enemy.GhostEyes>();

            var leftEye = new GameObject("LeftEye");
            leftEye.transform.SetParent(eyesGO.transform);
            leftEye.transform.localPosition = new Vector3(-0.15f, 0f, 0f);
            var leftLight = leftEye.AddComponent<Light>();
            leftLight.type = LightType.Point;
            leftLight.range = 1f;
            leftLight.intensity = 1f;
            leftLight.color = eyeColor;

            var rightEye = new GameObject("RightEye");
            rightEye.transform.SetParent(eyesGO.transform);
            rightEye.transform.localPosition = new Vector3(0.15f, 0f, 0f);
            var rightLight = rightEye.AddComponent<Light>();
            rightLight.type = LightType.Point;
            rightLight.range = 1f;
            rightLight.intensity = 1f;
            rightLight.color = eyeColor;

            var eyesSO = new SerializedObject(eyesComp);
            AssignRef(eyesSO, "_leftEye", leftLight);
            AssignRef(eyesSO, "_rightEye", rightLight);
            eyesSO.ApplyModifiedPropertiesWithoutUndo();

            // LightCone with custom color
            var lightConeGO = new GameObject("LightCone");
            lightConeGO.transform.SetParent(root.transform);
            lightConeGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var lightConeComp = lightConeGO.AddComponent<Enemy.GhostLightCone>();

            var coneSpotGO = new GameObject("ConeSpotLight");
            coneSpotGO.transform.SetParent(lightConeGO.transform);
            coneSpotGO.transform.localPosition = Vector3.zero;
            var coneSpot = coneSpotGO.AddComponent<Light>();
            coneSpot.type = LightType.Spot;
            coneSpot.spotAngle = 80f;
            coneSpot.range = 10f;
            coneSpot.intensity = 2f;
            coneSpot.color = coneLightColor;

            var coneSO = new SerializedObject(lightConeComp);
            AssignRef(coneSO, "_spotLight", coneSpot);
            coneSO.ApplyModifiedPropertiesWithoutUndo();

            AssignRef(ghostSO, "_lightCone", lightConeComp);
            ghostSO.ApplyModifiedPropertiesWithoutUndo();

            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.9f, 0f));

            SavePrefab(root, path);
            Log($"Created {enemyName} prefab.");
        }

        // ======================== TREASURE (all types) ========================

        private void CreateTreasurePrefab()
        {
            // Legacy Gold prefab (for backward compatibility)
            CreateTypedTreasurePrefab("Treasure", Core.TreasureType.Gold, PrimitiveType.Cube,
                "Treasure_Gold", new Color(1f, 0.84f, 0f, 1f), new Vector3(0.5f, 0.5f, 0.5f));

            // Per-type prefabs
            CreateTypedTreasurePrefab("Treasure_Gold", Core.TreasureType.Gold, PrimitiveType.Cube,
                "Treasure_Gold", new Color(1f, 0.84f, 0f, 1f), new Vector3(0.5f, 0.5f, 0.5f));

            CreateTypedTreasurePrefab("Treasure_Silver", Core.TreasureType.Silver, PrimitiveType.Sphere,
                "Treasure_Silver", new Color(0.75f, 0.75f, 0.82f, 1f), new Vector3(0.45f, 0.45f, 0.45f));

            CreateTypedTreasurePrefab("Treasure_Coin", Core.TreasureType.Coin, PrimitiveType.Cylinder,
                "Treasure_Coin", new Color(0.8f, 0.5f, 0.2f, 1f), new Vector3(0.5f, 0.1f, 0.5f));

            CreateTypedTreasurePrefab("Treasure_Artifact", Core.TreasureType.Artifact, PrimitiveType.Cube,
                "Treasure_Artifact", new Color(0.6f, 0.2f, 1f, 1f), new Vector3(0.35f, 0.55f, 0.35f));
        }

        private void CreateTypedTreasurePrefab(string prefabName, Core.TreasureType type,
            PrimitiveType shape, string materialName, Color glowColor, Vector3 visualScale)
        {
            string path = $"{PrefabsPath}/Environment/{prefabName}.prefab";
            if (PrefabExists(path)) { Log($"{prefabName} prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Environment");

            var root = new GameObject(prefabName);
            SafeSetTag(root, "Treasure");
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 1f, 1f);

            var pickupComp = root.AddComponent<Level.TreasurePickup>();

            var visual = CreatePrimitive("VisualRoot", root.transform, shape, materialName);
            visual.transform.localScale = visualScale;
            visual.transform.localPosition = new Vector3(0f, 0.3f, 0f);

            // For Artifact: rotate 45° to make diamond shape
            if (type == Core.TreasureType.Artifact)
                visual.transform.localRotation = Quaternion.Euler(0f, 45f, 45f);

            // Glow light with type-specific color
            var glowGO = new GameObject("GlowLight");
            glowGO.transform.SetParent(root.transform);
            glowGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var glow = glowGO.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 3f;
            glow.intensity = 1f;
            glow.color = glowColor;

            // Assign TreasurePickup references
            var pickupSO = new SerializedObject(pickupComp);
            AssignRef(pickupSO, "_visualRoot", visual.transform);
            AssignRef(pickupSO, "_glowLight", glow);

            // Set treasure type
            var typeProp = pickupSO.FindProperty("_treasureType");
            if (typeProp != null)
                typeProp.enumValueIndex = (int)type;

            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.5f, 0f));

            SavePrefab(root, path);
            Log($"Created {prefabName} prefab (type={type}, shape={shape}).");
        }

        // ======================== EXIT GATE ========================

        private void CreateExitGatePrefab()
        {
            string path = $"{PrefabsPath}/Environment/ExitGate.prefab";
            if (PrefabExists(path)) { Log("ExitGate prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Environment");

            var root = new GameObject("ExitGate");
            SafeSetTag(root, "ExitGate");
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2f, 3f, 0.5f);

            // ExitGate script for interaction logic
            var gateComp = root.AddComponent<Level.ExitGate>();

            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Cube, "ExitGate_Closed");
            visual.transform.localScale = new Vector3(2f, 3f, 0.3f);
            visual.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            // Gate light (disabled)
            var gateLightGO = new GameObject("GateLight");
            gateLightGO.transform.SetParent(root.transform);
            gateLightGO.transform.localPosition = new Vector3(0f, 2f, 0f);
            var gateLight = gateLightGO.AddComponent<Light>();
            gateLight.type = LightType.Point;
            gateLight.range = 3f;
            gateLight.intensity = 1.5f;
            gateLight.color = new Color(0f, 1f, 0.27f, 1f);
            gateLight.enabled = false;

            // Assign ExitGate references
            var gateSO = new SerializedObject(gateComp);
            AssignRef(gateSO, "_visualRoot", visual.transform);
            AssignRef(gateSO, "_gateLight", gateLight);
            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null) AssignRef(gateSO, "_gateRenderer", renderer);
            gateSO.ApplyModifiedPropertiesWithoutUndo();

            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 1.5f, 0f));

            SavePrefab(root, path);
            Log("Created ExitGate prefab.");
        }

        // ======================== WALL ========================

        private void CreateWallPrefab()
        {
            string path = $"{PrefabsPath}/Environment/Wall.prefab";
            if (PrefabExists(path)) { Log("Wall prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Environment");

            var root = new GameObject("Wall");
            var col = root.AddComponent<BoxCollider>();
            col.size = new Vector3(2f, 3f, 2f);
            col.center = new Vector3(0f, 1.5f, 0f);

            // Walls are excluded from NavMesh via bake markups in LevelManager
            // No NavMeshObstacle needed

            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Cube, "Wall_Stone");
            visual.transform.localScale = new Vector3(2f, 3f, 2f);
            visual.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            SavePrefab(root, path);
            Log("Created Wall prefab.");
        }

        // ======================== FLOOR ========================

        private void CreateFloorPrefab()
        {
            string path = $"{PrefabsPath}/Environment/Floor.prefab";
            if (PrefabExists(path)) { Log("Floor prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Environment");

            var root = new GameObject("Floor");

            // BoxCollider for NavMesh walkable surface
            var col = root.AddComponent<BoxCollider>();
            col.size = new Vector3(2f, 0.1f, 2f);
            col.center = new Vector3(0f, -0.05f, 0f);

            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Quad, "Floor_Dark");
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            visual.transform.localScale = new Vector3(2f, 2f, 1f);

            SavePrefab(root, path);
            Log("Created Floor prefab.");
        }

        // ======================== SPIKE TRAP ========================

        private void CreateSpikeTrapPrefab()
        {
            string path = $"{PrefabsPath}/Traps/SpikeTrap.prefab";
            if (PrefabExists(path)) { Log("SpikeTrap prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Traps");

            var root = new GameObject("SpikeTrap");
            root.AddComponent<Trap.SpikeTrap>();
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 0.5f, 1f);

            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Cylinder, "Trap_Spike");
            visual.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            visual.transform.localPosition = Vector3.zero;

            // Spike child
            var spike = CreatePrimitive("Spike", root.transform, PrimitiveType.Cylinder, "Trap_Spike");
            spike.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            spike.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            // Assign _spikesTransform
            var trapSO = new SerializedObject(root.GetComponent<Trap.SpikeTrap>());
            AssignRef(trapSO, "_spikesTransform", spike.transform);
            trapSO.ApplyModifiedPropertiesWithoutUndo();

            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.5f, 0f));

            SavePrefab(root, path);
            Log("Created SpikeTrap prefab.");
        }

        // ======================== NOISE TRAP ========================

        private void CreateNoiseTrapPrefab()
        {
            string path = $"{PrefabsPath}/Traps/NoiseTrap.prefab";
            if (PrefabExists(path)) { Log("NoiseTrap prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Traps");

            var root = new GameObject("NoiseTrap");
            root.AddComponent<Trap.NoiseTrap>();
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 0.5f, 1f);

            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Cube, "Trap_Noise");
            visual.transform.localScale = new Vector3(0.4f, 0.2f, 0.4f);
            visual.transform.localPosition = new Vector3(0f, 0.1f, 0f);

            // Assign _visualTransform
            var trapSO = new SerializedObject(root.GetComponent<Trap.NoiseTrap>());
            AssignRef(trapSO, "_visualTransform", visual.transform);
            trapSO.ApplyModifiedPropertiesWithoutUndo();

            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.3f, 0f));

            SavePrefab(root, path);
            Log("Created NoiseTrap prefab.");
        }

        // ======================== LIGHT BURST TRAP ========================

        private void CreateLightBurstTrapPrefab()
        {
            string path = $"{PrefabsPath}/Traps/LightBurstTrap.prefab";
            if (PrefabExists(path)) { Log("LightBurstTrap prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Traps");

            var root = new GameObject("LightBurstTrap");
            var lbtComp = root.AddComponent<Trap.LightBurstTrap>();
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 0.5f, 1f);

            var visual = CreatePrimitive("VisualRoot", root.transform, PrimitiveType.Sphere, "Trap_LightBurst");
            visual.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            visual.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            // Burst Light (disabled)
            var burstGO = new GameObject("BurstLight");
            burstGO.transform.SetParent(root.transform);
            burstGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var burstLight = burstGO.AddComponent<Light>();
            burstLight.type = LightType.Point;
            burstLight.range = 8f;
            burstLight.intensity = 0f;
            burstLight.color = Color.yellow;
            burstLight.enabled = false;

            var lbtSO = new SerializedObject(lbtComp);
            AssignRef(lbtSO, "_burstLight", burstLight);
            lbtSO.ApplyModifiedPropertiesWithoutUndo();

            CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.5f, 0f));

            SavePrefab(root, path);
            Log("Created LightBurstTrap prefab.");
        }

        // ======================== BOOSTERS (Generic) ========================

        private void CreateBoosterPrefab<T>(string name, string materialName, Core.BoosterType boosterType, PrimitiveType shape) where T : Booster.BoosterBase
        {
            string path = $"{PrefabsPath}/Boosters/{name}.prefab";
            if (PrefabExists(path)) { Log($"{name} prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Boosters");

            var root = new GameObject(name);
            var boosterComp = root.AddComponent<T>();
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 1f, 1f);

            var visual = CreatePrimitive("VisualRoot", root.transform, shape, materialName);
            visual.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            // Assign booster fields
            var boosterSO = new SerializedObject(boosterComp);
            var typeProp = boosterSO.FindProperty("_boosterType");
            if (typeProp != null)
                typeProp.enumValueIndex = (int)boosterType;
            AssignRef(boosterSO, "_visualRoot", visual.transform);

            // FX Point
            var fxPoint = CreateFXPoint("FX_Center", root.transform, new Vector3(0f, 0.5f, 0f));
            AssignRef(boosterSO, "_fxPoint", fxPoint.transform);
            boosterSO.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, path);
            Log($"Created {name} prefab.");
        }

        // ======================== OBSTACLE (Shelter) ========================

        private void CreateObstaclePrefab()
        {
            string path = $"{PrefabsPath}/Environment/Obstacle.prefab";
            if (PrefabExists(path)) { Log("Obstacle prefab already exists."); return; }
            EnsureFolder($"{PrefabsPath}/Environment");

            var root = new GameObject("Obstacle");

            // Large trigger collider — bigger than 1 maze cell (2 units) so player enters easily
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2.5f, 3f, 2.5f);
            col.center = new Vector3(0f, 1.5f, 0f);

            // ObstacleShelter script
            root.AddComponent<Level.ObstacleShelter>();

            // === Visual: ruined shelter (3-walled structure with roof) ===

            // Back wall (tall, wide)
            var backWall = CreatePrimitive("BackWall", root.transform, PrimitiveType.Cube, "Obstacle_Stone");
            backWall.transform.localScale = new Vector3(1.8f, 2.5f, 0.3f);
            backWall.transform.localPosition = new Vector3(0f, 1.25f, 0.8f);

            // Left wall
            var leftWall = CreatePrimitive("LeftWall", root.transform, PrimitiveType.Cube, "Obstacle_Stone");
            leftWall.transform.localScale = new Vector3(0.3f, 2.0f, 1.4f);
            leftWall.transform.localPosition = new Vector3(-0.8f, 1.0f, 0.1f);

            // Right wall
            var rightWall = CreatePrimitive("RightWall", root.transform, PrimitiveType.Cube, "Obstacle_Stone");
            rightWall.transform.localScale = new Vector3(0.3f, 2.0f, 1.4f);
            rightWall.transform.localPosition = new Vector3(0.8f, 1.0f, 0.1f);

            // Roof slab
            var roof = CreatePrimitive("Roof", root.transform, PrimitiveType.Cube, "Obstacle_Stone");
            roof.transform.localScale = new Vector3(1.8f, 0.2f, 1.6f);
            roof.transform.localPosition = new Vector3(0f, 2.5f, 0.1f);

            // Glow light inside — visible from far away
            var glowGO = new GameObject("ShelterGlow");
            glowGO.transform.SetParent(root.transform);
            glowGO.transform.localPosition = new Vector3(0f, 1.2f, 0.3f);
            var glow = glowGO.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 5f;
            glow.intensity = 1.2f;
            glow.color = new Color(0.3f, 0.9f, 0.5f, 1f); // green safe glow

            SavePrefab(root, path);
            Log("Created Obstacle (Shelter) prefab.");
        }

        // ======================== AUTO-ASSIGN GAME CONFIG ========================

        private void AutoAssignGameConfig()
        {
            string configPath = "Assets/_Game/ScriptableObjects/Configs/GameConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>(configPath);
            if (config == null)
            {
                Log("GameConfig not found, skipping auto-assign.");
                return;
            }

            var so = new SerializedObject(config);

            TryAssignPrefab(so, "PlayerPrefab", $"{PrefabsPath}/Characters/Player.prefab");
            TryAssignPrefab(so, "GhostPrefab", $"{PrefabsPath}/Characters/Ghost.prefab");
            TryAssignPrefab(so, "TreasurePrefab", $"{PrefabsPath}/Environment/Treasure.prefab");
            TryAssignPrefab(so, "GoldTreasurePrefab", $"{PrefabsPath}/Environment/Treasure_Gold.prefab");
            TryAssignPrefab(so, "SilverTreasurePrefab", $"{PrefabsPath}/Environment/Treasure_Silver.prefab");
            TryAssignPrefab(so, "CoinTreasurePrefab", $"{PrefabsPath}/Environment/Treasure_Coin.prefab");
            TryAssignPrefab(so, "ArtifactTreasurePrefab", $"{PrefabsPath}/Environment/Treasure_Artifact.prefab");
            TryAssignPrefab(so, "ExitGatePrefab", $"{PrefabsPath}/Environment/ExitGate.prefab");
            TryAssignPrefab(so, "WallPrefab", $"{PrefabsPath}/Environment/Wall.prefab");
            TryAssignPrefab(so, "FloorPrefab", $"{PrefabsPath}/Environment/Floor.prefab");
            TryAssignPrefab(so, "SpikeTrapPrefab", $"{PrefabsPath}/Traps/SpikeTrap.prefab");
            TryAssignPrefab(so, "NoiseTrapPrefab", $"{PrefabsPath}/Traps/NoiseTrap.prefab");
            TryAssignPrefab(so, "LightBurstTrapPrefab", $"{PrefabsPath}/Traps/LightBurstTrap.prefab");
            TryAssignPrefab(so, "SmokeBombPrefab", $"{PrefabsPath}/Boosters/SmokeBomb.prefab");
            TryAssignPrefab(so, "SpeedBootsPrefab", $"{PrefabsPath}/Boosters/SpeedBoots.prefab");
            TryAssignPrefab(so, "ShadowCloakPrefab", $"{PrefabsPath}/Boosters/ShadowCloak.prefab");
            TryAssignPrefab(so, "GhostVisionPrefab", $"{PrefabsPath}/Boosters/GhostVision.prefab");
            TryAssignPrefab(so, "ObstaclePrefab", $"{PrefabsPath}/Environment/Obstacle.prefab");

            so.ApplyModifiedPropertiesWithoutUndo();

            // Auto-populate EnemyTypes list with defaults + prefab references
            AutoPopulateEnemyTypes(config);

            EditorUtility.SetDirty(config);
            Log("Auto-assigned prefab references in GameConfig.");
        }

        private void AutoPopulateEnemyTypes(Data.GameConfig config)
        {
            var enemyTypes = new System.Collections.Generic.Dictionary<Core.EnemyType, string>
            {
                { Core.EnemyType.Ghost, $"{PrefabsPath}/Characters/Ghost.prefab" },
                { Core.EnemyType.Werewolf, $"{PrefabsPath}/Characters/Werewolf.prefab" },
                { Core.EnemyType.Monster, $"{PrefabsPath}/Characters/Monster.prefab" },
                { Core.EnemyType.Robot, $"{PrefabsPath}/Characters/Robot.prefab" }
            };

            // Ensure list has entries for all types
            foreach (var kvp in enemyTypes)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(kvp.Value);
                if (prefab == null) continue;

                // Find existing entry or create new
                Data.EnemyTypeData existing = null;
                foreach (var data in config.EnemyTypes)
                {
                    if (data.Type == kvp.Key) { existing = data; break; }
                }

                if (existing == null)
                {
                    existing = Data.EnemyTypeData.GetDefault(kvp.Key);
                    config.EnemyTypes.Add(existing);
                }

                existing.Prefab = prefab;
            }

            Log($"Auto-populated {config.EnemyTypes.Count} enemy types in GameConfig.");
        }

        private void TryAssignPrefab(SerializedObject so, string fieldName, string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            var prop = so.FindProperty(fieldName);
            if (prop != null)
                prop.objectReferenceValue = prefab;
        }

        // ======================== UTILITY ========================

        private GameObject CreatePrimitive(string name, Transform parent, PrimitiveType type, string materialName)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            // Remove default collider from primitive (collider is on root)
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            // Apply material
            var mat = MaterialFactory.GetOrCreateMaterial(materialName);
            if (mat != null)
            {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.sharedMaterial = mat;
            }

            return go;
        }

        private GameObject CreateFXPoint(string name, Transform parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            var fxPoint = go.AddComponent<FX.FXSpawnPoint>();

            var fxSO = new SerializedObject(fxPoint);
            var nameProp = fxSO.FindProperty("_fxPointName");
            if (nameProp != null)
            {
                nameProp.stringValue = name;
                fxSO.ApplyModifiedPropertiesWithoutUndo();
            }

            return go;
        }

        private bool PrefabExists(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
        }

        private void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private void AssignRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null)
                prop.objectReferenceValue = value;
        }

        private void SafeSetTag(GameObject go, string tagName)
        {
            try { go.tag = tagName; }
            catch { Log($"Tag '{tagName}' not found. Please add it in Edit > Project Settings > Tags and Layers."); }
        }

        private void EnsureFolder(string path)
        {
            path = path.Replace("\\", "/");
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}

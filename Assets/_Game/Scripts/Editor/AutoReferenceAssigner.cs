using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace GraveyardHunter.Editor
{
    public class AutoReferenceAssigner : EditorWindow
    {
        private Vector2 _scrollPos;
        private List<string> _log = new List<string>();
        private int _fixedCount;

        [MenuItem("GraveyardHunter/Auto Reference Assigner")]
        public static void ShowWindow()
        {
            GetWindow<AutoReferenceAssigner>("Auto Reference Assigner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Auto Reference Assigner", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Scans the scene and assigns ALL missing references:\n" +
                "- UIManager panels & GameplayUI\n" +
                "- InputManager joystick\n" +
                "- AudioManager music source\n" +
                "- LevelManager config & levels\n" +
                "- ShopManager config\n" +
                "- All UI panel buttons/texts\n" +
                "- GameConfig prefab slots",
                MessageType.Info);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Assign All References", GUILayout.Height(40)))
            {
                _log.Clear();
                _fixedCount = 0;
                AssignAll();
                Log($"--- Done! {_fixedCount} references assigned. ---");
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Assign GameConfig Prefabs Only", GUILayout.Height(30)))
            {
                _log.Clear();
                _fixedCount = 0;
                AssignGameConfigPrefabs();
                Log($"--- Done! {_fixedCount} prefab references assigned. ---");
            }

            if (GUILayout.Button("Assign LevelManager Levels Only", GUILayout.Height(30)))
            {
                _log.Clear();
                _fixedCount = 0;
                AssignLevelManagerLevels();
                Log($"--- Done! {_fixedCount} level references assigned. ---");
            }

            EditorGUILayout.Space(10);

            if (_log.Count > 0)
            {
                EditorGUILayout.LabelField($"Log ({_fixedCount} assigned):", EditorStyles.boldLabel);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(300));
                foreach (var entry in _log)
                {
                    if (entry.StartsWith("FIXED"))
                        EditorGUILayout.LabelField(entry, EditorStyles.boldLabel);
                    else if (entry.StartsWith("OK"))
                        EditorGUILayout.LabelField(entry);
                    else if (entry.StartsWith("WARN"))
                        EditorGUILayout.HelpBox(entry, MessageType.Warning);
                    else
                        EditorGUILayout.LabelField(entry, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void Log(string msg) { _log.Add(msg); }

        private void AssignAll()
        {
            AssignUIManagerRefs();
            AssignInputManagerRefs();
            AssignAudioManagerRefs();
            AssignLevelManagerRefs();
            AssignShopManagerRefs();
            AssignUIMainMenuRefs();
            AssignGameplayUIRefs();
            AssignWinPanelRefs();
            AssignFailPanelRefs();
            AssignPopupPauseRefs();
            AssignPopupSettingsRefs();
            AssignShopPanelRefs();
            AssignJoystickRefs();
            AssignGameConfigPrefabs();
            AssignLevelManagerLevels();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // ==================== MANAGERS ====================

        private void AssignUIManagerRefs()
        {
            var comp = FindInScene<UI.UIManager>();
            if (comp == null) { Log("WARN: UIManager not found in scene"); return; }

            var so = new SerializedObject(comp);

            // _panels - find all UIPanel in scene
            var allPanels = Object.FindObjectsOfType<UI.UIPanel>(true);
            var panelsProp = so.FindProperty("_panels");
            if (panelsProp != null && (panelsProp.arraySize == 0 || HasNullInArray(panelsProp)))
            {
                panelsProp.arraySize = allPanels.Length;
                for (int i = 0; i < allPanels.Length; i++)
                    panelsProp.GetArrayElementAtIndex(i).objectReferenceValue = allPanels[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                _fixedCount++;
                Log($"FIXED: UIManager._panels = {allPanels.Length} panels");
            }
            else { Log($"OK: UIManager._panels ({allPanels.Length} panels)"); }

            // _gameplayUI (GameplayUI extends UIPanel now, but UIManager still has a direct reference)
            var gameplayUI = FindInScene<UI.GameplayUI>();
            AssignIfNull(so, "_gameplayUI", gameplayUI, "UIManager._gameplayUI");
            so.ApplyModifiedPropertiesWithoutUndo();

            // Also ensure GameplayUI _panelName is set
            if (gameplayUI != null)
            {
                var gpSO = new SerializedObject(gameplayUI);
                var nameProp = gpSO.FindProperty("_panelName");
                if (nameProp != null && string.IsNullOrEmpty(nameProp.stringValue))
                {
                    nameProp.stringValue = "GameplayUI";
                    gpSO.ApplyModifiedPropertiesWithoutUndo();
                    _fixedCount++;
                    Log("FIXED: GameplayUI._panelName = GameplayUI");
                }
            }
        }

        private void AssignInputManagerRefs()
        {
            var comp = FindInScene<Input.InputManager>();
            if (comp == null) { Log("WARN: InputManager not found in scene"); return; }

            var joystick = FindInScene<Input.VirtualJoystick>();
            var so = new SerializedObject(comp);
            AssignIfNull(so, "_joystick", joystick, "InputManager._joystick");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignAudioManagerRefs()
        {
            var comp = FindInScene<Audio.AudioManager>();
            if (comp == null) { Log("WARN: AudioManager not found in scene"); return; }

            var so = new SerializedObject(comp);
            var prop = so.FindProperty("_musicSource");
            if (prop != null && prop.objectReferenceValue == null)
            {
                // Find or create AudioSource on same GO
                var audioSource = comp.GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = comp.gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = true;
                prop.objectReferenceValue = audioSource;
                so.ApplyModifiedPropertiesWithoutUndo();
                _fixedCount++;
                Log("FIXED: AudioManager._musicSource");
            }
            else { Log("OK: AudioManager._musicSource"); }
        }

        private void AssignLevelManagerRefs()
        {
            var comp = FindInScene<Level.LevelManager>();
            if (comp == null) { Log("WARN: LevelManager not found in scene"); return; }

            var so = new SerializedObject(comp);

            // _gameConfig
            var config = LoadAsset<Data.GameConfig>("Assets/_Game/ScriptableObjects/Configs/GameConfig.asset");
            AssignIfNull(so, "_gameConfig", config, "LevelManager._gameConfig");

            // _levels - auto-load from folder
            AssignLevelManagerLevelsInternal(so);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignShopManagerRefs()
        {
            var comp = FindInScene<Shop.ShopManager>();
            if (comp == null) { Log("WARN: ShopManager not found in scene"); return; }

            var so = new SerializedObject(comp);
            var config = LoadAsset<Data.GameConfig>("Assets/_Game/ScriptableObjects/Configs/GameConfig.asset");
            AssignIfNull(so, "_gameConfig", config, "ShopManager._gameConfig");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ==================== UI PANELS ====================

        private void AssignUIMainMenuRefs()
        {
            var comp = FindInScene<UI.UIMainMenu>();
            if (comp == null) { Log("WARN: UIMainMenu not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignButtonByName(so, "_playButton", comp.transform, "PlayButton");
            AssignButtonByName(so, "_settingsButton", comp.transform, "SettingsButton");
            AssignButtonByName(so, "_shopButton", comp.transform, "ShopButton");
            AssignTMPByName(so, "_levelText", comp.transform, "LevelText");
            AssignTMPByName(so, "_totalScoreText", comp.transform, "TotalScoreText");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignGameplayUIRefs()
        {
            var comp = FindInScene<UI.GameplayUI>();
            if (comp == null) { Log("WARN: GameplayUI not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignButtonByName(so, "_pauseButton", comp.transform, "PauseButton");
            AssignButtonByName(so, "_resetButton", comp.transform, "ResetButton");
            AssignTMPByName(so, "_levelText", comp.transform, "LevelText");
            AssignTMPByName(so, "_hpText", comp.transform, "HPText");
            AssignTMPByName(so, "_treasureText", comp.transform, "TreasureText");
            AssignTMPByName(so, "_scoreText", comp.transform, "ScoreText");
            AssignGOByName(so, "_escapeIndicator", comp.transform, "EscapeIndicator");
            AssignGOByName(so, "_boosterTimerUI", comp.transform, "BoosterTimerUI");
            AssignTMPByName(so, "_boosterNameText", comp.transform, "BoosterNameText");

            // _boosterTimerFill
            var fillProp = so.FindProperty("_boosterTimerFill");
            if (fillProp != null && fillProp.objectReferenceValue == null)
            {
                var fill = FindChildDeep(comp.transform, "BoosterFill");
                if (fill != null)
                {
                    fillProp.objectReferenceValue = fill.GetComponent<Image>();
                    _fixedCount++;
                    Log("FIXED: GameplayUI._boosterTimerFill");
                }
            }

            // _hpIcons
            var hpIconsProp = so.FindProperty("_hpIcons");
            if (hpIconsProp != null && (hpIconsProp.arraySize == 0 || HasNullInArray(hpIconsProp)))
            {
                var iconsParent = FindChildDeep(comp.transform, "HPIcons");
                if (iconsParent != null)
                {
                    var icons = new List<Image>();
                    for (int i = 0; i < iconsParent.childCount; i++)
                    {
                        var img = iconsParent.GetChild(i).GetComponent<Image>();
                        if (img != null) icons.Add(img);
                    }
                    hpIconsProp.arraySize = icons.Count;
                    for (int i = 0; i < icons.Count; i++)
                        hpIconsProp.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];
                    _fixedCount++;
                    Log($"FIXED: GameplayUI._hpIcons = {icons.Count} icons");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignWinPanelRefs()
        {
            var comp = FindInScene<UI.WinPanel>();
            if (comp == null) { Log("WARN: WinPanel not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignTMPByName(so, "_scoreText", comp.transform, "ScoreText");
            AssignTMPByName(so, "_timeText", comp.transform, "TimeText");
            AssignTMPByName(so, "_levelText", comp.transform, "LevelText");
            AssignButtonByName(so, "_nextButton", comp.transform, "NextButton");
            AssignButtonByName(so, "_retryButton", comp.transform, "RetryButton");
            AssignButtonByName(so, "_homeButton", comp.transform, "HomeButton");

            // _stars
            var starsProp = so.FindProperty("_stars");
            if (starsProp != null && (starsProp.arraySize == 0 || HasNullInArray(starsProp)))
            {
                var stars = new List<Image>();
                for (int i = 0; i < 3; i++)
                {
                    var star = FindChildDeep(comp.transform, $"Star_{i}");
                    if (star != null)
                    {
                        var img = star.GetComponent<Image>();
                        if (img != null) stars.Add(img);
                    }
                }
                if (stars.Count > 0)
                {
                    starsProp.arraySize = stars.Count;
                    for (int i = 0; i < stars.Count; i++)
                        starsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
                    _fixedCount++;
                    Log($"FIXED: WinPanel._stars = {stars.Count} stars");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignFailPanelRefs()
        {
            var comp = FindInScene<UI.FailPanel>();
            if (comp == null) { Log("WARN: FailPanel not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignTMPByName(so, "_failReasonText", comp.transform, "ReasonText");
            AssignTMPByName(so, "_levelText", comp.transform, "LevelText");
            AssignButtonByName(so, "_retryButton", comp.transform, "RetryButton");
            AssignButtonByName(so, "_homeButton", comp.transform, "HomeButton");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignPopupPauseRefs()
        {
            var comp = FindInScene<UI.PopupPause>();
            if (comp == null) { Log("WARN: PopupPause not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignButtonByName(so, "_resumeButton", comp.transform, "ResumeButton");
            AssignButtonByName(so, "_homeButton", comp.transform, "HomeButton");
            AssignButtonByName(so, "_settingsButton", comp.transform, "SettingsButton");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignPopupSettingsRefs()
        {
            var comp = FindInScene<UI.PopupSettings>();
            if (comp == null) { Log("WARN: PopupSettings not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignSliderByName(so, "_sfxSlider", comp.transform, "SFXSlider");
            AssignSliderByName(so, "_musicSlider", comp.transform, "MusicSlider");
            AssignButtonByName(so, "_closeButton", comp.transform, "CloseButton");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignShopPanelRefs()
        {
            var comp = FindInScene<UI.ShopPanel>();
            if (comp == null) { Log("WARN: ShopPanel not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignTMPByName(so, "_totalScoreText", comp.transform, "TotalScoreText");
            AssignButtonByName(so, "_closeButton", comp.transform, "CloseButton");

            var skinParentProp = so.FindProperty("_skinItemParent");
            if (skinParentProp != null && skinParentProp.objectReferenceValue == null)
            {
                var content = FindChildDeep(comp.transform, "Content");
                if (content != null)
                {
                    skinParentProp.objectReferenceValue = content;
                    _fixedCount++;
                    Log("FIXED: ShopPanel._skinItemParent");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignJoystickRefs()
        {
            var comp = FindInScene<Input.VirtualJoystick>();
            if (comp == null) { Log("WARN: VirtualJoystick not found in scene"); return; }

            var so = new SerializedObject(comp);

            var bgProp = so.FindProperty("_joystickBackground");
            if (bgProp != null && bgProp.objectReferenceValue == null)
            {
                var bg = FindChildDeep(comp.transform, "JoystickBackground");
                if (bg != null)
                {
                    bgProp.objectReferenceValue = bg.GetComponent<RectTransform>();
                    _fixedCount++;
                    Log("FIXED: VirtualJoystick._joystickBackground");
                }
            }

            var handleProp = so.FindProperty("_joystickHandle");
            if (handleProp != null && handleProp.objectReferenceValue == null)
            {
                var handle = FindChildDeep(comp.transform, "JoystickHandle");
                if (handle != null)
                {
                    handleProp.objectReferenceValue = handle.GetComponent<RectTransform>();
                    _fixedCount++;
                    Log("FIXED: VirtualJoystick._joystickHandle");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ==================== GAME CONFIG PREFABS ====================

        private void AssignGameConfigPrefabs()
        {
            var config = LoadAsset<Data.GameConfig>("Assets/_Game/ScriptableObjects/Configs/GameConfig.asset");
            if (config == null) { Log("WARN: GameConfig.asset not found"); return; }

            var so = new SerializedObject(config);

            var prefabMap = new Dictionary<string, string>
            {
                { "PlayerPrefab", "Assets/_Game/Prefabs/Characters/Player.prefab" },
                { "GhostPrefab", "Assets/_Game/Prefabs/Characters/Ghost.prefab" },
                { "TreasurePrefab", "Assets/_Game/Prefabs/Environment/Treasure.prefab" },
                { "ExitGatePrefab", "Assets/_Game/Prefabs/Environment/ExitGate.prefab" },
                { "WallPrefab", "Assets/_Game/Prefabs/Environment/Wall.prefab" },
                { "FloorPrefab", "Assets/_Game/Prefabs/Environment/Floor.prefab" },
                { "SpikeTrapPrefab", "Assets/_Game/Prefabs/Traps/SpikeTrap.prefab" },
                { "NoiseTrapPrefab", "Assets/_Game/Prefabs/Traps/NoiseTrap.prefab" },
                { "LightBurstTrapPrefab", "Assets/_Game/Prefabs/Traps/LightBurstTrap.prefab" },
                { "SmokeBombPrefab", "Assets/_Game/Prefabs/Boosters/SmokeBomb.prefab" },
                { "SpeedBootsPrefab", "Assets/_Game/Prefabs/Boosters/SpeedBoots.prefab" },
                { "ShadowCloakPrefab", "Assets/_Game/Prefabs/Boosters/ShadowCloak.prefab" },
                { "GhostVisionPrefab", "Assets/_Game/Prefabs/Boosters/GhostVision.prefab" },
            };

            foreach (var kvp in prefabMap)
            {
                var prop = so.FindProperty(kvp.Key);
                if (prop == null) continue;
                if (prop.objectReferenceValue != null)
                {
                    Log($"OK: GameConfig.{kvp.Key}");
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(kvp.Value);
                if (prefab != null)
                {
                    prop.objectReferenceValue = prefab;
                    _fixedCount++;
                    Log($"FIXED: GameConfig.{kvp.Key} = {kvp.Value}");
                }
                else
                {
                    Log($"WARN: Prefab not found at {kvp.Value}");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        // ==================== LEVEL MANAGER LEVELS ====================

        private void AssignLevelManagerLevels()
        {
            var comp = FindInScene<Level.LevelManager>();
            if (comp == null) { Log("WARN: LevelManager not found in scene"); return; }

            var so = new SerializedObject(comp);
            AssignLevelManagerLevelsInternal(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignLevelManagerLevelsInternal(SerializedObject so)
        {
            var levelsProp = so.FindProperty("_levels");
            if (levelsProp == null) return;

            // Load all LevelData from folder, sorted by name
            string levelsFolder = "Assets/_Game/ScriptableObjects/Levels";
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { levelsFolder });
            var levels = new List<Level.LevelData>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var levelData = AssetDatabase.LoadAssetAtPath<Level.LevelData>(path);
                if (levelData != null)
                    levels.Add(levelData);
            }

            levels.Sort((a, b) => a.LevelIndex.CompareTo(b.LevelIndex));

            if (levels.Count == 0)
            {
                Log("WARN: No LevelData assets found in " + levelsFolder);
                return;
            }

            if (levelsProp.arraySize == levels.Count && !HasNullInArray(levelsProp))
            {
                Log($"OK: LevelManager._levels ({levels.Count} levels)");
                return;
            }

            levelsProp.arraySize = levels.Count;
            for (int i = 0; i < levels.Count; i++)
                levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];

            _fixedCount++;
            Log($"FIXED: LevelManager._levels = {levels.Count} levels");
        }

        // ==================== HELPERS ====================

        private T FindInScene<T>() where T : Component
        {
            return Object.FindObjectOfType<T>(true);
        }

        private T LoadAsset<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private void AssignIfNull(SerializedObject so, string propName, Object value, string label)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Log($"WARN: Property '{propName}' not found"); return; }
            if (prop.objectReferenceValue != null) { Log($"OK: {label}"); return; }
            if (value == null) { Log($"WARN: {label} - value not found in scene"); return; }

            prop.objectReferenceValue = value;
            _fixedCount++;
            Log($"FIXED: {label}");
        }

        private void AssignButtonByName(SerializedObject so, string propName, Transform parent, string childName)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return;

            var child = FindChildDeep(parent, childName);
            if (child != null)
            {
                var btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    prop.objectReferenceValue = btn;
                    _fixedCount++;
                    Log($"FIXED: {propName} = {childName}");
                }
            }
        }

        private void AssignTMPByName(SerializedObject so, string propName, Transform parent, string childName)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return;

            var child = FindChildDeep(parent, childName);
            if (child != null)
            {
                var tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    prop.objectReferenceValue = tmp;
                    _fixedCount++;
                    Log($"FIXED: {propName} = {childName}");
                }
            }
        }

        private void AssignSliderByName(SerializedObject so, string propName, Transform parent, string childName)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return;

            var child = FindChildDeep(parent, childName);
            if (child != null)
            {
                var slider = child.GetComponent<Slider>();
                if (slider != null)
                {
                    prop.objectReferenceValue = slider;
                    _fixedCount++;
                    Log($"FIXED: {propName} = {childName}");
                }
            }
        }

        private void AssignGOByName(SerializedObject so, string propName, Transform parent, string childName)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return;

            var child = FindChildDeep(parent, childName);
            if (child != null)
            {
                prop.objectReferenceValue = child.gameObject;
                _fixedCount++;
                Log($"FIXED: {propName} = {childName}");
            }
        }

        private Transform FindChildDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name) return child;

                var found = FindChildDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private bool HasNullInArray(SerializedProperty arrayProp)
        {
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    return true;
            }
            return false;
        }
    }
}

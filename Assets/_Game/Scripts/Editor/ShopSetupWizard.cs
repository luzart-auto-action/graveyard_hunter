using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace GraveyardHunter.Editor
{
    public class ShopSetupWizard : EditorWindow
    {
        private Vector2 _scrollPos;
        private List<string> _log = new List<string>();
        private int _fixedCount;

        private static readonly string GameConfigPath = "Assets/_Game/ScriptableObjects/Configs/GameConfig.asset";
        private static readonly string PlayerPrefabPath = "Assets/_Game/Prefabs/Characters/Player.prefab";
        private static readonly string SkinItemPrefabPath = "Assets/_Game/Prefabs/UI/SkinItem.prefab";
        private static readonly string CharacterPrefabsFolder = "Assets/characters_5_02/Prefabs";

        // Default skin setup: name, price, model prefab name
        private static readonly (string name, int price, string model)[] DefaultSkins =
        {
            ("Default", 0, "f_5"),
            ("Shadow", 500, "m_1"),
            ("Phantom", 1000, "f_3"),
            ("Reaper", 2000, "m_7"),
        };

        [MenuItem("GraveyardHunter/Shop Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<ShopSetupWizard>("Shop Setup Wizard");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Shop Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "One-click setup for the entire Shop system:\n" +
                "1. GameConfig: 4 default skins with character models\n" +
                "2. SkinItem UI prefab for shop list\n" +
                "3. ShopPanel references in scene\n" +
                "4. SkinApplier on Player prefab\n\n" +
                "After setup, you can change the ModelPrefab in GameConfig\n" +
                "to use different character models.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Setup Shop (All)", GUILayout.Height(40)))
            {
                _log.Clear();
                _fixedCount = 0;
                SetupAll();
                Log($"--- Done! {_fixedCount} items configured. ---");
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Setup GameConfig Skins Only", GUILayout.Height(30)))
            {
                _log.Clear();
                _fixedCount = 0;
                SetupGameConfigSkins();
                Log($"--- Done! {_fixedCount} items configured. ---");
            }

            if (GUILayout.Button("Setup SkinItem Prefab Only", GUILayout.Height(30)))
            {
                _log.Clear();
                _fixedCount = 0;
                SetupSkinItemPrefab();
                Log($"--- Done! {_fixedCount} items configured. ---");
            }

            if (GUILayout.Button("Setup Player SkinApplier Only", GUILayout.Height(30)))
            {
                _log.Clear();
                _fixedCount = 0;
                SetupPlayerSkinApplier();
                Log($"--- Done! {_fixedCount} items configured. ---");
            }

            EditorGUILayout.Space(10);

            if (_log.Count > 0)
            {
                EditorGUILayout.LabelField($"Log ({_fixedCount} configured):", EditorStyles.boldLabel);
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

        // ===================== MAIN SETUP =====================

        private void SetupAll()
        {
            SetupGameConfigSkins();
            SetupSkinItemPrefab();
            SetupShopPanelRefs();
            SetupPlayerSkinApplier();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // ===================== 1. GAMECONFIG SKINS =====================

        private void SetupGameConfigSkins()
        {
            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>(GameConfigPath);
            if (config == null)
            {
                Log("WARN: GameConfig.asset not found at " + GameConfigPath);
                return;
            }

            var so = new SerializedObject(config);
            var skinsProp = so.FindProperty("AvailableSkins");

            if (skinsProp == null)
            {
                Log("WARN: AvailableSkins property not found on GameConfig");
                return;
            }

            if (skinsProp.arraySize > 0)
            {
                // Check if ModelPrefab is assigned for existing entries
                bool anyMissing = false;
                for (int i = 0; i < skinsProp.arraySize; i++)
                {
                    var element = skinsProp.GetArrayElementAtIndex(i);
                    var modelProp = element.FindPropertyRelative("ModelPrefab");
                    if (modelProp != null && modelProp.objectReferenceValue == null)
                        anyMissing = true;
                }

                if (!anyMissing)
                {
                    Log($"OK: GameConfig.AvailableSkins already has {skinsProp.arraySize} skins with models");
                    return;
                }

                // Try to fill missing ModelPrefab entries
                for (int i = 0; i < skinsProp.arraySize && i < DefaultSkins.Length; i++)
                {
                    var element = skinsProp.GetArrayElementAtIndex(i);
                    var modelProp = element.FindPropertyRelative("ModelPrefab");
                    if (modelProp != null && modelProp.objectReferenceValue == null)
                    {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                            $"{CharacterPrefabsFolder}/{DefaultSkins[i].model}.prefab");
                        if (prefab != null)
                        {
                            modelProp.objectReferenceValue = prefab;
                            _fixedCount++;
                            Log($"FIXED: Skin[{i}].ModelPrefab = {DefaultSkins[i].model}");
                        }
                    }
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                return;
            }

            // Create default skin entries
            skinsProp.arraySize = DefaultSkins.Length;

            for (int i = 0; i < DefaultSkins.Length; i++)
            {
                var skin = DefaultSkins[i];
                var element = skinsProp.GetArrayElementAtIndex(i);

                element.FindPropertyRelative("SkinName").stringValue = skin.name;
                element.FindPropertyRelative("Price").intValue = skin.price;

                // Colors
                var primaryColor = element.FindPropertyRelative("PrimaryColor");
                var secondaryColor = element.FindPropertyRelative("SecondaryColor");
                primaryColor.colorValue = GetDefaultColor(i, true);
                secondaryColor.colorValue = GetDefaultColor(i, false);

                // Model prefab
                var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"{CharacterPrefabsFolder}/{skin.model}.prefab");
                if (modelPrefab != null)
                {
                    element.FindPropertyRelative("ModelPrefab").objectReferenceValue = modelPrefab;
                    Log($"FIXED: Skin[{i}] = \"{skin.name}\" (Price: {skin.price}, Model: {skin.model})");
                }
                else
                {
                    Log($"WARN: Model prefab not found: {CharacterPrefabsFolder}/{skin.model}.prefab");
                }

                _fixedCount++;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Log($"Created {DefaultSkins.Length} default skin entries in GameConfig");
        }

        private Color GetDefaultColor(int index, bool primary)
        {
            switch (index)
            {
                case 0: return primary ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
                case 1: return primary ? new Color(0.2f, 0.2f, 0.3f) : new Color(0.1f, 0.1f, 0.2f);
                case 2: return primary ? new Color(0.6f, 0.8f, 1.0f) : new Color(0.3f, 0.5f, 0.8f);
                case 3: return primary ? new Color(0.3f, 0.0f, 0.0f) : new Color(0.1f, 0.0f, 0.0f);
                default: return Color.white;
            }
        }

        // ===================== 2. SKINITEM PREFAB =====================

        private void SetupSkinItemPrefab()
        {
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SkinItemPrefabPath);
            if (existing != null)
            {
                Log("OK: SkinItem prefab already exists at " + SkinItemPrefabPath);
                return;
            }

            // Ensure directory exists
            var dir = System.IO.Path.GetDirectoryName(SkinItemPrefabPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var parts = dir.Replace("\\", "/").Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            // Create SkinItem root
            var root = new GameObject("SkinItem");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(900f, 150f);

            // HorizontalLayoutGroup
            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Background
            var bgImage = root.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.1f, 0.25f, 0.9f);

            // Icon
            var iconGO = new GameObject("Icon");
            var iconRect = iconGO.AddComponent<RectTransform>();
            iconGO.transform.SetParent(root.transform, false);
            iconRect.sizeDelta = new Vector2(120f, 120f);
            var iconImage = iconGO.AddComponent<Image>();
            iconImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // Info container (name + price)
            var infoGO = new GameObject("Info");
            var infoRect = infoGO.AddComponent<RectTransform>();
            infoGO.transform.SetParent(root.transform, false);
            infoRect.sizeDelta = new Vector2(400f, 120f);

            var infoVLG = infoGO.AddComponent<VerticalLayoutGroup>();
            infoVLG.spacing = 5f;
            infoVLG.childAlignment = TextAnchor.MiddleLeft;
            infoVLG.childControlWidth = true;
            infoVLG.childControlHeight = true;
            infoVLG.childForceExpandWidth = true;
            infoVLG.childForceExpandHeight = false;

            // NameText
            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(infoGO.transform, false);
            var nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(400f, 50f);
            var nameText = nameGO.AddComponent<TextMeshProUGUI>();
            nameText.text = "Skin Name";
            nameText.fontSize = 32;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;

            // PriceText
            var priceGO = new GameObject("PriceText");
            priceGO.transform.SetParent(infoGO.transform, false);
            var priceRect = priceGO.AddComponent<RectTransform>();
            priceRect.sizeDelta = new Vector2(400f, 40f);
            var priceText = priceGO.AddComponent<TextMeshProUGUI>();
            priceText.text = "500";
            priceText.fontSize = 24;
            priceText.color = new Color(1f, 0.85f, 0.3f, 1f); // Gold color
            priceText.alignment = TextAlignmentOptions.Left;

            // ActionButton
            var actionBtnGO = new GameObject("ActionButton");
            var actionBtnRect = actionBtnGO.AddComponent<RectTransform>();
            actionBtnGO.transform.SetParent(root.transform, false);
            actionBtnRect.sizeDelta = new Vector2(200f, 80f);

            var actionBtnImage = actionBtnGO.AddComponent<Image>();
            actionBtnImage.color = new Color(0.2f, 0.6f, 0.3f, 1f); // Green button

            var actionBtn = actionBtnGO.AddComponent<Button>();
            var colors = actionBtn.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.4f, 1f);
            colors.pressedColor = new Color(0.15f, 0.5f, 0.25f, 1f);
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            actionBtn.colors = colors;

            var actionTextGO = new GameObject("Text");
            actionTextGO.transform.SetParent(actionBtnGO.transform, false);
            var actionTextRect = actionTextGO.AddComponent<RectTransform>();
            actionTextRect.anchorMin = Vector2.zero;
            actionTextRect.anchorMax = Vector2.one;
            actionTextRect.sizeDelta = Vector2.zero;
            actionTextRect.offsetMin = Vector2.zero;
            actionTextRect.offsetMax = Vector2.zero;
            var actionText = actionTextGO.AddComponent<TextMeshProUGUI>();
            actionText.text = "Buy";
            actionText.fontSize = 26;
            actionText.fontStyle = FontStyles.Bold;
            actionText.color = Color.white;
            actionText.alignment = TextAlignmentOptions.Center;

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, SkinItemPrefabPath);
            Object.DestroyImmediate(root);

            if (prefab != null)
            {
                _fixedCount++;
                Log($"FIXED: Created SkinItem prefab at {SkinItemPrefabPath}");
            }
            else
            {
                Log("WARN: Failed to create SkinItem prefab");
            }
        }

        // ===================== 3. SHOPPANEL REFS =====================

        private void SetupShopPanelRefs()
        {
            var shopPanel = Object.FindObjectOfType<UI.ShopPanel>(true);
            if (shopPanel == null)
            {
                Log("WARN: ShopPanel not found in scene");
                return;
            }

            var so = new SerializedObject(shopPanel);

            // _skinItemPrefab
            var prefabProp = so.FindProperty("_skinItemPrefab");
            if (prefabProp != null && prefabProp.objectReferenceValue == null)
            {
                var skinItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SkinItemPrefabPath);
                if (skinItemPrefab != null)
                {
                    prefabProp.objectReferenceValue = skinItemPrefab;
                    _fixedCount++;
                    Log("FIXED: ShopPanel._skinItemPrefab");
                }
                else
                {
                    Log("WARN: SkinItem prefab not found, run 'Setup SkinItem Prefab Only' first");
                }
            }
            else
            {
                Log("OK: ShopPanel._skinItemPrefab");
            }

            // _skinItemParent (Content)
            var parentProp = so.FindProperty("_skinItemParent");
            if (parentProp != null && parentProp.objectReferenceValue == null)
            {
                var content = FindChildDeep(shopPanel.transform, "Content");
                if (content != null)
                {
                    parentProp.objectReferenceValue = content;
                    _fixedCount++;
                    Log("FIXED: ShopPanel._skinItemParent = Content");
                }
            }
            else
            {
                Log("OK: ShopPanel._skinItemParent");
            }

            // _totalScoreText
            AssignTMPByName(so, "_totalScoreText", shopPanel.transform, "TotalScoreText");

            // _closeButton
            AssignButtonByName(so, "_closeButton", shopPanel.transform, "CloseButton");

            // _panelName
            var nameProp = so.FindProperty("_panelName");
            if (nameProp != null && string.IsNullOrEmpty(nameProp.stringValue))
            {
                nameProp.stringValue = "ShopPanel";
                _fixedCount++;
                Log("FIXED: ShopPanel._panelName = ShopPanel");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ===================== 4. PLAYER SKINAPPLIER =====================

        private void SetupPlayerSkinApplier()
        {
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Log("WARN: Player.prefab not found at " + PlayerPrefabPath);
                return;
            }

            // Open prefab for editing
            var prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            var skinApplier = prefabRoot.GetComponent<Shop.SkinApplier>();
            if (skinApplier == null)
            {
                skinApplier = prefabRoot.AddComponent<Shop.SkinApplier>();
                _fixedCount++;
                Log("FIXED: Added SkinApplier to Player prefab");
            }
            else
            {
                Log("OK: SkinApplier already on Player prefab");
            }

            // Assign _visualRoot
            var so = new SerializedObject(skinApplier);
            var visualRootProp = so.FindProperty("_visualRoot");
            if (visualRootProp != null && visualRootProp.objectReferenceValue == null)
            {
                var visualRoot = prefabRoot.transform.Find("VisualRoot");
                if (visualRoot != null)
                {
                    visualRootProp.objectReferenceValue = visualRoot;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    _fixedCount++;
                    Log("FIXED: SkinApplier._visualRoot = VisualRoot");
                }
                else
                {
                    Log("WARN: VisualRoot not found in Player prefab");
                }
            }
            else
            {
                Log("OK: SkinApplier._visualRoot");
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        // ===================== HELPERS =====================

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
    }
}

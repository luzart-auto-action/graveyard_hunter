using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace GraveyardHunter.Editor
{
    /// <summary>
    /// Creates:
    /// 1. A LevelNode prefab at Assets/_Game/Prefabs/UI/LevelNode.prefab
    ///    → User customizes this prefab (change visuals, fonts, sprites)
    /// 2. A UIMainMenu scene hierarchy (landscape-friendly, vertical scroll)
    ///    → Nodes are spawned from the prefab at runtime
    /// </summary>
    public class UIMainMenuBuilder : EditorWindow
    {
        [MenuItem("GraveyardHunter/Create UIMainMenu (Level Map)")]
        public static void ShowWindow()
        {
            GetWindow<UIMainMenuBuilder>("UIMainMenu Builder");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("UIMainMenu Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Step 1: Creates a LevelNode prefab you can customize.\n" +
                "Step 2: Creates UIMainMenu scene layout (landscape, vertical scroll).\n" +
                "Nodes are spawned from the prefab at runtime (currentLevel + 10).",
                MessageType.Info);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("1. Create LevelNode Prefab", GUILayout.Height(35)))
            {
                CreateLevelNodePrefab();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("2. Build UIMainMenu in Scene", GUILayout.Height(35)))
            {
                BuildMainMenu();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Do Both (Prefab + Scene)", GUILayout.Height(40)))
            {
                CreateLevelNodePrefab();
                BuildMainMenu();
            }
        }

        // ═══════════════════════════════════════════════════
        //  LEVEL NODE PREFAB
        // ═══════════════════════════════════════════════════

        private static readonly string LevelNodePrefabPath = "Assets/_Game/Prefabs/UI/LevelNode.prefab";

        private void CreateLevelNodePrefab()
        {
            EnsureFolder("Assets/_Game/Prefabs/UI");

            // Delete old
            if (AssetDatabase.LoadAssetAtPath<GameObject>(LevelNodePrefabPath) != null)
                AssetDatabase.DeleteAsset(LevelNodePrefabPath);

            float size = 100f;

            // Root
            var root = new GameObject("LevelNode");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(size, size + 30f); // extra for stars

            // Ring
            var ring = CreateChild("Ring", root, new Vector2(size + 14f, size + 14f), Vector2.zero);
            var ringImg = ring.AddComponent<Image>();
            ringImg.color = new Color(1f, 1f, 1f, 0.9f);
            ringImg.raycastTarget = false;

            // BG (the main circle button area)
            var bg = CreateChild("BG", root, new Vector2(size, size), Vector2.zero);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.55f, 0.85f, 1f);

            // Button on root
            var btn = root.AddComponent<Button>();
            btn.targetGraphic = bgImg;

            // Level number text
            var numGO = CreateChild("Number", root, new Vector2(80f, 60f), Vector2.zero);
            var numTMP = numGO.AddComponent<TextMeshProUGUI>();
            numTMP.text = "1";
            numTMP.fontSize = 38;
            numTMP.fontStyle = FontStyles.Bold;
            numTMP.alignment = TextAlignmentOptions.Center;
            numTMP.color = Color.white;
            numTMP.raycastTarget = false;

            // Lock icon
            var lockGO = CreateChild("LockIcon", root, new Vector2(60f, 40f), Vector2.zero);
            var lockTMP = lockGO.AddComponent<TextMeshProUGUI>();
            lockTMP.text = "\u0fd5"; // lock-ish unicode, user can swap to Image
            lockTMP.fontSize = 30;
            lockTMP.alignment = TextAlignmentOptions.Center;
            lockTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            lockTMP.raycastTarget = false;
            lockGO.SetActive(false);

            // Stars row (3 stars below the node)
            Image[] stars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                float sx = (i - 1) * 24f;
                var star = CreateChild($"Star_{i}", root,
                    new Vector2(20f, 20f),
                    new Vector2(sx, -(size * 0.5f + 8f)));
                var starImg = star.AddComponent<Image>();
                starImg.color = new Color(0.35f, 0.35f, 0.35f, 0.5f);
                starImg.raycastTarget = false;
                stars[i] = starImg;
            }

            // Add LevelNodeUI component & wire refs
            var nodeComp = root.AddComponent<UI.LevelNodeUI>();
            var so = new SerializedObject(nodeComp);
            Assign(so, "_button", btn);
            Assign(so, "_bgImage", bgImg);
            Assign(so, "_ringImage", ringImg);
            Assign(so, "_levelNumberText", numTMP);
            Assign(so, "_lockIcon", lockGO);
            var starsProp = so.FindProperty("_starImages");
            if (starsProp != null)
            {
                starsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                    starsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(root, LevelNodePrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.Refresh();

            Debug.Log($"[UIMainMenuBuilder] Created LevelNode prefab at {LevelNodePrefabPath}. Customize it in the Prefab editor!");
        }

        // ═══════════════════════════════════════════════════
        //  SCENE LAYOUT
        // ═══════════════════════════════════════════════════

        private void BuildMainMenu()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UIMainMenuBuilder] No Canvas found!");
                return;
            }

            // Delete old
            var old = Object.FindObjectOfType<UI.UIMainMenu>(true);
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);

            var canvasRT = canvas.GetComponent<RectTransform>();

            // ── Root panel (fullscreen) ──
            var root = CreateFullscreen("UIMainMenu", canvasRT);
            root.GetComponent<Image>().color = new Color(0.12f, 0.1f, 0.2f, 0.97f);
            var cg = root.AddComponent<CanvasGroup>();
            var menu = root.AddComponent<UI.UIMainMenu>();
            Undo.RegisterCreatedObjectUndo(root, "Create UIMainMenu");

            var menuSO = new SerializedObject(menu);
            SetString(menuSO, "_panelName", "UIMainMenu");

            // ── Left side: Scroll area (takes ~60% width) ──
            var scrollGO = CreateAnchored("ScrollView", root.transform,
                new Vector2(0f, 0f), new Vector2(0.65f, 1f),
                new Vector2(10f, 10f), new Vector2(-10f, -10f));
            var scrollComp = scrollGO.AddComponent<ScrollRect>();
            scrollComp.horizontal = false;
            scrollComp.vertical = true;
            scrollComp.movementType = ScrollRect.MovementType.Elastic;
            scrollComp.scrollSensitivity = 30f;

            // Viewport
            var viewport = CreateAnchored("Viewport", scrollGO.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            scrollComp.viewport = viewport.GetComponent<RectTransform>();

            // Content (starts empty — nodes spawned at runtime)
            var content = new GameObject("Content");
            var contentRT = content.AddComponent<RectTransform>();
            content.transform.SetParent(viewport.transform, false);
            contentRT.anchorMin = new Vector2(0.5f, 1f);
            contentRT.anchorMax = new Vector2(0.5f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.sizeDelta = new Vector2(300f, 800f); // runtime will resize
            scrollComp.content = contentRT;

            // Path line (center of content)
            var pathLine = new GameObject("PathLine");
            var pathRT = pathLine.AddComponent<RectTransform>();
            pathLine.transform.SetParent(content.transform, false);
            pathRT.anchorMin = new Vector2(0.5f, 0f);
            pathRT.anchorMax = new Vector2(0.5f, 1f);
            pathRT.sizeDelta = new Vector2(6f, 0f);
            pathRT.offsetMin = new Vector2(-3f, 60f);
            pathRT.offsetMax = new Vector2(3f, -60f);
            var pathImg = pathLine.AddComponent<Image>();
            pathImg.color = new Color(0.35f, 0.55f, 0.8f, 0.4f);
            pathImg.raycastTarget = false;

            // ── Right side: Info + Buttons (takes ~35% width) ──
            var rightPanel = CreateAnchored("RightPanel", root.transform,
                new Vector2(0.65f, 0f), new Vector2(1f, 1f),
                new Vector2(10f, 10f), new Vector2(-10f, -10f));
            rightPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);

            // Score text
            var scoreTxt = CreateTMP("ScoreText", rightPanel.transform,
                "Score: 0", 24, TextAlignmentOptions.Center, Color.white);
            var scoreRT = scoreTxt.GetComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0f, 0.85f);
            scoreRT.anchorMax = new Vector2(1f, 0.95f);
            scoreRT.offsetMin = Vector2.zero;
            scoreRT.offsetMax = Vector2.zero;

            // Play button (big, center-right)
            var playGO = CreateAnchored("PlayButton", rightPanel.transform,
                new Vector2(0.15f, 0.4f), new Vector2(0.85f, 0.6f),
                Vector2.zero, Vector2.zero);
            var playImg = playGO.AddComponent<Image>();
            playImg.color = new Color(0.2f, 0.78f, 0.3f, 1f);
            var playBtn = playGO.AddComponent<Button>();
            playBtn.targetGraphic = playImg;
            var pColors = playBtn.colors;
            pColors.normalColor = playImg.color;
            pColors.highlightedColor = new Color(0.25f, 0.88f, 0.35f, 1f);
            pColors.pressedColor = new Color(0.15f, 0.6f, 0.2f, 1f);
            playBtn.colors = pColors;
            var playTxt = CreateTMP("Text", playGO.transform, "Play", 36,
                TextAlignmentOptions.Center, Color.white);
            playTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            Stretch(playTxt);

            // Shop button
            var shopGO = CreateAnchored("ShopButton", rightPanel.transform,
                new Vector2(0.1f, 0.18f), new Vector2(0.48f, 0.32f),
                Vector2.zero, Vector2.zero);
            var shopImg = shopGO.AddComponent<Image>();
            shopImg.color = new Color(0.25f, 0.4f, 0.7f, 1f);
            var shopBtn = shopGO.AddComponent<Button>();
            shopBtn.targetGraphic = shopImg;
            var shopTxt = CreateTMP("Text", shopGO.transform, "Shop", 22,
                TextAlignmentOptions.Center, Color.white);
            Stretch(shopTxt);

            // Settings button
            var settGO = CreateAnchored("SettingsButton", rightPanel.transform,
                new Vector2(0.52f, 0.18f), new Vector2(0.9f, 0.32f),
                Vector2.zero, Vector2.zero);
            var settImg = settGO.AddComponent<Image>();
            settImg.color = new Color(0.25f, 0.4f, 0.7f, 1f);
            var settBtn = settGO.AddComponent<Button>();
            settBtn.targetGraphic = settImg;
            var settTxt = CreateTMP("Text", settGO.transform, "Settings", 22,
                TextAlignmentOptions.Center, Color.white);
            Stretch(settTxt);

            // ── Wire references ──
            var nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelNodePrefabPath);

            Assign(menuSO, "_scrollRect", scrollComp);
            Assign(menuSO, "_content", contentRT);
            Assign(menuSO, "_levelNodePrefab", nodePrefab);
            Assign(menuSO, "_pathLine", pathImg);
            Assign(menuSO, "_playButton", playBtn);
            Assign(menuSO, "_settingsButton", settBtn);
            Assign(menuSO, "_shopButton", shopBtn);
            Assign(menuSO, "_totalScoreText", scoreTxt.GetComponent<TextMeshProUGUI>());
            menuSO.ApplyModifiedPropertiesWithoutUndo();

            // Register in UIManager
            RegisterWithUIManager(menu);

            root.SetActive(true);
            EditorUtility.SetDirty(root);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[UIMainMenuBuilder] Built UIMainMenu scene layout. Nodes will spawn at runtime from prefab.");
        }

        // ═══════════════════════════════════════════════════
        //  UI MANAGER
        // ═══════════════════════════════════════════════════

        private void RegisterWithUIManager(UI.UIMainMenu menuComp)
        {
            var uiMgr = Object.FindObjectOfType<UI.UIManager>(true);
            if (uiMgr == null) return;

            var so = new SerializedObject(uiMgr);
            var panels = so.FindProperty("_panels");
            if (panels == null) return;

            // Remove null / old UIMainMenu entries
            for (int i = panels.arraySize - 1; i >= 0; i--)
            {
                var e = panels.GetArrayElementAtIndex(i);
                if (e.objectReferenceValue == null || e.objectReferenceValue is UI.UIMainMenu)
                {
                    panels.DeleteArrayElementAtIndex(i);
                    // Unity quirk: first delete nullifies, second actually removes
                    if (i < panels.arraySize && panels.GetArrayElementAtIndex(i).objectReferenceValue == null)
                        panels.DeleteArrayElementAtIndex(i);
                }
            }

            panels.InsertArrayElementAtIndex(panels.arraySize);
            panels.GetArrayElementAtIndex(panels.arraySize - 1).objectReferenceValue = menuComp;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(uiMgr);
        }

        // ═══════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════

        private GameObject CreateChild(string name, GameObject parent, Vector2 size, Vector2 pos)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent.transform, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return go;
        }

        private GameObject CreateFullscreen(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>();
            return go;
        }

        private GameObject CreateAnchored(string name, Transform parent,
            Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = offMin;
            rt.offsetMax = offMax;
            return go;
        }

        private GameObject CreateTMP(string name, Transform parent, string text,
            int fontSize, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rt.sizeDelta = new Vector2(200f, 50f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return go;
        }

        private void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void Assign(SerializedObject so, string prop, Object val)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.objectReferenceValue = val;
        }

        private void SetString(SerializedObject so, string prop, string val)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.stringValue = val;
        }

        private void EnsureFolder(string path)
        {
            path = path.Replace("\\", "/");
            var parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}

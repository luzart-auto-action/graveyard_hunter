using UnityEngine;
using UnityEditor;
using TMPro;

namespace GraveyardHunter.Editor
{
    public class SetupAll : EditorWindow
    {
        private Vector2 _scrollPos;
        private static readonly System.Collections.Generic.List<string> _log = new();

        [MenuItem("GraveyardHunter/=== SETUP ALL ===", priority = 0)]
        public static void ShowWindow()
        {
            GetWindow<SetupAll>("Setup All");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Graveyard Hunter - Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button("SETUP EVERYTHING", GUILayout.Height(45)))
            {
                _log.Clear();
                Log("── Materials ──");
                MaterialFactory.CreateAllMaterials();
                Log("── Prefabs ──");
                PrefabCreator.CreateAllPrefabsStatic();
                Log("── 20 Levels ──");
                LevelBatchCreator.CreateAllLevels();
                Log("── Bug Fixes ──");
                BugFixer.FixAllBugsStatic();
                Log("══ Done! ══");
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Create Materials"))           { _log.Clear(); MaterialFactory.CreateAllMaterials(); Log("Done."); Repaint(); }
            if (GUILayout.Button("Create Prefabs"))             { _log.Clear(); PrefabCreator.CreateAllPrefabsStatic(); Log("Done."); Repaint(); }
            if (GUILayout.Button("Create 20 Levels"))           { _log.Clear(); LevelBatchCreator.CreateAllLevels(); Log("Done."); Repaint(); }
            if (GUILayout.Button("Fix All Bugs + Wiring"))      { _log.Clear(); BugFixer.FixAllBugsStatic(); Log("Done."); Repaint(); }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
            if (GUILayout.Button("Create PopupSettings (Toggle)", GUILayout.Height(35)))
            {
                _log.Clear();
                CreatePopupSettings();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);
            if (_log.Count > 0)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
                foreach (var e in _log) EditorGUILayout.LabelField(e, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndScrollView();
            }
        }

        // ═══════════════════════════════════════════
        //  CREATE POPUP SETTINGS
        // ═══════════════════════════════════════════

        private void CreatePopupSettings()
        {
            // Delete old
            var old = Object.FindObjectOfType<UI.PopupSettings>(true);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old.gameObject);
                Log("Deleted old PopupSettings.");
            }

            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) { Log("ERROR: No Canvas!"); return; }

            // === Root (fullscreen dark overlay) ===
            var root = new GameObject("PopupSettings");
            root.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(root, "Create PopupSettings");
            Stretch(root);
            root.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.75f);

            // === Center panel ===
            var panel = MakeAnchored("Panel", root.transform, 0.2f, 0.1f, 0.8f, 0.9f);
            panel.AddComponent<UnityEngine.UI.Image>().color = new Color(0.15f, 0.12f, 0.22f, 0.97f);

            // === Title: "SETTING" ===
            var titleGO = MakeAnchored("Title", panel.transform, 0.15f, 0.82f, 0.85f, 0.95f);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "SETTING";
            titleTMP.fontSize = 36;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;
            titleTMP.raycastTarget = false;

            // === Close (X) button — top right ===
            var closeBtnGO = MakeAnchored("CloseButton", panel.transform, 0.82f, 0.85f, 0.95f, 0.97f);
            var closeBG = closeBtnGO.AddComponent<UnityEngine.UI.Image>();
            closeBG.color = new Color(0.55f, 0.25f, 0.25f, 1f);
            var closeBtn = closeBtnGO.AddComponent<UnityEngine.UI.Button>();
            closeBtn.targetGraphic = closeBG;
            var closeTxt = MakeAnchored("Text", closeBtnGO.transform, 0, 0, 1, 1);
            var closeTMP = closeTxt.AddComponent<TextMeshProUGUI>();
            closeTMP.text = "X";
            closeTMP.fontSize = 28;
            closeTMP.fontStyle = FontStyles.Bold;
            closeTMP.alignment = TextAlignmentOptions.Center;
            closeTMP.color = Color.white;
            closeTMP.raycastTarget = false;

            // === BGM row: label + toggle button ===
            // Label
            var bgmLabel = MakeAnchored("BGMLabel", panel.transform, 0.08f, 0.6f, 0.55f, 0.75f);
            var bgmLabelTMP = bgmLabel.AddComponent<TextMeshProUGUI>();
            bgmLabelTMP.text = "BGM";
            bgmLabelTMP.fontSize = 28;
            bgmLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;
            bgmLabelTMP.color = Color.white;
            bgmLabelTMP.raycastTarget = false;

            // Toggle button
            var musicBtnGO = MakeAnchored("MusicToggleButton", panel.transform, 0.58f, 0.6f, 0.88f, 0.75f);
            var musicBG = musicBtnGO.AddComponent<UnityEngine.UI.Image>();
            musicBG.color = new Color(0.2f, 0.75f, 0.3f, 1f);
            var musicBtn = musicBtnGO.AddComponent<UnityEngine.UI.Button>();
            musicBtn.targetGraphic = musicBG;
            var musicTxtGO = MakeAnchored("Text", musicBtnGO.transform, 0, 0, 1, 1);
            var musicTMP = musicTxtGO.AddComponent<TextMeshProUGUI>();
            musicTMP.text = "ON";
            musicTMP.fontSize = 26;
            musicTMP.fontStyle = FontStyles.Bold;
            musicTMP.alignment = TextAlignmentOptions.Center;
            musicTMP.color = Color.white;
            musicTMP.raycastTarget = false;

            // === Sound Effect row: label + toggle button ===
            var sfxLabel = MakeAnchored("SFXLabel", panel.transform, 0.08f, 0.4f, 0.55f, 0.55f);
            var sfxLabelTMP = sfxLabel.AddComponent<TextMeshProUGUI>();
            sfxLabelTMP.text = "Sound Effect";
            sfxLabelTMP.fontSize = 26;
            sfxLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;
            sfxLabelTMP.color = Color.white;
            sfxLabelTMP.raycastTarget = false;

            var sfxBtnGO = MakeAnchored("SFXToggleButton", panel.transform, 0.58f, 0.4f, 0.88f, 0.55f);
            var sfxBG = sfxBtnGO.AddComponent<UnityEngine.UI.Image>();
            sfxBG.color = new Color(0.2f, 0.75f, 0.3f, 1f);
            var sfxBtn = sfxBtnGO.AddComponent<UnityEngine.UI.Button>();
            sfxBtn.targetGraphic = sfxBG;
            var sfxTxtGO = MakeAnchored("Text", sfxBtnGO.transform, 0, 0, 1, 1);
            var sfxTMP = sfxTxtGO.AddComponent<TextMeshProUGUI>();
            sfxTMP.text = "ON";
            sfxTMP.fontSize = 26;
            sfxTMP.fontStyle = FontStyles.Bold;
            sfxTMP.alignment = TextAlignmentOptions.Center;
            sfxTMP.color = Color.white;
            sfxTMP.raycastTarget = false;

            // === Wire PopupSettings component ===
            var comp = root.AddComponent<UI.PopupSettings>();
            var so = new SerializedObject(comp);
            SetStr(so, "_panelName", "PopupSettings");
            SetRef(so, "_musicToggleButton", musicBtn);
            SetRef(so, "_musicToggleBG", musicBG);
            SetRef(so, "_musicToggleText", musicTMP);
            SetRef(so, "_sfxToggleButton", sfxBtn);
            SetRef(so, "_sfxToggleBG", sfxBG);
            SetRef(so, "_sfxToggleText", sfxTMP);
            SetRef(so, "_closeButton", closeBtn);
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);

            // Register in UIManager
            RegisterPanel(comp);

            EditorUtility.SetDirty(root);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Log("Created PopupSettings: BGM toggle + SFX toggle + Close(X).");
        }

        // ═══════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════

        private static void RegisterPanel(UI.UIPanel panel)
        {
            var uiMgr = Object.FindObjectOfType<UI.UIManager>(true);
            if (uiMgr == null) return;

            var uiSO = new SerializedObject(uiMgr);
            var panels = uiSO.FindProperty("_panels");
            if (panels == null) return;

            for (int i = panels.arraySize - 1; i >= 0; i--)
            {
                var e = panels.GetArrayElementAtIndex(i);
                if (e.objectReferenceValue == null || e.objectReferenceValue.GetType() == panel.GetType())
                {
                    panels.DeleteArrayElementAtIndex(i);
                    if (i < panels.arraySize && panels.GetArrayElementAtIndex(i).objectReferenceValue == null)
                        panels.DeleteArrayElementAtIndex(i);
                }
            }

            panels.InsertArrayElementAtIndex(panels.arraySize);
            panels.GetArrayElementAtIndex(panels.arraySize - 1).objectReferenceValue = panel;
            uiSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(uiMgr);
        }

        private static GameObject MakeAnchored(string name, Transform parent,
            float aMinX, float aMinY, float aMaxX, float aMaxY)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rt.anchorMin = new Vector2(aMinX, aMinY);
            rt.anchorMax = new Vector2(aMaxX, aMaxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private static void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRef(SerializedObject so, string prop, Object val)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.objectReferenceValue = val;
        }

        private static void SetStr(SerializedObject so, string prop, string val)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.stringValue = val;
        }

        private static void Log(string msg)
        {
            _log.Add(msg);
            Debug.Log($"[SetupAll] {msg}");
        }
    }
}

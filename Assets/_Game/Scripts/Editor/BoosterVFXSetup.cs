using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraveyardHunter.Editor
{
    public class BoosterVFXSetup : EditorWindow
    {
        private Vector2 _scrollPos;
        private List<string> _log = new List<string>();
        private int _fixedCount;

        private static readonly string PlayerPrefabPath = "Assets/_Game/Prefabs/Characters/Player.prefab";

        [MenuItem("GraveyardHunter/Booster VFX Setup")]
        public static void ShowWindow()
        {
            GetWindow<BoosterVFXSetup>("Booster VFX Setup");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Booster VFX Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "One-click setup for booster visual effects on the Player:\n\n" +
                "- Adds BoosterVFX component to Player prefab\n" +
                "- Creates FX attach points (FX_Center, FX_Top, FX_Bottom)\n" +
                "- Auto-assigns all references\n\n" +
                "Effects created per booster:\n" +
                "  SmokeBomb: Smoke ring burst + lingering fog\n" +
                "  SpeedBoots: Orange speed trail + wind lines\n" +
                "  ShadowCloak: Dark aura + purple shadow wisps\n" +
                "  GhostVision: Cyan eye glow + scan pulse ring",
                MessageType.Info);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Setup Booster VFX on Player", GUILayout.Height(40)))
            {
                _log.Clear();
                _fixedCount = 0;
                SetupBoosterVFX();
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

        private void SetupBoosterVFX()
        {
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Log("WARN: Player.prefab not found at " + PlayerPrefabPath);
                return;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            // 1. Ensure FX attach points exist
            var fxCenter = EnsureFXPoint(prefabRoot.transform, "FX_Center", new Vector3(0f, 0.9f, 0f));
            var fxTop = EnsureFXPoint(prefabRoot.transform, "FX_Top", new Vector3(0f, 1.8f, 0f));
            var fxBottom = EnsureFXPoint(prefabRoot.transform, "FX_Bottom", new Vector3(0f, 0.05f, 0f));

            // 2. Add BoosterVFX component
            var boosterVFX = prefabRoot.GetComponent<FX.BoosterVFX>();
            if (boosterVFX == null)
            {
                boosterVFX = prefabRoot.AddComponent<FX.BoosterVFX>();
                _fixedCount++;
                Log("FIXED: Added BoosterVFX component to Player");
            }
            else
            {
                Log("OK: BoosterVFX already on Player");
            }

            // 3. Assign references
            var so = new SerializedObject(boosterVFX);

            AssignTransform(so, "_fxCenter", fxCenter, "BoosterVFX._fxCenter");
            AssignTransform(so, "_fxTop", fxTop, "BoosterVFX._fxTop");
            AssignTransform(so, "_fxBottom", fxBottom, "BoosterVFX._fxBottom");

            so.ApplyModifiedPropertiesWithoutUndo();

            // 4. Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Log("Player prefab saved with BoosterVFX setup.");
        }

        private Transform EnsureFXPoint(Transform parent, string name, Vector3 localPos)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                Log($"OK: {name} already exists");
                return existing;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            _fixedCount++;
            Log($"FIXED: Created {name} at {localPos}");
            return go.transform;
        }

        private void AssignTransform(SerializedObject so, string propName, Transform value, string label)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Log($"WARN: Property '{propName}' not found"); return; }
            if (prop.objectReferenceValue != null) { Log($"OK: {label}"); return; }
            if (value == null) { Log($"WARN: {label} - value is null"); return; }

            prop.objectReferenceValue = value;
            _fixedCount++;
            Log($"FIXED: {label}");
        }
    }
}

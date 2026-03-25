using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraveyardHunter.Editor
{
    public class MaterialFactory : EditorWindow
    {
        private Vector2 _scrollPos;
        private static readonly string MaterialsPath = "Assets/_Game/Art/Materials";

        [System.Serializable]
        private class MaterialDef
        {
            public string Name;
            public Color Color;
            public float Metallic;
            public float Smoothness;
            public Color Emission;
            public float EmissionIntensity;
            public bool Transparent;

            public MaterialDef(string name, Color color, float metallic, float smoothness,
                Color emission, float emissionIntensity, bool transparent = false)
            {
                Name = name;
                Color = color;
                Metallic = metallic;
                Smoothness = smoothness;
                Emission = emission;
                EmissionIntensity = emissionIntensity;
                Transparent = transparent;
            }
        }

        private static readonly List<MaterialDef> AllMaterials = new List<MaterialDef>
        {
            new MaterialDef("Floor_Dark", HexColor("1A1A2E"), 0f, 0.3f, Color.black, 0f),
            new MaterialDef("Wall_Stone", HexColor("2D2D44"), 0.1f, 0.2f, Color.black, 0f),
            new MaterialDef("Player_Body", HexColor("00D4AA"), 0.2f, 0.5f, HexColor("00D4AA"), 0.3f),
            new MaterialDef("Ghost_Body", HexColor("E8E8FF"), 0.0f, 0.6f, HexColor("E8E8FF"), 0.8f),
            new MaterialDef("Ghost_Eyes", HexColor("FFD700"), 0.0f, 0.8f, HexColor("FFD700"), 2.0f),
            new MaterialDef("Ghost_Chase_Eyes", HexColor("FF0000"), 0.0f, 0.8f, HexColor("FF0000"), 2.5f),
            new MaterialDef("Treasure_Gold", HexColor("FFD700"), 0.6f, 0.8f, HexColor("FFD700"), 0.8f),
            new MaterialDef("Treasure_Silver", HexColor("C0C0C0"), 0.7f, 0.8f, HexColor("C0C0C0"), 0.3f),
            new MaterialDef("Treasure_Coin", HexColor("CD7F32"), 0.5f, 0.7f, HexColor("CD7F32"), 0.6f),
            new MaterialDef("Treasure_Artifact", HexColor("9B30FF"), 0.3f, 0.9f, HexColor("9B30FF"), 1.2f),
            new MaterialDef("Obstacle_Stone", HexColor("3A3A4A"), 0.1f, 0.2f, Color.black, 0f),
            new MaterialDef("ExitGate_Closed", HexColor("4A0000"), 0.1f, 0.3f, Color.black, 0f),
            new MaterialDef("ExitGate_Open", HexColor("00FF44"), 0.1f, 0.5f, HexColor("00FF44"), 1.5f),
            new MaterialDef("Trap_Spike", HexColor("333344"), 0.8f, 0.6f, Color.black, 0f),
            new MaterialDef("Trap_Noise", HexColor("FF8800"), 0.1f, 0.4f, HexColor("FF8800"), 0.3f),
            new MaterialDef("Trap_LightBurst", HexColor("FFFF00"), 0.1f, 0.5f, HexColor("FFFF00"), 2.0f),
            new MaterialDef("Booster_Smoke", HexColor("888888", 0.5f), 0.0f, 0.3f, Color.black, 0f, true),
            new MaterialDef("Booster_Speed", HexColor("00FFFF"), 0.1f, 0.5f, HexColor("00FFFF"), 1.0f),
            new MaterialDef("Booster_Shadow", HexColor("8800FF"), 0.1f, 0.5f, HexColor("8800FF"), 1.0f),
            new MaterialDef("Booster_Vision", HexColor("00FF00"), 0.1f, 0.5f, HexColor("00FF00"), 1.0f),
            new MaterialDef("Light_Warm", HexColor("FFE4B5"), 0.0f, 0.3f, HexColor("FFE4B5"), 0.2f),
            new MaterialDef("Fog_Dark", HexColor("0A0A1E"), 0.0f, 0.2f, Color.black, 0f),
        };

        [MenuItem("GraveyardHunter/Material Factory")]
        public static void ShowWindow()
        {
            GetWindow<MaterialFactory>("Material Factory");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Graveyard Hunter - Material Factory", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Create All Materials", GUILayout.Height(35)))
            {
                CreateAllMaterials();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Material List:", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var def in AllMaterials)
            {
                string path = $"{MaterialsPath}/{def.Name}.mat";
                bool exists = AssetDatabase.LoadAssetAtPath<Material>(path) != null;
                string status = exists ? "[EXISTS]" : "[NOT CREATED]";

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ColorField(GUIContent.none, def.Color, false, false, false, GUILayout.Width(30));
                EditorGUILayout.LabelField($"{def.Name} {status}");
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        public static void CreateAllMaterials()
        {
            EnsureFolderExists(MaterialsPath);

            int created = 0;
            foreach (var def in AllMaterials)
            {
                if (CreateMaterial(def.Name, def.Color, def.Metallic, def.Smoothness,
                    def.Emission, def.EmissionIntensity, def.Transparent))
                {
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MaterialFactory] Created {created} new materials. Total defined: {AllMaterials.Count}");
        }

        public static bool CreateMaterial(string name, Color color, float metallic, float smoothness,
            Color emission, float emissionIntensity, bool transparent)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                return false;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.name = name;

            // Base color
            mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);

            // Metallic / Smoothness
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);

            // Emission
            if (emissionIntensity > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission * emissionIntensity);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            // Transparency
            if (transparent)
            {
                mat.SetFloat("_Surface", 1f); // URP transparent
                mat.SetFloat("_Blend", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            AssetDatabase.CreateAsset(mat, path);
            return true;
        }

        /// <summary>
        /// Gets an existing material or creates it if it doesn't exist.
        /// Used by PrefabCreator.
        /// </summary>
        public static Material GetOrCreateMaterial(string name)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
                return mat;

            // Find the definition and create it
            foreach (var def in AllMaterials)
            {
                if (def.Name == name)
                {
                    CreateMaterial(def.Name, def.Color, def.Metallic, def.Smoothness,
                        def.Emission, def.EmissionIntensity, def.Transparent);
                    AssetDatabase.SaveAssets();
                    return AssetDatabase.LoadAssetAtPath<Material>(path);
                }
            }

            Debug.LogWarning($"[MaterialFactory] Material definition not found: {name}");
            return null;
        }

        private static Color HexColor(string hex, float alpha = 1f)
        {
            // Parse hex manually to avoid ColorUtility in static initializer
            hex = hex.TrimStart('#');
            float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            return new Color(r, g, b, alpha);
        }

        private static void EnsureFolderExists(string path)
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

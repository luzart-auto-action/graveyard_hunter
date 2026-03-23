using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraveyardHunter.Core;
using GraveyardHunter.Level;

namespace GraveyardHunter.Editor
{
    public class LevelBatchCreator : EditorWindow
    {
        private static readonly string LevelsPath = "Assets/_Game/ScriptableObjects/Levels";
        private Vector2 _scrollPos;

        private struct LevelDef
        {
            public int Index;
            public string Name;
            public int Width, Height;
            public int GhostCount;
            public int RequiredTreasures;
            public int TotalTreasures;
            public int SpikeTraps, NoiseTraps, LightBurstTraps;
            public int SmokeBombs, SpeedBoots, ShadowCloaks, GhostVisions;
            public int Seed;

            public int TotalTraps => SpikeTraps + NoiseTraps + LightBurstTraps;
            public int TotalBoosters => SmokeBombs + SpeedBoots + ShadowCloaks + GhostVisions;
        }

        private static readonly LevelDef[] Levels = new LevelDef[]
        {
            new LevelDef { Index=1,  Name="Tutorial",        Width=12, Height=12, GhostCount=1, RequiredTreasures=2, TotalTreasures=3, SpikeTraps=0, NoiseTraps=0, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, Seed=101 },
            new LevelDef { Index=2,  Name="Watch Your Step",  Width=12, Height=12, GhostCount=1, RequiredTreasures=2, TotalTreasures=3, SpikeTraps=1, NoiseTraps=0, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, Seed=102 },
            new LevelDef { Index=3,  Name="Quiet Steps",      Width=12, Height=12, GhostCount=1, RequiredTreasures=3, TotalTreasures=4, SpikeTraps=0, NoiseTraps=1, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, Seed=103 },
            new LevelDef { Index=4,  Name="Double Trouble",    Width=12, Height=12, GhostCount=2, RequiredTreasures=3, TotalTreasures=4, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, Seed=104 },
            new LevelDef { Index=5,  Name="Smoke Screen",      Width=12, Height=12, GhostCount=2, RequiredTreasures=3, TotalTreasures=5, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=0, SmokeBombs=1, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, Seed=105 },
            new LevelDef { Index=6,  Name="Bigger Grounds",    Width=16, Height=16, GhostCount=3, RequiredTreasures=4, TotalTreasures=5, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, Seed=106 },
            new LevelDef { Index=7,  Name="All Traps",         Width=16, Height=16, GhostCount=3, RequiredTreasures=4, TotalTreasures=6, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, Seed=107 },
            new LevelDef { Index=8,  Name="Treasure Hunt",     Width=16, Height=16, GhostCount=3, RequiredTreasures=5, TotalTreasures=7, SpikeTraps=2, NoiseTraps=1, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, Seed=108 },
            new LevelDef { Index=9,  Name="Ghost Army",        Width=16, Height=16, GhostCount=4, RequiredTreasures=5, TotalTreasures=7, SpikeTraps=2, NoiseTraps=2, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=1, GhostVisions=0, Seed=109 },
            new LevelDef { Index=10, Name="Grand Finale",      Width=20, Height=20, GhostCount=5, RequiredTreasures=6, TotalTreasures=8, SpikeTraps=2, NoiseTraps=2, LightBurstTraps=2, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=1, GhostVisions=0, Seed=110 },
        };

        [MenuItem("GraveyardHunter/Level Batch Creator")]
        public static void ShowWindow()
        {
            GetWindow<LevelBatchCreator>("Level Batch Creator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Graveyard Hunter - Level Batch Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Create All 10 Levels", GUILayout.Height(35)))
            {
                CreateAllLevels();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level List:", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var def in Levels)
            {
                string assetPath = GetLevelPath(def.Index);
                bool exists = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath) != null;
                string status = exists ? "[EXISTS]" : "[NOT CREATED]";

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Level {def.Index}: \"{def.Name}\" {def.Width}x{def.Height} - G:{def.GhostCount} T:{def.RequiredTreasures}/{def.TotalTreasures} Traps:{def.TotalTraps} Boost:{def.TotalBoosters} {status}");

                if (GUILayout.Button(exists ? "Recreate" : "Create", GUILayout.Width(80)))
                {
                    CreateLevel(def, true);
                    AssetDatabase.SaveAssets();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        public static void CreateAllLevels()
        {
            EnsureFolder(LevelsPath);

            foreach (var def in Levels)
            {
                CreateLevel(def, false);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LevelBatchCreator] Created all 10 levels.");
        }

        private static void CreateLevel(LevelDef def, bool overwrite)
        {
            string path = GetLevelPath(def.Index);

            if (!overwrite && AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
                return;

            // Delete existing if overwrite
            if (overwrite && AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            EnsureFolder(LevelsPath);

            var levelData = ScriptableObject.CreateInstance<LevelData>();
            levelData.LevelIndex = def.Index;
            levelData.LevelName = def.Name;
            levelData.GridWidth = def.Width;
            levelData.GridHeight = def.Height;
            levelData.GhostCount = def.GhostCount;
            levelData.RequiredTreasures = def.RequiredTreasures;
            levelData.MazeSeed = def.Seed;

            // Star thresholds
            levelData.ThreeStarHP = 5;
            levelData.TwoStarHP = 3;
            levelData.OneStarHP = 1;

            // Generate placements
            levelData.Placements = GeneratePlacements(def);

            AssetDatabase.CreateAsset(levelData, path);
            EditorUtility.SetDirty(levelData);
            Debug.Log($"[LevelBatchCreator] Created Level {def.Index}: {def.Name}");
        }

        private static List<ObjectPlacement> GeneratePlacements(LevelDef def)
        {
            var placements = new List<ObjectPlacement>();
            var usedPositions = new HashSet<Vector2Int>();

            int w = def.Width;
            int h = def.Height;

            // Player always spawns near (1,1)
            AddPlacement(placements, usedPositions, CellType.PlayerSpawn, 1, 1);

            // Exit at opposite corner
            AddPlacement(placements, usedPositions, CellType.ExitGate, w - 2, h - 2);

            // Treasures - spread across the grid
            var treasurePositions = GetSpreadPositions(def.TotalTreasures, w, h, usedPositions, def.Seed);
            foreach (var pos in treasurePositions)
            {
                var tp = new ObjectPlacement
                {
                    Type = CellType.Treasure,
                    TreasureType = TreasureType.Gold,
                    GridX = pos.x,
                    GridY = pos.y
                };
                placements.Add(tp);
                usedPositions.Add(pos);
            }

            // Traps - along middle paths
            var trapPositions = GetSpreadPositions(def.TotalTraps, w, h, usedPositions, def.Seed + 1000);
            int trapIdx = 0;

            for (int i = 0; i < def.SpikeTraps && trapIdx < trapPositions.Count; i++, trapIdx++)
            {
                var tp = new ObjectPlacement
                {
                    Type = CellType.Trap,
                    TrapType = TrapType.Spike,
                    GridX = trapPositions[trapIdx].x,
                    GridY = trapPositions[trapIdx].y
                };
                placements.Add(tp);
                usedPositions.Add(trapPositions[trapIdx]);
            }

            for (int i = 0; i < def.NoiseTraps && trapIdx < trapPositions.Count; i++, trapIdx++)
            {
                var tp = new ObjectPlacement
                {
                    Type = CellType.Trap,
                    TrapType = TrapType.Noise,
                    GridX = trapPositions[trapIdx].x,
                    GridY = trapPositions[trapIdx].y
                };
                placements.Add(tp);
                usedPositions.Add(trapPositions[trapIdx]);
            }

            for (int i = 0; i < def.LightBurstTraps && trapIdx < trapPositions.Count; i++, trapIdx++)
            {
                var tp = new ObjectPlacement
                {
                    Type = CellType.Trap,
                    TrapType = TrapType.LightBurst,
                    GridX = trapPositions[trapIdx].x,
                    GridY = trapPositions[trapIdx].y
                };
                placements.Add(tp);
                usedPositions.Add(trapPositions[trapIdx]);
            }

            // Boosters - near dangerous areas (middle-ish, spread)
            int totalBoosters = def.TotalBoosters;
            var boosterPositions = GetSpreadPositions(totalBoosters, w, h, usedPositions, def.Seed + 2000);
            int boosterIdx = 0;

            for (int i = 0; i < def.SmokeBombs && boosterIdx < boosterPositions.Count; i++, boosterIdx++)
            {
                var bp = new ObjectPlacement
                {
                    Type = CellType.Booster,
                    BoosterType = BoosterType.SmokeBomb,
                    GridX = boosterPositions[boosterIdx].x,
                    GridY = boosterPositions[boosterIdx].y
                };
                placements.Add(bp);
                usedPositions.Add(boosterPositions[boosterIdx]);
            }

            for (int i = 0; i < def.SpeedBoots && boosterIdx < boosterPositions.Count; i++, boosterIdx++)
            {
                var bp = new ObjectPlacement
                {
                    Type = CellType.Booster,
                    BoosterType = BoosterType.SpeedBoots,
                    GridX = boosterPositions[boosterIdx].x,
                    GridY = boosterPositions[boosterIdx].y
                };
                placements.Add(bp);
                usedPositions.Add(boosterPositions[boosterIdx]);
            }

            for (int i = 0; i < def.ShadowCloaks && boosterIdx < boosterPositions.Count; i++, boosterIdx++)
            {
                var bp = new ObjectPlacement
                {
                    Type = CellType.Booster,
                    BoosterType = BoosterType.ShadowCloak,
                    GridX = boosterPositions[boosterIdx].x,
                    GridY = boosterPositions[boosterIdx].y
                };
                placements.Add(bp);
                usedPositions.Add(boosterPositions[boosterIdx]);
            }

            for (int i = 0; i < def.GhostVisions && boosterIdx < boosterPositions.Count; i++, boosterIdx++)
            {
                var bp = new ObjectPlacement
                {
                    Type = CellType.Booster,
                    BoosterType = BoosterType.GhostVision,
                    GridX = boosterPositions[boosterIdx].x,
                    GridY = boosterPositions[boosterIdx].y
                };
                placements.Add(bp);
                usedPositions.Add(boosterPositions[boosterIdx]);
            }

            // Enemy spawn positions - spread around the grid
            var enemyPositions = GetSpreadPositions(def.GhostCount, w, h, usedPositions, def.Seed + 3000);
            foreach (var pos in enemyPositions)
            {
                placements.Add(new ObjectPlacement
                {
                    Type = CellType.EnemySpawn,
                    GridX = pos.x,
                    GridY = pos.y
                });
                usedPositions.Add(pos);
            }

            return placements;
        }

        private static void AddPlacement(List<ObjectPlacement> list, HashSet<Vector2Int> used, CellType type, int x, int y)
        {
            list.Add(new ObjectPlacement { Type = type, GridX = x, GridY = y });
            used.Add(new Vector2Int(x, y));
        }

        /// <summary>
        /// Generate spread-out positions across the grid, avoiding used positions and edges.
        /// </summary>
        private static List<Vector2Int> GetSpreadPositions(int count, int gridW, int gridH, HashSet<Vector2Int> used, int seed)
        {
            var result = new List<Vector2Int>();
            if (count <= 0) return result;

            var rng = new System.Random(seed);
            int margin = 2;
            int attempts = 0;
            int maxAttempts = count * 100;

            while (result.Count < count && attempts < maxAttempts)
            {
                attempts++;
                int x = rng.Next(margin, gridW - margin);
                int y = rng.Next(margin, gridH - margin);
                var pos = new Vector2Int(x, y);

                // Only use odd positions (cells, not walls in maze)
                if (x % 2 == 0) x++;
                if (y % 2 == 0) y++;
                if (x >= gridW - 1) x = gridW - 2;
                if (y >= gridH - 1) y = gridH - 2;
                pos = new Vector2Int(x, y);

                if (used.Contains(pos)) continue;

                // Check minimum distance from other results
                bool tooClose = false;
                int minDist = Mathf.Max(2, (gridW + gridH) / (count + 2));
                foreach (var existing in result)
                {
                    if (Mathf.Abs(existing.x - pos.x) + Mathf.Abs(existing.y - pos.y) < minDist)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                result.Add(pos);
                used.Add(pos);
            }

            // Fallback: if we didn't get enough, just place randomly
            while (result.Count < count)
            {
                int x = rng.Next(margin, gridW - margin);
                int y = rng.Next(margin, gridH - margin);
                if (x % 2 == 0) x++;
                if (y % 2 == 0) y++;
                if (x >= gridW - 1) x = gridW - 2;
                if (y >= gridH - 1) y = gridH - 2;

                var pos = new Vector2Int(x, y);
                if (!used.Contains(pos))
                {
                    result.Add(pos);
                    used.Add(pos);
                }
            }

            return result;
        }

        private static string GetLevelPath(int index)
        {
            return $"{LevelsPath}/Level_{index:D2}.asset";
        }

        private static void EnsureFolder(string path)
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

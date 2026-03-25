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
            public int ObstacleCount;
            public int Seed;

            public int TotalTraps => SpikeTraps + NoiseTraps + LightBurstTraps;
            public int TotalBoosters => SmokeBombs + SpeedBoots + ShadowCloaks + GhostVisions;
        }

        private static readonly LevelDef[] Levels = new LevelDef[]
        {
            // === Map 1: Small (12x12) — Level 1-5 ===
            new LevelDef { Index=1,  Name="Tutorial",         Width=12, Height=12, GhostCount=1, RequiredTreasures=2, TotalTreasures=3, SpikeTraps=0, NoiseTraps=0, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, ObstacleCount=2, Seed=101 },
            new LevelDef { Index=2,  Name="Watch Your Step",  Width=12, Height=12, GhostCount=1, RequiredTreasures=2, TotalTreasures=3, SpikeTraps=1, NoiseTraps=0, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, ObstacleCount=2, Seed=102 },
            new LevelDef { Index=3,  Name="Quiet Steps",      Width=12, Height=12, GhostCount=1, RequiredTreasures=3, TotalTreasures=4, SpikeTraps=0, NoiseTraps=1, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, ObstacleCount=3, Seed=103 },
            new LevelDef { Index=4,  Name="Double Trouble",   Width=12, Height=12, GhostCount=2, RequiredTreasures=3, TotalTreasures=4, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=0, SmokeBombs=0, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, ObstacleCount=3, Seed=104 },
            new LevelDef { Index=5,  Name="Smoke Screen",     Width=12, Height=12, GhostCount=2, RequiredTreasures=3, TotalTreasures=5, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=0, SmokeBombs=1, SpeedBoots=0, ShadowCloaks=0, GhostVisions=0, ObstacleCount=4, Seed=105 },

            // === Map 2: Medium (16x16) — Level 6-10 ===
            new LevelDef { Index=6,  Name="Bigger Grounds",   Width=16, Height=16, GhostCount=3, RequiredTreasures=4, TotalTreasures=5, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, ObstacleCount=5, Seed=106 },
            new LevelDef { Index=7,  Name="All Traps",        Width=16, Height=16, GhostCount=3, RequiredTreasures=4, TotalTreasures=6, SpikeTraps=1, NoiseTraps=1, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, ObstacleCount=5, Seed=107 },
            new LevelDef { Index=8,  Name="Treasure Hunt",    Width=16, Height=16, GhostCount=3, RequiredTreasures=5, TotalTreasures=7, SpikeTraps=2, NoiseTraps=1, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=0, GhostVisions=0, ObstacleCount=6, Seed=108 },
            new LevelDef { Index=9,  Name="Ghost Army",       Width=16, Height=16, GhostCount=4, RequiredTreasures=5, TotalTreasures=7, SpikeTraps=2, NoiseTraps=2, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=1, GhostVisions=0, ObstacleCount=7, Seed=109 },
            new LevelDef { Index=10, Name="Night Patrol",     Width=16, Height=16, GhostCount=4, RequiredTreasures=5, TotalTreasures=8, SpikeTraps=2, NoiseTraps=2, LightBurstTraps=2, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=1, GhostVisions=0, ObstacleCount=7, Seed=110 },

            // === Map 3: Large (20x20) — Level 11-15 ===
            new LevelDef { Index=11, Name="The Catacombs",    Width=20, Height=20, GhostCount=4, RequiredTreasures=5, TotalTreasures=8, SpikeTraps=2, NoiseTraps=2, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=1, GhostVisions=1, ObstacleCount=8, Seed=111 },
            new LevelDef { Index=12, Name="Shadow Maze",      Width=20, Height=20, GhostCount=4, RequiredTreasures=6, TotalTreasures=9, SpikeTraps=3, NoiseTraps=2, LightBurstTraps=1, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=1, GhostVisions=1, ObstacleCount=8, Seed=112 },
            new LevelDef { Index=13, Name="Cursed Temple",    Width=20, Height=20, GhostCount=5, RequiredTreasures=6, TotalTreasures=9, SpikeTraps=3, NoiseTraps=2, LightBurstTraps=2, SmokeBombs=1, SpeedBoots=1, ShadowCloaks=1, GhostVisions=1, ObstacleCount=9, Seed=113 },
            new LevelDef { Index=14, Name="Light Fortress",   Width=20, Height=20, GhostCount=5, RequiredTreasures=6, TotalTreasures=10,SpikeTraps=3, NoiseTraps=3, LightBurstTraps=2, SmokeBombs=2, SpeedBoots=1, ShadowCloaks=1, GhostVisions=1, ObstacleCount=9, Seed=114 },
            new LevelDef { Index=15, Name="The Labyrinth",    Width=20, Height=20, GhostCount=5, RequiredTreasures=7, TotalTreasures=10,SpikeTraps=3, NoiseTraps=3, LightBurstTraps=2, SmokeBombs=2, SpeedBoots=1, ShadowCloaks=1, GhostVisions=1, ObstacleCount=10, Seed=115 },

            // === Map 3+: Large (20x20) — Level 16-20 ===
            new LevelDef { Index=16, Name="Phantom Halls",    Width=20, Height=20, GhostCount=5, RequiredTreasures=7, TotalTreasures=11,SpikeTraps=4, NoiseTraps=3, LightBurstTraps=2, SmokeBombs=2, SpeedBoots=1, ShadowCloaks=1, GhostVisions=1, ObstacleCount=10, Seed=116 },
            new LevelDef { Index=17, Name="Wraith Domain",    Width=20, Height=20, GhostCount=6, RequiredTreasures=7, TotalTreasures=11,SpikeTraps=4, NoiseTraps=3, LightBurstTraps=3, SmokeBombs=2, SpeedBoots=2, ShadowCloaks=1, GhostVisions=1, ObstacleCount=11, Seed=117 },
            new LevelDef { Index=18, Name="Bone Yard",        Width=20, Height=20, GhostCount=6, RequiredTreasures=8, TotalTreasures=12,SpikeTraps=4, NoiseTraps=4, LightBurstTraps=3, SmokeBombs=2, SpeedBoots=2, ShadowCloaks=1, GhostVisions=1, ObstacleCount=11, Seed=118 },
            new LevelDef { Index=19, Name="Soul Asylum",      Width=20, Height=20, GhostCount=6, RequiredTreasures=8, TotalTreasures=12,SpikeTraps=5, NoiseTraps=4, LightBurstTraps=3, SmokeBombs=2, SpeedBoots=2, ShadowCloaks=2, GhostVisions=1, ObstacleCount=12, Seed=119 },
            new LevelDef { Index=20, Name="Grand Finale",     Width=20, Height=20, GhostCount=7, RequiredTreasures=8, TotalTreasures=13,SpikeTraps=5, NoiseTraps=4, LightBurstTraps=4, SmokeBombs=2, SpeedBoots=2, ShadowCloaks=2, GhostVisions=1, ObstacleCount=12, Seed=120 },
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

            if (GUILayout.Button("Create All 20 Levels", GUILayout.Height(35)))
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
                EditorGUILayout.LabelField($"Level {def.Index}: \"{def.Name}\" {def.Width}x{def.Height} - G:{def.GhostCount} T:{def.RequiredTreasures}/{def.TotalTreasures} Traps:{def.TotalTraps} Boost:{def.TotalBoosters} Obs:{def.ObstacleCount} {status}");

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
            Debug.Log($"[LevelBatchCreator] Created all {Levels.Length} levels.");
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

            // Obstacle, trap, booster counts (used as fallback for random spawning)
            levelData.ObstacleCount = def.ObstacleCount;
            levelData.TrapCount = def.TotalTraps;
            levelData.BoosterCount = def.TotalBoosters;

            // Per-type treasure requirements (progressive variety)
            levelData.TreasureRequirements = GenerateTreasureRequirements(def);

            // Generate placements
            levelData.Placements = GeneratePlacements(def);

            AssetDatabase.CreateAsset(levelData, path);
            EditorUtility.SetDirty(levelData);
            Debug.Log($"[LevelBatchCreator] Created Level {def.Index}: {def.Name}");
        }

        /// <summary>
        /// Progressive treasure variety per level:
        /// Level 1-2: Gold only
        /// Level 3-4: Gold + Silver
        /// Level 5-6: Gold + Silver + Coin
        /// Level 7+:  Gold + Silver + Coin + Artifact
        /// </summary>
        private static List<TreasureRequirement> GenerateTreasureRequirements(LevelDef def)
        {
            var reqs = new List<TreasureRequirement>();
            int remaining = def.RequiredTreasures;

            if (remaining <= 0) return reqs;

            // Always require Gold
            int goldCount = Mathf.Max(1, remaining / GetTypeCount(def.Index));
            reqs.Add(new TreasureRequirement { Type = TreasureType.Gold, Count = goldCount });
            remaining -= goldCount;

            // Silver from level 3+
            if (def.Index >= 3 && remaining > 0)
            {
                int silverCount = Mathf.Max(1, remaining / Mathf.Max(1, GetTypeCount(def.Index) - 1));
                reqs.Add(new TreasureRequirement { Type = TreasureType.Silver, Count = silverCount });
                remaining -= silverCount;
            }

            // Coin from level 5+
            if (def.Index >= 5 && remaining > 0)
            {
                int coinCount = Mathf.Max(1, remaining / Mathf.Max(1, GetTypeCount(def.Index) - 2));
                reqs.Add(new TreasureRequirement { Type = TreasureType.Coin, Count = coinCount });
                remaining -= coinCount;
            }

            // Artifact from level 7+
            if (def.Index >= 7 && remaining > 0)
            {
                reqs.Add(new TreasureRequirement { Type = TreasureType.Artifact, Count = remaining });
                remaining = 0;
            }

            // Distribute any remainder to Gold
            if (remaining > 0)
                reqs[0].Count += remaining;

            return reqs;
        }

        private static int GetTypeCount(int levelIndex)
        {
            if (levelIndex >= 7) return 4;
            if (levelIndex >= 5) return 3;
            if (levelIndex >= 3) return 2;
            return 1;
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

            // Treasures - spread across the grid, assign types from requirements
            var treasureRequirements = GenerateTreasureRequirements(def);
            var treasureTypeQueue = new List<TreasureType>();
            foreach (var req in treasureRequirements)
            {
                for (int t = 0; t < req.Count; t++)
                    treasureTypeQueue.Add(req.Type);
            }
            // Fill remaining with Gold for extra treasures beyond required
            while (treasureTypeQueue.Count < def.TotalTreasures)
                treasureTypeQueue.Add(TreasureType.Gold);

            var treasurePositions = GetSpreadPositions(def.TotalTreasures, w, h, usedPositions, def.Seed);
            for (int ti = 0; ti < treasurePositions.Count; ti++)
            {
                var pos = treasurePositions[ti];
                var tp = new ObjectPlacement
                {
                    Type = CellType.Treasure,
                    TreasureType = ti < treasureTypeQueue.Count ? treasureTypeQueue[ti] : TreasureType.Gold,
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

            // Obstacles (shelters) - spread around the grid
            var obstaclePositions = GetSpreadPositions(def.ObstacleCount, w, h, usedPositions, def.Seed + 4000);
            foreach (var pos in obstaclePositions)
            {
                placements.Add(new ObjectPlacement
                {
                    Type = CellType.Obstacle,
                    GridX = pos.x,
                    GridY = pos.y
                });
                usedPositions.Add(pos);
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

using System.Collections.Generic;
using GraveyardHunter.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Level
{
    [CreateAssetMenu(fileName = "Level_01", menuName = "GraveyardHunter/Level Data")]
    public class LevelData : ScriptableObject
    {
        [BoxGroup("Info")]
        public int LevelIndex;

        [BoxGroup("Info")]
        public string LevelName;

        [BoxGroup("Grid")]
        public int GridWidth = 12;

        [BoxGroup("Grid")]
        public int GridHeight = 12;

        [BoxGroup("Rules"), Tooltip("Legacy: total count. Use TreasureRequirements instead.")]
        public int RequiredTreasures = 3;

        [BoxGroup("Rules"), TableList, Tooltip("Required treasure types and quantities. If empty, falls back to RequiredTreasures count.")]
        public List<TreasureRequirement> TreasureRequirements = new();

        [BoxGroup("Rules"), Tooltip("Full HP remaining = 3 stars")]
        public int ThreeStarHP = 5;

        [BoxGroup("Rules")]
        public int TwoStarHP = 3;

        [BoxGroup("Rules")]
        public int OneStarHP = 1;

        [BoxGroup("Enemies")]
        public int GhostCount = 1;

        [BoxGroup("Enemies"), Tooltip("Which enemy types can spawn in this level. Empty = Ghost only.")]
        public List<Core.EnemyType> AllowedEnemyTypes = new();

        /// <summary>
        /// Returns a random allowed enemy type for this level.
        /// Falls back to Ghost if no types are configured.
        /// </summary>
        public Core.EnemyType GetRandomEnemyType()
        {
            if (AllowedEnemyTypes == null || AllowedEnemyTypes.Count == 0)
                return Core.EnemyType.Ghost;
            return AllowedEnemyTypes[UnityEngine.Random.Range(0, AllowedEnemyTypes.Count)];
        }

        [BoxGroup("Obstacles"), Tooltip("Number of hiding spots to spawn")]
        public int ObstacleCount = 3;

        [BoxGroup("Fallback Counts"), Tooltip("Random traps if no trap placements")]
        public int TrapCount = 0;

        [BoxGroup("Fallback Counts"), Tooltip("Random boosters if no booster placements")]
        public int BoosterCount = 0;

        [BoxGroup("Placements"), TableList]
        public List<ObjectPlacement> Placements = new();

        [BoxGroup("Maze"), Tooltip("-1 means random seed")]
        public int MazeSeed = -1;

        [BoxGroup("Maze"), TextArea(5, 10)]
        public string MazeLayout;
    }

    [System.Serializable]
    public class TreasureRequirement
    {
        public TreasureType Type;
        public int Count = 1;
    }

    [System.Serializable]
    public class ObjectPlacement
    {
        public CellType Type;

        [ShowIf("Type", CellType.Trap)]
        public TrapType TrapType;

        [ShowIf("Type", CellType.Booster)]
        public BoosterType BoosterType;

        [ShowIf("Type", CellType.Treasure)]
        public TreasureType TreasureType;

        public int GridX;
        public int GridY;
    }
}

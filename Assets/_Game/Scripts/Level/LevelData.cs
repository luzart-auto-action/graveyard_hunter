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

        [BoxGroup("Rules")]
        public int RequiredTreasures = 3;

        [BoxGroup("Rules"), Tooltip("Full HP remaining = 3 stars")]
        public int ThreeStarHP = 5;

        [BoxGroup("Rules")]
        public int TwoStarHP = 3;

        [BoxGroup("Rules")]
        public int OneStarHP = 1;

        [BoxGroup("Enemies")]
        public int GhostCount = 1;

        [BoxGroup("Placements"), TableList]
        public List<ObjectPlacement> Placements = new();

        [BoxGroup("Maze"), Tooltip("-1 means random seed")]
        public int MazeSeed = -1;

        [BoxGroup("Maze"), TextArea(5, 10)]
        public string MazeLayout;
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

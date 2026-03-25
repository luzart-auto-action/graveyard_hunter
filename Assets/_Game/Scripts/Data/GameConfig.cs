using System.Collections.Generic;
using GraveyardHunter.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "GraveyardHunter/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [BoxGroup("Player")]
        public int PlayerMaxHP = 5;

        [BoxGroup("Player")]
        public float PlayerMoveSpeed = 5f;

        [BoxGroup("Player"), Tooltip("15% faster than ghost")]
        public float PlayerSpeedAdvantage = 1.15f;

        [BoxGroup("Player"), Tooltip("Circle light around player")]
        public float PlayerLightRadius = 3f;

        [BoxGroup("Player"), Tooltip("Cone light in front")]
        public float PlayerFlashlightRange = 8f;

        [BoxGroup("Player")]
        public float PlayerFlashlightAngle = 30f;

        [BoxGroup("Player"), Tooltip("1 HP per second in light")]
        public float LightDamagePerSecond = 1f;

        [BoxGroup("Player"), Tooltip("20% slow in light")]
        public float LightSlowPercent = 0.2f;

        [BoxGroup("Player"), Tooltip("2s after leaving light")]
        public float SlowRecoveryDelay = 2f;

        [BoxGroup("Ghost (Legacy - use EnemyTypes instead)")]
        public float GhostScanSpeed = 2.5f;

        [BoxGroup("Ghost (Legacy - use EnemyTypes instead)")]
        public float GhostChaseSpeed = 4f;

        [BoxGroup("Ghost (Legacy - use EnemyTypes instead)"), Tooltip("70-90 degrees")]
        public float GhostLightConeAngle = 80f;

        [BoxGroup("Ghost (Legacy - use EnemyTypes instead)")]
        public float GhostLightRange = 10f;

        [BoxGroup("Ghost (Legacy - use EnemyTypes instead)"), Tooltip("20% boost in escape phase")]
        public float GhostEscapeSpeedBoost = 1.2f;

        [BoxGroup("Enemy Types"), TableList]
        public List<EnemyTypeData> EnemyTypes = new();

        public EnemyTypeData GetEnemyData(EnemyType type)
        {
            foreach (var data in EnemyTypes)
            {
                if (data.Type == type) return data;
            }
            return EnemyTypeData.GetDefault(type);
        }

        public GameObject GetEnemyPrefab(EnemyType type)
        {
            var data = GetEnemyData(type);
            return data?.Prefab != null ? data.Prefab : GhostPrefab;
        }

        [BoxGroup("Gameplay")]
        public float EscapeMusicSpeedMultiplier = 1.3f;

        [BoxGroup("Gameplay")]
        public int ScorePerLevel = 100;

        [BoxGroup("Gameplay")]
        public int ScorePerTreasure = 10;

        [BoxGroup("Gameplay")]
        public int ScorePerSecond = 1;

        [BoxGroup("Gameplay")]
        public int NoDamageBonus = 50;

        [BoxGroup("Booster Durations")]
        public float SmokeBombDuration = 3f;

        [BoxGroup("Booster Durations")]
        public float SpeedBootsDuration = 5f;

        [BoxGroup("Booster Durations")]
        public float SpeedBootsMultiplier = 1.3f;

        [BoxGroup("Booster Durations")]
        public float ShadowCloakDuration = 4f;

        [BoxGroup("Booster Durations")]
        public float GhostVisionDuration = 5f;

        [BoxGroup("Trap")]
        public int SpikeDamage = 1;

        [BoxGroup("Trap")]
        public float LightBurstDuration = 2f;

        [BoxGroup("Prefabs"), Required]
        public GameObject PlayerPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject GhostPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject TreasurePrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject ExitGatePrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject WallPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject FloorPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject SpikeTrapPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject NoiseTrapPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject LightBurstTrapPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject SmokeBombPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject SpeedBootsPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject ShadowCloakPrefab;

        [BoxGroup("Prefabs"), Required]
        public GameObject GhostVisionPrefab;

        [BoxGroup("Skins")]
        public List<SkinData> AvailableSkins;
    }
}

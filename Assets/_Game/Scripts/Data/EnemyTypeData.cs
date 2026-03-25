using GraveyardHunter.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Data
{
    [System.Serializable]
    public class EnemyTypeData
    {
        [HorizontalGroup("Type"), LabelWidth(80)]
        public EnemyType Type;

        [BoxGroup("Speed")]
        public float ScanSpeed = 2.5f;

        [BoxGroup("Speed")]
        public float ChaseSpeed = 4f;

        [BoxGroup("Vision"), Tooltip("Cone angle in degrees")]
        public float LightConeAngle = 80f;

        [BoxGroup("Vision"), Tooltip("Detection range")]
        public float LightRange = 10f;

        [BoxGroup("Vision"), Tooltip("Patrol radius when scanning")]
        public float PatrolRadius = 15f;

        [BoxGroup("Vision"), Tooltip("Seconds before giving up chase when player lost")]
        public float ChaseTimeout = 3f;

        [BoxGroup("Visual")]
        public Color EyeColor = new Color(1f, 0.84f, 0f);

        [BoxGroup("Visual")]
        public Color ChaseLightColor = Color.red;

        [BoxGroup("Visual")]
        public Color ConeLightColor = new Color(0.9f, 0.9f, 1f);

        [BoxGroup("Prefab"), Required]
        public GameObject Prefab;

        public static EnemyTypeData GetDefault(EnemyType type)
        {
            return type switch
            {
                EnemyType.Ghost => new EnemyTypeData
                {
                    Type = EnemyType.Ghost,
                    ScanSpeed = 2.5f,
                    ChaseSpeed = 4f,
                    LightConeAngle = 80f,
                    LightRange = 10f,
                    PatrolRadius = 15f,
                    ChaseTimeout = 3f,
                    EyeColor = new Color(1f, 0.84f, 0f),
                    ChaseLightColor = Color.red,
                    ConeLightColor = new Color(0.9f, 0.9f, 1f)
                },
                EnemyType.Werewolf => new EnemyTypeData
                {
                    Type = EnemyType.Werewolf,
                    ScanSpeed = 3.5f,
                    ChaseSpeed = 5.5f,
                    LightConeAngle = 100f,
                    LightRange = 8f,
                    PatrolRadius = 20f,
                    ChaseTimeout = 5f,
                    EyeColor = new Color(1f, 0.3f, 0f),
                    ChaseLightColor = new Color(1f, 0f, 0f),
                    ConeLightColor = new Color(1f, 0.6f, 0.4f)
                },
                EnemyType.Monster => new EnemyTypeData
                {
                    Type = EnemyType.Monster,
                    ScanSpeed = 1.8f,
                    ChaseSpeed = 3f,
                    LightConeAngle = 50f,
                    LightRange = 14f,
                    PatrolRadius = 10f,
                    ChaseTimeout = 6f,
                    EyeColor = new Color(0.4f, 1f, 0.4f),
                    ChaseLightColor = new Color(1f, 0.2f, 0.2f),
                    ConeLightColor = new Color(0.5f, 1f, 0.5f)
                },
                EnemyType.Robot => new EnemyTypeData
                {
                    Type = EnemyType.Robot,
                    ScanSpeed = 2f,
                    ChaseSpeed = 3.5f,
                    LightConeAngle = 120f,
                    LightRange = 6f,
                    PatrolRadius = 12f,
                    ChaseTimeout = 2f,
                    EyeColor = new Color(0.3f, 0.7f, 1f),
                    ChaseLightColor = new Color(1f, 0.5f, 0f),
                    ConeLightColor = new Color(0.4f, 0.7f, 1f)
                },
                _ => new EnemyTypeData { Type = type }
            };
        }
    }
}

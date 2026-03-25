using UnityEngine;

namespace GraveyardHunter.Core
{
    // === UI Button Events (GameManager subscribes) ===
    public struct PlayButtonEvent { }
    public struct ResetLevelEvent { }
    public struct NextLevelEvent { }
    public struct GoHomeEvent { }
    public struct PauseEvent { }
    public struct ResumeEvent { }
    public struct RetryEvent { }

    // === Game Flow Events ===
    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState NewState;
    }

    public struct LevelLoadedEvent
    {
        public int LevelIndex;
    }

    public struct LevelCompletedEvent
    {
        public int LevelIndex;
        public int Score;
        public int Stars;
        public float TimeElapsed;
        public int HPRemaining;
    }

    public struct LevelFailedEvent
    {
        public int LevelIndex;
        public string Reason;
    }

    // === Player Events ===
    public struct PlayerHPChangedEvent
    {
        public int CurrentHP;
        public int MaxHP;
    }

    public struct PlayerDiedEvent { }

    public struct PlayerInLightEvent
    {
        public bool InLight;
    }

    // === Treasure Events ===
    public struct TreasureCollectedEvent
    {
        public TreasureType Type;
        public int CurrentCount;
        public int RequiredCount;
        /// <summary>Per-type status: collected / required for each treasure type.</summary>
        public System.Collections.Generic.Dictionary<TreasureType, (int collected, int required)> TypeStatus;
    }

    public struct AllTreasuresCollectedEvent { }

    // === Shelter Events ===
    public struct PlayerShelterEvent
    {
        public bool IsInShelter;
    }

    // === Escape Events ===
    public struct EscapePhaseStartedEvent { }

    public struct PlayerEscapedEvent { }

    // === Enemy Events ===
    public struct GhostStateChangedEvent
    {
        public int GhostId;
        public GhostState NewState;
    }

    public struct NoiseTriggeredEvent
    {
        public Vector3 Position;
    }

    // === Booster Events ===
    public struct BoosterPickedUpEvent
    {
        public BoosterType Type;
        public BoosterPickedUpEvent(BoosterType type) { Type = type; }
    }

    public struct BoosterActivatedEvent
    {
        public BoosterType Type;
        public float Duration;
        public BoosterActivatedEvent(BoosterType type, float duration) { Type = type; Duration = duration; }
    }

    public struct BoosterExpiredEvent
    {
        public BoosterType Type;
        public BoosterExpiredEvent(BoosterType type) { Type = type; }
    }

    // === Score Events ===
    public struct ScoreChangedEvent
    {
        public int TotalScore;
    }

    // === Audio Events ===
    public struct PlaySFXEvent
    {
        public string SFXName;
        public PlaySFXEvent(string sfxName) { SFXName = sfxName; }
    }

    public struct PlayMusicEvent
    {
        public string MusicName;
        public bool Loop;
    }

    public struct StopMusicEvent { }

    // === FX Events ===
    public struct SpawnFXEvent
    {
        public string FXName;
        public Vector3 Position;
        public Quaternion Rotation;
        public SpawnFXEvent(string fxName, Vector3 position) { FXName = fxName; Position = position; Rotation = Quaternion.identity; }
    }

    // === Shop Events ===
    public struct SkinPurchasedEvent
    {
        public int SkinIndex;
    }

    public struct SkinEquippedEvent
    {
        public int SkinIndex;
    }
}

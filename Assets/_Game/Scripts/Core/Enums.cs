namespace GraveyardHunter.Core
{
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        EscapePhase,
        Paused,
        Win,
        Fail
    }

    public enum GhostState
    {
        Scan,
        Chase
    }

    public enum TrapType
    {
        Spike,
        Noise,
        LightBurst
    }

    public enum BoosterType
    {
        SmokeBomb,
        SpeedBoots,
        ShadowCloak,
        GhostVision
    }

    public enum TreasureType
    {
        Gold,
        Silver,
        Coin,
        Artifact
    }

    public enum CellType
    {
        Empty,
        Wall,
        PlayerSpawn,
        ExitGate,
        Treasure,
        Trap,
        Booster,
        EnemySpawn
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}

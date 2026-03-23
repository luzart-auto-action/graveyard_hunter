namespace GraveyardHunter.GameState
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update();
    }
}
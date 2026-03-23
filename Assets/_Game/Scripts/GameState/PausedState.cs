using UnityEngine;

namespace GraveyardHunter.GameState
{
    public class PausedState : IGameState
    {
        public void Enter()
        {
            Time.timeScale = 0f;
        }

        public void Exit()
        {
            Time.timeScale = 1f;
        }

        public void Update() { }
    }
}
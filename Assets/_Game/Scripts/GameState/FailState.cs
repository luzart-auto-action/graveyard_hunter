using UnityEngine;

namespace GraveyardHunter.GameState
{
    public class FailState : IGameState
    {
        public void Enter()
        {
            Time.timeScale = 0f;
        }

        public void Exit() { }

        public void Update() { }
    }
}
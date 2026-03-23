using UnityEngine;

namespace GraveyardHunter.GameState
{
    public class WinState : IGameState
    {
        public void Enter()
        {
            Time.timeScale = 0f;
        }

        public void Exit() { }

        public void Update() { }
    }
}
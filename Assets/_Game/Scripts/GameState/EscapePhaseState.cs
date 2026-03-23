using UnityEngine;

namespace GraveyardHunter.GameState
{
    public class EscapePhaseState : IGameState
    {
        public void Enter()
        {
            Time.timeScale = 1f;
        }

        public void Exit() { }

        public void Update() { }
    }
}
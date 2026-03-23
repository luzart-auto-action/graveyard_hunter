using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using GraveyardHunter.Core;

namespace GraveyardHunter.GameState
{
    public class GameStateManager : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        private Core.GameState _currentGameState;

        private Dictionary<Core.GameState, IGameState> _states;
        private IGameState _currentState;

        private void Awake()
        {
            _states = new Dictionary<Core.GameState, IGameState>
            {
                { Core.GameState.MainMenu, new MainMenuState() },
                { Core.GameState.Loading, new LoadingState() },
                { Core.GameState.Playing, new PlayingState() },
                { Core.GameState.EscapePhase, new EscapePhaseState() },
                { Core.GameState.Paused, new PausedState() },
                { Core.GameState.Win, new WinState() },
                { Core.GameState.Fail, new FailState() }
            };

            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameStateManager>();
        }

        private void Update()
        {
            _currentState?.Update();
        }

        public void ChangeState(Core.GameState newState)
        {
            if (!_states.ContainsKey(newState))
            {
                Debug.LogError($"[GameStateManager] State not found: {newState}");
                return;
            }

            var previousState = _currentGameState;

            _currentState?.Exit();

            _currentGameState = newState;
            _currentState = _states[newState];

            EventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = newState
            });

            _currentState.Enter();
        }

        public Core.GameState GetCurrentState()
        {
            return _currentGameState;
        }
    }
}
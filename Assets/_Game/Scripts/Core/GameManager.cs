using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using GraveyardHunter.Core;
using GameStateNS = GraveyardHunter.GameState;
using GraveyardHunter.Level;
using GraveyardHunter.UI;
using GraveyardHunter.Audio;
using GraveyardHunter.Data;

namespace GraveyardHunter.Core
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        private static GameManager _instance;
        public static GameManager Instance => _instance;

        #endregion

        #region Debug Fields

        [ShowInInspector, ReadOnly]
        private int _currentLevelIndex;

        [ShowInInspector, ReadOnly]
        private GameState _currentState;

        #endregion

        #region Cached References

        private GameStateNS.GameStateManager _gameStateManager;
        private LevelManager _levelManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            SubscribeEvents();
            _currentLevelIndex = PlayerProgressData.GetCurrentLevel();
        }

        private void Start()
        {
            // Start() runs after ALL Awake() calls -> services are registered
            CacheServiceReferences();

            // Show MainMenu on game start
            _gameStateManager.ChangeState(GameState.MainMenu);
            _currentState = GameState.MainMenu;
            _uiManager.ShowPanel("UIMainMenu");
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();

            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Service References

        private void CacheServiceReferences()
        {
            _gameStateManager = ServiceLocator.Get<GameStateNS.GameStateManager>();
            _levelManager = ServiceLocator.Get<LevelManager>();
            _uiManager = ServiceLocator.Get<UIManager>();
            _audioManager = ServiceLocator.Get<AudioManager>();
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            // UI Button Events
            EventBus.Subscribe<PlayButtonEvent>(OnPlayButton);
            EventBus.Subscribe<ResetLevelEvent>(OnResetLevel);
            EventBus.Subscribe<NextLevelEvent>(OnNextLevel);
            EventBus.Subscribe<GoHomeEvent>(OnGoHome);
            EventBus.Subscribe<PauseEvent>(OnPause);
            EventBus.Subscribe<ResumeEvent>(OnResume);
            EventBus.Subscribe<RetryEvent>(OnRetry);

            // Game Flow Events
            EventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            EventBus.Subscribe<LevelFailedEvent>(OnLevelFailed);

            // Player Events
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<PlayerEscapedEvent>(OnPlayerEscaped);
            EventBus.Subscribe<PlayerHPChangedEvent>(OnPlayerHPChanged);

            // Treasure Events
            EventBus.Subscribe<AllTreasuresCollectedEvent>(OnAllTreasuresCollected);
            EventBus.Subscribe<TreasureCollectedEvent>(OnTreasureCollected);

            // Score & Booster Events
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<BoosterPickedUpEvent>(OnBoosterPickedUp);

            // Escape Events
            EventBus.Subscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);
        }

        private void UnsubscribeEvents()
        {
            // UI Button Events
            EventBus.Unsubscribe<PlayButtonEvent>(OnPlayButton);
            EventBus.Unsubscribe<ResetLevelEvent>(OnResetLevel);
            EventBus.Unsubscribe<NextLevelEvent>(OnNextLevel);
            EventBus.Unsubscribe<GoHomeEvent>(OnGoHome);
            EventBus.Unsubscribe<PauseEvent>(OnPause);
            EventBus.Unsubscribe<ResumeEvent>(OnResume);
            EventBus.Unsubscribe<RetryEvent>(OnRetry);

            // Game Flow Events
            EventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            EventBus.Unsubscribe<LevelFailedEvent>(OnLevelFailed);

            // Player Events
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<PlayerEscapedEvent>(OnPlayerEscaped);
            EventBus.Unsubscribe<PlayerHPChangedEvent>(OnPlayerHPChanged);

            // Treasure Events
            EventBus.Unsubscribe<AllTreasuresCollectedEvent>(OnAllTreasuresCollected);
            EventBus.Unsubscribe<TreasureCollectedEvent>(OnTreasureCollected);

            // Score & Booster Events
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<BoosterPickedUpEvent>(OnBoosterPickedUp);

            // Escape Events
            EventBus.Unsubscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);
        }

        #endregion

        #region UI Button Handlers

        private void OnPlayButton(PlayButtonEvent evt)
        {
            // Read the selected level (UIMainMenu may have changed it)
            _currentLevelIndex = PlayerProgressData.GetCurrentLevel();
            StartLevel(_currentLevelIndex);
        }

        private void OnResetLevel(ResetLevelEvent evt)
        {
            StartLevel(_currentLevelIndex);
        }

        private void OnNextLevel(NextLevelEvent evt)
        {
            _currentLevelIndex++;
            StartLevel(_currentLevelIndex);
        }

        private void OnGoHome(GoHomeEvent evt)
        {
            Time.timeScale = 1f;
            _levelManager.ClearLevel();
            _uiManager.ForceHideAllPanels();
            _gameStateManager.ChangeState(GameState.MainMenu);
            _currentState = GameState.MainMenu;
            _uiManager.ShowPanel("UIMainMenu");
            _audioManager.StopMusic();
        }

        private void OnPause(PauseEvent evt)
        {
            if (_currentState != GameState.Playing && _currentState != GameState.EscapePhase)
                return;

            Time.timeScale = 0f;
            _gameStateManager.ChangeState(GameState.Paused);
            _currentState = GameState.Paused;
            _uiManager.ShowPanel("PopupPause");
        }

        private void OnResume(ResumeEvent evt)
        {
            if (_currentState != GameState.Paused)
                return;

            Time.timeScale = 1f;
            _gameStateManager.ChangeState(GameState.Playing);
            _currentState = GameState.Playing;
            _uiManager.HidePanel("PopupPause");
        }

        private void OnRetry(RetryEvent evt)
        {
            StartLevel(_currentLevelIndex);
        }

        #endregion

        #region Game Flow Handlers

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            HandleWin(evt.Score, evt.Stars);
        }

        private void OnLevelFailed(LevelFailedEvent evt)
        {
            HandleFail();
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            EventBus.Publish(new LevelFailedEvent
            {
                LevelIndex = _currentLevelIndex,
                Reason = "Player died"
            });
        }

        private void OnPlayerEscaped(PlayerEscapedEvent evt)
        {
            int score = _levelManager.CalculateScore();
            int stars = _levelManager.CalculateStars();

            EventBus.Publish(new LevelCompletedEvent
            {
                LevelIndex = _currentLevelIndex,
                Score = score,
                Stars = stars
            });
        }

        #endregion

        #region Treasure & Collectible Handlers

        private void OnAllTreasuresCollected(AllTreasuresCollectedEvent evt)
        {
            _gameStateManager.ChangeState(GameState.EscapePhase);
            _currentState = GameState.EscapePhase;

            EventBus.Publish(new EscapePhaseStartedEvent());
        }

        private void OnTreasureCollected(TreasureCollectedEvent evt)
        {
            // UI workers listen directly or UIManager handles via its own subscription
            // GameManager ensures state consistency
        }

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            // UI update handled by UIManager
        }

        private void OnPlayerHPChanged(PlayerHPChangedEvent evt)
        {
            // UI update handled by UIManager
        }

        private void OnBoosterPickedUp(BoosterPickedUpEvent evt)
        {
            // UI notification handled by UIManager
        }

        private void OnEscapePhaseStarted(EscapePhaseStartedEvent evt)
        {
            // Audio/UI workers react to this; GameManager already set state in OnAllTreasuresCollected
        }

        #endregion

        #region Core Game Flows

        private void StartLevel(int levelIndex)
        {
            Time.timeScale = 1f;
            _currentLevelIndex = levelIndex;

            _uiManager.ForceHideAllPanels();
            _levelManager.ClearLevel();
            _levelManager.LoadLevel(levelIndex);

            _gameStateManager.ChangeState(GameState.Playing);
            _currentState = GameState.Playing;

            // Explicitly show GameplayUI after state change
            // (ForceHideAllPanels unsubscribes GameplayUI from events,
            //  so it can't self-activate from the GameStateChangedEvent)
            _uiManager.ShowGameplayUI();

            // Reset UI display with initial values for the new level
            var levelData = _levelManager.GetCurrentLevelData();
            var gameConfig = _levelManager.GetGameConfig();
            var gameplayUI = _uiManager.GetPanel<UI.GameplayUI>("GameplayUI");
            if (gameplayUI != null && levelData != null && gameConfig != null)
            {
                gameplayUI.ResetDisplay(levelIndex, gameConfig.PlayerMaxHP, levelData.TreasureRequirements, levelData.RequiredTreasures);
            }

            // Publish initial score reset
            EventBus.Publish(new ScoreChangedEvent { TotalScore = 0 });

            _audioManager.PlayMusic("GameplayBGM", true);
        }

        private void HandleWin(int score, int stars)
        {
            Time.timeScale = 0f;

            _gameStateManager.ChangeState(GameState.Win);
            _currentState = GameState.Win;

            // Set data BEFORE showing panel (panel displays data in Show())
            var winPanel = _uiManager.GetPanel<UI.WinPanel>("WinPanel");
            if (winPanel != null)
            {
                float timeElapsed = _levelManager.GetLevelElapsedTime();
                winPanel.SetData(_currentLevelIndex, score, stars, timeElapsed);
            }

            _uiManager.ShowPanel("WinPanel");
            _audioManager.PlaySFX("LevelComplete");

            PlayerProgressData.SaveLevelProgress(_currentLevelIndex, score, stars);
        }

        private void HandleFail()
        {
            Time.timeScale = 0f;

            _gameStateManager.ChangeState(GameState.Fail);
            _currentState = GameState.Fail;

            // Set data BEFORE showing panel
            var failPanel = _uiManager.GetPanel<UI.FailPanel>("FailPanel");
            if (failPanel != null)
                failPanel.SetData(_currentLevelIndex, "Player died");

            _uiManager.ShowPanel("FailPanel");
            _audioManager.PlaySFX("LevelFail");
        }

        #endregion
    }
}

using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;

namespace GraveyardHunter.UI
{
    /// <summary>
    /// GameplayUI extends UIPanel. Overrides Show/Hide to use CanvasGroup only (never SetActive).
    /// Subscribes gameplay events in Show(), unsubscribes in Hide().
    /// </summary>
    public class GameplayUI : UIPanel
    {
        [BoxGroup("Buttons")]
        [SerializeField] private Button _pauseButton;
        [BoxGroup("Buttons")]
        [SerializeField] private Button _resetButton;

        [BoxGroup("Info")]
        [SerializeField] private TextMeshProUGUI _levelText;
        [BoxGroup("Info")]
        [SerializeField] private TextMeshProUGUI _hpText;
        [BoxGroup("Info")]
        [SerializeField] private TextMeshProUGUI _treasureText;
        [BoxGroup("Info")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [BoxGroup("Info")]
        [SerializeField] private Image[] _hpIcons;

        [BoxGroup("Escape")]
        [SerializeField] private GameObject _escapeIndicator;

        [BoxGroup("Booster")]
        [SerializeField] private GameObject _boosterTimerUI;
        [BoxGroup("Booster")]
        [SerializeField] private Image _boosterTimerFill;
        [BoxGroup("Booster")]
        [SerializeField] private TextMeshProUGUI _boosterNameText;

        private bool _eventsSubscribed;

        protected override void Init()
        {
            base.Init();

            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPauseClicked);
            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);
        }

        // === CanvasGroup-based Show/Hide (never SetActive) ===

        public override void Show()
        {
            // Ensure GameObject is active (in case it started disabled in scene)
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (!_initialized) Init();

            DOTween.Kill(this);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            SubscribeEvents();
        }

        public override void Hide()
        {
            DOTween.Kill(this);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            UnsubscribeEvents();
        }

        public void ForceHide()
        {
            // Ensure GO is active so CanvasGroup works, then hide via alpha
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (!_initialized) Init();

            DOTween.Kill(this);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            UnsubscribeEvents();
        }

        // === Subscribe / Unsubscribe ===

        private void SubscribeEvents()
        {
            if (_eventsSubscribed) return;
            _eventsSubscribed = true;

            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Subscribe<PlayerHPChangedEvent>(OnPlayerHPChanged);
            EventBus.Subscribe<TreasureCollectedEvent>(OnTreasureCollected);
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<BoosterActivatedEvent>(OnBoosterActivated);
            EventBus.Subscribe<BoosterExpiredEvent>(OnBoosterExpired);
            EventBus.Subscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);
        }

        private void UnsubscribeEvents()
        {
            if (!_eventsSubscribed) return;
            _eventsSubscribed = false;

            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Unsubscribe<PlayerHPChangedEvent>(OnPlayerHPChanged);
            EventBus.Unsubscribe<TreasureCollectedEvent>(OnTreasureCollected);
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<BoosterActivatedEvent>(OnBoosterActivated);
            EventBus.Unsubscribe<BoosterExpiredEvent>(OnBoosterExpired);
            EventBus.Unsubscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        // === Event Handlers ===

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.NewState)
            {
                case Core.GameState.MainMenu:
                case Core.GameState.Win:
                case Core.GameState.Fail:
                    Hide();
                    break;

                case Core.GameState.Playing:
                case Core.GameState.Loading:
                    Show();
                    if (_pauseButton != null) _pauseButton.interactable = true;
                    if (_resetButton != null) _resetButton.interactable = true;
                    if (_escapeIndicator != null) _escapeIndicator.SetActive(false);
                    break;

                case Core.GameState.EscapePhase:
                    Show();
                    if (_escapeIndicator != null) _escapeIndicator.SetActive(true);
                    break;

                case Core.GameState.Paused:
                    if (_pauseButton != null) _pauseButton.interactable = false;
                    if (_resetButton != null) _resetButton.interactable = false;
                    break;
            }
        }

        private void OnPlayerHPChanged(PlayerHPChangedEvent evt)
        {
            if (_hpText != null)
                _hpText.text = $"HP: {evt.CurrentHP}/{evt.MaxHP}";

            if (_hpIcons != null)
            {
                for (int i = 0; i < _hpIcons.Length; i++)
                {
                    if (_hpIcons[i] != null)
                        _hpIcons[i].enabled = i < evt.CurrentHP;
                }
            }
        }

        private void OnTreasureCollected(TreasureCollectedEvent evt)
        {
            if (_treasureText != null)
            {
                _treasureText.text = $"Treasure: {evt.CurrentCount}/{evt.RequiredCount}";
                _treasureText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f).SetUpdate(true);
            }
        }

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {evt.TotalScore}";
        }

        private void OnBoosterActivated(BoosterActivatedEvent evt)
        {
            if (_boosterTimerUI != null) _boosterTimerUI.SetActive(true);
            if (_boosterNameText != null) _boosterNameText.text = evt.Type.ToString();
            if (_boosterTimerFill != null) _boosterTimerFill.fillAmount = 1f;
        }

        private void OnBoosterExpired(BoosterExpiredEvent evt)
        {
            if (_boosterTimerUI != null) _boosterTimerUI.SetActive(false);
        }

        private void OnEscapePhaseStarted(EscapePhaseStartedEvent evt)
        {
            if (_escapeIndicator != null)
            {
                _escapeIndicator.SetActive(true);
                _escapeIndicator.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, 5, 0.5f).SetUpdate(true);
            }
        }

        private void OnPauseClicked()
        {
            EventBus.Publish(new PauseEvent());
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
        }

        private void OnResetClicked()
        {
            EventBus.Publish(new ResetLevelEvent());
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
        }
    }
}

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;
using GraveyardHunter.Data;

namespace GraveyardHunter.UI
{
    /// <summary>
    /// Shows contextual tutorial steps during Level 1.
    /// Each step: dark overlay + message + "OK" button. Game pauses while showing.
    /// Triggers are event-based (level start, first light hit, treasure near, etc.).
    /// Completes once. Saved to PlayerPrefs.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Image _overlay;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _stepCounterText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _skipButton;

        [Header("Settings")]
        [SerializeField] private float _overlayAlpha = 0.7f;

        private readonly List<TutorialStep> _steps = new();
        private int _currentStepIndex;
        private bool _isShowing;
        private bool _tutorialCompleted;
        private HashSet<string> _shownTriggers = new();

        private void Awake()
        {
            ServiceLocator.Register(this);

            if (_okButton != null)
                _okButton.onClick.AddListener(OnOKClicked);
            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkipClicked);

            if (_panel != null)
                _panel.SetActive(false);

            _tutorialCompleted = PlayerProgressData.IsTutorialCompleted();

            // Define all tutorial steps
            BuildSteps();

            // Subscribe to game events for contextual triggers
            EventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Subscribe<PlayerInLightEvent>(OnPlayerInLight);
            EventBus.Subscribe<TreasureCollectedEvent>(OnTreasureCollected);
            EventBus.Subscribe<AllTreasuresCollectedEvent>(OnAllTreasuresCollected);
            EventBus.Subscribe<PlayerShelterEvent>(OnPlayerShelter);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TutorialUI>();
            EventBus.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Unsubscribe<PlayerInLightEvent>(OnPlayerInLight);
            EventBus.Unsubscribe<TreasureCollectedEvent>(OnTreasureCollected);
            EventBus.Unsubscribe<AllTreasuresCollectedEvent>(OnAllTreasuresCollected);
            EventBus.Unsubscribe<PlayerShelterEvent>(OnPlayerShelter);
        }

        // ═══════════════════════════════════════════
        //  STEP DEFINITIONS
        // ═══════════════════════════════════════════

        private void BuildSteps()
        {
            _steps.Clear();

            _steps.Add(new TutorialStep
            {
                Trigger = "level_start",
                Message = "Welcome, Graveyard Hunter!\n\nUse the <b>joystick</b> to move through the maze.\nStay in the shadows to avoid the ghosts."
            });

            _steps.Add(new TutorialStep
            {
                Trigger = "level_start_2",
                Message = "Your mission:\n\n1. <b>Collect</b> all required treasures\n2. <b>Avoid</b> ghost light cones\n3. <b>Escape</b> through the exit gate"
            });

            _steps.Add(new TutorialStep
            {
                Trigger = "first_light_hit",
                Message = "<color=red>Warning!</color> Ghost light is hitting you!\n\nYou lose <b>1 HP per second</b> and move <b>20% slower</b> in the light.\nRun away quickly!"
            });

            _steps.Add(new TutorialStep
            {
                Trigger = "first_treasure",
                Message = "You collected a treasure!\n\nCollect <b>all required types</b> shown at the top.\nEach type has its own counter."
            });

            _steps.Add(new TutorialStep
            {
                Trigger = "first_shelter",
                Message = "You found a <color=green>Shelter</color>!\n\nHide inside to become <b>invisible</b> to ghosts.\nThey will stop chasing you."
            });

            _steps.Add(new TutorialStep
            {
                Trigger = "all_treasures",
                Message = "<color=yellow>All treasures collected!</color>\n\nThe exit gate is now <b>open</b>!\nRun to the gate to escape. Hurry — ghosts are <b>faster</b> now!"
            });
        }

        // ═══════════════════════════════════════════
        //  EVENT TRIGGERS
        // ═══════════════════════════════════════════

        private void OnLevelLoaded(LevelLoadedEvent evt)
        {
            if (_tutorialCompleted) return;
            if (evt.LevelIndex != 0) return; // Only Level 1

            _shownTriggers.Clear();

            // Show first 2 steps with slight delay
            DOVirtual.DelayedCall(0.5f, () => TryShowStep("level_start")).SetUpdate(true);
        }

        private void OnPlayerInLight(PlayerInLightEvent evt)
        {
            if (_tutorialCompleted || !evt.InLight) return;
            TryShowStep("first_light_hit");
        }

        private void OnTreasureCollected(TreasureCollectedEvent evt)
        {
            if (_tutorialCompleted) return;
            if (evt.CurrentCount == 1) // First treasure ever
                TryShowStep("first_treasure");
        }

        private void OnPlayerShelter(PlayerShelterEvent evt)
        {
            if (_tutorialCompleted || !evt.IsInShelter) return;
            TryShowStep("first_shelter");
        }

        private void OnAllTreasuresCollected(AllTreasuresCollectedEvent evt)
        {
            if (_tutorialCompleted) return;
            TryShowStep("all_treasures");
        }

        // ═══════════════════════════════════════════
        //  SHOW / HIDE
        // ═══════════════════════════════════════════

        private void TryShowStep(string trigger)
        {
            if (_isShowing) return;
            if (_shownTriggers.Contains(trigger)) return;

            // Find step with this trigger
            TutorialStep step = null;
            int stepIdx = -1;
            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].Trigger == trigger)
                {
                    step = _steps[i];
                    stepIdx = i;
                    break;
                }
            }

            if (step == null) return;

            _shownTriggers.Add(trigger);
            ShowStep(step, stepIdx);
        }

        private void ShowStep(TutorialStep step, int stepIndex)
        {
            _isShowing = true;
            _currentStepIndex = stepIndex;

            Time.timeScale = 0f;

            if (_panel != null) _panel.SetActive(true);

            if (_overlay != null)
            {
                _overlay.color = new Color(0f, 0f, 0f, 0f);
                _overlay.DOFade(_overlayAlpha, 0.3f).SetUpdate(true);
            }

            if (_messageText != null)
            {
                _messageText.text = step.Message;
                _messageText.transform.localScale = Vector3.one * 0.8f;
                _messageText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (_stepCounterText != null)
            {
                int shown = _shownTriggers.Count;
                _stepCounterText.text = $"{shown} / {_steps.Count}";
            }
        }

        private void HideStep()
        {
            _isShowing = false;
            Time.timeScale = 1f;

            if (_panel != null)
                _panel.SetActive(false);

            // If this was "level_start", show "level_start_2" right after
            if (_currentStepIndex == 0)
            {
                DOVirtual.DelayedCall(0.3f, () => TryShowStep("level_start_2")).SetUpdate(true);
            }
        }

        // ═══════════════════════════════════════════
        //  BUTTON CALLBACKS
        // ═══════════════════════════════════════════

        private void OnOKClicked()
        {
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            HideStep();
        }

        private void OnSkipClicked()
        {
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _tutorialCompleted = true;
            _isShowing = false;
            Time.timeScale = 1f;

            if (_panel != null)
                _panel.SetActive(false);

            PlayerProgressData.SetTutorialCompleted(true);
        }

        // ═══════════════════════════════════════════
        //  DATA
        // ═══════════════════════════════════════════

        private class TutorialStep
        {
            public string Trigger;
            public string Message;
        }
    }
}

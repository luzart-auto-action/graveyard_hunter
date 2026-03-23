using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;

namespace GraveyardHunter.UI
{
    public class WinPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image[] _stars;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Color _starActiveColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color _starInactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private int _score;
        private float _timeElapsed;
        private int _levelIndex;
        private int _starCount;

        protected override void Init()
        {
            base.Init();

            // Buttons do NOT call Hide() - state transition handles that
            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNextClicked);
            if (_retryButton != null)
                _retryButton.onClick.AddListener(OnRetryClicked);
            if (_homeButton != null)
                _homeButton.onClick.AddListener(OnHomeClicked);
        }

        /// <summary>
        /// GameManager calls this BEFORE ShowPanel to pass result data.
        /// </summary>
        public void SetData(int levelIndex, int score, int stars, float timeElapsed)
        {
            _levelIndex = levelIndex;
            _score = score;
            _starCount = stars;
            _timeElapsed = timeElapsed;
        }

        public override void Show()
        {
            if (!_initialized) Init();

            // Display cached data
            if (_scoreText != null) _scoreText.text = $"Score: {_score}";
            if (_timeText != null) _timeText.text = $"Time: {_timeElapsed:F1}s";
            if (_levelText != null) _levelText.text = $"Level {_levelIndex + 1} Complete!";

            // Star animations
            if (_stars != null)
            {
                for (int i = 0; i < _stars.Length; i++)
                {
                    if (_stars[i] == null) continue;

                    bool active = i < _starCount;
                    _stars[i].color = active ? _starActiveColor : _starInactiveColor;
                    _stars[i].transform.localScale = Vector3.zero;

                    if (active)
                    {
                        _stars[i].transform.DOScale(1f, 0.4f)
                            .SetDelay(0.3f * i)
                            .SetEase(Ease.OutBack)
                            .SetUpdate(true);
                        _stars[i].transform.DOPunchRotation(new Vector3(0f, 0f, 30f), 0.4f, 5, 0.5f)
                            .SetDelay(0.3f * i + 0.2f)
                            .SetUpdate(true);
                    }
                    else
                    {
                        _stars[i].transform.DOScale(1f, 0.3f)
                            .SetDelay(0.3f * i)
                            .SetUpdate(true);
                    }
                }
            }

            base.Show();
        }

        private void OnNextClicked()
        {
            if (_nextButton != null)
                _nextButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            EventBus.Publish(new NextLevelEvent());
        }

        private void OnRetryClicked()
        {
            if (_retryButton != null)
                _retryButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            EventBus.Publish(new RetryEvent());
        }

        private void OnHomeClicked()
        {
            if (_homeButton != null)
                _homeButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            EventBus.Publish(new GoHomeEvent());
        }
    }
}

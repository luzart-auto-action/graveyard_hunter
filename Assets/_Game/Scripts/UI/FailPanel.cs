using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;

namespace GraveyardHunter.UI
{
    public class FailPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI _failReasonText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;

        private string _reason;
        private int _levelIndex;

        protected override void Init()
        {
            base.Init();

            // Buttons do NOT call Hide() - state transition handles that
            if (_retryButton != null)
                _retryButton.onClick.AddListener(OnRetryClicked);
            if (_homeButton != null)
                _homeButton.onClick.AddListener(OnHomeClicked);
        }

        /// <summary>
        /// GameManager calls this BEFORE ShowPanel to pass fail data.
        /// </summary>
        public void SetData(int levelIndex, string reason)
        {
            _levelIndex = levelIndex;
            _reason = reason;
        }

        public override void Show()
        {
            if (!_initialized) Init();

            if (_failReasonText != null) _failReasonText.text = _reason;
            if (_levelText != null) _levelText.text = $"Level {_levelIndex + 1}";

            base.Show();

            // Shake effect
            if (_failReasonText != null)
            {
                _failReasonText.transform.DOShakePosition(0.5f, 10f, 20, 90f, false, true)
                    .SetUpdate(true);
            }
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

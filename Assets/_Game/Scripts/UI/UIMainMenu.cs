using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;
using GraveyardHunter.Data;

namespace GraveyardHunter.UI
{
    public class UIMainMenu : UIPanel
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _totalScoreText;

        protected override void Init()
        {
            base.Init();

            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_shopButton != null)
                _shopButton.onClick.AddListener(OnShopClicked);
        }

        public override void Show()
        {
            if (!_initialized) Init();

            if (_levelText != null)
                _levelText.text = $"Level {PlayerProgressData.GetCurrentLevel() + 1}";
            if (_totalScoreText != null)
                _totalScoreText.text = $"Score: {PlayerProgressData.GetTotalScore()}";

            base.Show();
        }

        private void OnPlayClicked()
        {
            if (_playButton != null)
                _playButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            EventBus.Publish(new PlayButtonEvent());
        }

        private void OnSettingsClicked()
        {
            if (_settingsButton != null)
                _settingsButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            ServiceLocator.Get<UIManager>().ShowPanel("PopupSettings");
        }

        private void OnShopClicked()
        {
            if (_shopButton != null)
                _shopButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            ServiceLocator.Get<UIManager>().ShowPanel("ShopPanel");
        }
    }
}

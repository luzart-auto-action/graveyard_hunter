using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using GraveyardHunter.Core;

namespace GraveyardHunter.UI
{
    public class PopupPause : UIPanel
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _settingsButton;

        protected override void Init()
        {
            base.Init();

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumeClicked);
            if (_homeButton != null)
                _homeButton.onClick.AddListener(OnHomeClicked);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnResumeClicked()
        {
            if (_resumeButton != null)
                _resumeButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            EventBus.Publish(new ResumeEvent());
        }

        private void OnHomeClicked()
        {
            if (_homeButton != null)
                _homeButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            EventBus.Publish(new GoHomeEvent());
        }

        private void OnSettingsClicked()
        {
            if (_settingsButton != null)
                _settingsButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            ServiceLocator.Get<UIManager>().ShowPanel("PopupSettings");
        }
    }
}

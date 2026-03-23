using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;
using GraveyardHunter.Data;

namespace GraveyardHunter.UI
{
    public class PopupSettings : UIPanel
    {
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Button _closeButton;

        protected override void Init()
        {
            base.Init();

            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
            if (_sfxSlider != null)
                _sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            if (_musicSlider != null)
                _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        public override void Show()
        {
            if (!_initialized) Init();

            // Refresh slider values from saved data
            if (_sfxSlider != null) _sfxSlider.value = PlayerProgressData.GetSFXVolume();
            if (_musicSlider != null) _musicSlider.value = PlayerProgressData.GetMusicVolume();

            base.Show();
        }

        private void OnSFXChanged(float val)
        {
            PlayerProgressData.SetSFXVolume(val);
            if (ServiceLocator.TryGet<Audio.AudioManager>(out var audio))
                audio.SetSFXVolume(val);
        }

        private void OnMusicChanged(float val)
        {
            PlayerProgressData.SetMusicVolume(val);
            if (ServiceLocator.TryGet<Audio.AudioManager>(out var audio))
                audio.SetMusicVolume(val);
        }

        private void OnCloseClicked()
        {
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            Hide();
        }
    }
}

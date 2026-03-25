using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;
using GraveyardHunter.Data;

namespace GraveyardHunter.UI
{
    public class PopupSettings : UIPanel
    {
        [SerializeField] private Button _musicToggleButton;
        [SerializeField] private Image _musicToggleBG;
        [SerializeField] private TextMeshProUGUI _musicToggleText;

        [SerializeField] private Button _sfxToggleButton;
        [SerializeField] private Image _sfxToggleBG;
        [SerializeField] private TextMeshProUGUI _sfxToggleText;

        [SerializeField] private Button _closeButton;

        private bool _musicOn;
        private bool _sfxOn;

        private static readonly Color ColorOn = new Color(0.2f, 0.75f, 0.3f, 1f);
        private static readonly Color ColorOff = new Color(0.45f, 0.4f, 0.4f, 1f);

        protected override void Init()
        {
            base.Init();

            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
            if (_musicToggleButton != null)
                _musicToggleButton.onClick.AddListener(OnMusicToggle);
            if (_sfxToggleButton != null)
                _sfxToggleButton.onClick.AddListener(OnSFXToggle);
        }

        public override void Show()
        {
            if (!_initialized) Init();

            _musicOn = PlayerProgressData.GetMusicVolume() > 0.5f;
            _sfxOn = PlayerProgressData.GetSFXVolume() > 0.5f;

            RefreshVisuals();
            base.Show();
        }

        private void OnMusicToggle()
        {
            _musicOn = !_musicOn;
            float vol = _musicOn ? 1f : 0f;

            PlayerProgressData.SetMusicVolume(vol);
            if (ServiceLocator.TryGet<Audio.AudioManager>(out var audio))
                audio.SetMusicVolume(vol);

            RefreshVisuals();
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
        }

        private void OnSFXToggle()
        {
            _sfxOn = !_sfxOn;
            float vol = _sfxOn ? 1f : 0f;

            PlayerProgressData.SetSFXVolume(vol);
            if (ServiceLocator.TryGet<Audio.AudioManager>(out var audio))
                audio.SetSFXVolume(vol);

            RefreshVisuals();
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
        }

        private void RefreshVisuals()
        {
            if (_musicToggleBG != null)
                _musicToggleBG.color = _musicOn ? ColorOn : ColorOff;
            if (_musicToggleText != null)
                _musicToggleText.text = _musicOn ? "ON" : "OFF";

            if (_sfxToggleBG != null)
                _sfxToggleBG.color = _sfxOn ? ColorOn : ColorOff;
            if (_sfxToggleText != null)
                _sfxToggleText.text = _sfxOn ? "ON" : "OFF";
        }

        private void OnCloseClicked()
        {
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            Hide();
        }
    }
}

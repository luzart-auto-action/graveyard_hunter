using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GraveyardHunter.Core;
using GraveyardHunter.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [System.Serializable]
        public class AudioEntry
        {
            public string Name;
            public AudioClip Clip;
        }

        [BoxGroup("SFX")]
        [TableList]
        [SerializeField] private List<AudioEntry> _sfxLibrary;

        [BoxGroup("Music")]
        [TableList]
        [SerializeField] private List<AudioEntry> _musicLibrary;

        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private int _sfxPoolSize = 10;

        private List<AudioSource> _sfxPool;
        private float _sfxVolume;
        private float _musicVolume;

        private void Awake()
        {
            ServiceLocator.Register(this);

            _sfxPool = new List<AudioSource>(_sfxPoolSize);
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxPool.Add(source);
            }

            _sfxVolume = PlayerProgressData.GetSFXVolume();
            _musicVolume = PlayerProgressData.GetMusicVolume();

            foreach (var source in _sfxPool)
                source.volume = _sfxVolume;

            if (_musicSource != null)
                _musicSource.volume = _musicVolume;

            EventBus.Subscribe<PlaySFXEvent>(OnPlaySFX);
            EventBus.Subscribe<PlayMusicEvent>(OnPlayMusic);
            EventBus.Subscribe<StopMusicEvent>(OnStopMusic);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<AudioManager>();
            EventBus.Unsubscribe<PlaySFXEvent>(OnPlaySFX);
            EventBus.Unsubscribe<PlayMusicEvent>(OnPlayMusic);
            EventBus.Unsubscribe<StopMusicEvent>(OnStopMusic);
        }

        public void PlaySFX(string name)
        {
            var entry = _sfxLibrary.FirstOrDefault(e => e.Name == name);
            if (entry == null || entry.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX not found: {name}");
                return;
            }

            var source = _sfxPool.FirstOrDefault(s => !s.isPlaying);
            if (source == null)
            {
                Debug.LogWarning("[AudioManager] No available SFX AudioSource in pool.");
                return;
            }

            source.clip = entry.Clip;
            source.volume = _sfxVolume;
            source.Play();
        }

        public void PlayMusic(string name, bool loop = true)
        {
            var entry = _musicLibrary.FirstOrDefault(e => e.Name == name);
            if (entry == null || entry.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] Music not found: {name}");
                return;
            }

            // Kill any running fade tween before switching tracks
            _musicSource.DOKill();

            _musicSource.clip = entry.Clip;
            _musicSource.loop = loop;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.DOFade(0f, 0.5f).OnComplete(() => _musicSource.Stop());
        }

        public void SetSFXVolume(float vol)
        {
            _sfxVolume = vol;
            foreach (var source in _sfxPool)
                source.volume = _sfxVolume;
        }

        public void SetMusicVolume(float vol)
        {
            _musicVolume = vol;
            _musicSource.volume = _musicVolume;
        }

        private void OnPlaySFX(PlaySFXEvent evt)
        {
            PlaySFX(evt.SFXName);
        }

        private void OnPlayMusic(PlayMusicEvent evt)
        {
            PlayMusic(evt.MusicName, evt.Loop);
        }

        private void OnStopMusic(StopMusicEvent evt)
        {
            StopMusic();
        }
    }
}

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
    /// Level-map main menu. Spawns LevelNode items from a prefab at runtime.
    /// Vertical scroll, landscape friendly.
    /// Shows currentLevel + 10 extra levels ahead.
    /// </summary>
    public class UIMainMenu : UIPanel
    {
        [Header("Level Map")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _levelNodePrefab;
        [SerializeField] private Image _pathLine;

        [Header("Spawn Settings")]
        [SerializeField] private float _nodeSpacing = 180f;
        [SerializeField] private float _zigzagAmplitude = 80f;
        [SerializeField] private int _extraLevels = 10;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _shopButton;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _totalScoreText;

        private int _selectedLevel;
        private readonly List<LevelNodeUI> _spawnedNodes = new();

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

            _selectedLevel = PlayerProgressData.GetCurrentLevel();

            if (_totalScoreText != null)
                _totalScoreText.text = $"Score: {PlayerProgressData.GetTotalScore()}";

            SpawnNodes();
            ScrollToCurrentLevel();

            base.Show();
        }

        public override void Hide()
        {
            ClearNodes();
            base.Hide();
        }

        // ═══════════════════════════════════════════
        //  SPAWN
        // ═══════════════════════════════════════════

        private void SpawnNodes()
        {
            ClearNodes();

            if (_levelNodePrefab == null || _content == null) return;

            int currentLevel = PlayerProgressData.GetCurrentLevel();
            int totalNodes = currentLevel + 1 + _extraLevels;

            // Resize content height (vertical scroll, top-to-bottom)
            float totalHeight = totalNodes * _nodeSpacing + 200f;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalHeight);

            // Stretch path line
            if (_pathLine != null)
            {
                var pathRect = _pathLine.rectTransform;
                pathRect.offsetMin = new Vector2(pathRect.offsetMin.x, 80f);
                pathRect.offsetMax = new Vector2(pathRect.offsetMax.x, -80f);
            }

            for (int i = 0; i < totalNodes; i++)
            {
                var go = Instantiate(_levelNodePrefab, _content);
                go.name = $"LevelNode_{i}";

                // Position: top-down, zigzag X
                var rect = go.GetComponent<RectTransform>();
                float yPos = -(120f + i * _nodeSpacing);
                float xOffset = Mathf.Sin(i * 0.9f) * _zigzagAmplitude;
                rect.anchoredPosition = new Vector2(xOffset, yPos);

                var node = go.GetComponent<LevelNodeUI>();
                if (node == null) continue;

                node.SetLevelIndex(i);

                bool unlocked = i <= currentLevel;
                bool isCurrent = i == currentLevel;
                int stars = PlayerProgressData.GetStars(i);

                node.SetState(unlocked, isCurrent, stars);
                node.SetClickCallback(OnLevelNodeClicked);

                _spawnedNodes.Add(node);
            }
        }

        private void ClearNodes()
        {
            foreach (var node in _spawnedNodes)
            {
                if (node != null)
                {
                    node.transform.DOKill();
                    Destroy(node.gameObject);
                }
            }
            _spawnedNodes.Clear();
        }

        // ═══════════════════════════════════════════
        //  SCROLL
        // ═══════════════════════════════════════════

        private void ScrollToCurrentLevel()
        {
            if (_scrollRect == null || _content == null) return;

            DOVirtual.DelayedCall(0.05f, () =>
            {
                int currentLevel = PlayerProgressData.GetCurrentLevel();
                if (currentLevel >= _spawnedNodes.Count) return;

                var nodeRect = _spawnedNodes[currentLevel].GetComponent<RectTransform>();
                if (nodeRect == null) return;

                float contentH = _content.rect.height;
                float viewportH = _scrollRect.viewport != null
                    ? _scrollRect.viewport.rect.height
                    : _scrollRect.GetComponent<RectTransform>().rect.height;

                if (contentH <= viewportH) return;

                float nodeY = -nodeRect.anchoredPosition.y;
                float norm = Mathf.Clamp01((nodeY - viewportH * 0.5f) / (contentH - viewportH));
                _scrollRect.verticalNormalizedPosition = 1f - norm;
            }).SetUpdate(true);
        }

        // ═══════════════════════════════════════════
        //  CALLBACKS
        // ═══════════════════════════════════════════

        private void OnLevelNodeClicked(int levelIndex)
        {
            if (levelIndex > PlayerProgressData.GetCurrentLevel()) return;
            _selectedLevel = levelIndex;
        }

        private void OnPlayClicked()
        {
            if (_playButton != null)
                _playButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetUpdate(true);
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));

            PlayerProgressData.SetCurrentLevel(_selectedLevel);
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

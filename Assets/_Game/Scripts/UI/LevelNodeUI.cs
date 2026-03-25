using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace GraveyardHunter.UI
{
    /// <summary>
    /// Individual level node on the main menu level map.
    /// Uses sprite swap for BG states — drag your images into Inspector.
    /// </summary>
    public class LevelNodeUI : MonoBehaviour
    {
        [SerializeField] private int _levelIndex;
        [SerializeField] private Button _button;
        [SerializeField] private Image _bgImage;
        [SerializeField] private Image _ringImage;
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private Image[] _starImages;
        [SerializeField] private GameObject _lockIcon;

        [Header("BG Sprites (drag your images here)")]
        [SerializeField] private Sprite _spriteCurrent;
        [SerializeField] private Sprite _spriteUnlocked;
        [SerializeField] private Sprite _spriteLocked;
        [SerializeField] private Sprite _spriteSelected;

        [Header("Star Sprites")]
        [SerializeField] private Sprite _starOnSprite;
        [SerializeField] private Sprite _starOffSprite;

        public int LevelIndex => _levelIndex;

        public void SetLevelIndex(int index)
        {
            _levelIndex = index;
        }

        private System.Action<int> _clickCallback;
        private bool _unlocked;
        private bool _isCurrent;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        public void SetState(bool unlocked, bool isCurrent, int stars)
        {
            _unlocked = unlocked;
            _isCurrent = isCurrent;

            // Level number
            if (_levelNumberText != null)
            {
                _levelNumberText.text = (_levelIndex + 1).ToString();
                _levelNumberText.gameObject.SetActive(unlocked);
            }

            // Lock icon
            if (_lockIcon != null)
                _lockIcon.SetActive(!unlocked);

            // BG: swap sprite, fallback to color if sprite is null
            if (_bgImage != null)
            {
                if (isCurrent && _spriteCurrent != null)
                    _bgImage.sprite = _spriteCurrent;
                else if (unlocked && _spriteUnlocked != null)
                    _bgImage.sprite = _spriteUnlocked;
                else if (!unlocked && _spriteLocked != null)
                    _bgImage.sprite = _spriteLocked;

                // Keep white tint so sprite shows original colors
                _bgImage.color = Color.white;
            }

            // Ring
            if (_ringImage != null)
                _ringImage.color = isCurrent ? new Color(1f, 0.85f, 0.3f) : Color.white;

            // Stars: swap sprite if available, fallback to color
            if (_starImages != null)
            {
                for (int i = 0; i < _starImages.Length; i++)
                {
                    if (_starImages[i] == null) continue;

                    bool earned = unlocked && i < stars;

                    if (earned && _starOnSprite != null)
                        _starImages[i].sprite = _starOnSprite;
                    else if (!earned && _starOffSprite != null)
                        _starImages[i].sprite = _starOffSprite;

                    _starImages[i].color = earned ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                    _starImages[i].gameObject.SetActive(unlocked);
                }
            }

            // Button
            if (_button != null)
                _button.interactable = unlocked;

            // Animate current level
            if (isCurrent)
            {
                transform.DOKill();
                transform.DOScale(1.15f, 0.6f).SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
            }
            else
            {
                transform.DOKill();
                transform.localScale = Vector3.one;
            }
        }

        public void SetSelected(bool selected)
        {
            if (_bgImage == null) return;

            if (selected && _unlocked)
            {
                if (_spriteSelected != null)
                    _bgImage.sprite = _spriteSelected;
            }
            else
            {
                // Restore to current/unlocked sprite
                if (_isCurrent && _spriteCurrent != null)
                    _bgImage.sprite = _spriteCurrent;
                else if (_unlocked && _spriteUnlocked != null)
                    _bgImage.sprite = _spriteUnlocked;
            }

            _bgImage.color = Color.white;
        }

        public void SetClickCallback(System.Action<int> callback)
        {
            _clickCallback = callback;
        }

        private void OnClicked()
        {
            if (!_unlocked) return;
            transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 5, 0.5f).SetUpdate(true);
            _clickCallback?.Invoke(_levelIndex);
        }
    }
}

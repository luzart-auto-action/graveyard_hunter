using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace GraveyardHunter.UI
{
    /// <summary>
    /// Level node on the main menu map.
    /// 3 states: Locked / Unlocked / Current.
    /// Drag sprites into Inspector for each state.
    /// </summary>
    public class LevelNodeUI : MonoBehaviour
    {
        [SerializeField] private int _levelIndex;
        [SerializeField] private Button _button;
        [SerializeField] private Image _bgImage;
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private Image[] _starImages;
        [SerializeField] private GameObject _lockIcon;

        [Header("BG Sprites")]
        [SerializeField] private Sprite _spriteCurrent;
        [SerializeField] private Sprite _spriteUnlocked;
        [SerializeField] private Sprite _spriteLocked;

        [Header("Star Sprites")]
        [SerializeField] private Sprite _starOnSprite;
        [SerializeField] private Sprite _starOffSprite;

        public int LevelIndex => _levelIndex;

        public void SetLevelIndex(int index) => _levelIndex = index;

        private System.Action<int> _clickCallback;
        private bool _unlocked;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        public void SetState(bool unlocked, bool isCurrent, int stars)
        {
            _unlocked = unlocked;

            // Level number
            if (_levelNumberText != null)
            {
                _levelNumberText.text = (_levelIndex + 1).ToString();
                _levelNumberText.gameObject.SetActive(unlocked);
            }

            // Lock icon
            if (_lockIcon != null)
                _lockIcon.SetActive(!unlocked);

            // BG sprite
            if (_bgImage != null)
            {
                if (isCurrent && _spriteCurrent != null)
                    _bgImage.sprite = _spriteCurrent;
                else if (unlocked && _spriteUnlocked != null)
                    _bgImage.sprite = _spriteUnlocked;
                else if (!unlocked && _spriteLocked != null)
                    _bgImage.sprite = _spriteLocked;

                _bgImage.color = Color.white;
            }

            // Stars
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

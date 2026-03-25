using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace GraveyardHunter.UI
{
    /// <summary>
    /// Individual level node on the main menu level map.
    /// Shows level number, lock state, stars, and current/selected highlight.
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

        public int LevelIndex => _levelIndex;

        /// <summary>Set at runtime when spawned from prefab.</summary>
        public void SetLevelIndex(int index)
        {
            _levelIndex = index;
        }

        private System.Action<int> _clickCallback;
        private bool _unlocked;

        private static readonly Color ColorCurrent = new Color(0.2f, 0.78f, 0.35f, 1f);   // green
        private static readonly Color ColorUnlocked = new Color(0.3f, 0.55f, 0.85f, 1f);   // blue
        private static readonly Color ColorLocked = new Color(0.4f, 0.4f, 0.45f, 1f);      // gray
        private static readonly Color ColorSelected = new Color(0.95f, 0.75f, 0.15f, 1f);   // gold
        private static readonly Color RingWhite = new Color(1f, 1f, 1f, 0.8f);
        private static readonly Color RingGold = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color StarOn = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color StarOff = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        public void SetState(bool unlocked, bool isCurrent, int stars)
        {
            _unlocked = unlocked;

            // Number text
            if (_levelNumberText != null)
            {
                _levelNumberText.text = (_levelIndex + 1).ToString();
                _levelNumberText.gameObject.SetActive(unlocked);
            }

            // Lock icon
            if (_lockIcon != null)
                _lockIcon.SetActive(!unlocked);

            // Background color
            if (_bgImage != null)
            {
                if (isCurrent)
                    _bgImage.color = ColorCurrent;
                else if (unlocked)
                    _bgImage.color = ColorUnlocked;
                else
                    _bgImage.color = ColorLocked;
            }

            // Ring
            if (_ringImage != null)
                _ringImage.color = isCurrent ? RingGold : RingWhite;

            // Stars
            if (_starImages != null)
            {
                for (int i = 0; i < _starImages.Length; i++)
                {
                    if (_starImages[i] != null)
                    {
                        _starImages[i].color = (unlocked && i < stars) ? StarOn : StarOff;
                        _starImages[i].gameObject.SetActive(unlocked);
                    }
                }
            }

            // Button interactable
            if (_button != null)
                _button.interactable = unlocked;

            // Animate current level
            if (isCurrent)
            {
                transform.DOKill();
                transform.DOScale(1.15f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
            }
            else
            {
                transform.DOKill();
                transform.localScale = Vector3.one;
            }
        }

        public void SetSelected(bool selected)
        {
            if (_ringImage != null)
                _ringImage.color = selected ? RingGold : RingWhite;

            if (selected && _bgImage != null && _unlocked)
                _bgImage.color = ColorSelected;
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

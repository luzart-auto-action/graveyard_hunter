using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.UI
{
    /// <summary>
    /// Base class for all UI panels.
    /// IMPORTANT: No Awake/Start/OnDestroy for events or button listeners.
    /// All setup happens in Show(), all cleanup in Hide().
    /// This ensures panels work even if they start with SetActive(false).
    /// </summary>
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] protected string _panelName;
        protected CanvasGroup _canvasGroup;
        protected bool _initialized;

        public string PanelName => _panelName;

        /// <summary>
        /// Called once when Show() is first called. Override to setup button listeners.
        /// </summary>
        protected virtual void Init()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _initialized = true;
        }

        public virtual void Show()
        {
            if (!_initialized) Init();

            gameObject.SetActive(true);
            DOTween.Kill(this);
            transform.localScale = Vector3.one * 0.5f;
            _canvasGroup.alpha = 0f;
            transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetId(this);
            _canvasGroup.DOFade(1f, 0.4f).SetUpdate(true).SetId(this);
        }

        public virtual void Hide()
        {
            if (!gameObject.activeSelf) return;

            DOTween.Kill(this);
            transform.DOScale(0.5f, 0.25f).SetEase(Ease.InBack).SetUpdate(true).SetId(this);
            _canvasGroup.DOFade(0f, 0.25f).SetUpdate(true).SetId(this)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}

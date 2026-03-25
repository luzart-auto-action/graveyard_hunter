using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using GraveyardHunter.Core;

namespace GraveyardHunter.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private List<UIPanel> _panels;
        [SerializeField] private GameplayUI _gameplayUI;

        private Dictionary<string, UIPanel> _panelDict;

        private void Awake()
        {
            ServiceLocator.Register(this);

            _panelDict = new Dictionary<string, UIPanel>();
            foreach (var panel in _panels)
            {
                if (panel != null && !string.IsNullOrEmpty(panel.PanelName))
                    _panelDict[panel.PanelName] = panel;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<UIManager>();
        }

        public void ShowPanel(string name)
        {
            if (_panelDict.TryGetValue(name, out var panel))
                panel.Show();
            else
                Debug.LogWarning($"[UIManager] Panel '{name}' not found.");
        }

        public void HidePanel(string name)
        {
            if (_panelDict.TryGetValue(name, out var panel))
                panel.Hide();
            else
                Debug.LogWarning($"[UIManager] Panel '{name}' not found.");
        }

        public void ForceHideAllPanels()
        {
            foreach (var panel in _panels)
            {
                if (panel == null) continue;
                DOTween.Kill(panel);

                // GameplayUI uses CanvasGroup (never SetActive)
                if (panel is GameplayUI gpu)
                    gpu.ForceHide();
                else
                    panel.gameObject.SetActive(false);
            }

            // Also handle _gameplayUI if it's not in _panels list
            if (_gameplayUI != null && !_panels.Contains(_gameplayUI))
                _gameplayUI.ForceHide();
        }

        public T GetPanel<T>(string name) where T : UIPanel
        {
            if (_panelDict.TryGetValue(name, out var panel))
                return panel as T;

            Debug.LogWarning($"[UIManager] Panel '{name}' not found.");
            return null;
        }

        public GameplayUI GetGameplayUI()
        {
            return _gameplayUI;
        }

        public void ShowGameplayUI()
        {
            if (_gameplayUI != null)
                _gameplayUI.Show();
        }

        public void HideGameplayUI()
        {
            if (_gameplayUI != null)
                _gameplayUI.Hide();
        }
    }
}

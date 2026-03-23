using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraveyardHunter.Core;
using GraveyardHunter.Data;

namespace GraveyardHunter.UI
{
    public class ShopPanel : UIPanel
    {
        [SerializeField] private Transform _skinItemParent;
        [SerializeField] private GameObject _skinItemPrefab;
        [SerializeField] private TextMeshProUGUI _totalScoreText;
        [SerializeField] private Button _closeButton;

        private readonly List<GameObject> _spawnedItems = new List<GameObject>();

        protected override void Init()
        {
            base.Init();

            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
        }

        public override void Show()
        {
            if (!_initialized) Init();

            if (_totalScoreText != null)
                _totalScoreText.text = $"Score: {PlayerProgressData.GetTotalScore()}";

            BuildSkinList();
            base.Show();
        }

        private void BuildSkinList()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null) Destroy(item);
            }
            _spawnedItems.Clear();

            if (!ServiceLocator.TryGet<Data.GameConfig>(out var config)) return;
            if (config.AvailableSkins == null) return;

            for (int i = 0; i < config.AvailableSkins.Count; i++)
            {
                var skinData = config.AvailableSkins[i];
                if (_skinItemPrefab == null || _skinItemParent == null) continue;

                var itemGO = Instantiate(_skinItemPrefab, _skinItemParent);
                _spawnedItems.Add(itemGO);

                var nameText = itemGO.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var priceText = itemGO.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
                var actionButton = itemGO.transform.Find("ActionButton")?.GetComponent<Button>();
                var actionButtonText = actionButton?.GetComponentInChildren<TextMeshProUGUI>();

                if (nameText != null) nameText.text = skinData.SkinName;

                bool isUnlocked = PlayerProgressData.IsSkinUnlocked(i);
                bool isEquipped = PlayerProgressData.GetEquippedSkin() == i;

                if (priceText != null)
                    priceText.text = isUnlocked ? "Owned" : $"{skinData.Price}";

                if (actionButton != null && actionButtonText != null)
                {
                    int skinIndex = i;

                    if (isEquipped)
                    {
                        actionButtonText.text = "Equipped";
                        actionButton.interactable = false;
                    }
                    else if (isUnlocked)
                    {
                        actionButtonText.text = "Equip";
                        actionButton.interactable = true;
                        actionButton.onClick.AddListener(() => OnEquipSkin(skinIndex));
                    }
                    else
                    {
                        actionButtonText.text = $"Buy ({skinData.Price})";
                        actionButton.interactable = PlayerProgressData.GetTotalScore() >= skinData.Price;
                        actionButton.onClick.AddListener(() => OnBuySkin(skinIndex));
                    }
                }
            }
        }

        private void OnBuySkin(int index)
        {
            if (!ServiceLocator.TryGet<Data.GameConfig>(out var config)) return;

            var skinData = config.AvailableSkins[index];
            if (PlayerProgressData.GetTotalScore() < skinData.Price) return;

            PlayerProgressData.UnlockSkin(index);
            EventBus.Publish(new SkinPurchasedEvent { SkinIndex = index });
            EventBus.Publish(new PlaySFXEvent("Purchase"));

            if (_totalScoreText != null)
                _totalScoreText.text = $"Score: {PlayerProgressData.GetTotalScore()}";
            BuildSkinList();
        }

        private void OnEquipSkin(int index)
        {
            PlayerProgressData.SetEquippedSkin(index);
            EventBus.Publish(new SkinEquippedEvent { SkinIndex = index });
            EventBus.Publish(new PlaySFXEvent("Equip"));
            BuildSkinList();
        }

        private void OnCloseClicked()
        {
            EventBus.Publish(new PlaySFXEvent("ButtonClick"));
            Hide();
        }
    }
}

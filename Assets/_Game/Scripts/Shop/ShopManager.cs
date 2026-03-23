using System.Collections.Generic;
using GraveyardHunter.Core;
using GraveyardHunter.Data;
using UnityEngine;

namespace GraveyardHunter.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private GameConfig _gameConfig;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ShopManager>();
        }

        public bool CanBuySkin(int index)
        {
            var skins = _gameConfig.AvailableSkins;
            if (index < 0 || index >= skins.Count) return false;
            if (PlayerProgressData.IsSkinUnlocked(index)) return false;

            return PlayerProgressData.GetTotalScore() >= skins[index].Price;
        }

        public bool BuySkin(int index)
        {
            if (!CanBuySkin(index)) return false;

            PlayerProgressData.UnlockSkin(index);
            EventBus.Publish(new SkinPurchasedEvent { SkinIndex = index });
            return true;
        }

        public void EquipSkin(int index)
        {
            if (!PlayerProgressData.IsSkinUnlocked(index)) return;

            PlayerProgressData.SetEquippedSkin(index);
            EventBus.Publish(new SkinEquippedEvent { SkinIndex = index });
        }

        public int GetEquippedSkin()
        {
            return PlayerProgressData.GetEquippedSkin();
        }

        public List<SkinData> GetAllSkins()
        {
            return _gameConfig.AvailableSkins;
        }
    }
}

using GraveyardHunter.Core;
using GraveyardHunter.Data;
using UnityEngine;

namespace GraveyardHunter.Shop
{
    public class SkinApplier : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;

        private Animator _animator;
        private GameObject _currentModel;

        private void Awake()
        {
            EventBus.Subscribe<SkinEquippedEvent>(OnSkinEquipped);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SkinEquippedEvent>(OnSkinEquipped);
        }

        public void ApplyEquippedSkin()
        {
            if (!ServiceLocator.TryGet<ShopManager>(out var shopManager)) return;

            int equippedIndex = shopManager.GetEquippedSkin();
            var skins = shopManager.GetAllSkins();

            if (equippedIndex < 0 || equippedIndex >= skins.Count) return;

            ApplySkin(skins[equippedIndex]);
        }

        private void OnSkinEquipped(SkinEquippedEvent evt)
        {
            if (!ServiceLocator.TryGet<ShopManager>(out var shopManager)) return;

            var skins = shopManager.GetAllSkins();
            if (evt.SkinIndex < 0 || evt.SkinIndex >= skins.Count) return;

            ApplySkin(skins[evt.SkinIndex]);
        }

        private void ApplySkin(SkinData skinData)
        {
            if (_visualRoot == null || skinData.ModelPrefab == null) return;

            // Destroy current model children under visual root (skip PlayerLight and FX)
            for (int i = _visualRoot.childCount - 1; i >= 0; i--)
            {
                var child = _visualRoot.GetChild(i);
                if (child.name == "PlayerLight") continue;
                Destroy(child.gameObject);
            }

            // Instantiate new model
            _currentModel = Instantiate(skinData.ModelPrefab, _visualRoot);
            _currentModel.transform.localPosition = Vector3.zero;
            _currentModel.transform.localRotation = Quaternion.identity;
            _currentModel.transform.localScale = Vector3.one;

            // Re-cache animator on PlayerController
            _animator = _currentModel.GetComponentInChildren<Animator>();

            var playerController = GetComponent<Player.PlayerController>();
            if (playerController != null)
            {
                // PlayerController uses GetComponentInChildren<Animator>() in Awake
                // After skin swap, the animator reference needs to be refreshed
                // We use reflection to update the private _animator field
                var field = typeof(Player.PlayerController).GetField("_animator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    field.SetValue(playerController, _animator);
            }
        }
    }
}

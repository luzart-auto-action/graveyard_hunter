using System.Collections;
using UnityEngine;
using DG.Tweening;
using GraveyardHunter.Core;
using GraveyardHunter.Player;

namespace GraveyardHunter.Booster
{
    public class ShadowCloakBooster : BoosterBase
    {
        [SerializeField] private float _cloakedAlpha = 0.3f;
        [SerializeField] private float _fadeDuration = 0.3f;

        protected override void Activate(GameObject player)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerController == null || playerHealth == null) return;

            playerController.IsInvisible = true;
            playerHealth.SetInvulnerable(true);

            PlaySound("ShadowCloak");
            SpawnFX("ShadowCloakFX");

            FadePlayer(player, _cloakedAlpha);

            EventBus.Publish(new BoosterActivatedEvent(BoosterType.ShadowCloak, _duration));

            playerController.StartCoroutine(CloakRoutine(player, playerController, playerHealth));
        }

        private IEnumerator CloakRoutine(GameObject player, PlayerController playerController, PlayerHealth playerHealth)
        {
            yield return new WaitForSeconds(_duration);

            playerController.IsInvisible = false;
            playerHealth.SetInvulnerable(false);

            FadePlayer(player, 1f);

            EventBus.Publish(new BoosterExpiredEvent(BoosterType.ShadowCloak));
        }

        private void FadePlayer(GameObject player, float targetAlpha)
        {
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        mat.DOColor(new Color(color.r, color.g, color.b, targetAlpha), _fadeDuration);
                    }
                }
            }
        }
    }
}

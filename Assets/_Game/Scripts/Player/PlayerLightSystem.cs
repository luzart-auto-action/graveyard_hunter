using GraveyardHunter.Data;
using UnityEngine;

namespace GraveyardHunter.Player
{
    public class PlayerLightSystem : MonoBehaviour
    {
        [SerializeField] private Light _pointLight;
        [SerializeField] private Light _spotLight;
        [SerializeField] private Color _lightColor = new Color(1f, 0.92f, 0.7f, 1f);
        [SerializeField] private float _pointIntensity = 1.5f;
        [SerializeField] private float _spotIntensity = 2f;

        public void Initialize(GameConfig config)
        {
            if (_pointLight != null)
            {
                _pointLight.type = LightType.Point;
                _pointLight.color = _lightColor;
                _pointLight.intensity = _pointIntensity;
                _pointLight.range = config.PlayerLightRadius;
                _pointLight.shadows = LightShadows.Soft;
            }

            if (_spotLight != null)
            {
                _spotLight.type = LightType.Spot;
                _spotLight.color = _lightColor;
                _spotLight.intensity = _spotIntensity;
                _spotLight.range = config.PlayerFlashlightRange;
                _spotLight.spotAngle = config.PlayerFlashlightAngle;
                _spotLight.shadows = LightShadows.Soft;
            }
        }

        public void UpdateLightDirection(Vector3 forward)
        {
            if (_spotLight != null && forward.sqrMagnitude > 0.01f)
            {
                _spotLight.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }

        public void SetLightsEnabled(bool enabled)
        {
            if (_pointLight != null)
            {
                _pointLight.enabled = enabled;
            }

            if (_spotLight != null)
            {
                _spotLight.enabled = enabled;
            }
        }
    }
}

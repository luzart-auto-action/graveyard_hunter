using System.Collections;
using DG.Tweening;
using GraveyardHunter.Core;
using UnityEngine;

namespace GraveyardHunter.FX
{
    /// <summary>
    /// Attach to the Player. Listens for booster events and spawns
    /// particle-system effects that follow the player while active.
    /// All particles are created via code – no prefabs needed.
    /// </summary>
    public class BoosterVFX : MonoBehaviour
    {
        [Header("Attach Points")]
        [SerializeField] private Transform _fxCenter;  // center of player
        [SerializeField] private Transform _fxBottom;  // feet
        [SerializeField] private Transform _fxTop;     // head

        private GameObject _activeVFX;
        private Coroutine _activeRoutine;

        private void Awake()
        {
            EventBus.Subscribe<BoosterActivatedEvent>(OnBoosterActivated);
            EventBus.Subscribe<BoosterExpiredEvent>(OnBoosterExpired);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<BoosterActivatedEvent>(OnBoosterActivated);
            EventBus.Unsubscribe<BoosterExpiredEvent>(OnBoosterExpired);
        }

        private void OnBoosterActivated(BoosterActivatedEvent evt)
        {
            StopActiveVFX();

            switch (evt.Type)
            {
                case BoosterType.SmokeBomb:
                    _activeVFX = CreateSmokeBombVFX(evt.Duration);
                    break;
                case BoosterType.SpeedBoots:
                    _activeVFX = CreateSpeedBootsVFX(evt.Duration);
                    break;
                case BoosterType.ShadowCloak:
                    _activeVFX = CreateShadowCloakVFX(evt.Duration);
                    break;
                case BoosterType.GhostVision:
                    _activeVFX = CreateGhostVisionVFX(evt.Duration);
                    break;
            }
        }

        private void OnBoosterExpired(BoosterExpiredEvent evt)
        {
            StopActiveVFX();
        }

        private void StopActiveVFX()
        {
            if (_activeVFX != null)
            {
                Destroy(_activeVFX);
                _activeVFX = null;
            }
        }

        private Transform GetAnchor(Transform preferred, Transform fallback)
        {
            return preferred != null ? preferred : (fallback != null ? fallback : transform);
        }

        // ===================== SMOKE BOMB =====================
        // Expanding smoke ring at feet + lingering fog particles

        private GameObject CreateSmokeBombVFX(float duration)
        {
            var root = new GameObject("VFX_SmokeBomb");
            root.transform.SetParent(GetAnchor(_fxBottom, _fxCenter), false);
            root.transform.localPosition = Vector3.zero;

            // 1. Burst ring
            var burstGO = new GameObject("SmokeBurst");
            burstGO.transform.SetParent(root.transform, false);
            var burst = CreatePS(burstGO);
            var burstMain = burst.main;
            burstMain.duration = 0.5f;
            burstMain.loop = false;
            burstMain.startLifetime = 1.5f;
            burstMain.startSpeed = 3f;
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            burstMain.startColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
            burstMain.gravityModifier = -0.1f;
            burstMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var burstEmission = burst.emission;
            burstEmission.rateOverTime = 0;
            burstEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

            var burstShape = burst.shape;
            burstShape.shapeType = ParticleSystemShapeType.Circle;
            burstShape.radius = 0.5f;

            var burstCol = burst.colorOverLifetime;
            burstCol.enabled = true;
            var burstGrad = new Gradient();
            burstGrad.SetKeys(
                new[] { new GradientColorKey(new Color(0.7f, 0.7f, 0.7f), 0f), new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            burstCol.color = burstGrad;

            var burstSize = burst.sizeOverLifetime;
            burstSize.enabled = true;
            burstSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f), new Keyframe(1f, 2f)));

            SetParticleRenderer(burstGO, new Color(0.6f, 0.6f, 0.6f, 0.7f));

            // 2. Lingering fog
            var fogGO = new GameObject("SmokeFog");
            fogGO.transform.SetParent(root.transform, false);
            var fog = CreatePS(fogGO);
            var fogMain = fog.main;
            fogMain.duration = duration;
            fogMain.loop = true;
            fogMain.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            fogMain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
            fogMain.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            fogMain.startColor = new Color(0.5f, 0.5f, 0.55f, 0.4f);
            fogMain.gravityModifier = -0.05f;
            fogMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var fogEmission = fog.emission;
            fogEmission.rateOverTime = 15;

            var fogShape = fog.shape;
            fogShape.shapeType = ParticleSystemShapeType.Sphere;
            fogShape.radius = 1.5f;

            var fogCol = fog.colorOverLifetime;
            fogCol.enabled = true;
            var fogGrad = new Gradient();
            fogGrad.SetKeys(
                new[] { new GradientColorKey(new Color(0.5f, 0.5f, 0.55f), 0f), new GradientColorKey(new Color(0.4f, 0.4f, 0.45f), 1f) },
                new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            fogCol.color = fogGrad;

            SetParticleRenderer(fogGO, new Color(0.5f, 0.5f, 0.55f, 0.4f));

            PlayAllPS(root);
            Destroy(root, duration + 2f);
            return root;
        }

        // ===================== SPEED BOOTS =====================
        // Trail particles from feet + motion lines

        private GameObject CreateSpeedBootsVFX(float duration)
        {
            var root = new GameObject("VFX_SpeedBoots");
            root.transform.SetParent(GetAnchor(_fxBottom, _fxCenter), false);
            root.transform.localPosition = Vector3.zero;

            // 1. Speed trail from feet
            var trailGO = new GameObject("SpeedTrail");
            DOVirtual.DelayedCall(duration, () =>
            {
                if (trailGO)
                {
                    trailGO.gameObject.SetActive(false);    
                }
            });
            trailGO.transform.SetParent(root.transform, false);
            var trail = CreatePS(trailGO);
            var trailMain = trail.main;
            trailMain.duration = duration;
            trailMain.loop = true;
            trailMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            trailMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            trailMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            trailMain.startColor = new Color(1f, 0.7f, 0.2f, 0.8f);
            trailMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var trailEmission = trail.emission;
            trailEmission.rateOverTime = 30;

            var trailShape = trail.shape;
            trailShape.shapeType = ParticleSystemShapeType.Cone;
            trailShape.angle = 25f;
            trailShape.radius = 0.2f;
            trailShape.rotation = new Vector3(90f, 0f, 0f); // point down/back

            var trailCol = trail.colorOverLifetime;
            trailCol.enabled = true;
            var trailGrad = new Gradient();
            trailGrad.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f), new GradientColorKey(new Color(1f, 0.4f, 0.1f), 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trailCol.color = trailGrad;

            var trailSize = trail.sizeOverLifetime;
            trailSize.enabled = true;
            trailSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(1f, 0f)));

            SetParticleRenderer(trailGO, new Color(1f, 0.7f, 0.2f, 0.8f));

            // 2. Burst on activate
            var burstGO = new GameObject("SpeedBurst");
            DOVirtual.DelayedCall(0.3f, () =>
            {
                if (burstGO)
                {
                    burstGO.gameObject.SetActive(false);
                }
            });
            burstGO.transform.SetParent(root.transform, false);
            var burstPS = CreatePS(burstGO);
            var burstMain = burstPS.main;
            burstMain.duration = 0.3f;
            burstMain.loop = false;
            burstMain.startLifetime = 0.5f;
            burstMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            burstMain.startColor = new Color(1f, 0.9f, 0.4f, 1f);
            burstMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var burstEmission = burstPS.emission;
            burstEmission.rateOverTime = 0;
            burstEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

            var burstShape = burstPS.shape;
            burstShape.shapeType = ParticleSystemShapeType.Sphere;
            burstShape.radius = 0.3f;

            SetParticleRenderer(burstGO, new Color(1f, 0.9f, 0.4f, 1f));

            // 3. Wind lines around player
            var windGO = new GameObject("WindLines");
            DOVirtual.DelayedCall(duration, () =>
            {
                if (windGO)
                {
                    windGO.gameObject.SetActive(false);
                }
            });
            windGO.transform.SetParent(root.transform, false);
            windGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var wind = CreatePS(windGO);
            var windMain = wind.main;
            windMain.duration = duration;
            windMain.loop = true;
            windMain.startLifetime = 0.4f;
            windMain.startSpeed = new ParticleSystem.MinMaxCurve(3f, 5f);
            windMain.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            windMain.startColor = new Color(1f, 1f, 1f, 0.5f);
            windMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var windEmission = wind.emission;
            windEmission.rateOverTime = 20;

            var windShape = wind.shape;
            windShape.shapeType = ParticleSystemShapeType.Circle;
            windShape.radius = 0.8f;

            // Stretch particles for line effect
            var windRenderer = windGO.GetComponent<ParticleSystemRenderer>();
            if (windRenderer != null)
            {
                windRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                windRenderer.lengthScale = 4f;
                windRenderer.velocityScale = 0.1f;
            }

            SetParticleRenderer(windGO, new Color(1f, 1f, 1f, 0.4f));

            PlayAllPS(root);
            Destroy(root, duration + 1f);
            return root;
        }

        // ===================== SHADOW CLOAK =====================
        // Dark aura + shadow wisps rising from body

        private GameObject CreateShadowCloakVFX(float duration)
        {
            var root = new GameObject("VFX_ShadowCloak");
            root.transform.SetParent(GetAnchor(_fxCenter, transform), false);
            root.transform.localPosition = Vector3.zero;

            // 1. Dark aura
            var auraGO = new GameObject("DarkAura");
            DOVirtual.DelayedCall(duration, () =>
            {
                if (auraGO)
                {
                    auraGO.gameObject.SetActive(false);
                }
            });
            auraGO.transform.SetParent(root.transform, false);
            var aura = CreatePS(auraGO);
            var auraMain = aura.main;
            auraMain.duration = duration;
            auraMain.loop = true;
            auraMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            auraMain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            auraMain.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            auraMain.startColor = new Color(0.1f, 0.0f, 0.15f, 0.5f);
            auraMain.gravityModifier = -0.2f;
            auraMain.simulationSpace = ParticleSystemSimulationSpace.Local;

            var auraEmission = aura.emission;
            auraEmission.rateOverTime = 20;

            var auraShape = aura.shape;
            auraShape.shapeType = ParticleSystemShapeType.Sphere;
            auraShape.radius = 0.6f;

            var auraCol = aura.colorOverLifetime;
            auraCol.enabled = true;
            var auraGrad = new Gradient();
            auraGrad.SetKeys(
                new[] { new GradientColorKey(new Color(0.15f, 0.0f, 0.2f), 0f), new GradientColorKey(new Color(0.05f, 0.0f, 0.1f), 1f) },
                new[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            auraCol.color = auraGrad;

            var auraSize = aura.sizeOverLifetime;
            auraSize.enabled = true;
            auraSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.8f), new Keyframe(0.5f, 1.2f), new Keyframe(1f, 0.3f)));

            SetParticleRenderer(auraGO, new Color(0.1f, 0.0f, 0.15f, 0.5f));

            // 2. Shadow wisps rising
            var wispsGO = new GameObject("ShadowWisps");
            DOVirtual.DelayedCall(duration, () =>
            {
                if (wispsGO)
                {
                    wispsGO.gameObject.SetActive(false);
                }
            });
            wispsGO.transform.SetParent(root.transform, false);
            wispsGO.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            var wisps = CreatePS(wispsGO);
            var wispsMain = wisps.main;
            wispsMain.duration = duration;
            wispsMain.loop = true;
            wispsMain.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            wispsMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            wispsMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            wispsMain.startColor = new Color(0.2f, 0.05f, 0.3f, 0.7f);
            wispsMain.gravityModifier = -0.5f;
            wispsMain.simulationSpace = ParticleSystemSimulationSpace.Local;

            var wispsEmission = wisps.emission;
            wispsEmission.rateOverTime = 12;

            var wispsShape = wisps.shape;
            wispsShape.shapeType = ParticleSystemShapeType.Circle;
            wispsShape.radius = 0.5f;

            var wispsCol = wisps.colorOverLifetime;
            wispsCol.enabled = true;
            var wispsGrad = new Gradient();
            wispsGrad.SetKeys(
                new[] { new GradientColorKey(new Color(0.3f, 0.05f, 0.4f), 0f), new GradientColorKey(new Color(0.1f, 0.0f, 0.15f), 1f) },
                new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            wispsCol.color = wispsGrad;

            SetParticleRenderer(wispsGO, new Color(0.2f, 0.05f, 0.3f, 0.7f));

            // 3. Activate burst
            var burstGO = new GameObject("CloakBurst");
            DOVirtual.DelayedCall(0.3f, () =>
            {
                if (burstGO)
                {
                    burstGO.gameObject.SetActive(false);
                }
            });
            burstGO.transform.SetParent(root.transform, false);
            var burstPS = CreatePS(burstGO);
            var burstMain = burstPS.main;
            burstMain.duration = 0.3f;
            burstMain.loop = false;
            burstMain.startLifetime = 0.8f;
            burstMain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            burstMain.startColor = new Color(0.3f, 0.0f, 0.5f, 0.8f);
            burstMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var burstEmission = burstPS.emission;
            burstEmission.rateOverTime = 0;
            burstEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });

            var burstShape = burstPS.shape;
            burstShape.shapeType = ParticleSystemShapeType.Sphere;
            burstShape.radius = 0.4f;

            SetParticleRenderer(burstGO, new Color(0.3f, 0.0f, 0.5f, 0.8f));

            PlayAllPS(root);
            Destroy(root, duration + 2f);
            return root;
        }

        // ===================== GHOST VISION =====================
        // Eye glow + scanning pulse ring + ethereal eye particles

        private GameObject CreateGhostVisionVFX(float duration)
        {
            var root = new GameObject("VFX_GhostVision");
            root.transform.SetParent(GetAnchor(_fxTop, _fxCenter), false);
            root.transform.localPosition = Vector3.zero;

            // 1. Eye glow aura on head
            var glowGO = new GameObject("EyeGlow");
            glowGO.transform.SetParent(root.transform, false);
            var glow = CreatePS(glowGO);
            var glowMain = glow.main;
            glowMain.duration = duration;
            glowMain.loop = true;
            glowMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            glowMain.startSpeed = 0.1f;
            glowMain.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            glowMain.startColor = new Color(0.2f, 0.8f, 1f, 0.6f);
            glowMain.simulationSpace = ParticleSystemSimulationSpace.Local;

            var glowEmission = glow.emission;
            glowEmission.rateOverTime = 8;

            var glowShape = glow.shape;
            glowShape.shapeType = ParticleSystemShapeType.Sphere;
            glowShape.radius = 0.15f;

            var glowCol = glow.colorOverLifetime;
            glowCol.enabled = true;
            var glowGrad = new Gradient();
            glowGrad.SetKeys(
                new[] { new GradientColorKey(new Color(0.3f, 0.9f, 1f), 0f), new GradientColorKey(new Color(0.1f, 0.5f, 0.8f), 1f) },
                new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            glowCol.color = glowGrad;

            SetParticleRenderer(glowGO, new Color(0.2f, 0.8f, 1f, 0.6f));

            // 2. Scanning pulse ring (repeating)
            var pulseGO = new GameObject("ScanPulse");
            pulseGO.transform.SetParent(root.transform, false);
            pulseGO.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            var pulse = CreatePS(pulseGO);
            var pulseMain = pulse.main;
            pulseMain.duration = duration;
            pulseMain.loop = true;
            pulseMain.startLifetime = 1.5f;
            pulseMain.startSpeed = 3f;
            pulseMain.startSize = 0.1f;
            pulseMain.startColor = new Color(0.2f, 0.8f, 1f, 0.4f);
            pulseMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var pulseEmission = pulse.emission;
            pulseEmission.rateOverTime = 0;
            pulseEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40, 1, 1.5f) });

            var pulseShape = pulse.shape;
            pulseShape.shapeType = ParticleSystemShapeType.Circle;
            pulseShape.radius = 0.1f;

            var pulseCol = pulse.colorOverLifetime;
            pulseCol.enabled = true;
            var pulseGrad = new Gradient();
            pulseGrad.SetKeys(
                new[] { new GradientColorKey(new Color(0.3f, 0.9f, 1f), 0f), new GradientColorKey(new Color(0.1f, 0.4f, 0.6f), 1f) },
                new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            pulseCol.color = pulseGrad;

            var pulseSize = pulse.sizeOverLifetime;
            pulseSize.enabled = true;
            pulseSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f), new Keyframe(1f, 0.05f)));

            SetParticleRenderer(pulseGO, new Color(0.2f, 0.8f, 1f, 0.4f));

            // 3. Floating data particles
            var dataGO = new GameObject("DataParticles");
            dataGO.transform.SetParent(root.transform, false);
            dataGO.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            var data = CreatePS(dataGO);
            var dataMain = data.main;
            dataMain.duration = duration;
            dataMain.loop = true;
            dataMain.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            dataMain.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            dataMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            dataMain.startColor = new Color(0.4f, 1f, 0.8f, 0.8f);
            dataMain.gravityModifier = -0.3f;
            dataMain.simulationSpace = ParticleSystemSimulationSpace.Local;

            var dataEmission = data.emission;
            dataEmission.rateOverTime = 10;

            var dataShape = data.shape;
            dataShape.shapeType = ParticleSystemShapeType.Box;
            dataShape.scale = new Vector3(1f, 1.5f, 1f);

            SetParticleRenderer(dataGO, new Color(0.4f, 1f, 0.8f, 0.8f));

            PlayAllPS(root);
            Destroy(root, duration + 2f);
            return root;
        }

        // ===================== HELPERS =====================

        /// <summary>
        /// AddComponent auto-plays the system. We must stop it first,
        /// configure everything, then explicitly Play() after setup.
        /// </summary>
        private ParticleSystem CreatePS(GameObject go)
        {
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        private void PlayAllPS(GameObject root)
        {
            var systems = root.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in systems)
                ps.Play(true);
        }

        private void SetParticleRenderer(GameObject go, Color color)
        {
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer == null) return;

            renderer.renderMode = renderer.renderMode == ParticleSystemRenderMode.Stretch
                ? ParticleSystemRenderMode.Stretch
                : ParticleSystemRenderMode.Billboard;

            // Use default particle material
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"))
            {
                color = color
            };
            renderer.material.SetFloat("_Mode", 3f); // Transparent
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        }
    }
}

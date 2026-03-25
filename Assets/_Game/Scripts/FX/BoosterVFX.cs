using DG.Tweening;
using GraveyardHunter.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.FX
{
    public class BoosterVFX : MonoBehaviour
    {
        [Header("Attach Points")]
        [SerializeField] private Transform _fxCenter;
        [SerializeField] private Transform _fxBottom;
        [SerializeField] private Transform _fxTop;

        [FoldoutGroup("Smoke Bomb")]
        [SerializeField] private SmokeBombVFXConfig _smoke = new();

        [FoldoutGroup("Speed Boots")]
        [SerializeField] private SpeedBootsVFXConfig _speed = new();

        [FoldoutGroup("Shadow Cloak")]
        [SerializeField] private ShadowCloakVFXConfig _shadow = new();

        [FoldoutGroup("Ghost Vision")]
        [SerializeField] private GhostVisionVFXConfig _ghost = new();

        private GameObject _activeVFX;

        // ===================== CONFIG STRUCTS =====================

        [System.Serializable]
        public class SmokeBombVFXConfig
        {
            [Title("Burst Ring")]
            public int BurstCount = 30;
            public float BurstSpeed = 3f;
            public float BurstLifetime = 1.5f;
            public Vector2 BurstSizeRange = new(0.8f, 1.5f);
            public float BurstRadius = 0.5f;
            public Color BurstColor = new(0.6f, 0.6f, 0.6f, 0.7f);
            public float BurstGravity = -0.1f;

            [Title("Fog")]
            public float FogEmissionRate = 15f;
            public Vector2 FogLifetimeRange = new(1f, 2f);
            public Vector2 FogSpeedRange = new(0.1f, 0.5f);
            public Vector2 FogSizeRange = new(0.5f, 1.2f);
            public float FogRadius = 1.5f;
            public Color FogColor = new(0.5f, 0.5f, 0.55f, 0.4f);
            public float FogGravity = -0.05f;
        }

        [System.Serializable]
        public class SpeedBootsVFXConfig
        {
            [Title("Speed Trail")]
            public float TrailEmissionRate = 30f;
            public Vector2 TrailLifetimeRange = new(0.3f, 0.6f);
            public Vector2 TrailSpeedRange = new(0.5f, 1.5f);
            public Vector2 TrailSizeRange = new(0.1f, 0.3f);
            public float TrailConeAngle = 25f;
            public float TrailConeRadius = 0.2f;
            public Color TrailColorStart = new(1f, 0.8f, 0.3f, 0.9f);
            public Color TrailColorEnd = new(1f, 0.4f, 0.1f, 0f);

            [Title("Activate Burst")]
            public int BurstCount = 20;
            public Vector2 BurstSpeedRange = new(2f, 4f);
            public Vector2 BurstSizeRange = new(0.15f, 0.3f);
            public Color BurstColor = new(1f, 0.9f, 0.4f, 1f);

            [Title("Wind Lines")]
            public float WindEmissionRate = 20f;
            public float WindLifetime = 0.4f;
            public Vector2 WindSpeedRange = new(3f, 5f);
            public Vector2 WindSizeRange = new(0.02f, 0.05f);
            public float WindCircleRadius = 0.8f;
            public float WindStretchLength = 4f;
            public Color WindColor = new(1f, 1f, 1f, 0.4f);
        }

        [System.Serializable]
        public class ShadowCloakVFXConfig
        {
            [Title("Dark Aura")]
            public float AuraEmissionRate = 20f;
            public Vector2 AuraLifetimeRange = new(0.8f, 1.5f);
            public Vector2 AuraSpeedRange = new(0.1f, 0.3f);
            public Vector2 AuraSizeRange = new(0.6f, 1.2f);
            public float AuraRadius = 0.6f;
            public Color AuraColor = new(0.1f, 0f, 0.15f, 0.5f);
            public float AuraGravity = -0.2f;

            [Title("Shadow Wisps")]
            public float WispsEmissionRate = 12f;
            public Vector2 WispsLifetimeRange = new(1f, 2f);
            public Vector2 WispsSpeedRange = new(0.5f, 1.2f);
            public Vector2 WispsSizeRange = new(0.1f, 0.25f);
            public float WispsRadius = 0.5f;
            public Color WispsColor = new(0.2f, 0.05f, 0.3f, 0.7f);
            public float WispsGravity = -0.5f;

            [Title("Activate Burst")]
            public int BurstCount = 25;
            public Vector2 BurstSpeedRange = new(1f, 3f);
            public Vector2 BurstSizeRange = new(0.2f, 0.5f);
            public Color BurstColor = new(0.3f, 0f, 0.5f, 0.8f);
        }

        [System.Serializable]
        public class GhostVisionVFXConfig
        {
            [Title("Eye Glow")]
            public float GlowEmissionRate = 8f;
            public Vector2 GlowLifetimeRange = new(0.5f, 1f);
            public Vector2 GlowSizeRange = new(0.3f, 0.5f);
            public float GlowRadius = 0.15f;
            public Color GlowColorStart = new(0.3f, 0.9f, 1f, 0.7f);
            public Color GlowColorEnd = new(0.1f, 0.5f, 0.8f, 0f);

            [Title("Scan Pulse")]
            public int PulseBurstCount = 40;
            public float PulseInterval = 1.5f;
            public float PulseSpeed = 3f;
            public float PulseLifetime = 1.5f;
            public Color PulseColor = new(0.2f, 0.8f, 1f, 0.4f);

            [Title("Data Particles")]
            public float DataEmissionRate = 10f;
            public Vector2 DataLifetimeRange = new(1f, 2f);
            public Vector2 DataSpeedRange = new(0.3f, 0.8f);
            public Vector2 DataSizeRange = new(0.05f, 0.12f);
            public Color DataColor = new(0.4f, 1f, 0.8f, 0.8f);
            public float DataGravity = -0.3f;
        }

        // ===================== EVENTS =====================

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

            _activeVFX = evt.Type switch
            {
                BoosterType.SmokeBomb => CreateSmokeBombVFX(evt.Duration),
                BoosterType.SpeedBoots => CreateSpeedBootsVFX(evt.Duration),
                BoosterType.ShadowCloak => CreateShadowCloakVFX(evt.Duration),
                BoosterType.GhostVision => CreateGhostVisionVFX(evt.Duration),
                _ => null
            };
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

        private GameObject CreateSmokeBombVFX(float duration)
        {
            var c = _smoke;
            var root = new GameObject("VFX_SmokeBomb");
            root.transform.SetParent(GetAnchor(_fxBottom, _fxCenter), false);
            root.transform.localPosition = Vector3.zero;

            // Burst ring
            var burstGO = new GameObject("SmokeBurst");
            burstGO.transform.SetParent(root.transform, false);
            var burst = CreatePS(burstGO);
            var bm = burst.main;
            bm.duration = 0.5f;
            bm.loop = false;
            bm.startLifetime = c.BurstLifetime;
            bm.startSpeed = c.BurstSpeed;
            bm.startSize = new ParticleSystem.MinMaxCurve(c.BurstSizeRange.x, c.BurstSizeRange.y);
            bm.startColor = c.BurstColor;
            bm.gravityModifier = c.BurstGravity;
            bm.simulationSpace = ParticleSystemSimulationSpace.World;

            var be = burst.emission;
            be.rateOverTime = 0;
            be.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)c.BurstCount) });

            var bs = burst.shape;
            bs.shapeType = ParticleSystemShapeType.Circle;
            bs.radius = c.BurstRadius;

            var bc = burst.colorOverLifetime;
            bc.enabled = true;
            var bg = new Gradient();
            bg.SetKeys(
                new[] { new GradientColorKey(c.BurstColor, 0f), new GradientColorKey(c.BurstColor * 0.7f, 1f) },
                new[] { new GradientAlphaKey(c.BurstColor.a, 0f), new GradientAlphaKey(0f, 1f) }
            );
            bc.color = bg;

            var bsol = burst.sizeOverLifetime;
            bsol.enabled = true;
            bsol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f), new Keyframe(1f, 2f)));

            SetParticleRenderer(burstGO, c.BurstColor);

            // Lingering fog
            var fogGO = new GameObject("SmokeFog");
            fogGO.transform.SetParent(root.transform, false);
            var fog = CreatePS(fogGO);
            var fm = fog.main;
            fm.duration = duration;
            fm.loop = true;
            fm.startLifetime = new ParticleSystem.MinMaxCurve(c.FogLifetimeRange.x, c.FogLifetimeRange.y);
            fm.startSpeed = new ParticleSystem.MinMaxCurve(c.FogSpeedRange.x, c.FogSpeedRange.y);
            fm.startSize = new ParticleSystem.MinMaxCurve(c.FogSizeRange.x, c.FogSizeRange.y);
            fm.startColor = c.FogColor;
            fm.gravityModifier = c.FogGravity;
            fm.simulationSpace = ParticleSystemSimulationSpace.World;

            var fe = fog.emission;
            fe.rateOverTime = c.FogEmissionRate;

            var fs = fog.shape;
            fs.shapeType = ParticleSystemShapeType.Sphere;
            fs.radius = c.FogRadius;

            var fc = fog.colorOverLifetime;
            fc.enabled = true;
            var fg = new Gradient();
            fg.SetKeys(
                new[] { new GradientColorKey(c.FogColor, 0f), new GradientColorKey(c.FogColor * 0.8f, 1f) },
                new[] { new GradientAlphaKey(c.FogColor.a, 0f), new GradientAlphaKey(0f, 1f) }
            );
            fc.color = fg;

            SetParticleRenderer(fogGO, c.FogColor);

            PlayAllPS(root);
            Destroy(root, duration + 2f);
            return root;
        }

        // ===================== SPEED BOOTS =====================

        private GameObject CreateSpeedBootsVFX(float duration)
        {
            var c = _speed;
            var root = new GameObject("VFX_SpeedBoots");
            root.transform.SetParent(GetAnchor(_fxBottom, _fxCenter), false);
            root.transform.localPosition = Vector3.zero;

            // Speed trail
            var trailGO = new GameObject("SpeedTrail");
            DOVirtual.DelayedCall(duration, () => { if (trailGO) trailGO.SetActive(false); });
            trailGO.transform.SetParent(root.transform, false);
            var trail = CreatePS(trailGO);
            var tm = trail.main;
            tm.duration = duration;
            tm.loop = true;
            tm.startLifetime = new ParticleSystem.MinMaxCurve(c.TrailLifetimeRange.x, c.TrailLifetimeRange.y);
            tm.startSpeed = new ParticleSystem.MinMaxCurve(c.TrailSpeedRange.x, c.TrailSpeedRange.y);
            tm.startSize = new ParticleSystem.MinMaxCurve(c.TrailSizeRange.x, c.TrailSizeRange.y);
            tm.startColor = c.TrailColorStart;
            tm.simulationSpace = ParticleSystemSimulationSpace.World;

            var te = trail.emission;
            te.rateOverTime = c.TrailEmissionRate;

            var ts = trail.shape;
            ts.shapeType = ParticleSystemShapeType.Cone;
            ts.angle = c.TrailConeAngle;
            ts.radius = c.TrailConeRadius;
            ts.rotation = new Vector3(90f, 0f, 0f);

            var tc = trail.colorOverLifetime;
            tc.enabled = true;
            var tg = new Gradient();
            tg.SetKeys(
                new[] { new GradientColorKey(c.TrailColorStart, 0f), new GradientColorKey(c.TrailColorEnd, 1f) },
                new[] { new GradientAlphaKey(c.TrailColorStart.a, 0f), new GradientAlphaKey(0f, 1f) }
            );
            tc.color = tg;

            var tsol = trail.sizeOverLifetime;
            tsol.enabled = true;
            tsol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(1f, 0f)));

            SetParticleRenderer(trailGO, c.TrailColorStart);

            // Activate burst
            var burstGO = new GameObject("SpeedBurst");
            DOVirtual.DelayedCall(0.3f, () => { if (burstGO) burstGO.SetActive(false); });
            burstGO.transform.SetParent(root.transform, false);
            var burstPS = CreatePS(burstGO);
            var bm = burstPS.main;
            bm.duration = 0.3f;
            bm.loop = false;
            bm.startLifetime = 0.5f;
            bm.startSpeed = new ParticleSystem.MinMaxCurve(c.BurstSpeedRange.x, c.BurstSpeedRange.y);
            bm.startSize = new ParticleSystem.MinMaxCurve(c.BurstSizeRange.x, c.BurstSizeRange.y);
            bm.startColor = c.BurstColor;
            bm.simulationSpace = ParticleSystemSimulationSpace.World;

            var bbe = burstPS.emission;
            bbe.rateOverTime = 0;
            bbe.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)c.BurstCount) });

            var bbs = burstPS.shape;
            bbs.shapeType = ParticleSystemShapeType.Sphere;
            bbs.radius = 0.3f;

            SetParticleRenderer(burstGO, c.BurstColor);

            // Wind lines
            var windGO = new GameObject("WindLines");
            DOVirtual.DelayedCall(duration, () => { if (windGO) windGO.SetActive(false); });
            windGO.transform.SetParent(root.transform, false);
            windGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var wind = CreatePS(windGO);
            var wm = wind.main;
            wm.duration = duration;
            wm.loop = true;
            wm.startLifetime = c.WindLifetime;
            wm.startSpeed = new ParticleSystem.MinMaxCurve(c.WindSpeedRange.x, c.WindSpeedRange.y);
            wm.startSize = new ParticleSystem.MinMaxCurve(c.WindSizeRange.x, c.WindSizeRange.y);
            wm.startColor = c.WindColor;
            wm.simulationSpace = ParticleSystemSimulationSpace.World;

            var we = wind.emission;
            we.rateOverTime = c.WindEmissionRate;

            var ws = wind.shape;
            ws.shapeType = ParticleSystemShapeType.Circle;
            ws.radius = c.WindCircleRadius;

            var wr = windGO.GetComponent<ParticleSystemRenderer>();
            if (wr != null)
            {
                wr.renderMode = ParticleSystemRenderMode.Stretch;
                wr.lengthScale = c.WindStretchLength;
                wr.velocityScale = 0.1f;
            }

            SetParticleRenderer(windGO, c.WindColor);

            PlayAllPS(root);
            Destroy(root, duration + 1f);
            return root;
        }

        // ===================== SHADOW CLOAK =====================

        private GameObject CreateShadowCloakVFX(float duration)
        {
            var c = _shadow;
            var root = new GameObject("VFX_ShadowCloak");
            root.transform.SetParent(GetAnchor(_fxCenter, transform), false);
            root.transform.localPosition = Vector3.zero;

            // Dark aura
            var auraGO = new GameObject("DarkAura");
            DOVirtual.DelayedCall(duration, () => { if (auraGO) auraGO.SetActive(false); });
            auraGO.transform.SetParent(root.transform, false);
            var aura = CreatePS(auraGO);
            var am = aura.main;
            am.duration = duration;
            am.loop = true;
            am.startLifetime = new ParticleSystem.MinMaxCurve(c.AuraLifetimeRange.x, c.AuraLifetimeRange.y);
            am.startSpeed = new ParticleSystem.MinMaxCurve(c.AuraSpeedRange.x, c.AuraSpeedRange.y);
            am.startSize = new ParticleSystem.MinMaxCurve(c.AuraSizeRange.x, c.AuraSizeRange.y);
            am.startColor = c.AuraColor;
            am.gravityModifier = c.AuraGravity;
            am.simulationSpace = ParticleSystemSimulationSpace.Local;

            var ae = aura.emission;
            ae.rateOverTime = c.AuraEmissionRate;

            var ash = aura.shape;
            ash.shapeType = ParticleSystemShapeType.Sphere;
            ash.radius = c.AuraRadius;

            var ac = aura.colorOverLifetime;
            ac.enabled = true;
            var ag = new Gradient();
            ag.SetKeys(
                new[] { new GradientColorKey(c.AuraColor, 0f), new GradientColorKey(c.AuraColor * 0.5f, 1f) },
                new[] { new GradientAlphaKey(c.AuraColor.a, 0f), new GradientAlphaKey(0f, 1f) }
            );
            ac.color = ag;

            var asol = aura.sizeOverLifetime;
            asol.enabled = true;
            asol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.8f), new Keyframe(0.5f, 1.2f), new Keyframe(1f, 0.3f)));

            SetParticleRenderer(auraGO, c.AuraColor);

            // Shadow wisps
            var wispsGO = new GameObject("ShadowWisps");
            DOVirtual.DelayedCall(duration, () => { if (wispsGO) wispsGO.SetActive(false); });
            wispsGO.transform.SetParent(root.transform, false);
            wispsGO.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            var wisps = CreatePS(wispsGO);
            var wm = wisps.main;
            wm.duration = duration;
            wm.loop = true;
            wm.startLifetime = new ParticleSystem.MinMaxCurve(c.WispsLifetimeRange.x, c.WispsLifetimeRange.y);
            wm.startSpeed = new ParticleSystem.MinMaxCurve(c.WispsSpeedRange.x, c.WispsSpeedRange.y);
            wm.startSize = new ParticleSystem.MinMaxCurve(c.WispsSizeRange.x, c.WispsSizeRange.y);
            wm.startColor = c.WispsColor;
            wm.gravityModifier = c.WispsGravity;
            wm.simulationSpace = ParticleSystemSimulationSpace.Local;

            var wse = wisps.emission;
            wse.rateOverTime = c.WispsEmissionRate;

            var wss = wisps.shape;
            wss.shapeType = ParticleSystemShapeType.Circle;
            wss.radius = c.WispsRadius;

            var wsc = wisps.colorOverLifetime;
            wsc.enabled = true;
            var wsg = new Gradient();
            wsg.SetKeys(
                new[] { new GradientColorKey(c.WispsColor, 0f), new GradientColorKey(c.WispsColor * 0.5f, 1f) },
                new[] { new GradientAlphaKey(c.WispsColor.a, 0f), new GradientAlphaKey(0f, 1f) }
            );
            wsc.color = wsg;

            SetParticleRenderer(wispsGO, c.WispsColor);

            // Activate burst
            var burstGO = new GameObject("CloakBurst");
            DOVirtual.DelayedCall(0.3f, () => { if (burstGO) burstGO.SetActive(false); });
            burstGO.transform.SetParent(root.transform, false);
            var burstPS = CreatePS(burstGO);
            var bm = burstPS.main;
            bm.duration = 0.3f;
            bm.loop = false;
            bm.startLifetime = 0.8f;
            bm.startSpeed = new ParticleSystem.MinMaxCurve(c.BurstSpeedRange.x, c.BurstSpeedRange.y);
            bm.startSize = new ParticleSystem.MinMaxCurve(c.BurstSizeRange.x, c.BurstSizeRange.y);
            bm.startColor = c.BurstColor;
            bm.simulationSpace = ParticleSystemSimulationSpace.World;

            var bbe = burstPS.emission;
            bbe.rateOverTime = 0;
            bbe.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)c.BurstCount) });

            var bbs = burstPS.shape;
            bbs.shapeType = ParticleSystemShapeType.Sphere;
            bbs.radius = 0.4f;

            SetParticleRenderer(burstGO, c.BurstColor);

            PlayAllPS(root);
            Destroy(root, duration + 2f);
            return root;
        }

        // ===================== GHOST VISION =====================

        private GameObject CreateGhostVisionVFX(float duration)
        {
            var c = _ghost;
            var root = new GameObject("VFX_GhostVision");
            root.transform.SetParent(GetAnchor(_fxTop, _fxCenter), false);
            root.transform.localPosition = Vector3.zero;

            // Eye glow
            var glowGO = new GameObject("EyeGlow");
            glowGO.transform.SetParent(root.transform, false);
            var glow = CreatePS(glowGO);
            var gm = glow.main;
            gm.duration = duration;
            gm.loop = true;
            gm.startLifetime = new ParticleSystem.MinMaxCurve(c.GlowLifetimeRange.x, c.GlowLifetimeRange.y);
            gm.startSpeed = 0.1f;
            gm.startSize = new ParticleSystem.MinMaxCurve(c.GlowSizeRange.x, c.GlowSizeRange.y);
            gm.startColor = c.GlowColorStart;
            gm.simulationSpace = ParticleSystemSimulationSpace.Local;

            var ge = glow.emission;
            ge.rateOverTime = c.GlowEmissionRate;

            var gs = glow.shape;
            gs.shapeType = ParticleSystemShapeType.Sphere;
            gs.radius = c.GlowRadius;

            var gc = glow.colorOverLifetime;
            gc.enabled = true;
            var gg = new Gradient();
            gg.SetKeys(
                new[] { new GradientColorKey(c.GlowColorStart, 0f), new GradientColorKey(c.GlowColorEnd, 1f) },
                new[] { new GradientAlphaKey(c.GlowColorStart.a, 0f), new GradientAlphaKey(0f, 1f) }
            );
            gc.color = gg;

            SetParticleRenderer(glowGO, c.GlowColorStart);

            // Scan pulse
            var pulseGO = new GameObject("ScanPulse");
            pulseGO.transform.SetParent(root.transform, false);
            pulseGO.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            var pulse = CreatePS(pulseGO);
            var pm = pulse.main;
            pm.duration = duration;
            pm.loop = true;
            pm.startLifetime = c.PulseLifetime;
            pm.startSpeed = c.PulseSpeed;
            pm.startSize = 0.1f;
            pm.startColor = c.PulseColor;
            pm.simulationSpace = ParticleSystemSimulationSpace.World;

            var pe = pulse.emission;
            pe.rateOverTime = 0;
            pe.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)c.PulseBurstCount, 1, c.PulseInterval) });

            var psh = pulse.shape;
            psh.shapeType = ParticleSystemShapeType.Circle;
            psh.radius = 0.1f;

            var pc = pulse.colorOverLifetime;
            pc.enabled = true;
            var pg = new Gradient();
            pg.SetKeys(
                new[] { new GradientColorKey(c.PulseColor, 0f), new GradientColorKey(c.PulseColor * 0.5f, 1f) },
                new[] { new GradientAlphaKey(c.PulseColor.a, 0f), new GradientAlphaKey(0f, 1f) }
            );
            pc.color = pg;

            var psol = pulse.sizeOverLifetime;
            psol.enabled = true;
            psol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f), new Keyframe(1f, 0.05f)));

            SetParticleRenderer(pulseGO, c.PulseColor);

            // Data particles
            var dataGO = new GameObject("DataParticles");
            dataGO.transform.SetParent(root.transform, false);
            dataGO.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            var data = CreatePS(dataGO);
            var dm = data.main;
            dm.duration = duration;
            dm.loop = true;
            dm.startLifetime = new ParticleSystem.MinMaxCurve(c.DataLifetimeRange.x, c.DataLifetimeRange.y);
            dm.startSpeed = new ParticleSystem.MinMaxCurve(c.DataSpeedRange.x, c.DataSpeedRange.y);
            dm.startSize = new ParticleSystem.MinMaxCurve(c.DataSizeRange.x, c.DataSizeRange.y);
            dm.startColor = c.DataColor;
            dm.gravityModifier = c.DataGravity;
            dm.simulationSpace = ParticleSystemSimulationSpace.Local;

            var de = data.emission;
            de.rateOverTime = c.DataEmissionRate;

            var ds = data.shape;
            ds.shapeType = ParticleSystemShapeType.Box;
            ds.scale = new Vector3(1f, 1.5f, 1f);

            SetParticleRenderer(dataGO, c.DataColor);

            PlayAllPS(root);
            Destroy(root, duration + 2f);
            return root;
        }

        // ===================== HELPERS =====================

        private ParticleSystem CreatePS(GameObject go)
        {
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        private void PlayAllPS(GameObject root)
        {
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
                ps.Play(true);
        }

        private void SetParticleRenderer(GameObject go, Color color)
        {
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer == null) return;

            if (renderer.renderMode != ParticleSystemRenderMode.Stretch)
                renderer.renderMode = ParticleSystemRenderMode.Billboard;

            renderer.material = new Material(Shader.Find("Particles/Standard Unlit")) { color = color };
            renderer.material.SetFloat("_Mode", 3f);
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        }
    }
}

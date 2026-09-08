namespace LumenFX.Core
{
    /// <summary>
    /// v2 tuning state. Parameter names, ranges and defaults are owned by
    /// this version and are stored/loaded as a whole.
    /// </summary>
    public class LightState
    {
        public float SunStrength = 1f;      // 0..2
        public float MoonStrength = 1f;     // 0..2
        public float Ambience = 1f;         // 0..2
        public float Warmth = 0f;           // -1..1, cold..warm
        public bool SkyTonemapping = true;
        public int ToneEnabled = -1;
        public bool LegacySceneLighting;
        public float LegacySceneSunMultiplier = 1f;
        public float LegacySceneWarmth;

        // Advanced per-source color controls (own semantics: positive temp =
        // warmer source, positive tint = greener source).
        public float SunTemp = 0f;          // -1..1
        public float SunTint = 0f;          // -1..1
        public float MoonTemp = 0f;         // -1..1
        public float MoonTint = 0f;         // -1..1
        public float SkyTemp = 0f;          // -1..1 (applied to ambient light)
        public float SkyTint = 0f;          // -1..1 (applied to ambient light)
        public float GlobalTint = 0f;       // -1..1
        public float TwilightTint = 0f;     // -1..1 (weighted around dawn/dusk)

        public float Brightness = 0f;       // -1..4 (por encima de 1, tramo ancho)
        public float Contrast = 0f;         // -1..1
        public float Gamma = 2.2f;          // 1.5..3.5
        public bool AdaptiveShadows = true;
        public bool ForceLowBias;           // keep shadows tight on the ground
        public float BiasScale = 1f;        // 0..2, scales the adaptive bias curve
        public bool SoftShadows = true;
        public bool AdaptiveExposure = false; // own day/night exposure compensation
        public float AdaptiveExposureGain = 0.5f; // 0..1
        public float SkyExposure;          // 0 = captured map exposure
        public float SkyRayleigh;           // absolute m_RayleighScattering, 0 = keep the map theme value
        public float SkyMie;                // absolute m_MieScattering, 0 = keep the map theme value
        public float SunPower;              // absolute m_SunIntensity, 0 = keep game value
        public float MoonPower;             // absolute m_MoonIntensity, 0 = keep game value
        public float WindowX = 620f;
        public float WindowY = 300f;
        public bool VanillaMode = true;            // suspend everything, game untouched

        public bool LightingDirty = true;

        /// <summary>
        /// Neutral state: no gains, no color shifts, standard tone.
        /// </summary>
        internal void ResetToNeutral()
        {
            SunStrength = 1f;
            MoonStrength = 1f;
            Ambience = 1f;
            Warmth = 0f;
            SunTemp = 0f;
            SunTint = 0f;
            MoonTemp = 0f;
            MoonTint = 0f;
            SkyTemp = 0f;
            SkyTint = 0f;
            GlobalTint = 0f;
            TwilightTint = 0f;
            SkyTonemapping = true;
            ToneEnabled = -1; LegacySceneLighting = false; LegacySceneSunMultiplier = 1f; LegacySceneWarmth = 0f;
            Brightness = 0f;
            Contrast = 0f;
            Gamma = 2.2f;
            AdaptiveShadows = true;
            ForceLowBias = false;
            BiasScale = 1f;
            SoftShadows = true;
            AdaptiveExposure = false;
            AdaptiveExposureGain = 0.5f;
            SkyExposure = 0f;
            SkyRayleigh = 0f;
            SkyMie = 0f;
            SunPower = 0f;
            MoonPower = 0f;
            LightingDirty = true;
        }
    }
}

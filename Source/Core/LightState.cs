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

        public float Brightness = 0f;       // -1..1
        public float Contrast = 0f;         // -1..1
        public float Gamma = 2.6f;          // 1.5..3.5
        public bool AdaptiveShadows = true;
        public bool ForceLowBias;           // keep shadows tight on the ground
        public float BiasScale = 1f;        // 0..2, scales the adaptive bias curve
        public bool SoftShadows = true;
        public bool VanillaMode;            // suspend everything, game untouched

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
            Brightness = 0f;
            Contrast = 0f;
            Gamma = 2.2f;
            AdaptiveShadows = true;
            ForceLowBias = false;
            BiasScale = 1f;
            SoftShadows = true;
        }
    }
}

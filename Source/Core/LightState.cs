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

        public float Brightness = 0f;       // -1..1
        public float Contrast = 0f;         // -1..1
        public float Gamma = 2.6f;          // 1.5..3.5
        public bool AdaptiveShadows = true;
        public float BiasScale = 1f;        // 0..2
        public bool SoftShadows = true;
    }
}

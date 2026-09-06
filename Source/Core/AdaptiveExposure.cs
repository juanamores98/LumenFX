using UnityEngine;

namespace LumenFX.Core
{
    /// <summary>
    /// Own analytic day/night exposure compensation model.
    /// Evaluates a smooth time-of-day curve to lift dark nights and maintain
    /// balanced contrast during transitions without expensive per-frame HDR histogram readbacks.
    /// </summary>
    public static class AdaptiveExposure
    {
        private static float _lastNormalizedTime = -999f;
        private static float _lastGain = -999f;
        private static float _cachedExposureMultiplier = 1f;

        public static void Reset()
        {
            _lastNormalizedTime = -999f;
            _lastGain = -999f;
            _cachedExposureMultiplier = 1f;
        }

        /// <summary>
        /// Computes a smooth multiplier for DayNightProperties.m_Exposure based on the current
        /// normalized time of day [0..1] and the configured adaptive gain [0..1].
        /// </summary>
        public static float Compute(float normalizedTime, float gain)
        {
            if (gain <= 0.001f)
            {
                return 1f;
            }

            // Recalculate only if time has progressed noticeably or gain changed
            if (Mathf.Abs(normalizedTime - _lastNormalizedTime) < 0.002f &&
                Mathf.Abs(gain - _lastGain) < 0.001f)
            {
                return _cachedExposureMultiplier;
            }

            _lastNormalizedTime = normalizedTime;
            _lastGain = gain;

            // Distance from midday (0.5). At midday distFromNoon = 0, at midnight distFromNoon = 0.5.
            float distFromNoon = Mathf.Abs(normalizedTime - 0.5f); // 0 (noon) .. 0.5 (midnight)
            float nightWeight = Mathf.Clamp01((distFromNoon - 0.20f) / 0.25f); // 0 during day, smooth step to 1 at night

            // Smooth hermite curve
            nightWeight = nightWeight * nightWeight * (3f - 2f * nightWeight);

            // Night lifts exposure smoothly by up to +40% with gain=1
            float multiplier = 1f + 0.40f * gain * nightWeight;

            _cachedExposureMultiplier = multiplier;
            return multiplier;
        }
    }
}

using System.Reflection;
using UnityEngine;

namespace LumenFX.Core
{
    /// <summary>
    /// v2 lighting model. Instead of authoring a fixed palette, it resamples
    /// the game's own day/night gradients at a set of key times, scales the
    /// daylight and night zones independently, applies a warmth shift and
    /// per-source temperature/tint offsets with its own response weights.
    /// The result is written back into the game gradients.
    /// </summary>
    internal static class LightingMixer
    {
        private static readonly float[] KeyTimes = { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f };

        private const float DawnStart = 0.20f;
        private const float DawnEnd = 0.32f;
        private const float DuskStart = 0.68f;
        private const float DuskEnd = 0.80f;

        internal static void Apply(LightState state)
        {
            var dayNight = Object.FindObjectOfType<DayNightProperties>();
            if (dayNight == null)
            {
                return;
            }

            dayNight.m_LightColor = Resample(dayNight.m_LightColor, state, true);

            var ambientType = typeof(DayNightProperties.AmbientColor);
            var ambient = dayNight.m_AmbientColor;
            ambientType.GetField("m_SkyColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ambient, Resample(GetAmbient(ambientType, ambient, "m_SkyColor"), state, false));
            ambientType.GetField("m_EquatorColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ambient, Resample(GetAmbient(ambientType, ambient, "m_EquatorColor"), state, false));
            ambientType.GetField("m_GroundColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ambient, Resample(GetAmbient(ambientType, ambient, "m_GroundColor"), state, false));

            dayNight.m_Tonemapping = state.SkyTonemapping;
        }

        private static Gradient GetAmbient(System.Type type, object instance, string field)
        {
            return (Gradient)type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
        }

        private static Gradient Resample(Gradient source, LightState state, bool isDirectLight)
        {
            if (source == null)
            {
                return null;
            }

            var keys = new GradientColorKey[KeyTimes.Length];
            for (int i = 0; i < KeyTimes.Length; i++)
            {
                float time = KeyTimes[i];
                Color sampled = source.Evaluate(time);

                float gain = isDirectLight ? ZoneGain(time, state) : state.Ambience;
                float warmth = isDirectLight ? state.Warmth : state.Warmth * 0.5f;

                // Own per-source offsets. Positive temperature warms a source
                // (red up, blue down); positive tint pushes green; the
                // twilight tint is weighted around dawn and dusk only.
                float tempShift;
                float tintShift;
                if (isDirectLight)
                {
                    float dayness = Dayness(time);
                    tempShift = 0.10f * (state.SunTemp * dayness - state.MoonTemp * (1f - dayness));
                    tintShift = 0.06f * (state.SunTint * dayness + state.MoonTint * (1f - dayness));
                    tempShift += 0.12f * state.TwilightTint * TwilightBand(time);
                }
                else
                {
                    tempShift = 0.08f * state.SkyTemp;
                    tintShift = 0.05f * state.SkyTint;
                }

                tintShift += 0.05f * state.GlobalTint;

                sampled.r = Mathf.Clamp01(sampled.r * gain * (1f + 0.18f * warmth) + tempShift);
                sampled.g = Mathf.Clamp01(sampled.g * gain + tintShift);
                sampled.b = Mathf.Clamp01(sampled.b * gain * (1f - 0.18f * warmth) - tempShift);
                sampled.a = 1f;

                keys[i] = new GradientColorKey(sampled, time);
            }

            var result = new Gradient
            {
                colorKeys = keys,
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f),
                },
            };
            return result;
        }

        /// <summary>
        /// Blends the moon and sun strengths across the transition bands so
        /// the change of zone is smooth instead of a hard switch.
        /// </summary>
        private static float ZoneGain(float time, LightState state)
        {
            return Mathf.Lerp(state.MoonStrength, state.SunStrength, Dayness(time));
        }

        private static float Dayness(float time)
        {
            if (time <= DawnStart)
            {
                return 0f;
            }

            if (time < DawnEnd)
            {
                return (time - DawnStart) / (DawnEnd - DawnStart);
            }

            if (time <= DuskStart)
            {
                return 1f;
            }

            if (time < DuskEnd)
            {
                return 1f - (time - DuskStart) / (DuskEnd - DuskStart);
            }

            return 0f;
        }

        /// <summary>
        /// Peaks at the dawn and dusk key times; used to weight the twilight
        /// tint so it only affects the transition bands.
        /// </summary>
        private static float TwilightBand(float time)
        {
            float dawn = 1f - Mathf.Abs(time - 0.27f) / 0.09f;
            float dusk = 1f - Mathf.Abs(time - 0.73f) / 0.09f;
            return Mathf.Clamp01(Mathf.Max(dawn, dusk));
        }
    }
}

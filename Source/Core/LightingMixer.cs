using System.Reflection;
using UnityEngine;

namespace LumenFX.Core
{
    /// <summary>
    /// v2 lighting model. Instead of authoring a fixed palette, it resamples
    /// the game's own day/night gradients at a set of key times, scales the
    /// daylight and night zones independently and applies a warmth shift to
    /// every channel. The result is written back into the game gradients.
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

                sampled.r = Mathf.Clamp01(sampled.r * gain * (1f + 0.18f * warmth));
                sampled.g = Mathf.Clamp01(sampled.g * gain);
                sampled.b = Mathf.Clamp01(sampled.b * gain * (1f - 0.18f * warmth));
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
            float dayness;
            if (time <= DawnStart)
            {
                dayness = 0f;
            }
            else if (time < DawnEnd)
            {
                dayness = (time - DawnStart) / (DawnEnd - DawnStart);
            }
            else if (time <= DuskStart)
            {
                dayness = 1f;
            }
            else if (time < DuskEnd)
            {
                dayness = 1f - (time - DuskStart) / (DuskEnd - DuskStart);
            }
            else
            {
                dayness = 0f;
            }

            return Mathf.Lerp(state.MoonStrength, state.SunStrength, dayness);
        }
    }
}

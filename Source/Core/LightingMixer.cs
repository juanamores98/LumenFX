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
        /// <summary>
        /// Los instantes en que se remuestrea la curva del dia.
        /// </summary>
        /// <remarks>
        /// <b>Son los del propio juego.</b> La gradiente que trae <c>DayNightProperties</c>
        /// tiene siete claves en 0.23, 0.26, 0.32, 0.5, 0.68, 0.74 y 0.77, y fuera de ese
        /// tramo mantiene el color del extremo. Copiar esa colocacion conserva la forma
        /// original de la curva en vez de imponerle una reticula uniforme.
        ///
        /// <b>Y no pueden ser mas de ocho.</b> Una <c>Gradient</c> de Unity admite ocho claves
        /// de color como maximo. Una version anterior de este arreglo puso nueve, y el
        /// resultado no fue que sobrara una: la gradiente entera degeneraba en dos claves
        /// blancas, o sea el sol plano y sin color a cualquier hora. Se midio en partida.
        /// Si alguna vez hay que anadir instantes, hay que quitar otros.
        /// </remarks>
        private const int MaxGradientKeys = 8;

        private static readonly float[] KeyTimes =
            { 0.23f, 0.26f, 0.32f, 0.5f, 0.68f, 0.74f, 0.77f };

        private const float DawnStart = 0.20f;
        private const float DawnEnd = 0.32f;
        private const float DuskStart = 0.68f;
        private const float DuskEnd = 0.80f;

        private static FieldInfo _skyColorField;
        private static FieldInfo _equatorColorField;
        private static FieldInfo _groundColorField;
        private static DayNightProperties _cachedDayNight;
        private static bool _exposureWritten;

        public static void ClearCache()
        {
            _cachedDayNight = null;
            _exposureWritten = false;
            Runtime.TunerRuntime.CurrentState.LightingDirty = true;
        }

        private static void EnsureFields()
        {
            if (_skyColorField != null)
            {
                return;
            }

            var ambientType = typeof(DayNightProperties.AmbientColor);
            _skyColorField = ambientType.GetField("m_SkyColor", BindingFlags.Instance | BindingFlags.NonPublic);
            _equatorColorField = ambientType.GetField("m_EquatorColor", BindingFlags.Instance | BindingFlags.NonPublic);
            _groundColorField = ambientType.GetField("m_GroundColor", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        internal static void Apply(LightState state)
        {
            if (_cachedDayNight == null)
            {
                _cachedDayNight = Object.FindObjectOfType<DayNightProperties>();
                if (_cachedDayNight == null)
                {
                    return;
                }
            }

            VanillaSnapshot.Capture();
            _cachedDayNight.m_Tonemapping = state.SkyTonemapping;
            if (state.SkyExposure > 0f)
            {
                _cachedDayNight.m_Exposure = state.SkyExposure;
                _exposureWritten = true;
            }
            else if (_exposureWritten)
            {
                if (VanillaSnapshot.Captured && !state.AdaptiveExposure && !ThemeOwnership.AtmosphereIsManaged)
                    _cachedDayNight.m_Exposure = VanillaSnapshot.CapturedExposure;
                _exposureWritten = false;
            }

            // Los cuatro ejes absolutos. Cero significa "el valor del juego o del tema del
            // mapa", y para que eso sea reversible hay que reponerlo desde lo capturado: dejar
            // de escribir no basta, el campo conservaria el ultimo valor puesto hasta recargar.
            VanillaSnapshot.Capture();

            // La dispersión del cielo también la administra el gestor de temas, pero eso no
            // impide escribirla: un valor que el usuario pide expresamente —su receta— se
            // aplica, igual que hacía Render It!+ conviviendo con Theme Mixer. Lo que no se
            // hace nunca es devolver la línea base capturada, porque se capturó antes de que
            // el tema aplicara la suya y devolverla lo borraría.
            if (state.SkyRayleigh > 0f)
            {
                _cachedDayNight.m_RayleighScattering = Mathf.Clamp(state.SkyRayleigh, 0.01f, 5f);
            }
            else if (VanillaSnapshot.Captured && !ThemeOwnership.AtmosphereIsManaged)
            {
                _cachedDayNight.m_RayleighScattering = VanillaSnapshot.CapturedRayleigh;
            }

            if (state.SkyMie > 0f)
            {
                _cachedDayNight.m_MieScattering = Mathf.Clamp(state.SkyMie, 0.01f, 5f);
            }
            else if (VanillaSnapshot.Captured && !ThemeOwnership.AtmosphereIsManaged)
            {
                _cachedDayNight.m_MieScattering = VanillaSnapshot.CapturedMie;
            }

            if (state.SunPower > 0f)
            {
                _cachedDayNight.m_SunIntensity = Mathf.Clamp(state.SunPower, 0f, 20f);
            }
            else if (VanillaSnapshot.Captured)
            {
                _cachedDayNight.m_SunIntensity = VanillaSnapshot.CapturedSunIntensity;
            }

            if (state.MoonPower > 0f)
            {
                _cachedDayNight.m_MoonIntensity = Mathf.Clamp(state.MoonPower, 0f, 20f);
            }
            else if (VanillaSnapshot.Captured)
            {
                _cachedDayNight.m_MoonIntensity = VanillaSnapshot.CapturedMoonIntensity;
            }

            if (!state.LightingDirty)
            {
                return;
            }

            EnsureFields();
            VanillaSnapshot.Capture();

            Gradient sourceDirect = VanillaSnapshot.CapturedDirect;
            if (sourceDirect != null)
            {
                _cachedDayNight.m_LightColor = Resample(sourceDirect, state, true);
            }

            var ambient = _cachedDayNight.m_AmbientColor;
            Gradient sourceSky = VanillaSnapshot.CapturedSky;
            if (_skyColorField != null && sourceSky != null)
            {
                _skyColorField.SetValue(ambient, Resample(sourceSky, state, false));
            }

            Gradient sourceEquator = VanillaSnapshot.CapturedEquator;
            if (_equatorColorField != null && sourceEquator != null)
            {
                _equatorColorField.SetValue(ambient, Resample(sourceEquator, state, false));
            }

            Gradient sourceGround = VanillaSnapshot.CapturedGround;
            if (_groundColorField != null && sourceGround != null)
            {
                _groundColorField.SetValue(ambient, Resample(sourceGround, state, false));
            }

            state.LightingDirty = false;
        }


        private static Gradient Resample(Gradient source, LightState state, bool isDirectLight)
        {
            if (source == null)
            {
                return null;
            }

            // Guarda explicita: si alguien vuelve a alargar KeyTimes, es mejor perder los
            // instantes sobrantes que perder la gradiente entera.
            int count = Mathf.Min(KeyTimes.Length, MaxGradientKeys);
            var keys = new GradientColorKey[count];
            for (int i = 0; i < count; i++)
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

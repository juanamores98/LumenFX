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
        private static bool _classicColor;
        private static Gradient _sourceDirect;

        public static void ClearCache()
        {
            _cachedDayNight = null;
            _classicColor = false; _sourceDirect = null;
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
            Infrastructure.PropertyLedger.Write(_cachedDayNight, "m_Tonemapping", state.SkyTonemapping);
            bool classicPower = Infrastructure.FxInterop.ClassicRequest("sunStrength");
            if (classicPower || !state.AdaptiveExposure) Absolute("m_Exposure", classicPower ? 1f : state.SkyExposure);
            Absolute("m_RayleighScattering", state.SkyRayleigh);
            Absolute("m_MieScattering", state.SkyMie);
            WaveLengths(state);
            if (!classicPower && state.LegacySceneLighting)
                Infrastructure.PropertyLedger.Write(_cachedDayNight, "m_SunIntensity", Infrastructure.PropertyLedger.Baseline<float>(_cachedDayNight, "m_SunIntensity") * state.LegacySceneSunMultiplier);
            else Absolute("m_SunIntensity", classicPower ? 3.318695f : state.SunPower);
            Absolute("m_MoonIntensity", state.MoonPower);

            bool classicColor = Infrastructure.FxInterop.ClassicRequest("sunColor");
            if (classicColor != _classicColor) { state.LightingDirty = true; _classicColor = classicColor; }
            var liveSource = Infrastructure.PropertyLedger.Baseline<Gradient>(_cachedDayNight, "m_LightColor");
            if (!object.ReferenceEquals(liveSource, _sourceDirect)) { _sourceDirect = liveSource; state.LightingDirty = true; }
            if (!state.LightingDirty)
            {
                return;
            }

            EnsureFields();
            VanillaSnapshot.Capture();

            Gradient sourceDirect = Infrastructure.PropertyLedger.Baseline<Gradient>(_cachedDayNight, "m_LightColor");
            if (sourceDirect != null)
            {
                Infrastructure.PropertyLedger.Write(_cachedDayNight, "m_LightColor", classicColor ? ClassicCurve(sourceDirect) : state.LegacySceneLighting ? LegacyCurve(sourceDirect, state.LegacySceneWarmth) : Resample(sourceDirect, state, true));
            }

            var ambient = _cachedDayNight.m_AmbientColor;
            Gradient sourceSky = Infrastructure.PropertyLedger.Baseline<Gradient>(ambient, "m_SkyColor");
            if (_skyColorField != null && sourceSky != null)
            {
                Infrastructure.PropertyLedger.Write(ambient, "m_SkyColor", Resample(sourceSky, state, false));
            }

            Gradient sourceEquator = Infrastructure.PropertyLedger.Baseline<Gradient>(ambient, "m_EquatorColor");
            if (_equatorColorField != null && sourceEquator != null)
            {
                Infrastructure.PropertyLedger.Write(ambient, "m_EquatorColor", Resample(sourceEquator, state, false));
            }

            Gradient sourceGround = Infrastructure.PropertyLedger.Baseline<Gradient>(ambient, "m_GroundColor");
            if (_groundColorField != null && sourceGround != null)
            {
                Infrastructure.PropertyLedger.Write(ambient, "m_GroundColor", Resample(sourceGround, state, false));
            }

            state.LightingDirty = false;
        }


        private static Gradient LegacyCurve(Gradient source, float warmth)
        {
            var keys = source.colorKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                var c = keys[i].color;
                c.r = Mathf.Clamp01(c.r * (1f + 0.15f * warmth));
                c.b = Mathf.Clamp01(c.b * (1f - 0.15f * warmth));
                keys[i] = new GradientColorKey(c, keys[i].time);
            }
            return new Gradient { colorKeys = keys, alphaKeys = source.alphaKeys };
        }

        private static Gradient ClassicCurve(Gradient source)
        {
            var keys = source.colorKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                float daylight = Mathf.Clamp01(1f - Mathf.Abs(keys[i].time - 0.5f) * 4f);
                keys[i] = new GradientColorKey(Color.Lerp(keys[i].color, Color.white, daylight * 0.35f), keys[i].time);
            }
            return new Gradient { colorKeys = keys, alphaKeys = source.alphaKeys };
        }

        /// <summary>
        /// Las longitudes de onda del cielo, canal a canal.
        /// </summary>
        /// <remarks>
        /// Cada canal en 0 significa «el del mapa», asi que se puede tocar solo el rojo y
        /// dejar los otros dos como estaban. Es la forma en que Render It! gobierna el color
        /// del cielo, y la unica de las cuatro que la suite no ofrecia.
        ///
        /// Con el tinte clasico pedido no se toca el campo: ese modo es un degradado sobre la
        /// hora que no admite otra forma, y quien lo pide manda.
        /// </remarks>
        private static void WaveLengths(LightState state)
        {
            if (Infrastructure.FxInterop.ClassicRequest("fogTint"))
            {
                return;
            }

            if (state.SkyWaveR <= 0f && state.SkyWaveG <= 0f && state.SkyWaveB <= 0f)
            {
                Infrastructure.PropertyLedger.Release(_cachedDayNight, "m_WaveLengths");
                return;
            }

            Vector3 map = Infrastructure.PropertyLedger.Baseline<Vector3>(_cachedDayNight, "m_WaveLengths");
            Infrastructure.PropertyLedger.Write(_cachedDayNight, "m_WaveLengths", new Vector3(
                state.SkyWaveR > 0f ? state.SkyWaveR : map.x,
                state.SkyWaveG > 0f ? state.SkyWaveG : map.y,
                state.SkyWaveB > 0f ? state.SkyWaveB : map.z));
        }

        private static void Absolute(string field, float value)
        {
            if (value > 0f) Infrastructure.PropertyLedger.Write(_cachedDayNight, field, value);
            else Infrastructure.PropertyLedger.Release(_cachedDayNight, field);
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

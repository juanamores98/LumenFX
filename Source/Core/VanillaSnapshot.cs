using System.Reflection;
using UnityEngine;

namespace LumenFX.Core
{
    /// <summary>
    /// Captures the game's untouched lighting, tonemapping and shadow state
    /// before the first write, so "vanilla mode" can return everything to the
    /// exact values the game shipped with.
    /// </summary>
    internal static class VanillaSnapshot
    {
        private static bool _captured;

        private static Gradient _direct;
        private static Gradient _sky;
        private static Gradient _equator;
        private static Gradient _ground;

        private static float _gamma;
        private static float _boost;
        private static float _luminance;
        private static float _exposure = 1f;
        private static float _filmicA;
        private static float _filmicB;
        private static float _filmicC;
        private static float _filmicD;
        private static float _filmicE;
        private static float _filmicF;
        private static float _filmicW;

        // Ejes absolutos del cielo y de las fuentes. Se capturan porque los controles
        // que los escriben usan 0 como "no tocar": sin un valor de partida guardado, ese 0
        // solo dejaria de escribir y el campo se quedaria con lo ultimo que se puso.
        private static float _rayleigh;
        private static float _mie;
        private static float _sunIntensity;
        private static float _moonIntensity;

        private static int _shadowQuality;

        internal static bool Captured
        {
            get { return _captured; }
        }

        internal static float CapturedExposure
        {
            get { return _exposure; }
        }

        internal static Gradient CapturedDirect
        {
            get { return _direct; }
        }

        internal static Gradient CapturedSky
        {
            get { return _sky; }
        }

        internal static Gradient CapturedEquator
        {
            get { return _equator; }
        }

        internal static Gradient CapturedGround
        {
            get { return _ground; }
        }

        internal static float CapturedRayleigh
        {
            get { return _rayleigh; }
        }

        internal static float CapturedMie
        {
            get { return _mie; }
        }

        internal static float CapturedSunIntensity
        {
            get { return _sunIntensity; }
        }

        internal static float CapturedMoonIntensity
        {
            get { return _moonIntensity; }
        }

        internal static void ResetCapture()
        {
            _captured = false;
            _direct = null;
            _sky = null;
            _equator = null;
            _ground = null;
        }

        internal static void Capture()
        {
            if (_captured)
            {
                return;
            }

            var dayNight = Object.FindObjectOfType<DayNightProperties>();
            if (dayNight == null)
            {
                return;
            }

            _direct = dayNight.m_LightColor;
            _exposure = dayNight.m_Exposure;
            _rayleigh = dayNight.m_RayleighScattering;
            _mie = dayNight.m_MieScattering;
            _sunIntensity = dayNight.m_SunIntensity;
            _moonIntensity = dayNight.m_MoonIntensity;

            var ambientType = typeof(DayNightProperties.AmbientColor);
            var ambient = dayNight.m_AmbientColor;
            _sky = (Gradient)ambientType.GetField("m_SkyColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ambient);
            _equator = (Gradient)ambientType.GetField("m_EquatorColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ambient);
            _ground = (Gradient)ambientType.GetField("m_GroundColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ambient);

            var cameraObject = GameObject.Find("Main Camera");
            var toneMap = cameraObject != null ? cameraObject.GetComponent<ColossalFramework.ToneMapping>() : null;
            if (toneMap != null)
            {
                _gamma = toneMap.m_ToneMappingGamma;
                _boost = toneMap.m_ToneMappingBoostFactor;
                _luminance = toneMap.m_Luminance;
                _filmicA = toneMap.m_ToneMappingParamsFilmic.A;
                _filmicB = toneMap.m_ToneMappingParamsFilmic.B;
                _filmicC = toneMap.m_ToneMappingParamsFilmic.C;
                _filmicD = toneMap.m_ToneMappingParamsFilmic.D;
                _filmicE = toneMap.m_ToneMappingParamsFilmic.E;
                _filmicF = toneMap.m_ToneMappingParamsFilmic.F;
                _filmicW = toneMap.m_ToneMappingParamsFilmic.W;
            }

            _shadowQuality = (int)QualitySettings.shadows;
            _captured = true;
        }

        internal static void Restore()
        {
            if (!_captured)
            {
                return;
            }

            var dayNight = Object.FindObjectOfType<DayNightProperties>();
            if (dayNight != null)
            {
                dayNight.m_Exposure = _exposure;
                dayNight.m_RayleighScattering = _rayleigh;
                dayNight.m_MieScattering = _mie;
                dayNight.m_SunIntensity = _sunIntensity;
                dayNight.m_MoonIntensity = _moonIntensity;

                if (_direct != null)
                {
                    dayNight.m_LightColor = _direct;
                }

                var ambientType = typeof(DayNightProperties.AmbientColor);
                var ambient = dayNight.m_AmbientColor;
                if (_sky != null)
                {
                    ambientType.GetField("m_SkyColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ambient, _sky);
                }

                if (_equator != null)
                {
                    ambientType.GetField("m_EquatorColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ambient, _equator);
                }

                if (_ground != null)
                {
                    ambientType.GetField("m_GroundColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ambient, _ground);
                }
            }

            var cameraObject = GameObject.Find("Main Camera");
            var toneMap = cameraObject != null ? cameraObject.GetComponent<ColossalFramework.ToneMapping>() : null;
            if (toneMap != null)
            {
                toneMap.m_ToneMappingGamma = _gamma;
                toneMap.m_ToneMappingBoostFactor = _boost;
                toneMap.m_Luminance = _luminance;
                toneMap.m_ToneMappingParamsFilmic.A = _filmicA;
                toneMap.m_ToneMappingParamsFilmic.B = _filmicB;
                toneMap.m_ToneMappingParamsFilmic.C = _filmicC;
                toneMap.m_ToneMappingParamsFilmic.D = _filmicD;
                toneMap.m_ToneMappingParamsFilmic.E = _filmicE;
                toneMap.m_ToneMappingParamsFilmic.F = _filmicF;
                toneMap.m_ToneMappingParamsFilmic.W = _filmicW;
            }

            QualitySettings.shadows = (ShadowQuality)_shadowQuality;
        }
    }
}

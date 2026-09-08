using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using ColossalFramework.IO;
using LumenFX.Core;

namespace LumenFX.IO
{
    /// <summary>
    /// v2 state document and persistence. The schema belongs to this version.
    /// </summary>
    [XmlRoot(ElementName = "lumenFx", Namespace = "", IsNullable = false)]
    public class StateDocument
    {
        [XmlAttribute("schema")]
        public int Schema = 2;

        private float _SunStrength = Runtime.TunerRuntime.CurrentState.SunStrength;
        [XmlElement("sunStrength")] public float SunStrength { get => _SunStrength; set => _SunStrength = Clamp(value, 0f, 3f); }
        private float _MoonStrength = Runtime.TunerRuntime.CurrentState.MoonStrength;
        [XmlElement("moonStrength")] public float MoonStrength { get => _MoonStrength; set => _MoonStrength = Clamp(value, 0f, 2f); }
        private float _Ambience = Runtime.TunerRuntime.CurrentState.Ambience;
        [XmlElement("ambience")] public float Ambience { get => _Ambience; set => _Ambience = Clamp(value, 0f, 2f); }
        private float _Warmth = Runtime.TunerRuntime.CurrentState.Warmth;
        [XmlElement("warmth")] public float Warmth { get => _Warmth; set => _Warmth = Clamp(value, -1f, 1f); }
        private float _SunTemp = Runtime.TunerRuntime.CurrentState.SunTemp;
        [XmlElement("sunTemp")] public float SunTemp { get => _SunTemp; set => _SunTemp = Clamp(value, -1f, 1f); }
        private float _SunTint = Runtime.TunerRuntime.CurrentState.SunTint;
        [XmlElement("sunTint")] public float SunTint { get => _SunTint; set => _SunTint = Clamp(value, -1f, 1f); }
        private float _MoonTemp = Runtime.TunerRuntime.CurrentState.MoonTemp;
        [XmlElement("moonTemp")] public float MoonTemp { get => _MoonTemp; set => _MoonTemp = Clamp(value, -1f, 1f); }
        private float _MoonTint = Runtime.TunerRuntime.CurrentState.MoonTint;
        [XmlElement("moonTint")] public float MoonTint { get => _MoonTint; set => _MoonTint = Clamp(value, -1f, 1f); }
        private float _SkyTemp = Runtime.TunerRuntime.CurrentState.SkyTemp;
        [XmlElement("skyTemp")] public float SkyTemp { get => _SkyTemp; set => _SkyTemp = Clamp(value, -1f, 1f); }
        private float _SkyTint = Runtime.TunerRuntime.CurrentState.SkyTint;
        [XmlElement("skyTint")] public float SkyTint { get => _SkyTint; set => _SkyTint = Clamp(value, -1f, 1f); }
        private float _GlobalTint = Runtime.TunerRuntime.CurrentState.GlobalTint;
        [XmlElement("globalTint")] public float GlobalTint { get => _GlobalTint; set => _GlobalTint = Clamp(value, -1f, 1f); }
        private float _TwilightTint = Runtime.TunerRuntime.CurrentState.TwilightTint;
        [XmlElement("twilightTint")] public float TwilightTint { get => _TwilightTint; set => _TwilightTint = Clamp(value, -1f, 1f); }
        private bool _SkyTonemapping = Runtime.TunerRuntime.CurrentState.SkyTonemapping;
        [XmlElement("skyTonemapping")] public bool SkyTonemapping { get => _SkyTonemapping; set => _SkyTonemapping = value; }
        private float _Brightness = Runtime.TunerRuntime.CurrentState.Brightness;
        [XmlElement("brightness")] public float Brightness { get => _Brightness; set => _Brightness = Clamp(value, -1f, 4f); }
        private float _Contrast = Runtime.TunerRuntime.CurrentState.Contrast;
        [XmlElement("contrast")] public float Contrast { get => _Contrast; set => _Contrast = Clamp(value, -1f, 1f); }
        private float _Gamma = Runtime.TunerRuntime.CurrentState.Gamma;
        [XmlElement("gamma")] public float Gamma { get => _Gamma; set => _Gamma = Clamp(value, 1.5f, 3.5f); }
        private bool _AdaptiveShadows = Runtime.TunerRuntime.CurrentState.AdaptiveShadows;
        [XmlElement("adaptiveShadows")] public bool AdaptiveShadows { get => _AdaptiveShadows; set => _AdaptiveShadows = value; }
        private bool _ForceLowBias = Runtime.TunerRuntime.CurrentState.ForceLowBias;
        [XmlElement("forceLowBias")] public bool ForceLowBias { get => _ForceLowBias; set => _ForceLowBias = value; }
        private float _BiasScale = Runtime.TunerRuntime.CurrentState.BiasScale;
        [XmlElement("biasScale")] public float BiasScale { get => _BiasScale; set => _BiasScale = Clamp(value, 0f, 2f); }
        private bool _SoftShadows = Runtime.TunerRuntime.CurrentState.SoftShadows;
        [XmlElement("softShadows")] public bool SoftShadows { get => _SoftShadows; set => _SoftShadows = value; }
        private bool _AdaptiveExposure = Runtime.TunerRuntime.CurrentState.AdaptiveExposure;
        [XmlElement("adaptiveExposure")] public bool AdaptiveExposure { get => _AdaptiveExposure; set => _AdaptiveExposure = value; }
        private float _AdaptiveExposureGain = Runtime.TunerRuntime.CurrentState.AdaptiveExposureGain;
        [XmlElement("adaptiveExposureGain")] public float AdaptiveExposureGain { get => _AdaptiveExposureGain; set => _AdaptiveExposureGain = Clamp(value, 0f, 1f); }
        private float _WindowX = Runtime.TunerRuntime.CurrentState.WindowX;
        [XmlElement("windowX")] public float WindowX { get => _WindowX; set => _WindowX = Infrastructure.FxStorage.Clamp(value, -100000f, 100000f); }
        private float _WindowY = Runtime.TunerRuntime.CurrentState.WindowY;
        [XmlElement("windowY")] public float WindowY { get => _WindowY; set => _WindowY = Infrastructure.FxStorage.Clamp(value, -100000f, 100000f); }
        private bool _VanillaMode = Runtime.TunerRuntime.CurrentState.VanillaMode;
        [XmlElement("vanillaMode")] public bool VanillaMode { get => _VanillaMode; set => _VanillaMode = value; }

        private float _SunPower = Runtime.TunerRuntime.CurrentState.SunPower;
        [XmlElement("sunPower")] public float SunPower { get => _SunPower; set => _SunPower = Clamp(value, 0f, 20f); }
        private float _MoonPower = Runtime.TunerRuntime.CurrentState.MoonPower;
        [XmlElement("moonPower")] public float MoonPower { get => _MoonPower; set => _MoonPower = Clamp(value, 0f, 20f); }
        private float _SkyRayleigh = Runtime.TunerRuntime.CurrentState.SkyRayleigh;
        [XmlElement("skyRayleigh")] public float SkyRayleigh { get => _SkyRayleigh; set => _SkyRayleigh = Clamp(value, 0f, 5f); }
        private float _SkyMie = Runtime.TunerRuntime.CurrentState.SkyMie;
        [XmlElement("skyMie")] public float SkyMie { get => _SkyMie; set => _SkyMie = Clamp(value, 0f, 5f); }


        private float _SkyExposure = Runtime.TunerRuntime.CurrentState.SkyExposure;
        [XmlElement("skyExposure")] public float SkyExposure { get => _SkyExposure; set => _SkyExposure = Clamp(value, 0f, 5f); }

        internal void Apply()
        {
            Runtime.TunerRuntime.CurrentState.SunStrength = SunStrength;
            Runtime.TunerRuntime.CurrentState.MoonStrength = MoonStrength;
            Runtime.TunerRuntime.CurrentState.Ambience = Ambience;
            Runtime.TunerRuntime.CurrentState.Warmth = Warmth;
            Runtime.TunerRuntime.CurrentState.SunTemp = SunTemp;
            Runtime.TunerRuntime.CurrentState.SunTint = SunTint;
            Runtime.TunerRuntime.CurrentState.MoonTemp = MoonTemp;
            Runtime.TunerRuntime.CurrentState.MoonTint = MoonTint;
            Runtime.TunerRuntime.CurrentState.SkyTemp = SkyTemp;
            Runtime.TunerRuntime.CurrentState.SkyTint = SkyTint;
            Runtime.TunerRuntime.CurrentState.GlobalTint = GlobalTint;
            Runtime.TunerRuntime.CurrentState.TwilightTint = TwilightTint;
            Runtime.TunerRuntime.CurrentState.SkyTonemapping = SkyTonemapping;
            Runtime.TunerRuntime.CurrentState.Brightness = Brightness;
            Runtime.TunerRuntime.CurrentState.Contrast = Contrast;
            Runtime.TunerRuntime.CurrentState.Gamma = Gamma;
            Runtime.TunerRuntime.CurrentState.AdaptiveShadows = AdaptiveShadows;
            Runtime.TunerRuntime.CurrentState.ForceLowBias = ForceLowBias;
            Runtime.TunerRuntime.CurrentState.BiasScale = BiasScale;
            Runtime.TunerRuntime.CurrentState.SoftShadows = SoftShadows;
            Runtime.TunerRuntime.CurrentState.AdaptiveExposure = AdaptiveExposure;
            Runtime.TunerRuntime.CurrentState.AdaptiveExposureGain = AdaptiveExposureGain;
            Runtime.TunerRuntime.CurrentState.WindowX = WindowX;
            Runtime.TunerRuntime.CurrentState.WindowY = WindowY;
            Runtime.TunerRuntime.CurrentState.VanillaMode = VanillaMode;
            Runtime.TunerRuntime.CurrentState.SunPower = SunPower;
            Runtime.TunerRuntime.CurrentState.MoonPower = MoonPower;
            Runtime.TunerRuntime.CurrentState.SkyRayleigh = SkyRayleigh;
            Runtime.TunerRuntime.CurrentState.SkyMie = SkyMie;
            Runtime.TunerRuntime.CurrentState.SkyExposure = SkyExposure;
            Runtime.TunerRuntime.CurrentState.LightingDirty = true;
        }

        private static float Clamp(float v, float min, float max)
        {
            return Infrastructure.FxStorage.Clamp(v, min, max);
        }
    }

    internal static class StateStore
    {
        private static readonly string FilePath =
            Path.Combine(DataLocation.localApplicationData, "LumenFX2.xml");

        private static float _lastSaveTime = -10f;
        private static bool _dirty;

        internal static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return;
                }

                using (var reader = new StreamReader(FilePath))
                {
                    ((StateDocument)new XmlSerializer(typeof(StateDocument)).Deserialize(reader)).Apply();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal static void Save(bool immediate = false)
        {
            _dirty = true;
            float now = Time.realtimeSinceStartup;
            if (immediate || now - _lastSaveTime >= 1.0f)
            {
                SaveImmediate();
            }
        }

        internal static void CheckPendingSave()
        {
            if (_dirty && Time.realtimeSinceStartup - _lastSaveTime >= 1.0f)
            {
                SaveImmediate();
            }
        }

        internal static void SaveImmediate()
        {
            _dirty = true;
            _lastSaveTime = Time.realtimeSinceStartup;
            try
            {
                Infrastructure.FxStorage.WriteXml(FilePath, new StateDocument());
                _dirty = false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

}

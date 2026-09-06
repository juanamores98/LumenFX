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

        [XmlElement("sunStrength")] public float SunStrength { get => Runtime.TunerRuntime.CurrentState.SunStrength; set => Runtime.TunerRuntime.CurrentState.SunStrength = Clamp(value, 0f, 2f); }
        [XmlElement("moonStrength")] public float MoonStrength { get => Runtime.TunerRuntime.CurrentState.MoonStrength; set => Runtime.TunerRuntime.CurrentState.MoonStrength = Clamp(value, 0f, 2f); }
        [XmlElement("ambience")] public float Ambience { get => Runtime.TunerRuntime.CurrentState.Ambience; set => Runtime.TunerRuntime.CurrentState.Ambience = Clamp(value, 0f, 2f); }
        [XmlElement("warmth")] public float Warmth { get => Runtime.TunerRuntime.CurrentState.Warmth; set => Runtime.TunerRuntime.CurrentState.Warmth = Clamp(value, -1f, 1f); }
        [XmlElement("sunTemp")] public float SunTemp { get => Runtime.TunerRuntime.CurrentState.SunTemp; set => Runtime.TunerRuntime.CurrentState.SunTemp = Clamp(value, -1f, 1f); }
        [XmlElement("sunTint")] public float SunTint { get => Runtime.TunerRuntime.CurrentState.SunTint; set => Runtime.TunerRuntime.CurrentState.SunTint = Clamp(value, -1f, 1f); }
        [XmlElement("moonTemp")] public float MoonTemp { get => Runtime.TunerRuntime.CurrentState.MoonTemp; set => Runtime.TunerRuntime.CurrentState.MoonTemp = Clamp(value, -1f, 1f); }
        [XmlElement("moonTint")] public float MoonTint { get => Runtime.TunerRuntime.CurrentState.MoonTint; set => Runtime.TunerRuntime.CurrentState.MoonTint = Clamp(value, -1f, 1f); }
        [XmlElement("skyTemp")] public float SkyTemp { get => Runtime.TunerRuntime.CurrentState.SkyTemp; set => Runtime.TunerRuntime.CurrentState.SkyTemp = Clamp(value, -1f, 1f); }
        [XmlElement("skyTint")] public float SkyTint { get => Runtime.TunerRuntime.CurrentState.SkyTint; set => Runtime.TunerRuntime.CurrentState.SkyTint = Clamp(value, -1f, 1f); }
        [XmlElement("globalTint")] public float GlobalTint { get => Runtime.TunerRuntime.CurrentState.GlobalTint; set => Runtime.TunerRuntime.CurrentState.GlobalTint = Clamp(value, -1f, 1f); }
        [XmlElement("twilightTint")] public float TwilightTint { get => Runtime.TunerRuntime.CurrentState.TwilightTint; set => Runtime.TunerRuntime.CurrentState.TwilightTint = Clamp(value, -1f, 1f); }
        [XmlElement("skyTonemapping")] public bool SkyTonemapping { get => Runtime.TunerRuntime.CurrentState.SkyTonemapping; set => Runtime.TunerRuntime.CurrentState.SkyTonemapping = value; }
        [XmlElement("brightness")] public float Brightness { get => Runtime.TunerRuntime.CurrentState.Brightness; set => Runtime.TunerRuntime.CurrentState.Brightness = Clamp(value, -1f, 1f); }
        [XmlElement("contrast")] public float Contrast { get => Runtime.TunerRuntime.CurrentState.Contrast; set => Runtime.TunerRuntime.CurrentState.Contrast = Clamp(value, -1f, 1f); }
        [XmlElement("gamma")] public float Gamma { get => Runtime.TunerRuntime.CurrentState.Gamma; set => Runtime.TunerRuntime.CurrentState.Gamma = Clamp(value, 1.5f, 3.5f); }
        [XmlElement("adaptiveShadows")] public bool AdaptiveShadows { get => Runtime.TunerRuntime.CurrentState.AdaptiveShadows; set => Runtime.TunerRuntime.CurrentState.AdaptiveShadows = value; }
        [XmlElement("forceLowBias")] public bool ForceLowBias { get => Runtime.TunerRuntime.CurrentState.ForceLowBias; set => Runtime.TunerRuntime.CurrentState.ForceLowBias = value; }
        [XmlElement("biasScale")] public float BiasScale { get => Runtime.TunerRuntime.CurrentState.BiasScale; set => Runtime.TunerRuntime.CurrentState.BiasScale = Clamp(value, 0f, 2f); }
        [XmlElement("softShadows")] public bool SoftShadows { get => Runtime.TunerRuntime.CurrentState.SoftShadows; set => Runtime.TunerRuntime.CurrentState.SoftShadows = value; }
        [XmlElement("adaptiveExposure")] public bool AdaptiveExposure { get => Runtime.TunerRuntime.CurrentState.AdaptiveExposure; set => Runtime.TunerRuntime.CurrentState.AdaptiveExposure = value; }
        [XmlElement("adaptiveExposureGain")] public float AdaptiveExposureGain { get => Runtime.TunerRuntime.CurrentState.AdaptiveExposureGain; set => Runtime.TunerRuntime.CurrentState.AdaptiveExposureGain = Clamp(value, 0f, 1f); }
        [XmlElement("windowX")] public float WindowX { get => Runtime.TunerRuntime.CurrentState.WindowX; set => Runtime.TunerRuntime.CurrentState.WindowX = value; }
        [XmlElement("windowY")] public float WindowY { get => Runtime.TunerRuntime.CurrentState.WindowY; set => Runtime.TunerRuntime.CurrentState.WindowY = value; }
        [XmlElement("vanillaMode")] public bool VanillaMode { get => Runtime.TunerRuntime.CurrentState.VanillaMode; set => Runtime.TunerRuntime.CurrentState.VanillaMode = value; }

        private static float Clamp(float v, float min, float max)
        {
            return v < min ? min : (v > max ? max : v);
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
                    new XmlSerializer(typeof(StateDocument)).Deserialize(reader);
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
            _dirty = false;
            _lastSaveTime = Time.realtimeSinceStartup;
            try
            {
                using (var writer = new StreamWriter(FilePath))
                {
                    new XmlSerializer(typeof(StateDocument)).Serialize(writer, new StateDocument());
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

}

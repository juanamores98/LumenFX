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
        [XmlElement("skyTonemapping")] public bool SkyTonemapping { get => Runtime.TunerRuntime.CurrentState.SkyTonemapping; set => Runtime.TunerRuntime.CurrentState.SkyTonemapping = value; }
        [XmlElement("brightness")] public float Brightness { get => Runtime.TunerRuntime.CurrentState.Brightness; set => Runtime.TunerRuntime.CurrentState.Brightness = Clamp(value, -1f, 1f); }
        [XmlElement("contrast")] public float Contrast { get => Runtime.TunerRuntime.CurrentState.Contrast; set => Runtime.TunerRuntime.CurrentState.Contrast = Clamp(value, -1f, 1f); }
        [XmlElement("gamma")] public float Gamma { get => Runtime.TunerRuntime.CurrentState.Gamma; set => Runtime.TunerRuntime.CurrentState.Gamma = Clamp(value, 1.5f, 3.5f); }
        [XmlElement("adaptiveShadows")] public bool AdaptiveShadows { get => Runtime.TunerRuntime.CurrentState.AdaptiveShadows; set => Runtime.TunerRuntime.CurrentState.AdaptiveShadows = value; }
        [XmlElement("biasScale")] public float BiasScale { get => Runtime.TunerRuntime.CurrentState.BiasScale; set => Runtime.TunerRuntime.CurrentState.BiasScale = Clamp(value, 0f, 2f); }
        [XmlElement("softShadows")] public bool SoftShadows { get => Runtime.TunerRuntime.CurrentState.SoftShadows; set => Runtime.TunerRuntime.CurrentState.SoftShadows = value; }

        private static float Clamp(float v, float min, float max)
        {
            return v < min ? min : (v > max ? max : v);
        }
    }

    internal static class StateStore
    {
        private static readonly string FilePath =
            Path.Combine(DataLocation.localApplicationData, "LumenFX2.xml");

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

        internal static void Save()
        {
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

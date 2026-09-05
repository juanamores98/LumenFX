using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using ColossalFramework.IO;

namespace LumenFX.Presets
{
    /// <summary>
    /// v2 preset documents: plain XML files stored in the mod's preset folder.
    /// </summary>
    public class PresetDocument
    {
        [XmlAttribute("name")]
        public string Name = "Untitled";

        [XmlElement("sunStrength")] public float SunStrength = 1f;
        [XmlElement("moonStrength")] public float MoonStrength = 1f;
        [XmlElement("ambience")] public float Ambience = 1f;
        [XmlElement("warmth")] public float Warmth = 0f;
        [XmlElement("skyTonemapping")] public bool SkyTonemapping = true;
        [XmlElement("brightness")] public float Brightness = 0f;
        [XmlElement("contrast")] public float Contrast = 0f;
        [XmlElement("gamma")] public float Gamma = 2.6f;
        [XmlElement("adaptiveShadows")] public bool AdaptiveShadows = true;
        [XmlElement("biasScale")] public float BiasScale = 1f;
        [XmlElement("softShadows")] public bool SoftShadows = true;
    }

    internal static class PresetLibrary
    {
        internal static readonly string Folder =
            Path.Combine(Path.Combine(DataLocation.localApplicationData, "ModConfig"), "LumenFXPresets");

        private static readonly string Pattern = "*.lumenfx.xml";

        internal static List<PresetDocument> LoadAll()
        {
            var presets = new List<PresetDocument>();
            EnsureFolder();

            var serializer = new XmlSerializer(typeof(PresetDocument));
            foreach (string file in Directory.GetFiles(Folder, Pattern))
            {
                try
                {
                    using (var reader = new StreamReader(file))
                    {
                        if (serializer.Deserialize(reader) is PresetDocument preset)
                        {
                            presets.Add(preset);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[LumenFX v2] skipping preset " + file + ": " + e.Message);
                }
            }

            return presets;
        }

        internal static bool Exists(string name)
        {
            return File.Exists(PathFor(name));
        }

        internal static void Save(PresetDocument preset)
        {
            EnsureFolder();
            using (var writer = new StreamWriter(PathFor(preset.Name)))
            {
                new XmlSerializer(typeof(PresetDocument)).Serialize(writer, preset);
            }
        }

        internal static void Delete(PresetDocument preset)
        {
            string path = PathFor(preset.Name);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string PathFor(string name)
        {
            return Path.Combine(Folder, Sanitize(name) + ".lumenfx.xml");
        }

        internal static string Sanitize(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "preset";
            }

            var invalid = Path.GetInvalidFileNameChars();
            var clean = name.Trim();
            foreach (char c in invalid)
            {
                clean = clean.Replace(c, '_');
            }

            return clean.Length == 0 ? "preset" : clean;
        }

        internal static void EnsureFolder()
        {
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }
        }
    }
}

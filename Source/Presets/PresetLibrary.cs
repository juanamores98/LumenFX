using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;

namespace LumenFX.Presets
{
    /// <summary>
    /// v2 preset documents: plain XML files stored in the mod's preset folder.
    /// </summary>
    public class PresetDocument
    {
        [XmlAttribute("name")]
        public string Name = "Untitled";

        [XmlElement("toneEnabled")] public int ToneEnabled = -1;
        [XmlElement("legacySceneLighting")] public bool LegacySceneLighting;
        [XmlElement("legacySceneSunMultiplier")] public float LegacySceneSunMultiplier = 1f;
        [XmlElement("legacySceneWarmth")] public float LegacySceneWarmth;
        [XmlElement("sunStrength")] public float SunStrength = 1f;
        [XmlElement("moonStrength")] public float MoonStrength = 1f;
        [XmlElement("ambience")] public float Ambience = 1f;
        [XmlElement("warmth")] public float Warmth = 0f;
        [XmlElement("sunTemp")] public float SunTemp;
        [XmlElement("sunTint")] public float SunTint;
        [XmlElement("moonTemp")] public float MoonTemp;
        [XmlElement("moonTint")] public float MoonTint;
        [XmlElement("skyTemp")] public float SkyTemp;
        [XmlElement("skyTint")] public float SkyTint;
        [XmlElement("globalTint")] public float GlobalTint;
        [XmlElement("twilightTint")] public float TwilightTint;
        [XmlElement("skyTonemapping")] public bool SkyTonemapping = true;
        [XmlElement("brightness")] public float Brightness = 0f;
        [XmlElement("contrast")] public float Contrast = 0f;
        [XmlElement("gamma")] public float Gamma = 2.6f;
        [XmlElement("adaptiveShadows")] public bool AdaptiveShadows = true;
        [XmlElement("forceLowBias")] public bool ForceLowBias;
        [XmlElement("biasScale")] public float BiasScale = 1f;
        [XmlElement("softShadows")] public bool SoftShadows = true;
        [XmlElement("adaptiveExposure")] public bool AdaptiveExposure = false;
        [XmlElement("adaptiveExposureGain")] public float AdaptiveExposureGain = 0.5f;
        [XmlElement("sunPower")] public float SunPower;
        [XmlElement("moonPower")] public float MoonPower;
        [XmlElement("skyExposure")] public float SkyExposure;
        [XmlElement("skyRayleigh")] public float SkyRayleigh;
        [XmlElement("skyMie")] public float SkyMie;
        [XmlElement("vanillaMode")] public bool VanillaMode;

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

        internal static PresetDocument Capture(string name)
        {
            var state = Runtime.TunerRuntime.CurrentState;
            return new PresetDocument
            {
                Name = Sanitize(name),
                ToneEnabled = state.ToneEnabled,
                LegacySceneLighting = state.LegacySceneLighting,
                LegacySceneSunMultiplier = state.LegacySceneSunMultiplier,
                LegacySceneWarmth = state.LegacySceneWarmth,
                SunStrength = state.SunStrength,
                MoonStrength = state.MoonStrength,
                Ambience = state.Ambience,
                Warmth = state.Warmth,
                SunTemp = state.SunTemp,
                SunTint = state.SunTint,
                MoonTemp = state.MoonTemp,
                MoonTint = state.MoonTint,
                SkyTemp = state.SkyTemp,
                SkyTint = state.SkyTint,
                GlobalTint = state.GlobalTint,
                TwilightTint = state.TwilightTint,
                SkyTonemapping = state.SkyTonemapping,
                Brightness = state.Brightness,
                Contrast = state.Contrast,
                Gamma = state.Gamma,
                AdaptiveShadows = state.AdaptiveShadows,
                ForceLowBias = state.ForceLowBias,
                BiasScale = state.BiasScale,
                SoftShadows = state.SoftShadows,
                AdaptiveExposure = state.AdaptiveExposure,
                AdaptiveExposureGain = state.AdaptiveExposureGain,
                SunPower = state.SunPower,
                MoonPower = state.MoonPower,
                SkyExposure = state.SkyExposure,
                SkyRayleigh = state.SkyRayleigh,
                SkyMie = state.SkyMie,
                VanillaMode = state.VanillaMode,
            };
        }

        internal static void Apply(PresetDocument preset)
        {
            var pending = new IO.StateDocument
            {
                ToneEnabled = preset.ToneEnabled,
                LegacySceneLighting = preset.LegacySceneLighting,
                LegacySceneSunMultiplier = preset.LegacySceneSunMultiplier,
                LegacySceneWarmth = preset.LegacySceneWarmth,
                SunStrength = preset.SunStrength,
                MoonStrength = preset.MoonStrength,
                Ambience = preset.Ambience,
                Warmth = preset.Warmth,
                SunTemp = preset.SunTemp,
                SunTint = preset.SunTint,
                MoonTemp = preset.MoonTemp,
                MoonTint = preset.MoonTint,
                SkyTemp = preset.SkyTemp,
                SkyTint = preset.SkyTint,
                GlobalTint = preset.GlobalTint,
                TwilightTint = preset.TwilightTint,
                SkyTonemapping = preset.SkyTonemapping,
                Brightness = preset.Brightness,
                Contrast = preset.Contrast,
                Gamma = preset.Gamma,
                AdaptiveShadows = preset.AdaptiveShadows,
                ForceLowBias = preset.ForceLowBias,
                BiasScale = preset.BiasScale,
                SoftShadows = preset.SoftShadows,
                AdaptiveExposure = preset.AdaptiveExposure,
                AdaptiveExposureGain = preset.AdaptiveExposureGain,
                SunPower = preset.SunPower,
                MoonPower = preset.MoonPower,
                SkyExposure = preset.SkyExposure,
                SkyRayleigh = preset.SkyRayleigh,
                SkyMie = preset.SkyMie,
                VanillaMode = preset.VanillaMode,
            };
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                new XmlSerializer(typeof(IO.StateDocument)).Serialize(writer, pending);
                if (!LumenFXMod.ApplySuiteSection(writer.ToString())) throw new InvalidOperationException(LumenFXMod.LastApplyError);
            }
        }

        internal static bool Exists(string name)
        {
            return File.Exists(PathFor(name));
        }

        internal static void Save(PresetDocument preset)
        {
            EnsureFolder();
            Infrastructure.FxStorage.WriteXml(PathFor(preset.Name), preset);
        }

        internal static void Delete(PresetDocument preset)
        {
            string path = PathFor(preset.Name);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Imports legacy Relight preset files (.light, "key = value" text
        /// format) found on this machine â€” local preset folders and subscribed
        /// Workshop items â€” converting them to the v2 preset schema. The
        /// original files are left untouched. Idempotent: already-imported
        /// names are skipped.
        /// </summary>
        internal static void ImportLegacyRelightPresets()
        {
            EnsureFolder();

            var sources = new List<string>();
            string relightFolder = Path.Combine(
                Path.Combine(Path.Combine(DataLocation.localApplicationData, "ModConfig"), "RelightPresets"),
                string.Empty);
            if (Directory.Exists(relightFolder))
            {
                sources.AddRange(Directory.GetFiles(relightFolder, "*.light"));
            }

            try
            {
                foreach (PublishedFileId id in PlatformService.workshop.GetSubscribedItems())
                {
                    string dir = PlatformService.workshop.GetSubscribedItemPath(id);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        sources.AddRange(Directory.GetFiles(dir, "*.light", SearchOption.AllDirectories));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LumenFX v2] workshop preset scan skipped: " + e.Message);
            }

            foreach (string file in sources)
            {
                try
                {
                    PresetDocument imported = ParseLegacyLight(file);
                    if (imported == null || File.Exists(PathFor(imported.Name)))
                    {
                        continue;
                    }

                    Save(imported);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[LumenFX v2] legacy preset skipped " + file + ": " + e.Message);
                }
            }
        }

        private static PresetDocument ParseLegacyLight(string path)
        {
            string name = null;
            var values = new float[13];
            bool skyTonemapping = true;

            foreach (string line in File.ReadAllLines(path))
            {
                int separator = line.IndexOf(" = ", StringComparison.Ordinal);
                if (separator < 0)
                {
                    continue;
                }

                string key = line.Substring(0, separator).Trim();
                string raw = line.Substring(separator + 3).Trim();

                if (key == "name")
                {
                    name = raw;
                    continue;
                }

                if (key == "skyTmpg")
                {
                    bool flag;
                    if (bool.TryParse(raw, out flag))
                    {
                        skyTonemapping = flag;
                    }

                    continue;
                }

                int index;
                float value;
                if (int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
                    && index >= 0 && index < 13
                    && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    values[index] = value;
                }
            }

            if (string.IsNullOrEmpty(name) || name == "[none]")
            {
                name = Path.GetFileNameWithoutExtension(path);
            }

            return TranslateLegacy(name, values, skyTonemapping);
        }

        /// <summary>
        /// Maps the legacy 13-value layout onto the v2 parameters. Documented
        /// translations: brightness compensates the legacy intensity divisor;
        /// gamma uses the legacy arithmetic mapping; moon strength derives
        /// from the legacy moon-light weight; the per-source temperature and
        /// tint values carry over into the v2 advanced controls.
        /// </summary>
        private static PresetDocument TranslateLegacy(string name, float[] v, bool skyTonemapping)
        {
            return new PresetDocument
            {
                Name = name + " (Relight)",
                SunStrength = 1f,
                MoonStrength = Mathf.Clamp(0.5f + 0.35f * v[8], 0f, 2f),
                Ambience = 1f,
                Warmth = Mathf.Clamp(0.6f * v[0], -1f, 1f),
                SunTemp = Mathf.Clamp(v[2], -1f, 1f),
                SunTint = Mathf.Clamp(v[3], -1f, 1f),
                MoonTemp = Mathf.Clamp(v[6], -1f, 1f),
                MoonTint = Mathf.Clamp(v[7], -1f, 1f),
                SkyTemp = Mathf.Clamp(v[4], -1f, 1f),
                SkyTint = Mathf.Clamp(v[5], -1f, 1f),
                GlobalTint = Mathf.Clamp(v[1], -1f, 1f),
                TwilightTint = Mathf.Clamp(v[9], -1f, 1f),
                SkyTonemapping = skyTonemapping,
                Brightness = Mathf.Clamp(-0.4f * (v[10] / 0.7f), -1f, 1f),
                Contrast = Mathf.Clamp(v[12], -1f, 1f),
                Gamma = Mathf.Clamp(2.6f * (((v[11] + 1f) / 4f) + 0.75f), 1.5f, 3.5f),
                AdaptiveShadows = true,
                ForceLowBias = false,
                BiasScale = 1f,
                SoftShadows = true,
            };
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

using System.Reflection;
using ICities;
using UnityEngine;
using Harmony;
using LumenFX.Core;
using LumenFX.Runtime;

namespace LumenFX
{
    /// <summary>
    /// LumenFX v2 entry point: original lighting, tonemapping and shadow
    /// tuning for Cities: Skylines with its own adaptive bias model.
    /// </summary>
    public class LumenFXMod : LoadingExtensionBase, IUserMod
    {
        private const string HostObjectName = "LumenFX2";
        private const string HarmonyId = "com.juanamores98.lumenfx2.bias";

        private static bool _patched;

        private GameObject _host;
        private HarmonyInstance _harmony;

        public string Name
        {
            get { return "LumenFX v2"; }
        }

        public string Description
        {
            get { return "Sun, moon and ambience balancing, filmic tone controls and adaptive shadow bias with presets."; }
        }

        /// <summary>
        /// Scene hosts created while the main menu is up die when the gameplay
        /// scene loads, so the host is (re)created here for every map.
        /// </summary>
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            CreateHost();
            TunerRuntime.ApplyAll();

            UI.UuiButton.Register(
                "LumenFX v2",
                "Lighting, tone and shadow tuner (Ctrl+Alt+L)",
                UI.TrayIcon.Make(),
                show => Core.TunerEngine.OpenWindow());
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            UI.UuiButton.Unregister();
            DestroyHosts();
        }

        public void OnEnabled()
        {
            // Covers the case of enabling the mod while a map is already
            // running; the gameplay scene replaces menu-time hosts anyway.
            CreateHost();

            if (_patched)
            {
                return;
            }

            _harmony = HarmonyInstance.Create(HarmonyId);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            _patched = true;
        }

        public void OnDisabled()
        {
            DestroyHosts();
        }

        private void CreateHost()
        {
            DestroyHosts();
            _host = new GameObject(HostObjectName);
            _host.AddComponent<Core.TunerEngine>();
        }

        private static void DestroyHosts()
        {
            while (true)
            {
                GameObject leftover = GameObject.Find(HostObjectName);
                if (!leftover)
                {
                    break;
                }

                UnityEngine.Object.DestroyImmediate(leftover);
            }
        }

        /// <summary>
        /// Suite profile coordinator API: applies the <lumenfx> section from a unified suite profile.
        /// </summary>
        public static bool ApplySuiteSection(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);
                return ApplySuiteSection(doc.DocumentElement);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static bool ApplySuiteSection(System.Xml.XmlElement element)
        {
            if (element == null)
            {
                return false;
            }

            try
            {
                var state = TunerRuntime.CurrentState;
                if (state == null)
                {
                    return false;
                }

                var ci = System.Globalization.CultureInfo.InvariantCulture;
                foreach (System.Xml.XmlNode node in element.ChildNodes)
                {
                    if (node.NodeType != System.Xml.XmlNodeType.Element) continue;
                    string name = node.Name.ToLowerInvariant();
                    string val = node.InnerText != null ? node.InnerText.Trim() : string.Empty;
                    float f;
                    bool b;

                    if (name == "sunstrength" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.SunStrength = f;
                    else if (name == "moonstrength" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.MoonStrength = f;
                    else if (name == "ambience" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.Ambience = f;
                    else if (name == "warmth" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.Warmth = f;
                    else if (name == "suntemp" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.SunTemp = f;
                    else if (name == "suntint" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.SunTint = f;
                    else if (name == "moontemp" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.MoonTemp = f;
                    else if (name == "moontint" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.MoonTint = f;
                    else if (name == "skytemp" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.SkyTemp = f;
                    else if (name == "skytint" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.SkyTint = f;
                    else if (name == "globaltint" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.GlobalTint = f;
                    else if (name == "twilighttint" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.TwilightTint = f;
                    else if (name == "skytonemapping" && bool.TryParse(val, out b)) state.SkyTonemapping = b;
                    else if (name == "brightness" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.Brightness = f;
                    else if (name == "contrast" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.Contrast = f;
                    else if (name == "gamma" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.Gamma = f;
                    else if (name == "adaptiveshadows" && bool.TryParse(val, out b)) state.AdaptiveShadows = b;
                    else if (name == "forcelowbias" && bool.TryParse(val, out b)) state.ForceLowBias = b;
                    else if (name == "biasscale" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.BiasScale = f;
                    else if (name == "softshadows" && bool.TryParse(val, out b)) state.SoftShadows = b;
                    else if (name == "adaptiveexposure" && bool.TryParse(val, out b)) state.AdaptiveExposure = b;
                    else if (name == "adaptiveexposuregain" && float.TryParse(val, System.Globalization.NumberStyles.Float, ci, out f)) state.AdaptiveExposureGain = f;
                }

                state.LightingDirty = true;
                TunerRuntime.ApplyAll();
                IO.StateStore.SaveImmediate();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static string ExportSuiteSection()
        {
            var s = TunerRuntime.CurrentState;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            return string.Format(
                "  <lumenfx>\n" +
                "    <sunStrength>{0}</sunStrength>\n" +
                "    <moonStrength>{1}</moonStrength>\n" +
                "    <ambience>{2}</ambience>\n" +
                "    <warmth>{3}</warmth>\n" +
                "    <sunTemp>{4}</sunTemp>\n" +
                "    <sunTint>{5}</sunTint>\n" +
                "    <moonTemp>{6}</moonTemp>\n" +
                "    <moonTint>{7}</moonTint>\n" +
                "    <skyTemp>{8}</skyTemp>\n" +
                "    <skyTint>{9}</skyTint>\n" +
                "    <globalTint>{10}</globalTint>\n" +
                "    <twilightTint>{11}</twilightTint>\n" +
                "    <skyTonemapping>{12}</skyTonemapping>\n" +
                "    <brightness>{13}</brightness>\n" +
                "    <contrast>{14}</contrast>\n" +
                "    <gamma>{15}</gamma>\n" +
                "    <adaptiveShadows>{16}</adaptiveShadows>\n" +
                "    <forceLowBias>{17}</forceLowBias>\n" +
                "    <biasScale>{18}</biasScale>\n" +
                "    <softShadows>{19}</softShadows>\n" +
                "    <adaptiveExposure>{20}</adaptiveExposure>\n" +
                "    <adaptiveExposureGain>{21}</adaptiveExposureGain>\n" +
                "  </lumenfx>",
                s.SunStrength.ToString("0.##", ci),
                s.MoonStrength.ToString("0.##", ci),
                s.Ambience.ToString("0.##", ci),
                s.Warmth.ToString("0.##", ci),
                s.SunTemp.ToString("0.##", ci),
                s.SunTint.ToString("0.##", ci),
                s.MoonTemp.ToString("0.##", ci),
                s.MoonTint.ToString("0.##", ci),
                s.SkyTemp.ToString("0.##", ci),
                s.SkyTint.ToString("0.##", ci),
                s.GlobalTint.ToString("0.##", ci),
                s.TwilightTint.ToString("0.##", ci),
                s.SkyTonemapping.ToString().ToLowerInvariant(),
                s.Brightness.ToString("0.##", ci),
                s.Contrast.ToString("0.##", ci),
                s.Gamma.ToString("0.##", ci),
                s.AdaptiveShadows.ToString().ToLowerInvariant(),
                s.ForceLowBias.ToString().ToLowerInvariant(),
                s.BiasScale.ToString("0.##", ci),
                s.SoftShadows.ToString().ToLowerInvariant(),
                s.AdaptiveExposure.ToString().ToLowerInvariant(),
                s.AdaptiveExposureGain.ToString("0.##", ci));
        }
    }
}


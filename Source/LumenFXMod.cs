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
        private static HarmonyInstance _harmony;

        private GameObject _host;

        public string Name
        {
            get { return "LumenFX v2"; }
        }

        public string Description
        {
            get { return "Sun, moon and ambience balancing, filmic tone controls and adaptive shadow bias with presets."; }
        }

        /// <summary>
        /// True while this mod owns the camera tonemapping component, so
        /// SceneFX can defer its own tone writes to this one.
        /// </summary>
        /// <remarks>
        /// Se conserva por compatibilidad con versiones de SceneFX que solo saben preguntar
        /// por el tono. Lo que de verdad describe quien escribe cada campo es
        /// <see cref="ActiveClaims"/>.
        /// </remarks>
        public static bool ToneWriterActive
        {
            get { return Claims("tone"); }
        }

        /// <summary>
        /// Los campos compartidos del juego que este mod esta escribiendo ahora mismo,
        /// separados por comas.
        /// </summary>
        /// <remarks>
        /// <b>Por que hace falta.</b> Cuatro mods escriben sobre los mismos campos de
        /// <c>DayNightProperties</c> y de <c>ToneMapping</c>. Un solo booleano de "yo mando en
        /// el tono" no alcanzaba: en cuanto LumenFX gano controles absolutos de intensidad
        /// solar y de cielo, pasaron a estar en disputa <c>m_SunIntensity</c>,
        /// <c>m_MoonIntensity</c>, <c>m_RayleighScattering</c> y <c>m_MieScattering</c>.
        ///
        /// Las reclamaciones son dinamicas: un eje solo se reclama mientras se esta
        /// escribiendo de verdad. Con el modo vanilla puesto no se reclama nada, que es lo
        /// coherente con no tocar el juego.
        ///
        /// Se publica como texto y no como un enum para que el otro lado pueda leerlo por
        /// reflexion sin compartir ningun tipo.
        /// </remarks>
        public static string ActiveClaims
        {
            get
            {
                if (!_patched)
                {
                    return string.Empty;
                }

                var state = TunerRuntime.CurrentState;
                if (state == null || state.VanillaMode)
                {
                    return string.Empty;
                }

                // El mezclador y el perfil de tono escriben siempre que el mod esta vivo.
                string claims = "tone,lightColor,skyTonemapping";
                if (state.SunPower > 0f || state.LegacySceneLighting || Infrastructure.FxInterop.ClassicRequest("sunStrength")) claims += ",sunIntensity";
                if (state.MoonPower > 0f) claims += ",moonIntensity";

                if (Infrastructure.FxInterop.ClassicRequest("sunStrength") || state.SkyExposure > 0f || (state.AdaptiveExposure && !ThemeOwnership.AtmosphereIsManaged))
                {
                    claims += ",exposure";
                }

                if (state.SkyRayleigh > 0f || state.SkyMie > 0f)
                {
                    claims += ",sky";
                }

                return claims;
            }
        }

        private static bool Claims(string field)
        {
            string claims = ActiveClaims;

            return claims.Length > 0
                && ("," + claims + ",").IndexOf("," + field + ",", System.StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// Scene hosts created while the main menu is up die when the gameplay
        /// scene loads, so the host is (re)created here for every map.
        /// </summary>
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            IO.StateStore.Load();
            CreateHost();

            var state = TunerRuntime.CurrentState;
            if (state != null)
            {
                if (state.VanillaMode)
                {
                    TunerRuntime.RestoreVanilla();
                }
                else if (state.ApplyOnLoad)
                {
                    TunerRuntime.ApplyAll();
                }
            }

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
            IO.StateStore.SaveImmediate();
            Infrastructure.PropertyLedger.Forget("LumenFX");
            DestroyHosts();
        }

        public void OnEnabled()
        {
            IO.StateStore.Load();
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
            UI.UuiButton.Unregister();
            TunerRuntime.RestoreVanilla();
            DestroyHosts();

            if (_patched && _harmony != null)
            {
                _harmony.UnpatchAll(HarmonyId);
                _harmony = null;
                _patched = false;
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var group = helper.AddGroup("LumenFX");
            group.AddButton("VANILLA", FxModule.Release);
            group.AddButton("OPTIMIZED / Default", FxModule.ApplyOptimized);
            group.AddButton("Open compact panel", () => FxModule.OpenStandalone());
            group.AddCheckbox("Apply saved settings when a city loads", TunerRuntime.CurrentState.ApplyOnLoad, value =>
            {
                TunerRuntime.CurrentState.ApplyOnLoad = value;
                IO.StateStore.SaveImmediate();
            });
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

        public static void RefreshDerivedState() { if (_patched && !Runtime.TunerRuntime.CurrentState.VanillaMode) { Runtime.TunerRuntime.CurrentState.LightingDirty = true; Runtime.TunerRuntime.ApplyAll(); } NotifyStateChanged(); }
        public static bool ReadyForSuite { get { return _patched && UnityEngine.Object.FindObjectOfType<DayNightProperties>() != null && GameObject.Find("Main Camera") != null; } }
        public static string LastApplyError { get; private set; }
        public static string ApplicationStatus { get; private set; }
        public static event System.Action StateChanged;
        public static void NotifyStateChanged()
        {
            var changed = StateChanged;
            if (changed == null) return;
            foreach (System.Action observer in changed.GetInvocationList())
                try { observer(); } catch (System.Exception e) { UnityEngine.Debug.LogException(e); }
        }
        public static bool ValidateSuiteSection(string xml)
        {
            try
            {
                var doc = new System.Xml.XmlDocument { XmlResolver = null }; doc.LoadXml(xml);
                return ParseSection(doc.DocumentElement, false);
            }
            catch (System.Exception e) { LastApplyError = e.Message; return false; }
        }
        public static bool ApplySuiteSection(System.Xml.XmlElement element)
        {
            if (!ParseSection(element, false)) return false;
            if (Infrastructure.FxTransaction.Active) return ParseSection(element, true);
            string previous = ExportSuiteSection();
            Infrastructure.FxTransaction.Begin();
            try
            {
                if (!ParseSection(element, true)) throw new System.InvalidOperationException(LastApplyError);
                Infrastructure.FxTransaction.Commit();
                return true;
            }
            catch (System.Exception failure)
            {
                if (!Infrastructure.FxTransaction.Active) Infrastructure.FxTransaction.Begin();
                var doc = new System.Xml.XmlDocument(); doc.LoadXml(previous);
                bool restored = ParseSection(doc.DocumentElement, true) && ExportSuiteSection() == previous;
                Infrastructure.FxTransaction.Abort();
                LastApplyError = failure.Message;
                ApplicationStatus = (restored && !failure.Message.StartsWith("PARTIAL:") ? "Failed; previous settings restored: " : "PARTIAL; rollback could not be verified: ") + failure.Message;
                NotifyStateChanged();
                return false;
            }
            finally { Infrastructure.FxTransaction.Abort(); }
        }
        private static bool ParseSection(System.Xml.XmlElement element, bool commit)
        {
            if (element == null || !element.Name.Equals("lumenfx", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                LastApplyError = string.Empty;
                Infrastructure.FxStorage.LastError = string.Empty;
                string schema = element.GetAttribute("schema");
                if (schema.Length > 0 && schema != "2" && schema != "3") throw new System.ArgumentException("Unsupported preset schema: " + schema);

                var state = new IO.StateDocument();
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



                    if (name == "legacyscenelighting") state.LegacySceneLighting = bool.Parse(val);
                    else if (name == "legacyscenesunmultiplier") state.LegacySceneSunMultiplier = Infrastructure.FxStorage.Clamp(float.Parse(val, ci), 0f, 3f);
                    else if (name == "legacyscenewarmth") state.LegacySceneWarmth = Infrastructure.FxStorage.Clamp(float.Parse(val, ci), -1f, 1f);
                    else if (name == "toneenabled") state.ToneEnabled = int.Parse(val, ci);
                    else if (name == "sunstrength") state.SunStrength = float.Parse(val, ci);
                    else if (name == "moonstrength") state.MoonStrength = float.Parse(val, ci);
                    else if (name == "ambience") state.Ambience = float.Parse(val, ci);
                    else if (name == "warmth") state.Warmth = float.Parse(val, ci);
                    else if (name == "suntemp") state.SunTemp = float.Parse(val, ci);
                    else if (name == "suntint") state.SunTint = float.Parse(val, ci);
                    else if (name == "moontemp") state.MoonTemp = float.Parse(val, ci);
                    else if (name == "moontint") state.MoonTint = float.Parse(val, ci);
                    else if (name == "skytemp") state.SkyTemp = float.Parse(val, ci);
                    else if (name == "skytint") state.SkyTint = float.Parse(val, ci);
                    else if (name == "globaltint") state.GlobalTint = float.Parse(val, ci);
                    else if (name == "twilighttint") state.TwilightTint = float.Parse(val, ci);
                    else if (name == "skytonemapping") state.SkyTonemapping = bool.Parse(val);
                    else if (name == "brightness") state.Brightness = float.Parse(val, ci);
                    else if (name == "contrast") state.Contrast = float.Parse(val, ci);
                    else if (name == "gamma") state.Gamma = float.Parse(val, ci);
                    else if (name == "adaptiveshadows") state.AdaptiveShadows = bool.Parse(val);
                    else if (name == "forcelowbias") state.ForceLowBias = bool.Parse(val);
                    else if (name == "biasscale") state.BiasScale = float.Parse(val, ci);
                    else if (name == "softshadows") state.SoftShadows = bool.Parse(val);
                    else if (name == "adaptiveexposure") state.AdaptiveExposure = bool.Parse(val);
                    else if (name == "adaptiveexposuregain") state.AdaptiveExposureGain = float.Parse(val, ci);
                    else if (name == "skyexposure") state.SkyExposure = float.Parse(val, ci);
                    else if (name == "skyrayleigh") state.SkyRayleigh = float.Parse(val, ci);
                    else if (name == "skymie") state.SkyMie = float.Parse(val, ci);
                    else if (name == "sunpower") state.SunPower = float.Parse(val, ci);
                    else if (name == "moonpower") state.MoonPower = float.Parse(val, ci);
                    // El modo vanilla decide si el mod escribe o no, y no se podia expresar en
                    // un perfil de suite: un perfil no tenia forma de encenderlo ni apagarlo.
                    else if (name == "vanillamode") state.VanillaMode = bool.Parse(val);
                    else if (name == "applyonload") state.ApplyOnLoad = bool.Parse(val);
                }

                if (state.ToneEnabled < -1 || state.ToneEnabled > 1) throw new System.ArgumentException("Invalid camera tone mode.");
                if (!commit) return true;
                state.Apply();
                TunerRuntime.ApplyAll();
                IO.StateStore.SaveImmediate();
                if (!string.IsNullOrEmpty(Infrastructure.FxStorage.LastError)) throw new System.IO.IOException(Infrastructure.FxStorage.LastError);
                ApplicationStatus = "Applied to settings; verify appearance in game";
                Infrastructure.FxInterop.RefreshCompanions();
                NotifyStateChanged();
                return true;
            }
            catch (System.Exception e)
            {
                LastApplyError = e.Message;
                ApplicationStatus = "Failed: " + e.Message;

                Debug.LogException(e);
                return false;
            }
        }

        public static string ExportSuiteSection()
        {
            var source = new System.Xml.XmlDocument();
            using (var writer = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture))
            {
                new System.Xml.Serialization.XmlSerializer(typeof(IO.StateDocument)).Serialize(writer, new IO.StateDocument());
                source.LoadXml(writer.ToString());
            }
            var doc = new System.Xml.XmlDocument();
            var section = doc.CreateElement("lumenfx"); doc.AppendChild(section);
            foreach (System.Xml.XmlNode node in source.DocumentElement.ChildNodes)
                if (node.Name != "windowX" && node.Name != "windowY") section.AppendChild(doc.ImportNode(node, true));
            return section.OuterXml;
        }
    }
}

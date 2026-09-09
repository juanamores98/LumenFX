using System;
using System.IO;
using ColossalFramework.UI;
using UnityEngine;
using LumenFX.UI;

namespace LumenFX
{
    /// <summary>Small public boundary for a standalone or embedded native panel.</summary>
    public static class FxModule
    {
        public const float PreferredWidth = 380f;
        public const float PreferredHeight = 540f;
        private static PanelView _standalone;
        public static string Mode { get { return Runtime.TunerRuntime.CurrentState.VanillaMode ? "VANILLA" : (Infrastructure.FxStorage.MatchesOptimized(ReadState(), typeof(LumenFXMod)) ? "OPTIMIZED" : "CUSTOM"); } }
        public static string ReadState() { return LumenFXMod.ExportSuiteSection(); }
        public static bool ApplyState(string xml) { return LumenFXMod.ApplySuiteSection(xml); }
        public static void Release() { if (!Presets.QuickPresets.ApplyVanilla()) throw new InvalidOperationException("VANILLA could not be applied."); Flush(); }
        public static void ApplyOptimized() { if (!Presets.QuickPresets.ApplyOptimized()) throw new InvalidOperationException(LumenFXMod.LastApplyError ?? "Default could not be applied."); Flush(); }
        public static void Flush() { IO.StateStore.SaveImmediate(); }
        public static string Status { get { return !string.IsNullOrEmpty(Infrastructure.FxStorage.LastError) ? Infrastructure.FxStorage.LastError : !string.IsNullOrEmpty(Infrastructure.PropertyLedger.LastWarning) ? Infrastructure.PropertyLedger.LastWarning : "Config: " + UiText.Get(Mode); } }

        public static PanelView CreatePanel(UIComponent parent, float width = PreferredWidth, float height = PreferredHeight)
        {
            var view = new PanelView("LumenFX", parent, width, height, Release, ApplyOptimized, () => Status, () => Mode);
            var page0 = view.AddPage("Light");
            view.Number(page0, "Sun power (0 = map)", () => Runtime.TunerRuntime.CurrentState.SunPower, v => Edit(() => Runtime.TunerRuntime.CurrentState.SunPower = v), 0f, 20f, 0.01f, enabled: () => !Infrastructure.FxInterop.ClassicRequest("sunStrength") && !Runtime.TunerRuntime.CurrentState.LegacySceneLighting);
            view.Number(page0, "Moon power (0 = map)", () => Runtime.TunerRuntime.CurrentState.MoonPower, v => Edit(() => Runtime.TunerRuntime.CurrentState.MoonPower = v), 0f, 20f, 0.01f);
            view.Number(page0, "Sun colour gain", () => Runtime.TunerRuntime.CurrentState.SunStrength, v => Edit(() => Runtime.TunerRuntime.CurrentState.SunStrength = v), 0f, 3f, 0.01f, enabled: () => !Runtime.TunerRuntime.CurrentState.LegacySceneLighting && !Infrastructure.FxInterop.ClassicRequest("sunColor"));
            view.Number(page0, "Moon colour gain", () => Runtime.TunerRuntime.CurrentState.MoonStrength, v => Edit(() => Runtime.TunerRuntime.CurrentState.MoonStrength = v), 0f, 2f, 0.01f, enabled: () => !Runtime.TunerRuntime.CurrentState.LegacySceneLighting && !Infrastructure.FxInterop.ClassicRequest("sunColor"));
            view.Number(page0, "Ambient gain", () => Runtime.TunerRuntime.CurrentState.Ambience, v => Edit(() => Runtime.TunerRuntime.CurrentState.Ambience = v), 0f, 2f, 0.01f);
            view.Number(page0, "Rayleigh (0 = map)", () => Runtime.TunerRuntime.CurrentState.SkyRayleigh, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyRayleigh = v), 0f, 5f, 0.001f);
            view.Number(page0, "Mie (0 = map)", () => Runtime.TunerRuntime.CurrentState.SkyMie, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyMie = v), 0f, 5f, 0.001f);
            view.Number(page0, "Sky red wavelength (0 = map)", () => Runtime.TunerRuntime.CurrentState.SkyWaveR, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyWaveR = v), 0f, 1000f, 1f, enabled: () => !Infrastructure.FxInterop.ClassicRequest("fogTint"));
            view.Number(page0, "Sky green wavelength (0 = map)", () => Runtime.TunerRuntime.CurrentState.SkyWaveG, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyWaveG = v), 0f, 1000f, 1f, enabled: () => !Infrastructure.FxInterop.ClassicRequest("fogTint"));
            view.Number(page0, "Sky blue wavelength (0 = map)", () => Runtime.TunerRuntime.CurrentState.SkyWaveB, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyWaveB = v), 0f, 1000f, 1f, enabled: () => !Infrastructure.FxInterop.ClassicRequest("fogTint"));
            view.Number(page0, "Sky exposure (0 = map)", () => Runtime.TunerRuntime.CurrentState.SkyExposure, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyExposure = v), 0f, 5f, 0.001f, enabled: () => !Infrastructure.FxInterop.ClassicRequest("sunStrength"));
            view.Check(page0, "Sky tonemapping", () => Runtime.TunerRuntime.CurrentState.SkyTonemapping, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyTonemapping = v));
            view.Check(page0, "Apply settings when a city loads", () => Runtime.TunerRuntime.CurrentState.ApplyOnLoad, v => { Runtime.TunerRuntime.CurrentState.ApplyOnLoad = v; IO.StateStore.SaveImmediate(); });
            view.Info(page0, () => LumenFXMod.ApplicationStatus ?? "Settings ready; appearance not yet verified in game");
            var page1 = view.AddPage("Color");
            view.Check(page1, "Use imported Scene lighting model", () => Runtime.TunerRuntime.CurrentState.LegacySceneLighting, v => Edit(() => Runtime.TunerRuntime.CurrentState.LegacySceneLighting = v));
            view.Info(page1, () => Runtime.TunerRuntime.CurrentState.LegacySceneLighting ? "Imported Scene multiplier and warmth are active. Disable to use native Lumen colour controls." : "Native Lumen lighting model");
            view.Number(page1, "Overall warmth", () => Runtime.TunerRuntime.CurrentState.Warmth, v => Edit(() => Runtime.TunerRuntime.CurrentState.Warmth = v), -1f, 1f, 0.01f);
            view.Number(page1, "Overall tint", () => Runtime.TunerRuntime.CurrentState.GlobalTint, v => Edit(() => Runtime.TunerRuntime.CurrentState.GlobalTint = v), -1f, 1f, 0.01f);
            view.Number(page1, "Sun temperature", () => Runtime.TunerRuntime.CurrentState.SunTemp, v => Edit(() => Runtime.TunerRuntime.CurrentState.SunTemp = v), -1f, 1f, 0.01f, enabled: () => !Runtime.TunerRuntime.CurrentState.LegacySceneLighting && !Infrastructure.FxInterop.ClassicRequest("sunColor"));
            view.Number(page1, "Sun tint", () => Runtime.TunerRuntime.CurrentState.SunTint, v => Edit(() => Runtime.TunerRuntime.CurrentState.SunTint = v), -1f, 1f, 0.01f, enabled: () => !Runtime.TunerRuntime.CurrentState.LegacySceneLighting && !Infrastructure.FxInterop.ClassicRequest("sunColor"));
            view.Number(page1, "Moon temperature", () => Runtime.TunerRuntime.CurrentState.MoonTemp, v => Edit(() => Runtime.TunerRuntime.CurrentState.MoonTemp = v), -1f, 1f, 0.01f, enabled: () => !Runtime.TunerRuntime.CurrentState.LegacySceneLighting && !Infrastructure.FxInterop.ClassicRequest("sunColor"));
            view.Number(page1, "Moon tint", () => Runtime.TunerRuntime.CurrentState.MoonTint, v => Edit(() => Runtime.TunerRuntime.CurrentState.MoonTint = v), -1f, 1f, 0.01f, enabled: () => !Runtime.TunerRuntime.CurrentState.LegacySceneLighting && !Infrastructure.FxInterop.ClassicRequest("sunColor"));
            view.Number(page1, "Sky temperature", () => Runtime.TunerRuntime.CurrentState.SkyTemp, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyTemp = v), -1f, 1f, 0.01f);
            view.Number(page1, "Sky tint", () => Runtime.TunerRuntime.CurrentState.SkyTint, v => Edit(() => Runtime.TunerRuntime.CurrentState.SkyTint = v), -1f, 1f, 0.01f);
            view.Number(page1, "Twilight tint", () => Runtime.TunerRuntime.CurrentState.TwilightTint, v => Edit(() => Runtime.TunerRuntime.CurrentState.TwilightTint = v), -1f, 1f, 0.01f, enabled: () => !Runtime.TunerRuntime.CurrentState.LegacySceneLighting && !Infrastructure.FxInterop.ClassicRequest("sunColor"));
            var page2 = view.AddPage("Tone");
            view.Choice(page2, "Camera tonemapping", () => new[] { "Game", "Off", "On" }, () => Runtime.TunerRuntime.CurrentState.ToneEnabled + 1, v => Edit(() => Runtime.TunerRuntime.CurrentState.ToneEnabled = v - 1));
            view.Number(page2, "Brightness", () => Runtime.TunerRuntime.CurrentState.Brightness, v => Edit(() => Runtime.TunerRuntime.CurrentState.Brightness = v), -1f, 4f, 0.01f, enabled: () => Runtime.TunerRuntime.CurrentState.ToneEnabled != 0);
            view.Number(page2, "Gamma", () => Runtime.TunerRuntime.CurrentState.Gamma, v => Edit(() => Runtime.TunerRuntime.CurrentState.Gamma = v), 1.5f, 3.5f, 0.01f, enabled: () => Runtime.TunerRuntime.CurrentState.ToneEnabled != 0);
            view.Number(page2, "Contrast", () => Runtime.TunerRuntime.CurrentState.Contrast, v => Edit(() => Runtime.TunerRuntime.CurrentState.Contrast = v), -1f, 1f, 0.01f, enabled: () => Runtime.TunerRuntime.CurrentState.ToneEnabled != 0);
            view.Check(page2, "Soft shadows", () => Runtime.TunerRuntime.CurrentState.SoftShadows, v => Edit(() => Runtime.TunerRuntime.CurrentState.SoftShadows = v));
            view.Check(page2, "Adaptive shadow bias", () => Runtime.TunerRuntime.CurrentState.AdaptiveShadows, v => Edit(() => Runtime.TunerRuntime.CurrentState.AdaptiveShadows = v));
            view.Check(page2, "Force low shadow bias", () => Runtime.TunerRuntime.CurrentState.ForceLowBias, v => Edit(() => Runtime.TunerRuntime.CurrentState.ForceLowBias = v));
            view.Number(page2, "Shadow bias scale", () => Runtime.TunerRuntime.CurrentState.BiasScale, v => Edit(() => Runtime.TunerRuntime.CurrentState.BiasScale = v), 0f, 2f, 0.01f, enabled: () => Runtime.TunerRuntime.CurrentState.AdaptiveShadows);
            view.Check(page2, "Adaptive exposure", () => Runtime.TunerRuntime.CurrentState.AdaptiveExposure, v => Edit(() => Runtime.TunerRuntime.CurrentState.AdaptiveExposure = v));
            view.Number(page2, "Adaptive exposure gain", () => Runtime.TunerRuntime.CurrentState.AdaptiveExposureGain, v => Edit(() => Runtime.TunerRuntime.CurrentState.AdaptiveExposureGain = v), 0f, 1f, 0.01f, enabled: () => Runtime.TunerRuntime.CurrentState.AdaptiveExposure);
            view.Refresh();
            return view;
        }

        internal static void OpenStandalone(bool toggle = false)
        {
            if (_standalone == null || _standalone.Root == null)
            {
                _standalone = CreatePanel(null, PreferredWidth, Mathf.Min(PreferredHeight, UIView.GetAView().fixedHeight - 24f));
                _standalone.Root.relativePosition = new Vector3(Mathf.Clamp(WindowX, 0f, Mathf.Max(0f, UIView.GetAView().fixedWidth - PreferredWidth)), Mathf.Clamp(WindowY, 0f, Mathf.Max(0f, UIView.GetAView().fixedHeight - _standalone.Root.height)));
                _standalone.Root.eventPositionChanged += (c, value) => { WindowX = value.x; WindowY = value.y; SavePosition(); };
            }
            else _standalone.Root.isVisible = toggle ? !_standalone.Root.isVisible : true;
            var screen = UIView.GetAView();
            _standalone.SetSize(PreferredWidth, Mathf.Min(PreferredHeight, screen.fixedHeight - 24f));
            var pos = _standalone.Root.relativePosition;
            _standalone.Root.relativePosition = new Vector3(Mathf.Clamp(pos.x, 0f, Mathf.Max(0f, screen.fixedWidth - _standalone.Root.width)), Mathf.Clamp(pos.y, 0f, Mathf.Max(0f, screen.fixedHeight - _standalone.Root.height)));
            _standalone.Refresh();
        }

        internal static void CloseStandalone()
        {
            if (_standalone != null) _standalone.Dispose();
            _standalone = null;
        }
        private static void Edit(Action edit)
        {
            edit(); var state = Runtime.TunerRuntime.CurrentState; state.VanillaMode = false; state.LightingDirty = true;
            Runtime.TunerRuntime.ApplyAll(); IO.StateStore.Save(); Infrastructure.FxInterop.RefreshCompanions(); LumenFXMod.NotifyStateChanged();
        }
        private static float WindowX { get { return Runtime.TunerRuntime.CurrentState.WindowX; } set { Runtime.TunerRuntime.CurrentState.WindowX = value; } }
        private static float WindowY { get { return Runtime.TunerRuntime.CurrentState.WindowY; } set { Runtime.TunerRuntime.CurrentState.WindowY = value; } }
        private static void SavePosition() { IO.StateStore.Save(); }
    }
}

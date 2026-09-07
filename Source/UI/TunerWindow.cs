using System;
using System.Collections.Generic;
using UnityEngine;
using LumenFX.Core;
using LumenFX.IO;
using LumenFX.Presets;
using LumenFX.Runtime;

namespace LumenFX.UI
{
    /// <summary>
    /// v2 tuner window: 4 structured ergonomic tabs
    /// (☀️ Sol &amp; Cielo, 🎨 Balance de Color, 📐 Sombras &amp; Tono, 📁 Presets)
    /// with scroll areas and human-readable parameters. Opened with Ctrl + Alt + L.
    /// </summary>
    internal sealed class TunerWindow
    {
        private const float RowHeight = 28f;
        private const float LabelWidth = 140f;
        private const float SliderX = 150f;
        private const float SliderWidth = 280f;
        private const float ValueX = 438f;
        private const float ValueWidth = 80f;

        private static readonly string[] Tabs = { "Sol & Cielo", "Balance de Color", "Sombras & Tono", "Presets" };

        private readonly LightState _state;
        private readonly Action _onChanged;

        private Rect _rect = new Rect(620f, 260f, 550f, 510f);
        private int _tab;
        private Vector2 _scrollTab0;
        private Vector2 _scrollTab1;
        private Vector2 _scrollTab2;
        private Vector2 _presetScroll;
        private string _presetName = "My look";
        private List<PresetDocument> _presets;

        internal TunerWindow(LightState state, Action onChanged)
        {
            _state = state;
            _onChanged = onChanged;
            float x = state.WindowX > 0f ? state.WindowX : 620f;
            float y = state.WindowY > 0f ? state.WindowY : 260f;
            if (Screen.width > 0 && Screen.height > 0)
            {
                x = Mathf.Clamp(x, 10f, Mathf.Max(10f, Screen.width - 560f));
                y = Mathf.Clamp(y, 10f, Mathf.Max(10f, Screen.height - 520f));
            }
            _rect = new Rect(x, y, 550f, 510f);
        }

        internal void Draw(int id)
        {
            float oldX = _rect.x;
            float oldY = _rect.y;
            _rect = GUI.Window(id, _rect, DrawWindow, "LumenFX Studio v2");
            if (!Mathf.Approximately(oldX, _rect.x) || !Mathf.Approximately(oldY, _rect.y))
            {
                _state.WindowX = _rect.x;
                _state.WindowY = _rect.y;
                StateStore.Save(false);
            }
        }

        private void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0f, 0f, _rect.width - 30f, 22f));
            if (GUI.Button(new Rect(_rect.width - 26f, 4f, 22f, 18f), "x"))
            {
                TunerEngine.CloseWindow();
            }

            _tab = GUI.Toolbar(new Rect(8f, 26f, _rect.width - 16f, 26f), _tab, Tabs);

            BeginChangeCheck();

            Rect contentArea = new Rect(8f, 56f, _rect.width - 16f, _rect.height - 66f);
            if (_tab == 0)
            {
                DrawSunSkyTab(contentArea);
            }
            else if (_tab == 1)
            {
                DrawColorBalanceTab(contentArea);
            }
            else if (_tab == 2)
            {
                DrawShadowsToneTab(contentArea);
            }
            else
            {
                DrawPresetsTab(contentArea);
            }

            if (EndChangeCheck())
            {
                _onChanged();
            }
        }

        private bool _dirty;

        private void BeginChangeCheck()
        {
            _dirty = false;
        }

        private bool EndChangeCheck()
        {
            return _dirty;
        }

        private void MarkDirty()
        {
            _dirty = true;
        }

        private static float Section(string title, float y)
        {
            GUI.Label(new Rect(8f, y, 400f, 22f), "<b><color=#4FC3F7>" + title + "</color></b>");
            return y + 24f;
        }

        private void DrawSunSkyTab(Rect area)
        {
            _scrollTab0 = GUI.BeginScrollView(area, _scrollTab0, new Rect(0f, 0f, area.width - 20f, 430f));
            float y = 4f;

            y = Section("GLOBAL MODE & RESTORE", y);
            _state.VanillaMode = Toggle("Modo Vanilla (suspender LumenFX)", _state.VanillaMode, y);
            if (GUI.Button(new Rect(320f, y, 180f, 22f), "Restaurar Vanilla"))
            {
                _state.ResetToNeutral();
                TunerRuntime.RestoreVanilla();
                MarkDirty();
            }
            y += RowHeight + 4f;

            y = Section("INTENSIDADES MAESTRAS (SOL, LUNA Y AMBIENTE)", y);
            _state.SunStrength = Slider("Fuerza del Sol", _state.SunStrength, 0f, 3f, 0.05f, y); y += RowHeight;
            _state.MoonStrength = Slider("Fuerza de la Luna", _state.MoonStrength, 0f, 2f, 0.05f, y); y += RowHeight;
            _state.Ambience = Slider("Luz Ambiental", _state.Ambience, 0f, 2f, 0.05f, y); y += RowHeight;
            _state.Warmth = Slider("Calidez General", _state.Warmth, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.SkyTonemapping = Toggle("Tonemapping en domo celeste", _state.SkyTonemapping, y); y += RowHeight + 4f;

            y = Section("PHYSICAL POWER & ATMOSPHERIC SCATTERING", y);
            _state.SunPower = Slider("Potencia Abs. Sol (0 = auto)", _state.SunPower, 0f, 20f, 0.1f, y); y += RowHeight;
            _state.MoonPower = Slider("Potencia Abs. Luna (0 = auto)", _state.MoonPower, 0f, 20f, 0.1f, y); y += RowHeight;
            _state.SkyRayleigh = Slider("Rayleigh scattering", _state.SkyRayleigh, 0f, 5f, 0.01f, y); y += RowHeight;
            _state.SkyMie = Slider("Mie scattering", _state.SkyMie, 0f, 5f, 0.01f, y); y += RowHeight + 6f;

            if (GUI.Button(new Rect(SliderX, y, SliderWidth, 24f), "Reset this tab to neutral"))
            {
                _state.SunStrength = 1f;
                _state.MoonStrength = 1f;
                _state.Ambience = 1f;
                _state.Warmth = 0f;
                _state.SkyTonemapping = true;
                _state.SkyRayleigh = 0f;
                _state.SkyMie = 0f;
                _state.SunPower = 0f;
                _state.MoonPower = 0f;
                MarkDirty();
            }

            GUI.EndScrollView();
        }

        private void DrawColorBalanceTab(Rect area)
        {
            _scrollTab1 = GUI.BeginScrollView(area, _scrollTab1, new Rect(0f, 0f, area.width - 20f, 360f));
            float y = 4f;

            y = Section("LUZ DIURNA (SOL & CIELO)", y);
            _state.SunTemp = Slider("Sun temperature (cool/warm)", _state.SunTemp, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.SunTint = Slider("Tinte Solar (Verde/Magenta)", _state.SunTint, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.SkyTemp = Slider("Temp. Cielo", _state.SkyTemp, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.SkyTint = Slider("Tinte Cielo", _state.SkyTint, -1f, 1f, 0.05f, y); y += RowHeight + 4f;

            y = Section("NIGHT LIGHT & TWILIGHT", y);
            _state.MoonTemp = Slider("Temp. Lunar", _state.MoonTemp, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.MoonTint = Slider("Tinte Lunar", _state.MoonTint, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.TwilightTint = Slider("Tinte Crepuscular", _state.TwilightTint, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.GlobalTint = Slider("Tinte Global", _state.GlobalTint, -1f, 1f, 0.05f, y); y += RowHeight + 6f;

            if (GUI.Button(new Rect(SliderX, y, SliderWidth, 24f), "Restablecer tintes a neutro (0)"))
            {
                _state.SunTemp = 0f;
                _state.SunTint = 0f;
                _state.MoonTemp = 0f;
                _state.MoonTint = 0f;
                _state.SkyTemp = 0f;
                _state.SkyTint = 0f;
                _state.GlobalTint = 0f;
                _state.TwilightTint = 0f;
                MarkDirty();
            }

            GUI.EndScrollView();
        }

        private void DrawShadowsToneTab(Rect area)
        {
            _scrollTab2 = GUI.BeginScrollView(area, _scrollTab2, new Rect(0f, 0f, area.width - 20f, 430f));
            float y = 4f;

            y = Section("MAPEO DE TONOS (FILMIC TONEMAPPING)", y);
            _state.Brightness = Slider("Brillo", _state.Brightness, -1f, 4f, 0.05f, y); y += RowHeight;
            _state.Contrast = Slider("Contraste", _state.Contrast, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.Gamma = Slider("Gamma", _state.Gamma, 1.5f, 3.5f, 0.05f, y); y += RowHeight + 4f;

            y = Section("SHADOW QUALITY & BIAS", y);
            _state.SoftShadows = Toggle("Sombras suaves (Soft shadows)", _state.SoftShadows, y); y += RowHeight;
            _state.AdaptiveShadows = Toggle("Sesgo de sombra adaptativo (Anti-acne)", _state.AdaptiveShadows, y); y += RowHeight;
            _state.ForceLowBias = Toggle("Forzar sesgo bajo", _state.ForceLowBias, y); y += RowHeight;
            _state.BiasScale = Slider("Escala de sesgo", _state.BiasScale, 0f, 2f, 0.05f, y); y += RowHeight + 4f;

            y = Section("ADAPTIVE DAY/NIGHT EXPOSURE", y);
            _state.AdaptiveExposure = Toggle("Adaptive exposure", _state.AdaptiveExposure, y); y += RowHeight;
            if (_state.AdaptiveExposure)
            {
                _state.AdaptiveExposureGain = Slider("Ganancia nocturna", _state.AdaptiveExposureGain, 0f, 1f, 0.05f, y); y += RowHeight;
            }
            y += 6f;

            if (GUI.Button(new Rect(SliderX, y, SliderWidth, 24f), "Restablecer tono y sombras"))
            {
                _state.Brightness = 0f;
                _state.Contrast = 0f;
                _state.Gamma = 2.2f;
                _state.AdaptiveShadows = true;
                _state.ForceLowBias = false;
                _state.BiasScale = 1f;
                _state.SoftShadows = true;
                _state.AdaptiveExposure = false;
                _state.AdaptiveExposureGain = 0.5f;
                MarkDirty();
            }

            GUI.EndScrollView();
        }

        private void DrawPresetsTab(Rect area)
        {
            float y = area.y;

            if (GUI.Button(new Rect(area.x, y, 120f, 26f), "Vanilla"))
            {
                QuickPresets.ApplyVanilla();
            }

            if (GUI.Button(new Rect(area.x + 130f, y, 140f, 26f), "Optimized"))
            {
                QuickPresets.ApplyOptimized();
            }

            if (GUI.Button(new Rect(area.x + 280f, y, 110f, 26f), "Refrescar"))
            {
                _presets = PresetLibrary.LoadAll();
            }

            if (GUI.Button(new Rect(area.x + 400f, y, 120f, 26f), "Carpeta"))
            {
                PresetLibrary.EnsureFolder();
                Application.OpenURL("file://" + PresetLibrary.Folder);
            }

            y += 34f;
            _presets = _presets ?? PresetLibrary.LoadAll();

            float listHeight = 220f;
            _presetScroll = GUI.BeginScrollView(new Rect(area.x, y, area.width, listHeight), _presetScroll,
                new Rect(0f, 0f, area.width - 24f, Mathf.Max(1, _presets.Count) * 28f));

            float rowY = 0f;
            foreach (var preset in _presets)
            {
                GUI.Label(new Rect(4f, rowY + 3f, 280f, 24f), preset.Name);
                if (GUI.Button(new Rect(290f, rowY, 70f, 24f), "Cargar"))
                {
                    ApplyPreset(preset);
                }

                if (GUI.Button(new Rect(368f, rowY, 70f, 24f), "Borrar"))
                {
                    PresetLibrary.Delete(preset);
                    _presets = PresetLibrary.LoadAll();
                }

                rowY += 28f;
            }

            GUI.EndScrollView();

            y += listHeight + 10f;
            GUI.Label(new Rect(area.x, y, 120f, 22f), "Nombre del preset:");
            _presetName = GUI.TextField(new Rect(area.x + 130f, y, 260f, 24f), _presetName);
            if (GUI.Button(new Rect(area.x + 400f, y, 120f, 24f), "Guardar"))
            {
                PresetLibrary.Save(DocumentFromState(_presetName));
                _presets = PresetLibrary.LoadAll();
            }
        }

        private void ApplyPreset(PresetDocument preset)
        {
            _state.SunStrength = preset.SunStrength;
            _state.MoonStrength = preset.MoonStrength;
            _state.Ambience = preset.Ambience;
            _state.Warmth = preset.Warmth;
            _state.SunTemp = preset.SunTemp;
            _state.SunTint = preset.SunTint;
            _state.MoonTemp = preset.MoonTemp;
            _state.MoonTint = preset.MoonTint;
            _state.SkyTemp = preset.SkyTemp;
            _state.SkyTint = preset.SkyTint;
            _state.GlobalTint = preset.GlobalTint;
            _state.TwilightTint = preset.TwilightTint;
            _state.SkyTonemapping = preset.SkyTonemapping;
            _state.Brightness = preset.Brightness;
            _state.Contrast = preset.Contrast;
            _state.Gamma = preset.Gamma;
            _state.AdaptiveShadows = preset.AdaptiveShadows;
            _state.ForceLowBias = preset.ForceLowBias;
            _state.BiasScale = preset.BiasScale;
            _state.SoftShadows = preset.SoftShadows;
            _state.AdaptiveExposure = preset.AdaptiveExposure;
            _state.AdaptiveExposureGain = preset.AdaptiveExposureGain;
            MarkDirty();
        }

        private static PresetDocument DocumentFromState(string name)
        {
            var state = TunerRuntime.CurrentState;
            return new PresetDocument
            {
                Name = PresetLibrary.Sanitize(name),
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
            };
        }

        private float Slider(string label, float value, float min, float max, float step, float y)
        {
            GUI.Label(new Rect(10f, y, LabelWidth, 24f), label);

            bool changedBefore = GUI.changed;
            GUI.changed = false;
            float raw = GUI.HorizontalSlider(new Rect(SliderX, y + 3f, SliderWidth, 22f), value, min, max);
            bool moved = GUI.changed;
            GUI.changed = changedBefore || moved;

            if (!moved)
            {
                GUI.Label(new Rect(ValueX, y, ValueWidth, 24f), value.ToString("0.00"));
                return value;
            }

            float snapped = Mathf.Round(raw / step) * step;
            GUI.Label(new Rect(ValueX, y, ValueWidth, 24f), snapped.ToString("0.00"));
            if (!Mathf.Approximately(snapped, value))
            {
                MarkDirty();
            }

            return snapped;
        }

        private bool Toggle(string label, bool value, float y)
        {
            bool result = GUI.Toggle(new Rect(10f, y, 380f, 24f), value, " " + label);
            if (result != value)
            {
                MarkDirty();
            }

            return result;
        }
    }
}

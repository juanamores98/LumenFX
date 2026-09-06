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
    /// v2 tuner window: three tabs (Sun &amp; Sky, Tone &amp; Shadows, Presets)
    /// with its own layout, opened with Ctrl + Alt + L.
    /// </summary>
    internal sealed class TunerWindow
    {
        private const float RowHeight = 30f;
        private const float LabelWidth = 120f;
        private const float SliderX = 135f;
        private const float SliderWidth = 290f;
        private const float ValueX = 435f;
        private const float ValueWidth = 90f;

        private static readonly string[] Tabs = { "Sun && Sky", "Advanced", "Tone && Shadows", "Presets" };

        private readonly LightState _state;
        private readonly Action _onChanged;

        private Rect _rect = new Rect(620f, 300f, 540f, 440f);
        private int _tab;
        private Vector2 _presetScroll;
        private string _presetName = "My look";
        private List<PresetDocument> _presets = new List<PresetDocument>();

        internal TunerWindow(LightState state, Action onChanged)
        {
            _state = state;
            _onChanged = onChanged;
            float x = state.WindowX > 0f ? state.WindowX : 620f;
            float y = state.WindowY > 0f ? state.WindowY : 300f;
            if (Screen.width > 0 && Screen.height > 0)
            {
                x = Mathf.Clamp(x, 10f, Mathf.Max(10f, Screen.width - 550f));
                y = Mathf.Clamp(y, 10f, Mathf.Max(10f, Screen.height - 480f));
            }
            _rect = new Rect(x, y, 540f, 470f);
        }

        internal void Draw(int id)
        {
            float oldX = _rect.x;
            float oldY = _rect.y;
            _rect = GUI.Window(id, _rect, DrawWindow, "LumenFX v2");
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

            _tab = GUI.Toolbar(new Rect(8f, 26f, _rect.width - 16f, 24f), _tab, Tabs);

            BeginChangeCheck();

            if (_tab == 0)
            {
                DrawSunSkyTab();
            }
            else if (_tab == 1)
            {
                DrawAdvancedTab();
            }
            else if (_tab == 2)
            {
                DrawToneTab();
            }
            else
            {
                DrawPresetsTab();
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
            GUI.Label(new Rect(10f, y, 350f, 22f), "<b><color=#4FC3F7>" + title + "</color></b>");
            return y + 24f;
        }

        private void DrawSunSkyTab()
        {
            float y = 58f;

            y = Section("GLOBAL & MODE", y);
            _state.VanillaMode = Toggle("Vanilla mode (suspend LumenFX)", _state.VanillaMode, y); y += RowHeight;

            if (GUI.Button(new Rect(SliderX, y + 4f, SliderWidth, 24f), "Restore vanilla now"))
            {
                _state.ResetToNeutral();
                TunerRuntime.RestoreVanilla();
                MarkDirty();
                return;
            }
            y += RowHeight + 6f;

            y = Section("SUN & SKY GAIN", y);
            _state.SunStrength = Slider("Sun strength", _state.SunStrength, 0f, 2f, 0.05f, y); y += RowHeight;
            _state.MoonStrength = Slider("Moon strength", _state.MoonStrength, 0f, 2f, 0.05f, y); y += RowHeight;
            _state.Ambience = Slider("Ambience", _state.Ambience, 0f, 2f, 0.05f, y); y += RowHeight;
            _state.Warmth = Slider("Warmth", _state.Warmth, -1f, 1f, 0.05f, y); y += RowHeight;

            _state.SkyTonemapping = Toggle("Sky tonemapping", _state.SkyTonemapping, y); y += RowHeight;

            if (GUI.Button(new Rect(SliderX, y + 4f, SliderWidth, 24f), "Reset this tab"))
            {
                _state.SunStrength = 1f;
                _state.MoonStrength = 1f;
                _state.Ambience = 1f;
                _state.Warmth = 0f;
                _state.SkyTonemapping = true;
                MarkDirty();
            }
        }

        private void DrawAdvancedTab()
        {
            float y = 58f;

            y = Section("SOURCE TEMPERATURE & TINT", y);
            _state.SunTemp = Slider("Sun temp", _state.SunTemp, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.SunTint = Slider("Sun tint", _state.SunTint, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.MoonTemp = Slider("Moon temp", _state.MoonTemp, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.MoonTint = Slider("Moon tint", _state.MoonTint, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.SkyTemp = Slider("Sky temp", _state.SkyTemp, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.SkyTint = Slider("Sky tint", _state.SkyTint, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.GlobalTint = Slider("Global tint", _state.GlobalTint, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.TwilightTint = Slider("Twilight tint", _state.TwilightTint, -1f, 1f, 0.05f, y); y += RowHeight;

            if (GUI.Button(new Rect(SliderX, y + 4f, SliderWidth, 24f), "Reset this tab"))
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
        }

        private void DrawToneTab()
        {
            float y = 58f;

            y = Section("TONE MAPPING", y);
            _state.Brightness = Slider("Brightness", _state.Brightness, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.Contrast = Slider("Contrast", _state.Contrast, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.Gamma = Slider("Gamma", _state.Gamma, 1.5f, 3.5f, 0.05f, y); y += RowHeight;

            y = Section("SHADOWS & EXPOSURE", y);
            _state.AdaptiveShadows = Toggle("Adaptive shadow bias", _state.AdaptiveShadows, y); y += RowHeight;
            _state.ForceLowBias = Toggle("Force low bias", _state.ForceLowBias, y); y += RowHeight;
            _state.BiasScale = Slider("Bias scale", _state.BiasScale, 0f, 2f, 0.05f, y); y += RowHeight;
            _state.SoftShadows = Toggle("Soft shadows", _state.SoftShadows, y); y += RowHeight;

            _state.AdaptiveExposure = Toggle("Adaptive day/night exposure", _state.AdaptiveExposure, y); y += RowHeight;
            if (_state.AdaptiveExposure)
            {
                _state.AdaptiveExposureGain = Slider("Exposure lift gain", _state.AdaptiveExposureGain, 0f, 1f, 0.05f, y); y += RowHeight;
            }

            if (GUI.Button(new Rect(SliderX, y + 4f, SliderWidth, 24f), "Reset this tab"))
            {
                _state.Brightness = 0f;
                _state.Contrast = 0f;
                _state.Gamma = 2.6f;
                _state.AdaptiveShadows = true;
                _state.ForceLowBias = false;
                _state.BiasScale = 1f;
                _state.SoftShadows = true;
                _state.AdaptiveExposure = false;
                _state.AdaptiveExposureGain = 0.5f;
                MarkDirty();
            }
        }

        private void DrawPresetsTab()
        {
            if (GUI.Button(new Rect(8f, 56f, 120f, 24f), "Refresh"))
            {
                _presets = PresetLibrary.LoadAll();
            }

            if (GUI.Button(new Rect(134f, 56f, 120f, 24f), "Open folder"))
            {
                PresetLibrary.EnsureFolder();
                Application.OpenURL("file://" + PresetLibrary.Folder);
            }

            _presets = _presets ?? PresetLibrary.LoadAll();

            float listHeight = 180f;
            _presetScroll = GUI.BeginScrollView(new Rect(8f, 86f, _rect.width - 16f, listHeight), _presetScroll,
                new Rect(0f, 0f, _rect.width - 40f, Mathf.Max(1, _presets.Count) * 28f));

            float rowY = 0f;
            foreach (var preset in _presets)
            {
                GUI.Label(new Rect(4f, rowY + 3f, 250f, 24f), preset.Name);
                if (GUI.Button(new Rect(260f, rowY, 60f, 24f), "Load"))
                {
                    ApplyPreset(preset);
                }

                if (GUI.Button(new Rect(326f, rowY, 60f, 24f), "Delete"))
                {
                    PresetLibrary.Delete(preset);
                    _presets = PresetLibrary.LoadAll();
                }

                rowY += 28f;
            }

            GUI.EndScrollView();

            float y = 86f + listHeight + 10f;
            _presetName = GUI.TextField(new Rect(8f, y, _rect.width - 16f, 24f), _presetName);
            y += 30f;

            if (GUI.Button(new Rect(8f, y, _rect.width - 16f, 26f), "Save current look as preset"))
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
            float raw = GUI.HorizontalSlider(new Rect(SliderX, y + 3f, SliderWidth, 22f), value, min, max);
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
            bool result = GUI.Toggle(new Rect(10f, y, 320f, 24f), value, label);
            if (result != value)
            {
                MarkDirty();
            }

            return result;
        }
    }
}

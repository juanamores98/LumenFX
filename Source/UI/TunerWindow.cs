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

        private static readonly string[] Tabs = { "Sun & Sky", "Tone & Shadows", "Presets" };

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
        }

        internal void Draw(int id)
        {
            _rect = GUI.Window(id, _rect, DrawWindow, "LumenFX v2");
        }

        private void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0f, 0f, 500f, 22f));
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

        private void DrawSunSkyTab()
        {
            float y = 64f;

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

        private void DrawToneTab()
        {
            float y = 64f;

            _state.Brightness = Slider("Brightness", _state.Brightness, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.Contrast = Slider("Contrast", _state.Contrast, -1f, 1f, 0.05f, y); y += RowHeight;
            _state.Gamma = Slider("Gamma", _state.Gamma, 1.5f, 3.5f, 0.05f, y); y += RowHeight;

            _state.AdaptiveShadows = Toggle("Adaptive shadow bias", _state.AdaptiveShadows, y); y += RowHeight;
            _state.BiasScale = Slider("Bias scale", _state.BiasScale, 0f, 2f, 0.05f, y); y += RowHeight;
            _state.SoftShadows = Toggle("Soft shadows", _state.SoftShadows, y); y += RowHeight;

            if (GUI.Button(new Rect(SliderX, y + 4f, SliderWidth, 24f), "Reset this tab"))
            {
                _state.Brightness = 0f;
                _state.Contrast = 0f;
                _state.Gamma = 2.6f;
                _state.AdaptiveShadows = true;
                _state.BiasScale = 1f;
                _state.SoftShadows = true;
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
            _state.SkyTonemapping = preset.SkyTonemapping;
            _state.Brightness = preset.Brightness;
            _state.Contrast = preset.Contrast;
            _state.Gamma = preset.Gamma;
            _state.AdaptiveShadows = preset.AdaptiveShadows;
            _state.BiasScale = preset.BiasScale;
            _state.SoftShadows = preset.SoftShadows;
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
                SkyTonemapping = state.SkyTonemapping,
                Brightness = state.Brightness,
                Contrast = state.Contrast,
                Gamma = state.Gamma,
                AdaptiveShadows = state.AdaptiveShadows,
                BiasScale = state.BiasScale,
                SoftShadows = state.SoftShadows,
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

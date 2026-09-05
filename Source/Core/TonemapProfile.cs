using UnityEngine;

namespace LumenFX.Core
{
    /// <summary>
    /// v2 tonemapping profile: exposes exposure-like controls directly and
    /// maps them onto the game's filmic curve with its own coefficients.
    /// </summary>
    internal static class TonemapProfile
    {
        internal static void Apply(LightState state)
        {
            var cameraObject = GameObject.Find("Main Camera");
            if (cameraObject == null)
            {
                return;
            }

            var toneMap = cameraObject.GetComponent<ColossalFramework.ToneMapping>();
            if (toneMap == null)
            {
                return;
            }

            float b = state.Brightness;
            float c = state.Contrast;

            toneMap.m_ToneMappingGamma = state.Gamma;
            toneMap.m_ToneMappingBoostFactor = 1f + 0.6f * b;
            toneMap.m_Luminance = 0.10f + 0.02f * c;

            toneMap.m_ToneMappingParamsFilmic.A = 0.50f + 0.20f * c;
            toneMap.m_ToneMappingParamsFilmic.B = 0.25f - 0.15f * c;
            toneMap.m_ToneMappingParamsFilmic.C = 0.10f - 0.01f * c;
            toneMap.m_ToneMappingParamsFilmic.D = 0.70f + 0.20f * c;
            toneMap.m_ToneMappingParamsFilmic.E = 0.01f;
            toneMap.m_ToneMappingParamsFilmic.F = 0.25f - 0.12f * c;
            toneMap.m_ToneMappingParamsFilmic.W = 11.2f + 2.5f * c;
        }
    }
}

using Harmony;
using LumenFX.Core;
using LumenFX.Runtime;

namespace LumenFX.Shadows
{
    /// <summary>
    /// Feeds the v2 adaptive bias into the main light and smooth day/night
    /// adaptive exposure whenever the game refreshes its lighting.
    /// </summary>
    [HarmonyPatch(typeof(DayNightProperties))]
    [HarmonyPatch("UpdateLighting")]
    internal static class UpdateLightingPatch
    {
        private static bool _adaptiveExposureActive;

        private static void Postfix(DayNightProperties __instance)
        {
            var state = TunerRuntime.CurrentState;
            if (state == null || state.VanillaMode)
            {
                return;
            }

            if (state.AdaptiveShadows && RenderManager.instance != null && RenderManager.instance.MainLight != null)
            {
                RenderManager.instance.MainLight.shadowBias = AdaptiveBias.Compute(state);
            }

            bool exposureApplied = false;
            if (state.SkyExposure > 0f && __instance != null) { __instance.m_Exposure = state.SkyExposure; exposureApplied = true; }
            // La exposición es del tema del mapa cuando hay quien lo administre.
            if (state.AdaptiveExposure && __instance != null && VanillaSnapshot.Captured
                && !ThemeOwnership.AtmosphereIsManaged)
            {
                float factor = AdaptiveExposure.Compute(__instance.normalizedTimeOfDay, state.AdaptiveExposureGain);
                __instance.m_Exposure = (state.SkyExposure > 0f ? state.SkyExposure : VanillaSnapshot.CapturedExposure) * factor;
                exposureApplied = true;
            }

            if (_adaptiveExposureActive && !exposureApplied)
            {
                if (__instance != null && VanillaSnapshot.Captured)
                {
                    __instance.m_Exposure = VanillaSnapshot.CapturedExposure;
                }

                AdaptiveExposure.Reset();
            }

            _adaptiveExposureActive = exposureApplied;
        }
    }
}


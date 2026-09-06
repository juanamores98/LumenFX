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

            if (state.AdaptiveExposure && __instance != null && VanillaSnapshot.Captured)
            {
                float factor = AdaptiveExposure.Compute(__instance.normalizedTimeOfDay, state.AdaptiveExposureGain);
                __instance.m_Exposure = VanillaSnapshot.CapturedExposure * factor;
            }
        }
    }
}


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

            if (RenderManager.instance != null && RenderManager.instance.MainLight != null)
            {
                var light = RenderManager.instance.MainLight;
                if (state.AdaptiveShadows) Infrastructure.PropertyLedger.Write(light, "shadowBias", AdaptiveBias.Compute(state));
                else Infrastructure.PropertyLedger.Release(light, "shadowBias");
            }
            if (__instance == null) return;
            bool classic = Infrastructure.FxInterop.ClassicRequest("sunStrength");
            if (classic) Infrastructure.PropertyLedger.Write(__instance, "m_Exposure", 1f);
            else if (state.AdaptiveExposure && !ThemeOwnership.AtmosphereIsManaged)
            {
                float baseline = state.SkyExposure > 0f ? state.SkyExposure : Infrastructure.PropertyLedger.Baseline<float>(__instance, "m_Exposure");
                Infrastructure.PropertyLedger.Write(__instance, "m_Exposure", baseline * AdaptiveExposure.Compute(__instance.normalizedTimeOfDay, state.AdaptiveExposureGain));
            }
            else if (state.SkyExposure > 0f) Infrastructure.PropertyLedger.Write(__instance, "m_Exposure", state.SkyExposure);
            else { Infrastructure.PropertyLedger.Release(__instance, "m_Exposure"); AdaptiveExposure.Reset(); }
        }
    }
}

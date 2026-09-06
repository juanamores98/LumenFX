using Harmony;
using LumenFX.Runtime;

namespace LumenFX.Shadows
{
    /// <summary>
    /// Feeds the v2 adaptive bias into the main light whenever the game
    /// refreshes its lighting.
    /// </summary>
    [HarmonyPatch(typeof(DayNightProperties))]
    [HarmonyPatch("UpdateLighting")]
    internal static class UpdateLightingPatch
    {
        private static void Postfix()
        {
            if (RenderManager.instance == null || RenderManager.instance.MainLight == null)
            {
                return;
            }

            var state = TunerRuntime.CurrentState;
            if (state != null && state.AdaptiveShadows && !state.VanillaMode)
            {
                RenderManager.instance.MainLight.shadowBias = AdaptiveBias.Compute(state);
            }
        }
    }
}

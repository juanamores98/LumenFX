using UnityEngine;

namespace LumenFX.Presets
{
    /// <summary>
    /// Los dos ajustes de un clic: el del juego sin tocar, y la receta del usuario.
    /// </summary>
    /// <remarks>
    /// <b>Que son.</b> <c>Vanilla</c> deja este mod sin imponer nada: el juego tal cual.
    /// <c>Optimized</c> es la receta calibrada del usuario, la misma que llevaba el preset
    /// «Default» de Render It!+, derivada en su dia de como tenia configurados los mods
    /// clasicos que esta suite sustituye.
    ///
    /// <b>Por que pasan por ApplySuiteSection.</b> Es el mismo camino que recorre un perfil de
    /// suite guardado, con sus validaciones y sus efectos inmediatos. Un atajo que escribiera
    /// los campos por su cuenta se desincronizaria del resto en cuanto alguien anadiera un
    /// ajuste nuevo.
    /// </remarks>
    internal static class QuickPresets
    {
        private const string VanillaXml =
            "<lumenfx>" +
            "<vanillaMode>true</vanillaMode>" +
            "</lumenfx>";

        private const string OptimizedXml =
            "<lumenfx>" +
            "<vanillaMode>false</vanillaMode>" +
            "<sunTemp>0.55</sunTemp>" +
            "<sunTint>-0.5</sunTint>" +
            "<moonTemp>0.2</moonTemp>" +
            "<moonTint>-0.5</moonTint>" +
            "<skyTemp>-0.7</skyTemp>" +
            "<skyTint>-0.25</skyTint>" +
            "<globalTint>0.25</globalTint>" +
            "<twilightTint>-0.1</twilightTint>" +
            "<skyTonemapping>true</skyTonemapping>" +
            "<brightness>-0.4</brightness>" +
            "<contrast>-0.7</contrast>" +
            "<gamma>3.15</gamma>" +
            "<softShadows>true</softShadows>" +
            "<forceLowBias>false</forceLowBias>" +
            "<biasScale>1</biasScale>" +
            "<warmth>0.4</warmth>" +
            "<moonStrength>0.5</moonStrength>" +
            "<adaptiveShadows>true</adaptiveShadows>" +
            "<adaptiveExposure>true</adaptiveExposure>" +
            "<adaptiveExposureGain>1</adaptiveExposureGain>" +
            "<skyRayleigh>1.116</skyRayleigh>" +
            "<skyMie>1.858</skyMie>" +
            "</lumenfx>";

        internal static bool ApplyVanilla()
        {
            return Apply(VanillaXml, "Vanilla");
        }

        internal static bool ApplyOptimized()
        {
            return Apply(OptimizedXml, "Optimized");
        }

        private static bool Apply(string xml, string name)
        {
            bool ok = LumenFX.LumenFXMod.ApplySuiteSection(xml);
            Debug.Log("[LumenFX] preset " + name + (ok ? " aplicado" : " RECHAZADO"));
            return ok;
        }
    }
}

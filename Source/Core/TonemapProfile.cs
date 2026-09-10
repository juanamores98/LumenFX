using UnityEngine;

namespace LumenFX.Core
{
    /// <summary>
    /// v2 tonemapping profile: exposes exposure-like controls directly and
    /// maps them onto the game's filmic curve with its own coefficients.
    /// </summary>
    internal static class TonemapProfile
    {
        /// <summary>El realce que corresponde a un valor de brillo.</summary>
        /// <remarks>
        /// <b>Por que dos tramos.</b> De -1 a 1 la curva es la de siempre, <c>1 + 0,6b</c>, y
        /// llega a 1,6. Ninguna receta guardada cambia de aspecto por esto: todas caen en ese
        /// tramo. Por encima de 1 la recta sigue mas inclinada hasta 4,1 en b = 4, que es el
        /// techo que se le atribuye al control de brillo de Relight. La norma de la suite es
        /// conservar siempre el rango mas ancho de los mods que se sustituyen, y esta es la
        /// forma de ensancharlo sin mover ni un valor de los que ya estaban.
        ///
        /// El techo de 4,1 viene de un informe externo, no de haber leido el codigo de Relight
        /// —que no tiene licencia y no se toca—. Si algun dia se mide de verdad y sale otro,
        /// el que cambia es el segundo tramo; el primero se queda como esta.
        /// </remarks>
        internal static float BoostFor(float brightness)
        {
            if (brightness <= 1f)
            {
                return 1f + 0.6f * brightness;
            }

            return 1.6f + 0.84f * (brightness - 1f);
        }

        /// <summary>Todo lo que este metodo llega a escribir, para poder soltarlo.</summary>
        private static readonly string[] Held =
        {
            "enabled", "m_ToneMappingGamma", "m_ToneMappingBoostFactor", "m_Luminance",
            "m_ToneMappingParamsFilmic.A", "m_ToneMappingParamsFilmic.B",
            "m_ToneMappingParamsFilmic.C", "m_ToneMappingParamsFilmic.D",
            "m_ToneMappingParamsFilmic.E", "m_ToneMappingParamsFilmic.F",
            "m_ToneMappingParamsFilmic.W"
        };

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

            if (!state.OwnsTonemapping)
            {
                // El perfil no da el mando aqui: se devuelve la curva del juego entera.
                // Escribirle encima la neutra de este motor lavaria la imagen, que es
                // justo lo que no debe pasar cuando no se ha pedido nada.
                foreach (string held in Held)
                {
                    Infrastructure.PropertyLedger.Release(toneMap, held);
                }

                return;
            }

            if (state.ToneEnabled < 0) Infrastructure.PropertyLedger.Release(toneMap, "enabled");
            else Infrastructure.PropertyLedger.Write(toneMap, "enabled", state.ToneEnabled == 1);

            if (state.ToneOverride != null && state.ToneOverride.Length == Held.Length - 1)
            {
                // El anfitrion trae su propia calibracion. Se escribe tal cual: mezclarla
                // con la de este motor daria una tercera que no es la de nadie.
                for (int i = 1; i < Held.Length; i++)
                {
                    Infrastructure.PropertyLedger.Write(toneMap, Held[i], state.ToneOverride[i - 1]);
                }

                return;
            }

            float b = state.Brightness;
            float c = state.Contrast;

            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingGamma", state.Gamma);
            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingBoostFactor", BoostFor(b));
            Infrastructure.PropertyLedger.Write(toneMap, "m_Luminance", 0.10f + 0.02f * c);

            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingParamsFilmic.A", 0.50f + 0.20f * c);
            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingParamsFilmic.B", 0.25f - 0.15f * c);
            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingParamsFilmic.C", 0.10f - 0.01f * c);
            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingParamsFilmic.D", 0.70f + 0.20f * c);
            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingParamsFilmic.E", 0.01f);
            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingParamsFilmic.F", 0.25f - 0.12f * c);
            Infrastructure.PropertyLedger.Write(toneMap, "m_ToneMappingParamsFilmic.W", 11.2f + 2.5f * c);
        }
    }
}

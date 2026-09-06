using UnityEngine;
using LumenFX.Core;

namespace LumenFX.Shadows
{
    /// <summary>
    /// v2 shadow bias model. One forward ground probe is combined with the
    /// camera height and mapped through a linear response curve. The scale
    /// slider shifts the whole curve up or down.
    /// </summary>
    public static class AdaptiveBias
    {
        private const float HeightBlend = 0.5f;   // camera height vs probe weight
        private const float HeightNorm = 500f;    // distance that saturates the height term
        private const float PitchFloor = 0.35f;   // probe influence when looking straight down
        private const float BiasFloor = 0.05f;
        private const float BiasCeiling = 2f;

        internal static float Compute(LightState state)
        {
            var cameraController = ToolsModifierControl.cameraController;
            if (cameraController == null || Camera.main == null)
            {
                return 0.5f;
            }

            float probe = GroundProbe.Distance();
            float cameraHeight = Mathf.Max(0f, cameraController.transform.position.y - cameraController.m_currentHeight);
            float height = HeightBlend * cameraHeight + (1f - HeightBlend) * probe;

            float heightTerm = Mathf.Clamp01(height / HeightNorm);
            float pitch = Mathf.Clamp01(cameraController.m_targetAngle.y / 90f);
            float response = heightTerm * Mathf.Lerp(1f, PitchFloor, pitch);

            float bias = Mathf.Lerp(BiasFloor, 1f, response) * state.BiasScale;

            if (state.ForceLowBias)
            {
                // Keep the bias tight so ground shadows stay crisp even when
                // the camera climbs.
                bias = Mathf.Min(bias, 0.45f) * 0.75f;
            }
            return Mathf.Clamp(bias, BiasFloor, BiasCeiling);
        }
    }
}

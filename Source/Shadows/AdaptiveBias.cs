using UnityEngine;
using LumenFX.Core;

namespace LumenFX.Shadows
{
    /// <summary>
    /// v2 shadow bias model. One forward ground probe is combined with the
    /// camera height and mapped through a linear response curve. The scale
    /// slider shifts the whole curve up or down.
    /// Optimized: caches camera references, throttles raycasts by movement/angle thresholds,
    /// and guarantees zero allocations per frame.
    /// </summary>
    public static class AdaptiveBias
    {
        private const float HeightBlend = 0.5f;   // camera height vs probe weight
        private const float HeightNorm = 500f;    // distance that saturates the height term
        private const float PitchFloor = 0.35f;   // probe influence when looking straight down
        private const float BiasFloor = 0.05f;
        private const float BiasCeiling = 2f;

        private const float PosThresholdSq = 0.25f; // recalculate if camera moves > 0.5m
        private const float AngleThresholdCos = 0.9996f; // ~0.5 degree cos
        private const int MaxTicksBetweenUpdates = 20;

        private static Camera _cachedCamera;
        private static CameraController _cachedCameraController;
        private static Vector3 _lastCameraPos;
        private static Vector3 _lastCameraForward;
        private static int _ticksSinceLastUpdate = 999;
        private static float _cachedBias = 0.15f;

        public static void ClearCache()
        {
            _cachedCamera = null;
            _cachedCameraController = null;
            _ticksSinceLastUpdate = 999;
        }

        internal static float Compute(LightState state)
        {
            if (_cachedCameraController == null)
            {
                _cachedCameraController = ToolsModifierControl.cameraController;
                if (_cachedCameraController == null)
                {
                    return 0.5f;
                }
            }

            if (_cachedCamera == null)
            {
                _cachedCamera = _cachedCameraController.GetComponent<Camera>();
                if (_cachedCamera == null)
                {
                    _cachedCamera = Camera.main;
                    if (_cachedCamera == null)
                    {
                        return 0.5f;
                    }
                }
            }

            Transform camTransform = _cachedCamera.transform;
            Vector3 camPos = camTransform.position;
            Vector3 camForward = camTransform.forward;

            _ticksSinceLastUpdate++;
            bool cameraMoved = (camPos - _lastCameraPos).sqrMagnitude > PosThresholdSq;
            bool cameraRotated = Vector3.Dot(camForward, _lastCameraForward) < AngleThresholdCos;

            if (!cameraMoved && !cameraRotated && _ticksSinceLastUpdate < MaxTicksBetweenUpdates)
            {
                return _cachedBias;
            }

            _lastCameraPos = camPos;
            _lastCameraForward = camForward;
            _ticksSinceLastUpdate = 0;

            float probe = GroundProbe.Distance(_cachedCamera);
            float cameraHeight = Mathf.Max(0f, _cachedCameraController.transform.position.y - _cachedCameraController.m_currentHeight);
            float height = HeightBlend * cameraHeight + (1f - HeightBlend) * probe;

            float heightTerm = Mathf.Clamp01(height / HeightNorm);
            float pitch = Mathf.Clamp01(_cachedCameraController.m_targetAngle.y / 90f);
            float response = heightTerm * Mathf.Lerp(1f, PitchFloor, pitch);

            float bias = Mathf.Lerp(BiasFloor, 1f, response) * state.BiasScale;

            if (state.ForceLowBias)
            {
                // Keep the bias tight so ground shadows stay crisp even when
                // the camera climbs.
                bias = Mathf.Min(bias, 0.45f) * 0.75f;
            }

            _cachedBias = Mathf.Clamp(bias, BiasFloor, BiasCeiling);
            return _cachedBias;
        }
    }
}

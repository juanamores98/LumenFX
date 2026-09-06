using UnityEngine;

namespace LumenFX.Shadows
{
    /// <summary>
    /// Single ground probe used by the v2 bias model. Derives from ToolBase
    /// only to reach the raycast API. Optimized to avoid heap allocations and Camera.main searches.
    /// </summary>
    public class GroundProbe : ToolBase
    {
        private const float MaxDistance = 1500f;

        private static ToolBase.RaycastInput _templateInput;
        private static bool _templateInitialized;

        private static void EnsureTemplate()
        {
            if (_templateInitialized)
            {
                return;
            }

            _templateInput = default(ToolBase.RaycastInput);
            _templateInput.m_ignoreBuildingFlags = Building.Flags.None;
            _templateInput.m_ignoreNodeFlags = NetNode.Flags.None;
            _templateInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
            _templateInput.m_ignorePropFlags = PropInstance.Flags.None;
            _templateInput.m_buildingService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            _templateInput.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            _templateInput.m_netService2 = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            _templateInput.m_propService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            _templateInitialized = true;
        }

        public static float Distance(Camera camera)
        {
            if (camera == null)
            {
                return MaxDistance;
            }

            EnsureTemplate();
            Transform camTransform = camera.transform;
            Vector3 camPos = camTransform.position;
            Vector3 camForward = camTransform.forward;

            ToolBase.RaycastInput input = _templateInput;
            input.m_ray = new Ray(camPos, camForward);
            input.m_length = camera.farClipPlane;

            ToolBase.RaycastOutput output;
            return ToolBase.RayCast(input, out output)
                ? Vector3.Distance(camPos, output.m_hitPos)
                : MaxDistance;
        }

        public static float Distance()
        {
            return Distance(Camera.main);
        }
    }
}


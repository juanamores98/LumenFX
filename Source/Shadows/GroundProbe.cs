using UnityEngine;

namespace LumenFX.Shadows
{
    /// <summary>
    /// Single ground probe used by the v2 bias model. Derives from ToolBase
    /// only to reach the raycast API.
    /// </summary>
    public class GroundProbe : ToolBase
    {
        private const float MaxDistance = 1500f;

        public static float Distance()
        {
            if (Camera.main == null)
            {
                return MaxDistance;
            }

            Transform camera = Camera.main.transform;
            var input = new ToolBase.RaycastInput(new Ray(camera.position, camera.forward), Camera.main.farClipPlane);
            input.m_ignoreBuildingFlags = Building.Flags.None;
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            input.m_ignorePropFlags = PropInstance.Flags.None;
            input.m_buildingService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_netService2 = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_propService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);

            ToolBase.RaycastOutput output;
            return ToolBase.RayCast(input, out output)
                ? Vector3.Distance(camera.position, output.m_hitPos)
                : MaxDistance;
        }
    }
}

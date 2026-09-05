using System.Reflection;
using ICities;
using UnityEngine;
using Harmony;
using LumenFX.Core;
using LumenFX.Runtime;

namespace LumenFX
{
    /// <summary>
    /// LumenFX v2 entry point: original lighting, tonemapping and shadow
    /// tuning for Cities: Skylines with its own adaptive bias model.
    /// </summary>
    public class LumenFXMod : LoadingExtensionBase, IUserMod
    {
        private const string HostObjectName = "LumenFX2";
        private const string HarmonyId = "com.juanamores98.lumenfx2.bias";

        private static bool _patched;

        private GameObject _host;
        private HarmonyInstance _harmony;

        public string Name
        {
            get { return "LumenFX v2"; }
        }

        public string Description
        {
            get { return "Sun, moon and ambience balancing, filmic tone controls and adaptive shadow bias with presets."; }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            TunerRuntime.ApplyAll();
        }

        public void OnEnabled()
        {
            DestroyHosts();
            _host = new GameObject(HostObjectName);
            _host.AddComponent<TunerEngine>();

            if (_patched)
            {
                return;
            }

            _harmony = HarmonyInstance.Create(HarmonyId);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            _patched = true;
        }

        public void OnDisabled()
        {
            DestroyHosts();
        }

        private static void DestroyHosts()
        {
            while (true)
            {
                GameObject leftover = GameObject.Find(HostObjectName);
                if (!leftover)
                {
                    break;
                }

                UnityEngine.Object.DestroyImmediate(leftover);
            }
        }
    }
}

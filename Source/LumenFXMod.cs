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

        /// <summary>
        /// Scene hosts created while the main menu is up die when the gameplay
        /// scene loads, so the host is (re)created here for every map.
        /// </summary>
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            CreateHost();
            TunerRuntime.ApplyAll();

            UI.UuiButton.Register(
                "LumenFX v2",
                "Lighting, tone and shadow tuner (Ctrl+Alt+L)",
                UI.TrayIcon.Make(),
                show => Core.TunerEngine.OpenWindow());
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            UI.UuiButton.Unregister();
            DestroyHosts();
        }

        public void OnEnabled()
        {
            // Covers the case of enabling the mod while a map is already
            // running; the gameplay scene replaces menu-time hosts anyway.
            CreateHost();

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

        private void CreateHost()
        {
            DestroyHosts();
            _host = new GameObject(HostObjectName);
            _host.AddComponent<Core.TunerEngine>();
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

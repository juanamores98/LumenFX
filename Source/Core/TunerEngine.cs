using UnityEngine;
using LumenFX.IO;
using LumenFX.Runtime;
using LumenFX.Shadows;
namespace LumenFX.Core
{
    public class TunerEngine : MonoBehaviour
    {
        internal static void OpenWindow() { FxModule.OpenStandalone(); }
        internal static void CloseWindow() { FxModule.CloseStandalone(); }
        public static void ToggleWindow() { FxModule.OpenStandalone(true); }
        private void Start() { StateStore.Load(); TunerRuntime.CurrentState.LightingDirty = true; TunerRuntime.ApplyAll(); }
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.L)) ToggleWindow();
            StateStore.CheckPendingSave();
        }
        private void OnDestroy()
        {
            StateStore.SaveImmediate(); FxModule.CloseStandalone();
            AdaptiveBias.ClearCache(); LightingMixer.ClearCache(); VanillaSnapshot.ResetCapture();
            TunerRuntime.CurrentState.LightingDirty = true;
        }
    }
}

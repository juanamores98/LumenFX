using UnityEngine;
using LumenFX.IO;
using LumenFX.Runtime;
using LumenFX.UI;

namespace LumenFX.Core
{
    /// <summary>
    /// Scene host for the v2 tuner: owns the hotkey (Ctrl + Alt + L) and the
    /// optional tuner window, and pushes changes into the runtime.
    /// </summary>
    public class TunerEngine : MonoBehaviour
    {
        private static bool _open;

        private TunerWindow _window;
        private int _windowId;

        internal static void CloseWindow()
        {
            _open = false;
        }

        private void Start()
        {
            _windowId = GetInstanceID();
            StateStore.Load();
            _window = new TunerWindow(TunerRuntime.CurrentState, OnTuned);
            TunerRuntime.ApplyAll();
        }

        private static void OnTuned()
        {
            TunerRuntime.ApplyAll();
            StateStore.Save();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.L))
            {
                _open = !_open;
            }
        }

        private void OnGUI()
        {
            if (_open)
            {
                _window.Draw(_windowId);
            }
        }
    }
}

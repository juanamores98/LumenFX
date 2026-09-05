using UnityEngine;
using System;
using LumenFX.IO;
using LumenFX.Presets;
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

        internal static void OpenWindow()
        {
            _open = true;
        }

        private void Start()
        {
            _windowId = GetInstanceID();
            EnsureBuiltInPresets();
            StateStore.Load();
            _window = new TunerWindow(TunerRuntime.CurrentState, OnTuned);
            TunerRuntime.ApplyAll();
        }

        /// <summary>
        /// Writes the presets shipped with the mod into the preset folder the
        /// first time (or if the user deleted them).
        /// </summary>
        private static void EnsureBuiltInPresets()
        {
            string[] builtins = { "Vanilla", "Optimized" };

            foreach (string name in builtins)
            {
                try
                {
                    string path = System.IO.Path.Combine(PresetLibrary.Folder, PresetLibrary.Sanitize(name) + ".lumenfx.xml");
                    if (System.IO.File.Exists(path))
                    {
                        continue;
                    }

                    PresetLibrary.EnsureFolder();
                    using (var stream = System.Reflection.Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("LumenFX.BuiltIns." + name + ".lumenfx.xml"))
                    {
                        if (stream == null)
                        {
                            continue;
                        }

                        using (var reader = new System.IO.StreamReader(stream))
                        {
                            System.IO.File.WriteAllText(path, reader.ReadToEnd());
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
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

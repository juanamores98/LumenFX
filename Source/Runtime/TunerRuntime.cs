using UnityEngine;
using LumenFX.Core;

namespace LumenFX.Runtime
{
    /// <summary>
    /// Process-wide access to the v2 tuning state and the combined apply
    /// routine used by the level loader, the patch and the tuner window.
    /// </summary>
    public static class TunerRuntime
    {
        private static LightState _state = new LightState();

        public static LightState CurrentState
        {
            get { return _state; }
        }

        public static void ApplyAll()
        {
            if (_state.VanillaMode)
            {
                VanillaSnapshot.Restore();
                return;
            }

            VanillaSnapshot.Capture();
            LightingMixer.Apply(_state);
            TonemapProfile.Apply(_state);
            QualitySettings.shadows = _state.SoftShadows ? ShadowQuality.All : ShadowQuality.HardOnly;
        }

        /// <summary>
        /// Returns every touched component to the exact state the game
        /// shipped with.
        /// </summary>
        public static void RestoreVanilla()
        {
            VanillaSnapshot.Restore();
        }
    }
}

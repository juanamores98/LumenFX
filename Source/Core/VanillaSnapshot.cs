namespace LumenFX.Core
{
    internal static class VanillaSnapshot
    {
        // Each property is captured lazily by PropertyLedger, before its first write.
        // No all-or-nothing component snapshot: late camera components remain supported.
        internal static void Capture() { }
        internal static void ResetCapture() { }
        internal static void Restore() { Infrastructure.PropertyLedger.ReleaseAll(); }
    }
}

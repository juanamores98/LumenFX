using System.IO;
using System.Reflection;

namespace LumenFX.Presets
{
    internal static class QuickPresets
    {
        internal static bool ApplyVanilla()
        {
            return LumenFXMod.ApplySuiteSection("<lumenfx><vanillaMode>true</vanillaMode></lumenfx>");
        }

        internal static bool ApplyOptimized()
        {
            using (var stream = typeof(QuickPresets).Assembly.GetManifestResourceStream("LumenFX.BuiltIns.Optimized.xml"))
            {
                if (stream == null) return false;
                using (var reader = new StreamReader(stream)) return LumenFXMod.ApplySuiteSection(reader.ReadToEnd());
            }
        }
    }
}

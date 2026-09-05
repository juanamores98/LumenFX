using UnityEngine;

namespace LumenFX.UI
{
    /// <summary>
    /// Procedurally drawn tray icon: a sun disc with rays.
    /// </summary>
    internal static class TrayIcon
    {
        internal static Texture2D Make()
        {
            const int size = 48;
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                name = "LumenFX.tray"
            };

            var center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x, y);
                    float distance = Vector2.Distance(p, center);
                    Color c = new Color(0f, 0f, 0f, 0f);

                    if (distance < 22f)
                    {
                        c = new Color32(24, 30, 44, 255); // midnight disc
                    }

                    if (distance < 22f)
                    {
                        // Ray spokes every 45 degrees.
                        Vector2 dir = (p - center).normalized;
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180f;
                        float spoke = Mathf.Repeat(angle, 45f);
                        float ray = Mathf.Clamp01(1f - Mathf.Abs(spoke - 22.5f) / 4f);
                        float ring = Mathf.Clamp01((distance - 8f) / 4f) * Mathf.Clamp01((20f - distance) / 3f);
                        float disc = 1f - Mathf.Clamp01((distance - 9f) / 2f);
                        float glow = Mathf.Max(disc, ray * ring);
                        c = Color.Lerp(c, new Color(1f, 0.82f, 0.35f, 1f), glow);
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply(false, true);
            return tex;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Blacksite.WeaponShop
{
    public static class WeaponIconFactory
    {
        private const int Width = 128;
        private const int Height = 64;
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string iconKey, Color color)
        {
            string key = $"{iconKey}:{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            PixelCanvas canvas = new PixelCanvas(Width, Height);
            Color body = color;
            Color dark = Color.Lerp(color, new Color(0.03f, 0.04f, 0.03f, 1f), 0.68f);
            Color highlight = Color.Lerp(color, Color.white, 0.34f);

            switch (iconKey)
            {
                case "smg":
                    DrawSmg(canvas, body, dark, highlight);
                    break;
                case "sniper":
                    DrawSniper(canvas, body, dark, highlight);
                    break;
                case "shotgun":
                    DrawShotgun(canvas, body, dark, highlight);
                    break;
                case "pistol":
                    DrawPistol(canvas, body, dark, highlight);
                    break;
                case "ammo":
                    DrawAmmo(canvas, body, dark, highlight);
                    break;
                case "armor-ammo":
                    DrawArmorAmmo(canvas, body, dark, highlight);
                    break;
                case "shell":
                    DrawShell(canvas, body, dark, highlight);
                    break;
                default:
                    DrawRifle(canvas, body, dark, highlight);
                    break;
            }

            Texture2D texture = canvas.ToTexture();
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, Width, Height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect
            );
            sprite.name = $"{iconKey}-icon";
            Cache[key] = sprite;
            return sprite;
        }

        private static void DrawRifle(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Line(13, 31, 92, 31, body, 5);
            c.Rect(55, 28, 78, 14, dark);
            c.Line(28, 31, 42, 18, body, 5);
            c.Line(17, 24, 37, 31, highlight, 3);
            c.Rect(93, 29, 26, 3, body);
            c.Rect(102, 25, 12, 3, dark);
            c.Rect(78, 34, 23, 8, body);
            c.Line(80, 39, 78, 49, body, 4);
            c.Circle(119, 30, 2, dark);
        }

        private static void DrawSmg(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Line(25, 31, 83, 31, body, 6);
            c.Rect(47, 28, 29, 12, dark);
            c.Line(35, 31, 47, 20, body, 4);
            c.Line(27, 24, 40, 31, highlight, 3);
            c.Rect(84, 29, 23, 4, body);
            c.Rect(70, 35, 20, 8, body);
            c.Line(74, 42, 70, 52, body, 4);
            c.Rect(89, 36, 8, 13, dark);
        }

        private static void DrawSniper(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Line(8, 31, 109, 31, body, 4);
            c.Rect(50, 26, 42, 4, highlight);
            c.Rect(65, 21, 20, 5, dark);
            c.Rect(68, 18, 14, 3, dark);
            c.Line(22, 31, 36, 22, body, 4);
            c.Line(15, 25, 34, 31, highlight, 3);
            c.Rect(110, 29, 12, 3, body);
            c.Rect(75, 34, 20, 7, body);
        }

        private static void DrawShotgun(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Line(12, 27, 94, 27, body, 4);
            c.Line(12, 33, 94, 33, body, 4);
            c.Rect(36, 26, 42, 3, dark);
            c.Line(21, 32, 33, 20, body, 4);
            c.Line(15, 24, 34, 31, highlight, 3);
            c.Rect(95, 29, 23, 5, body);
            c.Rect(64, 36, 23, 8, body);
            c.Line(68, 43, 65, 52, body, 4);
        }

        private static void DrawPistol(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Line(34, 30, 84, 30, body, 8);
            c.Line(57, 30, 66, 51, body, 8);
            c.Line(65, 50, 47, 50, body, 5);
            c.Rect(76, 27, 28, 5, body);
            c.Rect(48, 27, 27, 3, highlight);
            c.Rect(53, 34, 18, 4, dark);
            c.Circle(105, 29, 2, dark);
        }

        private static void DrawAmmo(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Rect(24, 19, 65, 31, body);
            c.Rect(28, 22, 57, 5, highlight);
            c.Rect(31, 35, 51, 10, dark);
            c.Line(91, 22, 91, 48, body, 4);
            c.Line(99, 20, 99, 48, body, 4);
            c.Line(107, 18, 107, 48, body, 4);
            c.Line(115, 16, 115, 48, body, 4);
            c.Circle(91, 20, 3, body);
            c.Circle(99, 18, 3, body);
            c.Circle(107, 16, 3, body);
            c.Circle(115, 14, 3, body);
        }

        private static void DrawArmorAmmo(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Rect(20, 20, 58, 28, body);
            c.Rect(24, 24, 50, 5, highlight);
            c.Rect(26, 35, 46, 9, dark);
            c.Rect(78, 17, 9, 29, body);
            c.Rect(90, 15, 9, 31, body);
            c.Rect(102, 13, 9, 33, body);
            c.Circle(82, 17, 4, body);
            c.Circle(94, 15, 4, body);
            c.Circle(106, 13, 4, body);
        }

        private static void DrawShell(PixelCanvas c, Color body, Color dark, Color highlight)
        {
            c.Rect(34, 27, 59, 15, body);
            c.Rect(92, 29, 15, 11, body);
            c.Rect(41, 31, 44, 4, highlight);
            c.Rect(20, 29, 14, 11, body);
            c.Line(44, 15, 44, 27, body, 8);
            c.Line(62, 15, 62, 27, body, 8);
            c.Line(80, 15, 80, 27, body, 8);
            c.Circle(44, 15, 4, highlight);
            c.Circle(62, 15, 4, highlight);
            c.Circle(80, 15, 4, highlight);
            c.Rect(41, 35, 44, 3, dark);
        }

        private sealed class PixelCanvas
        {
            private readonly int width;
            private readonly int height;
            private readonly Color[] pixels;

            public PixelCanvas(int width, int height)
            {
                this.width = width;
                this.height = height;
                pixels = new Color[width * height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.clear;
                }
            }

            public void Rect(int x, int y, int rectWidth, int rectHeight, Color color)
            {
                for (int py = y; py < y + rectHeight; py++)
                {
                    for (int px = x; px < x + rectWidth; px++)
                    {
                        Blend(px, py, color, 1f);
                    }
                }
            }

            public void Circle(int centerX, int centerY, int radius, Color color)
            {
                int radiusSquared = radius * radius;
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        if (x * x + y * y <= radiusSquared)
                        {
                            Blend(centerX + x, centerY + y, color, 1f);
                        }
                    }
                }
            }

            public void Line(int x0, int y0, int x1, int y1, Color color, int thickness)
            {
                int dx = Mathf.Abs(x1 - x0);
                int sx = x0 < x1 ? 1 : -1;
                int dy = -Mathf.Abs(y1 - y0);
                int sy = y0 < y1 ? 1 : -1;
                int error = dx + dy;
                int radius = Mathf.Max(0, thickness / 2);

                while (true)
                {
                    Circle(x0, y0, radius, color);
                    if (x0 == x1 && y0 == y1)
                    {
                        break;
                    }

                    int doubled = 2 * error;
                    if (doubled >= dy)
                    {
                        error += dy;
                        x0 += sx;
                    }

                    if (doubled <= dx)
                    {
                        error += dx;
                        y0 += sy;
                    }
                }
            }

            public Texture2D ToTexture()
            {
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    name = "Runtime-Weapon-Icon"
                };
                texture.SetPixels(pixels);
                texture.Apply(false, true);
                return texture;
            }

            private void Blend(int x, int y, Color color, float alpha)
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    return;
                }

                int index = y * width + x;
                Color destination = pixels[index];
                float sourceAlpha = color.a * alpha;
                float outputAlpha = sourceAlpha + destination.a * (1f - sourceAlpha);

                if (outputAlpha <= 0f)
                {
                    pixels[index] = Color.clear;
                    return;
                }

                Color output = (color * sourceAlpha + destination * destination.a * (1f - sourceAlpha)) / outputAlpha;
                output.a = outputAlpha;
                pixels[index] = output;
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.IO;

namespace TetrisEditor
{
    /// <summary>
    /// Sinh toàn bộ art của giao diện bằng code (bo góc, nền home) thay vì import ảnh rời,
    /// để chỉnh màu / bán kính chỉ cần sửa hằng số rồi chạy lại menu.
    /// </summary>
    public static class UIArtGenerator
    {
        public const string Folder = "Assets/Sprites/UI/Style/";

        public const string RoundedPath = Folder + "ui_round_rect.png";
        public const string RoundedSoftPath = Folder + "ui_round_rect_soft.png";
        public const string HomeBgPath = Folder + "bg_home.png";
        public const string ArrowLeftPath = Folder + "ui_arrow_left.png";

        private const int RoundedSize = 128;
        private const float RoundedRadius = 34f;
        private const float SoftRadius = 22f;
        private const int RoundedBorder = 38;   // > radius để 9-slice không cắt vào góc cong
        private const int SoftBorder = 26;

        private const int BgWidth = 540;
        private const int BgHeight = 1170;      // đúng tỉ lệ 1080x2340

        [MenuItem("Tools/Tetris/Generate UI Art")]
        public static void GenerateAll()
        {
            EnsureFolder();

            WritePng(RoundedPath, BuildRoundedRect(RoundedSize, RoundedRadius));
            ImportAsSprite(RoundedPath, RoundedBorder);

            WritePng(RoundedSoftPath, BuildRoundedRect(RoundedSize, SoftRadius));
            ImportAsSprite(RoundedSoftPath, SoftBorder);

            WritePng(HomeBgPath, BuildHomeBackground(BgWidth, BgHeight));
            ImportAsSprite(HomeBgPath, 0);

            WritePng(ArrowLeftPath, BuildChevronLeft(64, 9f));
            ImportAsSprite(ArrowLeftPath, 0);

            AssetDatabase.Refresh();
            Debug.Log("[UIArt] Đã sinh " + RoundedPath + ", " + RoundedSoftPath + ", "
                + HomeBgPath + ", " + ArrowLeftPath);
        }

        // ---------------------------------------------------------------- helpers

        private static void EnsureFolder()
        {
            string full = Path.GetFullPath(Folder);
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
        }

        private static void WritePng(string assetPath, Texture2D texture)
        {
            File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static void ImportAsSprite(string assetPath, int border)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("[UIArt] Không import được " + assetPath);
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
        }

        /// <summary>Hệ số che phủ 0..1 của một điểm so với hình chữ nhật bo góc (khử răng cưa ~1px).</summary>
        private static float RoundedCoverage(float px, float py, float halfW, float halfH, float radius)
        {
            float dx = Mathf.Abs(px) - (halfW - radius);
            float dy = Mathf.Abs(py) - (halfH - radius);
            float ox = Mathf.Max(dx, 0f);
            float oy = Mathf.Max(dy, 0f);

            // signed distance tới biên hình bo góc
            float outside = Mathf.Sqrt(ox * ox + oy * oy) - radius;
            float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);
            float distance = outside + inside;

            return Mathf.Clamp01(0.5f - distance);
        }

        private static Texture2D BuildRoundedRect(int size, float radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;
                    float a = RoundedCoverage(px, py, half, half, radius);

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>Mũi tên "quay lại" vẽ bằng 2 đoạn thẳng bo đầu, màu trắng để tint tuỳ ý.</summary>
        private static Texture2D BuildChevronLeft(int size, float thickness)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            Vector2 top = new Vector2(size * 0.68f, size * 0.80f);
            Vector2 mid = new Vector2(size * 0.32f, size * 0.50f);
            Vector2 bottom = new Vector2(size * 0.68f, size * 0.20f);

            float half = thickness * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);

                    float d = Mathf.Min(DistanceToSegment(p, top, mid), DistanceToSegment(p, mid, bottom));
                    float a = Mathf.Clamp01(half - d + 0.5f);

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 0.0001f) return Vector2.Distance(p, a);

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);

            return Vector2.Distance(p, a + ab * t);
        }

        // ---------------------------------------------------------------- nền home

        private static readonly Color BgBottom = new Color32(0x0C, 0x0E, 0x22, 0xFF);
        private static readonly Color BgMid = new Color32(0x2A, 0x1D, 0x4E, 0xFF);
        private static readonly Color BgTop = new Color32(0x1D, 0x24, 0x5A, 0xFF);

        private static readonly Color GlowCool = new Color32(0x5B, 0x8B, 0xFF, 0xFF);
        private static readonly Color GlowWarm = new Color32(0x8A, 0x45, 0xC8, 0xFF);

        /// <summary>Bốn khối tetromino dùng làm hoa văn chìm.</summary>
        private static readonly Vector2Int[][] Shapes =
        {
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(3,0) }, // I
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // O
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(1,1) }, // T
            new[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,0) }, // L
            new[] { new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // S
        };

        private static Texture2D BuildHomeBackground(int w, int h)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            float aspect = (float)h / w; // để vòng sáng tròn thật trên màn hình

            for (int y = 0; y < h; y++)
            {
                float v = (float)y / (h - 1);

                Color baseColor = v < 0.45f
                    ? Color.Lerp(BgBottom, BgMid, v / 0.45f)
                    : Color.Lerp(BgMid, BgTop, (v - 0.45f) / 0.55f);

                for (int x = 0; x < w; x++)
                {
                    float u = (float)x / (w - 1);
                    Color c = baseColor;

                    c = AddGlow(c, u, v, aspect, 0.50f, 0.74f, 0.62f, GlowCool, 0.22f);
                    c = AddGlow(c, u, v, aspect, 0.12f, 0.20f, 0.55f, GlowWarm, 0.14f);

                    // vignette nhẹ cho đỡ phẳng
                    float dx = u - 0.5f;
                    float dy = (v - 0.5f) * aspect;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float vignette = 1f - 0.40f * Mathf.SmoothStep(0.35f, 1.15f, d);
                    c *= vignette;

                    c.a = 1f;
                    pixels[y * w + x] = c;
                }
            }

            DrawPattern(pixels, w, h);

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        private static Color AddGlow(Color c, float u, float v, float aspect,
            float cx, float cy, float radius, Color glow, float strength)
        {
            float dx = u - cx;
            float dy = (v - cy) * aspect;
            float d = Mathf.Sqrt(dx * dx + dy * dy);

            float falloff = 1f - Mathf.Clamp01(d / radius);
            falloff *= falloff;

            return c + glow * (falloff * strength);
        }

        /// <summary>Rải vài khối tetromino mờ làm hoa văn nền.</summary>
        private static void DrawPattern(Color[] pixels, int w, int h)
        {
            // (chỉ số shape, x ô, y ô, kích thước ô, độ mờ)
            float[][] placements =
            {
                new[] { 0f,  0.60f, 0.90f, 34f, 0.045f },
                new[] { 1f,  0.10f, 0.78f, 30f, 0.035f },
                new[] { 2f,  0.72f, 0.55f, 26f, 0.030f },
                new[] { 3f,  0.08f, 0.44f, 30f, 0.032f },
                new[] { 4f,  0.55f, 0.24f, 34f, 0.040f },
                new[] { 1f,  0.82f, 0.13f, 26f, 0.030f },
            };

            for (int i = 0; i < placements.Length; i++)
            {
                var p = placements[i];
                Vector2Int[] shape = Shapes[(int)p[0]];
                int originX = Mathf.RoundToInt(p[1] * w);
                int originY = Mathf.RoundToInt(p[2] * h);
                int cell = Mathf.RoundToInt(p[3]);
                float alpha = p[4];

                for (int s = 0; s < shape.Length; s++)
                {
                    int cx = originX + shape[s].x * cell;
                    int cy = originY + shape[s].y * cell;
                    FillCell(pixels, w, h, cx, cy, cell - 4, alpha);
                }
            }
        }

        private static void FillCell(Color[] pixels, int w, int h, int x0, int y0, int size, float alpha)
        {
            for (int y = y0; y < y0 + size; y++)
            {
                if (y < 0 || y >= h) continue;

                for (int x = x0; x < x0 + size; x++)
                {
                    if (x < 0 || x >= w) continue;

                    int index = y * w + x;
                    Color c = pixels[index];
                    pixels[index] = new Color(c.r + alpha, c.g + alpha, c.b + alpha, 1f);
                }
            }
        }
    }
}

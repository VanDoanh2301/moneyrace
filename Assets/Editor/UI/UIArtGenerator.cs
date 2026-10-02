using UnityEngine;
using UnityEditor;
using System.IO;

namespace BallEditor
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
        public const string BallsBgPath = Folder + "bg_balls.png";
        public const string ArrowLeftPath = Folder + "ui_arrow_left.png";
        public const string GlossPath = Folder + "ui_gloss.png";
        public const string OutlinePath = Folder + "ui_outline.png";

        private const int RoundedSize = 128;
        private const float RoundedRadius = 34f;
        private const float SoftRadius = 22f;
        private const int RoundedBorder = 38;   // > radius để 9-slice không cắt vào góc cong
        private const int SoftBorder = 26;

        private const int BgWidth = 540;
        private const int BgHeight = 960;       // đúng tỉ lệ 1080x1920 của Ball

        private const float OutlineThickness = 3f;
        /// <summary>Highlight tắt hẳn ở 55% chiều cao tính từ đỉnh.</summary>
        private const float GlossSpan = 0.55f;

        [MenuItem("Tools/Ball/Generate UI Art")]
        public static void GenerateAll()
        {
            EnsureFolder();

            WritePng(RoundedPath, BuildRoundedRect(RoundedSize, RoundedRadius));
            ImportAsSprite(RoundedPath, RoundedBorder);

            WritePng(RoundedSoftPath, BuildRoundedRect(RoundedSize, SoftRadius));
            ImportAsSprite(RoundedSoftPath, SoftBorder);

            WritePng(GlossPath, BuildGloss(RoundedSize, RoundedRadius));
            ImportAsSprite(GlossPath, RoundedBorder);

            WritePng(OutlinePath, BuildOutline(RoundedSize, RoundedRadius, OutlineThickness));
            ImportAsSprite(OutlinePath, RoundedBorder);

            WritePng(BallsBgPath, BuildBallsBackground(BgWidth, BgHeight));
            ImportAsSprite(BallsBgPath, 0);

            WritePng(ArrowLeftPath, BuildChevronLeft(64, 9f));
            ImportAsSprite(ArrowLeftPath, 0);

            AssetDatabase.Refresh();
            Debug.Log("[UIArt] Đã sinh " + RoundedPath + ", " + RoundedSoftPath + ", "
                + GlossPath + ", " + OutlinePath + ", " + BallsBgPath + ", " + ArrowLeftPath);
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

        // ---------------------------------------------------------------- gloss / viền

        /// <summary>
        /// Highlight "bóng": trắng, alpha mạnh ở đỉnh rồi tắt dần, cắt theo đúng hình bo
        /// góc nên phủ lên nút không bị lòi ra ngoài 4 góc.
        /// </summary>
        private static Texture2D BuildGloss(int size, float radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                // v: 1 ở đỉnh, 0 ở đáy
                float v = (y + 0.5f) / size;
                float fromTop = 1f - v;

                float ramp = fromTop >= GlossSpan
                    ? 0f
                    : 1f - (fromTop / GlossSpan);

                // mũ 1.6 cho dải sáng tụ về sát mép trên, giống phản chiếu thật
                ramp = Mathf.Pow(Mathf.Clamp01(ramp), 1.6f);

                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;

                    float shape = RoundedCoverage(px, py, half, half, radius);
                    float a = Mathf.Clamp01(shape * ramp);

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>Vành sáng: hiệu của hình bo góc và hình bo góc thu nhỏ đi thickness px.</summary>
        private static Texture2D BuildOutline(int size, float radius, float thickness)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            float half = size * 0.5f;
            float innerHalf = half - thickness;
            float innerRadius = Mathf.Max(radius - thickness, 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;

                    float outer = RoundedCoverage(px, py, half, half, radius);
                    float inner = RoundedCoverage(px, py, innerHalf, innerHalf, innerRadius);
                    float a = Mathf.Clamp01(outer - inner);

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        // ---------------------------------------------------------------- nền shop

        /// <summary>
        /// Nền shop: TRUNG TÍNH (R=G=B) để ThemedGraphic tint theo bảng màu đang chạy.
        /// Cố tình không bake màu như bản cũ của Tetra Rush — ở đó nền navy/tím và 6 khối
        /// tetromino là bản sắc game khác, tint lại không ra được tông mong muốn.
        /// </summary>
        private static Texture2D BuildBallsBackground(int w, int h)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float v = (float)y / (h - 1);   // 0 đáy, 1 đỉnh

                // Đỉnh sáng hơn đáy để gradient dọc đọc được ngay cả khi tint màu tối.
                float lum = Mathf.Lerp(0.72f, 1f, v);

                for (int x = 0; x < w; x++)
                {
                    pixels[y * w + x] = new Color(lum, lum, lum, 1f);
                }
            }

            DrawBalls(pixels, w, h);

            // Vignette tối 4 góc, làm nội dung ở giữa nổi lên.
            float aspect = (float)w / h;
            for (int y = 0; y < h; y++)
            {
                float v = (float)y / (h - 1);

                for (int x = 0; x < w; x++)
                {
                    float u = (float)x / (w - 1);

                    float dx = (u - 0.5f) * aspect;
                    float dy = v - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    float vignette = 1f - 0.34f * Mathf.SmoothStep(0.30f, 1.05f, d);

                    int i = y * w + x;
                    Color c = pixels[i];
                    pixels[i] = new Color(c.r * vignette, c.g * vignette, c.b * vignette, 1f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>Các "ball" mờ rải nền — chủ đề của game, thay cho khối tetromino cũ.</summary>
        private static void DrawBalls(Color[] pixels, int w, int h)
        {
            // (x chuẩn hoá, y chuẩn hoá, bán kính theo chiều rộng, độ sáng cộng thêm)
            float[][] balls =
            {
                new[] { 0.18f, 0.90f, 0.150f,  0.055f },
                new[] { 0.78f, 0.82f, 0.095f, -0.045f },
                new[] { 0.52f, 0.70f, 0.060f,  0.040f },
                new[] { 0.10f, 0.58f, 0.110f, -0.035f },
                new[] { 0.86f, 0.52f, 0.145f,  0.050f },
                new[] { 0.34f, 0.40f, 0.080f, -0.040f },
                new[] { 0.72f, 0.28f, 0.120f,  0.045f },
                new[] { 0.14f, 0.18f, 0.090f, -0.035f },
                new[] { 0.58f, 0.08f, 0.135f,  0.040f },
            };

            for (int i = 0; i < balls.Length; i++)
            {
                float[] b = balls[i];
                FillCircle(pixels, w, h, b[0] * w, b[1] * h, b[2] * w, b[3]);
            }
        }

        /// <summary>Hình tròn khử răng cưa, cộng/trừ độ sáng (không đổi sắc, giữ trung tính).</summary>
        private static void FillCircle(Color[] pixels, int w, int h, float cx, float cy, float radius, float delta)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - radius - 1f));
            int x1 = Mathf.Min(w - 1, Mathf.CeilToInt(cx + radius + 1f));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - radius - 1f));
            int y1 = Mathf.Min(h - 1, Mathf.CeilToInt(cy + radius + 1f));

            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    // coverage khử răng cưa ~1px ở mép
                    float a = Mathf.Clamp01(radius - d + 0.5f);
                    if (a <= 0f) continue;

                    int index = y * w + x;
                    Color c = pixels[index];
                    float add = delta * a;

                    pixels[index] = new Color(
                        Mathf.Clamp01(c.r + add),
                        Mathf.Clamp01(c.g + add),
                        Mathf.Clamp01(c.b + add),
                        1f);
                }
            }
        }
    }
}

using System.IO;
using UnityEngine;
using UnityEditor;

namespace RingGameEditor
{
    /// <summary>
    /// Vẽ sẵn icon UI đơn giản bằng code (thay vì cần AI asset-generation, hiện không khả dụng
    /// trong phiên Editor này — Unity_AssetGeneration_GetModels trả về danh sách rỗng).
    /// </summary>
    public static class IconGenerator
    {
        private const string SettingsIconPath = "Assets/Sprites/icon_settings.png";

        /// <summary>Đảm bảo có sprite icon bánh răng (trắng, nền trong suốt). Tạo nếu chưa có.</summary>
        public static Sprite EnsureSettingsIcon()
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(SettingsIconPath);
            if (existing != null) return existing;

            Texture2D tex = DrawGear(128, 8, 0.72f, 0.45f, 0.30f);

            File.WriteAllBytes(SettingsIconPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(SettingsIconPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(SettingsIconPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SettingsIconPath);
        }

        /// <summary>
        /// Vẽ icon bánh răng (gear) trắng trên nền trong suốt bằng toán học đơn giản:
        /// bán kính biên ngoài dao động theo góc để tạo răng cưa, có lỗ tròn ở giữa.
        /// </summary>
        private static Texture2D DrawGear(int size, int toothCount, float bodyRadiusFrac,
            float toothRadiusFrac, float holeRadiusFrac)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size / 2f, size / 2f);

            float bodyRadius = size * 0.5f * bodyRadiusFrac;
            float toothRadius = size * 0.5f * toothRadiusFrac;
            float holeRadius = size * 0.5f * holeRadiusFrac;

            float toothArc = (Mathf.PI * 2f) / toothCount;
            float toothWidth = toothArc * 0.5f; // răng chiếm nửa mỗi cung, nửa còn lại là rãnh

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center.x;
                    float dy = y + 0.5f - center.y;
                    float radius = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    if (angle < 0f) angle += Mathf.PI * 2f;

                    float angleInTooth = angle % toothArc;
                    bool inToothSector = angleInTooth < toothWidth;
                    float outerRadius = inToothSector ? toothRadius : bodyRadius;

                    bool filled = radius <= outerRadius && radius >= holeRadius;

                    pixels[y * size + x] = filled ? Color.white : new Color(1f, 1f, 1f, 0f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }
    }
}

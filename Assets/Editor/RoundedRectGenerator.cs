using System.IO;
using UnityEngine;
using UnityEditor;

namespace RingGameEditor
{
    /// <summary>
    /// Vẽ sẵn 1 sprite hình chữ nhật bo góc trắng, dùng 9-slice (Image.Type.Sliced) rồi tint màu qua
    /// Image.color — thay cho việc kéo giãn ảnh có sẵn (bg_button_iap/rounded, Image.Type.Simple) vốn
    /// làm góc bo bị bóp méo/gần như mất hẳn khi resize không đều tỉ lệ (vd hàng Shop rất dẹt).
    /// Không dùng AI asset-generation vì hiện không khả dụng trong phiên Editor này (GetModels rỗng).
    /// </summary>
    public static class RoundedRectGenerator
    {
        private const string SpritePath = "Assets/Sprites/rounded_rect.png";
        private const int Size = 40;
        private const int Radius = 14;

        /// <summary>Trả về sprite trắng bo góc dùng chung, tạo mới nếu chưa có (idempotent).</summary>
        public static Sprite Ensure()
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (existing != null) return existing;

            Texture2D tex = Draw(Size, Radius);

            File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.spriteBorder = new Vector4(Radius, Radius, Radius, Radius);
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static Texture2D Draw(int size, int radius)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = InsideRoundedRect(x + 0.5f, y + 0.5f, size, size, radius);
                    pixels[y * size + x] = inside ? Color.white : new Color(1f, 1f, 1f, 0f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Trong 4 ô góc (cách cả 2 cạnh gần &lt; r) thì kiểm tra khoảng cách tới tâm bo; còn lại (giữa
        /// theo trục X hoặc Y) luôn là "trong" — đúng hình chữ nhật bo góc chuẩn.
        /// </summary>
        private static bool InsideRoundedRect(float x, float y, float w, float h, float r)
        {
            bool cornerZoneX = x < r || x > w - r;
            bool cornerZoneY = y < r || y > h - r;

            if (!cornerZoneX || !cornerZoneY) return true;

            float cx = x < r ? r : w - r;
            float cy = y < r ? r : h - r;
            float dx = x - cx;
            float dy = y - cy;

            return (dx * dx + dy * dy) <= r * r;
        }
    }
}

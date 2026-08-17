using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace RingGameEditor
{
    /// <summary>
    /// Hàm dựng UI dùng chung cho <see cref="ShopScreenBuilder"/>.
    /// </summary>
    public static class UIBuildUtil
    {
        public const string SpriteFolder = "Assets/Sprites/";

        public const string CanvasName = "Canvas";

        /// <summary>Font mặc định của game (dùng lại font UI có sẵn thay vì TextMeshPro).</summary>
        public const string DefaultFontPath = "Assets/Images/junegull rg.ttf";

        public static readonly Color GoldText = new Color(1f, 0.85f, 0.32f, 1f);

        // ----- Sprite -----

        public static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");
        }

        /// <summary>
        /// Kiểm tra đủ sprite trước khi dựng, tránh dựng nửa vời.
        /// Nếu file .png có mà đang import sai kiểu (Texture thay vì Sprite) thì tự sửa và reimport —
        /// ảnh copy từ project khác hay dính lỗi này.
        /// </summary>
        public static bool EnsureSprites(string dialogTitle, params string[] names)
        {
            bool ok = true;

            for (int i = 0; i < names.Length; i++)
            {
                if (LoadSprite(names[i]) != null) continue;

                if (TryFixTextureType(names[i]) && LoadSprite(names[i]) != null) continue;

                Debug.LogError("[UIBuild] Thiếu sprite: " + SpriteFolder + names[i] + ".png");
                ok = false;
            }

            if (!ok)
            {
                EditorUtility.DisplayDialog(dialogTitle, "Thiếu sprite trong Assets/Sprites. Xem Console.", "OK");
            }

            return ok;
        }

        /// <summary>Đổi import settings của một .png sang Sprite (2D and UI). Trả về false nếu không có file.</summary>
        private static bool TryFixTextureType(string name)
        {
            string path = SpriteFolder + name + ".png";

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            Debug.Log("[UIBuild] Đã tự chuyển " + path + " sang Texture Type = Sprite (2D and UI).");

            return true;
        }

        // ----- Font -----

        public static Font LoadDefaultFont()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(DefaultFontPath);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        // ----- Scene -----

        public static GameObject FindRootObject(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
            }

            return null;
        }

        public static void DestroyIfExists(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found != null)
            {
                Object.DestroyImmediate(found.gameObject);
            }
        }

        /// <summary>Đọc reference resolution thật từ CanvasScaler của canvas (thay vì hardcode 1080x1920).</summary>
        public static Vector2 GetReferenceResolution(GameObject canvasGo)
        {
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                return scaler.referenceResolution;
            }

            return new Vector2(1080f, 1920f);
        }

        // ----- RectTransform -----

        public static RectTransform NewUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);

            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Đặt rect ở một điểm neo cố định. anchor = điểm neo, pivot quyết định gốc của anchoredPosition.</summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        /// <summary>Neo theo mép trên, canh giữa ngang — kiểu dùng nhiều nhất trong các panel dọc.</summary>
        public static RectTransform TopCenter(string name, Transform parent, float y, float width, float height)
        {
            RectTransform rect = NewUI(name, parent);
            Place(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(width, height));

            return rect;
        }

        // ----- Components -----

        public static Image AddImage(RectTransform rect, string spriteName, bool raycastTarget, bool preserveAspect)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = LoadSprite(spriteName);
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = raycastTarget;

            return image;
        }

        public static Text AddText(RectTransform rect, Font font, string text, int size,
            TextAnchor alignment, Color color)
        {
            Text label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            return label;
        }

        public static Button AddButton(RectTransform rect, Graphic target)
        {
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            button.transition = Selectable.Transition.ColorTint;

            return button;
        }

        /// <summary>Nút icon: Image + Button, tiện cho các nút tròn/vuông nhỏ.</summary>
        public static Button IconButton(RectTransform rect, string spriteName)
        {
            Image image = AddImage(rect, spriteName, true, true);

            return AddButton(rect, image);
        }

        // ----- Serialized fields -----

        public static void SetObjectField(Object owner, string fieldName, Object value)
        {
            SerializedProperty property;
            SerializedObject so = Find(owner, fieldName, out property);
            if (so == null) return;

            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedObject Find(Object owner, string fieldName, out SerializedProperty property)
        {
            SerializedObject so = new SerializedObject(owner);
            property = so.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning("[UIBuild] Không tìm thấy field '" + fieldName + "' trên " + owner.GetType().Name);
                return null;
            }

            return so;
        }
    }
}

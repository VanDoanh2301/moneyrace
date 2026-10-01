using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace CubeJumpEditor
{
    /// <summary>
    /// Hàm dựng UI dùng chung cho <see cref="ShopMenuBuilder"/>.
    /// Port từ UIBuildUtil của game Terra, chỉnh cho scene và canvas của CubeJump.
    ///
    /// Có namespace (khác với code runtime) vì "UIBuildUtil" là tên rất dễ trùng
    /// trong assembly Assembly-CSharp-Editor dùng chung.
    /// </summary>
    public static class UIBuildUtil
    {
        public const string SpriteFolder = "Assets/_Game/Sprites/IAP/";
        public const string IconFolder = "Assets/_Game/Sprites/Icon/";

        /// <summary>Menu Canvas của Game.unity: ScaleWithScreenSize 1080x1920, match height.</summary>
        public const float RefWidth = 1080f;
        public const float RefHeight = 1920f;

        /// <summary>Nền 9-slice dùng chung với panel Settings.</summary>
        public const string PanelSpritePath = "Assets/_Game/Sprites/bg-button.png";
        public const string PanelSpriteName = "bg-button_0";

        public const string FontPath = "Assets/_Game/Font/04B_19_ SDF.asset";

        public static readonly Color GoldText = new Color(1f, 0.85f, 0.32f, 1f);

        // ----- Sprite -----

        public static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");
        }

        public static Sprite LoadIcon(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(IconFolder + name + ".png");
        }

        /// <summary>
        /// Lấy một sub-sprite trong sprite sheet (spriteImportMode = Multiple).
        /// LoadAssetAtPath&lt;Sprite&gt; trả về null với sheet nhiều sprite, nên phải quét hết asset.
        /// </summary>
        public static Sprite LoadSubSprite(string assetPath, string spriteName)
        {
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            for (int i = 0; i < all.Length; i++)
            {
                Sprite sprite = all[i] as Sprite;

                if (sprite != null && sprite.name == spriteName) return sprite;
            }

            // Sheet một sprite thì vẫn load được theo cách thường.
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        public static Sprite LoadPanelSprite()
        {
            return LoadSubSprite(PanelSpritePath, PanelSpriteName);
        }

        public static TMP_FontAsset LoadFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            return font != null ? font : TMP_Settings.defaultFontAsset;
        }

        /// <summary>
        /// Kiểm tra đủ sprite trước khi dựng, tránh dựng nửa vời.
        /// File .png có mà import sai kiểu (Texture thay vì Sprite) thì tự sửa và reimport.
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
                EditorUtility.DisplayDialog(dialogTitle, "Thiếu sprite trong " + SpriteFolder + ". Xem Console.", "OK");

            return ok;
        }

        private static bool TryFixTextureType(string name)
        {
            string path = SpriteFolder + name + ".png";

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            Debug.Log("[UIBuild] Đã tự chuyển " + path + " sang Texture Type = Sprite (2D and UI).");

            return true;
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

        /// <summary>
        /// Tìm theo đường dẫn từ root của scene, vd "UI/Menu Canvas".
        /// Cần vì Menu Canvas của CubeJump là con của root "UI", không phải root.
        /// </summary>
        public static Transform FindDeep(UnityEngine.SceneManagement.Scene scene, string path)
        {
            string[] parts = path.Split('/');

            GameObject root = FindRootObject(scene, parts[0]);
            if (root == null) return null;

            Transform current = root.transform;

            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);

                if (current == null) return null;
            }

            return current;
        }

        public static void DestroyIfExists(Transform parent, string name)
        {
            Transform found = parent.Find(name);

            if (found != null)
                Object.DestroyImmediate(found.gameObject);
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

        /// <summary>Đặt rect ở một điểm neo cố định.</summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        /// <summary>Neo theo mép trên, canh giữa ngang — kiểu dùng nhiều nhất trong panel dọc.</summary>
        public static RectTransform TopCenter(string name, Transform parent, float y, float width, float height)
        {
            RectTransform rect = NewUI(name, parent);
            Place(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(width, height));

            return rect;
        }

        public static RectTransform Centered(string name, Transform parent, Vector2 position, Vector2 size)
        {
            RectTransform rect = NewUI(name, parent);
            Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

            return rect;
        }

        // ----- Components -----

        public static Image AddSolidImage(RectTransform rect, Color color, bool raycastTarget)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = null;
            image.color = color;
            image.raycastTarget = raycastTarget;

            return image;
        }

        /// <summary>Nền 9-slice, co giãn không méo góc.</summary>
        public static Image AddPanelImage(RectTransform rect, Color color, bool raycastTarget)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = LoadPanelSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = raycastTarget;

            return image;
        }

        public static Image AddImage(RectTransform rect, Sprite sprite, bool raycastTarget, bool preserveAspect)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = raycastTarget;

            return image;
        }

        public static TextMeshProUGUI AddText(RectTransform rect, TMP_FontAsset font, string text, float size,
            TextAlignmentOptions alignment, Color color)
        {
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.color = color;

            return label;
        }

        public static Button AddButton(RectTransform rect, Graphic target)
        {
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            button.transition = Selectable.Transition.ColorTint;

            return button;
        }

        public static Button IconButton(RectTransform rect, Sprite sprite)
        {
            Image image = AddImage(rect, sprite, true, true);

            return AddButton(rect, image);
        }

        // ----- Serialized fields -----

        public static void SetObjectField(Object owner, string fieldName, Object value)
        {
            SerializedObject so = Find(owner, fieldName, out SerializedProperty property);
            if (so == null) return;

            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetIntField(Object owner, string fieldName, int value)
        {
            SerializedObject so = Find(owner, fieldName, out SerializedProperty property);
            if (so == null) return;

            property.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetBoolField(Object owner, string fieldName, bool value)
        {
            SerializedObject so = Find(owner, fieldName, out SerializedProperty property);
            if (so == null) return;

            property.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetObjectArrayField(Object owner, string fieldName, params Object[] values)
        {
            SerializedObject so = Find(owner, fieldName, out SerializedProperty property);
            if (so == null) return;

            property.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Thêm vào cuối một mảng/List serialize, bỏ qua nếu đã có.
        /// Đây là cách đăng ký menu mới vào MenuManager._menus.
        /// </summary>
        public static void AppendToObjectArrayField(Object owner, string fieldName, Object value)
        {
            SerializedObject so = Find(owner, fieldName, out SerializedProperty property);
            if (so == null) return;

            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            }

            int index = property.arraySize;
            property.arraySize = index + 1;
            property.GetArrayElementAtIndex(index).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Dọn các phần tử null/trùng trong một mảng serialize.</summary>
        public static void CompactObjectArrayField(Object owner, string fieldName)
        {
            SerializedObject so = Find(owner, fieldName, out SerializedProperty property);
            if (so == null) return;

            var kept = new System.Collections.Generic.List<Object>();

            for (int i = 0; i < property.arraySize; i++)
            {
                Object value = property.GetArrayElementAtIndex(i).objectReferenceValue;

                if (value != null && !kept.Contains(value))
                    kept.Add(value);
            }

            property.arraySize = kept.Count;

            for (int i = 0; i < kept.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = kept[i];

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedObject Find(Object owner, string fieldName, out SerializedProperty property)
        {
            SerializedObject so = new SerializedObject(owner);
            property = so.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning($"[UIBuild] Không tìm thấy field '{fieldName}' trên {owner.GetType().Name}");
                return null;
            }

            return so;
        }
    }
}

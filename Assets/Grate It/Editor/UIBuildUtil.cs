using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Hàm dựng UI dùng chung cho <see cref="ShopScreenBuilder"/>.
/// </summary>
public static class UIBuildUtil
{
    public const string SpriteFolder = "Assets/Grate It/UI/IAP/";

    /// <summary>Nền Shop Screen — image 96 (sunburst gradient).</summary>
    public const string ShopBackgroundPath = "Assets/Grate It/Textures/image 96.png";

    /// <summary>Sprite bo góc dùng cho hàng item (radius 12, viền trắng).</summary>
    public const string CapsuleSpritePath = "Assets/Grate It/UI/IAP/ShopItemBg.png";

    /// <summary>Sprite bo góc mạnh (capsule) — nút Shop Main Menu.</summary>
    public const string ShopButtonCapsulePath = "Assets/Grate It/UI/TITLE PAGE/ShopButtonBg.png";

    public const string CanvasName = "Canvas";

    /// <summary>Canvas tham chiếu Main Menu: ScaleWithScreenSize 1080x1920.</summary>
    public const float RefWidth = 1080f;
    public const float RefHeight = 1920f;

    public static readonly Color GoldText = new Color(1f, 0.85f, 0.32f, 1f);

    private static Font _defaultFont;

    public static Font DefaultFont
    {
        get
        {
            if (_defaultFont == null)
                _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return _defaultFont;
        }
    }

    public static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");
    }

    public static Sprite LoadSpriteAt(string assetPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null) return sprite;

        // Texture chưa import đúng kiểu Sprite → sửa rồi load lại
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return null;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    public static Image AddImageFromPath(RectTransform rect, string assetPath, bool raycastTarget, bool preserveAspect)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = LoadSpriteAt(assetPath);
        image.type = Image.Type.Simple;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = raycastTarget;
        image.color = Color.white;

        return image;
    }

    /// <summary>
    /// Hàng item kiểu ShopButton: Border trắng full + Fill màu inset (capsule).
    /// </summary>
    public static void StyleCapsuleRow(RectTransform row, Color fillColor, float borderInset = 8f)
    {
        Sprite capsule = LoadSpriteAt(CapsuleSpritePath);
        if (capsule == null)
        {
            Debug.LogWarning("[UIBuild] Thiếu capsule sprite: " + CapsuleSpritePath);
            return;
        }

        bool sliced = capsule.border != Vector4.zero;
        Image.Type imgType = sliced ? Image.Type.Sliced : Image.Type.Simple;

        // Root image gần như trong suốt (chỉ raycast nếu cần)
        Image rootImg = row.GetComponent<Image>();
        if (rootImg == null) rootImg = row.gameObject.AddComponent<Image>();
        rootImg.sprite = capsule;
        rootImg.type = imgType;
        rootImg.useSpriteMesh = !sliced;
        rootImg.color = new Color(1f, 1f, 1f, 0.01f);
        rootImg.raycastTarget = true;

        Transform borderT = row.Find("Border");
        GameObject borderGO = borderT != null ? borderT.gameObject : null;
        if (borderGO == null)
        {
            borderGO = new GameObject("Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGO.layer = row.gameObject.layer;
            borderGO.transform.SetParent(row, false);
            borderGO.transform.SetAsFirstSibling();
        }

        RectTransform borderRt = borderGO.GetComponent<RectTransform>();
        Stretch(borderRt);
        Image borderImg = borderGO.GetComponent<Image>();
        borderImg.sprite = capsule;
        borderImg.type = imgType;
        borderImg.useSpriteMesh = !sliced;
        borderImg.color = Color.white;
        borderImg.raycastTarget = false;

        Transform fillT = row.Find("Fill");
        GameObject fillGO = fillT != null ? fillT.gameObject : null;
        if (fillGO == null)
        {
            fillGO = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGO.layer = row.gameObject.layer;
            fillGO.transform.SetParent(row, false);
            fillGO.transform.SetSiblingIndex(1);
        }

        RectTransform fillRt = fillGO.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.pivot = new Vector2(0.5f, 0.5f);
        fillRt.offsetMin = new Vector2(borderInset, borderInset);
        fillRt.offsetMax = new Vector2(-borderInset, -borderInset);
        Image fillImg = fillGO.GetComponent<Image>();
        fillImg.sprite = capsule;
        fillImg.type = imgType;
        fillImg.useSpriteMesh = !sliced;
        fillImg.color = fillColor;
        fillImg.raycastTarget = false;
    }

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
            EditorUtility.DisplayDialog(dialogTitle, "Thiếu sprite trong " + SpriteFolder + ". Xem Console.", "OK");
        }

        return ok;
    }

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

    public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    public static RectTransform TopCenter(string name, Transform parent, float y, float width, float height)
    {
        RectTransform rect = NewUI(name, parent);
        Place(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(width, height));

        return rect;
    }

    public static Image AddSolidImage(RectTransform rect, Color color, bool raycastTarget)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = null;
        image.color = color;
        image.raycastTarget = raycastTarget;

        return image;
    }

    public static Image AddImage(RectTransform rect, string spriteName, bool raycastTarget, bool preserveAspect)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = LoadSprite(spriteName);
        image.type = Image.Type.Simple;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = raycastTarget;

        return image;
    }

    public static Text AddText(RectTransform rect, Font font, string text, float size,
        TextAnchor alignment, Color color)
    {
        Text label = rect.gameObject.AddComponent<Text>();
        label.font = font != null ? font : DefaultFont;
        label.text = text;
        label.fontSize = Mathf.RoundToInt(size);
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

    public static Button IconButton(RectTransform rect, string spriteName)
    {
        Image image = AddImage(rect, spriteName, true, true);

        return AddButton(rect, image);
    }

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

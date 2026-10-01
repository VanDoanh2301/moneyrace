using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using TMPro;

namespace CubeJumpEditor
{
    /// <summary>
    /// Dựng toàn bộ UI cửa hàng IAP vào scene. Port từ ShopPanelBuilder của game Terra,
    /// nhưng dựng theo khuôn <see cref="Menu"/> của CubeJump thay vì một panel SetActive rời.
    ///
    /// Chạy lại được nhiều lần: mỗi lần sẽ xoá "Shop Menu" cũ rồi dựng lại từ đầu.
    /// Danh mục gói đọc từ <see cref="IAPCatalog"/> nên không bị lệch với IAPManager.
    /// </summary>
    public static class ShopMenuBuilder
    {
        private const string MenuCanvasPath = "UI/Menu Canvas";
        private const string ShopMenuName = "Shop Menu";
        private const string SettingsMenuName = "Settings Menu";

        // Layout 1080x1920 (dọc)
        private const float PanelWidth = 900f;
        private const float PanelHeight = 1540f;
        private const float RowWidth = 820f;
        private const float RowHeight = 150f;
        private const float RowGap = 14f;
        private const float FirstRowY = 260f;

        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color PanelTint = new Color(0.14f, 0.16f, 0.24f, 1f);
        private static readonly Color RowTint = new Color(0.22f, 0.25f, 0.36f, 1f);
        private static readonly Color White = Color.white;

        [MenuItem("SansDev/IAP/Build Shop Menu", priority = 20)]
        public static void BuildShopMenu()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            Transform canvas = UIBuildUtil.FindDeep(scene, MenuCanvasPath);

            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Build Shop Menu",
                    $"Không tìm thấy '{MenuCanvasPath}' trong scene đang mở.\nMở Game.unity rồi chạy lại.", "OK");
                return;
            }

            if (!UIBuildUtil.EnsureSprites("Build Shop Menu",
                    "coin", "iv_buy", "iv_back", "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5"))
                return;

            MenuManager menuManager = Object.FindAnyObjectByType<MenuManager>(FindObjectsInactive.Include);

            if (menuManager == null)
            {
                EditorUtility.DisplayDialog("Build Shop Menu", "Không tìm thấy MenuManager trong scene.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Build Shop Menu");
            int group = Undo.GetCurrentGroup();

            UIBuildUtil.DestroyIfExists(canvas, ShopMenuName);

            ShopMenu shop = CreateShopMenuRoot(canvas, scene);
            BuildShopContents(shop);

            // Không có bước này MenuManager sẽ báo "trying to open a Menu Shop that has not been registered".
            UIBuildUtil.CompactObjectArrayField(menuManager, "_menus");
            UIBuildUtil.AppendToObjectArrayField(menuManager, "_menus", shop);

            Undo.CollapseUndoOperations(group);

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"[ShopMenuBuilder] Đã dựng '{ShopMenuName}' với {IAPCatalog.Packs.Length} gói và " +
                      "đăng ký vào MenuManager._menus.", shop);
        }

        /// <summary>
        /// Tạo GameObject menu với Canvas + MenuAnimation + ShopMenu, lấy Settings Menu
        /// làm khuôn để Canvas và animation giống hệt các menu đã có.
        /// </summary>
        private static ShopMenu CreateShopMenuRoot(Transform canvasRoot, UnityEngine.SceneManagement.Scene scene)
        {
            RectTransform root = UIBuildUtil.NewUI(ShopMenuName, canvasRoot);
            UIBuildUtil.Stretch(root);

            Transform template = canvasRoot.Find(SettingsMenuName);

            // Canvas lồng: cho phép bật/tắt riêng, đúng cách 7 menu kia đang làm.
            Canvas canvas = root.gameObject.AddComponent<Canvas>();
            Canvas templateCanvas = template != null ? template.GetComponent<Canvas>() : null;

            if (templateCanvas != null)
            {
                canvas.overrideSorting = templateCanvas.overrideSorting;
                canvas.sortingOrder = templateCanvas.sortingOrder;
            }
            else
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 10;
            }

            root.gameObject.AddComponent<GraphicRaycaster>();

            // MenuAnimation phải có TRƯỚC ShopMenu: Menu có [RequireComponent(typeof(MenuAnimation))].
            MenuAnimation anim = root.gameObject.AddComponent<MenuAnimation>();

            ShopMenu shop = root.gameObject.AddComponent<ShopMenu>();

            UIBuildUtil.SetIntField(shop, "_type", (int)MenuType.Shop);
            UIBuildUtil.SetBoolField(shop, "_useAnimation", true);

            // TweenUI trên panel con sẽ được gắn ở BuildShopContents rồi nhồi vào MenuAnimation.
            UIBuildUtil.SetObjectArrayField(anim, "_objectToAnimate");

            return shop;
        }

        private static void BuildShopContents(ShopMenu shop)
        {
            Transform root = shop.transform;
            TMP_FontAsset font = UIBuildUtil.LoadFont();

            // Nền mờ chặn click xuống menu dưới.
            RectTransform dim = UIBuildUtil.NewUI("Dim", root);
            UIBuildUtil.Stretch(dim);
            UIBuildUtil.AddSolidImage(dim, Dim, true);

            // Panel
            RectTransform panel = UIBuildUtil.Centered("panel", root, Vector2.zero, new Vector2(PanelWidth, PanelHeight));
            UIBuildUtil.AddPanelImage(panel, PanelTint, true);

            AttachPanelTween(shop, panel);

            // Tiêu đề
            RectTransform title = UIBuildUtil.TopCenter("title", panel, 40f, 500f, 90f);
            UIBuildUtil.AddText(title, font, "SHOP", 64f, TextAlignmentOptions.Center, White);

            // Nút back góc trên phải
            RectTransform back = UIBuildUtil.NewUI("close button", panel);
            UIBuildUtil.Place(back, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(90f, 90f));
            Button backButton = UIBuildUtil.IconButton(back, UIBuildUtil.LoadSprite("iv_back"));

            // Viên hiển thị coin
            RectTransform coinPill = UIBuildUtil.TopCenter("coin pill", panel, 140f, 420f, 80f);
            UIBuildUtil.AddPanelImage(coinPill, RowTint, false);

            RectTransform coinIcon = UIBuildUtil.NewUI("coin icon", coinPill);
            UIBuildUtil.Place(coinIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(56f, 56f));
            UIBuildUtil.AddImage(coinIcon, UIBuildUtil.LoadSprite("coin"), false, true);

            RectTransform coinLabelRect = UIBuildUtil.NewUI("coin text", coinPill);
            UIBuildUtil.Place(coinLabelRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(90f, 0f), new Vector2(310f, 70f));
            TextMeshProUGUI coinLabel = UIBuildUtil.AddText(coinLabelRect, font, "{0}", 44f, TextAlignmentOptions.Left,
                UIBuildUtil.GoldText);

            // 7 dòng gói
            RectTransform rowsRoot = UIBuildUtil.NewUI("rows", panel);
            UIBuildUtil.Stretch(rowsRoot);

            for (int i = 0; i < IAPCatalog.Packs.Length; i++)
                BuildRow(rowsRoot, font, IAPCatalog.Packs[i], i);

            // Dòng thông báo lỗi mua
            RectTransform message = UIBuildUtil.NewUI("message text", panel);
            UIBuildUtil.Place(message, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f),
                new Vector2(RowWidth, 60f));
            TextMeshProUGUI messageLabel = UIBuildUtil.AddText(message, font, string.Empty, 32f,
                TextAlignmentOptions.Center, new Color(1f, 0.45f, 0.45f, 1f));

            UIBuildUtil.SetObjectField(shop, "_backButton", backButton);
            UIBuildUtil.SetObjectField(shop, "_coinText", coinLabel);
            UIBuildUtil.SetObjectField(shop, "_messageText", messageLabel);
            UIBuildUtil.SetObjectField(shop, "_restoreButton", null);
        }

        private static void BuildRow(Transform parent, TMP_FontAsset font, IAPCatalog.Pack pack, int index)
        {
            float y = FirstRowY + index * (RowHeight + RowGap);

            RectTransform row = UIBuildUtil.TopCenter($"row {pack.ProductId}", parent, y, RowWidth, RowHeight);
            UIBuildUtil.AddPanelImage(row, RowTint, false);

            // Icon túi vàng
            RectTransform gold = UIBuildUtil.NewUI("gold icon", row);
            UIBuildUtil.Place(gold, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f),
                new Vector2(110f, 110f));
            UIBuildUtil.AddImage(gold, UIBuildUtil.LoadSprite(GoldSpriteFor(index)), false, true);

            // Tên gói
            RectTransform name = UIBuildUtil.NewUI("name text", row);
            UIBuildUtil.Place(name, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(152f, 26f),
                new Vector2(330f, 50f));
            UIBuildUtil.AddText(name, font, pack.DisplayName, 34f, TextAlignmentOptions.Left, White);

            // Số coin nhận được: icon coin rồi tới số
            RectTransform amountIcon = UIBuildUtil.NewUI("coin icon", row);
            UIBuildUtil.Place(amountIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(152f, -26f),
                new Vector2(44f, 44f));
            UIBuildUtil.AddImage(amountIcon, UIBuildUtil.LoadSprite("coin"), false, true);

            RectTransform amount = UIBuildUtil.NewUI("amount text", row);
            UIBuildUtil.Place(amount, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(206f, -26f),
                new Vector2(280f, 50f));
            UIBuildUtil.AddText(amount, font, $"{pack.Coins:n0}", 40f, TextAlignmentOptions.Left, UIBuildUtil.GoldText);

            // Nút mua + chữ giá
            RectTransform buy = UIBuildUtil.NewUI("buy button", row);
            UIBuildUtil.Place(buy, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f),
                new Vector2(210f, 100f));
            Image buyImage = UIBuildUtil.AddImage(buy, UIBuildUtil.LoadSprite("iv_buy"), true, false);
            UIBuildUtil.AddButton(buy, buyImage);

            RectTransform priceRect = UIBuildUtil.NewUI("price text", buy);
            UIBuildUtil.Stretch(priceRect);
            TextMeshProUGUI priceLabel = UIBuildUtil.AddText(priceRect, font, pack.PricePlaceholder, 36f,
                TextAlignmentOptions.Center, White);

            // IAPButton nằm TRÊN nút mua: nó tự AddListener trong Awake, nên không cần
            // persistent listener trong YAML — khớp quy ước "mọi button đều wire bằng code".
            IAPButton iapButton = buy.gameObject.AddComponent<IAPButton>();
            iapButton.productId = pack.ProductId;
            iapButton.priceText = priceLabel;

            EditorUtility.SetDirty(iapButton);
        }

        private static string GoldSpriteFor(int index)
        {
            // 7 gói, chỉ có 5 icon túi vàng: hai gói cuối dùng lại icon to nhất (đúng như Terra).
            string[] names = { "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5", "iv_gold4", "iv_gold5" };

            return names[Mathf.Clamp(index, 0, names.Length - 1)];
        }

        /// <summary>
        /// Gắn TweenUI cho panel và nhồi vào MenuAnimation. Copy nguyên component từ
        /// panel Settings nếu có, để shop mở/đóng giống các popup khác.
        /// </summary>
        private static void AttachPanelTween(ShopMenu shop, RectTransform panel)
        {
            MenuAnimation anim = shop.GetComponent<MenuAnimation>();

            TweenUI template = FindSettingsPanelTween(shop.transform.parent);
            TweenUI tween;

            if (template != null)
            {
                ComponentUtility.CopyComponent(template);
                ComponentUtility.PasteComponentAsNew(panel.gameObject);
                tween = panel.GetComponent<TweenUI>();
            }
            else
            {
                tween = panel.gameObject.AddComponent<TweenUI>();
                ConfigureDefaultTween(tween);
            }

            UIBuildUtil.SetObjectArrayField(anim, "_objectToAnimate", tween);
        }

        private static TweenUI FindSettingsPanelTween(Transform canvasRoot)
        {
            Transform settings = canvasRoot != null ? canvasRoot.Find(SettingsMenuName) : null;
            if (settings == null) return null;

            MenuAnimation anim = settings.GetComponent<MenuAnimation>();
            if (anim == null) return null;

            var so = new SerializedObject(anim);
            SerializedProperty list = so.FindProperty("_objectToAnimate");

            if (list == null || list.arraySize == 0) return null;

            return list.GetArrayElementAtIndex(0).objectReferenceValue as TweenUI;
        }

        /// <summary>Popup phóng to khi mở, thu nhỏ khi đóng — dùng khi không có khuôn để copy.</summary>
        private static void ConfigureDefaultTween(TweenUI tween)
        {
            var so = new SerializedObject(tween);

            Set(so, "_onEnable", 0.3f, Vector3.zero, Vector3.one, (int)AnimationTypes.Scale, (int)LeanTweenType.easeOutBack);
            Set(so, "_onDisable", 0.2f, Vector3.one, Vector3.zero, (int)AnimationTypes.Scale, (int)LeanTweenType.easeInBack);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(SerializedObject so, string statName, float duration, Vector3 from, Vector3 to,
            int animationType, int easeType)
        {
            SerializedProperty stat = so.FindProperty(statName);
            if (stat == null) return;

            stat.FindPropertyRelative("_duration").floatValue = duration;
            stat.FindPropertyRelative("_delay").floatValue = 0f;
            stat.FindPropertyRelative("_from").vector3Value = from;
            stat.FindPropertyRelative("_to").vector3Value = to;
            // intValue (giá trị thô) chứ không phải enumValueIndex: LeanTweenType không
            // chắc là enum liên tục từ 0, dùng index sẽ gán sai kiểu ease.
            stat.FindPropertyRelative("_animationType").intValue = animationType;
            stat.FindPropertyRelative("_easeType").intValue = easeType;
        }
    }
}

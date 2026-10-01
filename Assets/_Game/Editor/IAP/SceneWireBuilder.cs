using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace CubeJumpEditor
{
    /// <summary>
    /// Nối phần coin/shop/revive vào các menu đã có sẵn trong scene.
    ///
    /// Cố tình KHÔNG hardcode fileID hay toạ độ tuyệt đối: tool dò component theo kiểu,
    /// rồi lấy một phần tử có sẵn của menu đó làm khuôn (cha, kích thước, điểm neo) và
    /// đặt phần tử mới lệch xuống dưới. Nhờ vậy nó vẫn chạy khi layout bị sửa tay.
    /// Vị trí cuối cùng vẫn nên ngắm lại bằng mắt — xem log để biết nó đặt ở đâu.
    /// </summary>
    public static class SceneWireBuilder
    {
        private const string CoinPillName = "coin pill";
        private const string ShopButtonName = "shop button";
        private const string NotEnoughName = "not enough text";

        private static readonly Color PillTint = new Color(0.22f, 0.25f, 0.36f, 0.85f);

        [MenuItem("SansDev/IAP/Add Coin HUD + Shop Buttons", priority = 21)]
        public static void AddCoinHudAndShopButtons()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (!UIBuildUtil.EnsureSprites("Add Coin HUD", "coin")) return;

            Undo.SetCurrentGroupName("Add Coin HUD + Shop Buttons");
            int group = Undo.GetCurrentGroup();

            AddCoinHud<GameplayMenu>("_scoreText");
            AddCoinHud<MainMenu>("_settingsButton");

            AddShopButton<MainMenu>("_settingsButton", "_rateButton", "_creditButton");
            AddShopButton<GameoverMenu>("_homeButton");

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[SceneWire] Xong Coin HUD + nút Shop. Ngắm lại vị trí trong Game view 1080x1920.");
        }

        [MenuItem("SansDev/IAP/Convert Revive Button To Coin", priority = 22)]
        public static void ConvertReviveButtonToCoin()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (!UIBuildUtil.EnsureSprites("Convert Revive Button", "coin")) return;

            ReviveMenu revive = Object.FindAnyObjectByType<ReviveMenu>(FindObjectsInactive.Include);

            if (revive == null)
            {
                Debug.LogError("[SceneWire] Không tìm thấy ReviveMenu trong scene.");
                return;
            }

            Button continueButton = GetField<Button>(revive, "_continueButton");

            if (continueButton == null)
            {
                Debug.LogError("[SceneWire] ReviveMenu._continueButton chưa được gán trong Inspector.");
                return;
            }

            Undo.SetCurrentGroupName("Convert Revive Button To Coin");
            int group = Undo.GetCurrentGroup();

            int cost = EconomyConfig.Instance.ReviveCost;

            // Đổi icon "xem quảng cáo" thành icon coin.
            Image icon = FindChildIcon(continueButton);

            if (icon != null)
            {
                Undo.RecordObject(icon, "Revive icon");
                icon.sprite = UIBuildUtil.LoadSprite("coin");
                icon.preserveAspect = true;
                EditorUtility.SetDirty(icon);

                Debug.Log($"[SceneWire] Đổi icon nút revive ('{icon.gameObject.name}') sang coin.", icon);
            }
            else
            {
                Debug.LogWarning("[SceneWire] Không tìm thấy Image con của nút Continue — đổi icon bằng tay.");
            }

            // Nhãn nút thành giá coin, và gán vào _costText để runtime tự cập nhật.
            TMP_Text label = continueButton.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
            {
                Undo.RecordObject(label, "Revive label");
                label.text = cost.ToString();
                EditorUtility.SetDirty(label);

                UIBuildUtil.SetObjectField(revive, "_costText", label);

                Debug.Log($"[SceneWire] Nhãn nút revive = \"{cost}\", gán vào ReviveMenu._costText.", label);
            }
            else
            {
                Debug.LogWarning("[SceneWire] Nút Continue không có TMP_Text con — gán _costText bằng tay.");
            }

            // Dòng "không đủ coin".
            Transform panel = continueButton.transform.parent != null
                ? continueButton.transform.parent
                : revive.transform;

            UIBuildUtil.DestroyIfExists(panel, NotEnoughName);

            RectTransform notEnough = UIBuildUtil.NewUI(NotEnoughName, panel);
            RectTransform buttonRect = (RectTransform)continueButton.transform;

            UIBuildUtil.Place(notEnough, buttonRect.anchorMin, buttonRect.pivot,
                buttonRect.anchoredPosition + new Vector2(0f, -(buttonRect.sizeDelta.y * 0.5f + 44f)),
                new Vector2(560f, 56f));

            UIBuildUtil.AddText(notEnough, UIBuildUtil.LoadFont(), $"Cần {cost} coin", 32f,
                TextAlignmentOptions.Center, new Color(1f, 0.45f, 0.45f, 1f));

            notEnough.gameObject.SetActive(false);

            UIBuildUtil.SetObjectField(revive, "_notEnoughRoot", notEnough.gameObject);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"[SceneWire] Nút revive giờ tốn {cost} coin.", revive);
        }

        /// <summary>Thêm viên coin + <see cref="CoinHUD"/> vào menu, lấy một phần tử có sẵn làm khuôn.</summary>
        private static void AddCoinHud<T>(string templateFieldName) where T : Menu
        {
            T menu = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

            if (menu == null)
            {
                Debug.LogWarning($"[SceneWire] Không tìm thấy {typeof(T).Name} trong scene.");
                return;
            }

            if (menu.GetComponentInChildren<CoinHUD>(true) != null)
            {
                Debug.Log($"[SceneWire] {typeof(T).Name} đã có CoinHUD, bỏ qua.");
                return;
            }

            Transform parent = ResolveParent(menu, templateFieldName);

            UIBuildUtil.DestroyIfExists(parent, CoinPillName);

            // Neo góc trên trái, cách mép một khoảng an toàn với tai thỏ.
            RectTransform pill = UIBuildUtil.NewUI(CoinPillName, parent);
            UIBuildUtil.Place(pill, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -60f),
                new Vector2(300f, 80f));
            UIBuildUtil.AddPanelImage(pill, PillTint, false);

            RectTransform icon = UIBuildUtil.NewUI("coin icon", pill);
            UIBuildUtil.Place(icon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f),
                new Vector2(52f, 52f));
            UIBuildUtil.AddImage(icon, UIBuildUtil.LoadSprite("coin"), false, true);

            RectTransform textRect = UIBuildUtil.NewUI("coin text", pill);
            UIBuildUtil.Place(textRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 0f),
                new Vector2(200f, 70f));
            TextMeshProUGUI label = UIBuildUtil.AddText(textRect, UIBuildUtil.LoadFont(), "{0}", 40f,
                TextAlignmentOptions.Left, UIBuildUtil.GoldText);

            CoinHUD hud = pill.gameObject.AddComponent<CoinHUD>();
            UIBuildUtil.SetObjectField(hud, "_coinText", label);

            Debug.Log($"[SceneWire] Thêm Coin HUD vào {typeof(T).Name} (cha: '{parent.name}').", pill);
        }

        /// <summary>
        /// Thêm nút Shop và gán vào field "_shopButton" của menu.
        ///
        /// Nhân bản nút lấy làm khuôn thay vì dựng từ con số 0: như vậy nút Shop thừa hưởng
        /// nguyên nền 9-slice, kích thước, điểm neo và cấu trúc icon con của hàng nút hiện có,
        /// chỉ cần đổi sprite của icon. Dựng tay sẽ ra nút trần không nền, lệch hẳn thiết kế.
        /// </summary>
        /// <param name="slotFieldNames">
        /// Các nút anh em mà nếu đang bị ẩn thì Shop sẽ chiếm đúng vị trí của chúng.
        /// </param>
        private static void AddShopButton<T>(string templateFieldName, params string[] slotFieldNames)
            where T : Menu
        {
            T menu = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

            if (menu == null)
            {
                Debug.LogWarning($"[SceneWire] Không tìm thấy {typeof(T).Name} trong scene.");
                return;
            }

            Button template = GetField<Button>(menu, templateFieldName);

            if (template == null)
            {
                Debug.LogWarning($"[SceneWire] {typeof(T).Name}.{templateFieldName} chưa gán — " +
                                 "không có khuôn để đặt nút Shop, làm bằng tay.");
                return;
            }

            RectTransform templateRect = (RectTransform)template.transform;
            Transform parent = templateRect.parent;

            UIBuildUtil.DestroyIfExists(parent, ShopButtonName);

            GameObject go = Object.Instantiate(template.gameObject, parent);
            go.name = ShopButtonName;
            go.SetActive(true);

            Undo.RegisterCreatedObjectUndo(go, "Add Shop Button");

            Button shopButton = go.GetComponent<Button>();
            shopButton.interactable = true;

            var shopRect = (RectTransform)go.transform;
            shopRect.localScale = Vector3.one;
            shopRect.anchoredPosition = ResolveSlot(menu, templateRect, slotFieldNames);

            // Xếp ngay cạnh nút mẫu trong hierarchy cho dễ tìm.
            shopRect.SetSiblingIndex(templateRect.GetSiblingIndex());

            Image icon = FindChildIcon(shopButton);

            if (icon != null)
            {
                icon.sprite = UIBuildUtil.LoadIcon("shop");
                icon.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning($"[SceneWire] Bản sao nút Shop ({typeof(T).Name}) không có Image con " +
                                 "để đổi thành icon shop — kiểm tra lại bằng tay.");
            }

            UIBuildUtil.SetObjectField(menu, "_shopButton", shopButton);

            RegisterInMenuAnimation(menu, template.gameObject, go);

            Debug.Log($"[SceneWire] Nút Shop của {typeof(T).Name}: nhân bản từ " +
                      $"'{template.gameObject.name}', đặt tại {shopRect.anchoredPosition}.", shopButton);
        }

        /// <summary>
        /// Nếu nút mẫu có TweenUI nằm trong MenuAnimation của menu thì bản sao cũng phải được
        /// đăng ký, không thì lúc mở menu mọi nút khác bay vào còn nút Shop đứng im.
        /// </summary>
        private static void RegisterInMenuAnimation(Menu menu, GameObject template, GameObject clone)
        {
            TweenUI cloneTween = clone.GetComponent<TweenUI>();
            TweenUI templateTween = template.GetComponent<TweenUI>();

            if (cloneTween == null || templateTween == null) return;

            MenuAnimation anim = menu.GetComponent<MenuAnimation>();
            if (anim == null) return;

            var so = new SerializedObject(anim);
            SerializedProperty list = so.FindProperty("_objectToAnimate");
            if (list == null) return;

            bool templateIsAnimated = false;

            for (int i = 0; i < list.arraySize; i++)
            {
                Object v = list.GetArrayElementAtIndex(i).objectReferenceValue;

                if (v == cloneTween) return;              // đã đăng ký rồi
                if (v == templateTween) templateIsAnimated = true;
            }

            if (!templateIsAnimated) return;              // nút mẫu vốn không animate thì thôi

            UIBuildUtil.AppendToObjectArrayField(anim, "_objectToAnimate", cloneTween);

            Debug.Log($"[SceneWire] Đăng ký TweenUI của nút Shop vào {menu.GetType().Name}.MenuAnimation.");
        }

        /// <summary>
        /// Chọn chỗ đặt nút Shop: ưu tiên ô của một nút anh em đang bị ẩn — người thiết kế
        /// tắt nút nào thì Shop vào đúng chỗ đó. Không có thì xếp xuống dưới nút mẫu.
        /// </summary>
        private static Vector2 ResolveSlot(Menu menu, RectTransform templateRect, string[] slotFieldNames)
        {
            foreach (string field in slotFieldNames)
            {
                Button candidate = GetField<Button>(menu, field);

                if (candidate == null || candidate.gameObject.activeSelf) continue;

                var rect = (RectTransform)candidate.transform;

                Debug.Log($"[SceneWire] Dùng lại ô của '{candidate.gameObject.name}' (đang ẩn) " +
                          $"tại {rect.anchoredPosition} cho nút Shop.");

                return rect.anchoredPosition;
            }

            return templateRect.anchoredPosition + new Vector2(0f, -(templateRect.sizeDelta.y + 24f));
        }

        /// <summary>Lấy cha của một phần tử có sẵn trong menu để đặt phần tử mới cùng cấp.</summary>
        private static Transform ResolveParent(Menu menu, string templateFieldName)
        {
            Object template = GetField<Object>(menu, templateFieldName);

            if (template is Component component && component.transform.parent != null)
                return component.transform.parent;

            return menu.transform;
        }

        private static TValue GetField<TValue>(Object owner, string fieldName) where TValue : Object
        {
            var so = new SerializedObject(owner);
            SerializedProperty property = so.FindProperty(fieldName);

            return property != null ? property.objectReferenceValue as TValue : null;
        }

        /// <summary>Image con đầu tiên không phải chính nút — tức là icon bên trong nút.</summary>
        private static Image FindChildIcon(Button button)
        {
            Image[] images = button.GetComponentsInChildren<Image>(true);

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].gameObject != button.gameObject) return images[i];
            }

            return null;
        }
    }
}

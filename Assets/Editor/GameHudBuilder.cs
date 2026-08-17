using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

using U = RingGameEditor.UIBuildUtil;

namespace RingGameEditor
{
    /// <summary>
    /// Dựng nút "Hint" (gợi ý, tốn coin) cạnh nút Reload trong TopBar của Game scene.
    /// Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class GameHudBuilder
    {
        private const string TopBarName = "TopBar";
        private const string HintName = "Hint";
        private const string ToastName = "Toast";

        // Sprite nằm ngoài Assets/Sprites/ nên không resolve được qua UIBuildUtil.LoadSprite,
        // phải load thẳng theo đường dẫn — cùng ảnh nền tròn với nút Reload để đồng bộ giao diện.
        private const string BackdropSpritePath = "Assets/Images/Panels@2x-assets/Circles/CircleSmall_Stroke_4px.png";
        private const string IconSpritePath = "Assets/Images/Point Hand.png";

        private static readonly Vector2 ButtonSize = new Vector2(100f, 100f);
        // Reload neo (1,0.5) tại (-80,0), rộng 100 => trải -130..-30. Đặt Hint sát bên trái,
        // cách 10px: tâm tại -80-100-10 = -190, trải -240..-140.
        private const float AnchoredX = -190f;

        [MenuItem("Tools/RingGame/Build Game Hint Button")]
        public static void BuildGameHintButton()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Game Hint Button", "Đã dựng nút Hint cạnh Reload.", "OK");
        }

        internal static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Game Hint Button", "Mở scene Game.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[GameHudBuilder] Không tìm thấy GameObject '" + U.CanvasName + "' trong scene.");
                return false;
            }

            Transform canvas = canvasGo.transform;

            Transform topBar = canvas.Find(TopBarName);
            if (topBar == null)
            {
                Debug.LogError("[GameHudBuilder] Không tìm thấy '" + TopBarName + "' trong Canvas. Mở scene Game.unity trước.");
                EditorUtility.DisplayDialog("Build Game Hint Button", "Mở scene Game.unity trước đã.", "OK");
                return false;
            }

            Sprite backdropSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackdropSpritePath);
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconSpritePath);
            if (backdropSprite == null || iconSprite == null)
            {
                Debug.LogError("[GameHudBuilder] Thiếu sprite: " + BackdropSpritePath + " hoặc " + IconSpritePath);
                EditorUtility.DisplayDialog("Build Game Hint Button", "Thiếu sprite nút Hint. Xem Console.", "OK");
                return false;
            }

            Font font = U.LoadDefaultFont();
            if (font == null)
            {
                Debug.LogError("[GameHudBuilder] Không tìm được font để dựng UI.");
                return false;
            }

            U.DestroyIfExists(topBar, HintName);
            U.DestroyIfExists(canvas, ToastName);

            RectTransform hintSlot = U.NewUI(HintName, topBar);
            U.Place(hintSlot, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(AnchoredX, 0f), ButtonSize);
            Image backdrop = U.AddImage(hintSlot, backdropSprite, true, false);

            RectTransform icon = U.NewUI("Icon", hintSlot);
            U.Place(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50f, 50f));
            U.AddImage(icon, iconSprite, false, true);

            HintController hintController = hintSlot.gameObject.AddComponent<HintController>();

            Button hintButton = U.AddButton(hintSlot, backdrop);
            UnityEventTools.AddVoidPersistentListener(hintButton.onClick, hintController.UseHint);

            // ---------- Toast: thông báo ngắn (vd "Không đủ coin!"), giữa dưới màn hình ----------

            RectTransform toastRoot = U.NewUI(ToastName, canvas);
            U.Place(toastRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(420f, 70f));

            RectTransform toastPanel = U.NewUI("Panel", toastRoot);
            U.Stretch(toastPanel);
            U.AddSlicedImage(toastPanel, RoundedRectGenerator.Ensure(), new Color(0f, 0f, 0f, 0.8f), false);

            RectTransform toastTextRt = U.NewUI("Text", toastPanel);
            U.Stretch(toastTextRt);
            Text toastText = U.AddText(toastTextRt, font, "", 32, TextAnchor.MiddleCenter, Color.white);

            ToastMessage toast = toastRoot.gameObject.AddComponent<ToastMessage>();
            U.SetObjectField(toast, "m_Root", toastPanel.gameObject);
            U.SetObjectField(toast, "m_Text", toastText);
            toastPanel.gameObject.SetActive(false);

            U.SetObjectField(hintController, "m_Toast", toast);

            Debug.Log("[GameHudBuilder] Đã dựng nút Hint (tốn " + HintController.HintCost + " coin) cạnh Reload, kèm thông báo Toast.", hintSlot.gameObject);

            return true;
        }
    }
}

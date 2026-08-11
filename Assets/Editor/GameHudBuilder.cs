using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TapTap;

using U = TapTapEditor.UIBuildUtil;

namespace TapTapEditor
{
    /// <summary>
    /// Bổ sung phần HUD gắn với <see cref="GameUI"/>: điểm cao nhất và huy hiệu "NEW BEST!".
    /// Dùng legacy <c>UnityEngine.UI.Text</c> cho khớp với UI sẵn có của scene.
    /// </summary>
    public static class GameHudBuilder
    {
        private const string BestScoreName = "Best Score Text";
        private const string NewBestName = "New Best Badge";

        [MenuItem("Tools/TapTap/Build Game HUD (best score)")]
        public static void BuildGameHud()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Game HUD", "Đã thêm best score + huy hiệu NEW BEST.", "OK");
        }

        internal static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return false;

            GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[HudBuilder] Không tìm thấy '" + U.CanvasName + "'.");
                return false;
            }

            GameUI gameUI = canvasGo.GetComponentInChildren<GameUI>(true);
            if (gameUI == null)
            {
                Debug.LogError("[HudBuilder] Không tìm thấy component GameUI trong Canvas.");
                return false;
            }

            Font font = LegacyFont();
            if (font == null)
            {
                Debug.LogError("[HudBuilder] Không lấy được font builtin của Unity.");
                return false;
            }

            Transform canvas = canvasGo.transform;

            // Đặt cùng chỗ với các text sẵn có để thứ tự vẽ hợp lý.
            Transform scoreParent = canvas.Find("Score Canvas") ?? canvas;
            Transform overParent = canvas.Find("UI Canvas") ?? canvas;

            U.DestroyIfExists(scoreParent, BestScoreName);
            U.DestroyIfExists(overParent, NewBestName);

            // Điểm cao nhất: ngay dưới điểm hiện tại (Score Text thụt 64px từ mép trên, cỡ 144).
            RectTransform best = U.NewUI(BestScoreName, scoreParent);
            U.Place(best, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(700f, 80f));
            Text bestText = AddText(best, font, "BEST {0}", 56, TextAnchor.MiddleCenter, U.GoldText);

            // Huy hiệu kỷ lục: phía trên chữ GAME OVER (chữ đó nằm giữa, lệch lên 72).
            RectTransform badge = U.NewUI(NewBestName, overParent);
            U.Place(badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(900f, 110f));
            AddText(badge, font, "NEW BEST!", 72, TextAnchor.MiddleCenter, U.GoldText);
            badge.gameObject.AddComponent<GraphicBlinker>();
            U.SetEntityName(badge.GetComponent<GraphicBlinker>(), "New Best Badge");

            badge.gameObject.SetActive(false);

            U.SetObjectField(gameUI, "m_BestScoreText", bestText);
            U.SetObjectField(gameUI, "m_NewBestBadge", badge.gameObject);

            Debug.Log("[HudBuilder] Đã thêm '" + BestScoreName + "' và '" + NewBestName + "'.", gameUI);

            return true;
        }

        private static Text AddText(RectTransform rect, Font font, string content, int size, TextAnchor anchor, Color color)
        {
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        private static Font LegacyFont()
        {
            // Unity 2022+ đổi tên font builtin từ Arial.ttf sang LegacyRuntime.ttf.
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}

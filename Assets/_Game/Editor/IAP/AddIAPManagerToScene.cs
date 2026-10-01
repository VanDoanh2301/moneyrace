using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CubeJumpEditor
{
    /// <summary>
    /// Tự động đảm bảo có GameObject "IAPManager" trong scene trước khi Play,
    /// kèm menu item để thêm thủ công. Port từ game Terra.
    /// </summary>
    public static class AddIAPManagerToScene
    {
        private const string GameScenePath = "Assets/_Game/Scenes/Game.unity";

        [InitializeOnLoad]
        private static class EditorStartup
        {
            static EditorStartup()
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }

            private static void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                    EnsureIAPManagerInActiveScene();
            }
        }

        private static void EnsureIAPManagerInActiveScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;

            if (FindIAPManager(scene) != null) return;

            var go = new GameObject("IAPManager");
            go.AddComponent<IAPManager>();

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"[AddIAPManager] Tự động thêm IAPManager vào scene '{scene.name}' khi khởi động game.");
        }

        [MenuItem("SansDev/IAP/Add IAPManager to Game Scene")]
        public static void AddIAPManagerToGameScene()
        {
            AddIAPManagerToSceneAtPath(GameScenePath);
        }

        [MenuItem("SansDev/IAP/Add IAPManager to Current Scene")]
        public static void AddIAPManagerToCurrentScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                EditorUtility.DisplayDialog("Add IAPManager", "Mở một scene trước (ví dụ Game.unity).", "OK");
                return;
            }

            AddIAPManagerToSceneAtPath(scene.path);
        }

        private static void AddIAPManagerToSceneAtPath(string scenePath)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError("[AddIAPManager] Không tìm thấy scene: " + scenePath);
                EditorUtility.DisplayDialog("Add IAPManager", "Không tìm thấy scene:\n" + scenePath, "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (scene.path != scenePath)
                scene = EditorSceneManager.OpenScene(scenePath);

            var existing = FindIAPManager(scene);

            if (existing != null)
            {
                Debug.Log($"[AddIAPManager] IAPManager đã có trong scene {scene.name} ({existing.name}).", existing);
                return;
            }

            var go = new GameObject("IAPManager");
            go.AddComponent<IAPManager>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[AddIAPManager] Đã thêm GameObject 'IAPManager' vào {scene.name}.");
        }

        /// <summary>Dò cả trong con, không chỉ root.</summary>
        private static GameObject FindIAPManager(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = root.GetComponentInChildren<IAPManager>(true);

                if (found != null) return found.gameObject;
            }

            return null;
        }
    }
}

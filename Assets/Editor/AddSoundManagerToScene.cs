using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RingGameEditor
{
    /// <summary>
    /// Tự động đảm bảo có GameObject "SoundManager" trong scene trước khi Play,
    /// kèm menu item để thêm thủ công. Cùng kiểu với AddIAPManagerToScene.cs.
    /// </summary>
    public static class AddSoundManagerToScene
    {
        // Menu.unity là scene đầu tiên load — SoundManager là DontDestroyOnLoad nên chỉ cần
        // có mặt ở đây là sống xuyên suốt sang Game.unity.
        private const string GameplayScenePath = "Assets/Scenes/Menu.unity";

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
                {
                    var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        EnsureSoundManagerInScene(scene);
                    }
                }
            }
        }

        /// <summary>Đảm bảo SoundManager có trong scene. Trả về true nếu vừa tạo mới.</summary>
        internal static bool EnsureSoundManagerInScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (FindSoundManager(scene) != null) return false;

            var go = new GameObject("SoundManager");
            go.AddComponent<SoundManager>();

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[AddSoundManager] Đã thêm GameObject 'SoundManager' vào scene '" + scene.name + "'.");

            return true;
        }

        [MenuItem("Tools/RingGame/Add SoundManager to Gameplay Scene")]
        public static void AddSoundManagerToGameplay()
        {
            AddSoundManagerToSceneAtPath(GameplayScenePath);
        }

        [MenuItem("Tools/RingGame/Add SoundManager to Current Scene")]
        public static void AddSoundManagerToCurrentScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                EditorUtility.DisplayDialog("Add SoundManager", "Mở một scene trước (ví dụ: Menu.unity).", "OK");
                return;
            }

            AddSoundManagerToSceneAtPath(scene.path);
        }

        private static void AddSoundManagerToSceneAtPath(string scenePath)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError("[AddSoundManager] Không tìm thấy scene: " + scenePath);
                EditorUtility.DisplayDialog("Add SoundManager", "Không tìm thấy scene:\n" + scenePath, "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != scenePath)
            {
                scene = EditorSceneManager.OpenScene(scenePath);
            }

            if (FindSoundManager(scene) != null)
            {
                Debug.Log("[AddSoundManager] SoundManager đã có trong scene: " + scene.name);
                EditorUtility.DisplayDialog("Add SoundManager", "SoundManager đã có trong scene.", "OK");
                return;
            }

            EnsureSoundManagerInScene(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Add SoundManager", "Đã thêm SoundManager vào scene " + scene.name + ".", "OK");
        }

        private static GameObject FindSoundManager(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = root.GetComponentInChildren<SoundManager>(true);
                if (found != null) return found.gameObject;
            }

            return null;
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TapTap;

using U = TapTapEditor.UIBuildUtil;

namespace TapTapEditor
{
    /// <summary>
    /// Tạo GameObject "Sound Manager" trong scene và gán sẵn các clip trong <c>Assets/Audio</c>.
    /// </summary>
    public static class SoundManagerSetup
    {
        private const string AudioFolder = "Assets/Audio/";

        private const string ObjectName = "Sound Manager";

        /// <summary>Tên field trên SoundManager → tên file wav.</summary>
        private static readonly string[,] Clips =
        {
            { "m_MusicLoop", "music_loop" },
            { "m_Tap",       "sfx_tap" },
            { "m_Coin",      "sfx_coin" },
            { "m_GameOver",  "sfx_gameover" },
            { "m_Button",    "sfx_button" },
            { "m_Purchase",  "sfx_purchase" },
            { "m_Best",      "sfx_best" },
        };

        [MenuItem("Tools/TapTap/Add SoundManager to Scene")]
        public static void AddSoundManager()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Add SoundManager", "Đã thêm Sound Manager và gán đủ clip.", "OK");
        }

        internal static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return false;

            SoundManager manager = FindSoundManager(scene);
            if (manager == null)
            {
                GameObject go = new GameObject(ObjectName);
                manager = go.AddComponent<SoundManager>();
            }

            U.SetEntityName(manager, "Sound Manager");

            bool missing = false;

            for (int i = 0; i < Clips.GetLength(0); i++)
            {
                string field = Clips[i, 0];
                string file = Clips[i, 1];

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + file + ".wav");
                if (clip == null)
                {
                    Debug.LogError("[SoundSetup] Thiếu clip: " + AudioFolder + file + ".wav");
                    missing = true;
                    continue;
                }

                U.SetObjectField(manager, field, clip);
            }

            if (missing)
            {
                EditorUtility.DisplayDialog("Add SoundManager", "Thiếu file âm thanh trong Assets/Audio. Xem Console.", "OK");
                return false;
            }

            Debug.Log("[SoundSetup] Sound Manager sẵn sàng với " + Clips.GetLength(0) + " clip.", manager);

            return true;
        }

        private static SoundManager FindSoundManager(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                SoundManager found = root.GetComponentInChildren<SoundManager>(true);
                if (found != null) return found;
            }

            return null;
        }
    }
}

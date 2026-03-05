using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddIAPManagerToScene
{
    private const string GameplayScenePath = "Assets/Scenes/Main.unity";

    [InitializeOnLoad]
    private static class EditorStartup
    {
        static EditorStartup()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Khi sắp vào Play mode, kiểm tra và thêm IAPManager nếu chưa có
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EnsureIAPManagerInActiveScene();
            }
        }
    }

    /// <summary>Đảm bảo IAPManager có trong scene hiện tại trước khi Play.</summary>
    private static void EnsureIAPManagerInActiveScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<IAPManager>() != null)
                return; // Đã có IAPManager, không cần làm gì
        }

        var go = new GameObject("IAPManager");
        go.AddComponent<IAPManager>();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[AddIAPManager] Tự động thêm IAPManager vào scene '" + scene.name + "' khi khởi động game.");
    }

    [MenuItem("Tools/Add IAPManager to Gameplay Scene")]
    public static void AddIAPManagerToGameplay()
    {
        AddIAPManagerToSceneAtPath(GameplayScenePath);
    }

    [MenuItem("Tools/Add IAPManager to Current Scene")]
    public static void AddIAPManagerToCurrentScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog("Add IAPManager", "Mở một scene trước (ví dụ: Main.unity).", "OK");
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

        var scene = EditorSceneManager.OpenScene(scenePath);

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<IAPManager>() != null)
            {
                Debug.Log("[AddIAPManager] IAPManager đã có trong scene: " + scene.name + " (" + root.name + ")");
                EditorUtility.DisplayDialog("Add IAPManager", "IAPManager đã có trong scene.", "OK");
                return;
            }
        }

        var go = new GameObject("IAPManager");
        go.AddComponent<IAPManager>();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[AddIAPManager] Đã thêm GameObject 'IAPManager' vào " + scene.name + ". Chạy game để IAP khởi động.");
        EditorUtility.DisplayDialog("Add IAPManager", "Đã thêm IAPManager vào scene " + scene.name + ".", "OK");
    }
}

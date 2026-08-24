using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Tự động đảm bảo có GameObject "IAPManager" trong scene trước khi Play,
/// kèm menu item để thêm thủ công.
/// </summary>
public static class AddIAPManagerToScene
{
    private const string MainMenuScenePath = "Assets/Grate It/Levels/Main Menu.unity";

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
                EnsureIAPManagerInActiveScene();
            }
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

        Debug.Log("[AddIAPManager] Tự động thêm IAPManager vào scene '" + scene.name + "' khi khởi động game.");
    }

    [MenuItem("Tools/GraceGame/Add IAPManager to Main Menu Scene")]
    public static void AddIAPManagerToMainMenu()
    {
        AddIAPManagerToSceneAtPath(MainMenuScenePath);
    }

    [MenuItem("Tools/GraceGame/Add IAPManager to Current Scene")]
    public static void AddIAPManagerToCurrentScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog("Add IAPManager", "Mở một scene trước (ví dụ: Main Menu.unity).", "OK");
            return;
        }

        AddIAPManagerToSceneAtPath(scene.path);
    }

    public static void AddIAPManagerToSceneAtPath(string scenePath)
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
        {
            scene = EditorSceneManager.OpenScene(scenePath);
        }

        var existing = FindIAPManager(scene);
        if (existing != null)
        {
            Debug.Log("[AddIAPManager] IAPManager đã có trong scene: " + scene.name + " (" + existing.name + ")", existing);
            EditorUtility.DisplayDialog("Add IAPManager", "IAPManager đã có trong scene.", "OK");
            return;
        }

        var go = new GameObject("IAPManager");
        go.AddComponent<IAPManager>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[AddIAPManager] Đã thêm GameObject 'IAPManager' vào " + scene.name + ". Chạy game để IAP khởi động.");
        EditorUtility.DisplayDialog("Add IAPManager", "Đã thêm IAPManager vào scene " + scene.name + ".", "OK");
    }

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

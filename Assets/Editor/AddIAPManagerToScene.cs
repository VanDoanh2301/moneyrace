using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddIAPManagerToScene
{
    private const string IAPManagerScenePath = "Assets/Original Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Add IAPManager to Sample Scene")]
    public static void AddIAPManager()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var scene = EditorSceneManager.OpenScene(IAPManagerScenePath);
        bool found = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<IAPManager>() != null)
            {
                found = true;
                Debug.Log("[AddIAPManager] IAPManager đã có trong SampleScene: " + root.name);
                return;
            }
        }

        var go = new GameObject("IAPManager");
        go.AddComponent<IAPManager>();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[AddIAPManager] Đã thêm GameObject 'IAPManager' với component IAPManager vào SampleScene.unity. Chạy game để IAP khởi động và xem log.");
    }
}

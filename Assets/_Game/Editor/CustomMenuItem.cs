using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class CustomMenuItem : MonoBehaviour
{
    [MenuItem("SansDev/Open Game Scene", priority = 0)]
    static void LoadGameScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game.unity");
        }
    }

    [MenuItem("SansDev/Customize/Credit Panel")]
    static void OpenCreditData()
    {
        string path = "Assets/_Game/ScriptableObject/Credit Data.asset";
        CreditDataSO data = (CreditDataSO)AssetDatabase.LoadAssetAtPath(path, typeof(CreditDataSO));
        Selection.activeObject = data;
    }

    /// <summary>
    /// Mở cấu hình tỉ lệ score->coin và giá hồi sinh. Tự tạo asset nếu chưa có.
    /// Phải nằm trong Resources vì IAPManager đọc nó trước khi scene load.
    /// </summary>
    [MenuItem("SansDev/Customize/Economy Config")]
    static void OpenEconomyConfig()
    {
        const string folder = "Assets/Resources";
        const string path = folder + "/Economy Config.asset";

        EconomyConfig config = AssetDatabase.LoadAssetAtPath<EconomyConfig>(path);

        if (config == null)
        {
            if (!Directory.Exists(folder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            config = ScriptableObject.CreateInstance<EconomyConfig>();

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CustomMenuItem] Đã tạo {path} với giá trị mặc định " +
                      $"({config.CoinPerScore} coin / điểm, hồi sinh {config.ReviveCost} coin).", config);
        }

        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }
}

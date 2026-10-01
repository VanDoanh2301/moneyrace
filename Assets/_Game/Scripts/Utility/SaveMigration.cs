using UnityEngine;

/// <summary>
/// Dọn dữ liệu lưu đã lỗi thời khi game khởi động.
/// Chạy trước khi scene load nên không cần GameObject nào trong scene.
/// </summary>
public static class SaveMigration
{
    private const string PPK_SAVE_VERSION = "SaveVersion";

    private const int CurrentVersion = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Migrate()
    {
        int version = PlayerPrefs.GetInt(PPK_SAVE_VERSION, 0);

        if (version >= CurrentVersion) return;

        if (version < 1)
        {
            // "npa" = cờ non-personalized ads của AdMob, không còn ai đọc sau khi bỏ quảng cáo.
            PlayerPrefs.DeleteKey("npa");
        }

        PlayerPrefs.SetInt(PPK_SAVE_VERSION, CurrentVersion);
        PlayerPrefs.Save();
    }
}

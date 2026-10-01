using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    public static void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(levelIndex);
        }
        else
        {
            Debug.LogWarning("LEVELLOADER LoadLevel Error: invalid scene specified");
        }
    }

    public static void ReloadLevel()
    {
        LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Load lại scene đang chạy theo TÊN, không theo buildIndex: scene chưa được
    /// đăng ký trong Build Settings có buildIndex = -1 và sẽ không load được.
    /// </summary>
    public static IEnumerator ReloadLevelAsync(Action OnSceneLoaded = null)
    {
        yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        OnSceneLoaded?.Invoke();
    }
}
using UnityEngine;

public class AnalyticManager : MonoBehaviour
{
    void Start()
    {
        LogEventStartGame();
    }

    public static void LogEventStartGame()
    {
        PlayerPrefs.SetInt("joingame", PlayerPrefs.GetInt("joingame", 0) + 1);
    }

    public static void LogEventStartLevel(int level)
    {
        // Analytics removed (Firebase)
    }

    public static void LogEventLevelComplete(int level)
    {
        // Analytics removed (Firebase)
    }

    public static void LogEventLevelFail(int level)
    {
        // Analytics removed (Firebase)
    }

    public static void LogEventAdsInter()
    {
        // Analytics removed (Firebase)
    }

    public static void LogEventAdsReward()
    {
        // Analytics removed (Firebase)
    }
}

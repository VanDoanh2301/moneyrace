using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;

public class AnalyticManager : MonoBehaviour
{
    FirebaseApp app;
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
        LogEventStartGame();
    }
    public static void LogEventStartGame()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("start_level", "total_join", PlayerPrefs.GetInt("joingame", 0));
        PlayerPrefs.SetInt("joingame", PlayerPrefs.GetInt("joingame", 0) + 1);
    }
    public static void LogEventStartLevel(int level)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("start_level", "level", level);
    }
    public static void LogEventLevelComplete(int level)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("level_complete", "level", level);
    }
    public static void LogEventLevelFail(int level)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("level_fail", "level", level);
    }
    public static void LogEventAdsInter()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("inter_ads");
    }
    public static void LogEventAdsReward()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("reward_ads");
    }
}

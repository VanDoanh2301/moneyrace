using System;
using UnityEngine;

/// <summary>
/// Stub: Ads removed from app. Kept for compatibility with existing scenes/prefabs.
/// </summary>
public class AdmobManager : MonoBehaviour
{
    public static AdmobManager instance;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void ShowAdReward(Action complete, Action fail)
    {
        complete?.Invoke();
    }

    public void ShowAdInter()
    {
        // No-op: ads removed
    }
}

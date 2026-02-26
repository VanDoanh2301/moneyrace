#if USE_ADMOB
using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AdmobManager : MonoBehaviour
{
    public static AdmobManager instance;
    public AdmobOpenApp openApp;
    public AdmobBanner banner;
    public AdmobInter inter;
    public AdmobReward reward;
    public bool isShowBanner;
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
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });
        if (isShowBanner)
            banner.LoadAd();
        Invoke("DelayShowAppOpen", 3f);
    }

    void DelayShowAppOpen()
    {
        openApp.LoadAd();
    }
    public void ShowAdReward(Action complete, Action fail)
    {
#if UNITY_EDITOR
        complete?.Invoke();
#endif
        reward.ShowRewardedAd(complete, fail);
    }
    public void ShowAdInter()
    {
        inter.ShowAd();
    }
}
#endif

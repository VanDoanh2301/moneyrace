#if USE_ADMOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobOpenApp : MonoBehaviour
{
#if UNITY_ANDROID
    private const string AD_UNIT_ID = "ca-app-pub-1866944813898601/6319736771";
#elif UNITY_IOS
    private const string AD_UNIT_ID = "ca-app-pub-3940256099942544/5662855259";
#else
    private const string AD_UNIT_ID = "unexpected_platform";
#endif

   

    private AppOpenAd ad;

    private bool isShowingAd = false;
    private DateTime loadTime;

    private bool IsAdAvailable
    {
        get
        {
            return ad != null && (System.DateTime.UtcNow - loadTime).TotalHours < 4;
        }
    }
    private void Start()
    {
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }
    public void LoadAd()
    {
        AdRequest request = new AdRequest.Builder().Build();

        // Load an app open ad for portrait orientation
        AppOpenAd.LoadAd(AD_UNIT_ID, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
        {
            if (error != null)
            {
                // Handle the error.
                Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
                return;
            }

            // App open ad is loaded.
            ad = appOpenAd;
            loadTime = DateTime.UtcNow;
        }));
    }
    public void ShowAdIfAvailable()
    {
        if (!IsAdAvailable || isShowingAd)
        {
            return;
        }

        ad.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
        ad.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
        ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
        ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
        ad.OnPaidEvent += HandlePaidEvent;

        ad.Show();
    }

    private void HandleAdDidDismissFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Closed app open ad");
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        ad = null;
        isShowingAd = false;
        LoadAd();
    }

    private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
    {
        Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        ad = null;
        LoadAd();
    }

    private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Displayed app open ad");
        isShowingAd = true;
    }

    private void HandleAdDidRecordImpression(object sender, EventArgs args)
    {
        Debug.Log("Recorded ad impression");
    }

    private void HandlePaidEvent(object sender, AdValueEventArgs args)
    {
        Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
                args.AdValue.CurrencyCode, args.AdValue.Value);
    }
    private void OnAppStateChanged(AppState state)
    {
        // Display the app open ad when the app is foregrounded.
        Debug.Log("App State is " + state);
        if (state == AppState.Foreground)
        {
           ShowAdIfAvailable();
        }
    }
}
#endif
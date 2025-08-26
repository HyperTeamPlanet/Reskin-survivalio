using System;
using GameAnalyticsSDK;
using GoogleMobileAds.Api;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    private void Awake()
    {
        GameAnalytics.Initialize();
        Instance = this;
    }

    public void OnLevelComplete(int level)
    {
        Debug.Log("Analytic Worked2" + level);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Completed Level " + level);
    }
    
    public void OnLevelStart(int level)
    {
        Debug.Log("Analytic Worked" + level);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Started Level " + level);
    }

    public void OnLevelFailed(int level)
    {
        Debug.Log("Analytic Worked3" + level);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Failed Level " + level);
    }
}

public class AdMob
{
    private bool _isFirstTime;
    public AdMob()
    {
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }

            Debug.Log("Google Mobile Ads initialization complete.");
        });
    }

    public void ShowInterstitial()
    {
        if (_isFirstTime)
        {
            _isFirstTime = true;
            return;
        }
        var adRequest = new AdRequest();

        var id = "ca-app-pub-3689652447565522/3284616141";
        #if UNITY_EDITOR
        id = "ca-app-pub-3940256099942544/4411468910";
        #endif 
        InterstitialAd.Load("AD_UNIT_ID", adRequest, (InterstitialAd interstitialAd, LoadAdError error) =>
        {
            if (error != null)
            {
                // The ad failed to load.
                return;
            }

            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                interstitialAd.Show();
            }
            // The ad loaded successfully.
        });
    }
}

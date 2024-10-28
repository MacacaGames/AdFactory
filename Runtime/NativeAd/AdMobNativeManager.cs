#if AdFactory_Admob_Native
using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.AdManager;
using UnityEngine;

public class AdMobNativeManager
{
    static AdMobNativeManager _Instance;
    public static AdMobNativeManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new AdMobNativeManager();
            }
            return _Instance;
        }
    }

    private NativeAd nativeAd;

    /// <summary>
    /// 設置原生廣告
    /// </summary>
    /// <param name="AD_UNIT">廣告單元 ID</param>
    /// <param name="OnLoadSuccess">成功加載後的回調</param>
    /// <param name="OnLoadFaild">加載失敗的回調</param>
    /// <param name="extras">其他附加參數</param>
    public void SetUpNativeAd(string AD_UNIT, Action<NativeAd> OnLoadSuccess, Action OnLoadFaild, Dictionary<string, string> extras = null)
    {
        var adLoader = new AdLoader.Builder(AD_UNIT)
            .ForNativeAd()
            .Build();

        // 加載成功處理
        adLoader.OnNativeAdLoaded += delegate (object sender, NativeAdEventArgs e)
        {
            nativeAd = e.nativeAd;
            Debug.Log("Native Ad Loaded: " + AD_UNIT);
            OnLoadSuccess?.Invoke(nativeAd);
        };

        // 加載失敗處理
        adLoader.OnAdFailedToLoad += delegate (object sender, AdFailedToLoadEventArgs e)
        {
            Debug.LogError("Native Ad Failed to Load: " + AD_UNIT + " ,msg: " + e.LoadAdError.GetMessage());
            OnLoadFaild?.Invoke();
        };

        var adRequest = new AdRequest();
        if (extras != null)
        {
            adRequest.Extras = extras;
        }

        // 開始加載廣告
        adLoader.LoadAd(adRequest);
    }

    /// <summary>
    /// 銷毀已加載的廣告
    /// </summary>
    public void Destroy()
    {
        nativeAd?.Destroy();
    }
}
#endif
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AdFactory 統一對外提供所有廣告的顯示與撥放，具體的廣告供應者則以 IAdManager 的實作為主
/// </summary>
public class AdFactory : MonoBehaviour
{
    static AdFactory _instance;
    public static AdFactory Instance
    {
        get
        {
            if (_instance == null)
                _instance = Initiate();
            return _instance;
        }
    }
    static AdFactory Initiate()
    {
        GameObject host = new GameObject();
        host.name = "AdFactory";
        DontDestroyOnLoad(host);
        return host.AddComponent<AdFactory>();
    }

    public static bool IsInternetAvaliable
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
    IAdManager fallbackAdManager;
    IAdManager mainAdManager;
    [Header("Test Parameters")]
    [SerializeField]
    AdFactory.RewardResult EditorTestResult = AdFactory.RewardResult.Success;
    [Header("IsRewardViedoAvaliable")]
    [SerializeField]
    bool IsRewardViedoAvaliableDirectResult = false;
    [SerializeField]
    bool IsRewardViedoAvaliableLoadedResult = true;
    [SerializeField]
    bool IsInterstitialAvaliable = true;
    [SerializeField]
    float IsRewardViedoAvaliableLoadedTime = 2f;
    public FallbackHandle fallbackHandle = FallbackHandle.DontFallback;

    public static float FixedLoadingTime = 1f;

    /// <summary>
    /// 註冊一個事件，該事件將會於 廣告顯示「前」執行
    /// </summary>
    public Action OnBeforeAdShow;

    /// <summary>
    /// 註冊一個事件，該事件將會於 廣告顯示「後」執行
    /// </summary>
    public Action OnAfterAdShow;
    /// <summary>
    /// AdType
    /// Result
    /// analysic data
    /// placement
    /// </summary>
    public Action<AdType, RewardResult, string, string> OnAdResult;

    /// <summary>
    /// AdType
    /// analysic data
    /// placement
    /// </summary>
    public Action<AdType, string, string> OnAdShowSuccess;

    /// <summary>
    /// AdType
    /// analysic data
    /// placement
    /// </summary>
    public Action<AdType, string, string> OnAdClick;
    /// <summary>
    /// AdType
    /// analysic data
    /// placement
    /// </summary>
    public Action<AdType, string, string> OnAdRequestToShow;

    /// <summary>
    /// AdType
    /// analysic data
    /// placement
    /// </summary>
    public Action<AdType, string, string> OnAdImpress;
    public void Init(IAdManager provider)
    {
        if (CheckInit())
        {
#if UNITY_EDITOR
            Debug.LogError("AdFactory is Inited Return");
#endif
            return;
        }

        if (provider == null)
        {
            Debug.LogError("AdFactory provider is null, Return");
            return;
        }

        Debug.LogWarning("Init AdFactory with " + provider);

        mainAdManager = provider;
        mainAdManager.Init();
    }

    public void AddFallbackAdmanager(IAdManager provider)
    {
        fallbackAdManager = provider;
        fallbackAdManager.Init();
    }

    public void PreLoadRewardedAd(string placements, AdManagerType adManagerType = AdManagerType.Main)
    {


#if UNITY_EDITOR
        //Do nothing in Editor
#else
        var currentAdManager = mainAdManager;
        if (fallbackAdManager != null &&
            adManagerType == AdManagerType.Fallback)
        {
            currentAdManager = fallbackAdManager;
        }
        currentAdManager.PreLoadRewardedAd(placements);
#endif
    }
    public void PreLoadInterstitialAds(string placements, AdManagerType adManagerType = AdManagerType.Main)
    {

#if UNITY_EDITOR
        //Do nothing in Editor
#else
        var currentAdManager = mainAdManager;
        if (fallbackAdManager != null &&
            adManagerType == AdManagerType.Fallback)
        {
            currentAdManager = fallbackAdManager;
        }
        currentAdManager.PreLoadInterstitialAds(placements);
#endif
    }
    /// <summary>
    /// 請求並顯示橫幅廣告
    /// </summary>
    /// <returns>true 代表請求成功, false 代表請求失敗或是或是廣告提供者不支援橫幅廣告</returns>
    public bool ShowBannerAd(string placement = "")
    {
        if (!CheckInit())
        {
            Debug.LogError("AdFactory is not Init");
            return false;
        }
#if UNITY_EDITOR
        return false;
#else
        return mainAdManager.ShowBannerAd(placement);
#endif
    }
    /// <summary>
    /// 請求並顯示橫幅廣告
    /// </summary>
    /// <returns>true 代表請求成功, false 代表請求失敗或是或是廣告提供者不支援橫幅廣告</returns>
    public bool LoadBannerAd()
    {
        if (!CheckInit())
        {
            Debug.LogError("AdFactory is not Init");
            return false;
        }
#if UNITY_EDITOR
        return false;
#else
        return mainAdManager.LoadBannerAd();
#endif
    }
    public int GetBannerHeight()
    {
        if (!CheckInit())
        {
            Debug.LogError("AdFactory is not Init");
            return 0;
        }
#if UNITY_EDITOR
        return 0;
#else
       return mainAdManager.GetBannerHeight();
#endif

    }

    /// <summary>
    /// 查詢目前畫面上是否有橫幅顯示
    /// </summary>
    /// <returns>true 代表有, false 代表無</returns>
    public bool HasBannerView()
    {
        if (!CheckInit())
        {
            Debug.LogError("AdFactory is not Init");
            return false;
        }
#if UNITY_EDITOR
        return false;
#else
        return mainAdManager.HasBannerView();
#endif
    }

    /// <summary>
    /// 移除目前畫面上的橫幅顯示
    /// </summary>
    /// <returns>true 代表移除成功, false 代表移除失敗或該廣告提供者的橫幅無法移除</returns>
    public bool RemoveBannerView()
    {
        if (!CheckInit())
        {
            Debug.LogError("AdFactory is not Init");
            return false;

        }
#if UNITY_EDITOR
        return false;
#else
       return mainAdManager.RemoveBannerView();
#endif

    }

    /// <summary>
    /// 顯示一則插業廣告
    /// </summary>
    /// <returns>一個代表廣告顯示進程的 Coroutine</returns>
    public Coroutine ShowInterstitialAds(Action<AdFactory.RewardResult> OnFinish, string placement = "", string analysicData = "")
    {
        OnAdRequestToShow?.Invoke(AdType.Interstitial, analysicData, placement);
        return StartCoroutine(ShowInterstitialAdsRunner(OnFinish, placement, analysicData));
    }

    IEnumerator ShowInterstitialAdsRunner(Action<AdFactory.RewardResult> OnFinish, string placement, string analysicData)
    {
        lastIntertistialAdAnalysicData = analysicData;
        //顯示讀取，如果有的話
        OnBeforeAdShow?.Invoke();
        yield return new WaitForSecondsRealtime(FixedLoadingTime);
        AdFactory.RewardResult result = AdFactory.RewardResult.Faild;
#if UNITY_EDITOR
        result = EditorTestResult;
#else
        if (CheckInit() && IsInternetAvaliable)
        {
            var currentAdManager = mainAdManager;
            if (!mainAdManager.IsInterstitialAdsAvaliable(placement) &&
                fallbackAdManager != null &&
                fallbackHandle == FallbackHandle.AlwaysFallbackIfPossiable &&
                fallbackAdManager.IsInterstitialAdsAvaliable(placement) )
            {
                currentAdManager = fallbackAdManager;
                // preload the ad for the next show
                // mainAdManager.PreLoadInterstitialAds(placement);
            }
            yield return currentAdManager.ShowInterstitialAds(placement,(r)=>{
                result = r;
            });
        }
        else
        {
            yield return new WaitForSecondsRealtime(FixedLoadingTime);
           Debug.Log("Video is not ready please check your network or try again later.");
        }
#endif
        OnFinish?.Invoke(result);
        //關閉讀取，如果有的話
        OnAfterAdShow?.Invoke();
        OnAdResult?.Invoke(AdType.Interstitial, result, analysicData, placement);
    }
    string lastIntertistialAdAnalysicData;
    public void DoOnIntertistialAdClick(string placement)
    {
        OnAdClick?.Invoke(AdType.Interstitial, lastIntertistialAdAnalysicData, placement);
    }
    public void DoOnIntertistialAdImpress(string placement)
    {
        OnAdImpress?.Invoke(AdType.Interstitial, lastIntertistialAdAnalysicData, placement);
    }
    public bool IsInterstitialAdsAvaliable(string placement, AdManagerType adManagerType = AdManagerType.Main)
    {

#if UNITY_EDITOR
        return IsInterstitialAvaliable;
#else
        var currentAdManager = mainAdManager;
        if (!mainAdManager.IsInterstitialAdsAvaliable(placement) &&
            fallbackAdManager != null &&
            adManagerType == AdManagerType.Fallback)
        {
            currentAdManager = fallbackAdManager;
        }
        return currentAdManager.IsInterstitialAdsAvaliable(placement);
#endif
    }

    /// <summary>
    /// 顯示一則獎勵廣告
    /// </summary>
    /// <returns>一個代表廣告顯示進程的 Coroutine</returns>
    public Coroutine ShowRewardedAds(Action<AdFactory.RewardResult> OnFinish, string placement = "", string analysicData = "")
    {
        OnAdRequestToShow?.Invoke(AdType.Reward, analysicData, placement);
        return StartCoroutine(ShowRewardedAdsRunner(OnFinish, placement, analysicData));
    }

    IEnumerator ShowRewardedAdsRunner(Action<AdFactory.RewardResult> OnFinish, string placement, string analysicData)
    {
        lastRewardAdAnalysicData = analysicData;
        //顯示讀取，如果有的話
        OnBeforeAdShow?.Invoke();


        yield return new WaitForSecondsRealtime(FixedLoadingTime);
        AdFactory.RewardResult result = AdFactory.RewardResult.Faild;
#if UNITY_EDITOR
        result = EditorTestResult;
#else
       
        if (CheckInit() && IsInternetAvaliable)
        {
            var currentAdManager = mainAdManager;
            if (!mainAdManager.IsRewardViedoAvaliable(placement) &&
                fallbackAdManager != null &&
                fallbackHandle == FallbackHandle.AlwaysFallbackIfPossiable&&
                fallbackAdManager.IsRewardViedoAvaliable(placement))
            {
                currentAdManager = fallbackAdManager;
                // preload the ads for the next show
                // mainAdManager.PreLoadRewardedAd(new string[] { placement });
            }
            yield return currentAdManager.ShowRewardedAds(placement,(r)=>{
                result = r;
            });
        }
        else
        {
            yield return new WaitForSecondsRealtime(FixedLoadingTime);
           Debug.Log("Video is not ready please check your network or try again later.");
        }
#endif
        OnFinish?.Invoke(result);
        //關閉讀取，如果有的話
        OnAfterAdShow?.Invoke();
        OnAdResult?.Invoke(AdType.Reward, result, analysicData, placement);
    }
    string lastRewardAdAnalysicData;
    public void DoOnRewardAdClick(string placement)
    {
        OnAdClick?.Invoke(AdType.Reward, lastRewardAdAnalysicData, placement);
    }
    public void DoOnRewardAdImpress(string placement)
    {
        OnAdImpress?.Invoke(AdType.Reward, lastRewardAdAnalysicData, placement);
    }

    public bool IsRewardViedoAvaliabale(string placement = "", System.Action<bool> OnAdLoaded = null, AdManagerType adManagerType = AdManagerType.Main)
    {

#if UNITY_EDITOR
        StartCoroutine(EditorIsRewardVideoAvaliabale(OnAdLoaded));
        return IsRewardViedoAvaliableDirectResult;
#else
        var currentAdManager = mainAdManager;
        if (!mainAdManager.IsRewardViedoAvaliable(placement) &&
            fallbackAdManager != null &&
            adManagerType == AdManagerType.Fallback)
        {
            currentAdManager = fallbackAdManager;
        }
        return currentAdManager.IsRewardViedoAvaliable(placement);
#endif
    }
    IEnumerator EditorIsRewardVideoAvaliabale(System.Action<bool> OnAdLoaded)
    {
        yield return new WaitForSecondsRealtime(IsRewardViedoAvaliableLoadedTime);
        OnAdLoaded?.Invoke(IsRewardViedoAvaliableLoadedResult);
    }


    public bool CheckInit()
    {

#if UNITY_EDITOR
        return true;
#endif
        return mainAdManager != null;
    }


    public enum AdType
    {
        None,
        Banner,
        Reward,
        Interstitial
    }
    public enum RewardResult
    {
        Success,
        Declined,
        Faild,
        Error
    }

    public enum AdsLoadState
    {
        Loading,
        Loaded,
        Failed,
        Rewarded,
        RewardSuccess,
        Declined,
        Exception,
        Complete,
    }
    void OnApplicationPause(bool isPaused)
    {
        mainAdManager?.OnApplicationPause(isPaused);
    }
    public enum FallbackHandle
    {
        DontFallback,
        AlwaysFallbackIfPossiable,
    }
    public enum AdManagerType
    {
        Main,
        Fallback
    }
}

public interface IAdManager
{
    void Init();
    void Destroy();
    void OnApplicationPause(bool isPaused);

    /// <summary>
    /// Show the banner ad
    /// </summary>
    /// <returns>true 代表請求成功, false 代表請求失敗或是 VIP 用戶或是還沒玩超過三次</returns>
    bool ShowBannerAd(string placement);
    bool LoadBannerAd();
    bool HasBannerView();
    bool RemoveBannerView();
    int GetBannerHeight();

    /// <summary>
    /// 顯示一則插業廣告
    /// </summary>
    /// <returns>一個代表廣告顯示進程的 Coroutine</returns>
    IEnumerator ShowInterstitialAds(string placement, Action<AdFactory.RewardResult> OnFinish);
    void PreLoadInterstitialAds(string placements);
    bool IsInterstitialAdsAvaliable(string placement);

    /// <summary>
    /// 顯示一則獎勵廣告
    /// </summary>
    /// <returns>一個代表廣告顯示進程的 Coroutine</returns>
    IEnumerator ShowRewardedAds(string placement, Action<AdFactory.RewardResult> OnFinish);

    void PreLoadRewardedAd(string placements);
    bool IsRewardViedoAvaliable(string placement);
}


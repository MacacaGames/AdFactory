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
    IAdManager adManager;
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
    public delegate void AdViewEventAnalysic(string Data);
    /// <summary>
    /// 註冊一個事件，該事件將會於 廣告顯示「前」執行
    /// </summary>
    public event AdViewEventAnalysic OnAdAnalysic;

    /// <summary>
    /// 註冊一個事件，該事件將會於 廣告顯示「前」執行
    /// </summary>
    public Action OnBeforeAdShow;

    /// <summary>
    /// 註冊一個事件，該事件將會於 廣告顯示「後」執行
    /// </summary>
    public Action OnAfterAdShow;
    public Action<AdType, RewardResult, string> OnAdResult;


    public void Init(IAdManager provider)
    {
        if (CheckInit())
        {
            Debug.LogError("AdFactory is Inited Return");
            return;
        }

        if (provider == null)
        {
            Debug.LogError("AdFactory provider is null, Return");
            return;
        }

        Debug.LogWarning("Init AdFactory with " + provider);

        adManager = provider;
        adManager.Init();
    }


    public void PreLoadRewardedAd(string[] placements)
    {
#if UNITY_EDITOR
        //Do nothing in Editor
#else
        adManager.PreLoadRewardedAd(placements);
#endif
    }
    public void PreLoadInterstitialAds(string placements)
    {
#if UNITY_EDITOR
        //Do nothing in Editor
#else
        adManager.PreLoadInterstitialAds(placements);
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

        return adManager.ShowBannerAd(placement);
    }

    public int GetBannerHeight()
    {
        if (!CheckInit())
        {
            Debug.LogError("AdFactory is not Init");
            return 0;
        }

        return adManager.GetBannerHeight();
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

        return adManager.HasBannerView();
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
        return adManager.RemoveBannerView();
    }

    /// <summary>
    /// 顯示一則插業廣告
    /// </summary>
    /// <returns>一個代表廣告顯示進程的 Coroutine</returns>
    public Coroutine ShowInterstitialAds(Action<AdFactory.RewardResult> OnFinish, string placement = "")
    {
        return StartCoroutine(ShowInterstitialAdsRunner(OnFinish, placement));
    }

    IEnumerator ShowInterstitialAdsRunner(Action<AdFactory.RewardResult> OnFinish, string placement)
    {
        //顯示讀取，如果有的話
        OnBeforeAdShow?.Invoke();
        yield return new WaitForSecondsRealtime(1f);
        AdFactory.RewardResult result = AdFactory.RewardResult.Faild;
#if UNITY_EDITOR
        result = EditorTestResult;
#else
        if (CheckInit() && IsInternetAvaliable)
        {
            yield return adManager.ShowInterstitialAds(placement,(r)=>{
                result = r;
            });
        }
        else
        {
            yield return new WaitForSecondsRealtime(1.5f);
           Debug.Log("Video is not ready please check your network or try again later.");
        }
#endif
        OnFinish?.Invoke(result);
        //關閉讀取，如果有的話
        OnAfterAdShow?.Invoke();
        OnAdResult?.Invoke(AdType.Interstitial, result, placement);
    }
    public bool IsInterstitialAdsAvaliable(string placement)
    {
#if UNITY_EDITOR
        return IsInterstitialAvaliable;
#else

        return adManager.IsInterstitialAdsAvaliable(placement);
#endif
    }
    /// <summary>
    /// 顯示一則獎勵廣告
    /// </summary>
    /// <returns>一個代表廣告顯示進程的 Coroutine</returns>
    public Coroutine ShowRewardedAds(Action<AdFactory.RewardResult> OnFinish, string placement = "", string analysicData = "")
    {
        OnAdAnalysic?.Invoke(analysicData);
        return StartCoroutine(ShowRewardedAdsRunner(OnFinish, placement));
    }

    IEnumerator ShowRewardedAdsRunner(Action<AdFactory.RewardResult> OnFinish, string placement)
    {
        //顯示讀取，如果有的話
        OnBeforeAdShow?.Invoke();
        yield return new WaitForSecondsRealtime(1f);
        AdFactory.RewardResult result = AdFactory.RewardResult.Faild;
#if UNITY_EDITOR
        result = EditorTestResult;
#else
        if (CheckInit() && IsInternetAvaliable)
        {
            yield return adManager.ShowRewardedAds(placement,(r)=>{
                result = r;
            });
        }
        else
        {
            yield return new WaitForSecondsRealtime(1f);
           Debug.Log("Video is not ready please check your network or try again later.");
        }
#endif
        OnFinish?.Invoke(result);
        //關閉讀取，如果有的話
        OnAfterAdShow?.Invoke();
        OnAdResult?.Invoke(AdType.Reward, result, placement);
    }

    public bool IsRewardViedoAvaliabale(string placement = "", System.Action<bool> OnAdLoaded = null)
    {
#if UNITY_EDITOR
        StartCoroutine(EditorIsRewardVideoAvaliabale(OnAdLoaded));
        return IsRewardViedoAvaliableDirectResult;
#else
        return adManager.IsRewardViedoAvaliable(placement, OnAdLoaded);
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
        return adManager != null;
    }

    public enum AdProvider
    {
        AdMob = 0,
        UnityAd = 1,
        FacebookAudienceNetwork = 2,
        IronSource = 3
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
        adManager?.OnApplicationPause(isPaused);
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

    void PreLoadRewardedAd(string[] placements);
    bool IsRewardViedoAvaliable(string placement, System.Action<bool> OnAdLoaded);
}


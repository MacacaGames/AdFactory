#if AdFactory_IronSource
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class IronSourceManager : IAdManager
{
    static string _appKey;
    static string _defaultRewaredPlacement;
    static string _defaultIterstitialPlacement;
    static string _defaultBannerPlacement;

    public IronSourceManager(string AppId, string DefaultRewaredPlacement, string DefaultIterstitialPlacement, string DefaultBannerPlacement)
    {
        _appKey = AppId;
        _defaultRewaredPlacement = DefaultRewaredPlacement;
        _defaultIterstitialPlacement = DefaultIterstitialPlacement;
        _defaultBannerPlacement = DefaultBannerPlacement;
    }
    public void Init()
    {
        Debug.Log("unity-script: IronSource.Agent.validateIntegration");
        IronSource.Agent.validateIntegration();

        Debug.Log("unity-script: unity version" + IronSource.unityVersion());

        // SDK init
        Debug.Log("unity-script: IronSource.Agent.init");
        IronSource.Agent.init(_appKey);
        #region Interstitial
        IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
        IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
        IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
        IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
        IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
        #endregion

        #region Rewarded
        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
        IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;

        #endregion
    }

    public void Destroy()
    {

    }

    public void OnApplicationPause(bool isPaused)
    {
        Debug.Log("unity-script: OnApplicationPause = " + isPaused);
        IronSource.Agent.onApplicationPause(isPaused);
    }


    #region BannerAd
    public bool ShowBannerAd(string placement)
    {
        Debug.Log("IronSource doesn't support Bannse");
        return false;
    }
    public int GetBannerHeight()
    {
        return 0;
    }
    public bool HasBannerView()
    {
        return false;
    }
    public bool RemoveBannerView()
    {

        return false;
    }
    #endregion
    #region InterstitialAd

    bool isShowedInterstitialAds;
    public AdFactory.AdsLoadState loadState_interstitialAds = AdFactory.AdsLoadState.Exception;
    bool isInterstitialAdClose = false;

    public IEnumerator ShowInterstitialAds(string placement, Action<AdFactory.RewardResult> OnComplete)
    {
        string id = _defaultIterstitialPlacement;
        AdFactory.RewardResult result = AdFactory.RewardResult.Error;

        isInterstitialAdClose = false;
        int try_preload_times = 0;

        //等一秒，騙使用者很忙
        yield return new WaitForSecondsRealtime(1f);

        //沒有讀到的情況
        if (loadState_interstitialAds != AdFactory.AdsLoadState.Loaded)
        {
            PreloadInterstitial(id);

            float wait = 0;
            while (wait < 2)
            {
                wait += Time.deltaTime;
                if (loadState_interstitialAds == AdFactory.AdsLoadState.Loaded)
                {
                    goto SHOW;
                }
                yield return null;
            }
            try_preload_times++;
            Debug.Log("Try load times : " + try_preload_times);

            result = AdFactory.RewardResult.Faild;
            goto FINISH;
        }

    SHOW:
        if (IronSource.Agent.isInterstitialReady())
        {
            result = AdFactory.RewardResult.Success;
            _ShowInterstitialAds();
        }

        while (!isInterstitialAdClose)
        {
            yield return null;
        }

    FINISH:
        PreloadInterstitial(id);

        if (OnComplete != null)
            OnComplete(result);
    }
    public void PreLoadInterstitialAds(string placements)
    {
        PreloadInterstitial(placements);
    }
    public bool IsInterstitialAdsAvaliable(string placement)
    {
        return IronSource.Agent.isInterstitialReady();
    }
    void _ShowInterstitialAds()
    {
        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
            isInterstitialLoading = false;
            isShowedInterstitialAds = true;
        }
        else
        {
            Debug.Log("Cannot show interstitialAds, Handler is loaded, but somehow IsLoaded is still not loaded.");
        }
    }
    bool isInterstitialLoading = false;
    void PreloadInterstitial(string id)
    {
        if (isInterstitialLoading)
        {
            Debug.Log("PreloadInterstitial return due to last load is not finish");
            return;
        }
        if (IronSource.Agent.isInterstitialReady())
        {
            return;
        }
        isInterstitialLoading = true;
        // Load the interstitial with the request.
        IronSource.Agent.loadInterstitial();
    }

    void InterstitialAdReadyEvent()
    {
        loadState_interstitialAds = AdFactory.AdsLoadState.Loaded;
        isInterstitialLoading = false;
        Debug.Log("unity-script: I got InterstitialAdReadyEvent");
    }

    void InterstitialAdLoadFailedEvent(IronSourceError error)
    {
        isInterstitialLoading = false;
        Debug.Log("unity-script: I got InterstitialAdLoadFailedEvent, code: " + error.getCode() + ", description : " + error.getDescription());
    }

    void InterstitialAdShowSucceededEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdShowSucceededEvent");
    }

    void InterstitialAdShowFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got InterstitialAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
    }

    void InterstitialAdClickedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdClickedEvent");
    }

    void InterstitialAdOpenedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdOpenedEvent");
    }

    void InterstitialAdClosedEvent()
    {
        isInterstitialAdClose = true;
        Debug.Log("unity-script: I got InterstitialAdClosedEvent");
    }

    #endregion
    #region RewardedAd

    System.Action<bool> OnAdLoaded;
    public bool IsRewardViedoAvaliable(string placement, System.Action<bool> OnAdLoaded)
    {
        if (!AdFactory.IsInternetAvaliable)
        {
            OnAdLoaded?.Invoke(false);
            return false;
        }

        if (string.IsNullOrEmpty(placement))
        {
            placement = _defaultRewaredPlacement;
        }
        this.OnAdLoaded = OnAdLoaded;
        return IronSource.Agent.isRewardedVideoAvailable();
    }

    void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
    {
        Debug.Log("unity-script: I got RewardedVideoAvailabilityChangedEvent, value = " + canShowAd);
        OnAdLoaded?.Invoke(canShowAd);
    }

    void RewardedVideoAdOpenedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdOpenedEvent");
    }

    void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
    {
        Debug.Log("unity-script: I got RewardedVideoAdRewardedEvent, amount = " + ssp.getRewardAmount() + " name = " + ssp.getRewardName());
        isRewarded = true;
    }

    void RewardedVideoAdClosedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdClosedEvent");
        isRewardAdClose = true;
    }

    void RewardedVideoAdStartedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdStartedEvent");
    }

    void RewardedVideoAdEndedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdEndedEvent");
    }

    void RewardedVideoAdShowFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got RewardedVideoAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
    }

    void RewardedVideoAdClickedEvent(IronSourcePlacement ssp)
    {
        Debug.Log("unity-script: I got RewardedVideoAdClickedEvent, name = " + ssp.getRewardName());
    }

    bool isRewardAdClose = false;
    bool isRewarded = false;
    public IEnumerator ShowRewardedAds(string placement, Action<AdFactory.RewardResult> OnFinish)
    {
        AdFactory.RewardResult result = AdFactory.RewardResult.Error;
        int try_preload_times = 0;
        isRewardAdClose = false;
        isRewarded = false;

        if (string.IsNullOrEmpty(placement))
        {
            placement = _defaultRewaredPlacement;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        //沒有讀到的情況
        if (!IronSource.Agent.isRewardedVideoAvailable())
        {
            while (try_preload_times < 3)
            {
                float wait = 0;
                while (wait < 1.5f)
                {
                    wait += Time.deltaTime;
                    if (IronSource.Agent.isRewardedVideoAvailable())
                    {
                        goto SHOW;
                    }
                    yield return null;
                }
                try_preload_times++;
                Debug.Log("Try load times : " + try_preload_times);
            }
            result = AdFactory.RewardResult.Faild;
            goto FINISH;
        }

    SHOW:
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo(placement);
        }
        else
        {
            result = AdFactory.RewardResult.Faild;
            goto FINISH;
        }

        while (isRewardAdClose == false)
        {
            yield return null;
        }

        if (isRewarded)
        {
            result = AdFactory.RewardResult.Success;
        }
        else
        {
            result = AdFactory.RewardResult.Declined;
        }

    FINISH:
        OnFinish?.Invoke(result);
    }

    public void PreLoadRewardedAd(string[] placements)
    {
        // foreach (var item in placements)
        // {
        //     CreateAndLoadRewardedAd(item);
        // }
    }

    #endregion
}

#endif
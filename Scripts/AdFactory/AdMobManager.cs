#if AdFactory_Admob
using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

using System.Linq;
public class AdMobManager : IAdManager
{
    static string _admobAppId;
    static string _defaultRewaredPlacement;
    static string _defaultIterstitialPlacement;
    static string _defaultBannerPlacement;

    public AdMobManager(string AppId, string DefaultRewaredPlacement, string DefaultIterstitialPlacement, string DefaultBannerPlacement)
    {
        _admobAppId = AppId;
        _defaultRewaredPlacement = DefaultRewaredPlacement;
        _defaultIterstitialPlacement = DefaultIterstitialPlacement;
        _defaultBannerPlacement = DefaultBannerPlacement;
    }
    public void Init()
    {
        GoogleMobileAds.Api.MobileAds.Initialize(initStatus => { });
    }

    public void Destroy()
    {

    }
    #region BannerAd
    BannerView bannerView;
    public bool ShowBannerAd(string placement)
    {
        string adUnitId = _defaultBannerPlacement;
        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
        return true;
    }
    public int GetBannerHeight()
    {
#if UNITY_IOS
        if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX)
        {
            return 50 * Mathf.RoundToInt(Screen.dpi / 160);
        }
#endif
        Debug.Log(Screen.height);
        if (Screen.height <= 400 * Mathf.RoundToInt(Screen.dpi / 160))
        {
            return 32 * Mathf.RoundToInt(Screen.dpi / 160);
        }
        else if (Screen.height <= 720 * Mathf.RoundToInt(Screen.dpi / 160))
        {
            return 50 * Mathf.RoundToInt(Screen.dpi / 160);
        }
        else
        {
            return 90 * Mathf.RoundToInt(Screen.dpi / 160);
        }
    }
    public bool HasBannerView()
    {
        return bannerView == null ? false : true;
    }
    public bool RemoveBannerView()
    {
        if (bannerView == null) return false;
        bannerView.Hide();
        return true;
    }
    #endregion
    #region InterstitialAd
    public AdFactory.AdsLoadState loadState_interstitialAds = AdFactory.AdsLoadState.Exception;
    bool isInterstitialAdClose = false;
    InterstitialAd interstitial;
    bool isShowedInterstitialAds;

    public IEnumerator ShowInterstitialAds(string placement, Action<AdFactory.RewardResult> callback)
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
            while (try_preload_times < 2)
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
            }
            result = AdFactory.RewardResult.Faild;
            goto FINISH;
        }

    SHOW:
        if (interstitial != null)
        {
            if (interstitial.IsLoaded())
            {
                result = AdFactory.RewardResult.Success;
                _ShowInterstitialAds();
            }
        }

        while (!isInterstitialAdClose)
        {
            yield return null;
        }

    FINISH:
        PreloadInterstitial(id);

        DestroyInterstitial();

        if (callback != null)
            callback(result);
    }

    void _ShowInterstitialAds()
    {
        if (interstitial.IsLoaded())
        {
            interstitial.Show();
            isShowedInterstitialAds = true;
        }
        else
        {
            Debug.Log("Cannot show interstitialAds, Handler is loaded, but somehow IsLoaded is still not loaded.");
        }
    }
    void PreloadInterstitial(string id)
    {
        DestroyInterstitial();
        interstitial = new InterstitialAd(id);

        interstitial.OnAdLoaded += HandleOnInterstitialLoaded;
        interstitial.OnAdFailedToLoad += HandleOnInterstitialFailedToLoad;
        interstitial.OnAdClosed += HandleOnInterstitialClosed;
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        interstitial.LoadAd(request);
    }
    void HandleOnInterstitialLoaded(object sender, EventArgs args)
    {
        // Handle the ad loaded event.
        loadState_interstitialAds = AdFactory.AdsLoadState.Loaded;
    }

    void HandleOnInterstitialFailedToLoad(object sender, EventArgs args)
    {
        loadState_interstitialAds = AdFactory.AdsLoadState.Failed;
    }

    void HandleOnInterstitialClosed(object sender, EventArgs args)
    {
        isInterstitialAdClose = true;
        loadState_interstitialAds = AdFactory.AdsLoadState.Complete;
    }

    void DestroyInterstitial()
    {
        if (interstitial == null)
        {
            return;
        }
        interstitial.OnAdLoaded -= HandleOnInterstitialLoaded;
        interstitial.OnAdFailedToLoad -= HandleOnInterstitialFailedToLoad;
        interstitial.OnAdClosed -= HandleOnInterstitialClosed;
        interstitial.Destroy();
    }



    #endregion
    #region RewardedAd

    static Dictionary<string, RewardedAd> rewardAdDict = new Dictionary<string, RewardedAd>();
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
        RewardedAd rewardedAd = null;
        if (!rewardAdDict.TryGetValue(placement, out rewardedAd))
        {
            rewardedAd = CreateAndLoadRewardedAd(placement);
        }

        rewardedAd.OnAdLoaded -= (object sender, EventArgs e) =>
        {
            CheckingLoadSuccess(rewardedAd, OnAdLoaded);
        };

        rewardedAd.OnAdLoaded += (object sender, EventArgs e) =>
        {
            CheckingLoadSuccess(rewardedAd, OnAdLoaded);
        };

        rewardedAd.OnAdFailedToLoad -= (object sender, AdErrorEventArgs e) =>
        {
            CheckingLoadFaild(rewardedAd, OnAdLoaded);
        };

        rewardedAd.OnAdFailedToLoad += (object sender, AdErrorEventArgs e) =>
        {
            CheckingLoadFaild(rewardedAd, OnAdLoaded);
        };

        return rewardedAd.IsLoaded();
    }
    void CheckingLoadSuccess(RewardedAd rewardedAd, System.Action<bool> OnAdLoaded)
    {
        OnAdLoaded?.Invoke(true);
    }

    void CheckingLoadFaild(RewardedAd rewardedAd, System.Action<bool> OnAdLoaded)
    {
        OnAdLoaded?.Invoke(false);
        RemoveRewardAdByValue(rewardedAd);
    }

    public RewardedAd CreateAndLoadRewardedAd(string placement)
    {
        var rewardedAd = new RewardedAd(placement);
        // Called when an ad request has successfully loaded.
        rewardedAd.OnAdLoaded += OnAdLoaded;
        // Called when an ad request failed to load.
        rewardedAd.OnAdFailedToLoad += OnAdFailedToLoad;
        // Called when an ad is shown.
        rewardedAd.OnAdOpening += OnAdOpening;
        // Called when an ad request failed to show.
        rewardedAd.OnAdFailedToShow += OnAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        rewardedAd.OnUserEarnedReward += OnUserEarnedReward;
        // Called when the ad is closed.
        rewardedAd.OnAdClosed += OnAdClosed;

        if (rewardAdDict.ContainsKey(placement))
        {
            rewardAdDict[placement] = rewardedAd;
        }
        else
        {
            rewardAdDict.Add(placement, rewardedAd);
        }

        AdRequest request = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(request);
        return rewardedAd;
    }

    private void OnAdLoaded(object sender, EventArgs e)
    {
        MonoBehaviour.print("OnAdLoaded event received");
    }

    private void OnAdFailedToLoad(object sender, AdErrorEventArgs e)
    {
        MonoBehaviour.print("OnAdFailedToLoad event received ,msg : {e.Message}");
        RemoveRewardAdByValue(sender as RewardedAd);
    }

    void RemoveRewardAdByValue(RewardedAd rewardedAd)
    {
        if (rewardedAd != null)
        {
            foreach (var item in rewardAdDict.Where(kvp => kvp.Value == rewardedAd).ToList())
            {
                rewardAdDict.Remove(item.Key);
            }
        }
    }

    private void OnAdOpening(object sender, EventArgs e)
    {
        MonoBehaviour.print("OnAdOpening event received");
    }

    private void OnAdFailedToShow(object sender, AdErrorEventArgs e)
    {
        MonoBehaviour.print($"OnAdFailedToShow event received ,msg : {e.Message}");
    }

    private void OnUserEarnedReward(object sender, Reward e)
    {
        MonoBehaviour.print("OnUserEarnedReward event received");
        isRewarded = true;
    }

    private void OnAdClosed(object sender, EventArgs e)
    {
        MonoBehaviour.print("OnAdClosed event received");
        isRewardAdClose = true;
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

        rewardAdDict.TryGetValue(placement, out RewardedAd rewardedAd);

        if (rewardedAd == null)
        {
            rewardedAd = CreateAndLoadRewardedAd(placement);
        }

        yield return new WaitForSecondsRealtime(0.2f);

        //沒有讀到的情況
        if (!rewardedAd.IsLoaded())
        {
            while (try_preload_times < 3)
            {
                float wait = 0;
                while (wait < 1.5f)
                {
                    wait += Time.deltaTime;
                    if (rewardedAd.IsLoaded())
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
        if (rewardedAd.IsLoaded())
        {
            rewardedAd.Show();
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

        //Unbind Event
        rewardedAd.OnAdLoaded -= OnAdLoaded;
        rewardedAd.OnAdFailedToLoad -= OnAdFailedToLoad;
        rewardedAd.OnAdOpening -= OnAdOpening;
        rewardedAd.OnAdFailedToShow -= OnAdFailedToShow;
        rewardedAd.OnUserEarnedReward -= OnUserEarnedReward;
        rewardedAd.OnAdClosed -= OnAdClosed;
        CreateAndLoadRewardedAd(placement);

    FINISH:
        OnFinish?.Invoke(result);
    }

    public void PreLoadRewardedAd(string[] placements)
    {
        foreach (var item in placements)
        {
            CreateAndLoadRewardedAd(item);
        }
    }

    #endregion
}

#endif
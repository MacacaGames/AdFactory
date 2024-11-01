using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

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
    public void OnApplicationPause(bool isPaused)
    {

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
        AdRequest request = new AdRequest();
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

    public bool LoadBannerAd()
    {
        return false;
    }

    #endregion

    #region InterstitialAd

    public AdFactory.AdsLoadState loadState_interstitialAds = AdFactory.AdsLoadState.Exception;
    bool isInterstitialAdClose = false;
    private InterstitialAd interstitialAd;
    bool isShowedInterstitialAds;

    public IEnumerator ShowInterstitialAds(string placement, Action<AdFactory.RewardResult> callback)
    {
        string id = _defaultIterstitialPlacement;
        AdFactory.RewardResult result = AdFactory.RewardResult.Error;
        isInterstitialAdClose = false;
        int try_preload_times = 0;

        yield return new WaitForSecondsRealtime(1f);

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
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            result = AdFactory.RewardResult.Success;
            interstitialAd.Show();
            isShowedInterstitialAds = true;
        }

        while (!isInterstitialAdClose)
        {
            yield return null;
        }

        FINISH:
        PreloadInterstitial(id);
        callback?.Invoke(result);
    }
    public void PreLoadInterstitialAds(string placements)
    {
        PreloadInterstitial(placements);
    }
    public bool IsInterstitialAdsAvaliable(string placement)
    {
        return interstitialAd != null && interstitialAd.CanShowAd();
    }

    void PreloadInterstitial(string id)
    {
        DestroyInterstitial();

        InterstitialAd.Load(id, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError("Interstitial ad failed to load: " + error);
                loadState_interstitialAds = AdFactory.AdsLoadState.Failed;
                return;
            }

            interstitialAd = ad;
            loadState_interstitialAds = AdFactory.AdsLoadState.Loaded;
            RegisterInterstitialEvents(ad);
        });
    }

    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            // AnalyticsManager.LogAdsRevenue("AdMob", ad.GetAdUnitID(), "Interstitial", adValue);
            Debug.Log($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}");
        };
        ad.OnAdClicked += () =>
            Debug.Log("Interstitial ad clicked.");
        ad.OnAdImpressionRecorded += () =>
            Debug.Log("Interstitial ad impression recorded.");
        ad.OnAdFullScreenContentOpened += () =>
            Debug.Log("Interstitial ad full screen content opened.");
        ad.OnAdFullScreenContentClosed += () =>
        {
            isInterstitialAdClose = true; loadState_interstitialAds = AdFactory.AdsLoadState.Complete;
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
            Debug.LogError("Interstitial ad failed to show: " + error);
    }

    void DestroyInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
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

        if (!rewardAdDict.TryGetValue(placement, out RewardedAd rewardedAd))
        {
            rewardedAd = CreateAndLoadRewardedAd(placement);
        }

        return rewardedAd.CanShowAd();
    }

    bool isRewardAdClose = false;
    bool isRewarded = false;

    public RewardedAd CreateAndLoadRewardedAd(string placement)
    {
        RewardedAd loadRewardedAd = null;

        if (rewardAdDict.TryGetValue(placement, out var existingAd))
        {
            rewardAdDict.Remove(placement);
        }

        RewardedAd.Load(placement, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError("Rewarded ad failed to load with error: " + error);
                return;
            }

            rewardAdDict[placement] = ad;
            RegisterRewardedAdEvents(ad);
            loadRewardedAd = ad;
        });

        return loadRewardedAd;
    }

    private void RegisterRewardedAdEvents(RewardedAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            // AnalyticsManager.LogAdsRevenue("AdMob", ad.GetAdUnitID(), "Rewarded", adValue);
            Debug.Log($"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}");
        };
        ad.OnAdClicked += () => Debug.Log("Rewarded ad clicked.");
        ad.OnAdFullScreenContentOpened += () => Debug.Log("Rewarded ad full screen content opened.");
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad closed.");
            isRewardAdClose = true;
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to show with error: " + error);
        };
    }

    public IEnumerator ShowRewardedAds(string placement, Action<AdFactory.RewardResult> OnFinish)
    {
        AdFactory.RewardResult result = AdFactory.RewardResult.Error;
        isRewardAdClose = false;
        isRewarded = false;

        if (rewardAdDict.TryGetValue(placement, out RewardedAd rewardedAd) && rewardedAd.CanShowAd())
        {
            rewardedAd.SetServerSideVerificationOptions(options);
            rewardedAd.Show(reward =>
            {
                Debug.Log($"Rewarded ad granted reward: {reward.Amount} {reward.Type}");
                isRewarded = true;
            });
        }
        else
        {
            result = AdFactory.RewardResult.Faild;
            OnFinish?.Invoke(result);
            yield break;
        }

        while (!isRewardAdClose)
        {
            yield return null;
        }

        result = isRewarded ? AdFactory.RewardResult.Success : AdFactory.RewardResult.Declined;
        OnFinish?.Invoke(result);
    }

    static GoogleMobileAds.Api.ServerSideVerificationOptions options;
    public static void SetCustomData(GoogleMobileAds.Api.ServerSideVerificationOptions _options)
    {
        options = _options;
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


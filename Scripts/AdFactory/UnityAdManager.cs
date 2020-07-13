#if AdFactory_Unity
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdManager : IAdManager, IUnityAdsListener
{
    static string _gameId = "";
    static string _defaultRewaredPlacement;
    static string _defaultIterstitialPlacement;
    static string _defaultBannerPlacement;
    List<string> rewardPlacement = new List<string>();
    List<string> iterstitialPlacament = new List<string>();
    public UnityAdManager(string GameId, string DefaultRewaredPlacement, string DefaultIterstitialPlacement, string DefaultBannerPlacement)
    {
        _gameId = GameId;
        _defaultRewaredPlacement = DefaultRewaredPlacement;
        _defaultIterstitialPlacement = DefaultIterstitialPlacement;
        _defaultBannerPlacement = DefaultBannerPlacement;
    }
    public void Init()
    {
        Advertisement.Initialize(_gameId);
        Advertisement.AddListener(this);
    }
 public void OnApplicationPause(bool isPaused)
    {

    }
    /// <summary>
    /// Add two number
    /// </summary>
    /// <returns>true 代表請求成功, false 代表請求失敗或是 VIP 用戶或是還沒玩超過三次</returns>
    public bool ShowBannerAd(string placement)
    {
        string id = "";
        if (string.IsNullOrEmpty(placement))
        {
            id = _defaultBannerPlacement;
        }
        else
        {
            id = placement;
        }

        Advertisement.Banner.Show(id);
        return Advertisement.IsReady(id);
    }
    public bool HasBannerView()
    {

        return false;
    }
    public bool RemoveBannerView()
    {
        Advertisement.Banner.Hide();
        return false;
    }
    public int GetBannerHeight()
    {
        return 0;
    }

    AdFactory.RewardResult resultInterstitialAd = AdFactory.RewardResult.Faild;
    bool waitInterstitialAdFinish = false;
    public IEnumerator ShowInterstitialAds(string placement, Action<AdFactory.RewardResult> OnFinish)
    {
        string id = "";
        if (string.IsNullOrEmpty(placement))
        {
            id = _defaultIterstitialPlacement;
        }
        else
        {
            if (!rewardPlacement.Contains(placement))
            {
                rewardPlacement.Add(placement);
            }
            id = placement;
        }

        waitInterstitialAdFinish = false;
        if (Advertisement.IsReady(id))
        {
            Advertisement.Show(id);
        }
        else
        {
            resultInterstitialAd = AdFactory.RewardResult.Faild;
            waitInterstitialAdFinish = true;
        }

        yield return new WaitUntil(() => waitInterstitialAdFinish == true);
        OnFinish(resultInterstitialAd);
    }


    bool waitRewardedAdFinish = false;
    AdFactory.RewardResult resultRewardAd = AdFactory.RewardResult.Faild;
    /// <summary>
    /// 顯示一則獎勵廣告
    /// </summary>
    /// <returns>一個代表廣告顯示進程的 Coroutine</returns>
    public IEnumerator ShowRewardedAds(string placement, Action<AdFactory.RewardResult> OnFinish)
    {
        string id = "";
        if (string.IsNullOrEmpty(placement))
        {
            id = _defaultRewaredPlacement;
        }
        else
        {
            if (!iterstitialPlacament.Contains(placement))
            {
                iterstitialPlacament.Add(placement);
            }
            id = placement;
        }
        waitRewardedAdFinish = false;
        if (Advertisement.IsReady(id))
        {
            Advertisement.Show(id);
        }
        else
        {
            resultRewardAd = AdFactory.RewardResult.Faild;
            waitRewardedAdFinish = true;
        }

        yield return new WaitUntil(() => waitRewardedAdFinish == true);
        OnFinish(resultRewardAd);
    }
    public void PreLoadRewardedAd(string[] placements)
    {
        //nothinh in Unity Ads
    }

    public bool IsRewardViedoAvaliable(string placement, System.Action<bool> OnAdLoaded)
    {
        if (string.IsNullOrEmpty(placement))
        {
            placement = _defaultRewaredPlacement;
        }
        return Advertisement.IsReady(placement);
    }

    public void OnUnityAdsReady(string placementId)
    {

    }

    public void OnUnityAdsDidError(string message)
    {

    }

    public void OnUnityAdsDidStart(string placementId)
    {

    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (placementId == _defaultRewaredPlacement || rewardPlacement.Contains(placementId))
        {
            waitRewardedAdFinish = true;
        }
        else if (placementId == _defaultRewaredPlacement || iterstitialPlacament.Contains(placementId))
        {
            waitInterstitialAdFinish = true;
        }
        switch (showResult)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                resultRewardAd = AdFactory.RewardResult.Success;
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                resultRewardAd = AdFactory.RewardResult.Declined;
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                resultRewardAd = AdFactory.RewardResult.Faild;
                break;
        }
    }

    public void Destroy()
    {

    }

    public void PreLoadInterstitialAds(string placements)
    {
        //throw new NotImplementedException();
    }

    public bool IsInterstitialAdsAvaliable(string placement)
    {
        //throw new NotImplementedException();
        return Advertisement.IsReady(placement);
    }
}

#endif
#if AdFactory_Unity
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdManager : IAdManager
{
    static string _gameId = "";
    static string _defaultRewaredPlacement;
    static string _defaultIterstitialPlacement;
    static string _defaultBannerPlacement;
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
    }
    public void Destroy()
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
            id = placement;
        }

        waitInterstitialAdFinish = false;
        if (Advertisement.IsReady(id))
        {
            ShowOptions so = new ShowOptions();
            so.resultCallback = HandleShownterstitialResult;
            Advertisement.Show(id, so);
        }
        else
        {
            resultInterstitialAd = AdFactory.RewardResult.Faild;
            waitInterstitialAdFinish = true;
        }

        yield return new WaitUntil(() => waitInterstitialAdFinish == true);
        OnFinish(resultInterstitialAd);
    }
    private void HandleShownterstitialResult(ShowResult result)
    {
        Debug.Log("HandleShowResult" + result);
        waitInterstitialAdFinish = true;
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                resultInterstitialAd = AdFactory.RewardResult.Success;
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                resultInterstitialAd = AdFactory.RewardResult.Declined;
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                resultInterstitialAd = AdFactory.RewardResult.Faild;
                break;
        }
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
            id = placement;
        }
        waitRewardedAdFinish = false;
        if (Advertisement.IsReady(id))
        {
            ShowOptions so = new ShowOptions();
            so.resultCallback = HandleShowRewardResult;
            Advertisement.Show(id, so);
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
    void HandleShowRewardResult(ShowResult result)
    {
        Debug.Log("HandleShowResult" + result);
        waitRewardedAdFinish = true;
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                resultRewardAd = AdFactory.RewardResult.Success;
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
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
}

#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EmptyImplAdManager : IAdManager
{
    public void Destroy()
    {

    }

    public int GetBannerHeight()
    {
        return 0;
    }

    public bool HasBannerView()
    {
        return false;
    }

    public void Init()
    {        
    }

    public bool IsInterstitialAdsAvaliable(string placement)
    {
        return false;
    }

    public bool IsRewardViedoAvaliable(string placement, Action<bool> OnAdLoaded)
    {
        return false;
    }

    public void OnApplicationPause(bool isPaused)
    {        
    }

    public void PreLoadInterstitialAds(string placements)
    {
    }

    public void PreLoadRewardedAd(string[] placements)
    {
    }

    public bool RemoveBannerView()
    {
        return false;
    }

    public bool ShowBannerAd(string placement)
    {
        return false;
    }

    public IEnumerator ShowInterstitialAds(string placement, Action<AdFactory.RewardResult> OnFinish)
    {
        yield break;
    }

    public IEnumerator ShowRewardedAds(string placement, Action<AdFactory.RewardResult> OnFinish)
    {
        yield break;
    }
}

#if AdFactory_Admob_Native
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.AdManager;
using UnityEngine;

public class NativeAdFactory : MonoBehaviour
{
    INativeAdManager nativeAdManager;

    /// <summary>
    /// 初始化 NativeAdFactory 並指定實做的廣告供應者
    /// </summary>
    public void Init(INativeAdManager adManager, string AD_UNIT)
    {
        nativeAdManager = adManager;
        nativeAdManager.Init(AD_UNIT);
    }

    public void LoadAd(string AD_UNIT)
    {
        if (nativeAdManager != null)
        {
            nativeAdManager.LoadNativeAd(AD_UNIT);
        }
    }

    public void DestroyAd()
    {
        if (nativeAdManager != null)
        {
            nativeAdManager.Destroy();
        }
    }
}

public interface INativeAdManager
{
    void Init(string AD_UNIT);
    void LoadNativeAd(string AD_UNIT);
    void Destroy();
}
#endif

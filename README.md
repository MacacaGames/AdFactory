See [Document](http://macacagames.github.io/AdFactory/) for more detail.

# Welcome to Macaca AdFactory
AdFactory is a Mobile Ad Wrapper for Unity3D, you can easy change Ad's implemention by simply implement different IAdManager, and also makes testing Ad in Editor easilier.

## Installation

### Option 1: Unity Package manager (Recommended)
Add it to your editor's `manifest.json` file like this:
```json
{
    "dependencies": {
        "com.macacagames.adfactory": "https://github.com/MacacaGames/AdFactory.git#1.0.0",
    }
}
```
You can remove the #1.0.0 to use the latest version (unstable)


### Option 2: Git SubModule
Note: when using git submodule to import, you can use pre-build IAdManager directly without make a copy into your project folder.

```bash
git submodule add https://github.com/MacacaGames/AdFactory.git Assets/MacacaGameSystem
```

## Implement the IAdManager for your project
`Implement your IAdManager to makes AdFactory work`, or use `pre-build IAdManager`, currentlly we have implement Admob, IronSource, UnityAds.

### Use pre-build IAdManager implement
AdFactory has implement three IAdManager, to use the pre-build IAdManager just copy the IAdManager from `PackageRoot/IAdManagerImpl` folder to any folder under your project  `UnityProject/Assets/...`.
Please remember to import the third-party Ad SDK.

<table>
    <tr>
        <td>IAdManager</td>
        <td>SDK</td>
    </tr>
    <tr>
        <td>AdMobManager</td>
        <td>
        <a href="https://developers.google.com/admob/unity/quick-start">SDK</a>
        </td>
    </tr>
    <tr>
        <td>IronSourceManager</td>
         <td>
        <a href="https://developers.ironsrc.com/ironsource-mobile/unity/unity-plugin/#step-1">SDK</a>
        </td>
    </tr>
    <tr>
        <td>UnityAdManager</td>
        <td>Install the SDK via UPM</td>
    </tr>
</table>

## Usage
All sample code use the pre-build AdMobManager.cs as the example.

- Initialize the AdFactory

```csharp
string ad_admobAppId = "{your admob app id}";
string ad_admobRewarded = "{your admob reward video placement}";
string ad_admobInterstitial = "{your admob interstital placement}";
string ad_admobBannerID = "{your admob banner placement}";

AdFactory.Instance.Init(
    new AdMobManager(
        ad_admobAppId,
        ad_admobRewarded,
        ad_admobInterstitial,
        ad_admobBannerID)
);
```

- Perload ads (Recommend, not required)
```csharp
string[] ad_admobRewardeds = new string[]{
                                "{your admob reward video placement-1}",
                                "{your admob reward video placement-2}",
                                .....};
string ad_admobInterstitial = "{your admob interstital placement}";

AdFactory.Instance.PreLoadInterstitialAds(ad_admobRewardeds);
AdFactory.Instance.PreLoadInterstitialAds(ad_admobInterstitial);
```

- Show Reward Video ads

```csharp
/// Show a reward video with default placement
AdFactory.Instance.ShowRewardedAds(RewardAdResult);

/// Show a reward video with the placement
string special_reward_placement = "{your reward video placement-3}";
AdFactory.Instance.ShowRewardedAds(RewardAdResult, special_reward_placement);

void RewardAdResult(AdFactory.RewardResult result){
    if(result == AdFactory.RewardResult.Success){
        // Ad shows and reward success
        // do your server-side verification if required
    }
}
```

- Show Intertistial ads

```csharp
/// Show a interstital ad with default placement
AdFactory.Instance.ShowInterstitialAds(InterstitialAdResult);

/// Show a interstital ad with the placement
string special_interstital_placement = "{your interstital placement-2}";
AdFactory.Instance.ShowInterstitialAds(InterstitialAdResult, special_interstital_placement);

void InterstitialAdResult(AdFactory.RewardResult result){
    if(result == AdFactory.RewardResult.Success){
        // Ad shows success
    }
}
```

- Show Banner
```csharp
/// Show a banner with default placement
bool result = AdFactory.Instance.ShowBannerAd();

/// Show a banner with default placement
string banner_placement = "{banner placement}";
bool result = AdFactory.Instance.ShowBannerAd(banner_placement);

/// result == true if banner show success
```

## Events
- OnBeforeAdShow

OnBeforeAdShow will fire once everytime the ShowInterstitialAds/ShowRewardedAds is called. It is useful to show a loading UI.
```csharp
AdFactory.Instance.OnBeforeAdShow += ()=>{
    // do something while the ShowInterstitialAds/ShowRewardedAds is called.
}
```

- OnAfterAdShow

OnAfterAdShow will fire once after the ad is closed by user or the ad is finish automatically. It is recommended to close your loading UI here.
Note: if there is no ad to show, OnAfterAdShow will wait 1 sec and fired after the OnBeforeAdShow is call.
```csharp
AdFactory.Instance.OnAfterAdShow += ()=>{
    // do something after user close the ads or the ad is finish automatically.
}
```

- OnAdResult

Fire everytime after ShowInterstitialAds/ShowRewardedAds.
```csharp
AdFactory.Instance.OnAdResult += (adType, result, placement)=>{
    // adType: the ad's type, reward video or interstital.
    // result: the ad's result.
    // placement the ad's placement.
}
```

- OnAdAnalysic

Fire everytime when ShowInterstitialAds/ShowRewardedAds is call but before ad is shown.
```csharp
AdFactory.Instance.OnAdAnalysic += (string data)=>{
    // data is the value of ShowInterstitialAds/ShowRewardedAds's "analysicData" parameter value 
}
```

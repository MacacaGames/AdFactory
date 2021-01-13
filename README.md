See [Document](http://macacagames.github.io/AdFactory/) for more detail.

# Welcome to Macaca AdFactory
AdFactory is a Mobile Ad Wrapper for Unity3D, you can easy change Ad's implemention by simply implement dirrerent IAdManager, and also makes testing Ad in Editor easilier.

## Installation

### Option 1: Unity Package manager
Add it to your editor's `manifest.json` file like this:
```json
{
    "dependencies": {
        "com.macacagames.adfactory": "https://github.com/MacacaGames/AdFactory.git#1.0.0",
        "com.macacagames.utility": "https://github.com/MacacaGames/MacacaUtility.git#1.0.0",
    }
}
```
You can remove the #1.0.0 to use the latest version (unstable)


### Option 2: Git SubModule
Note: AdFactory is dependency with Macaca Utility so also add it in git submodule.

```bash
git submodule add https://github.com/MacacaGames/AdFactory.git Assets/MacacaGameSystem

git submodule add https://github.com/MacacaGames/MacacaUtility.git Assets/Mast
```
## Implement the IAdManager for your project
Implement your IAdManager to makes AdFactory work, or use pre-build IAdManager, currentlly we have implement Admob, IronSource, UnityAds.

## Use pre-build IAdManager implement
AdFactory has implement three IAdManager, to use the pre-build IAdManager just copy the IAdManager from PackageRoot/IAdManagerImpl folder to your project UnityProject/Assets folder.
Please remember also import the third party Ad SDK.

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
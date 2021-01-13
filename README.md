See [Document](http://macacagames.github.io/AdFactory/) for more detail.

# Welcome to Macaca AdFactory

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

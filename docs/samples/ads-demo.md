# Ads Demo

Sample scene demonstrating ad integration.

---

## Scene Overview

**Location:** `Assets/_IntelliVerseXSDK/Samples/Scenes/IVX_AdsTest.unity`

This sample demonstrates:

- Banner ads (show/hide)
- Interstitial ads
- Rewarded video ads
- Reward handling
- Ad events

---

## Scene Hierarchy

```
Canvas
├── ControlPanel
│   ├── BannerSection
│   │   ├── ShowBannerButton
│   │   └── HideBannerButton
│   ├── InterstitialSection
│   │   ├── LoadButton
│   │   ├── ShowButton
│   │   └── StatusText
│   └── RewardedSection
│       ├── LoadButton
│       ├── WatchAdButton
│       ├── StatusText
│       └── RewardDisplay
├── CoinDisplay
│   └── CoinText
└── EventLog
    └── LogScrollView
```

---

## Key Components

### AdsDemoController.cs

```csharp
using IntelliVerseX.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdsDemoController : MonoBehaviour
{
    [Header("Banner")]
    [SerializeField] private Button _showBannerBtn;
    [SerializeField] private Button _hideBannerBtn;
    
    [Header("Interstitial")]
    [SerializeField] private Button _loadInterstitialBtn;
    [SerializeField] private Button _showInterstitialBtn;
    [SerializeField] private TMP_Text _interstitialStatus;
    
    [Header("Rewarded")]
    [SerializeField] private Button _loadRewardedBtn;
    [SerializeField] private Button _watchAdBtn;
    [SerializeField] private TMP_Text _rewardedStatus;
    
    [Header("Rewards")]
    [SerializeField] private TMP_Text _coinText;
    private int _coins = 0;
    
    [Header("Log")]
    [SerializeField] private TMP_Text _logText;
    
    void Start()
    {
        SetupButtons();
        SetupEventHandlers();
        UpdateUI();
        
        // Preload ads
        LoadInterstitial();
        LoadRewarded();
    }
    
    void SetupButtons()
    {
        _showBannerBtn.onClick.AddListener(ShowBanner);
        _hideBannerBtn.onClick.AddListener(HideBanner);
        _loadInterstitialBtn.onClick.AddListener(LoadInterstitial);
        _showInterstitialBtn.onClick.AddListener(ShowInterstitial);
        _loadRewardedBtn.onClick.AddListener(LoadRewarded);
        _watchAdBtn.onClick.AddListener(ShowRewarded);
    }
    
    void SetupEventHandlers()
    {
        // Banner events
        IVXAdsManager.OnBannerLoaded += () => Log("Banner loaded");
        IVXAdsManager.OnBannerFailed += (err) => Log($"Banner failed: {err}");
        
        // Interstitial events
        IVXAdsManager.OnInterstitialLoaded += () => 
        {
            Log("Interstitial ready");
            UpdateUI();
        };
        IVXAdsManager.OnInterstitialShown += () => Log("Interstitial shown");
        IVXAdsManager.OnInterstitialClosed += () => 
        {
            Log("Interstitial closed");
            LoadInterstitial();
        };
        
        // Rewarded events
        IVXAdsManager.OnRewardedVideoLoaded += () => 
        {
            Log("Rewarded video ready");
            UpdateUI();
        };
        IVXAdsManager.OnRewardedVideoCompleted += (reward) => 
        {
            Log($"Reward earned: {reward.Amount} {reward.Currency}");
            AddCoins(100);
        };
        IVXAdsManager.OnRewardedVideoClosed += (wasRewarded) => 
        {
            Log($"Rewarded closed (rewarded: {wasRewarded})");
            LoadRewarded();
        };
    }
    
    // Banner methods
    void ShowBanner()
    {
        IVXAdsManager.Instance.ShowBanner(BannerPosition.Bottom);
        Log("Showing banner");
    }
    
    void HideBanner()
    {
        IVXAdsManager.Instance.HideBanner();
        Log("Hiding banner");
    }
    
    // Interstitial methods
    void LoadInterstitial()
    {
        IVXAdsManager.Instance.LoadInterstitial();
        _interstitialStatus.text = "Loading...";
        Log("Loading interstitial");
    }
    
    void ShowInterstitial()
    {
        if (IVXAdsManager.Instance.IsInterstitialReady())
        {
            IVXAdsManager.Instance.ShowInterstitial();
        }
        else
        {
            Log("Interstitial not ready");
        }
    }
    
    // Rewarded methods
    void LoadRewarded()
    {
        IVXAdsManager.Instance.LoadRewardedVideo();
        _rewardedStatus.text = "Loading...";
        Log("Loading rewarded video");
    }
    
    void ShowRewarded()
    {
        if (IVXAdsManager.Instance.IsRewardedVideoReady())
        {
            IVXAdsManager.Instance.ShowRewardedVideo();
        }
        else
        {
            Log("Rewarded video not ready");
        }
    }
    
    // Helpers
    void AddCoins(int amount)
    {
        _coins += amount;
        _coinText.text = $"Coins: {_coins}";
    }
    
    void UpdateUI()
    {
        _showInterstitialBtn.interactable = IVXAdsManager.Instance.IsInterstitialReady();
        _interstitialStatus.text = _showInterstitialBtn.interactable ? "Ready" : "Not Ready";
        
        _watchAdBtn.interactable = IVXAdsManager.Instance.IsRewardedVideoReady();
        _rewardedStatus.text = _watchAdBtn.interactable ? "Ready" : "Not Ready";
    }
    
    void Log(string message)
    {
        Debug.Log($"[Ads] {message}");
        _logText.text = $"{System.DateTime.Now:HH:mm:ss} - {message}\n{_logText.text}";
    }
}
```

---

## How to Use

### Running the Sample

1. Open `IVX_AdsTest.unity`
2. Configure ad provider in `IntelliVerseXConfig`
3. Press **Play**

### Testing Banner

1. Click **"Show Banner"**
2. Banner appears at bottom
3. Click **"Hide Banner"** to remove

### Testing Interstitial

1. Click **"Load"** (auto-loads on start)
2. Wait for "Ready" status
3. Click **"Show"**
4. Watch ad play
5. Close to return

### Testing Rewarded

1. Click **"Load"** (auto-loads on start)
2. Wait for "Ready" status
3. Click **"Watch Ad"**
4. Watch full video
5. Observe coin reward

---

## Test Mode

!!! warning "Use Test Ads"
    Always use test mode during development to avoid account flags.

In `IntelliVerseXConfig`:

- Enable **"Dev Test Ads"** checkbox
- Ads will show test content

---

## Event Log

The sample includes a real-time event log showing:

- Ad load events
- Ad show events
- Ad completion events
- Errors

---

## Customization

### Change Reward Amount

```csharp
// In IntelliVerseXConfig or code:
private const int REWARD_AMOUNT = 100;

IVXAdsManager.OnRewardedVideoCompleted += (reward) => 
{
    AddCoins(REWARD_AMOUNT);
};
```

### Different Banner Position

```csharp
// Top banner
IVXAdsManager.Instance.ShowBanner(BannerPosition.Top);

// Center banner
IVXAdsManager.Instance.ShowBanner(BannerPosition.Center);
```

---

## See Also

- [Monetization Module](../modules/monetization.md)
- [Ad Integration Guide](../guides/ad-integration.md)
- [Ads Configuration](../configuration/ads-config.md)

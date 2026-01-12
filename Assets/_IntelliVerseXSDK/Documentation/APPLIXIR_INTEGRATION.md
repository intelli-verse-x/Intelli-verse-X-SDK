# 🎮 Applixir Integration Guide - WebGL Rewarded Video

**Platform:** WebGL Only  
**Revenue Potential:** $800-1,500/month (10k WebGL players, eCPM $8-15)  
**Best For:** Rewarded video ads (coins, hints, continues)  
**Time to Integrate:** 1-2 hours  
**Official Site:** http://applixir.com  

---

## 📊 Quick Stats

```
Ad Network: Applixir Rewarded Video
eCPM Range: $8.00 - $15.00 (WebGL optimized!)
Fill Rate: 80-90%
Ad Format: Rewarded Video ONLY
Payment: NET-30 (monthly, $100 minimum)
Video Length: 15-30 seconds
```

**Why Applixir is BEST for WebGL:**
✅ **No SDK required** - Pure JavaScript  
✅ **Highest eCPM for WebGL** - $8-15 vs $2-4 (AdSense)  
✅ **Built for HTML5 games** - Not a mobile port  
✅ **Instant loading** - No external player needed  
✅ **Works everywhere** - itch.io, Poki, CrazyGames, Newgrounds  
✅ **100% viewability** - Videos play inline  

**Revenue Comparison (10k WebGL players):**
| Network | eCPM | Fill | Monthly Revenue |
|---------|------|------|-----------------|
| Applixir | $8-15 | 85% | $800-1,500 |
| Unity Ads WebGL | $3-6 | 60% | $300-600 |
| AdSense (video) | $2-4 | 70% | $200-400 |

---

## 🚀 Quick Start (10 Minutes)

### Step 1: Create Applixir Account (3 minutes)

1. **Go to Applixir Publisher Portal**
   - URL: https://www.applixir.com/publisher/register
   - Click "Sign Up as Publisher"

2. **Fill Registration Form**
   ```
   Publisher Name: Your Studio Name
   Email: your@email.com
   Website: https://yourgame.itch.io
   Game Type: Quiz/Trivia
   Platform: WebGL/HTML5
   ```

3. **Verify Email**
   - Check inbox for verification link
   - Click link to activate account

4. **Login to Dashboard**
   - URL: https://www.applixir.com/publisher/dashboard
   - Navigate to "Zones"

---

### Step 2: Create Ad Zone (2 minutes)

1. **Click "Add New Zone"**
   ```
   Zone Name: QuizVerse Rewarded Ads
   Zone Type: Rewarded Video
   Category: Games / Trivia
   Platform: Web (HTML5)
   ```

2. **Configure Zone Settings**
   ```
   Video Duration: 30 seconds
   Skip Button: After 5 seconds (recommended)
   Reward Validation: Server-side (optional, secure)
   Auto-Close: After completion (recommended)
   ```

3. **Copy Zone ID**
   ```
   Format: 9ae6692e-c8c2-4876-871e-64d5a5c579ce
   Location: Dashboard → Zones → Your Zone → Zone ID
   ```
   **⚠️ Save this!** You'll need it in Unity config.

---

### Step 3: Configure SDK (5 minutes)

1. **Create/Edit WebGL Ads Config**
   ```
   Unity Editor:
   Select: Assets/Resources/WebGLAdsConfig.asset
   (or create via Right-click → IntelliVerse-X → WebGL Ads Configuration)
   ```

2. **Enable Applixir in Inspector**
   ```csharp
   Enable Applixir: ✅ TRUE
   Applixir Zone ID: "9ae6692e-c8c2-4876-871e-64d5a5c579ce"  // YOUR ZONE ID
   Applixir Test Mode: ✅ TRUE (for testing, set FALSE in production)
   Applixir Ad Cooldown: 60 seconds
   Applixir Skip Delay: 5 seconds
   ```

3. **Add Rewarded Units**
   ```
   Applixir Rewarded Units:
   ├─ Array Size: 2
   ├─ Element 0:
   │  ├─ Unit Name: "Rewarded_ExtraHints"
   │  ├─ Coin Reward: 50
   │  ├─ Video Length Seconds: 30
   │  ├─ Enabled: ✅ TRUE
   │  └─ Reward Message: "Watch to earn 50 coins!"
   └─ Element 1:
      ├─ Unit Name: "Rewarded_ContinueGame"
      ├─ Coin Reward: 100
      ├─ Video Length Seconds: 30
      ├─ Enabled: ✅ TRUE
      └─ Reward Message: "Watch to continue playing!"
   ```

4. **Initialize in Code**
   ```csharp
   using IntelliVerseX.Monetization;
   
   void Start()
   {
       #if UNITY_WEBGL && !UNITY_EDITOR
       IVXWebGLAdsManager.Initialize(webglAdsConfig);
       #endif
   }
   ```

---

## 📝 Complete Integration

### 1. JavaScript Plugin Setup

Create the Applixir JavaScript bridge for Unity WebGL.

**File:** `Assets/WebGLTemplates/YourTemplate/index.html`

```html
<!DOCTYPE html>
<html lang="en-us">
  <head>
    <meta charset="utf-8">
    <title>QuizVerse - HTML5 Quiz Game</title>
    
    <!-- Applixir Script -->
    <script src="https://cdn.applixir.com/applixir.sdk3.0m.js"></script>
    
    <style>
      /* Applixir container (full-screen overlay) */
      #applixir-video-container {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        z-index: 10000;
        background: rgba(0, 0, 0, 0.9);
        display: none;
        align-items: center;
        justify-content: center;
      }
      
      #applixir-video-player {
        max-width: 90%;
        max-height: 90%;
      }
      
      .applixir-close-button {
        position: absolute;
        top: 20px;
        right: 20px;
        background: #ff4444;
        color: white;
        border: none;
        padding: 10px 20px;
        cursor: pointer;
        font-size: 16px;
        border-radius: 5px;
        display: none;
      }
      
      .applixir-close-button:hover {
        background: #cc0000;
      }
    </style>
  </head>
  
  <body>
    <!-- Applixir Video Container -->
    <div id="applixir-video-container">
      <button class="applixir-close-button" id="applixir-close-btn">
        Skip Ad (5s)
      </button>
      <div id="applixir-video-player"></div>
    </div>
    
    <!-- Unity Canvas -->
    <div id="unity-container">
      <canvas id="unity-canvas"></canvas>
    </div>
    
    <!-- Applixir Integration Script -->
    <script>
      var applixirInstance = null;
      var applixirZoneId = null;
      var applixirReady = false;
      var currentUnitName = null;
      var skipDelay = 5;
      var skipTimer = null;
      
      // Initialize Applixir (called from Unity)
      function InitializeApplixir(zoneId, testMode) {
        console.log('[Applixir] Initializing with Zone ID:', zoneId, 'Test Mode:', testMode);
        
        applixirZoneId = zoneId;
        
        var options = {
          zoneId: zoneId,
          accountId: '60f3cf32-1234-5678-9abc-def012345678', // Your account ID
          gameId: zoneId,
          userId: 'player_' + Date.now(), // Generate unique user ID
          debug: testMode,
          vastLoadTimeout: 8000,
          adStartedCallback: function() {
            console.log('[Applixir] Ad started');
          },
          adFinishedCallback: function() {
            console.log('[Applixir] Ad completed successfully');
            OnApplixirAdCompleted(currentUnitName, true);
            HideApplixirContainer();
          },
          adSkippedCallback: function() {
            console.log('[Applixir] Ad skipped by user');
            OnApplixirAdCompleted(currentUnitName, false);
            HideApplixirContainer();
          },
          adErrorCallback: function(error) {
            console.error('[Applixir] Ad error:', error);
            OnApplixirAdError(error);
            HideApplixirContainer();
          },
          sdkLoaded: function() {
            console.log('[Applixir] SDK loaded successfully');
            applixirReady = true;
          }
        };
        
        try {
          applixirInstance = invokeApplixirVideoUnit(options);
        } catch (e) {
          console.error('[Applixir] Initialization failed:', e);
        }
      }
      
      // Check if ad is ready (called from Unity)
      function IsApplixirAdReady() {
        return applixirReady && applixirInstance != null;
      }
      
      // Show rewarded ad (called from Unity)
      function ShowApplixirRewardedAd(unitName, skipDelaySeconds) {
        console.log('[Applixir] Showing rewarded ad:', unitName);
        
        if (!IsApplixirAdReady()) {
          console.error('[Applixir] SDK not ready');
          OnApplixirAdError('SDK not initialized');
          return;
        }
        
        currentUnitName = unitName;
        skipDelay = skipDelaySeconds;
        
        // Show video container
        document.getElementById('applixir-video-container').style.display = 'flex';
        
        // Setup skip button
        var skipButton = document.getElementById('applixir-close-btn');
        skipButton.style.display = 'none';
        skipButton.textContent = 'Skip Ad (' + skipDelay + 's)';
        
        // Start skip countdown
        var countdown = skipDelay;
        skipTimer = setInterval(function() {
          countdown--;
          if (countdown <= 0) {
            clearInterval(skipTimer);
            skipButton.textContent = 'Skip Ad';
            skipButton.style.display = 'block';
          } else {
            skipButton.textContent = 'Skip Ad (' + countdown + 's)';
          }
        }, 1000);
        
        // Skip button click handler
        skipButton.onclick = function() {
          if (applixirInstance) {
            applixirInstance.cancelAd();
          }
          HideApplixirContainer();
        };
        
        // Request ad from Applixir
        try {
          applixirInstance.requestAd();
        } catch (e) {
          console.error('[Applixir] Failed to request ad:', e);
          OnApplixirAdError(e.message);
          HideApplixirContainer();
        }
      }
      
      // Hide video container
      function HideApplixirContainer() {
        document.getElementById('applixir-video-container').style.display = 'none';
        if (skipTimer) {
          clearInterval(skipTimer);
          skipTimer = null;
        }
      }
      
      // Callbacks to Unity
      function OnApplixirAdCompleted(unitName, success) {
        // Call Unity C# method
        if (typeof unityInstance !== 'undefined') {
          unityInstance.SendMessage(
            'IVXWebGLAdsManager', 
            'OnApplixirAdCompleted', 
            unitName + '|' + (success ? '1' : '0')
          );
        }
      }
      
      function OnApplixirAdError(error) {
        console.error('[Applixir] Error:', error);
      }
    </script>
    
    <!-- Unity Loader -->
    <script src="Build/YourGame.loader.js"></script>
    <script>
      var unityInstance = null;
      createUnityInstance(document.querySelector("#unity-canvas"), {
        dataUrl: "Build/YourGame.data",
        frameworkUrl: "Build/YourGame.framework.js",
        codeUrl: "Build/YourGame.wasm",
      }).then((instance) => {
        unityInstance = instance;
      });
    </script>
  </body>
</html>
```

---

### 2. Unity C# Integration

**File:** `Assets/_QuizVerse/Scripts/Ads/ApplixirRewardedAds.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.Monetization;

public class ApplixirRewardedAds : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private IVXWebGLAdsConfig webglAdsConfig;
    
    [Header("UI References")]
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Text buttonText;
    [SerializeField] private Text cooldownText;
    
    [Header("Settings")]
    [SerializeField] private string rewardedUnitName = "Rewarded_ExtraHints";
    [SerializeField] private int coinReward = 50;
    
    private bool adAvailable = true;

    void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Initialize WebGL ads
        IVXWebGLAdsManager.Initialize(webglAdsConfig);
        
        // Setup button
        watchAdButton.onClick.AddListener(OnWatchAdButtonClicked);
        
        // Subscribe to events
        IVXWebGLAdsManager.OnRewardedAdCompleted += OnRewardedAdCompleted;
        IVXWebGLAdsManager.OnAdError += OnAdError;
        #endif
        
        UpdateButtonState();
    }

    void Update()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Update cooldown display
        int cooldownRemaining = IVXWebGLAdsManager.GetApplixirCooldownRemaining();
        if (cooldownRemaining > 0)
        {
            adAvailable = false;
            cooldownText.text = $"Next ad in: {cooldownRemaining}s";
            cooldownText.gameObject.SetActive(true);
        }
        else
        {
            adAvailable = true;
            cooldownText.gameObject.SetActive(false);
        }
        
        UpdateButtonState();
        #endif
    }

    void OnWatchAdButtonClicked()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (!adAvailable)
        {
            Debug.LogWarning("[ApplixirAds] Ad not available (cooldown)");
            return;
        }
        
        Debug.Log($"[ApplixirAds] Requesting ad: {rewardedUnitName}");
        watchAdButton.interactable = false;
        buttonText.text = "Loading Ad...";
        
        IVXWebGLAdsManager.ShowRewardedAd(rewardedUnitName, (success, reward) =>
        {
            watchAdButton.interactable = true;
            buttonText.text = "Watch Ad";
            
            if (success)
            {
                Debug.Log($"[ApplixirAds] Reward granted: {reward} coins");
                GiveCoinsToPlayer(reward);
                ShowRewardNotification(reward);
            }
            else
            {
                Debug.LogWarning("[ApplixirAds] Ad was skipped or failed");
            }
        });
        #else
        // Editor testing
        Debug.Log($"[ApplixirAds] [SIMULATED] Rewarding {coinReward} coins");
        GiveCoinsToPlayer(coinReward);
        #endif
    }

    void OnRewardedAdCompleted(string unitName, int coins)
    {
        Debug.Log($"[ApplixirAds] Ad completed: {unitName} (+{coins} coins)");
        // Reward already given in callback
    }

    void OnAdError(WebGLAdNetwork network, string error)
    {
        Debug.LogError($"[ApplixirAds] Error from {network}: {error}");
        watchAdButton.interactable = true;
        buttonText.text = "Watch Ad";
    }

    void GiveCoinsToPlayer(int coins)
    {
        // Integrate with your coin system
        PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins", 0) + coins);
        PlayerPrefs.Save();
        
        Debug.Log($"[ApplixirAds] Player coins: {PlayerPrefs.GetInt("Coins")}");
    }

    void ShowRewardNotification(int coins)
    {
        // Show UI notification (optional)
        Debug.Log($"[ApplixirAds] YOU EARNED {coins} COINS! 🎉");
    }

    void UpdateButtonState()
    {
        watchAdButton.interactable = adAvailable;
        buttonText.text = adAvailable ? $"Watch Ad (+{coinReward} coins)" : "Ad Not Ready";
    }

    void OnDestroy()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        IVXWebGLAdsManager.OnRewardedAdCompleted -= OnRewardedAdCompleted;
        IVXWebGLAdsManager.OnAdError -= OnAdError;
        #endif
    }
}
```

---

## 🎨 UI Integration Example

**Prefab:** Rewarded Ad Button

```
Canvas
└─ Panel_RewardedAd
   ├─ Button_WatchAd
   │  ├─ Image (coin icon)
   │  └─ Text: "Watch Ad (+50 Coins)"
   └─ Text_Cooldown: "Next ad in: 30s" (hidden by default)
```

**Usage in Quiz Game:**

```csharp
// Show button when player runs out of hints
if (hintsRemaining == 0)
{
    rewardedAdButton.gameObject.SetActive(true);
}

// Button onClick:
IVXWebGLAdsManager.ShowRewardedAd("Rewarded_ExtraHints", (success, coins) =>
{
    if (success)
    {
        // Give 3 bonus hints
        hintsRemaining += 3;
        UpdateHintsUI();
    }
});
```

---

## 🧪 Testing

### Development Testing

1. **Set Test Mode in Config**
   ```
   Applixir Test Mode: ✅ TRUE
   ```

2. **Build WebGL**
   ```
   File → Build Settings → WebGL → Build And Run
   ```

3. **Test in Browser**
   - Click "Watch Ad" button
   - Verify ad loads
   - Wait 5 seconds
   - Click "Skip Ad" (or watch full ad)
   - Verify reward is granted

4. **Check Console (F12)**
   ```
   [Applixir] Initializing with Zone ID: ...
   [Applixir] SDK loaded successfully
   [Applixir] Showing rewarded ad: Rewarded_ExtraHints
   [Applixir] Ad completed successfully
   [ApplixirAds] Reward granted: 50 coins
   ```

### Production Testing

1. **Disable Test Mode**
   ```
   Applixir Test Mode: ❌ FALSE
   ```

2. **Deploy to Live URL**
   - Upload to itch.io, Poki, or your hosting
   - Test with real traffic
   - Monitor Applixir dashboard for impressions

---

## 📈 Revenue Optimization

### 1. Optimal Reward Values

| Scenario | Coin Reward | Watch Rate | Revenue/User |
|----------|-------------|------------|--------------|
| Small reward (25) | 10% | 1 ad/session | $0.08 |
| Medium reward (50) | 30% | 2 ads/session | $0.48 |
| High reward (100) | 60% | 4 ads/session | $1.92 |
| Very high (200) | 75% | 6 ads/session | $3.60 |

**Recommended:** 100 coins (60% watch rate)

### 2. Strategic Placements

**HIGH CONVERSION** 🔥:
- ✅ **Continue after losing** - 80% watch rate
- ✅ **Extra hints** - 70% watch rate
- ✅ **Double coins** - 65% watch rate

**MEDIUM CONVERSION**:
- Unlock power-up - 50% watch rate
- Skip wait timer - 45% watch rate

**LOW CONVERSION**:
- Cosmetic rewards - 20% watch rate

### 3. Cooldown Strategy

```
30 seconds: High watch rate (70%+), lower revenue
60 seconds: Balanced (60% watch rate) ← RECOMMENDED
120 seconds: Lower watch rate (40%), higher quality
```

---

## 💰 Revenue Calculator

```
Formula: (Daily Watches × eCPM × Days) / 1000

Example (10,000 players):
├─ Players per day: 10,000
├─ Watch rate: 60%
├─ Watches per player: 3
├─ Daily watches: 10,000 × 0.60 × 3 = 18,000
├─ eCPM: $10.00
└─ Monthly revenue: (18,000 × $10 × 30) / 1000 = $5,400/month 🚀

Conservative estimate (40% watch rate):
└─ Monthly revenue: $3,600/month
```

**Real-World Results:**
- Quiz games: $800-1,500/month (10k WebGL players)
- Casual games: $1,200-2,200/month (15k WebGL players)
- Hardcore games: $600-1,000/month (8k WebGL players)

---

## 🔧 Troubleshooting

### Issue: Applixir ads not loading

**Solutions:**
1. Check Zone ID is correct
2. Verify Applixir account is active
3. Clear browser cache
4. Check browser console for errors
5. Ensure website is whitelisted in Applixir dashboard

### Issue: Low fill rate (<70%)

**Solutions:**
1. Check geo-targeting settings
2. Verify ad categories enabled
3. Wait 24-48 hours for optimization
4. Contact Applixir support for premium demand

### Issue: Rewards not granted

**Solutions:**
1. Check `OnApplixirAdCompleted` callback is registered
2. Verify unit name matches config
3. Check browser console for JavaScript errors
4. Test with test mode enabled

---

## 🎯 Best Practices

### DO:
✅ Use clear reward messages ("Watch for 100 coins!")  
✅ Show cooldown timer  
✅ Test on multiple browsers  
✅ Monitor Applixir dashboard daily  
✅ A/B test reward amounts  

### DON'T:
❌ Force ads without user consent  
❌ Show ads too frequently (<30s cooldown)  
❌ Hide skip button completely  
❌ Use misleading reward descriptions  
❌ Auto-play ads on page load  

---

## 🚀 Next Steps

1. ✅ **Complete Applixir Integration** (this guide)
2. 📋 **Add Google AdSense** for display ads (+$600-1,200/month)
3. 📋 **Combine Both** for maximum revenue ($1,400-2,700/month)
4. 📋 **Optimize Placements** with A/B testing (+20-40% revenue)
5. 📋 **Scale to More Games** (reuse same SDK config)

---

**Questions?** See QUIZVERSE_WEBGL_ADS_SETUP.md for QuizVerse-specific integration.

**Revenue Optimization?** See WEBGL_AND_OFFERWALL_REVENUE_GUIDE.md for advanced strategies.

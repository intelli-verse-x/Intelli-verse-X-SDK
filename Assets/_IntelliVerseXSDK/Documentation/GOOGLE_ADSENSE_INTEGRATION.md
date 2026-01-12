# 🌐 Google AdSense Integration Guide - WebGL Builds

**Platform:** WebGL Only  
**Revenue Potential:** $600-1,200/month (10k WebGL players, CPM $2-4)  
**Best For:** Display ads, native ads, in-feed content ads  
**Time to Integrate:** 2-3 hours  

---

## 📊 Quick Stats

```
Ad Network: Google AdSense for Games
eCPM Range: $2.00 - $4.00
Fill Rate: 70-85%
Ad Formats: Display, Native, In-Feed, In-Article
Payment: NET-30 (monthly, $100 minimum)
```

**Revenue Comparison (10k DAU):**
| Metric | Mobile Ads | WebGL AdSense |
|--------|-----------|---------------|
| eCPM | $8-12 | $2-4 |
| Fill Rate | 90-95% | 70-85% |
| Monthly Revenue | $3,600-5,400 | $600-1,200 |

**Why Use AdSense for WebGL:**
✅ No SDK required (JavaScript integration)  
✅ Excellent browser compatibility  
✅ Works on all HTML5 platforms  
✅ Google's trusted ad quality  
✅ Automatic optimization  
✅ Direct integration with Itch.io, Poki, CrazyGames  

---

## 🚀 Quick Start (15 Minutes)

### Step 1: Create AdSense Account (5 minutes)

1. **Go to Google AdSense**
   - URL: https://www.google.com/adsense/
   - Click "Get Started"

2. **Sign in with Google Account**
   - Use existing Google account or create new one
   - Recommended: Use same account as Unity Dashboard

3. **Fill Application Form**
   ```
   Website: https://yourgame.com (or itch.io page)
   Content Type: Games
   Platform: Web (HTML5)
   ```

4. **Wait for Approval**
   - Typical approval: 24-48 hours
   - Check: Sufficient content, policy compliant

5. **Get Publisher ID**
   - Format: `ca-pub-XXXXXXXXXXXXXXXX`
   - Example: `ca-pub-3940256099942544`
   - **Copy this** - you'll need it!

---

### Step 2: Create Ad Units (5 minutes)

1. **Navigate to Ad Units**
   ```
   AdSense Dashboard → Ads → Overview → Ad units → Display ads
   ```

2. **Create Banner Ad Unit**
   ```
   Name: QuizVerse_Banner_Top
   Size: Responsive (recommended)
   Ad type: Display ads
   ```
   
   Click "Create" → Copy "Ad unit ID" (data-ad-slot)
   ```
   Example: 1234567890
   ```

3. **Create Native Ad Unit** (Optional, higher eCPM)
   ```
   Name: QuizVerse_Native_InGame
   Size: Responsive
   Ad type: Native ads
   ```

4. **Create In-Feed Ad Unit** (Optional)
   ```
   Name: QuizVerse_InFeed_Results
   Size: Custom (match your layout)
   Ad type: In-feed ads
   ```

---

### Step 3: Configure SDK (5 minutes)

1. **Create WebGL Ads Config Asset**
   ```
   Unity Editor:
   Right-click in Project → IntelliVerse-X → WebGL Ads Configuration
   Name: WebGLAdsConfig
   ```

2. **Configure AdSense Settings**
   ```csharp
   // Inspector values:
   Enable AdSense: ✅ TRUE
   AdSense Publisher ID: ca-pub-XXXXXXXXXXXXXXXX  // YOUR ID
   Enable Auto Ads: ❌ FALSE (manual control recommended)
   Enable Ad Block Recovery: ✅ TRUE
   ```

3. **Add Ad Units in Inspector**
   ```
   Display Ad Units:
   ├─ Array Size: 1
   └─ Element 0:
      ├─ Unit Name: "Banner_Top"
      ├─ Ad Slot ID: "1234567890"  // FROM STEP 2
      ├─ Ad Format: Display
      ├─ Ad Size: Responsive
      └─ Enabled: ✅ TRUE
   ```

4. **Initialize in Game Code**
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

AdSense requires JavaScript code in your WebGL template. Create or modify your template:

**File:** `Assets/WebGLTemplates/YourTemplate/index.html`

```html
<!DOCTYPE html>
<html lang="en-us">
  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>QuizVerse - Play Now!</title>
    
    <!-- Google AdSense Script -->
    <script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-XXXXXXXXXXXXXXXX"
         crossorigin="anonymous"></script>
    
    <style>
      /* Ad container styling */
      .adsbygoogle {
        display: block;
        max-width: 100%;
      }
      
      #banner-top-container {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        z-index: 1000;
        background: #f0f0f0;
        text-align: center;
      }
    </style>
  </head>
  
  <body>
    <!-- Top Banner Ad Container -->
    <div id="banner-top-container" style="display:none;">
      <ins class="adsbygoogle"
           id="banner-top-ad"
           style="display:block"
           data-ad-client="ca-pub-XXXXXXXXXXXXXXXX"
           data-ad-slot="1234567890"
           data-ad-format="auto"
           data-full-width-responsive="true"></ins>
    </div>
    
    <!-- Your Unity WebGL Content -->
    <div id="unity-container">
      <canvas id="unity-canvas"></canvas>
    </div>
    
    <!-- Unity to JavaScript Bridge -->
    <script>
      // Initialize AdSense (called from Unity)
      function InitializeAdSense(publisherId, autoAds) {
        console.log('[AdSense] Initializing with publisher:', publisherId);
        
        if (autoAds) {
          (adsbygoogle = window.adsbygoogle || []).push({});
        }
      }
      
      // Show AdSense banner (called from Unity)
      function ShowAdSenseBanner(unitName, slotId, size) {
        console.log('[AdSense] Showing banner:', unitName, slotId, size);
        
        // Map Unity unit names to HTML element IDs
        const elementId = 'banner-' + unitName.toLowerCase().replace('_', '-') + '-ad';
        const containerElement = document.getElementById(elementId + '-container') || 
                                document.getElementById('banner-top-container');
        
        if (!containerElement) {
          console.error('[AdSense] Container not found for:', unitName);
          return;
        }
        
        // Show container
        containerElement.style.display = 'block';
        
        // Push ad request
        try {
          (adsbygoogle = window.adsbygoogle || []).push({});
        } catch (e) {
          console.error('[AdSense] Failed to load ad:', e);
        }
      }
      
      // Hide AdSense banner (called from Unity)
      function HideAdSenseBanner(unitName) {
        console.log('[AdSense] Hiding banner:', unitName);
        
        const containerElement = document.getElementById('banner-top-container');
        if (containerElement) {
          containerElement.style.display = 'none';
        }
      }
      
      // Refresh AdSense banner (called from Unity)
      function RefreshAdSenseBanner(unitName) {
        console.log('[AdSense] Refreshing banner:', unitName);
        
        // Hide then show to force refresh
        HideAdSenseBanner(unitName);
        setTimeout(() => {
          ShowAdSenseBanner(unitName, '', '');
        }, 100);
      }
    </script>
    
    <!-- Unity Loader -->
    <script src="Build/YourGame.loader.js"></script>
    <script>
      createUnityInstance(document.querySelector("#unity-canvas"), {
        dataUrl: "Build/YourGame.data",
        frameworkUrl: "Build/YourGame.framework.js",
        codeUrl: "Build/YourGame.wasm",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "YourCompany",
        productName: "YourGame",
        productVersion: "1.0",
      });
    </script>
  </body>
</html>
```

---

### 2. Unity C# Integration

**File:** `Assets/_QuizVerse/Scripts/Ads/WebGLAdsController.cs`

```csharp
using UnityEngine;
using IntelliVerseX.Monetization;

public class WebGLAdsController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private IVXWebGLAdsConfig webglAdsConfig;
    
    [Header("Settings")]
    [SerializeField] private bool showBannerOnStart = true;
    [SerializeField] private float bannerAutoRefreshSeconds = 60f;
    
    private bool bannerVisible = false;
    private float bannerRefreshTimer = 0f;

    void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Initialize WebGL ads
        IVXWebGLAdsManager.Initialize(webglAdsConfig);
        
        // Show banner on start
        if (showBannerOnStart)
        {
            ShowBanner();
        }
        
        // Subscribe to events
        IVXWebGLAdsManager.OnBannerShown += OnBannerShown;
        IVXWebGLAdsManager.OnAdError += OnAdError;
        #endif
    }

    void Update()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Auto-refresh banner
        if (bannerVisible && bannerAutoRefreshSeconds > 0)
        {
            bannerRefreshTimer += Time.deltaTime;
            if (bannerRefreshTimer >= bannerAutoRefreshSeconds)
            {
                IVXWebGLAdsManager.RefreshAdSenseBanner("Banner_Top");
                bannerRefreshTimer = 0f;
            }
        }
        #endif
    }

    public void ShowBanner()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        IVXWebGLAdsManager.ShowBannerAd("Banner_Top");
        #else
        Debug.Log("[WebGLAds] Banner would show (WebGL only)");
        #endif
    }

    public void HideBanner()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        IVXWebGLAdsManager.HideBannerAd("Banner_Top");
        bannerVisible = false;
        #endif
    }

    private void OnBannerShown(string unitName, bool success)
    {
        if (success)
        {
            bannerVisible = true;
            bannerRefreshTimer = 0f;
            Debug.Log($"[WebGLAds] Banner shown: {unitName}");
        }
    }

    private void OnAdError(WebGLAdNetwork network, string error)
    {
        Debug.LogError($"[WebGLAds] Error from {network}: {error}");
    }

    void OnDestroy()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        IVXWebGLAdsManager.OnBannerShown -= OnBannerShown;
        IVXWebGLAdsManager.OnAdError -= OnAdError;
        #endif
    }
}
```

---

### 3. Advanced: Native Ads Integration

Native ads blend into your game UI for higher eCPM ($3-6 vs $2-4 for display).

**HTML Template Addition:**

```html
<!-- Native Ad Container (in-game UI) -->
<div id="native-ad-container" style="display:none; margin: 20px;">
  <ins class="adsbygoogle"
       id="native-ad"
       style="display:block"
       data-ad-format="fluid"
       data-ad-layout-key="-6t+ed+2i-1n-4w"
       data-ad-client="ca-pub-XXXXXXXXXXXXXXXX"
       data-ad-slot="0987654321"></ins>
</div>

<script>
  function ShowNativeAd() {
    document.getElementById('native-ad-container').style.display = 'block';
    (adsbygoogle = window.adsbygoogle || []).push({});
  }
</script>
```

**Unity C#:**

```csharp
[DllImport("__Internal")]
private static extern void ShowNativeAd();

public void ShowNativeAdInUI()
{
    #if UNITY_WEBGL && !UNITY_EDITOR
    ShowNativeAd();
    #endif
}
```

---

## 🧪 Testing

### Test Mode

AdSense doesn't have a dedicated test mode, but you can use Google's test publisher ID:

```
Test Publisher ID: ca-pub-3940256099942544
Test Ad Slot: 1234567890
```

**⚠️ CRITICAL:** Replace test IDs before production!

### Verification Checklist

```
✅ AdSense account approved
✅ Publisher ID added to config
✅ Ad units created in AdSense dashboard
✅ Ad slot IDs added to config
✅ JavaScript plugin added to WebGL template
✅ Unity bridge functions implemented
✅ Test build loads ads correctly
✅ Ads visible on desktop browser
✅ Ads visible on mobile browser
✅ Production IDs used (not test IDs)
```

---

## 📈 Revenue Optimization

### 1. Ad Placement Strategy

**HIGH REVENUE** 🔥:
- ✅ Top banner (above game): $3-4 CPM
- ✅ Native ads in menu: $4-6 CPM
- ✅ In-feed ads in results screen: $3-5 CPM

**MEDIUM REVENUE**:
- Bottom banner: $2-3 CPM
- Side banners: $2-3 CPM

**LOW REVENUE**:
- Inline text ads: $0.50-1.50 CPM

### 2. Refresh Strategy

```csharp
// Optimal refresh intervals
Banner Ads: 60-90 seconds (higher = better)
Native Ads: No refresh (one-time load)
In-Feed Ads: On scroll/page change
```

### 3. A/B Testing

Test different ad placements:

```csharp
public enum BannerPlacement { Top, Bottom, None }

// A/B test code
BannerPlacement testPlacement = Random.value > 0.5f ? 
    BannerPlacement.Top : BannerPlacement.Bottom;

if (testPlacement == BannerPlacement.Top)
{
    ShowTopBanner();
}
else
{
    ShowBottomBanner();
}

// Track revenue per placement in analytics
IVXAnalytics.LogEvent("banner_placement_test", new {
    placement = testPlacement.ToString(),
    user_id = userId
});
```

---

## 🔧 Troubleshooting

### Issue: Ads not showing

**Solutions:**
1. Check browser console for errors (F12)
2. Verify publisher ID is correct
3. Ensure ad slot ID matches AdSense dashboard
4. Check if ad blocker is active
5. Wait 15-30 minutes after creating new ad units

### Issue: Blank ad spaces

**Solutions:**
1. Check AdSense account status (not suspended)
2. Verify website URL is approved
3. Ensure sufficient traffic (AdSense needs 10+ visits/day)
4. Check responsive ad sizing in CSS

### Issue: Low fill rate (<50%)

**Solutions:**
1. Enable more ad categories in AdSense settings
2. Allow display ads + native ads
3. Check geo-targeting (some regions have low fill)
4. Verify content policy compliance

---

## 💰 Revenue Projections

### Monthly Revenue Calculator

```
Formula: (Daily Impressions × CPM × Days) / 1000

Example (10,000 DAU):
Daily Impressions: 30,000 (3 ads per session)
CPM: $3.00
Days: 30

Revenue = (30,000 × $3.00 × 30) / 1000 = $2,700/month
```

**Realistic Ranges (10k WebGL players):**
| Ad Strategy | Low | Average | High |
|-------------|-----|---------|------|
| Banner Only | $400 | $600 | $800 |
| Banner + Native | $800 | $1,000 | $1,400 |
| Full Suite | $1,000 | $1,500 | $2,200 |

---

## 🎯 Next Steps

1. ✅ **Complete AdSense Integration** (this guide)
2. 📋 **Add Applixir for Rewarded Video** (See APPLIXIR_INTEGRATION.md)
3. 📋 **Combine Both for Maximum Revenue** ($1,400-2,700/month)
4. 📋 **Optimize with A/B Testing** (+20-30% revenue)
5. 📋 **Monitor AdSense Dashboard** (weekly performance review)

---

**Questions?** See QUIZVERSE_WEBGL_ADS_SETUP.md for QuizVerse-specific integration.

**Revenue Optimization?** See WEBGL_AND_OFFERWALL_REVENUE_GUIDE.md for advanced strategies.

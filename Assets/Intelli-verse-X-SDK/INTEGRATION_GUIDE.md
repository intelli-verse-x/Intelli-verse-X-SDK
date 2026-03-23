# IntelliVerse-X SDK - Complete Integration Guide

## 📦 What is IntelliVerse-X SDK?

A unified SDK for Unity games that provides:
- 🎮 **Authentication** - Guest, Email/Password, Apple, Google
- 🏆 **Leaderboards** - Daily, Weekly, Monthly, All-Time, Global
- 💰 **Wallets** - Per-game and cross-game currency
- 🎯 **Multiplayer** - Photon integration
- 📊 **Analytics** - User behavior tracking
- 💵 **Monetization** - Ads, IAP, Offerwall
- 🌍 **Localization** - Multi-language support
- 🎨 **Ready-to-Use UI** - Intro and Login scenes

---

## 🚀 Quick Start (5 Minutes)

### Step 1: Import SDK
```
Assets/
  _IntelliVerseXSDK/          ← SDK folder (drag and drop into your project)
    Core/
    Backend/
    Monetization/
    UI/
    Scenes/
```

### Step 2: Create Game Configuration
1. Right-click in `Assets/Resources/IntelliVerseX/`
2. Choose **Create → IntelliVerse-X → Game Configuration**
3. Name it `<YourGameName>Config.asset`
4. Fill in:
   - **Game ID**: Get from admin panel (e.g., `126bf539-dae2-4bcf-964d-316c0fa1f92b`)
   - **Game Name**: Your game's name (e.g., `QuizVerse`)
   - **Features**: Enable what you need (Leaderboards, Wallets, Ads, etc.)

### Step 3: Add Intro Scene 
1. Create a new scene: `File → New Scene`
2. Drag `_IntelliVerseXSDK/UI/Prefabs/IVX_IntroScene.prefab` into hierarchy
3. Configure **IVX Intro Controller**:
   - **Next Scene Name**: `"LoginScene"` or `"MainMenu"`
4. Add to Build Settings as Scene 0

### Step 4: Add Login Scene (Optional)
1. Create a new scene: `File → New Scene`
2. Drag `_IntelliVerseXSDK/UI/Prefabs/IVX_LoginScene.prefab` into hierarchy
3. Configure **IVX Login Controller**:
   - **Main Menu Scene Name**: `"MainMenu"`
4. Add to Build Settings

### Step 5: Initialize SDK in Your Game
```csharp
using IntelliVerseX.Core;

public class MyGameBootstrap : MonoBehaviour
{
    void Start()
    {
        // Load your game config
        var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/MyGameConfig");
        
        // Initialize SDK
        IntelliVerseXManager.Initialize(config);
        
        // Optional: Subscribe to events
        IntelliVerseXManager.Instance.OnReady += OnSDKReady;
    }
    
    void OnSDKReady()
    {
        Debug.Log("✅ SDK Ready!");
        Debug.Log($"User: {IntelliVerseXIdentity.Username}");
        Debug.Log($"Device ID: {IntelliVerseXIdentity.DeviceId}");
    }
}
```

**That's it!** Your game now has authentication, leaderboards, wallets, and more.

---

## 🎨 Using Pre-Built UI Components

### Option A: Full SDK Flow (Intro → Login → Your Game)

**Build Settings:**
```
Scene 0: IVX_IntroScene          ✅ Use As-Is (consistent branding)
Scene 1: YourGameLoginScene      🎨 Reskinned IVX_LoginScene (your theme)
Scene 2: MainMenu                ← Your game
```

**What You Get:**
- ✅ Professional intro animation (use as-is for brand consistency)
- ✅ Multiple login options (guest, email, social)
- ✅ Login screen reskinned to match YOUR game's visual identity
- ✅ Auto-login on subsequent launches
- ✅ Automatic SDK initialization

**Setup Time:** 10-15 minutes (5 min setup + 10 min reskinning)

**Reskinning Steps:**
1. **Intro Scene:** Drag `IVX_IntroScene` prefab → Scene 0, set `nextSceneName = "YourGameLoginScene"`, **Do NOT reskin**
2. **Login Scene:** Duplicate `IVX_LoginScene` → `YourGameLoginScene`, **Reskin** to match your game (see [LOGIN_RESKINNING_GUIDE.md](LOGIN_RESKINNING_GUIDE.md)), set `mainMenuSceneName = "MainMenu"`
3. Done! Professional flow with your game's unique visual identity

**Why This Approach?**
- ✅ Intro: Consistent IntelliVerse-X branding across ALL games
- ✅ Login: Matches each game's unique visual identity
- ✅ Keep SDK functionality, customize appearance
- ✅ Quick reskinning (10-30 minutes)

### Option B: Custom UI with SDK Backend

**Build Settings:**
```
Scene 0: YourCustomIntro    ← Your custom scene
Scene 1: YourGameScene      ← Your game
```

**In your code:**
```csharp
using IntelliVerseX.Core;
using IntelliVerseX.Backend;

public class YourCustomLogin : MonoBehaviour
{
    async void LoginAsGuest()
    {
        var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/MyGameConfig");
        IntelliVerseXManager.Initialize(config);
        
        // Your custom UI/logic here
        await AuthenticateUser();
    }
}
```

**What You Get:**
- ✅ Full UI control
- ✅ SDK backend functionality
- ✅ Leaderboards, wallets, etc.

---

## 📚 Core Features Guide

### 1. Authentication

#### Guest Login (Simplest)
```csharp
using IntelliVerseX.Core;

public async void LoginAsGuest()
{
    var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/MyGameConfig");
    IntelliVerseXManager.Initialize(config);
    
    // Guest account automatically created
    var user = IntelliVerseXIdentity.CurrentUser;
    Debug.Log($"Logged in as: {user.Username}");
}
```

#### Email/Password Login
```csharp
public async Task<bool> LoginWithEmail(string email, string password)
{
    // TODO: Cognito integration coming soon
    // For now, use guest login
    return true;
}
```

#### Current User Info
```csharp
using IntelliVerseX.Core;

void GetUserInfo()
{
    var user = IntelliVerseXIdentity.CurrentUser;
    Debug.Log($"Username: {user.Username}");
    Debug.Log($"Device ID: {user.DeviceId}");
    Debug.Log($"Game ID: {user.GameId}");
    Debug.Log($"User ID: {user.UserId}");
}
```

### 2. Leaderboards

#### Submit Score
```csharp
using IntelliVerseX.Backend;

public async void SubmitPlayerScore(int score)
{
    var manager = IVXLeaderboardManager.Instance;
    bool success = await manager.SubmitScore(score);
    
    if (success)
    {
        Debug.Log("Score submitted to all leaderboards!");
    }
}
```

**Automatically submits to:**
- Daily leaderboard (resets midnight UTC)
- Weekly leaderboard (resets Sunday)
- Monthly leaderboard (resets 1st of month)
- All-Time leaderboard (never resets)
- Global leaderboard (cross-game)

#### Fetch Leaderboards
```csharp
public async void LoadLeaderboards()
{
    var manager = IVXLeaderboardManager.Instance;
    var leaderboards = await manager.GetAllLeaderboards(50);
    
    if (leaderboards != null && leaderboards.success)
    {
        // Daily
        foreach (var record in leaderboards.daily.records)
        {
            Debug.Log($"#{record.rank}: {record.username} - {record.score}");
        }
        
        // Player's rank
        Debug.Log($"Your daily rank: #{leaderboards.player_ranks.daily_rank}");
    }
}
```

### 3. Wallets

#### Get Balance
```csharp
using IntelliVerseX.Backend;

public async void CheckBalance()
{
    var manager = IVXWalletManager.Instance;
    
    // Game wallet (per-game currency)
    long gameBalance = await manager.GetGameBalance();
    Debug.Log($"Game coins: {gameBalance}");
    
    // Global wallet (cross-game currency)
    long globalBalance = await manager.GetGlobalBalance();
    Debug.Log($"Global coins: {globalBalance}");
}
```

#### Update Balance
```csharp
public async void GrantReward(long amount)
{
    var manager = IVXWalletManager.Instance;
    
    // Increment game wallet
    bool success = await manager.UpdateGameBalance(amount, "increment");
    
    if (success)
    {
        Debug.Log($"Granted {amount} coins!");
    }
}
```

#### Wallet Operations
- `"increment"` - Add to balance
- `"decrement"` - Subtract from balance
- `"set"` - Set exact balance

### 4. Multiplayer (Photon)

**Setup:**
1. Get Photon App ID from [Photon Dashboard](https://dashboard.photonengine.com)
2. Add to your game config: `photonAppId = "your-app-id-here"`
3. Leave empty to use shared IntelliVerse-X App ID

```csharp
using Photon.Pun;
using IntelliVerseX.Core;

public class MyMultiplayerManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Configure Photon with game-specific App ID
        var settings = PhotonNetwork.PhotonServerSettings;
        settings.AppSettings.AppIdRealtime = IVXPhotonConfig.GetAppId();
        
        // Connect to Photon
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon!");
        PhotonNetwork.JoinLobby();
    }
}
```

**Where to Set Photon App ID:**
- **In Unity Editor:** `Resources/IntelliVerseX/YourGameConfig.asset` → Inspector → `Photon App Id` field
- **Leave Empty:** Uses shared IntelliVerse-X App ID (recommended for quick testing)
- **Set Your Own:** Use your own Photon App ID for production (better analytics, control)

**Getting Your Photon App ID:**
1. Visit [Photon Dashboard](https://dashboard.photonengine.com)
2. Create account (free)
3. Click "Create New App" → Select "Photon PUN"
4. Copy App ID and paste in Unity config

**See also:** [PHOTON_SETUP_GUIDE.md](PHOTON_SETUP_GUIDE.md) for complete setup instructions

### 5. Ads (Coming Soon)

```csharp
using IntelliVerseX.Monetization;

public void ShowRewardedAd()
{
    IVXAdsManager.ShowRewardedAd((success, reward) =>
    {
        if (success)
        {
            Debug.Log($"User earned: {reward} coins");
            // Grant reward
        }
    });
}
```

### 6. In-App Purchases

```csharp
using IntelliVerseX.Monetization;

public void BuyCoins()
{
    IVXIAPManager.PurchaseProduct("coins_100", (success, productId) =>
    {
        if (success)
        {
            Debug.Log($"Purchased: {productId}");
            // Grant coins
        }
    });
}
```

---

## 🎮 Game-Specific Integration

### Integrate Into Existing Game

If you already have a game and want to add SDK features:

#### 1. Install SDK
Drag `_IntelliVerseXSDK/` into your Assets

#### 2. Create Config
`Assets/Resources/IntelliVerseX/<YourGame>Config.asset`

#### 3. Initialize in Your Bootstrap
```csharp
using IntelliVerseX.Core;

public class YourGameManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitSDK()
    {
        var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/YourGameConfig");
        IntelliVerseXManager.Initialize(config);
    }
}
```

#### 4. Add Features Where Needed

**Submit score after game over:**
```csharp
async void OnGameOver(int finalScore)
{
    await IVXLeaderboardManager.Instance.SubmitScore(finalScore);
}
```

**Grant coins for achievements:**
```csharp
async void OnAchievementUnlocked(string achievementId)
{
    await IVXWalletManager.Instance.UpdateGameBalance(50, "increment");
}
```

---

## 🛠️ Advanced Configuration

### IntelliVerseXConfig Options

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "IntelliVerse-X/Game Configuration")]
public class IntelliVerseXConfig : ScriptableObject
{
    // Required
    public string gameId;              // From admin panel
    public string gameName;            // Your game name
    
    // Multiplayer (Optional)
    public bool enablePhotonMultiplayer = true;
    public string photonAppId = "";    // Your Photon App ID (leave empty for shared)
    
    // Features (enable/disable)
    public bool enableGuestAccounts = true;
    public bool enableAutoLogin = true;
    public bool enableLeaderboards = true;
    public bool enableWallets = true;
    public bool enableAds = true;
    
    // Localization
    public SystemLanguage defaultLanguage = SystemLanguage.English;
    public SystemLanguage[] supportedLanguages;
    
    // Debug
    public bool enableDebugLogs = false;
}
```

### Backend Configuration (Hardcoded)

All games use shared backend infrastructure:

**Nakama Server:**
- Host: `nakama-rest.intelli-verse-x.ai`
- Port: `443` (HTTPS)
- Server Key: `defaultkey`

**Photon:**
- Shared App ID: `fa2f730e-1c81-4d01-b11f-708680dcaf37` (fallback)
- Per-Game App ID: Set in config (`photonAppId` field)
- Get your own: [Photon Dashboard](https://dashboard.photonengine.com)

**Cognito:**
- User Pool: `aicart-user-pool`
- Region: `us-east-1`

You don't need to configure these - SDK handles it automatically.

---

## 📊 RPC Reference

### Available Server RPCs

| RPC Name | Purpose | Required Fields |
|----------|---------|----------------|
| `create_or_sync_user` | Create/sync user identity | `username`, `device_id`, `game_id` |
| `submit_score_and_sync` | Submit score to all leaderboards | `score`, `device_id`, `game_id` |
| `get_all_leaderboards` | Fetch all leaderboards | `device_id`, `game_id`, `limit` |
| `update_wallet_balance` | Update wallet balance | `amount`, `wallet_type`, `change_type` |
| `get_wallet_balance` | Get current balance | `device_id`, `game_id` |
| `send_direct_message` | Send DM | `recipient_user_id`, `message` |
| `get_direct_message_history` | Get DMs | `other_user_id`, `limit` |

### Example RPC Call (Advanced)

```csharp
using Nakama;

public async Task CallCustomRPC()
{
    var client = new Client("https", "nakama-rest.intelli-verse-x.ai", 443, "defaultkey");
    var session = await client.AuthenticateDeviceAsync(deviceId);
    
    var payload = new { score = 1000, device_id = deviceId, game_id = gameId };
    var json = JsonConvert.SerializeObject(payload);
    
    var response = await client.RpcAsync(session, "submit_score_and_sync", json);
    Debug.Log($"RPC Response: {response.Payload}");
}
```

---

## 🎯 Example: Complete Game Integration

### 1. Setup (One Time)

```bash
1. Drag _IntelliVerseXSDK/ into Assets/
2. Create Resources/IntelliVerseX/MyGameConfig.asset
3. Set Game ID and Game Name
4. Add IVX_IntroScene and IVX_LoginScene to Build Settings
```

### 2. Game Bootstrap

```csharp
// MyGameBootstrap.cs
using UnityEngine;
using IntelliVerseX.Core;

public class MyGameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/MyGameConfig");
        IntelliVerseXManager.Initialize(config);
    }
}
```

### 3. Game Manager

```csharp
// MyGameManager.cs
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Backend;

public class MyGameManager : MonoBehaviour
{
    void Start()
    {
        // SDK already initialized by bootstrap
        Debug.Log($"Welcome, {IntelliVerseXIdentity.Username}!");
    }
    
    public async void OnGameOver(int score)
    {
        // Submit to leaderboards
        await IVXLeaderboardManager.Instance.SubmitScore(score);
        
        // Grant coins
        await IVXWalletManager.Instance.UpdateGameBalance(score / 10, "increment");
        
        Debug.Log("Score submitted and coins granted!");
    }
}
```

### 4. Leaderboard UI

```csharp
// MyLeaderboardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Backend;

public class MyLeaderboardUI : MonoBehaviour
{
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    
    async void Start()
    {
        await LoadLeaderboards();
    }
    
    async Task LoadLeaderboards()
    {
        var manager = IVXLeaderboardManager.Instance;
        var data = await manager.GetAllLeaderboards(50);
        
        if (data != null && data.success)
        {
            // Display daily leaderboard
            foreach (var record in data.daily.records)
            {
                var entry = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                entry.GetComponentInChildren<TextMeshProUGUI>().text = 
                    $"#{record.rank}: {record.username} - {record.score}";
            }
        }
    }
}
```

---

## 🐛 Troubleshooting

### Issue: "Game ID is required!"
**Solution:** Create config in `Resources/IntelliVerseX/` and set Game ID

### Issue: "SDK not initialized"
**Solution:** Call `IntelliVerseXManager.Initialize(config)` before using any SDK features

### Issue: Leaderboards not showing
**Solution:** Submit at least one score first. Leaderboards auto-create on first submission.

### Issue: Intro scene not loading
**Solution:** Check Build Settings - IVX_IntroScene should be Scene 0

### Issue: Login buttons not working
**Solution:** Ensure config has `enableGuestAccounts = true`

---

## 📱 Platform-Specific Notes

### iOS
- Apple Sign In requires setup in Xcode
- Add `Sign In with Apple` capability
- Configure Apple Developer account

### Android
- Google Sign In requires Firebase setup
- Add `google-services.json` to `Assets/`
- Configure SHA-1 fingerprint

### WebGL
- Some features limited (no device ID)
- Use guest accounts with generated IDs
- Ad networks may have restrictions

---

## 🎓 Best Practices

### 1. Initialize Early
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void Init()
{
    IntelliVerseXManager.Initialize(config);
}
```

### 2. Handle Errors
```csharp
try
{
    await IVXLeaderboardManager.Instance.SubmitScore(score);
}
catch (Exception ex)
{
    Debug.LogError($"Score submission failed: {ex.Message}");
}
```

### 3. Cache User Data
```csharp
private IntelliVerseXUser _currentUser;

void Start()
{
    _currentUser = IntelliVerseXIdentity.CurrentUser;
    // Use _currentUser instead of calling CurrentUser repeatedly
}
```

### 4. Test with Guest Accounts
```csharp
// Enable guest accounts in config for easy testing
config.enableGuestAccounts = true;
config.enableAutoLogin = true;
```

---

## 📖 Additional Resources

- **Server Documentation:** `CHAT_AND_STORAGE_FIX_DOCUMENTATION.md`
- **Deployment Guide:** `DEPLOYMENT_CHECKLIST.md`
- **Server Update:** `SERVER_UPDATE_NOVEMBER_2025.md`
- **Nakama Integration:** `NAKAMA_INTEGRATION_MIGRATION.md`
- **Testing Guide:** `TESTING_QUICK_REFERENCE.md`

---

## 🆘 Support

- **Documentation:** `_IntelliVerseXSDK/README.md`
- **Examples:** `_IntelliVerseXSDK/Examples/`
- **Server Logs:** `https://nakama-console.intelli-verse-x.ai`

---

## ✅ SDK Checklist

Before releasing your game:

- [ ] Created game config in `Resources/IntelliVerseX/`
- [ ] Set unique Game ID from admin panel
- [ ] Tested guest login
- [ ] Tested score submission
- [ ] Tested leaderboard fetching
- [ ] Tested wallet operations
- [ ] Added intro/login scenes (optional)
- [ ] Configured build settings
- [ ] Tested on target platform
- [ ] Verified analytics events
- [ ] Checked Nakama console

---

**Version:** 1.0  
**Last Updated:** November 16, 2025  
**Compatible With:** Unity 2021.3+, Nakama 3.x, Photon PUN2

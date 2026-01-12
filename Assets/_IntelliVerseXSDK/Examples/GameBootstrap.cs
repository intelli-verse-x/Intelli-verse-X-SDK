using UnityEngine;
using IntelliVerseX.Core;

/// <summary>
/// Example game bootstrap for IntelliVerse-X SDK integration.
/// This shows the minimal code needed to integrate any game with the SDK.
/// 
/// Steps to integrate your game:
/// 1. Copy this script to your game folder
/// 2. Create a GameConfig ScriptableObject (Assets → Create → IntelliVerse-X → Game Configuration)
/// 3. Set your game ID in the config
/// 4. Attach this script to a GameObject in your first scene
/// 5. Assign the config in the inspector
/// 6. Implement your game logic in StartGame()
/// 
/// That's it! Your game now has:
/// - Unified identity (Cognito + Nakama + Photon)
/// - Dual-wallet system
/// - Leaderboards
/// - Ads
/// - Multi-language support
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("SDK Configuration")]
    [Tooltip("Assign your game's config asset here")]
    [SerializeField] private IntelliVerseXConfig gameConfig;

    [Header("Debug")]
    [SerializeField] private bool showSDKInfo = true;

    private void Start()
    {
        // Load config from Resources if not assigned
        if (gameConfig == null)
        {
            // Try to load from Resources/IntelliVerseX/
            gameConfig = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/GameConfig");
            
            if (gameConfig == null)
            {
                Debug.LogError("[GameBootstrap] No config assigned! Create one in Assets → Create → IntelliVerse-X → Game Configuration");
                return;
            }
        }

        // Initialize SDK
        Debug.Log($"[GameBootstrap] Initializing SDK for {gameConfig.gameName}...");
        IntelliVerseXManager.Initialize(gameConfig);

        // Wait for SDK to be ready
        IntelliVerseXManager.Instance.OnReady += OnSDKReady;
        IntelliVerseXManager.Instance.OnError += OnSDKError;

        // Subscribe to identity events
        IntelliVerseXIdentity.OnIdentityUpdated += OnIdentityUpdated;
        IntelliVerseXIdentity.OnWalletBalanceChanged += OnWalletBalanceChanged;
    }

    private void OnSDKReady()
    {
        Debug.Log("[GameBootstrap] ✅ SDK Ready!");

        if (showSDKInfo)
        {
            PrintSDKInfo();
        }

        // Start your game
        StartGame();
    }

    private void OnSDKError(string error)
    {
        Debug.LogError($"[GameBootstrap] ❌ SDK Error: {error}");
    }

    private void OnIdentityUpdated()
    {
        Debug.Log($"[GameBootstrap] Identity updated - Username: {IntelliVerseXIdentity.Username}");
    }

    private void OnWalletBalanceChanged(int gameBalance, int globalBalance)
    {
        Debug.Log($"[GameBootstrap] Wallet updated - Game: {gameBalance}, Global: {globalBalance}");
    }

    /// <summary>
    /// Implement your game startup logic here
    /// </summary>
    private void StartGame()
    {
        Debug.Log("=== GAME STARTING ===");
        Debug.Log($"Welcome {IntelliVerseXIdentity.Username}!");
        Debug.Log($"Game Balance: {IntelliVerseXIdentity.GameWalletBalance}");
        Debug.Log($"Global Balance: {IntelliVerseXIdentity.GlobalWalletBalance}");
        
        // Example: Load main menu scene
        // SceneManager.LoadScene("MainMenu");
        
        // Example: Show login screen if guest
        // if (IntelliVerseXIdentity.IsGuest)
        // {
        //     ShowGuestWarning();
        // }
    }

    private void PrintSDKInfo()
    {
        Debug.Log("=== IntelliVerseX SDK Info ===");
        Debug.Log(IntelliVerseXManager.GetSDKInfo());
        Debug.Log($"Is Guest: {IntelliVerseXIdentity.IsGuest}");
        Debug.Log($"Has Cognito Account: {IntelliVerseXIdentity.HasCognitoAccount}");
        Debug.Log($"Has Wallet IDs: {IntelliVerseXIdentity.HasWalletIds}");
        Debug.Log($"Email: {IntelliVerseXIdentity.Email}");
        Debug.Log("==============================");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (IntelliVerseXManager.Instance != null)
        {
            IntelliVerseXManager.Instance.OnReady -= OnSDKReady;
            IntelliVerseXManager.Instance.OnError -= OnSDKError;
        }

        IntelliVerseXIdentity.OnIdentityUpdated -= OnIdentityUpdated;
        IntelliVerseXIdentity.OnWalletBalanceChanged -= OnWalletBalanceChanged;
    }

    // Example: Show guest account warning
    private void ShowGuestWarning()
    {
        if (IntelliVerseXIdentity.IsGuest && !IntelliVerseXIdentity.IsGuestExpired)
        {
            int daysRemaining = IntelliVerseXIdentity.GuestDaysRemaining;
            Debug.LogWarning($"Guest account expires in {daysRemaining} days. Create full account to keep progress!");
        }
        else if (IntelliVerseXIdentity.IsGuestExpired)
        {
            Debug.LogError("Guest account expired! Data will be lost.");
        }
    }

    // Example: Show rewarded ad
    public void ShowRewardedAd()
    {
        IntelliVerseX.Monetization.IVXAdsManager.ShowRewardedAd((success, reward) =>
        {
            if (success)
            {
                Debug.Log($"Rewarded ad success! Earned {reward} coins");
                // Give reward to player
            }
            else
            {
                Debug.LogWarning("Rewarded ad failed or cancelled");
            }
        });
    }

    // Example: Submit score to leaderboard using new IVXGLeaderboardManager
    public async void SubmitScore(long score)
    {
        var result = await IntelliVerseX.Games.Leaderboard.IVXGLeaderboardManager.SubmitScoreAsync((int)score);
        if (result != null && result.success)
        {
            Debug.Log($"Score {score} submitted successfully! Reward: {result.reward_earned}");
        }
        else
        {
            Debug.LogWarning($"Score submission failed: {result?.error ?? "Unknown error"}");
        }
    }

    // Example: Check if can buy item
    public bool CanBuyItem(int cost)
    {
        return IntelliVerseX.Backend.IVXWalletManager.CanAfford(cost);
    }
}

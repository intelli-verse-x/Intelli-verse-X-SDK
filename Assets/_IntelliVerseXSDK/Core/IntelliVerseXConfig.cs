using System;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Configuration ScriptableObject for IntelliVerse-X SDK.
    /// Create one per game with unique Game ID.
    /// 
    /// Menu: Assets → Create → IntelliVerse-X → Game Configuration
    /// 
    /// Example:
    ///   QuizVerseConfig.asset:
    ///     Game ID: "126bf539-dae2-4bcf-964d-316c0fa1f92b"
    ///     Game Name: "QuizVerse"
    ///   
    ///   TerminalRushConfig.asset:
    ///     Game ID: "abc-123-def-456"
    ///     Game Name: "Terminal Rush"
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "IntelliVerse-X/Game Configuration", order = 0)]
    public class IntelliVerseXConfig : ScriptableObject
    {
        /// <summary>
        /// SDK version string (semantic versioning)
        /// </summary>
        public static string version => "1.0.0";

        [Header("Game Identity")]
        [Tooltip("Unique game identifier (UUID). Get from IntelliVerse-X admin panel.")]
        public string gameId = "";
        
        [Tooltip("Human-readable game name")]
        public string gameName = "My Game";
        
        [Header("Backend Configuration")]
        [Tooltip("Backend services are hardcoded at SDK level. Nakama: nakama-rest.intelli-verse-x.ai:443 | Photon: fa2f730e-1c81-4d01-b11f-708680dcaf37 | Ads: IronSource/AdMob")]
        public bool useSharedBackend = true;
        
        // Legacy fields - kept for backward compatibility, but SDK uses hardcoded config
        [HideInInspector] public string nakamaScheme = "https";
        [HideInInspector] public string nakamaHost = "nakama-rest.intelli-verse-x.ai";
        [HideInInspector] public int nakamaPort = 443;
        [HideInInspector] public string nakamaServerKey = "defaultkey";
        
        [Header("Authentication")]
        [Tooltip("Cognito settings are hardcoded at SDK level (see IVXCognitoConfig.cs). All games use shared pool: aicart-user-pool")]
        public bool useCognitoAuthentication = true;
        
        [Header("Multiplayer (Optional)")]
        [Tooltip("Enable Photon multiplayer for this game")]
        public bool enablePhotonMultiplayer = true;
        
        [Tooltip("Photon App ID for this game. Leave empty to use shared IntelliVerse-X App ID: fa2f730e-1c81-4d01-b11f-708680dcaf37")]
        public string photonAppId = "";
        
        [Header("Features")]
        [Tooltip("Enable guest account support (4-day expiry)")]
        public bool enableGuestAccounts = true;
        
        [Tooltip("Enable auto-login with saved credentials")]
        public bool enableAutoLogin = true;
        
        [Tooltip("Enable leaderboard features")]
        public bool enableLeaderboards = true;
        
        [Tooltip("Enable wallet system (game + global)")]
        public bool enableWallets = true;
        
        [Tooltip("Enable ad monetization")]
        public bool enableAds = true;
        
        [Tooltip("Enable In-App Purchases (IAP)")]
        public bool enableIAP = true;
        
        [Tooltip("Enable Photon multiplayer")]
        public bool enableMultiplayer = false;
        
        [Header("Ads Configuration")]
        [Tooltip("Ad network configuration for this game")]
        public IVXAdsConfig adsConfig = new IVXAdsConfig();
        
        [Header("Localization")]
        [Tooltip("Default language for the game")]
        public SystemLanguage defaultLanguage = SystemLanguage.English;
        
        [Tooltip("Supported languages")]
        public SystemLanguage[] supportedLanguages = new SystemLanguage[]
        {
            SystemLanguage.English,
            SystemLanguage.Spanish,
            SystemLanguage.French,
            SystemLanguage.German,
            SystemLanguage.Portuguese,
            SystemLanguage.Chinese,
            SystemLanguage.Japanese,
            SystemLanguage.Korean,
            SystemLanguage.Russian,
            SystemLanguage.Italian,
            SystemLanguage.Dutch,
            SystemLanguage.Polish
        };
        
        [Header("Debug")]
        [Tooltip("Enable verbose logging")]
        public bool enableDebugLogs = false;
        
        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError("[IntelliVerseX] Game ID is required! Set gameId in IntelliVerseXConfig.");
                return false;
            }
            
            if (string.IsNullOrEmpty(gameName))
            {
                Debug.LogError("[IntelliVerseX] Game name is required!");
                return false;
            }
            
            if (string.IsNullOrEmpty(nakamaHost))
            {
                Debug.LogError("[IntelliVerseX] Nakama host is required!");
                return false;
            }
            
            // Validate secure connection for all builds (iOS requires HTTPS, Android 9+ enforces HTTPS)
            // Always enforce HTTPS to prevent "InvalidOperationException: Insecure connection not allowed"
            EnsureSecureConnection();
            
            // Ensure port matches scheme (443 for HTTPS)
            if (nakamaScheme == "https" && nakamaPort != 443)
            {
                Debug.LogWarning($"[IntelliVerseX] Using HTTPS with non-standard port {nakamaPort}. Consider using port 443.");
            }
            
            _secureConnectionValidated = true;
            return true;
        }
        
        // Track if secure connection has been validated to avoid repeated checks
        [System.NonSerialized] private bool _secureConnectionValidated = false;
        
        /// <summary>
        /// Ensure configuration uses secure connection.
        /// Call this before making any network requests to prevent "Insecure connection not allowed" errors.
        /// </summary>
        public void EnsureSecureConnection()
        {
            if (nakamaScheme != "https")
            {
                Debug.LogWarning("[IntelliVerseX] Enforcing HTTPS for secure connection.");
                nakamaScheme = "https";
            }

            if (nakamaScheme == "https" && (nakamaPort == 0 || nakamaPort == 80))
            {
                Debug.LogWarning("[IntelliVerseX] Adjusting Nakama port to 443 for HTTPS.");
                nakamaPort = 443;
            }
        }
        
        /// <summary>
        /// Get Nakama server URL
        /// </summary>
        public string GetNakamaUrl()
        {
            // Only validate once, not on every URL generation
            if (!_secureConnectionValidated)
            {
                EnsureSecureConnection();
                _secureConnectionValidated = true;
            }
            return $"{nakamaScheme}://{nakamaHost}:{nakamaPort}";
        }
    }
}

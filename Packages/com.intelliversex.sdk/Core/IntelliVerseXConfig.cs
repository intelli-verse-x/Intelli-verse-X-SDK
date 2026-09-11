using System;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Legacy serialized config fields (ads/Photon toggles on old assets).
    /// Do not create new assets — use <c>IVXBootstrapConfig</c>.
    /// </summary>
    [Obsolete("Use IVXBootstrapConfig with IVXBootstrap. Do not create new IntelliVerseXConfig assets.")]
    public class IntelliVerseXConfig : ScriptableObject
    {
        /// <summary>
        /// SDK version string (semantic versioning)
        /// </summary>
        public static string version => "6.0.0";

        [Header("Game Identity")]
        [Tooltip("Unique game identifier (UUID). Get from IntelliVerse-X admin panel.")]
        public string gameId = "";
        
        [Tooltip("Human-readable game name")]
        public string gameName = "My Game";
        
        [Header("Backend Configuration")]
        [Tooltip("Use the shared IntelliVerse-X cloud backend (Nakama host configured in IVXNakamaConfig / Bootstrap). Do not embed third-party App IDs here.")]
        public bool useSharedBackend = true;
        
        // Legacy fields - kept for backward compatibility, but SDK uses hardcoded config
        [HideInInspector] public string nakamaScheme = "https";
        [HideInInspector] public string nakamaHost = "nakama-rest.intelli-verse-x.ai";
        [HideInInspector] public int nakamaPort = 443;
        [HideInInspector] public string nakamaServerKey = "";
        
        [Header("Authentication")]
        [Tooltip("Cognito settings are hardcoded at SDK level (see IVXCognitoConfig.cs). All games use shared pool: aicart-user-pool")]
        public bool useCognitoAuthentication = true;
        
        [Header("Multiplayer (Optional)")]
        [Tooltip("Enable Photon multiplayer for this game")]
        public bool enablePhotonMultiplayer = true;
        
        [Tooltip("Photon App ID for this game. Leave empty and set via Bootstrap / keys.json — never commit shared App IDs.")]
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

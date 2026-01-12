using System;
using UnityEngine;
using IntelliVerseX.Multiplayer;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Main coordinator for IntelliVerse-X SDK.
    /// Initializes all modules and provides unified entry point.
    /// 
    /// Usage:
    ///   IntelliVerseXManager.Initialize(config);
    ///   IntelliVerseXManager.Instance.OnReady += OnSDKReady;
    ///   
    ///   void OnSDKReady() {
    ///       // SDK is ready, start game
    ///   }
    /// </summary>
    public class IntelliVerseXManager : MonoBehaviour
    {
        public const string SDKVersion = "1.0.0";

        private static IntelliVerseXManager _instance;
        private static IntelliVerseXConfig _config;
        private static bool _isInitialized = false;
        private static IVXMultiplayerManager _multiplayerManager;

        // Events
        public event Action OnReady;
        public event Action<string> OnError;

        public static IntelliVerseXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("[IntelliVerseX] SDK not initialized! Call IntelliVerseXManager.Initialize(config) first.");
                }
                return _instance;
            }
        }

        public static IntelliVerseXConfig Config => _config;
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Access to multiplayer manager for Photon integration
        /// </summary>
        public IVXMultiplayerManager Multiplayer
        {
            get
            {
                if (_multiplayerManager == null)
                {
                    _multiplayerManager = new IVXMultiplayerManager();
                    _multiplayerManager.Initialize();
                }
                return _multiplayerManager;
            }
        }

        /// <summary>
        /// Initialize SDK with configuration
        /// </summary>
        public static void Initialize(IntelliVerseXConfig config)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IntelliVerseX] SDK already initialized!");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[IntelliVerseX] Config is null!");
                return;
            }

            if (!config.IsValid())
            {
                Debug.LogError("[IntelliVerseX] Invalid configuration!");
                return;
            }

            _config = config;

            // Create manager instance if doesn't exist
            if (_instance == null)
            {
                var go = new GameObject("IntelliVerseXManager");
                _instance = go.AddComponent<IntelliVerseXManager>();
                DontDestroyOnLoad(go);
            }

            _instance.InitializeModules();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void InitializeModules()
        {
            Debug.Log($"[IntelliVerseX] Initializing SDK v{SDKVersion} for game: {_config.gameName} ({_config.gameId})");

            try
            {
                // 1. Initialize Core (Identity)
                IntelliVerseXIdentity.Initialize(_config);
                Debug.Log("[IntelliVerseX] Core module initialized");

                // 2. Initialize Authentication (Cognito config is hardcoded at SDK level)
                if (_config.useCognitoAuthentication)
                {
                    // IVXAuthentication.Initialize(_config);
                    Debug.Log("[IntelliVerseX] Authentication module ready");
                }

                // 3. Initialize Backend (Nakama)
                if (!string.IsNullOrEmpty(_config.nakamaHost))
                {
                    // IVXNakamaClient.Initialize(_config);
                    Debug.Log("[IntelliVerseX] Backend module ready");
                }

                // 4. Initialize Multiplayer (Photon config is hardcoded at SDK level)
                if (_config.enablePhotonMultiplayer)
                {
                    _multiplayerManager = new IVXMultiplayerManager();
                    _multiplayerManager.Initialize();
                    Debug.Log("[IntelliVerseX] Multiplayer module initialized");
                }

                // 5. Initialize Monetization (Ads)
                if (_config.enableAds)
                {
                    // Use reflection to avoid cyclic dependency with Monetization package
                    var adsManagerType = System.Type.GetType("IntelliVerseX.Monetization.IVXAdsManager, IntelliVerseX.Monetization");
                    if (adsManagerType != null)
                    {
                        // Find the Initialize method with matching signature: Initialize(IntelliVerseXConfig, IVXAdNetwork = None)
                        var initMethod = adsManagerType.GetMethod("Initialize", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                            null,
                            new System.Type[] { typeof(IntelliVerseXConfig), typeof(int) }, // IVXAdNetwork is enum (int)
                            null);
                        
                        if (initMethod != null)
                        {
                            // Pass default value (0 = IVXAdNetwork.None) for second parameter
                            initMethod.Invoke(null, new object[] { _config, 0 });
                            Debug.Log("[IntelliVerseX] Ads module initialized");
                        }
                        else
                        {
                            Debug.LogWarning("[IntelliVerseX] IVXAdsManager.Initialize method not found with expected signature.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[IntelliVerseX] Monetization package not found. Ads disabled.");
                    }
                }

                // 6. Initialize IAP
                // IVXIAPManager will be initialized per-game with product catalog
                Debug.Log("[IntelliVerseX] IAP module ready");

                // 7. Initialize Localization
                var languageManagerType = System.Type.GetType("IntelliVerseX.Localization.IVXLanguageManager, IntelliVerseX.Localization");
                if (languageManagerType != null)
                {
                    // Find the Initialize method with exact signature: Initialize(IntelliVerseXConfig)
                    var initMethod = languageManagerType.GetMethod("Initialize", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                        null,
                        new System.Type[] { typeof(IntelliVerseXConfig) },
                        null);
                    
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, new object[] { _config });
                        Debug.Log("[IntelliVerseX] Localization module initialized");
                    }
                    else
                    {
                        Debug.LogWarning("[IntelliVerseX] IVXLanguageManager.Initialize method not found with expected signature.");
                    }
                }
                else
                {
                    Debug.LogWarning("[IntelliVerseX] Localization package not found.");
                }

                _isInitialized = true;
                Debug.Log($"[IntelliVerseX] SDK initialization complete! User: {IntelliVerseXIdentity.Username}");

                // Notify listeners
                OnReady?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IntelliVerseX] SDK initialization failed: {ex.Message}\n{ex.StackTrace}");
                OnError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Get SDK info
        /// </summary>
        public static string GetSDKInfo()
        {
            return $"IntelliVerseX SDK v{SDKVersion}\n" +
                   $"Game: {_config?.gameName ?? "Unknown"}\n" +
                   $"Game ID: {_config?.gameId ?? "Unknown"}\n" +
                   $"User: {IntelliVerseXIdentity.Username}\n" +
                   $"Device ID: {IntelliVerseXIdentity.DeviceId}\n" +
                   $"Initialized: {_isInitialized}";
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _isInitialized = false;
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using IntelliVerseX.Identity;
using UnityEngine;

namespace IntelliVerseX.Bootstrap
{
    /// <summary>
    /// One-drop bootstrap for the IntelliVerseX SDK.
    /// Attach to a GameObject, assign an <see cref="IVXBootstrapConfig"/>,
    /// and call <see cref="InitializeAsync"/> — or enable <c>AutoInitialize</c>
    /// to start automatically in <c>Start()</c>.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://intelli-verse-x.github.io/Intelli-verse-X-SDK/getting-started/quickstart/")]
    public sealed class IVXBootstrap : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("Drag the IVXBootstrapConfig ScriptableObject here")]
        [SerializeField] private IVXBootstrapConfig _config;

        [Tooltip("Automatically initialize all systems on Start()")]
        [SerializeField] private bool _autoInitialize = true;

        #endregion

        #region Private Fields

        private static IVXBootstrap _instance;
        private bool _isInitialized;
        private bool _isInitializing;
        private string _userId;
        private string _userName;
        private string _authToken;

        #if INTELLIVERSEX_HAS_NAKAMA
        private Nakama.IClient _nakamaClient;
        private Nakama.ISession _nakamaSession;
        private Nakama.ISocket _socket;
        #endif

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXBootstrap Instance => _instance;
        /// <summary>Whether bootstrap initialization has completed successfully.</summary>
        public bool IsInitialized => _isInitialized;
        /// <summary>Whether initialization is currently in progress.</summary>
        public bool IsInitializing => _isInitializing;
        /// <summary>The bootstrap configuration.</summary>
        public IVXBootstrapConfig Config => _config;
        /// <summary>The authenticated user ID (after init).</summary>
        public string UserId => _userId;
        /// <summary>The authenticated user name (after init).</summary>
        public string UserName => _userName;
        /// <summary>The authenticated auth token (after init).</summary>
        public string AuthToken => _authToken;
        /// <summary>The Game ID from the config (set during init).</summary>
        public string GameId => _config != null ? _config.GameId : "";

        #endregion

        #region Events

        /// <summary>Fired when bootstrap completes. Bool = success.</summary>
        public event Action<bool> OnBootstrapComplete;
        /// <summary>Fired when a specific module initializes. String = module name.</summary>
        public event Action<string> OnModuleInitialized;
        /// <summary>Fired if a module fails. (module name, error message).</summary>
        public event Action<string, string> OnModuleFailed;

        #endregion

        #region Unity Lifecycle

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

        private async void Start()
        {
            try
            {
                if (_autoInitialize)
                {
                    await InitializeAsync();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[IVXBootstrap] Initialization failed: {e.Message}\n{e.StackTrace}");
                OnBootstrapComplete?.Invoke(false);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Shutdown();
                _instance = null;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialize all enabled SDK modules in dependency order.
        /// Safe to call multiple times — returns immediately if already initialized.
        /// </summary>
        /// <returns>True if all enabled modules initialized successfully.</returns>
        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return true;
            if (_isInitializing) return false;
            _isInitializing = true;

            if (_config == null)
            {
                Debug.LogError("[IVXBootstrap] No IVXBootstrapConfig assigned. Drag one onto the Inspector.");
                _isInitializing = false;
                OnBootstrapComplete?.Invoke(false);
                return false;
            }

            _config.Validate();

            if (!string.IsNullOrWhiteSpace(_config.GameId))
            {
                IVXURLs.GameId = _config.GameId;
                Log($"Game ID set: {_config.GameId}");
            }
            else
            {
                Debug.LogWarning("[IVXBootstrap] Game ID is empty in config. Backend calls will use the fallback default. " +
                                 "Get your Game ID from https://intelli-verse-x.ai/developers");
            }

            Log("Starting SDK bootstrap...");
            var success = true;

            // ── Phase 1: Platform ──
            if (_config.EnablePlatform) InitPlatform();

            // ── Phase 2: Backend (Nakama auth) ──
            if (_config.AutoDeviceAuth)
            {
                success = await InitBackendAsync();
                if (!success)
                {
                    Log("Backend auth failed — continuing in offline mode");
                }
            }

            // ── Phase 3: Hiro + Satori (need Nakama session) ──
            if (success)
            {
                if (_config.EnableHiro) InitHiro();
                if (_config.EnableSatori) InitSatori();
            }

            // ── Phase 4: Discord ──
            if (_config.EnableDiscord) InitDiscord();

            // ── Phase 5: AI ──
            if (_config.EnableAI) InitAI();

            // ── Phase 6: Multiplayer (self-initializing, just touch the singleton) ──
            if (_config.EnableMultiplayer) InitMultiplayer();

            _isInitialized = true;
            _isInitializing = false;
            Log($"Bootstrap complete. User: {_userId ?? "(offline)"}");
            OnBootstrapComplete?.Invoke(true);
            return true;
        }

        /// <summary>
        /// Manually set authentication details if you handle auth yourself.
        /// Call this BEFORE <see cref="InitializeAsync"/> if <c>AutoDeviceAuth</c> is disabled.
        /// </summary>
        public void SetAuth(string userId, string userName, string authToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning("[IVXBootstrap] SetAuth called with null/empty userId.");
                return;
            }
            _userId = userId;
            _userName = userName;
            _authToken = authToken;
        }

        /// <summary>
        /// Shut down all SDK modules cleanly. Call from OnApplicationQuit.
        /// </summary>
        public void Shutdown()
        {
            Log("Shutting down SDK...");

            #if INTELLIVERSEX_HAS_DISCORD
            try { IntelliVerseX.Discord.IVXDiscordManager.Instance?.Shutdown(); }
            catch (Exception e) { Debug.LogWarning($"[IVXBootstrap] Discord shutdown: {e.Message}"); }
            #endif

            #if INTELLIVERSEX_HAS_NAKAMA
            if (_socket != null) { _socket.CloseAsync(); _socket = null; }
            #endif

            _isInitialized = false;
            Debug.Log("[IVXBootstrap] SDK shutdown complete.");
        }

        #endregion

        #region Private — Module Initialization

        private void InitPlatform()
        {
            try
            {
                Log("Initializing Platform Optimizer...");
                var existing = FindFirstObjectByType<IntelliVerseX.Platform.IVXPlatformOptimizer>();
                if (existing == null)
                {
                    gameObject.AddComponent<IntelliVerseX.Platform.IVXPlatformOptimizer>();
                }
                EmitModuleReady("Platform");
            }
            catch (Exception e) { EmitModuleFail("Platform", e); }
        }

        private async Task<bool> InitBackendAsync()
        {
            #if INTELLIVERSEX_HAS_NAKAMA
            try
            {
                Log($"Connecting to Nakama at {_config.ServerHost}:{_config.ServerPort}...");
                var scheme = _config.UseSSL ? "https" : "http";
                var client = new Nakama.Client(scheme, _config.ServerHost, _config.ServerPort, _config.ServerKey);

                var deviceId = SystemInfo.deviceUniqueIdentifier;
                if (deviceId == SystemInfo.unsupportedIdentifier)
                    deviceId = PlayerPrefs.GetString("IVX_DeviceId", Guid.NewGuid().ToString());
                PlayerPrefs.SetString("IVX_DeviceId", deviceId);

                Nakama.ISession session = null;

                if (_config.PersistSession)
                {
                    var savedToken = PlayerPrefs.GetString("IVX_SessionToken", "");
                    if (!string.IsNullOrEmpty(savedToken))
                    {
                        try
                        {
                            session = Nakama.Session.Restore(savedToken);
                            if (session.IsExpired)
                            {
                                Log("Saved session expired, re-authenticating...");
                                session = null;
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"[IVXBootstrap] Session restore failed: {e.Message}");
                            session = null;
                        }
                    }
                }

                if (session == null)
                {
                    session = await client.AuthenticateDeviceAsync(deviceId);
                    if (_config.PersistSession)
                        PlayerPrefs.SetString("IVX_SessionToken", session.AuthToken);
                }

                _nakamaClient = client;
                _nakamaSession = session;
                _userId = session.UserId;
                _userName = session.Username;
                _authToken = session.AuthToken;

                PlayerPrefs.Save();
                EmitModuleReady("Backend");
                Log($"Authenticated: {_userName} ({_userId})");
                return true;
            }
            catch (Exception e)
            {
                EmitModuleFail("Backend", e);
                return false;
            }
            #else
            Log("Nakama not installed — skipping backend auth. Add com.heroiclabs.nakama-unity to enable.");
            _userId = _userId ?? "offline-" + SystemInfo.deviceUniqueIdentifier.Substring(0, 8);
            _userName = _userName ?? "Player";
            await Task.CompletedTask;
            return false;
            #endif
        }

        private void InitHiro()
        {
            #if INTELLIVERSEX_HAS_NAKAMA
            try
            {
                Log("Initializing Hiro (33 live-ops systems)...");
                var coord = IntelliVerseX.Hiro.IVXHiroCoordinator.Instance;
                if (coord == null)
                {
                    var go = new GameObject("IVX_HiroCoordinator");
                    go.transform.SetParent(transform);
                    coord = go.AddComponent<IntelliVerseX.Hiro.IVXHiroCoordinator>();
                }
                coord.InitializeSystems(_nakamaClient, _nakamaSession);
                EmitModuleReady("Hiro");
            }
            catch (Exception e) { EmitModuleFail("Hiro", e); }
            #else
            Log("Hiro skipped (Nakama not installed).");
            #endif
        }

        private void InitSatori()
        {
            #if INTELLIVERSEX_HAS_NAKAMA
            try
            {
                Log("Initializing Satori analytics...");
                var satori = IntelliVerseX.Satori.IVXSatoriClient.Instance;
                if (satori == null)
                {
                    var go = new GameObject("IVX_SatoriClient");
                    go.transform.SetParent(transform);
                    satori = go.AddComponent<IntelliVerseX.Satori.IVXSatoriClient>();
                }
                satori.Initialize(_nakamaClient, _nakamaSession);
                EmitModuleReady("Satori");
            }
            catch (Exception e) { EmitModuleFail("Satori", e); }
            #else
            Log("Satori skipped (Nakama not installed).");
            #endif
        }

        private void InitDiscord()
        {
            try
            {
                Log("Initializing Discord Social SDK...");
                var mgr = IntelliVerseX.Discord.IVXDiscordManager.Instance;
                if (mgr == null)
                {
                    var go = new GameObject("IVX_Discord");
                    go.transform.SetParent(transform);
                    mgr = go.AddComponent<IntelliVerseX.Discord.IVXDiscordManager>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordPresence>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordFriends>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordMessages>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordLobby>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordVoice>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordInvites>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordLinkedChannels>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordModeration>();
                    go.AddComponent<IntelliVerseX.Discord.IVXDiscordDebug>();
                }
                var cfg = _config.DiscordConfig as IntelliVerseX.Discord.IVXDiscordConfig;
                mgr.Initialize(cfg);
                EmitModuleReady("Discord");
            }
            catch (Exception e) { EmitModuleFail("Discord", e); }
        }

        private void InitAI()
        {
            try
            {
                Log("Initializing AI stack (7 subsystems)...");
                var aiCfg = _config.AIConfig as IntelliVerseX.AI.IVXAIConfig;
                if (aiCfg == null)
                {
                    Log("No AI config assigned — AI subsystems will not initialize.");
                    return;
                }

                EnsureAISingleton<IntelliVerseX.AI.IVXAISessionManager>("IVX_AISessionManager", go => go.AddComponent<AudioSource>());
                IntelliVerseX.AI.IVXAISessionManager.Instance?.Initialize(_userId, _userName, _authToken);

                EnsureAISingleton<IntelliVerseX.AI.IVXAINPCDialogManager>("IVX_AINPCDialog");
                IntelliVerseX.AI.IVXAINPCDialogManager.Instance?.Initialize(aiCfg);
                IntelliVerseX.AI.IVXAINPCDialogManager.Instance?.SetAuthToken(_authToken);

                EnsureAISingleton<IntelliVerseX.AI.IVXAIAssistant>("IVX_AIAssistant");
                IntelliVerseX.AI.IVXAIAssistant.Instance?.Initialize(aiCfg);
                IntelliVerseX.AI.IVXAIAssistant.Instance?.SetAuthToken(_authToken);

                EnsureAISingleton<IntelliVerseX.AI.IVXAIModerator>("IVX_AIModerator");
                IntelliVerseX.AI.IVXAIModerator.Instance?.Initialize(aiCfg);

                EnsureAISingleton<IntelliVerseX.AI.IVXAIContentGenerator>("IVX_AIContentGen");
                IntelliVerseX.AI.IVXAIContentGenerator.Instance?.Initialize(aiCfg);

                EnsureAISingleton<IntelliVerseX.AI.IVXAIProfiler>("IVX_AIProfiler");
                IntelliVerseX.AI.IVXAIProfiler.Instance?.Initialize(aiCfg, _userId);

                EnsureAISingleton<IntelliVerseX.AI.IVXAIVoiceServices>("IVX_AIVoiceServices");
                IntelliVerseX.AI.IVXAIVoiceServices.Instance?.Initialize(aiCfg);

                EmitModuleReady("AI");
            }
            catch (Exception e) { EmitModuleFail("AI", e); }
        }

        private void InitMultiplayer()
        {
            try
            {
                Log("Initializing Multiplayer...");
                var _ = IntelliVerseX.GameModes.IVXGameModeManager.Instance;
                EmitModuleReady("Multiplayer");
            }
            catch (Exception e) { EmitModuleFail("Multiplayer", e); }
        }

        #endregion

        #region Private Helpers

        private void EnsureAISingleton<T>(string goName, Action<GameObject> extras = null) where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() != null) return;
            var go = new GameObject(goName);
            go.transform.SetParent(transform);
            extras?.Invoke(go);
            go.AddComponent<T>();
        }

        private void Log(string msg)
        {
            if (_config != null && _config.DebugLogging)
                Debug.Log($"[IVXBootstrap] {msg}");
        }

        private void EmitModuleReady(string moduleName)
        {
            Log($"  ✓ {moduleName} ready");
            OnModuleInitialized?.Invoke(moduleName);
        }

        private void EmitModuleFail(string moduleName, Exception e)
        {
            Debug.LogError($"[IVXBootstrap] {moduleName} failed: {e.Message}");
            OnModuleFailed?.Invoke(moduleName, e.Message);
        }

        #endregion
    }
}

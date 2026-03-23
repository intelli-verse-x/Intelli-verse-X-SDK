// IVXNakamaManager.cs
// Base Nakama integration manager for IntelliVerse-X SDK
// Provides core functionality for all games: Authentication, Leaderboards, Scores, Wallets
// Games can extend this class to add game-specific features

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using Newtonsoft.Json;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

namespace IntelliVerseX.Backend
{
    /// <summary>
    /// Base Nakama manager that all games can extend
    /// Provides: Authentication, Identity Sync, Score Submission, Leaderboards, Wallets, Adaptive Rewards
    /// 
    /// Usage:
    /// 1. Extend this class in your game namespace
    /// 2. Override GetLogPrefix() to customize logging
    /// 3. Add game-specific methods as needed
    /// 4. Call base methods for core functionality
    /// 
    /// Example:
    ///   public class MyGameNakamaManager : IVXNakamaManager
    ///   {
    ///       protected override string GetLogPrefix() => "[MYGAME]";
    ///   }
    /// </summary>
    public abstract class IVXNakamaManager : MonoBehaviour
    {
        [Header("SDK Configuration")]
        [Tooltip("Leave empty to auto-load from Resources/IntelliVerseX/<GameName>Config")]
        [SerializeField] protected IntelliVerseXConfig sdkConfig;

        // Config values loaded from SDK
        protected string _scheme;
        protected string _host;
        protected int _port;
        protected string _serverKey;
        protected string _gameId;

        /// <summary>Public accessor for game ID</summary>
        public string GameId => _gameId;

        // RPC endpoint names (MUST match server)
        protected const string RPC_CREATE_OR_SYNC_USER = "create_or_sync_user";
        protected const string RPC_SUBMIT_SCORE_AND_SYNC = "submit_score_and_sync";
        protected const string RPC_GET_ALL_LEADERBOARDS = "get_all_leaderboards";
        protected const string RPC_CALCULATE_SCORE_REWARD = "calculate_score_reward";
        protected const string RPC_UPDATE_GAME_REWARD_CONFIG = "update_game_reward_config";
        protected const string RPC_UPDATE_WALLET_BALANCE = "update_wallet_balance";
        protected const string RPC_GET_WALLET_BALANCE = "get_wallet_balance";

        // Session storage keys
        protected const string PREF_REFRESH_TOKEN = "nakama_refresh_token";
        protected const string PREF_AUTH_TOKEN = "nakama_auth_token";

        // Nakama client state
        protected IClient _client;
        protected ISession _session;
        protected ISocket _socket;
        protected bool _isInitialized = false;

        // Adaptive rewards
        protected int _currentWinStreak = 0;

        [Header("Initialization")]
        [SerializeField] public bool _initializeOnStart = false;

        // Public accessors
        public IClient Client => _client;
        public ISession Session => _session;
        public bool IsInitialized => _isInitialized;
        public virtual int CurrentWinStreak => _currentWinStreak;

        // Events
        public event Action<bool> OnInitialized;
        public event Action<IVXAllLeaderboardsResponse> OnLeaderboardsUpdated;
        public event Action<bool> OnScoreSubmitted;
        public event Action<IVXCalculateScoreRewardResponse> OnRewardCalculated;

        /// <summary>Override this to customize log prefix for your game</summary>
        protected virtual string GetLogPrefix() => "[IVX-NAKAMA]";

        protected virtual void Awake()
        {
            LoadConfig();
        }

        protected virtual async void Start()
        {
            if (_initializeOnStart)
            {
                await InitializeAsync();
            }
        }

        /// <summary>
        /// Load SDK configuration
        /// Override to customize config loading behavior
        /// </summary>
        protected virtual void LoadConfig()
        {
            if (sdkConfig == null)
            {
                // Try multiple config paths in order of preference
                string[] configPaths = new string[]
                {
                    GetConfigResourcePath(),           // Use configured path first
                    "IntelliVerseX/QuizVerseConfig",   // QuizVerse-specific config
                    "IntelliVerseX/GameConfig",        // Default SDK config path
                    "QuizVerseConfig"                  // Fallback at Resources root
                };

                foreach (var path in configPaths)
                {
                    sdkConfig = Resources.Load<IntelliVerseXConfig>(path);
                    if (sdkConfig != null)
                    {
                        Debug.Log($"{GetLogPrefix()} Config loaded from Resources/{path}");
                        break;
                    }
                }

                if (sdkConfig == null)
                {
                    Debug.LogError($"{GetLogPrefix()} Config not found at any expected path. Checked: {string.Join(", ", configPaths)}");
                    // Set secure defaults from IVXNakamaConfig to prevent "Insecure connection not allowed" error
                    _scheme = IVXNakamaConfig.PROTOCOL;
                    _host = IVXNakamaConfig.HOST;
                    _port = IVXNakamaConfig.PORT;
                    _serverKey = IVXNakamaConfig.SERVER_KEY;
                    _gameId = "";
                    Debug.LogWarning($"{GetLogPrefix()} Using IVXNakamaConfig secure defaults for Nakama connection.");
                    return;
                }
            }

            // Load config values with secure fallbacks from IVXNakamaConfig
            _scheme = !string.IsNullOrWhiteSpace(sdkConfig.nakamaScheme) ? sdkConfig.nakamaScheme : IVXNakamaConfig.PROTOCOL;
            _host = !string.IsNullOrWhiteSpace(sdkConfig.nakamaHost) ? sdkConfig.nakamaHost : IVXNakamaConfig.HOST;
            _port = sdkConfig.nakamaPort > 0 ? sdkConfig.nakamaPort : IVXNakamaConfig.PORT;
            _serverKey = !string.IsNullOrWhiteSpace(sdkConfig.nakamaServerKey) ? sdkConfig.nakamaServerKey : IVXNakamaConfig.SERVER_KEY;
            _gameId = sdkConfig.gameId;
            
            // Ensure secure connection to prevent "Insecure connection not allowed" error
            sdkConfig.EnsureSecureConnection();
            if (_scheme != "https")
            {
                Debug.LogWarning($"{GetLogPrefix()} Enforcing HTTPS for secure connection.");
                _scheme = "https";
            }

            if (_scheme == "https" && (_port == 0 || _port == 80))
            {
                _port = 443;
            }

            Debug.Log($"{GetLogPrefix()} Config loaded: {_gameId} @ {_scheme}://{_host}:{_port}");
        }

        /// <summary>
        /// Override to specify custom config resource path
        /// Default: "IntelliVerseX/GameConfig"
        /// </summary>
        protected virtual string GetConfigResourcePath()
        {
            return "IntelliVerseX/GameConfig";
        }

        // ========================================================================
        // INITIALIZATION & AUTHENTICATION
        // ========================================================================

        /// <summary>
        /// Initialize Nakama client and authenticate
        /// </summary>
        public virtual async Task<bool> InitializeAsync()
        {
            if (_isInitialized)
            {
                Debug.Log($"{GetLogPrefix()} Already initialized");
                return true;
            }

            try
            {
                Debug.Log($"{GetLogPrefix()} Initializing Nakama client...");

                if (string.IsNullOrEmpty(_gameId))
                {
                    Debug.LogError($"{GetLogPrefix()} SDK config not loaded!");
                    OnInitialized?.Invoke(false);
                    return false;
                }

                // Create client using SDK config
                _client = new Client(_scheme, _host, _port, _serverKey, UnityWebRequestAdapter.Instance);

                // Authenticate and sync identity (this is where wallet ids are set via SyncUserIdentity)
                _session = await AuthenticateAndSyncIdentity();

                if (_session == null)
                {
                    Debug.LogError($"{GetLogPrefix()} Failed to authenticate");
                    OnInitialized?.Invoke(false);
                    return false;
                }

                _isInitialized = true;
                Debug.Log($"{GetLogPrefix()} ✓ Initialized successfully");

                // NOTE:
                // We do NOT hard-reference IVXNUserRuntime here.
                // IVXNUserRuntime itself subscribes to IVXNManager.OnInitialized
                // and will call RefreshFromGlobals() when this event fires.
                OnInitialized?.Invoke(true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Initialization failed: {ex.Message}");
                OnInitialized?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// Authenticate with Nakama using unified identity
        /// Handles session restore and device authentication
        /// </summary>
        protected virtual async Task<ISession> AuthenticateAndSyncIdentity()
        {
            try
            {
                // 1) Try to restore previous Nakama session
                var authToken = PlayerPrefs.GetString(PREF_AUTH_TOKEN, "");
                if (!string.IsNullOrEmpty(authToken))
                {
                    var restoredSession = global::Nakama.Session.Restore(authToken);

                    if (restoredSession != null && !restoredSession.IsExpired)
                    {
                        Debug.Log($"{GetLogPrefix()} ✓ Session restored");
                        return restoredSession;
                    }

                    Debug.Log($"{GetLogPrefix()} Session expired, re-authenticating...");
                }

                // 2) Build identity from IntelliVerseX (must be set by login / guest)
                var user = IntelliVerseXIdentity.CurrentUser;
                if (user == null)
                {
                    Debug.LogError($"{GetLogPrefix()} No IntelliVerseX user loaded. " +
                                   "Make sure login/guest has completed BEFORE initializing Nakama.");
                    return null;
                }

                string nakamaUserId = IntelliVerseXIdentity.NakamaUserId; // from login response
                if (string.IsNullOrEmpty(nakamaUserId))
                {
                    Debug.LogError($"{GetLogPrefix()} NakamaUserId is empty. " +
                                   "Check that UpdateCognitoIdentity / guest identity is being called correctly.");
                    return null;
                }

                string deviceId = IntelliVerseXIdentity.DeviceId;
                string username = GetUsername(user);

                Debug.Log($"{GetLogPrefix()} Authenticating with Nakama:");
                Debug.Log($"{GetLogPrefix()}   NakamaUserId: {nakamaUserId}");
                Debug.Log($"{GetLogPrefix()}   Device ID:    {deviceId}");
                Debug.Log($"{GetLogPrefix()}   Username:     {username}");
                Debug.Log($"{GetLogPrefix()}   Game ID:      {_gameId}");

                // 3) Use CUSTOM auth with your backend user id
                var newSession = await _client.AuthenticateCustomAsync(
                    nakamaUserId,
                    username,
                    create: true
                );

                Debug.Log($"{GetLogPrefix()} ✓ Authenticated (Nakama User ID: {newSession.UserId})");

                // 4) Sync user identity with your Nakama RPC
                await SyncUserIdentity(newSession, username, nakamaUserId, deviceId, _gameId);

                // 5) Persist Nakama tokens
                SaveSession(newSession);

                return newSession;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Authentication failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get username from user identity
        /// Override to customize username generation logic
        /// </summary>
        protected virtual string GetUsername(IntelliVerseXUser user)
        {
            string username = user.Username;

            if (string.IsNullOrEmpty(username))
            {
                if (!string.IsNullOrEmpty(user.IdpUsername))
                {
                    username = user.IdpUsername;
                }
                else if (!string.IsNullOrEmpty(user.Email))
                {
                    username = user.Email.Split('@')[0];
                }
                else
                {
                    username = "Player_" + user.DeviceId.Substring(0, 8);
                }
                Debug.LogWarning($"{GetLogPrefix()} No username set, using: {username}");
            }

            return username;
        }

        /// <summary>
        /// Sync user identity with Nakama server
        /// Calls create_or_sync_user RPC to create wallets and store user data
        /// </summary>
        protected virtual async Task SyncUserIdentity(
            ISession session,
            string username,
            string platformUserId,   // your Cognito/Game/Device user id
            string deviceId,
            string gameId)
        {
            try
            {
                Debug.Log($"{GetLogPrefix()} Syncing user identity...");

                var payload = new
                {
                    username = username,
                    user_id = session.UserId,     // real Nakama user id (UUID)
                    platform_user_id = platformUserId,
                    device_id = deviceId,
                    game_id = gameId
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var rpcResponse = await _client.RpcAsync(session, RPC_CREATE_OR_SYNC_USER, jsonPayload);

                var response = JsonConvert.DeserializeObject<IVXCreateOrSyncUserResponse>(rpcResponse.Payload);

                if (response != null && response.success)
                {
                    Debug.Log($"{GetLogPrefix()} ✓ Identity {(response.created ? "created" : "synced")}");
                    Debug.Log($"{GetLogPrefix()}   Username:      {response.username}");
                    Debug.Log($"{GetLogPrefix()}   Game Wallet:   {response.wallet_id}");
                    Debug.Log($"{GetLogPrefix()}   Global Wallet: {response.global_wallet_id}");

                    if (!string.IsNullOrEmpty(response.wallet_id))
                    {
                        IntelliVerseXIdentity.SetWalletIds(response.wallet_id, response.global_wallet_id);
                    }
                }
                else
                {
                    Debug.LogError($"{GetLogPrefix()} Identity sync failed: {response?.error ?? "Unknown error"}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Identity sync error: {ex.Message}");
            }
        }

        protected virtual void SaveSession(ISession session)
        {
            PlayerPrefs.SetString(PREF_AUTH_TOKEN, session.AuthToken);
            PlayerPrefs.SetString(PREF_REFRESH_TOKEN, session.RefreshToken);
            PlayerPrefs.Save();
            Debug.Log($"{GetLogPrefix()} Session saved");
        }

        /// <summary>
        /// Ensure session is valid, re-authenticate if needed
        /// </summary>
        protected virtual async Task<bool> EnsureValidSession()
        {
            if (!_isInitialized || _client == null || _session == null)
            {
                Debug.LogError($"{GetLogPrefix()} Not initialized");
                return false;
            }

            if (_session.IsExpired)
            {
                Debug.LogWarning($"{GetLogPrefix()} Session expired, re-authenticating...");
                _session = await AuthenticateAndSyncIdentity();
                return _session != null;
            }

            return true;
        }

        // ========================================================================
        // SCORE SUBMISSION
        // ========================================================================

        public virtual async Task<IVXScoreSubmissionResponse> SubmitScore(
            int score,
            int subscore = 0,
            Dictionary<string, string> metadata = null)
        {
            if (!await EnsureValidSession())
                return null;

            try
            {
                var user = IntelliVerseXIdentity.CurrentUser;

                var payload = new
                {
                    user_id = IntelliVerseXIdentity.NakamaUserId,
                    username = user.Username,
                    device_id = user.DeviceId,
                    game_id = _gameId,
                    score = score,
                    subscore = subscore,
                    current_streak = _currentWinStreak,
                    metadata = metadata
                };

                Debug.Log($"{GetLogPrefix()} Submitting score: {score} (streak: {_currentWinStreak})");

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var rpcResponse = await _client.RpcAsync(_session, RPC_SUBMIT_SCORE_AND_SYNC, jsonPayload);

                var response = JsonConvert.DeserializeObject<IVXScoreSubmissionResponse>(rpcResponse.Payload);

                if (response != null && response.success)
                {
                    _currentWinStreak++;
                    LogScoreSubmissionSuccess(response);
                    OnScoreSubmitted?.Invoke(true);
                    return response;
                }

                _currentWinStreak = 0;
                Debug.LogError($"{GetLogPrefix()} Score submission failed: {response?.error ?? "Unknown error"}");
                OnScoreSubmitted?.Invoke(false);
                return response;
            }
            catch (Exception ex)
            {
                _currentWinStreak = 0;
                Debug.LogError($"{GetLogPrefix()} Score submission error: {ex.Message}");
                OnScoreSubmitted?.Invoke(false);
                return null;
            }
        }

        protected virtual void LogScoreSubmissionSuccess(IVXScoreSubmissionResponse response)
        {
            Debug.Log($"{GetLogPrefix()} ✓ Score submitted!");
            Debug.Log($"{GetLogPrefix()}   Reward:  {response.reward_earned} {response.reward_currency}");
            Debug.Log($"{GetLogPrefix()}   Balance: {response.wallet_balance}");
            Debug.Log($"{GetLogPrefix()}   Streak:  {_currentWinStreak}");

            if (response.reward_details != null)
            {
                var d = response.reward_details;
                Debug.Log($"{GetLogPrefix()}   Base: {d.base_reward} ({d.score} × {d.multiplier})");
                if (d.streak_multiplier > 1.0f)
                    Debug.Log($"{GetLogPrefix()}   Streak: ×{d.streak_multiplier}");
                if (d.milestone_bonus > 0)
                    Debug.Log($"{GetLogPrefix()}   Milestone: +{d.milestone_bonus}");
            }
        }

        // ========================================================================
        // LEADERBOARD FETCHING
        // ========================================================================

        public virtual async Task<IVXAllLeaderboardsResponse> GetAllLeaderboards(int limit = 50)
        {
            if (!await EnsureValidSession())
                return null;

            try
            {
                var user = IntelliVerseXIdentity.CurrentUser;

                var payload = new
                {
                    user_id = IntelliVerseXIdentity.NakamaUserId,
                    device_id = user.DeviceId,
                    game_id = _gameId,
                    limit = limit
                };

                Debug.Log($"{GetLogPrefix()} Fetching leaderboards (limit: {limit})...");

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var rpcResponse = await _client.RpcAsync(_session, RPC_GET_ALL_LEADERBOARDS, jsonPayload);

                var response = JsonConvert.DeserializeObject<IVXAllLeaderboardsResponse>(rpcResponse.Payload);

                if (response != null && response.success)
                {
                    Debug.Log($"{GetLogPrefix()} ✓ Leaderboards fetched");
                    OnLeaderboardsUpdated?.Invoke(response);
                    return response;
                }

                Debug.LogError($"{GetLogPrefix()} Leaderboard fetch failed: {response?.error ?? "Unknown error"}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Leaderboard error: {ex.Message}");
                return null;
            }
        }

        // ========================================================================
        // WALLET OPERATIONS
        // ========================================================================

        public virtual async Task<bool> UpdateWalletBalance(long amount, string walletType = "game", string changeType = "increment")
        {
            if (!await EnsureValidSession())
                return false;

            try
            {
                var user = IntelliVerseXIdentity.CurrentUser;

                var payload = new
                {
                    device_id = user.DeviceId,
                    game_id = _gameId,
                    amount = amount,
                    wallet_type = walletType,
                    change_type = changeType
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var rpcResponse = await _client.RpcAsync(_session, RPC_UPDATE_WALLET_BALANCE, jsonPayload);

                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(rpcResponse.Payload);

                if (response != null &&
                    response.TryGetValue("success", out var successObj) &&
                    successObj is bool success &&
                    success)
                {
                    Debug.Log($"{GetLogPrefix()} ✓ Wallet updated: {changeType} {amount}");
                    return true;
                }

                Debug.LogWarning($"{GetLogPrefix()} Wallet update RPC returned no success flag or success=false.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Wallet update error: {ex.Message}");
                return false;
            }
        }

        public virtual async Task<long?> GetWalletBalance(string walletType = "game")
        {
            if (!await EnsureValidSession())
                return null;

            try
            {
                var user = IntelliVerseXIdentity.CurrentUser;

                var payload = new
                {
                    device_id = user.DeviceId,
                    game_id = _gameId,
                    wallet_type = walletType
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var rpcResponse = await _client.RpcAsync(_session, RPC_GET_WALLET_BALANCE, jsonPayload);

                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(rpcResponse.Payload);

                if (response != null && response.TryGetValue("balance", out var balObj))
                {
                    return Convert.ToInt64(balObj);
                }

                Debug.LogWarning($"{GetLogPrefix()} GetWalletBalance: response had no 'balance' field.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Get balance error: {ex.Message}");
                return null;
            }
        }

        // ========================================================================
        // ADAPTIVE REWARD SYSTEM
        // ========================================================================

        public virtual async Task<IVXCalculateScoreRewardResponse> CalculateScoreReward(int score, int currentStreak = 0)
        {
            if (!await EnsureValidSession())
                return null;

            try
            {
                var payload = new
                {
                    game_id = _gameId,
                    score = score,
                    current_streak = currentStreak > 0 ? currentStreak : _currentWinStreak
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var rpcResponse = await _client.RpcAsync(_session, RPC_CALCULATE_SCORE_REWARD, jsonPayload);

                var response = JsonConvert.DeserializeObject<IVXCalculateScoreRewardResponse>(rpcResponse.Payload);

                if (response != null && response.success)
                {
                    Debug.Log($"{GetLogPrefix()} Reward preview: {response.reward_amount} {response.currency}");
                    OnRewardCalculated?.Invoke(response);
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Calculate reward error: {ex.Message}");
                return null;
            }
        }

        public virtual async Task<IVXUpdateGameRewardConfigResponse> UpdateGameRewardConfig(IVXGameRewardConfig config)
        {
            if (!await EnsureValidSession())
                return null;

            try
            {
                var payload = new
                {
                    game_id = _gameId,
                    config = config
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var rpcResponse = await _client.RpcAsync(_session, RPC_UPDATE_GAME_REWARD_CONFIG, jsonPayload);

                return JsonConvert.DeserializeObject<IVXUpdateGameRewardConfigResponse>(rpcResponse.Payload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetLogPrefix()} Update config error: {ex.Message}");
                return null;
            }
        }

        public virtual void ResetWinStreak()
        {
            _currentWinStreak = 0;
            Debug.Log($"{GetLogPrefix()} Streak reset");
        }

        protected virtual void OnDestroy()
        {
            if (_socket != null && _socket.IsConnected)
            {
                _socket.CloseAsync();
            }
        }
    }
}

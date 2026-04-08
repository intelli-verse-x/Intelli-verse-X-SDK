using System;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Manages the fortune-wheel feature — state queries, spinning, and
    /// configuration retrieval via Nakama RPCs.
    /// </summary>
    public sealed class IVXFortuneWheelManager : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXFortuneWheel]";
        private const string RPC_GET_STATE = "fortune_wheel_get_state";
        private const string RPC_SPIN = "fortune_wheel_spin";
        private const string RPC_GET_CONFIG = "fortune_wheel_get_config";

        #endregion

        #region Private Fields

        private static IVXFortuneWheelManager _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;
        private FortuneWheelState _cachedState;
        private FortuneWheelConfig _cachedConfig;

        #endregion

        #region Properties

        /// <summary>Singleton accessor.</summary>
        public static IVXFortuneWheelManager Instance => _instance;

        /// <summary>Whether the manager has been initialized.</summary>
        public bool IsInitialized => _initialized;

        /// <summary>Last fetched wheel state (may be null before first query).</summary>
        public FortuneWheelState CurrentState => _cachedState;

        /// <summary>Last fetched wheel configuration (may be null before first query).</summary>
        public FortuneWheelConfig CurrentConfig => _cachedConfig;

        /// <summary>Whether a free spin is currently available.</summary>
        public bool HasFreeSpin => _cachedState != null && _cachedState.freeSpinsRemaining > 0;

        #endregion

        #region Events

        /// <summary>Fired when a spin completes with the result.</summary>
        public event Action<SpinResult> OnSpinCompleted;

        /// <summary>Fired when a free spin becomes available.</summary>
        public event Action OnFreeSpinAvailable;

        /// <summary>Fired when the wheel configuration is refreshed.</summary>
        public event Action<FortuneWheelConfig> OnConfigUpdated;

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

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize with a valid Nakama client and session.
        /// </summary>
        /// <param name="client">Authenticated Nakama client.</param>
        /// <param name="session">Authenticated Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (session == null) throw new ArgumentNullException(nameof(session));

            _rpcClient = new IVXHiroRpcClient(client, session);
            _initialized = true;

            Debug.Log($"{LOG_TAG} Initialized.");
        }

        /// <summary>
        /// Update the session after a token refresh.
        /// </summary>
        public void RefreshSession(ISession session)
        {
            _rpcClient?.UpdateSession(session);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetch the current fortune-wheel state from the server.
        /// </summary>
        /// <returns>Wheel state or null on failure.</returns>
        public async Task<FortuneWheelState> GetStateAsync()
        {
            EnsureReady();

            var hadNoFreeSpins = _cachedState != null && _cachedState.freeSpinsRemaining <= 0;

            var response = await _rpcClient.CallAsync<FortuneWheelState>(RPC_GET_STATE);

            if (response.success && response.data != null)
            {
                _cachedState = response.data;

                if (hadNoFreeSpins && response.data.freeSpinsRemaining > 0)
                    OnFreeSpinAvailable?.Invoke();

                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} GetState failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Perform a spin on the fortune wheel.
        /// </summary>
        /// <param name="wheelId">Optional wheel identifier (defaults to "default").</param>
        /// <returns>Spin result or null on failure.</returns>
        public async Task<SpinResult> SpinAsync(string wheelId = "default")
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<SpinResult>(
                RPC_SPIN,
                new { wheel_id = wheelId });

            if (response.success && response.data != null)
            {
                Debug.Log($"{LOG_TAG} Spin result — segment: {response.data.segmentId}, " +
                          $"reward: {response.data.rewardAmount} {response.data.rewardType}");
                OnSpinCompleted?.Invoke(response.data);
                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} Spin failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Fetch the wheel configuration (segments, costs, intervals).
        /// </summary>
        /// <param name="wheelId">Optional wheel identifier (defaults to "default").</param>
        /// <returns>Wheel config or null on failure.</returns>
        public async Task<FortuneWheelConfig> GetConfigAsync(string wheelId = "default")
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<FortuneWheelConfig>(
                RPC_GET_CONFIG,
                new { wheel_id = wheelId });

            if (response.success && response.data != null)
            {
                _cachedConfig = response.data;
                OnConfigUpdated?.Invoke(response.data);
                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} GetConfig failed: {response.error}");
            return null;
        }

        #endregion

        #region Helpers

        private void EnsureReady()
        {
            if (!_initialized || _rpcClient == null)
                throw new InvalidOperationException($"{LOG_TAG} Not initialized. Call Initialize() first.");
        }

        #endregion
    }
}

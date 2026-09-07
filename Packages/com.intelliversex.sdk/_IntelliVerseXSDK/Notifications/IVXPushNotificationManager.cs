using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Notifications
{
    /// <summary>
    /// Manages push notification registration, delivery, and deep-link handling
    /// through Nakama RPC endpoints.
    /// </summary>
    public sealed class IVXPushNotificationManager : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXPushNotification]";
        private const string RPC_REGISTER_TOKEN = "push_register_token";
        private const string RPC_SEND_EVENT = "push_send_event";
        private const string RPC_GET_ENDPOINTS = "push_get_endpoints";

        #endregion

        #region Private Fields

        private static IVXPushNotificationManager _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;

        #endregion

        #region Properties

        /// <summary>Singleton accessor.</summary>
        public static IVXPushNotificationManager Instance => _instance;

        /// <summary>Whether the manager has been initialized.</summary>
        public bool IsInitialized => _initialized;

        #endregion

        #region Events

        /// <summary>Fired after a push token is successfully registered.</summary>
        public event Action<PushTokenRegistrationResult> OnTokenRegistered;

        /// <summary>Fired when a push notification is received.</summary>
        public event Action<PushEvent> OnPushReceived;

        /// <summary>Fired when a deep-link from a push notification is opened.</summary>
        public event Action<string> OnDeepLinkOpened;

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
        /// Register a device push token with the backend.
        /// </summary>
        /// <param name="token">Device push token string.</param>
        /// <param name="platform">Target push platform.</param>
        /// <returns>Registration result containing the endpoint ID.</returns>
        public async Task<PushTokenRegistrationResult> RegisterTokenAsync(string token, PushPlatform platform)
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<PushTokenRegistrationResult>(
                RPC_REGISTER_TOKEN,
                new { device_token = token, platform = (int)platform });

            if (response.success && response.data != null)
            {
                Debug.Log($"{LOG_TAG} Token registered — endpoint: {response.data.endpointId}");
                OnTokenRegistered?.Invoke(response.data);
                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} Token registration failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Retrieve all registered push endpoints for the current user.
        /// </summary>
        /// <returns>List of registered push endpoints.</returns>
        public async Task<List<PushEndpoint>> GetEndpointsAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<PushEndpointsResponse>(RPC_GET_ENDPOINTS);

            if (response.success && response.data != null)
                return response.data.endpoints ?? new List<PushEndpoint>();

            Debug.LogWarning($"{LOG_TAG} Failed to get endpoints: {response.error}");
            return new List<PushEndpoint>();
        }

        /// <summary>
        /// Handle an incoming deep-link URL from a push notification.
        /// Fires <see cref="OnDeepLinkOpened"/> with the URL.
        /// </summary>
        /// <param name="url">The deep-link URL to process.</param>
        public void HandleDeepLink(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning($"{LOG_TAG} HandleDeepLink called with empty URL.");
                return;
            }

            Debug.Log($"{LOG_TAG} Deep-link received: {url}");
            OnDeepLinkOpened?.Invoke(url);
        }

        /// <summary>
        /// Process an incoming push event payload. Call this from the native
        /// push callback to route events through the SDK.
        /// </summary>
        /// <param name="pushEvent">Deserialized push event.</param>
        public void HandlePushReceived(PushEvent pushEvent)
        {
            if (pushEvent == null)
            {
                Debug.LogWarning($"{LOG_TAG} HandlePushReceived called with null event.");
                return;
            }

            Debug.Log($"{LOG_TAG} Push received — type: {pushEvent.type}");
            OnPushReceived?.Invoke(pushEvent);

            if (!string.IsNullOrEmpty(pushEvent.deepLink))
                HandleDeepLink(pushEvent.deepLink);
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

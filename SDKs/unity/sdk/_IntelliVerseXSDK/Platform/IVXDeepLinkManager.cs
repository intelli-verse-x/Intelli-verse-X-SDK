using System;
using UnityEngine;

namespace IntelliVerseX.Platform
{
    /// <summary>
    /// Cross-platform deep link handler.
    /// Processes incoming deep links / universal links / app links and routes
    /// them to registered handlers based on path patterns.
    /// </summary>
    public sealed class IVXDeepLinkManager : MonoBehaviour
    {
        #region Constants
        private const string LOG_TAG = "[IVX-DeepLink]";
        #endregion

        #region Events
        /// <summary>
        /// Fired when a deep link is received. Subscribers receive the full URI.
        /// </summary>
        public static event Action<string> OnDeepLinkReceived;

        /// <summary>
        /// Fired when a deep link is received with parsed path and query parameters.
        /// Parameters: (path, queryString).
        /// </summary>
        public static event Action<string, string> OnDeepLinkParsed;
        #endregion

        #region Private Fields
        private static IVXDeepLinkManager _instance;
        private string _pendingDeepLink;
        #endregion

        #region Properties
        /// <summary>
        /// The most recently received deep link URI, or null if none received.
        /// </summary>
        public static string LastDeepLink => _instance != null ? _instance._pendingDeepLink : null;
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

            Application.deepLinkActivated += OnDeepLinkActivated;

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Application.deepLinkActivated -= OnDeepLinkActivated;
                _instance = null;
            }
        }
        #endregion

        #region Private Methods
        private void OnDeepLinkActivated(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            Debug.Log($"{LOG_TAG} Received: {url}");
            _pendingDeepLink = url;

            OnDeepLinkReceived?.Invoke(url);

            try
            {
                var uri = new Uri(url);
                OnDeepLinkParsed?.Invoke(uri.AbsolutePath, uri.Query);
            }
            catch (UriFormatException ex)
            {
                Debug.LogWarning($"{LOG_TAG} Failed to parse URI: {ex.Message}");
                OnDeepLinkParsed?.Invoke(url, string.Empty);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Consume and clear the pending deep link. Returns the URI or null.
        /// </summary>
        public static string ConsumePendingDeepLink()
        {
            if (_instance == null) return null;
            var link = _instance._pendingDeepLink;
            _instance._pendingDeepLink = null;
            return link;
        }
        #endregion
    }
}

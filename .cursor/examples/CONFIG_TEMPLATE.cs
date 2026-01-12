using UnityEngine;

namespace IntelliVerseX.[Module]
{
    /// <summary>
    /// Configuration settings for the [feature] module.
    /// </summary>
    /// <remarks>
    /// Create an instance via Assets > Create > IntelliVerseX > [Feature] Config.
    /// Assign to the appropriate manager in the inspector.
    /// </remarks>
    [CreateAssetMenu(
        fileName = "IVX[Feature]Config",
        menuName = "IntelliVerseX/[Feature] Config",
        order = 100)]
    public class IVX[Feature]Config : ScriptableObject
    {
        #region Serialized Fields

        [Header("General Settings")]
        [Tooltip("Enable or disable the [feature] module")]
        [SerializeField] private bool _enabled = true;

        [Tooltip("Enable debug logging for troubleshooting")]
        [SerializeField] private bool _debugMode;

        [Header("Connection Settings")]
        [Tooltip("The server endpoint URL")]
        [SerializeField] private string _serverUrl = "https://api.intelliversex.com";

        [Tooltip("Connection timeout in seconds")]
        [Range(5, 120)]
        [SerializeField] private int _timeoutSeconds = 30;

        [Header("Retry Settings")]
        [Tooltip("Maximum number of retry attempts")]
        [Range(0, 10)]
        [SerializeField] private int _maxRetries = 3;

        [Tooltip("Delay between retries in seconds")]
        [Range(0.5f, 10f)]
        [SerializeField] private float _retryDelaySeconds = 1f;

        [Header("Platform Overrides")]
        [Tooltip("Override settings for Android platform")]
        [SerializeField] private PlatformOverride _androidOverride;

        [Tooltip("Override settings for iOS platform")]
        [SerializeField] private PlatformOverride _iosOverride;

        [Tooltip("Override settings for WebGL platform")]
        [SerializeField] private PlatformOverride _webglOverride;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the [feature] module is enabled.
        /// </summary>
        public bool Enabled => _enabled;

        /// <summary>
        /// Gets whether debug mode is enabled.
        /// </summary>
        public bool DebugMode => _debugMode;

        /// <summary>
        /// Gets the server URL for the current platform.
        /// </summary>
        public string ServerUrl => GetPlatformValue(_serverUrl, o => o.ServerUrl);

        /// <summary>
        /// Gets the timeout in seconds.
        /// </summary>
        public int TimeoutSeconds => _timeoutSeconds;

        /// <summary>
        /// Gets the maximum number of retry attempts.
        /// </summary>
        public int MaxRetries => _maxRetries;

        /// <summary>
        /// Gets the delay between retries in seconds.
        /// </summary>
        public float RetryDelaySeconds => _retryDelaySeconds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates the configuration settings.
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise.</returns>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(ServerUrl))
            {
                Debug.LogError("[IVX[Feature]Config] Server URL is required");
                return false;
            }

            if (_timeoutSeconds < 5)
            {
                Debug.LogWarning("[IVX[Feature]Config] Timeout is very low, may cause issues");
            }

            return true;
        }

        /// <summary>
        /// Creates a copy of this configuration.
        /// </summary>
        /// <returns>A new configuration instance with the same values.</returns>
        public IVX[Feature]Config Clone()
        {
            var clone = CreateInstance<IVX[Feature]Config>();
            clone._enabled = _enabled;
            clone._debugMode = _debugMode;
            clone._serverUrl = _serverUrl;
            clone._timeoutSeconds = _timeoutSeconds;
            clone._maxRetries = _maxRetries;
            clone._retryDelaySeconds = _retryDelaySeconds;
            return clone;
        }

        #endregion

        #region Private Methods

        private T GetPlatformValue<T>(T defaultValue, System.Func<PlatformOverride, T> selector)
        {
            PlatformOverride currentOverride = null;

#if UNITY_ANDROID
            currentOverride = _androidOverride;
#elif UNITY_IOS
            currentOverride = _iosOverride;
#elif UNITY_WEBGL
            currentOverride = _webglOverride;
#endif

            if (currentOverride != null && currentOverride.Enabled)
            {
                var overrideValue = selector(currentOverride);
                if (overrideValue != null && !overrideValue.Equals(default(T)))
                {
                    return overrideValue;
                }
            }

            return defaultValue;
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Platform-specific configuration overrides.
        /// </summary>
        [System.Serializable]
        public class PlatformOverride
        {
            [Tooltip("Enable platform-specific overrides")]
            public bool Enabled;

            [Tooltip("Override server URL for this platform")]
            public string ServerUrl;
        }

        #endregion
    }
}

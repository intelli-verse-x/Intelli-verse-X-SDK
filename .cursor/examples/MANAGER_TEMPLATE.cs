using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.[Module]
{
    /// <summary>
    /// Manages [feature description] for the IntelliVerseX SDK.
    /// </summary>
    /// <remarks>
    /// This manager follows the singleton pattern and persists across scene loads.
    /// Initialize with <see cref="InitializeAsync"/> before use.
    /// </remarks>
    public class IVX[Feature]Manager : MonoBehaviour
    {
        #region Constants

        private const int MAX_RETRY_COUNT = 3;
        private const float RETRY_DELAY_SECONDS = 1.0f;

        #endregion

        #region Singleton

        private static IVX[Feature]Manager _instance;

        /// <summary>
        /// Gets the singleton instance of the [feature] manager.
        /// </summary>
        /// <value>The singleton instance, or null if not initialized.</value>
        public static IVX[Feature]Manager Instance => _instance;

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private IVX[Feature]Config _config;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs;

        #endregion

        #region Private Fields

        private bool _isInitialized;
        private bool _isInitializing;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the manager is initialized.
        /// </summary>
        public event Action OnInitialized;

        /// <summary>
        /// Invoked when an error occurs.
        /// </summary>
        public event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the manager is initialized and ready for use.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public IVX[Feature]Config Config => _config;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSingleton();
        }

        private void OnDestroy()
        {
            CleanupResources();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the [feature] manager with the specified configuration.
        /// </summary>
        /// <param name="config">Optional configuration override.</param>
        /// <returns>A task representing the initialization operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if already initializing.</exception>
        public async Task InitializeAsync(IVX[Feature]Config config = null)
        {
            if (_isInitialized)
            {
                LogWarning("Already initialized");
                return;
            }

            if (_isInitializing)
            {
                throw new InvalidOperationException(
                    "Initialization already in progress");
            }

            _isInitializing = true;

            try
            {
                _config = config ?? _config;
                ValidateConfig();

                await PerformInitializationAsync();

                _isInitialized = true;
                OnInitialized?.Invoke();

                Log("Initialized successfully");
            }
            catch (Exception ex)
            {
                LogError($"Initialization failed: {ex.Message}");
                OnError?.Invoke(ex.Message);
                throw;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        /// <summary>
        /// Shuts down the manager and releases resources.
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            Log("Shutting down...");

            CleanupResources();
            _isInitialized = false;

            Log("Shutdown complete");
        }

        #endregion

        #region Private Methods

        private void InitializeSingleton()
        {
            if (_instance != null && _instance != this)
            {
                LogWarning("Duplicate instance detected, destroying...");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Log("Singleton initialized");
        }

        private void ValidateConfig()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    "Configuration is required. Assign IVX[Feature]Config in inspector or pass to InitializeAsync.");
            }

            // Add specific validation here
        }

        private async Task PerformInitializationAsync()
        {
            Log("Performing initialization...");

            // TODO: Replace with actual initialization logic
            await Task.Yield();

            Log("Initialization complete");
        }

        private void CleanupResources()
        {
            // TODO: Clean up any resources, subscriptions, etc.

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[IVX[Feature]Manager] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[IVX[Feature]Manager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[IVX[Feature]Manager] {message}");
        }

        #endregion
    }
}

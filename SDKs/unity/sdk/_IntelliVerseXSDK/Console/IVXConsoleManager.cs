using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Console
{
    /// <summary>
    /// Singleton manager for console platform integration.
    /// Register a platform-specific <see cref="IIVXConsoleAdapter"/> to enable
    /// achievements, presence, sign-in, and overlay features on consoles.
    /// </summary>
    public sealed class IVXConsoleManager : MonoBehaviour
    {
        #region Constants
        private const string LOG_TAG = nameof(IVXConsoleManager);
        #endregion

        #region Events
        /// <summary>Fired when a console platform adapter is registered.</summary>
        public event Action<string> OnConsolePlatformDetected;
        /// <summary>Fired when console sign-in completes successfully.</summary>
        public event Action OnConsoleSignInComplete;
        #endregion

        #region Private Fields
        private IIVXConsoleAdapter _adapter;
        #endregion

        #region Properties
        /// <summary>Whether a console adapter has been registered.</summary>
        public bool IsConsoleAvailable => _adapter != null;
        /// <summary>The platform identifier of the active adapter, or null.</summary>
        public string PlatformId => _adapter?.PlatformId;
        #endregion

        #region Singleton
        private static IVXConsoleManager _instance;
        public static IVXConsoleManager Instance => _instance;

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

        #region Public Methods
        /// <summary>
        /// Register a platform adapter. Replaces any previously registered adapter.
        /// </summary>
        public void RegisterAdapter(IIVXConsoleAdapter adapter)
        {
            if (adapter == null)
            {
                Debug.LogWarning($"[{LOG_TAG}] Attempted to register a null adapter");
                return;
            }
            _adapter = adapter;
            Debug.Log($"[{LOG_TAG}] Console adapter registered: {adapter.PlatformId}");
            OnConsolePlatformDetected?.Invoke(adapter.PlatformId);
        }

        /// <summary>Retrieve the platform-specific user ID.</summary>
        public async Task<string> GetPlatformUserIdAsync()
        {
            if (!WarnIfNoAdapter()) return null;
            return await _adapter.GetPlatformUserIdAsync();
        }

        /// <summary>Show the native platform overlay.</summary>
        public async Task<bool> ShowPlatformOverlayAsync()
        {
            if (!WarnIfNoAdapter()) return false;
            return await _adapter.ShowPlatformOverlayAsync();
        }

        /// <summary>Check whether a named platform feature is supported.</summary>
        public bool SupportsFeature(string feature)
        {
            if (!WarnIfNoAdapter()) return false;
            return _adapter.SupportsFeature(feature);
        }

        /// <summary>Sign in using platform credentials.</summary>
        public async Task SignInWithPlatformAsync()
        {
            if (!WarnIfNoAdapter()) return;
            await _adapter.SignInWithPlatformAsync();
            Debug.Log($"[{LOG_TAG}] Console sign-in complete");
            OnConsoleSignInComplete?.Invoke();
        }

        /// <summary>Unlock an achievement on the native platform.</summary>
        public async Task<bool> UnlockAchievementAsync(string achievementId)
        {
            if (!WarnIfNoAdapter()) return false;
            return await _adapter.UnlockAchievementAsync(achievementId);
        }

        /// <summary>Set the player's rich-presence status string.</summary>
        public async Task<bool> SetPresenceAsync(string status)
        {
            if (!WarnIfNoAdapter()) return false;
            return await _adapter.SetPresenceAsync(status);
        }
        #endregion

        #region Private Methods
        private bool WarnIfNoAdapter()
        {
            if (_adapter != null) return true;
            Debug.LogWarning($"[{LOG_TAG}] No console adapter registered. Call RegisterAdapter() first.");
            return false;
        }
        #endregion
    }
}

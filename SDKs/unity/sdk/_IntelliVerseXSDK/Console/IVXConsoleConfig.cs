using UnityEngine;

namespace IntelliVerseX.Console
{
    /// <summary>
    /// Configuration asset for console platform integration.
    /// Create via Assets > Create > IntelliVerseX > Console Configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "ConsoleConfig", menuName = "IntelliVerseX/Console Configuration", order = 200)]
    public sealed class IVXConsoleConfig : ScriptableObject
    {
        #region Serialized Fields
        [Header("General")]
        [Tooltip("Enable or disable console platform integration globally.")]
        [SerializeField] private bool _enableConsoleIntegration = true;

        [Tooltip("Automatically sign in with the platform when the game starts.")]
        [SerializeField] private bool _autoSignIn = true;

        [Header("Presence")]
        [Tooltip("Default rich-presence status shown on the platform profile.")]
        [SerializeField] private string _defaultPresenceStatus = "Playing IntelliVerseX";
        #endregion

        #region Properties
        /// <summary>Whether console platform integration is enabled.</summary>
        public bool EnableConsoleIntegration => _enableConsoleIntegration;
        /// <summary>Whether to auto-sign-in on startup.</summary>
        public bool AutoSignIn => _autoSignIn;
        /// <summary>Default rich-presence status string.</summary>
        public string DefaultPresenceStatus => _defaultPresenceStatus;
        #endregion
    }
}

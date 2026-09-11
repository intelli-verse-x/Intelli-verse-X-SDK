using System;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// REMOVED as a public init path. Use <c>IVXBootstrap</c> + <c>IVXBootstrapConfig</c>
    /// (IntelliVerseX → Control Center). Kept as a deserialize stub for old scenes only.
    /// Prefer <c>IVXAuthClient</c> / <c>IVXAPIClient</c> for auth and <c>IVXNWalletManager</c> for wallet.
    /// </summary>
    [Obsolete("Removed. Use IVXBootstrap + IVXBootstrapConfig (IntelliVerseX → Control Center).")]
    public class IntelliVerseXManager : MonoBehaviour
    {
        public const string SDKVersion = "6.0.0";

        private static IntelliVerseXManager _instance;
        private static IntelliVerseXConfig _config;

        public event Action OnReady;
        public event Action<string> OnError;

        public static IntelliVerseXManager Instance
        {
            get
            {
                Debug.LogError("[IntelliVerseX] IntelliVerseXManager is removed. Use IVXBootstrap.");
                return _instance;
            }
        }

        public static IntelliVerseXConfig Config => _config;
        public static bool IsInitialized => false;
        public static bool IsMultiplayerAvailable => false;
        public object MultiplayerManager => null;

        /// <summary>Always fails — legacy init path is retired.</summary>
        public static void Initialize(IntelliVerseXConfig config)
        {
            const string msg =
                "IntelliVerseXManager.Initialize is removed. " +
                "Use IntelliVerseX → Control Center, assign IVXBootstrapConfig, add IVXBootstrap to the scene.";
            Debug.LogError("[IntelliVerseX] " + msg);
            throw new InvalidOperationException(msg);
        }

        private void Awake()
        {
            Debug.LogError(
                "[IntelliVerseX] IntelliVerseXManager component is obsolete. Replace with IVXBootstrap + IVXBootstrapConfig.");
            enabled = false;
        }
    }
}

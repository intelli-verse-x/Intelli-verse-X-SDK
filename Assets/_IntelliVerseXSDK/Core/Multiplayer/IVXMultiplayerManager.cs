// IVXMultiplayerManager.cs
// IntelliVerseX SDK - Photon Multiplayer Manager
// This assembly only compiles when INTELLIVERSEX_HAS_PHOTON is defined

using Photon.Pun;
using IntelliVerseX.Core;
using UnityEngine;

namespace IntelliVerseX.Multiplayer
{
    /// <summary>
    /// Manages Photon multiplayer connections for IntelliVerse-X games.
    /// This class is only available when Photon PUN2 is installed.
    /// </summary>
    public class IVXMultiplayerManager
    {
        private static bool isConnecting;

        /// <summary>
        /// Initializes the multiplayer manager.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[IVXMultiplayerManager] Initialized with Photon PUN2");
        }

        /// <summary>
        /// Connects to Photon using the App ID from the game's configuration.
        /// </summary>
        public void Connect()
        {
            if (PhotonNetwork.IsConnected || isConnecting)
            {
                return;
            }

            isConnecting = true;
            string appId = IVXPhotonConfig.GetAppId();
            Debug.Log($"[IVXMultiplayerManager] Connecting to Photon with App ID: {appId}");

            if (string.IsNullOrEmpty(appId))
            {
                Debug.LogError("[IVXMultiplayerManager] Photon App ID is not configured in IntelliVerseXConfig.");
                isConnecting = false;
                return;
            }

            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = appId;
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// Checks if Photon PUN2 is available. Always true when this assembly is loaded.
        /// </summary>
        public static bool IsPhotonAvailable => true;

        /// <summary>
        /// Checks if connected to Photon.
        /// </summary>
        public static bool IsConnected => PhotonNetwork.IsConnected;
    }
}

// IVXMultiplayerManager.cs
// IntelliVerseX SDK - Photon Multiplayer Manager
// Requires Photon PUN2 package to be installed

#if PHOTON_PUN2 || PUN_2_0_OR_NEWER || PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif
using IntelliVerseX.Core;
using UnityEngine;

namespace IntelliVerseX.Multiplayer
{
    /// <summary>
    /// Manages Photon multiplayer connections for IntelliVerse-X games.
    /// Requires Photon PUN2 package to be installed.
    /// </summary>
    public class IVXMultiplayerManager
    {
        private static bool isConnecting;

        /// <summary>
        /// Initializes the multiplayer manager.
        /// </summary>
        public void Initialize()
        {
#if PHOTON_PUN2 || PUN_2_0_OR_NEWER || PHOTON_UNITY_NETWORKING
            Debug.Log("[IVXMultiplayerManager] Initialized with Photon PUN2");
#else
            Debug.LogWarning("[IVXMultiplayerManager] Photon PUN2 not installed. Multiplayer features disabled.");
#endif
        }

        /// <summary>
        /// Connects to Photon using the App ID from the game's configuration.
        /// </summary>
        public void Connect()
        {
#if PHOTON_PUN2 || PUN_2_0_OR_NEWER || PHOTON_UNITY_NETWORKING
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
#else
            Debug.LogError("[IVXMultiplayerManager] Cannot connect - Photon PUN2 not installed.");
            Debug.LogError("[IVXMultiplayerManager] Install from: https://assetstore.unity.com/packages/tools/network/pun-2-free-119922");
#endif
        }

        /// <summary>
        /// Checks if Photon PUN2 is available.
        /// </summary>
        public static bool IsPhotonAvailable
        {
            get
            {
#if PHOTON_PUN2 || PUN_2_0_OR_NEWER || PHOTON_UNITY_NETWORKING
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Checks if connected to Photon.
        /// </summary>
        public static bool IsConnected
        {
            get
            {
#if PHOTON_PUN2 || PUN_2_0_OR_NEWER || PHOTON_UNITY_NETWORKING
                return PhotonNetwork.IsConnected;
#else
                return false;
#endif
            }
        }
    }
}

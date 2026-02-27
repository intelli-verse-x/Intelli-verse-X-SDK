// ============================================================================
// IVXWebGLAdsCallback.cs
// WebGL JavaScript callback receiver for IntelliVerse-X SDK Ads
// 
// Copyright (c) IntelliVerseX. All rights reserved.
// Version: 2.0.0
// 
// This MonoBehaviour receives callbacks from JavaScript via SendMessage.
// It's automatically created by IVXWebGLAdsManager when needed.
// ============================================================================

using UnityEngine;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Receives JavaScript callbacks for WebGL ads.
    /// Created automatically by IVXWebGLAdsManager.
    /// </summary>
    public class IVXWebGLAdsCallback : MonoBehaviour
    {
        private const string LOG_PREFIX = "[IVXWebGLAdsCallback]";

        private static IVXWebGLAdsCallback _instance;

        public static IVXWebGLAdsCallback Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("IVXWebGLAdsCallback");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<IVXWebGLAdsCallback>();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Called from JavaScript when AdSense interstitial completes
        /// Message format: "unitName:success"
        /// </summary>
        public void OnAdSenseInterstitialCompleted(string message)
        {
            Debug.Log($"{LOG_PREFIX} AdSense interstitial completed: {message}");

            ParseCallback(message, out string unitName, out bool success);
            IVXWebGLAdsManager.OnAdSenseInterstitialCompleted(unitName, success);
        }

        /// <summary>
        /// Called from JavaScript when Applixir ad completes
        /// Message format: "unitName:success"
        /// </summary>
        public void OnApplixirAdCompleted(string message)
        {
            Debug.Log($"{LOG_PREFIX} Applixir ad completed: {message}");

            ParseCallback(message, out string unitName, out bool success);
            IVXWebGLAdsManager.OnApplixirAdCompleted(unitName, success);
        }

        /// <summary>
        /// Called from JavaScript for general ad events
        /// </summary>
        public void OnAdEvent(string message)
        {
            Debug.Log($"{LOG_PREFIX} Ad event: {message}");
        }

        /// <summary>
        /// Called from JavaScript for ad errors
        /// </summary>
        public void OnAdError(string message)
        {
            Debug.LogWarning($"{LOG_PREFIX} Ad error: {message}");
        }

        private void ParseCallback(string message, out string unitName, out bool success)
        {
            unitName = "";
            success = false;

            if (string.IsNullOrEmpty(message)) return;

            var parts = message.Split(':');
            if (parts.Length >= 1)
            {
                unitName = parts[0];
            }
            if (parts.Length >= 2)
            {
                success = parts[1].ToLower() == "true";
            }
        }

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
    }
}

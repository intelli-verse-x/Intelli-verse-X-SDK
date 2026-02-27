using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace ApplixirSDK.Runtime
{
    internal delegate void StringCallback(string val);

    public static class ApplixirWebGL
    {
        #region Public methods

        /// <summary>
        /// Initialise the SDK by loading the AppLixirAdsConfig. Be sure to create the config
        /// in your resources and fill-in tre appropriate values. 
        /// </summary>
        public static void Initialise()
        {
            _config = Resources.Load<AppLixirAdsConfig>("AppLixirAdsConfig");
            if (_config == null)
            {
                Debug.LogError("Please create AppLixirAdsConfig");
                _isInitialized = false;
                return;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Calls out to the applixir service to show a video ad.
        /// Result is returned via the resultCallback.
        /// </summary>
        public static void PlayVideo(Action<PlayVideoResult> resultCallback)
        {
            if (_isInitialized == false)
            {
                LogError("Applixir SDK is not initialized. Use Initialise() before calling PlayVideo");
                resultCallback?.Invoke(PlayVideoResult.unavailable);
                return;
            }

            _videoResultCallback = resultCallback;
#if UNITY_EDITOR
            Log(
                "Show video is not supported in editor. You may select desired result status " +
                "in the SDK config (Tools/ApplixirSDK/Applixir Ads Config) ");
            ApplixirVideoEventHandler(_config.debugPlayVideoResponse.ToString());
#else
            ShowVideo(_config.UserId, ApplixirVideoEventHandler, ApplixirVideoErrorHandler, (int)_config.logLevel);
#endif
        }

        #endregion

        #region Private

        private const string Tag = "ApplixirWebGL";

        [DllImport("__Internal")]
        private static extern void ShowVideo(
            string userId,
            StringCallback onCompleted,
            StringCallback onError,
            int verbosity);

        private static int _userId;
        private static AppLixirAdsConfig _config;
        private static bool _isInitialized;
        private static Action<PlayVideoResult> _videoResultCallback;


        [MonoPInvokeCallback(typeof(StringCallback))]
        public static void ApplixirVideoEventHandler(string result)
        {
            Log("Got Video result: " + result);
            PlayVideoResult enumResult;
            if (Enum.TryParse(result, true, out enumResult))
            {
                _videoResultCallback?.Invoke(enumResult);
                if (enumResult == PlayVideoResult.thankYouModalClosed)
                {
                    _videoResultCallback?.Invoke(PlayVideoResult.adsRewarded);
                }
            }
            else
            {
                Log("Could not parse result: " + result);
                _videoResultCallback?.Invoke(PlayVideoResult.unknown);
            }
        }

        [MonoPInvokeCallback(typeof(StringCallback))]
        public static void ApplixirVideoErrorHandler(string result)
        {
            Log("Got Video error: " + result);
            PlayVideoResult enumResult;
            if (Enum.TryParse(result, true, out enumResult))
            {
                _videoResultCallback?.Invoke(enumResult);
            }
            else
            {
                Log("Could not parse result: " + result);
                _videoResultCallback?.Invoke(PlayVideoResult.unknown);
            }
        }

        private static void Log(string message)
        {
            if (_config.logLevel >= LogLevel.Info)
            {
                Debug.Log($"[{Tag}] {message}");
            }
        }

        private static void LogError(string message)
        {
            if (_config.logLevel >= LogLevel.Error)
            {
                Debug.LogError($"[{Tag}] {message}");
            }
        }

        #endregion
    }
}
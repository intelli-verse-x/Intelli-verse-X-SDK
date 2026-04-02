using System;
using UnityEngine;

namespace IntelliVerseX.Platform
{
    /// <summary>
    /// Detects VR/AR/XR capabilities and provides platform-specific helpers.
    /// Supports Meta Quest, Apple Vision Pro, SteamVR, PSVR2, and AR Foundation.
    /// </summary>
    public sealed class IVXXRPlatformHelper : MonoBehaviour
    {
        #region Constants
        private const string LOG_TAG = nameof(IVXXRPlatformHelper);
        #endregion

        #region Enums
        public enum XRPlatformType
        {
            None,
            MetaQuest,
            SteamVR,
            AppleVisionPro,
            PSVR2,
            WindowsMR,
            GenericOpenXR,
            ARFoundation
        }

        public enum XRTrackingState
        {
            NotTracking,
            Limited,
            Full
        }
        #endregion

        #region Events
        /// <summary>Fired when an XR device is detected or disconnected.</summary>
        public event Action<XRPlatformType> OnXRDeviceChanged;
        /// <summary>Fired when tracking state changes (e.g. lost tracking).</summary>
        public event Action<XRTrackingState> OnTrackingStateChanged;
        /// <summary>Fired when the user pauses/resumes in VR (e.g. removes headset).</summary>
        public event Action<bool> OnXRFocusChanged;
        #endregion

        #region Properties
        /// <summary>Whether any XR device is currently active.</summary>
        public bool IsXRActive { get; private set; }
        /// <summary>Detected XR platform type.</summary>
        public XRPlatformType ActivePlatform { get; private set; } = XRPlatformType.None;
        /// <summary>Current tracking state.</summary>
        public XRTrackingState TrackingState { get; private set; } = XRTrackingState.NotTracking;
        /// <summary>Whether hand tracking is available on the current device.</summary>
        public bool HandTrackingAvailable { get; private set; }
        /// <summary>Whether eye tracking is available on the current device.</summary>
        public bool EyeTrackingAvailable { get; private set; }
        /// <summary>Whether passthrough/AR mode is available.</summary>
        public bool PassthroughAvailable { get; private set; }
        #endregion

        #region Singleton
        private static IVXXRPlatformHelper _instance;
        public static IVXXRPlatformHelper Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            DetectXRPlatform();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Detect the current XR platform. Called automatically on Awake.
        /// Call manually if XR subsystem initializes after SDK startup.
        /// </summary>
        public void DetectXRPlatform()
        {
            ActivePlatform = XRPlatformType.None;
            IsXRActive = false;
            HandTrackingAvailable = false;
            EyeTrackingAvailable = false;
            PassthroughAvailable = false;

#if UNITY_XR_MANAGEMENT
            var xrSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
            if (xrSettings != null && xrSettings.Manager != null && xrSettings.Manager.activeLoader != null)
            {
                IsXRActive = true;
                string loaderName = xrSettings.Manager.activeLoader.name ?? "";

                if (loaderName.Contains("Oculus") || loaderName.Contains("Meta"))
                {
                    ActivePlatform = XRPlatformType.MetaQuest;
                    HandTrackingAvailable = true;
                    EyeTrackingAvailable = true;
                    PassthroughAvailable = true;
                }
                else if (loaderName.Contains("OpenXR"))
                {
                    ActivePlatform = XRPlatformType.GenericOpenXR;
#if UNITY_STANDALONE_WIN
                    ActivePlatform = XRPlatformType.SteamVR;
#endif
                }
                else if (loaderName.Contains("Apple") || loaderName.Contains("Vision"))
                {
                    ActivePlatform = XRPlatformType.AppleVisionPro;
                    HandTrackingAvailable = true;
                    EyeTrackingAvailable = true;
                    PassthroughAvailable = true;
                }

                TrackingState = XRTrackingState.Full;
            }
#endif

#if UNITY_AR_FOUNDATION
            if (!IsXRActive)
            {
                ActivePlatform = XRPlatformType.ARFoundation;
                IsXRActive = true;
                TrackingState = XRTrackingState.Full;
            }
#endif

            Debug.Log($"[{LOG_TAG}] XR Detection: active={IsXRActive}, platform={ActivePlatform}");
            OnXRDeviceChanged?.Invoke(ActivePlatform);
            OnTrackingStateChanged?.Invoke(TrackingState);
        }

        /// <summary>
        /// Get recommended SDK settings for the current XR platform.
        /// Useful for adjusting UI scale, input mode, and rendering.
        /// </summary>
        public XRRecommendedSettings GetRecommendedSettings()
        {
            return ActivePlatform switch
            {
                XRPlatformType.MetaQuest => new XRRecommendedSettings
                {
                    UIScale = 0.001f,
                    UseWorldSpaceUI = true,
                    PreferHandTracking = true,
                    TargetFrameRate = 72,
                    RecommendedRenderScale = 1.0f,
                },
                XRPlatformType.AppleVisionPro => new XRRecommendedSettings
                {
                    UIScale = 0.001f,
                    UseWorldSpaceUI = true,
                    PreferHandTracking = true,
                    TargetFrameRate = 90,
                    RecommendedRenderScale = 1.2f,
                },
                XRPlatformType.SteamVR => new XRRecommendedSettings
                {
                    UIScale = 0.001f,
                    UseWorldSpaceUI = true,
                    PreferHandTracking = false,
                    TargetFrameRate = 90,
                    RecommendedRenderScale = 1.0f,
                },
                XRPlatformType.ARFoundation => new XRRecommendedSettings
                {
                    UIScale = 1.0f,
                    UseWorldSpaceUI = false,
                    PreferHandTracking = false,
                    TargetFrameRate = 60,
                    RecommendedRenderScale = 1.0f,
                },
                _ => new XRRecommendedSettings(),
            };
        }
        #endregion

        #region Data Types
        [Serializable]
        public struct XRRecommendedSettings
        {
            public float UIScale;
            public bool UseWorldSpaceUI;
            public bool PreferHandTracking;
            public int TargetFrameRate;
            public float RecommendedRenderScale;
        }
        #endregion
    }
}

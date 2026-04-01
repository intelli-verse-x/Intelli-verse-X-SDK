using System;
using UnityEngine;

namespace IntelliVerseX.Platform
{
    /// <summary>
    /// Detects foldable device states (folded, half-open, flat) and screen
    /// configuration changes. Games can subscribe to layout events to
    /// adapt UI for large-screen, tabletop, or split-screen modes.
    /// </summary>
    public sealed class IVXFoldableHelper : MonoBehaviour
    {
        #region Constants
        private const string LOG_TAG = "[IVX-Foldable]";
        private const float TABLET_MIN_DPI_INCHES = 6.5f;
        #endregion

        #region Enums
        public enum DeviceFormFactor
        {
            Phone,
            Tablet,
            Foldable,
            Desktop,
            Unknown
        }

        public enum FoldState
        {
            Unknown,
            Flat,
            HalfOpen,
            Folded
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when the screen resolution or orientation changes.
        /// Parameters: (width, height).
        /// </summary>
        public static event Action<int, int> OnScreenConfigChanged;

        /// <summary>
        /// Fired when the aspect ratio crosses the tablet threshold.
        /// </summary>
        public static event Action<bool> OnLargeScreenChanged;
        #endregion

        #region Private Fields
        private static IVXFoldableHelper _instance;
        private int _lastWidth;
        private int _lastHeight;
        private bool _wasLargeScreen;
        #endregion

        #region Properties
        /// <summary>
        /// Current screen diagonal in inches (approximate).
        /// </summary>
        public static float ScreenDiagonalInches
        {
            get
            {
                float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
                float widthInches = Screen.width / dpi;
                float heightInches = Screen.height / dpi;
                return Mathf.Sqrt(widthInches * widthInches + heightInches * heightInches);
            }
        }

        /// <summary>
        /// True if the screen diagonal exceeds the tablet threshold.
        /// </summary>
        public static bool IsLargeScreen => ScreenDiagonalInches >= TABLET_MIN_DPI_INCHES;

        /// <summary>
        /// Best-guess form factor for the current device.
        /// </summary>
        public static DeviceFormFactor FormFactor
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                return DeviceFormFactor.Desktop;
#elif UNITY_ANDROID || UNITY_IOS
                return IsLargeScreen ? DeviceFormFactor.Tablet : DeviceFormFactor.Phone;
#else
                return DeviceFormFactor.Unknown;
#endif
            }
        }

        /// <summary>
        /// Current aspect ratio (width / height, always >= 1).
        /// </summary>
        public static float AspectRatio
        {
            get
            {
                float w = Screen.width;
                float h = Screen.height;
                return w >= h ? w / h : h / w;
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _wasLargeScreen = IsLargeScreen;
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                _lastWidth = Screen.width;
                _lastHeight = Screen.height;

                Debug.Log($"{LOG_TAG} Screen config changed: {_lastWidth}x{_lastHeight}");
                OnScreenConfigChanged?.Invoke(_lastWidth, _lastHeight);

                bool isLarge = IsLargeScreen;
                if (isLarge != _wasLargeScreen)
                {
                    _wasLargeScreen = isLarge;
                    OnLargeScreenChanged?.Invoke(isLarge);
                }
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
        #endregion
    }
}

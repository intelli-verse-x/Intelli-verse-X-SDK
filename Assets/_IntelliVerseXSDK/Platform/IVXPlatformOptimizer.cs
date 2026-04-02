using UnityEngine;

namespace IntelliVerseX.Platform
{
    /// <summary>
    /// Cross-platform performance optimizer.
    /// Applies device-appropriate quality settings, frame rate targets,
    /// and battery-aware rendering adjustments at runtime.
    /// </summary>
    public sealed class IVXPlatformOptimizer : MonoBehaviour
    {
        #region Constants
        private const string LOG_TAG = "[IVX-PlatformOpt]";
        private const int DEFAULT_TARGET_FPS_MOBILE = 60;
        private const int DEFAULT_TARGET_FPS_DESKTOP = -1;
        private const float LOW_BATTERY_THRESHOLD = 0.2f;
        #endregion

        #region Serialized Fields
        [Header("Frame Rate")]
        [SerializeField] private int _mobileTargetFps = DEFAULT_TARGET_FPS_MOBILE;
        [SerializeField] private int _desktopTargetFps = DEFAULT_TARGET_FPS_DESKTOP;
        [SerializeField] private int _lowPowerTargetFps = 30;

        [Header("Battery")]
        [SerializeField] private bool _enableBatteryThrottling = true;
        [SerializeField] private float _batteryCheckIntervalSec = 30f;
        #endregion

        #region Private Fields
        private static IVXPlatformOptimizer _instance;
        private float _lastBatteryCheck;
        private bool _isLowPower;
        #endregion

        #region Properties
        /// <summary>
        /// True when the device is in a low-battery state and throttling is active.
        /// </summary>
        public static bool IsLowPowerMode => _instance != null && _instance._isLowPower;

        /// <summary>
        /// Current battery level (0..1) or 1 if unknown / plugged in.
        /// </summary>
        public static float BatteryLevel
        {
            get
            {
                float level = SystemInfo.batteryLevel;
                return level < 0 ? 1f : level;
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

            ApplyPlatformDefaults();
        }

        private void Update()
        {
            if (!_enableBatteryThrottling) return;

            if (Time.unscaledTime - _lastBatteryCheck < _batteryCheckIntervalSec) return;
            _lastBatteryCheck = Time.unscaledTime;

            CheckBattery();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Override the target frame rate at runtime.
        /// </summary>
        public static void SetTargetFrameRate(int fps)
        {
            Application.targetFrameRate = fps;
            Debug.Log($"{LOG_TAG} Target FPS set to {fps}");
        }

        /// <summary>
        /// Set the render scale (0.5 = half resolution, 1.0 = full).
        /// Useful for dynamic resolution on low-end devices.
        /// </summary>
        public static void SetRenderScale(float scale)
        {
            scale = Mathf.Clamp(scale, 0.25f, 1f);
            QualitySettings.resolutionScalingFixedDPIFactor = scale;
            Debug.Log($"{LOG_TAG} Render scale set to {scale}");
        }
        #endregion

        #region Private Methods
        private void ApplyPlatformDefaults()
        {
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = _mobileTargetFps;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#else
            Application.targetFrameRate = _desktopTargetFps;
#endif
            Debug.Log($"{LOG_TAG} Platform defaults applied. FPS={Application.targetFrameRate}");
        }

        private void CheckBattery()
        {
            if (SystemInfo.batteryStatus == BatteryStatus.Charging ||
                SystemInfo.batteryStatus == BatteryStatus.Full)
            {
                if (_isLowPower) ExitLowPowerMode();
                return;
            }

            bool lowNow = BatteryLevel < LOW_BATTERY_THRESHOLD;

            if (lowNow && !_isLowPower)
                EnterLowPowerMode();
            else if (!lowNow && _isLowPower)
                ExitLowPowerMode();
        }

        private void EnterLowPowerMode()
        {
            _isLowPower = true;
            Application.targetFrameRate = _lowPowerTargetFps;
            Debug.Log($"{LOG_TAG} Low power mode ON (battery={BatteryLevel:P0}, fps={_lowPowerTargetFps})");
        }

        private void ExitLowPowerMode()
        {
            _isLowPower = false;
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = _mobileTargetFps;
#else
            Application.targetFrameRate = _desktopTargetFps;
#endif
            Debug.Log($"{LOG_TAG} Low power mode OFF (fps={Application.targetFrameRate})");
        }
        #endregion
    }
}

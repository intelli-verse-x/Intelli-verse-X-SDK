// IVXGettingStartedDemo.cs
// Getting Started sample - demonstrates basic SDK initialization and usage

using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Samples.GettingStarted
{
    /// <summary>
    /// Demonstrates basic IntelliVerseX SDK initialization and usage.
    /// This script shows the recommended way to set up the SDK in your game.
    /// </summary>
    public class IVXGettingStartedDemo : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Enable to connect to backend on start (requires Nakama)")]
        [SerializeField] private bool connectToBackend = false;
        
        [Tooltip("Enable verbose logging")]
        [SerializeField] private bool verboseLogging = true;
        
        [Header("Status")]
        [SerializeField] private string sdkStatus = "Not Initialized";
        [SerializeField] private string userStatus = "Not Identified";
        [SerializeField] private string backendStatus = "Not Connected";
        
        // Events for UI updates
        public event System.Action<string> OnSDKStatusChanged;
        public event System.Action<string> OnUserStatusChanged;
        public event System.Action<string> OnBackendStatusChanged;
        
        private void Awake()
        {
            // Ensure this object persists across scenes (optional)
            // DontDestroyOnLoad(gameObject);
        }
        
        private async void Start()
        {
            await InitializeSDKAsync();
        }
        
        /// <summary>
        /// Initialize the IntelliVerseX SDK.
        /// </summary>
        public async Task InitializeSDKAsync()
        {
            try
            {
                UpdateStatus("Initializing SDK...", ref sdkStatus, OnSDKStatusChanged);
                
                // Step 1: Initialize device identity
                // This creates a unique identifier for this device
                InitializeIdentity();
                
                // Step 2: Optionally connect to backend
                if (connectToBackend)
                {
                    await ConnectToBackendAsync();
                }
                else
                {
                    UpdateStatus("Skipped (Disabled)", ref backendStatus, OnBackendStatusChanged);
                }
                
                // SDK is ready!
                UpdateStatus("Ready", ref sdkStatus, OnSDKStatusChanged);
                Log("IntelliVerseX SDK initialized successfully!");
            }
            catch (System.Exception e)
            {
                UpdateStatus($"Error: {e.Message}", ref sdkStatus, OnSDKStatusChanged);
                LogError($"SDK initialization failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Initialize device identity.
        /// </summary>
        private void InitializeIdentity()
        {
            UpdateStatus("Identifying device...", ref userStatus, OnUserStatusChanged);
            
            try
            {
                // Use reflection to call IntelliVerseXUserIdentity.InitializeDevice()
                // This avoids hard dependency on the Identity assembly
                var identityType = System.Type.GetType("IntelliVerseX.Identity.IntelliVerseXUserIdentity, IntelliVerseXIdentity");
                
                if (identityType != null)
                {
                    var initMethod = identityType.GetMethod("InitializeDevice", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, null);
                        
                        // Get device ID
                        var deviceIdProp = identityType.GetProperty("DeviceId", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        string deviceId = deviceIdProp?.GetValue(null) as string ?? "Unknown";
                        
                        UpdateStatus($"Device ID: {deviceId.Substring(0, Mathf.Min(8, deviceId.Length))}...", ref userStatus, OnUserStatusChanged);
                        Log($"Device identity initialized");
                        return;
                    }
                }
                
                // Fallback: use SystemInfo
                string fallbackId = SystemInfo.deviceUniqueIdentifier;
                UpdateStatus($"Fallback ID: {fallbackId.Substring(0, Mathf.Min(8, fallbackId.Length))}...", ref userStatus, OnUserStatusChanged);
                Log("Using fallback device identity");
            }
            catch (System.Exception e)
            {
                UpdateStatus("Failed", ref userStatus, OnUserStatusChanged);
                LogWarning($"Identity initialization failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Connect to the backend server.
        /// </summary>
        private async Task ConnectToBackendAsync()
        {
            UpdateStatus("Connecting...", ref backendStatus, OnBackendStatusChanged);
            
            try
            {
                // Use reflection to access IVXBackendService
                var backendType = System.Type.GetType("IntelliVerseX.Backend.IVXBackendService, IntelliVerseX.Backend");
                
                if (backendType != null)
                {
                    var instanceProp = backendType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var instance = instanceProp?.GetValue(null);
                    
                    if (instance != null)
                    {
                        var initMethod = backendType.GetMethod("InitializeAsync");
                        if (initMethod != null)
                        {
                            var task = (Task<bool>)initMethod.Invoke(instance, null);
                            bool connected = await task;
                            
                            UpdateStatus(connected ? "Connected" : "Failed to Connect", ref backendStatus, OnBackendStatusChanged);
                            Log($"Backend connection: {(connected ? "Success" : "Failed")}");
                            return;
                        }
                    }
                }
                
                UpdateStatus("Backend not available", ref backendStatus, OnBackendStatusChanged);
                LogWarning("Backend service not found. Install Nakama for backend features.");
            }
            catch (System.Exception e)
            {
                UpdateStatus($"Error: {e.Message}", ref backendStatus, OnBackendStatusChanged);
                LogError($"Backend connection failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Demonstrates logging with the SDK.
        /// </summary>
        public void DemoLogging()
        {
            Log("This is an info log");
            LogWarning("This is a warning log");
            
            // Using IVXLogger directly (if available)
            var loggerType = System.Type.GetType("IntelliVerseX.Core.IVXLogger, IntelliVerseX.Core");
            if (loggerType != null)
            {
                var logMethod = loggerType.GetMethod("Log", new[] { typeof(string) });
                logMethod?.Invoke(null, new object[] { "This is logged via IVXLogger" });
            }
        }
        
        /// <summary>
        /// Demonstrates analytics tracking (if available).
        /// </summary>
        public void DemoAnalytics()
        {
            try
            {
                var analyticsType = System.Type.GetType("IntelliVerseX.Analytics.IVXAnalyticsService, IntelliVerseX.Analytics");
                if (analyticsType != null)
                {
                    var instanceProp = analyticsType.GetProperty("Instance");
                    var instance = instanceProp?.GetValue(null);
                    
                    if (instance != null)
                    {
                        var trackMethod = analyticsType.GetMethod("TrackEvent", new[] { typeof(string) });
                        trackMethod?.Invoke(instance, new object[] { "demo_button_clicked" });
                        Log("Analytics event tracked!");
                    }
                }
                else
                {
                    LogWarning("Analytics module not available");
                }
            }
            catch (System.Exception e)
            {
                LogError($"Analytics demo failed: {e.Message}");
            }
        }
        
        #region Helper Methods
        
        private void UpdateStatus(string status, ref string field, System.Action<string> callback)
        {
            field = status;
            callback?.Invoke(status);
        }
        
        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[IVX Demo] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[IVX Demo] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[IVX Demo] {message}");
        }
        
        #endregion
        
        #region Public Accessors
        
        public string SDKStatus => sdkStatus;
        public string UserStatus => userStatus;
        public string BackendStatus => backendStatus;
        
        #endregion
    }
}

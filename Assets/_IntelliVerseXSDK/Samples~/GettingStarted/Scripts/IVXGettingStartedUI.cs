// IVXGettingStartedUI.cs
// Simple UI for the Getting Started sample

using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Samples.GettingStarted
{
    /// <summary>
    /// Simple UI to display SDK status and demo buttons.
    /// </summary>
    public class IVXGettingStartedUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private IVXGettingStartedDemo demo;
        
        [Header("Status UI")]
        [SerializeField] private Text sdkStatusText;
        [SerializeField] private Text userStatusText;
        [SerializeField] private Text backendStatusText;
        
        [Header("Buttons")]
        [SerializeField] private Button reinitButton;
        [SerializeField] private Button logDemoButton;
        [SerializeField] private Button analyticsDemoButton;
        
        private void Start()
        {
            // Find demo if not assigned
            if (demo == null)
            {
                demo = FindObjectOfType<IVXGettingStartedDemo>();
            }
            
            if (demo != null)
            {
                // Subscribe to status changes
                demo.OnSDKStatusChanged += UpdateSDKStatus;
                demo.OnUserStatusChanged += UpdateUserStatus;
                demo.OnBackendStatusChanged += UpdateBackendStatus;
                
                // Initial update
                UpdateSDKStatus(demo.SDKStatus);
                UpdateUserStatus(demo.UserStatus);
                UpdateBackendStatus(demo.BackendStatus);
            }
            
            // Setup buttons
            if (reinitButton != null)
            {
                reinitButton.onClick.AddListener(OnReinitClicked);
            }
            
            if (logDemoButton != null)
            {
                logDemoButton.onClick.AddListener(OnLogDemoClicked);
            }
            
            if (analyticsDemoButton != null)
            {
                analyticsDemoButton.onClick.AddListener(OnAnalyticsDemoClicked);
            }
        }
        
        private void OnDestroy()
        {
            if (demo != null)
            {
                demo.OnSDKStatusChanged -= UpdateSDKStatus;
                demo.OnUserStatusChanged -= UpdateUserStatus;
                demo.OnBackendStatusChanged -= UpdateBackendStatus;
            }
        }
        
        private void UpdateSDKStatus(string status)
        {
            if (sdkStatusText != null)
            {
                sdkStatusText.text = $"SDK: {status}";
            }
        }
        
        private void UpdateUserStatus(string status)
        {
            if (userStatusText != null)
            {
                userStatusText.text = $"User: {status}";
            }
        }
        
        private void UpdateBackendStatus(string status)
        {
            if (backendStatusText != null)
            {
                backendStatusText.text = $"Backend: {status}";
            }
        }
        
        private async void OnReinitClicked()
        {
            if (demo != null)
            {
                await demo.InitializeSDKAsync();
            }
        }
        
        private void OnLogDemoClicked()
        {
            if (demo != null)
            {
                demo.DemoLogging();
            }
        }
        
        private void OnAnalyticsDemoClicked()
        {
            if (demo != null)
            {
                demo.DemoAnalytics();
            }
        }
        
        private void OnGUI()
        {
            // Fallback GUI if no Canvas is present
            if (sdkStatusText == null)
            {
                GUILayout.BeginArea(new Rect(10, 10, 400, 300));
                
                GUILayout.Label("IntelliVerseX SDK - Getting Started", GUI.skin.box);
                GUILayout.Space(10);
                
                if (demo != null)
                {
                    GUILayout.Label($"SDK Status: {demo.SDKStatus}");
                    GUILayout.Label($"User Status: {demo.UserStatus}");
                    GUILayout.Label($"Backend Status: {demo.BackendStatus}");
                    
                    GUILayout.Space(10);
                    
                    if (GUILayout.Button("Reinitialize"))
                    {
                        OnReinitClicked();
                    }
                    
                    if (GUILayout.Button("Demo Logging"))
                    {
                        OnLogDemoClicked();
                    }
                    
                    if (GUILayout.Button("Demo Analytics"))
                    {
                        OnAnalyticsDemoClicked();
                    }
                }
                else
                {
                    GUILayout.Label("Demo script not found!");
                }
                
                GUILayout.EndArea();
            }
        }
    }
}

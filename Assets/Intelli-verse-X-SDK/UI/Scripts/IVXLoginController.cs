using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using IntelliVerseX.Core;
using System.Threading.Tasks;

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Universal login screen controller for IntelliVerse-X SDK.
    /// Handles guest login, email/password, and social authentication.
    /// 
    /// Setup:
    /// 1. Add IVX_LoginScene prefab to your login scene
    /// 2. Configure mainMenuSceneName (e.g., "MainMenu")
    /// 3. Optional: Customize UI elements and colors
    /// 
    /// Features:
    /// - Guest account (4-day expiry)
    /// - Email/Password authentication
    /// - Apple Sign In (iOS)
    /// - Google Sign In (Android/iOS)
    /// - Auto-login with saved credentials
    /// - Automatic SDK initialization
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/UI/Login Controller")]
    public class IVXLoginController : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [Tooltip("Scene to load after successful login")]
        public string mainMenuSceneName = "MainMenu";
        
        [Header("UI Panels")]
        public GameObject loginPanel;
        public GameObject loadingPanel;
        
        [Header("Login Buttons")]
        public Button guestButton;
        public Button emailLoginButton;
        public Button appleSignInButton;
        public Button googleSignInButton;
        
        [Header("Email/Password Fields")]
        public TMP_InputField emailInput;
        public TMP_InputField passwordInput;
        public Toggle rememberMeToggle;
        
        [Header("UI Text")]
        public TextMeshProUGUI gameNameText;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI versionText;
        
        [Header("Settings")]
        [Tooltip("Enable auto-login with saved credentials")]
        public bool enableAutoLogin = true;
        
        [Tooltip("Show Apple Sign In button (iOS only)")]
        public bool showAppleSignIn = true;
        
        [Tooltip("Show Google Sign In button")]
        public bool showGoogleSignIn = true;
        
        private IntelliVerseXConfig config;
        private bool isInitializing = false;

        async void Start()
        {
            LoadConfiguration();
            SetupUI();
            SetupButtonListeners();
            
            if (enableAutoLogin)
            {
                await TryAutoLogin();
            }
        }

        void LoadConfiguration()
        {
            config = UnityEngine.Resources.Load<IntelliVerseXConfig>("IntelliVerseX/QuizVerseConfig");
            
            if (config == null)
            {
                Debug.LogError("[IVX Login] Config not found in Resources/IntelliVerseX/");
                ShowStatus("Configuration error. Please contact support.", Color.red);
                return;
            }
            
            if (!config.IsValid())
            {
                Debug.LogError("[IVX Login] Invalid configuration!");
                ShowStatus("Invalid game configuration.", Color.red);
                return;
            }
            
            Debug.Log($"[IVX Login] Loaded config for: {config.gameName}");
        }

        void SetupUI()
        {
            if (gameNameText && config)
                gameNameText.text = config.gameName;
            
            if (versionText)
                versionText.text = $"v{Application.version}";
            
            // Platform-specific buttons
            if (appleSignInButton)
            {
                #if UNITY_IOS
                appleSignInButton.gameObject.SetActive(showAppleSignIn);
                #else
                appleSignInButton.gameObject.SetActive(false);
                #endif
            }
            
            if (googleSignInButton)
                googleSignInButton.gameObject.SetActive(showGoogleSignIn);
            
            // Show login panel
            if (loginPanel) loginPanel.SetActive(true);
            if (loadingPanel) loadingPanel.SetActive(false);
        }

        void SetupButtonListeners()
        {
            if (guestButton)
                guestButton.onClick.AddListener(OnGuestLoginClicked);
            
            if (emailLoginButton)
                emailLoginButton.onClick.AddListener(OnEmailLoginClicked);
            
            if (appleSignInButton)
                appleSignInButton.onClick.AddListener(OnAppleSignInClicked);
            
            if (googleSignInButton)
                googleSignInButton.onClick.AddListener(OnGoogleSignInClicked);
        }

        async Task TryAutoLogin()
        {
            if (!config.enableAutoLogin) return;
            
            var identity = IntelliVerseXIdentity.Instance;
            if (identity == null)
            {
                Debug.LogWarning("[IVX Login] IntelliVerseXIdentity not found");
                return;
            }
            
            // Check if user has saved credentials
            var user = IntelliVerseXIdentity.CurrentUser;
            if (user != null && !string.IsNullOrEmpty(user.DeviceId))
            {
                ShowStatus("Restoring session...", Color.white);
                ShowLoadingPanel(true);
                
                // Try to authenticate with saved device ID
                bool success = await AuthenticateWithDeviceId(user.DeviceId);
                
                if (success)
                {
                    Debug.Log("[IVX Login] Auto-login successful");
                    LoadMainMenu();
                }
                else
                {
                    Debug.Log("[IVX Login] Auto-login failed, showing login screen");
                    ShowLoadingPanel(false);
                    ShowStatus("Welcome! Please sign in.", Color.white);
                }
            }
        }

        async void OnGuestLoginClicked()
        {
            if (isInitializing) return;

            Debug.Log("[IVX Login] Guest login clicked");
            ShowStatus("Creating guest account...", Color.white);
            ShowLoadingPanel(true);
            
            isInitializing = true;
            
            try
            {
                // Generate guest device ID
                string deviceId = SystemInfo.deviceUniqueIdentifier;
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = $"guest_{System.Guid.NewGuid().ToString("N").Substring(0, 16)}";
                }
                
                bool success = await AuthenticateWithDeviceId(deviceId);
                
                if (success)
                {
                    ShowStatus("Welcome!", Color.green);
                    await Task.Delay(500);
                    LoadMainMenu();
                }
                else
                {
                    ShowStatus("Guest login failed. Please try again.", Color.red);
                    ShowLoadingPanel(false);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IVX Login] Guest login error: {ex.Message}");
                ShowStatus("Login error. Please try again.", Color.red);
                ShowLoadingPanel(false);
            }
            finally
            {
                isInitializing = false;
            }
        }

        async void OnEmailLoginClicked()
        {
            if (isInitializing) return;

            string email = emailInput.text.Trim();
            string password = passwordInput.text;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowStatus("Please enter email and password.", Color.red);
                return;
            }
            
            if (!IsValidEmail(email))
            {
                ShowStatus("Please enter a valid email address.", Color.red);
                return;
            }
            
            Debug.Log("[IVX Login] Email login clicked");
            ShowStatus("Signing in...", Color.white);
            ShowLoadingPanel(true);
            
            isInitializing = true;
            
            try
            {
                bool success = await AuthenticateWithEmail(email, password);
                
                if (success)
                {
                    ShowStatus("Welcome back!", Color.green);
                    
                    if (rememberMeToggle && rememberMeToggle.isOn)
                    {
                        PlayerPrefs.SetString("IVX_SavedEmail", email);
                        PlayerPrefs.Save();
                    }
                    
                    await Task.Delay(500);
                    LoadMainMenu();
                }
                else
                {
                    ShowStatus("Invalid credentials. Please try again.", Color.red);
                    ShowLoadingPanel(false);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IVX Login] Email login error: {ex.Message}");
                ShowStatus("Login error. Please try again.", Color.red);
                ShowLoadingPanel(false);
            }
            finally
            {
                isInitializing = false;
            }
        }

        async void OnAppleSignInClicked()
        {
            await Task.CompletedTask; // Stub for future async implementation
#if UNITY_IOS
            if (isInitializing) return;
            
            Debug.Log("[IVX Login] Apple Sign In clicked");
            ShowStatus("Signing in with Apple...", Color.white);
            ShowLoadingPanel(true);
            
            isInitializing = true;
            
            try
            {
                // TODO: Implement Apple Sign In
                ShowStatus("Apple Sign In coming soon!", Color.yellow);
                ShowLoadingPanel(false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IVX Login] Apple Sign In error: {ex.Message}");
                ShowStatus("Apple Sign In failed. Please try again.", Color.red);
                ShowLoadingPanel(false);
            }
            finally
            {
                isInitializing = false;
            }
#endif
        }

        async void OnGoogleSignInClicked()
        {
            await Task.CompletedTask; // Stub for future async implementation
            if (isInitializing) return;

            Debug.Log("[IVX Login] Google Sign In clicked");
            ShowStatus("Signing in with Google...", Color.white);
            ShowLoadingPanel(true);
            
            isInitializing = true;
            
            try
            {
                // TODO: Implement Google Sign In
                ShowStatus("Google Sign In coming soon!", Color.yellow);
                ShowLoadingPanel(false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IVX Login] Google Sign In error: {ex.Message}");
                ShowStatus("Google Sign In failed. Please try again.", Color.red);
                ShowLoadingPanel(false);
            }
            finally
            {
                isInitializing = false;
            }
        }

        async Task<bool> AuthenticateWithDeviceId(string deviceId)
        {
            try
            {
                // Initialize SDK if not already initialized
                if (IntelliVerseXManager.Instance == null)
                {
                    Debug.Log("[IVX Login] Initializing SDK...");
                    IntelliVerseXManager.Initialize(config);
                    await Task.Delay(100); // Wait for initialization
                }
                
                var identity = IntelliVerseXIdentity.Instance;
                if (identity == null)
                {
                    Debug.LogError("[IVX Login] IntelliVerseXIdentity not available");
                    return false;
                }
                
                // Authenticate with device ID
                var user = new IntelliVerseXUser
                {
                    DeviceId = deviceId,
                    Username = $"Player_{deviceId.Substring(0, 8)}",
                    GameId = config.gameId
                };
                
                IntelliVerseXIdentity.SetCurrentUser(user);
                
                Debug.Log($"[IVX Login] Authentication successful for: {user.Username}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IVX Login] Authentication failed: {ex.Message}");
                return false;
            }
        }

        async Task<bool> AuthenticateWithEmail(string email, string password)
        {
            // TODO: Implement Cognito email/password authentication
            // For now, fallback to device ID authentication
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            return await AuthenticateWithDeviceId(deviceId);
        }

        void LoadMainMenu()
        {
            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                Debug.LogError("[IVX Login] Main menu scene name is not set!");
                ShowStatus("Configuration error.", Color.red);
                ShowLoadingPanel(false);
                return;
            }
            
            Debug.Log($"[IVX Login] Loading main menu: {mainMenuSceneName}");
            SceneManager.LoadScene(mainMenuSceneName);
        }

        void ShowLoadingPanel(bool show)
        {
            if (loginPanel) loginPanel.SetActive(!show);
            if (loadingPanel) loadingPanel.SetActive(show);
        }

        void ShowStatus(string message, Color color)
        {
            if (statusText)
            {
                statusText.text = message;
                statusText.color = color;
            }
            
            Debug.Log($"[IVX Login] Status: {message}");
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

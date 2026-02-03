using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using IntelliVerseX.Identity;

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// OTP verification panel. Calls real backend verify-otp API; on success configures session and notifies auth canvas.
    /// TODO: OTP verification activates account and logs user in automatically.
    /// This is a mandatory step - users cannot login until OTP is verified.
    /// </summary>
    public class IVXPanelOTP : MonoBehaviour
    {
        private const int OTP_LENGTH = 6;

        [Header("Input")]
        [SerializeField] private TMP_InputField _otpInput;

        [Header("Buttons")]
        [SerializeField] private Button _verifyButton;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _errorText;

        [Header("Scene Loading")]
        [Tooltip("Name of the game scene to load after successful OTP verification. Leave empty to use default behavior.")]
        [SerializeField] private string _gameSceneName = string.Empty;

        [Tooltip("If true, automatically load game scene after successful OTP verification.")]
        [SerializeField] private bool _autoLoadGameScene = false;

        private IVXCanvasAuth _canvasAuth;
        private AuthService _authService;
        private string _email;
        private bool _isProcessing;

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            TryFindAuthService();

            if (_canvasAuth == null)
                Debug.LogError("[IVXPanelOTP] IVXCanvasAuth not found in parent!");
            // AuthService will be auto-created if not found, so no error needed

            if (_verifyButton != null)
                _verifyButton.onClick.AddListener(Verify);
        }

        private void OnEnable()
        {
            // Try to find AuthService again in case it was added to the scene
            if (_authService == null)
            {
                TryFindAuthService();
            }
        }

        /// <summary>
        /// Set the email used for OTP verification (called from register panel).
        /// </summary>
        public void SetEmail(string e)
        {
            _email = e ?? string.Empty;
        }

        /// <summary>
        /// Set the game scene name to load after successful OTP verification.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void SetGameSceneName(string sceneName)
        {
            _gameSceneName = sceneName ?? string.Empty;
        }

        /// <summary>
        /// Enable or disable automatic scene loading after successful OTP verification.
        /// </summary>
        /// <param name="autoLoad">If true, automatically load game scene after verification</param>
        public void SetAutoLoadGameScene(bool autoLoad)
        {
            _autoLoadGameScene = autoLoad;
        }

        /// <summary>
        /// Verify OTP with backend. Validates 6 digits; on success configures APIManager and notifies auth success.
        /// </summary>
        public void Verify()
        {
            if (_isProcessing) return;
            
            // Try to find AuthService if not already found
            if (_authService == null)
            {
                TryFindAuthService();
            }
            
            if (_authService == null)
            {
                ShowError("Auth service not available. Please ensure AuthService component exists in the scene.");
                return;
            }

            string otp = _otpInput != null ? _otpInput.text?.Trim() ?? string.Empty : string.Empty;

            if (string.IsNullOrEmpty(_email))
            {
                ShowError("Email missing. Please start from registration.");
                return;
            }

            if (otp.Length != OTP_LENGTH || !System.Text.RegularExpressions.Regex.IsMatch(otp, @"^\d{6}$"))
            {
                ShowError("Please enter a valid 6-digit code.");
                return;
            }

            _isProcessing = true;
            ClearError();
            _canvasAuth?.ShowLoading();

            _authService.VerifyOtp(
                _email,
                otp,
                OnVerifySuccess,
                OnVerifyError);
        }

        /// <summary>
        /// Handles successful OTP verification.
        /// OTP verification = Account activation + Automatic login.
        /// User is logged in immediately after OTP verification.
        /// </summary>
        private void OnVerifySuccess(APIManager.LoginResponse loginResp)
        {
            _isProcessing = false;
            _canvasAuth?.HideLoading();

            if (loginResp?.data?.user == null)
            {
                ShowError("Invalid response from server.");
                return;
            }

            Debug.Log($"[IVXPanelOTP] OTP verified successfully → Account activated → User logged in: {loginResp.data.user.email}");

            // Configure user authentication with backend tokens
            // This activates the account and logs the user in
            APIManager.ConfigureUserAuthFromLoginResponse(loginResp, true);

            // Build auth result and notify success (user is now logged in)
            var result = BuildAuthResult(loginResp);
            _canvasAuth?.NotifyAuthSuccess(result);

            // Load game scene if auto-load is enabled and scene name is set
            if (_autoLoadGameScene && !string.IsNullOrWhiteSpace(_gameSceneName))
            {
                LoadGameScene(_gameSceneName);
            }
        }

        /// <summary>
        /// Loads the game scene after successful OTP verification.
        /// Call this method manually if auto-load is disabled, or it will be called automatically if auto-load is enabled.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load. If null or empty, uses the configured game scene name.</param>
        public void LoadGameSceneAfterVerification(string sceneName = null)
        {
            string targetScene = !string.IsNullOrWhiteSpace(sceneName) ? sceneName : _gameSceneName;

            if (string.IsNullOrWhiteSpace(targetScene))
            {
                Debug.LogWarning("[IVXPanelOTP] No game scene name provided. Cannot load scene.");
                return;
            }

            LoadGameScene(targetScene);
        }

        /// <summary>
        /// Loads the specified game scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        private void LoadGameScene(string sceneName)
        {
            try
            {
                // Check if scene exists in build settings
                if (!SceneExists(sceneName))
                {
                    Debug.LogError($"[IVXPanelOTP] Scene '{sceneName}' not found in build settings. Please add it to Build Settings > Scenes in Build.");
                    ShowError($"Scene '{sceneName}' not found. Please check build settings.");
                    return;
                }

                Debug.Log($"[IVXPanelOTP] Loading game scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXPanelOTP] Failed to load scene '{sceneName}': {ex.Message}");
                ShowError($"Failed to load scene: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a scene exists in the build settings.
        /// </summary>
        /// <param name="sceneName">Name of the scene to check</param>
        /// <returns>True if scene exists in build settings</returns>
        private bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (sceneNameFromPath.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles OTP verification error from backend.
        /// Extracts error message from backend JSON response.
        /// </summary>
        private void OnVerifyError(string message)
        {
            _isProcessing = false;
            _canvasAuth?.HideLoading();
            
            // Extract and display backend error message
            string errorMsg = message ?? "OTP verification failed";
            
            // Check for specific backend error messages
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Contains("invalid") || message.Contains("incorrect") || message.Contains("wrong"))
                {
                    errorMsg = "Invalid OTP code. Please check your email and try again.";
                }
                else if (message.Contains("expired"))
                {
                    errorMsg = "OTP code has expired. Please register again to receive a new code.";
                }
                else if (message.Contains("not found") || message.Contains("does not exist"))
                {
                    errorMsg = "OTP verification failed. Please start from registration.";
                }
            }
            
            Debug.LogWarning($"[IVXPanelOTP] OTP verification failed: {errorMsg}");
            ShowError(errorMsg);
        }

        private static AuthResult BuildAuthResult(APIManager.LoginResponse loginResp)
        {
            var d = loginResp.data;
            var u = d.user;
            string userId = !string.IsNullOrEmpty(u.idpUsername) ? u.idpUsername : u.id ?? string.Empty;
            string access = !string.IsNullOrEmpty(d.accessToken) ? d.accessToken : d.token ?? string.Empty;
            long expEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn);
            DateTime expAt;
            try
            {
                expAt = DateTimeOffset.FromUnixTimeSeconds(expEpoch).UtcDateTime;
            }
            catch
            {
                expAt = DateTime.UtcNow.AddMinutes(30);
            }

            return new AuthResult
            {
                UserId = userId,
                Email = u.email ?? string.Empty,
                DisplayName = u.userName ?? u.firstName ?? string.Empty,
                AccessToken = access,
                RefreshToken = d.refreshToken ?? string.Empty,
                IsGuest = false,
                ExpiresAt = expAt
            };
        }

        private void TryFindAuthService()
        {
            if (_authService == null)
            {
                _authService = FindObjectOfType<AuthService>();
                if (_authService == null)
                {
                    // Automatically create AuthService GameObject if it doesn't exist
                    GameObject authServiceObj = new GameObject("AuthService");
                    _authService = authServiceObj.AddComponent<AuthService>();
                    Debug.Log("[IVXPanelOTP] AuthService automatically created in scene.");
                }
                else
                {
                    Debug.Log("[IVXPanelOTP] AuthService found successfully.");
                }
            }
        }

        private void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
        }

        private void ClearError()
        {
            if (_errorText != null)
            {
                _errorText.text = string.Empty;
                _errorText.gameObject.SetActive(false);
            }
        }
    }
}

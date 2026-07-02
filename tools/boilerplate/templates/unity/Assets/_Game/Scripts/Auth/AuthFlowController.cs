using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using IntelliVerseX.Identity;
using IntelliVerseX.Analytics;
using IntelliVerseX.Satori;

namespace {{game_name}}.Auth
{
    /// <summary>
    /// Manages the login/registration flow with guest, email, and social auth.
    /// Transitions to MainMenu on successful authentication.
    /// </summary>
    public class AuthFlowController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panels")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _registerPanel;
        [SerializeField] private GameObject _loadingPanel;

        [Header("Login Fields")]
        [SerializeField] private TMP_InputField _loginEmail;
        [SerializeField] private TMP_InputField _loginPassword;

        [Header("Register Fields")]
        [SerializeField] private TMP_InputField _registerEmail;
        [SerializeField] private TMP_InputField _registerPassword;
        [SerializeField] private TMP_InputField _registerUsername;

        [Header("Buttons")]
        [SerializeField] private Button _guestButton;
        [SerializeField] private Button _emailLoginButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _googleButton;
        [SerializeField] private Button _appleButton;
        [SerializeField] private Button _switchToRegister;
        [SerializeField] private Button _switchToLogin;
        [SerializeField] private Button _confirmRegisterButton;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Navigation")]
        [SerializeField] private string _mainMenuScene = "MainMenu";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _guestButton?.onClick.AddListener(() => _ = SignInGuestAsync());
            _emailLoginButton?.onClick.AddListener(() => _ = SignInEmailAsync());
            _confirmRegisterButton?.onClick.AddListener(() => _ = RegisterEmailAsync());
            _googleButton?.onClick.AddListener(() => _ = SignInSocialAsync("google"));
            _appleButton?.onClick.AddListener(() => _ = SignInSocialAsync("apple"));

            _switchToRegister?.onClick.AddListener(() => ShowPanel(_registerPanel));
            _switchToLogin?.onClick.AddListener(() => ShowPanel(_loginPanel));

            ShowPanel(_loginPanel);
        }

        #endregion

        #region Auth Methods

        private async Task SignInGuestAsync()
        {
            ShowLoading("Signing in as guest...");
            try
            {
                await IVXAuthManager.Instance.SignInGuestAsync();
                TrackAuth("guest");
                await TransitionToMainMenu();
            }
            catch (Exception ex)
            {
                ShowError($"Guest login failed: {ex.Message}");
            }
        }

        private async Task SignInEmailAsync()
        {
            var email = _loginEmail?.text?.Trim();
            var password = _loginPassword?.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter email and password");
                return;
            }

            ShowLoading("Signing in...");
            try
            {
                await IVXAuthManager.Instance.SignInEmailAsync(email, password);
                TrackAuth("email");
                await TransitionToMainMenu();
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
        }

        private async Task RegisterEmailAsync()
        {
            var email = _registerEmail?.text?.Trim();
            var password = _registerPassword?.text;
            var username = _registerUsername?.text?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please fill all fields");
                return;
            }

            ShowLoading("Creating account...");
            try
            {
                await IVXAuthManager.Instance.RegisterEmailAsync(email, password, username);
                TrackAuth("register");
                await TransitionToMainMenu();
            }
            catch (Exception ex)
            {
                ShowError($"Registration failed: {ex.Message}");
            }
        }

        private async Task SignInSocialAsync(string provider)
        {
            ShowLoading($"Signing in with {provider}...");
            try
            {
                await IVXAuthManager.Instance.SignInSocialAsync(provider);
                TrackAuth(provider);
                await TransitionToMainMenu();
            }
            catch (Exception ex)
            {
                ShowError($"{provider} login failed: {ex.Message}");
            }
        }

        #endregion

        #region Navigation

        private async Task TransitionToMainMenu()
        {
            ShowLoading("Loading game...");
            var coordinator = IVXHiroCoordinator.Instance;
            if (coordinator != null)
                await coordinator.InitializeAllAsync();

            SceneManager.LoadScene(_mainMenuScene);
        }

        #endregion

        #region UI Helpers

        private void ShowPanel(GameObject panel)
        {
            _loginPanel?.SetActive(panel == _loginPanel);
            _registerPanel?.SetActive(panel == _registerPanel);
            _loadingPanel?.SetActive(false);
            _statusText.text = "";
        }

        private void ShowLoading(string message)
        {
            _loginPanel?.SetActive(false);
            _registerPanel?.SetActive(false);
            _loadingPanel?.SetActive(true);
            _statusText.text = message;
        }

        private void ShowError(string message)
        {
            _loadingPanel?.SetActive(false);
            _loginPanel?.SetActive(true);
            _statusText.text = $"<color=#FF4444>{message}</color>";
        }

        private void TrackAuth(string method)
        {
            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized) return;
            _ = satori.CaptureEventAsync("auth_complete", new Dictionary<string, string>
            {
                { "method", method },
                { "game_id", "{{game_id}}" },
            });
        }

        #endregion
    }
}

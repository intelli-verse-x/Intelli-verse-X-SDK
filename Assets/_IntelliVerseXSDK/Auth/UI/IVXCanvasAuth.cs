using System;
using System.Threading.Tasks;
using UnityEngine;
using IntelliVerseX.Identity;

namespace IntelliVerseX.Auth.UI
{
    public class IVXCanvasAuth : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _registerPanel;
        [SerializeField] private GameObject _otpPanel;
        [SerializeField] private GameObject _forgotPasswordPanel;
        [SerializeField] private GameObject _loadingPanel;

        [Header("Config")]
        [SerializeField] private bool _allowGuestLogin = true;

        // ===================== PUBLIC PROPERTIES =====================
        public bool AllowGuestLogin => _allowGuestLogin;

        // ===================== PANEL CONTROL =====================

        public void ShowLogin()
        {
            HideAll();
            if (_loginPanel) _loginPanel.SetActive(true);
        }

        public void ShowRegister()
        {
            HideAll();
            if (_registerPanel) _registerPanel.SetActive(true);
        }

        public void ShowOTP()
        {
            HideAll();
            if (_otpPanel) _otpPanel.SetActive(true);
        }

        public void ShowForgotPassword()
        {
            HideAll();
            if (_forgotPasswordPanel) _forgotPasswordPanel.SetActive(true);
        }

        public void ShowLoading()
        {
            if (_loadingPanel) _loadingPanel.SetActive(true);
        }

        public void HideLoading()
        {
            if (_loadingPanel) _loadingPanel.SetActive(false);
        }

        private void HideAll()
        {
            if (_loginPanel) _loginPanel.SetActive(false);
            if (_registerPanel) _registerPanel.SetActive(false);
            if (_otpPanel) _otpPanel.SetActive(false);
            if (_forgotPasswordPanel) _forgotPasswordPanel.SetActive(false);
            if (_loadingPanel) _loadingPanel.SetActive(false);
        }

        // ===================== AUTH CALLBACKS =====================

        public void NotifyAuthSuccess(AuthResult result)
        {
            HideLoading();
            Debug.Log("AUTH SUCCESS: " + result.Email);
        }

        public void NotifyAuthFailed(string error)
        {
            HideLoading();
            Debug.LogError("AUTH FAILED: " + error);
        }

        // ===================== GUEST LOGIN =====================

        public async void LoginAsGuest()
        {
            if (!_allowGuestLogin)
            {
                Debug.LogWarning("[IVXCanvasAuth] Guest login disabled");
                return;
            }

            ShowLoading();

            try
            {
                var response = await APIManager.GuestSignupAsync(
                    role: "user",
                    configureUserAuthOnSuccess: true,
                    persistSession: true);

                if (response?.data != null)
                {
                    var result = BuildAuthResultFromGuestResponse(response);
                    NotifyAuthSuccess(result);
                }
                else
                {
                    NotifyAuthFailed(response?.message ?? "Guest signup failed");
                }
            }
            catch (Exception ex)
            {
                NotifyAuthFailed(ex.Message);
            }
        }

        private static AuthResult BuildAuthResultFromGuestResponse(APIManager.GuestSignupResponse response)
        {
            var d = response.data;
            var u = d?.user;
            string userId = u != null && !string.IsNullOrEmpty(u.idpUsername) ? u.idpUsername : u?.id ?? string.Empty;
            string access = d != null ? (!string.IsNullOrEmpty(d.accessToken) ? d.accessToken : d.token ?? string.Empty) : string.Empty;
            long expEpoch = d != null
                ? (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn))
                : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1800;
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
                Email = u?.email ?? "guest@local",
                DisplayName = u?.userName ?? u?.firstName ?? "Guest",
                AccessToken = access,
                RefreshToken = d?.refreshToken ?? string.Empty,
                IsGuest = true,
                ExpiresAt = expAt
            };
        }
    }

    // ===================== DATA MODEL =====================

    [Serializable]
    public class AuthResult
    {
        public string UserId;
        public string Email;
        public string DisplayName;
        public string AccessToken;
        public string RefreshToken;
        public bool IsGuest;
        public DateTime ExpiresAt;
    }
}
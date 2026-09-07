using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// Demo controller for testing the Friends panel.
    /// Handles login status check and opens the Friends panel.
    /// 
    /// Usage:
    ///   1. Add this component to a button or any GameObject
    ///   2. Optionally wire up the openButton reference
    ///   3. Click the button or call OpenFriendsPanel() to open
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Social/Friends Demo Controller")]
    public class IVXFriendsDemoController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button openButton;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Settings")]
        [SerializeField] private bool autoOpenOnStart = false;
        [SerializeField] private bool autoLoginAsGuest = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        private bool _isLoggingIn;

        private void Start()
        {
            // Try to find button if not assigned
            if (openButton == null)
            {
                openButton = GetComponent<Button>();
                if (openButton == null)
                {
                    openButton = GetComponentInChildren<Button>();
                }
            }

            // Wire up click handler
            if (openButton != null)
            {
                openButton.onClick.AddListener(OnOpenButtonClicked);
                Log("Button wired up. Click to open Friends panel.");
            }
            else
            {
                LogWarning("No button found. Call OpenFriendsPanel() manually.");
            }

            // Update status display
            UpdateStatusUI();

            // Auto-open for testing
            if (autoOpenOnStart)
            {
                Invoke(nameof(TryOpenFriendsPanel), 0.5f);
            }
        }

        /// <summary>
        /// Called when the open button is clicked.
        /// </summary>
        private async void OnOpenButtonClicked()
        {
            // Check if user is logged in
            var session = UserSessionManager.Current;
            if (session == null || string.IsNullOrEmpty(session.accessToken))
            {
                Log("No user session found.");
                
                if (autoLoginAsGuest)
                {
                    Log("Auto-logging in as guest...");
                    await LoginAsGuestAsync();
                }
                else
                {
                    ShowStatus("Please login first to use Friends feature.", true);
                    return;
                }
            }
            
            OpenFriendsPanel();
        }

        /// <summary>
        /// Attempts to open friends panel, handling login if needed.
        /// </summary>
        private async void TryOpenFriendsPanel()
        {
            var session = UserSessionManager.Current;
            if (session == null || string.IsNullOrEmpty(session.accessToken))
            {
                if (autoLoginAsGuest)
                {
                    await LoginAsGuestAsync();
                }
                else
                {
                    ShowStatus("Please login first.", true);
                    return;
                }
            }
            
            OpenFriendsPanel();
        }

        /// <summary>
        /// Opens the Friends panel.
        /// </summary>
        public void OpenFriendsPanel()
        {
            var panel = IVXFriendsPanel.Instance;
            if (panel != null)
            {
                panel.Open();
                Log("Friends panel opened.");
            }
            else
            {
                LogError("IVXFriendsPanel not found in scene! Add IVXFriendsCanvas prefab to scene.");
                ShowStatus("Friends UI not found in scene!", true);
            }
        }

        /// <summary>
        /// Closes the Friends panel.
        /// </summary>
        public void CloseFriendsPanel()
        {
            var panel = IVXFriendsPanel.Instance;
            if (panel != null)
            {
                panel.Close();
            }
        }

        /// <summary>
        /// Logs in as a guest user for testing.
        /// </summary>
        public async System.Threading.Tasks.Task LoginAsGuestAsync()
        {
            if (_isLoggingIn)
            {
                Log("Login already in progress...");
                return;
            }

            _isLoggingIn = true;
            ShowStatus("Logging in as guest...", false);

            try
            {
                Log("Calling APIManager.GuestSignupAsync...");
                
                var response = await APIManager.GuestSignupAsync();
                
                if (response?.status == true && response.data != null)
                {
                    Log($"Guest login successful! User: {response.data.user?.userName ?? response.data.user?.id}");
                    
                    // Save session
                    UserSessionManager.SaveFromGuestResponse(response);
                    
                    ShowStatus($"Logged in as: {response.data.user?.userName ?? "Guest"}", false);
                    UpdateStatusUI();
                }
                else
                {
                    LogError($"Guest login failed: {response?.message ?? "Unknown error"}");
                    ShowStatus($"Login failed: {response?.message ?? "Unknown error"}", true);
                }
            }
            catch (Exception ex)
            {
                LogError($"Guest login exception: {ex.Message}");
                ShowStatus($"Login error: {ex.Message}", true);
            }
            finally
            {
                _isLoggingIn = false;
            }
        }

        /// <summary>
        /// Updates the status UI based on current session.
        /// </summary>
        private void UpdateStatusUI()
        {
            var session = UserSessionManager.Current;
            if (session != null && !string.IsNullOrEmpty(session.accessToken))
            {
                string name = !string.IsNullOrEmpty(session.userName) ? session.userName : 
                              !string.IsNullOrEmpty(session.email) ? session.email : 
                              session.userId ?? "User";
                ShowStatus($"Logged in: {name}", false);
            }
            else
            {
                ShowStatus("Not logged in", false);
            }
        }

        /// <summary>
        /// Shows a status message in the UI.
        /// </summary>
        private void ShowStatus(string message, bool isError)
        {
            Log(message);
            
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
            }
        }

        private void OnDestroy()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OnOpenButtonClicked);
            }
        }

        #region Logging

        private void Log(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[IVXFriends Demo] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[IVXFriends Demo] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[IVXFriends Demo] {message}");
        }

        #endregion
    }
}

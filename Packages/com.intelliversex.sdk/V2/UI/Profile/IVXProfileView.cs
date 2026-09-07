using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.V2.UI.Profile
{
    /// <summary>
    /// UI bindings for the profile panel.
    /// Attach this to the root of your profile Canvas prefab.
    /// </summary>
    public class IVXProfileView : MonoBehaviour
    {
        #region Header Section
        [Header("Header")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TMP_Text _displayNameText;
        [SerializeField] private TMP_Text _usernameText;
        [SerializeField] private TMP_Text _emailText;
        [SerializeField] private TMP_Text _userIdText;
        #endregion

        #region Edit Fields
        [Header("Edit Fields")]
        [SerializeField] private TMP_InputField _displayNameInput;
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private TMP_InputField _firstNameInput;
        [SerializeField] private TMP_InputField _lastNameInput;
        [SerializeField] private TMP_InputField _avatarUrlInput;
        #endregion

        #region Location Fields
        [Header("Location")]
        [SerializeField] private TMP_Text _locationText;
        [SerializeField] private TMP_InputField _cityInput;
        [SerializeField] private TMP_InputField _regionInput;
        [SerializeField] private TMP_InputField _countryInput;
        #endregion

        #region Avatar Presets
        [Header("Avatar Presets")]
        [SerializeField] private Transform _avatarPresetContainer;
        [SerializeField] private Button _avatarPresetButtonPrefab;
        [SerializeField] private Sprite[] _avatarPresets;
        #endregion

        #region Action Buttons
        [Header("Actions")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private Button _changeAvatarButton;
        [SerializeField] private Button _changeUsernameButton;
        #endregion

        #region Player Stats
        [Header("Player Stats")]
        [SerializeField] private GameObject _playerStatsPanel;
        [SerializeField] private TMP_Text _totalGamesText;
        [SerializeField] private TMP_Text _winsText;
        [SerializeField] private TMP_Text _lossesText;
        [SerializeField] private TMP_Text _winRateText;
        #endregion

        #region State
        [Header("State")]
        [SerializeField] private GameObject _viewModePanel;
        [SerializeField] private GameObject _editModePanel;
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private GameObject _errorPanel;
        [SerializeField] private TMP_Text _errorText;
        #endregion

        #region Properties
        public Image AvatarImage => _avatarImage;
        public TMP_Text DisplayNameText => _displayNameText;
        public TMP_Text UsernameText => _usernameText;
        public TMP_Text EmailText => _emailText;
        public TMP_Text UserIdText => _userIdText;

        public TMP_InputField DisplayNameInput => _displayNameInput;
        public TMP_InputField UsernameInput => _usernameInput;
        public TMP_InputField FirstNameInput => _firstNameInput;
        public TMP_InputField LastNameInput => _lastNameInput;
        public TMP_InputField AvatarUrlInput => _avatarUrlInput;
        public TMP_InputField CityInput => _cityInput;
        public TMP_InputField RegionInput => _regionInput;
        public TMP_InputField CountryInput => _countryInput;

        public TMP_Text LocationText => _locationText;

        public Transform AvatarPresetContainer => _avatarPresetContainer;
        public Button AvatarPresetButtonPrefab => _avatarPresetButtonPrefab;
        public Sprite[] AvatarPresets => _avatarPresets;

        public Button SaveButton => _saveButton;
        public Button CancelButton => _cancelButton;
        public Button EditButton => _editButton;
        public Button RefreshButton => _refreshButton;
        public Button ChangeAvatarButton => _changeAvatarButton;
        public Button ChangeUsernameButton => _changeUsernameButton;

        public GameObject PlayerStatsPanel => _playerStatsPanel;
        public TMP_Text TotalGamesText => _totalGamesText;
        public TMP_Text WinsText => _winsText;
        public TMP_Text LossesText => _lossesText;
        public TMP_Text WinRateText => _winRateText;

        public GameObject ViewModePanel => _viewModePanel;
        public GameObject EditModePanel => _editModePanel;
        public GameObject LoadingOverlay => _loadingOverlay;
        public TMP_Text StatusText => _statusText;
        public GameObject ErrorPanel => _errorPanel;
        public TMP_Text ErrorText => _errorText;
        #endregion

        #region Public Methods
        public void SetLoading(bool isLoading)
        {
            if (_loadingOverlay != null)
            {
                _loadingOverlay.SetActive(isLoading);
            }

            SetButtonsInteractable(!isLoading);
        }

        public void SetViewMode(bool viewMode)
        {
            if (_viewModePanel != null) _viewModePanel.SetActive(viewMode);
            if (_editModePanel != null) _editModePanel.SetActive(!viewMode);
        }

        public void SetStatus(string message, bool isError = false)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.color = isError ? Color.red : Color.white;
            }
        }

        public void ShowError(string message)
        {
            if (_errorPanel != null)
            {
                _errorPanel.SetActive(true);
                if (_errorText != null)
                {
                    _errorText.text = message;
                }
            }
            else
            {
                SetStatus(message, true);
            }
        }

        public void HideError()
        {
            if (_errorPanel != null)
            {
                _errorPanel.SetActive(false);
            }
        }

        public void SetProfileData(string displayName, string username, string email, string userId, string location)
        {
            if (_displayNameText != null) _displayNameText.text = displayName ?? "Player";
            if (_usernameText != null) _usernameText.text = !string.IsNullOrEmpty(username) ? $"@{username}" : "";
            if (_emailText != null) _emailText.text = email ?? "";
            if (_userIdText != null) _userIdText.text = !string.IsNullOrEmpty(userId) ? $"ID: {userId.Substring(0, Math.Min(8, userId.Length))}..." : "";
            if (_locationText != null) _locationText.text = location ?? "";
        }

        public void SetEditFieldValues(string displayName, string username, string firstName, string lastName, 
            string avatarUrl, string city, string region, string country)
        {
            if (_displayNameInput != null) _displayNameInput.text = displayName ?? "";
            if (_usernameInput != null) _usernameInput.text = username ?? "";
            if (_firstNameInput != null) _firstNameInput.text = firstName ?? "";
            if (_lastNameInput != null) _lastNameInput.text = lastName ?? "";
            if (_avatarUrlInput != null) _avatarUrlInput.text = avatarUrl ?? "";
            if (_cityInput != null) _cityInput.text = city ?? "";
            if (_regionInput != null) _regionInput.text = region ?? "";
            if (_countryInput != null) _countryInput.text = country ?? "";
        }

        public void SetAvatarSprite(Sprite sprite)
        {
            if (_avatarImage != null && sprite != null)
            {
                _avatarImage.sprite = sprite;
            }
        }

        public void SetPlayerStats(int totalGames, int wins, int losses, float winRate)
        {
            if (_totalGamesText != null) _totalGamesText.text = $"Total Games Played: {totalGames}";
            if (_winsText != null) _winsText.text = $"Wins: {wins}";
            if (_lossesText != null) _lossesText.text = $"Losses: {losses}";
            if (_winRateText != null) _winRateText.text = totalGames > 0 ? $"Win Rate: {winRate:F1}%" : "Win Rate: -%";
        }
        #endregion

        #region Private Methods
        private void SetButtonsInteractable(bool interactable)
        {
            if (_saveButton != null) _saveButton.interactable = interactable;
            if (_cancelButton != null) _cancelButton.interactable = interactable;
            if (_editButton != null) _editButton.interactable = interactable;
            if (_refreshButton != null) _refreshButton.interactable = interactable;
            if (_changeAvatarButton != null) _changeAvatarButton.interactable = interactable;
            if (_changeUsernameButton != null) _changeUsernameButton.interactable = interactable;
        }
        #endregion
    }
}

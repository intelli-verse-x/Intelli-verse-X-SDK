using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Backend.Nakama;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.V2.UI.Profile
{
    /// <summary>
    /// Controller for the Profile UI.
    /// Handles all business logic and communicates with IVXNProfileManager.
    /// </summary>
    [RequireComponent(typeof(IVXProfileView))]
    public class IVXProfileController : MonoBehaviour
    {
        #region Constants
        private const string LOG_PREFIX = "[IVX-PROFILE-UI]";
        #endregion

        #region Serialized Fields
        [Header("Configuration")]
        [SerializeField] private bool _autoFetchOnEnable = true;
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private float _avatarLoadTimeout = 10f;
        #endregion

        #region Private Fields
        private IVXProfileView _view;
        private CancellationTokenSource _cts;
        private bool _isEditMode;
        private IVXNProfileManager.IVXNProfileSnapshot _currentSnapshot;
        private int _selectedAvatarPresetId = -1;
        #endregion

        #region Events
        public event Action OnProfileSaved;
        public event Action OnProfileRefreshed;
        public event Action<string> OnError;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _view = GetComponent<IVXProfileView>();
            if (_view == null)
            {
                LogError("IVXProfileView component not found!");
                enabled = false;
                return;
            }

            SetupButtonListeners();
        }

        private void OnEnable()
        {
            IVXNProfileManager.OnProfileLoaded += HandleProfileLoaded;
            IVXNProfileManager.OnProfileUpdated += HandleProfileUpdated;
            IVXNProfileManager.OnUsernameChanged += HandleUsernameChanged;
            IVXNProfileManager.OnProfileError += HandleProfileError;

            if (_autoFetchOnEnable)
            {
                RefreshProfile();
            }
        }

        private void OnDisable()
        {
            IVXNProfileManager.OnProfileLoaded -= HandleProfileLoaded;
            IVXNProfileManager.OnProfileUpdated -= HandleProfileUpdated;
            IVXNProfileManager.OnUsernameChanged -= HandleUsernameChanged;
            IVXNProfileManager.OnProfileError -= HandleProfileError;

            CancelOperations();
        }

        private void OnDestroy()
        {
            CancelOperations();
        }
        #endregion

        #region Public Methods
        public void RefreshProfile()
        {
            _ = RefreshProfileAsync();
        }

        public async Task RefreshProfileAsync()
        {
            CancelOperations();
            _cts = new CancellationTokenSource();

            try
            {
                _view.SetLoading(true);
                _view.SetStatus("Loading profile...");
                _view.HideError();

                var result = await IVXNProfileManager.FetchProfileAsync(_cts.Token);

                if (result.Success)
                {
                    _currentSnapshot = result.Profile;
                    UpdateViewFromSnapshot();
                    _view.SetStatus("Profile loaded");
                    _view.SetViewMode(true);
                    OnProfileRefreshed?.Invoke();
                }
                else
                {
                    _view.ShowError(result.ErrorMessage ?? "Failed to load profile");
                    OnError?.Invoke(result.ErrorMessage);
                }
            }
            catch (OperationCanceledException)
            {
                Log("Profile refresh cancelled");
            }
            catch (Exception ex)
            {
                LogError($"Profile refresh failed: {ex.Message}");
                _view.ShowError($"Error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
            finally
            {
                _view.SetLoading(false);
            }
        }

        public void EnterEditMode()
        {
            _isEditMode = true;
            _view.SetViewMode(false);
            PopulateEditFields();
            _view.SetStatus("Editing profile...");
            _view.HideError();
        }

        public void CancelEdit()
        {
            _isEditMode = false;
            _selectedAvatarPresetId = -1;
            _view.SetViewMode(true);
            UpdateViewFromSnapshot();
            _view.SetStatus("");
            _view.HideError();
        }

        public void SaveProfile()
        {
            _ = SaveProfileAsync();
        }

        public async Task SaveProfileAsync()
        {
            if (!_isEditMode) return;

            CancelOperations();
            _cts = new CancellationTokenSource();

            try
            {
                _view.SetLoading(true);
                _view.SetStatus("Saving profile...");
                _view.HideError();

                var request = BuildUpdateRequest();
                var result = await IVXNProfileManager.UpdateProfileAsync(request, _cts.Token);

                if (result.Success)
                {
                    _currentSnapshot = result.Profile;
                    _isEditMode = false;
                    _selectedAvatarPresetId = -1;
                    _view.SetViewMode(true);
                    UpdateViewFromSnapshot();
                    _view.SetStatus("Profile saved successfully!");
                    OnProfileSaved?.Invoke();
                }
                else
                {
                    _view.ShowError(result.ErrorMessage ?? "Failed to save profile");
                    OnError?.Invoke(result.ErrorMessage);
                }
            }
            catch (OperationCanceledException)
            {
                Log("Profile save cancelled");
            }
            catch (Exception ex)
            {
                LogError($"Profile save failed: {ex.Message}");
                _view.ShowError($"Error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
            finally
            {
                _view.SetLoading(false);
            }
        }

        public void ChangeUsername()
        {
            _ = ChangeUsernameAsync();
        }

        public async Task ChangeUsernameAsync()
        {
            if (_view.UsernameInput == null) return;

            var newUsername = _view.UsernameInput.text?.Trim();
            if (string.IsNullOrEmpty(newUsername))
            {
                _view.ShowError("Username cannot be empty");
                return;
            }

            CancelOperations();
            _cts = new CancellationTokenSource();

            try
            {
                _view.SetLoading(true);
                _view.SetStatus("Changing username...");
                _view.HideError();

                var result = await IVXNProfileManager.ChangeUsernameAsync(newUsername, _cts.Token);

                if (result.Success)
                {
                    _currentSnapshot.Username = result.NewUsername;
                    UpdateViewFromSnapshot();
                    _view.SetStatus("Username changed successfully!");
                }
                else
                {
                    _view.ShowError(result.ErrorMessage ?? "Failed to change username");
                    OnError?.Invoke(result.ErrorMessage);
                }
            }
            catch (OperationCanceledException)
            {
                Log("Username change cancelled");
            }
            catch (Exception ex)
            {
                LogError($"Username change failed: {ex.Message}");
                _view.ShowError($"Error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
            finally
            {
                _view.SetLoading(false);
            }
        }

        public void SelectAvatarPreset(int presetId)
        {
            _selectedAvatarPresetId = presetId;
            if (_view.AvatarPresets != null && presetId >= 0 && presetId < _view.AvatarPresets.Length)
            {
                _view.SetAvatarSprite(_view.AvatarPresets[presetId]);
            }
            Log($"Selected avatar preset: {presetId}");
        }
        #endregion

        #region Private Methods
        private void SetupButtonListeners()
        {
            if (_view.EditButton != null)
            {
                _view.EditButton.onClick.AddListener(EnterEditMode);
            }

            if (_view.CancelButton != null)
            {
                _view.CancelButton.onClick.AddListener(CancelEdit);
            }

            if (_view.SaveButton != null)
            {
                _view.SaveButton.onClick.AddListener(SaveProfile);
            }

            if (_view.RefreshButton != null)
            {
                _view.RefreshButton.onClick.AddListener(RefreshProfile);
            }

            if (_view.ChangeUsernameButton != null)
            {
                _view.ChangeUsernameButton.onClick.AddListener(ChangeUsername);
            }

            SetupAvatarPresetButtons();
        }

        private void SetupAvatarPresetButtons()
        {
            if (_view.AvatarPresetContainer == null || _view.AvatarPresetButtonPrefab == null || _view.AvatarPresets == null)
            {
                return;
            }

            for (int i = 0; i < _view.AvatarPresets.Length; i++)
            {
                var presetId = i;
                var button = Instantiate(_view.AvatarPresetButtonPrefab, _view.AvatarPresetContainer);
                var image = button.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.sprite = _view.AvatarPresets[i];
                }
                button.onClick.AddListener(() => SelectAvatarPreset(presetId));
            }
        }

        private void UpdateViewFromSnapshot()
        {
            if (_currentSnapshot == null) return;

            var location = BuildLocationString(_currentSnapshot);

            _view.SetProfileData(
                _currentSnapshot.FullName,
                _currentSnapshot.Username,
                _currentSnapshot.Email,
                _currentSnapshot.UserId,
                location
            );

            if (!string.IsNullOrEmpty(_currentSnapshot.AvatarUrl))
            {
                _ = LoadAvatarAsync(_currentSnapshot.AvatarUrl);
            }
            else if (_currentSnapshot.AvatarPresetId >= 0 && 
                     _view.AvatarPresets != null && 
                     _currentSnapshot.AvatarPresetId < _view.AvatarPresets.Length)
            {
                _view.SetAvatarSprite(_view.AvatarPresets[_currentSnapshot.AvatarPresetId]);
            }
        }

        private void PopulateEditFields()
        {
            if (_currentSnapshot == null) return;

            _view.SetEditFieldValues(
                _currentSnapshot.DisplayName,
                _currentSnapshot.Username,
                _currentSnapshot.FirstName,
                _currentSnapshot.LastName,
                _currentSnapshot.AvatarUrl,
                _currentSnapshot.City,
                _currentSnapshot.Region,
                _currentSnapshot.Country
            );
        }

        private IVXNProfileManager.IVXNProfileUpdateRequest BuildUpdateRequest()
        {
            var request = new IVXNProfileManager.IVXNProfileUpdateRequest();

            if (_view.DisplayNameInput != null)
            {
                request.DisplayName = _view.DisplayNameInput.text?.Trim();
            }

            if (_view.FirstNameInput != null)
            {
                request.FirstName = _view.FirstNameInput.text?.Trim();
            }

            if (_view.LastNameInput != null)
            {
                request.LastName = _view.LastNameInput.text?.Trim();
            }

            if (_view.AvatarUrlInput != null)
            {
                request.AvatarUrl = _view.AvatarUrlInput.text?.Trim();
            }

            if (_view.CityInput != null)
            {
                request.City = _view.CityInput.text?.Trim();
            }

            if (_view.RegionInput != null)
            {
                request.Region = _view.RegionInput.text?.Trim();
            }

            if (_view.CountryInput != null)
            {
                request.Country = _view.CountryInput.text?.Trim();
            }

            if (_selectedAvatarPresetId >= 0)
            {
                request.AvatarPresetId = _selectedAvatarPresetId;
            }

            return request;
        }

        private string BuildLocationString(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrEmpty(snapshot.City))
            {
                parts.Add(snapshot.City);
            }

            if (!string.IsNullOrEmpty(snapshot.Region))
            {
                parts.Add(snapshot.Region);
            }

            if (!string.IsNullOrEmpty(snapshot.Country))
            {
                parts.Add(snapshot.Country);
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "";
        }

        private async Task LoadAvatarAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                using var request = UnityWebRequestTexture.GetTexture(url);
                request.timeout = (int)_avatarLoadTimeout;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                    if (_cts?.IsCancellationRequested == true) return;
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    var sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    _view.SetAvatarSprite(sprite);
                }
                else
                {
                    Log($"Failed to load avatar: {request.error}");
                }
            }
            catch (Exception ex)
            {
                Log($"Avatar load error: {ex.Message}");
            }
        }

        private void HandleProfileLoaded(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            _currentSnapshot = snapshot;
            if (!_isEditMode)
            {
                UpdateViewFromSnapshot();
            }
        }

        private void HandleProfileUpdated(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            _currentSnapshot = snapshot;
            UpdateViewFromSnapshot();
        }

        private void HandleUsernameChanged(IVXNProfileManager.IVXNUsernameChangeResult result)
        {
            if (result.Success && _currentSnapshot != null)
            {
                _currentSnapshot.Username = result.NewUsername;
                UpdateViewFromSnapshot();
            }
        }

        private void HandleProfileError(string error)
        {
            _view.ShowError(error);
        }

        private void CancelOperations()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void Log(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"{LOG_PREFIX} {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"{LOG_PREFIX} {message}");
        }
        #endregion
    }
}

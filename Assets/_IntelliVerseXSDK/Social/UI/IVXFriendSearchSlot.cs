using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// UI component for displaying a user in search results.
    /// Shows user info with add friend button.
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Social/Friend Search Slot")]
    public class IVXFriendSearchSlot : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Action Button")]
        [SerializeField] private Button addButton;
        [SerializeField] private TextMeshProUGUI addButtonText;
        [SerializeField] private Image addButtonImage;

        [Header("Button States")]
        [SerializeField] private Color addColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        [SerializeField] private Color pendingColor = new Color(0.6f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color friendColor = new Color(0.3f, 0.7f, 0.3f, 1f);

        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        [Header("Loading State")]
        [SerializeField] private GameObject loadingIndicator;

        #endregion

        #region Events

        /// <summary>Fired when the add button is clicked.</summary>
        public event Action<FriendSearchResult> OnAddClicked;

        #endregion

        #region Private Fields

        private FriendSearchResult _searchResult;
        private IVXFriendsConfig _config;
        private Coroutine _avatarLoadCoroutine;
        private bool _isProcessing;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _config = IVXFriendsConfig.Instance;

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            SetupButtons();
            SetLoadingState(false);
        }

        private void OnDestroy()
        {
            if (_avatarLoadCoroutine != null)
            {
                StopCoroutine(_avatarLoadCoroutine);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the slot with search result data.
        /// </summary>
        /// <param name="result">The search result data to display.</param>
        /// <param name="animationIndex">Index for staggered animation.</param>
        public void Initialize(FriendSearchResult result, int animationIndex = 0)
        {
            _searchResult = result;
            _isProcessing = false;

            // Set display name
            if (nameText != null)
            {
                nameText.text = result.displayName ?? "Unknown";
            }

            // Update button state based on relationship
            UpdateButtonState();

            // Load avatar
            LoadAvatar(result.avatarUrl);

            // Reset state
            SetLoadingState(false);

            // Animate appearance
            if (_config.enableSlotAnimations && canvasGroup != null && rectTransform != null)
            {
                IVXFriendsAnimations.AnimateSlotAppear(rectTransform, canvasGroup, animationIndex);
            }
        }

        /// <summary>
        /// Gets the search result data associated with this slot.
        /// </summary>
        public FriendSearchResult GetSearchResult() => _searchResult;

        /// <summary>
        /// Shows the loading state while processing.
        /// </summary>
        public void SetLoadingState(bool isLoading)
        {
            _isProcessing = isLoading;

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(isLoading);
            }

            if (addButton != null)
            {
                addButton.interactable = !isLoading && CanSendRequest();
            }
        }

        /// <summary>
        /// Updates the button to show "Pending" state after sending request.
        /// </summary>
        public void SetPendingState()
        {
            if (_searchResult != null)
            {
                _searchResult.requestPending = true;
                _searchResult.pendingDirection = "sent";
            }
            UpdateButtonState();
        }

        /// <summary>
        /// Plays success animation after sending request.
        /// </summary>
        public void PlaySuccessAnimation(Action onComplete = null)
        {
            IVXFriendsAnimations.AnimateSuccess(rectTransform, onComplete);
        }

        #endregion

        #region Private Methods

        private void SetupButtons()
        {
            if (addButton != null)
            {
                addButton.onClick.AddListener(OnAddButtonClicked);
            }
        }

        private void OnAddButtonClicked()
        {
            if (_isProcessing || !CanSendRequest()) return;

            if (addButton != null)
            {
                IVXFriendsAnimations.AnimateButtonPress(addButton.GetComponent<RectTransform>());
            }

            OnAddClicked?.Invoke(_searchResult);
        }

        private bool CanSendRequest()
        {
            if (_searchResult == null) return false;
            return !_searchResult.alreadyFriend && !_searchResult.requestPending;
        }

        private void UpdateButtonState()
        {
            if (addButton == null) return;

            if (_searchResult == null)
            {
                addButton.interactable = false;
                return;
            }

            // Already friends
            if (_searchResult.alreadyFriend)
            {
                addButton.interactable = false;
                if (addButtonText != null) addButtonText.text = "Friends";
                if (addButtonImage != null) addButtonImage.color = friendColor;
                if (statusText != null)
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = "Already friends";
                }
                return;
            }

            // Request pending
            if (_searchResult.requestPending)
            {
                addButton.interactable = false;
                
                string pendingText = _searchResult.pendingDirection == "received" 
                    ? "Respond" 
                    : "Pending";
                
                if (addButtonText != null) addButtonText.text = pendingText;
                if (addButtonImage != null) addButtonImage.color = pendingColor;
                if (statusText != null)
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = _searchResult.pendingDirection == "received"
                        ? "Sent you a request"
                        : "Request sent";
                }
                return;
            }

            // Can add
            addButton.interactable = true;
            if (addButtonText != null) addButtonText.text = "Add";
            if (addButtonImage != null) addButtonImage.color = addColor;
            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
        }

        private void LoadAvatar(string url)
        {
            // Set default avatar first
            if (avatarImage != null && _config.defaultAvatar != null)
            {
                avatarImage.sprite = _config.defaultAvatar;
            }

            // Load from URL if provided
            if (!string.IsNullOrEmpty(url) && avatarImage != null)
            {
                if (_avatarLoadCoroutine != null)
                {
                    StopCoroutine(_avatarLoadCoroutine);
                }
                _avatarLoadCoroutine = StartCoroutine(LoadAvatarCoroutine(url));
            }
        }

        private IEnumerator LoadAvatarCoroutine(string url)
        {
            using (var request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
#else
                if (!request.isNetworkError && !request.isHttpError)
#endif
                {
                    var texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                    if (texture != null && avatarImage != null)
                    {
                        var sprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f));
                        avatarImage.sprite = sprite;
                    }
                }
            }

            _avatarLoadCoroutine = null;
        }

        #endregion
    }
}

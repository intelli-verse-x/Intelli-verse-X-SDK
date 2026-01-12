using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// UI component for displaying an incoming friend request.
    /// Shows sender info with accept/reject buttons.
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Social/Friend Request Slot")]
    public class IVXFriendRequestSlot : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Action Buttons")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button rejectButton;

        [Header("Button Visuals")]
        [SerializeField] private Image acceptButtonImage;
        [SerializeField] private Image rejectButtonImage;
        [SerializeField] private Color acceptColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        [SerializeField] private Color rejectColor = new Color(0.7f, 0.2f, 0.2f, 1f);

        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        [Header("Loading State")]
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private GameObject buttonsContainer;

        #endregion

        #region Events

        /// <summary>Fired when the accept button is clicked.</summary>
        public event Action<FriendRequest> OnAcceptClicked;

        /// <summary>Fired when the reject button is clicked.</summary>
        public event Action<FriendRequest> OnRejectClicked;

        #endregion

        #region Private Fields

        private FriendRequest _requestData;
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
        /// Initializes the slot with friend request data.
        /// </summary>
        /// <param name="request">The friend request data to display.</param>
        /// <param name="animationIndex">Index for staggered animation.</param>
        public void Initialize(FriendRequest request, int animationIndex = 0)
        {
            _requestData = request;
            _isProcessing = false;

            // Set display name
            if (nameText != null)
            {
                nameText.text = request.fromDisplayName ?? "Unknown";
            }

            // Set time text
            if (timeText != null)
            {
                timeText.text = request.GetSentAtText();
            }

            // Set message (if any)
            if (messageText != null)
            {
                if (!string.IsNullOrEmpty(request.message))
                {
                    messageText.gameObject.SetActive(true);
                    messageText.text = $"\"{request.message}\"";
                }
                else
                {
                    messageText.gameObject.SetActive(false);
                }
            }

            // Apply button colors
            if (acceptButtonImage != null)
            {
                acceptButtonImage.color = acceptColor;
            }
            if (rejectButtonImage != null)
            {
                rejectButtonImage.color = rejectColor;
            }

            // Load avatar
            LoadAvatar(request.fromAvatarUrl);

            // Reset state
            SetLoadingState(false);

            // Animate appearance
            if (_config.enableSlotAnimations && canvasGroup != null && rectTransform != null)
            {
                IVXFriendsAnimations.AnimateSlotAppear(rectTransform, canvasGroup, animationIndex);
            }
        }

        /// <summary>
        /// Gets the request data associated with this slot.
        /// </summary>
        public FriendRequest GetRequestData() => _requestData;

        /// <summary>
        /// Shows the loading state while processing the request.
        /// </summary>
        public void SetLoadingState(bool isLoading)
        {
            _isProcessing = isLoading;

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(isLoading);
            }

            if (buttonsContainer != null)
            {
                buttonsContainer.SetActive(!isLoading);
            }

            if (acceptButton != null)
            {
                acceptButton.interactable = !isLoading;
            }

            if (rejectButton != null)
            {
                rejectButton.interactable = !isLoading;
            }
        }

        /// <summary>
        /// Animates the slot removal and invokes callback when complete.
        /// </summary>
        public void AnimateRemoval(Action onComplete)
        {
            if (rectTransform != null && canvasGroup != null)
            {
                IVXFriendsAnimations.AnimateSlotDisappear(rectTransform, canvasGroup, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Plays success animation (e.g., when accepted).
        /// </summary>
        public void PlaySuccessAnimation(Action onComplete = null)
        {
            IVXFriendsAnimations.AnimateSuccess(rectTransform, onComplete);
        }

        #endregion

        #region Private Methods

        private void SetupButtons()
        {
            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            }

            if (rejectButton != null)
            {
                rejectButton.onClick.AddListener(OnRejectButtonClicked);
            }
        }

        private void OnAcceptButtonClicked()
        {
            if (_isProcessing) return;

            if (acceptButton != null)
            {
                IVXFriendsAnimations.AnimateButtonPress(acceptButton.GetComponent<RectTransform>());
            }

            OnAcceptClicked?.Invoke(_requestData);
        }

        private void OnRejectButtonClicked()
        {
            if (_isProcessing) return;

            if (rejectButton != null)
            {
                IVXFriendsAnimations.AnimateButtonPress(rejectButton.GetComponent<RectTransform>());
            }

            OnRejectClicked?.Invoke(_requestData);
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

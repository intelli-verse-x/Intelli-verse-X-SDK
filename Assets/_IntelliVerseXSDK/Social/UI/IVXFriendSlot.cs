using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// UI component for displaying a single friend in the friends list.
    /// Attach to a prefab with avatar, name, status indicator, and action buttons.
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Social/Friend Slot")]
    public class IVXFriendSlot : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image statusIndicator;

        [Header("Action Buttons")]
        [SerializeField] private Button removeButton;
        [SerializeField] private Button blockButton;
        [SerializeField] private Button profileButton;

        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Sprite defaultAvatar;

        #endregion

        #region Events

        /// <summary>Fired when the remove button is clicked.</summary>
        public event Action<FriendInfo> OnRemoveClicked;

        /// <summary>Fired when the block button is clicked.</summary>
        public event Action<FriendInfo> OnBlockClicked;

        /// <summary>Fired when the profile button is clicked.</summary>
        public event Action<FriendInfo> OnProfileClicked;

        #endregion

        #region Private Fields

        private FriendInfo _friendData;
        private Coroutine _avatarLoadCoroutine;
        
        // Configuration constants
        private static readonly Color ONLINE_COLOR = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color OFFLINE_COLOR = new Color(0.5f, 0.5f, 0.5f);
        private const bool SHOW_ONLINE_STATUS = true;
        private const bool ENABLE_BLOCKING = true;
        private const bool ENABLE_SLOT_ANIMATIONS = true;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            SetupButtons();
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
        /// Initializes the slot with friend data.
        /// </summary>
        /// <param name="friend">The friend data to display.</param>
        /// <param name="animationIndex">Index for staggered animation (0 = first slot).</param>
        public void Initialize(FriendInfo friend, int animationIndex = 0)
        {
            _friendData = friend;

            // Set display name
            if (nameText != null)
            {
                nameText.text = friend.displayName ?? "Unknown";
            }

            // Set status text
            if (statusText != null)
            {
                statusText.text = friend.GetLastSeenText();
            }

            // Set online indicator
            if (statusIndicator != null && SHOW_ONLINE_STATUS)
            {
                statusIndicator.gameObject.SetActive(true);
                statusIndicator.color = friend.isOnline ? ONLINE_COLOR : OFFLINE_COLOR;
            }
            else if (statusIndicator != null)
            {
                statusIndicator.gameObject.SetActive(false);
            }

            // Configure buttons based on config
            if (blockButton != null)
            {
                blockButton.gameObject.SetActive(ENABLE_BLOCKING);
            }

            // Load avatar
            LoadAvatar(friend.avatarUrl);

            // Animate appearance
            if (ENABLE_SLOT_ANIMATIONS && canvasGroup != null && rectTransform != null)
            {
                IVXFriendsAnimations.AnimateSlotAppear(rectTransform, canvasGroup, animationIndex);
            }
        }

        /// <summary>
        /// Gets the friend data associated with this slot.
        /// </summary>
        public FriendInfo GetFriendData() => _friendData;

        /// <summary>
        /// Updates the online status display.
        /// </summary>
        public void UpdateOnlineStatus(bool isOnline)
        {
            if (_friendData != null)
            {
                _friendData.isOnline = isOnline;
            }

            if (statusIndicator != null && SHOW_ONLINE_STATUS)
            {
                statusIndicator.color = isOnline ? ONLINE_COLOR : OFFLINE_COLOR;
            }

            if (statusText != null)
            {
                statusText.text = isOnline ? "Online" : _friendData?.GetLastSeenText() ?? "";
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

        #endregion

        #region Private Methods

        private void SetupButtons()
        {
            if (removeButton != null)
            {
                removeButton.onClick.AddListener(OnRemoveButtonClicked);
            }

            if (blockButton != null)
            {
                blockButton.onClick.AddListener(OnBlockButtonClicked);
            }

            if (profileButton != null)
            {
                profileButton.onClick.AddListener(OnProfileButtonClicked);
            }
        }

        private void OnRemoveButtonClicked()
        {
            if (removeButton != null)
            {
                IVXFriendsAnimations.AnimateButtonPress(removeButton.GetComponent<RectTransform>());
            }
            OnRemoveClicked?.Invoke(_friendData);
        }

        private void OnBlockButtonClicked()
        {
            if (blockButton != null)
            {
                IVXFriendsAnimations.AnimateButtonPress(blockButton.GetComponent<RectTransform>());
            }
            OnBlockClicked?.Invoke(_friendData);
        }

        private void OnProfileButtonClicked()
        {
            if (profileButton != null)
            {
                IVXFriendsAnimations.AnimateButtonPress(profileButton.GetComponent<RectTransform>());
            }
            OnProfileClicked?.Invoke(_friendData);
        }

        private void LoadAvatar(string url)
        {
            // Set default avatar first
            if (avatarImage != null && defaultAvatar != null)
            {
                avatarImage.sprite = defaultAvatar;
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

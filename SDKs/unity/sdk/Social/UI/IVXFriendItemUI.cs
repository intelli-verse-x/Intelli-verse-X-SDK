// ============================================================================
// IVXFriendItemUI.cs - Friend list item (IApiFriend)
// ============================================================================
// Prefab: IVXFriendItem
// Structure: FriendItem (Horizontal Layout) | Avatar | NameSection | Actions
// Font: DisplayName 30 Bold, Username 22 Grey, Buttons 80 height
// ============================================================================

using System;
using Nakama;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// Friend list item. Setup with IApiFriend or IApiUser (search result).
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Social/Friend Item UI")]
    public class IVXFriendItemUI : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI displayNameText;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private Image avatarImage;

        [Header("Actions")]
        [SerializeField] private Button removeButton;
        [SerializeField] private Button blockButton;
        [SerializeField] private Button addButton;

        private IApiFriend _friend;
        private IApiUser _user;
        private string _userId;

        public void Setup(IApiFriend friend)
        {
            _friend = friend;
            _user = friend?.User;
            _userId = _user?.Id;
            ApplyDisplay();
            SetupFriendActions();
        }

        public void SetupFromUser(IApiUser user)
        {
            _friend = null;
            _user = user;
            _userId = user?.Id;
            ApplyDisplay();
            SetupAddAction();
        }

        private void ApplyDisplay()
        {
            if (displayNameText != null)
                displayNameText.text = _user?.DisplayName ?? _user?.Username ?? "Unknown";
            if (usernameText != null)
                usernameText.text = _user?.Username ?? "";

            if (removeButton != null) removeButton.gameObject.SetActive(_friend != null);
            if (blockButton != null) blockButton.gameObject.SetActive(_friend != null);
            if (addButton != null) addButton.gameObject.SetActive(_friend == null && _user != null);
        }

        private void SetupFriendActions()
        {
            if (removeButton != null)
                removeButton.onClick.RemoveAllListeners();
            if (blockButton != null)
                blockButton.onClick.RemoveAllListeners();
            if (removeButton != null)
                removeButton.onClick.AddListener(OnRemove);
            if (blockButton != null)
                blockButton.onClick.AddListener(OnBlock);
        }

        private void SetupAddAction()
        {
            if (addButton != null)
            {
                addButton.onClick.RemoveAllListeners();
                addButton.onClick.AddListener(OnAdd);
            }
        }

        private async void OnRemove()
        {
            if (string.IsNullOrEmpty(_userId)) return;
            try
            {
                await IVXFriendsManager.Instance.RemoveFriend(_userId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendItem] Remove: {ex.Message}");
            }
        }

        private async void OnBlock()
        {
            if (string.IsNullOrEmpty(_userId)) return;
            try
            {
                await IVXFriendsManager.Instance.BlockFriend(_userId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendItem] Block: {ex.Message}");
            }
        }

        private async void OnAdd()
        {
            if (string.IsNullOrEmpty(_userId)) return;
            try
            {
                await IVXFriendsManager.Instance.AddFriendByIdAsync(_userId);
                if (addButton != null)
                    addButton.interactable = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendItem] Add: {ex.Message}");
            }
        }
    }
}

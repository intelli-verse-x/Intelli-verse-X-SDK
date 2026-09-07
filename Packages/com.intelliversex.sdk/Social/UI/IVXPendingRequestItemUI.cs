// ============================================================================
// IVXPendingRequestItemUI.cs - Pending friend request item (Accept/Reject)
// ============================================================================
// Uses AddFriendsAsync to accept, DeleteFriendsAsync to reject.
// ============================================================================

using System;
using Nakama;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// Pending friend request item. Accept = AddFriendsAsync, Reject = DeleteFriendsAsync.
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Social/Pending Request Item UI")]
    public class IVXPendingRequestItemUI : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI displayNameText;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private Image avatarImage;

        [Header("Actions")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private GameObject buttonsContainer;

        private IApiFriend _friend;
        private string _userId;
        private bool _isProcessing;

        public void Setup(IApiFriend friend)
        {
            _friend = friend;
            _userId = friend?.User?.Id;

            if (displayNameText != null)
                displayNameText.text = friend?.User?.DisplayName ?? friend?.User?.Username ?? "Unknown";
            if (usernameText != null)
                usernameText.text = friend?.User?.Username ?? "";

            if (acceptButton != null)
            {
                acceptButton.onClick.RemoveAllListeners();
                acceptButton.onClick.AddListener(OnAccept);
            }
            if (rejectButton != null)
            {
                rejectButton.onClick.RemoveAllListeners();
                rejectButton.onClick.AddListener(OnReject);
            }
            SetProcessing(false);
        }

        private void SetProcessing(bool processing)
        {
            _isProcessing = processing;
            if (loadingIndicator != null)
                loadingIndicator.SetActive(processing);
            if (buttonsContainer != null)
                buttonsContainer.SetActive(!processing);
            if (acceptButton != null)
                acceptButton.interactable = !processing;
            if (rejectButton != null)
                rejectButton.interactable = !processing;
        }

        private async void OnAccept()
        {
            if (string.IsNullOrEmpty(_userId) || _isProcessing) return;
            SetProcessing(true);
            try
            {
                await IVXFriendsManager.Instance.AddFriendByIdAsync(_userId);
                Destroy(gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXPendingRequest] Accept: {ex.Message}");
                SetProcessing(false);
            }
        }

        private async void OnReject()
        {
            if (string.IsNullOrEmpty(_userId) || _isProcessing) return;
            SetProcessing(true);
            try
            {
                await IVXFriendsManager.Instance.RemoveFriendAsync(_userId);
                Destroy(gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXPendingRequest] Reject: {ex.Message}");
                SetProcessing(false);
            }
        }
    }
}

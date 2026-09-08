using System;
using System.Threading.Tasks;
using IntelliVerseX.Social;
using IntelliVerseX.Social.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Friends Demo")]
    public sealed class IVXUITKFriendsDemo : IVXUITKFeatureDemoBase
    {
        private TextField _usernameField;
        private IVXFriendsManager _manager;

        protected override void BuildFeatureUI()
        {
            SetTitle("Friends", "Refresh, search, and add via IVXFriendsManager");
            _usernameField = AddField("friendUsername", "Username to add/search");
            AddAction("Ensure Guest Session", () => _ = EnsureGuestAsync());
            AddAction("Refresh Friends", () => _ = RefreshAsync());
            AddAction("Search User", () => _ = SearchAsync());
            AddAction("Add Friend", () => _ = AddAsync());
            AddAction("Open Friends Panel", OpenPanel, "ivx-btn--ghost");
            EnsureManager();
            SetStatus("Friends demo ready.");
        }

        private void EnsureManager()
        {
            _manager = FindFirstObjectByType<IVXFriendsManager>();
            if (_manager == null)
            {
                var go = new GameObject("[IVXFriendsManager]");
                _manager = go.AddComponent<IVXFriendsManager>();
                DontDestroyOnLoad(go);
            }
        }

        private async Task EnsureGuestAsync()
        {
            SetBusy(true);
            try
            {
                var resp = await APIManager.GuestSignupAsync();
                SetStatus(resp?.data != null ? "Guest session ready." : resp?.message ?? "Guest failed.", resp?.data == null);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task RefreshAsync()
        {
            EnsureManager();
            SetBusy(true);
            SetStatus("Refreshing friends...");
            try
            {
                await _manager.RefreshFriendsAsync();
                var friends = await _manager.GetFriendsAsync();
                int count = friends?.Count ?? 0;
                if (count == 0)
                {
                    SetResult("No friends yet.");
                }
                else
                {
                    var lines = new System.Text.StringBuilder();
                    lines.AppendLine($"Friends ({count}):");
                    int shown = Math.Min(count, 12);
                    for (int i = 0; i < shown; i++)
                    {
                        var info = IVXFriendInfo.FromApiFriend(friends[i]);
                        lines.AppendLine("• " + (info?.displayName ?? friends[i]?.User?.Username ?? "?"));
                    }

                    if (count > shown)
                        lines.AppendLine($"…and {count - shown} more");
                    SetResult(lines.ToString().TrimEnd());
                }

                SetStatus("Friends refreshed.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SearchAsync()
        {
            EnsureManager();
            string username = _usernameField?.value?.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                SetStatus("Enter a username.", true);
                return;
            }

            SetBusy(true);
            try
            {
                var users = await _manager.SearchUsersAsync(username);
                SetResult($"Search hits: {users?.Count ?? 0}");
                SetStatus("Search complete.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task AddAsync()
        {
            EnsureManager();
            string username = _usernameField?.value?.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                SetStatus("Enter a username.", true);
                return;
            }

            SetBusy(true);
            try
            {
                var user = await _manager.SearchUserByUsernameAsync(username);
                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    SetStatus("User not found.", true);
                    return;
                }

                await _manager.AddFriendByIdAsync(user.Id);
                SetStatus($"Friend request sent to {username}.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void OpenPanel()
        {
            var panel = IVXFriendsPanel.Instance;
            if (panel != null)
            {
                panel.Open();
                SetStatus("Friends panel opened.");
            }
            else
            {
                SetStatus("IVXFriendsPanel not in scene.", true);
            }
        }
    }
}

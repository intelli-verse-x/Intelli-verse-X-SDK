# IntelliVerse-X SDK - Friends Module

**Version:** 2.0.0  
**Namespace:** `IntelliVerseX.Social`  
**Requires:** DOTween (free version works), Nakama Backend

---

## 🚀 Quick Start

### 1. Configure Nakama Backend

Ensure your Nakama backend is configured via the SDK Setup Wizard.

### 2. Add Prefabs to Your Scene

Add the `IVXFriendsCanvas` prefab to your scene, or use individual slot prefabs for custom UI.

Alternatively, open the demo scene: `Assets/Scenes/Tests/IVX_Friends.unity`

### 3. Open the Friends Panel

```csharp
using IntelliVerseX.Social.UI;

// Open the friends panel
IVXFriendsPanel.Instance.Open();

// Close the panel
IVXFriendsPanel.Instance.Close();
```

---

## 📁 Module Structure

```
Social/
├── Runtime/          # Friends service + models
├── Friends/          # IVXFriendsManager (native Nakama)
├── Clans/            # IVXClanService (get_user_groups / create_game_group + native)
├── Chat/             # IVXSocialChatService (native DM / clan channels)
├── Referral/         # IVXNakamaReferralService (hiro_incentives_*)
├── FriendStreak/     # Streaks + friend quests (prod RPCs)
├── UI/               # Friends / clan / referral panels
└── Prefabs/ …
```

**Prod usage guide:** [docs/guides/social-nakama-usage.md](../../../docs/guides/social-nakama-usage.md)

---

## Clans (quick)

```csharp
await IVXClanService.LoadCurrentClanAsync(client, session, gameId);
await IVXClanService.CreateClanAsync(client, session, gameId, "Guild", "", true, 50);
```

## Chat (quick)

```csharp
var dm = await IVXSocialChatService.JoinDirectAsync(socket, otherUserId);
await IVXSocialChatService.SendTextAsync(socket, dm.Id, "gg");
```

## Referral (Nakama)

```csharp
var code = await IVXNakamaReferralService.GetReferralCodeAsync(client, session);
```

---

## 🔧 Configuration

The Friends module uses **Nakama's native friend system** and does not require a separate configuration asset.

Configuration is managed via the `IVXFriendsService` static class and Nakama backend settings.
| `autoRefreshIntervalSeconds` | `60` | Auto-refresh interval (0 = disabled) |
| `defaultAvatar` | `null` | Fallback avatar sprite |

### Animation Settings

| Property | Default | Description |
|----------|---------|-------------|
| `panelAnimationDuration` | `0.3` | Panel open/close duration |
| `slotAnimationDuration` | `0.15` | Slot appear duration |
| `slotStaggerDelay` | `0.03` | Delay between slot animations |
| `enableSlotAnimations` | `true` | Enable slot animations |

---

## 📡 API Reference

### IVXFriendsService (Static)

All methods are async and use the SDK's session token automatically.

```csharp
using IntelliVerseX.Social;

// Get friends list
List<FriendInfo> friends = await IVXFriendsService.GetFriendsAsync();

// Get incoming friend requests
List<FriendRequest> requests = await IVXFriendsService.GetIncomingRequestsAsync();

// Search for users
List<FriendSearchResult> results = await IVXFriendsService.SearchUsersAsync("john");

// Send friend request
bool success = await IVXFriendsService.SendFriendRequestAsync("user123", "Hey!");

// Accept friend request
bool success = await IVXFriendsService.AcceptRequestAsync("request123");

// Reject friend request
bool success = await IVXFriendsService.RejectRequestAsync("request123");

// Remove friend
bool success = await IVXFriendsService.RemoveFriendAsync("friend123");

// Block user
bool success = await IVXFriendsService.BlockUserAsync("user123");
```

### Events

```csharp
// Subscribe to events
IVXFriendsService.OnFriendsListUpdated += (friends) => { /* ... */ };
IVXFriendsService.OnRequestsListUpdated += (requests) => { /* ... */ };
IVXFriendsService.OnNewRequestReceived += (request) => { /* ... */ };
IVXFriendsService.OnFriendAdded += (friend) => { /* ... */ };
IVXFriendsService.OnFriendRemoved += (userId) => { /* ... */ };
IVXFriendsService.OnError += (errorMessage) => { /* ... */ };
```

---

## 🎨 UI Components

### IVXFriendsPanel

Main panel controller with tabs for Friends, Requests, and Search.

```csharp
using IntelliVerseX.Social.UI;

// Get singleton instance
var panel = IVXFriendsPanel.Instance;

// Open/Close
panel.Open();
panel.Close();

// Refresh current tab
panel.RefreshCurrentTab();

// Show toast message
panel.ShowToast("Friend added!", isError: false);

// Events
panel.OnPanelOpened += () => { /* ... */ };
panel.OnPanelClosed += () => { /* ... */ };
panel.OnFriendSelected += (friend) => { /* Show profile */ };
```

### IVXFriendSlot

Individual friend row component.

```csharp
// Events
slot.OnRemoveClicked += (friend) => { /* ... */ };
slot.OnBlockClicked += (friend) => { /* ... */ };
slot.OnProfileClicked += (friend) => { /* ... */ };

// Methods
slot.Initialize(friendInfo, animationIndex);
slot.UpdateOnlineStatus(isOnline);
slot.AnimateRemoval(() => Destroy(slot.gameObject));
```

### IVXFriendRequestSlot

Friend request row component.

```csharp
// Events
slot.OnAcceptClicked += (request) => { /* ... */ };
slot.OnRejectClicked += (request) => { /* ... */ };

// Methods
slot.Initialize(request, animationIndex);
slot.SetLoadingState(isLoading);
slot.PlaySuccessAnimation();
```

### IVXFriendSearchSlot

Search result row component.

```csharp
// Events
slot.OnAddClicked += (result) => { /* ... */ };

// Methods
slot.Initialize(result, animationIndex);
slot.SetLoadingState(isLoading);
slot.SetPendingState();
```

---

## 🎬 Animations

The module uses DOTween for smooth animations. If DOTween is not installed, animations gracefully degrade to instant transitions.

### Using IVXFriendsAnimations

```csharp
using IntelliVerseX.Social.UI;

// Panel animations
IVXFriendsAnimations.AnimatePanelOpen(canvasGroup, rectTransform, onComplete);
IVXFriendsAnimations.AnimatePanelClose(canvasGroup, rectTransform, onComplete);

// Slot animations
IVXFriendsAnimations.AnimateSlotAppear(rectTransform, canvasGroup, index);
IVXFriendsAnimations.AnimateSlotDisappear(rectTransform, canvasGroup, onComplete);

// Button animations
IVXFriendsAnimations.AnimateButtonPress(buttonRect);
IVXFriendsAnimations.AnimateSuccess(targetRect, onComplete);

// Loading animations
IVXFriendsAnimations.StartLoadingPulse(canvasGroup);
IVXFriendsAnimations.StopLoadingPulse(canvasGroup);
IVXFriendsAnimations.StartSpinnerRotation(spinnerRect);
IVXFriendsAnimations.StopSpinnerRotation(spinnerRect);

// Tab animations
IVXFriendsAnimations.AnimateTabSwitch(outgoing, incoming, onSwitch, onComplete);
```

---

## 🔐 Authentication

The Friends module automatically uses the SDK's session management:

1. **Token Resolution:** Uses `APIManager.EnsureTokenForExternalUseAsync()` for Bearer tokens
2. **Auto-Refresh:** Automatically refreshes tokens on 401 responses
3. **Fallback:** Falls back to `UserSessionManager.AccessToken` if needed

**Requirement:** User must be logged in before using Friends features.

---

## 📱 Building Prefabs

If you want to create custom prefabs:

### Friend Slot Prefab

Required components:
- `IVXFriendSlot` component
- `Image` for avatar
- `TextMeshProUGUI` for name
- `Image` for status indicator
- `Button` for remove action

### Request Slot Prefab

Required components:
- `IVXFriendRequestSlot` component
- `Image` for avatar
- `TextMeshProUGUI` for name
- `Button` for accept (green)
- `Button` for reject (red)

### Search Slot Prefab

Required components:
- `IVXFriendSearchSlot` component
- `Image` for avatar
- `TextMeshProUGUI` for name
- `Button` for add action

---

## ❓ Troubleshooting

### "No valid session token" error

The user is not logged in. Ensure the user has authenticated via the SDK before opening the Friends panel.

### DOTween not working

1. Run the DOTween Setup Panel: `Tools → Demigiant → DOTween Utility Panel`
2. Click "Setup DOTween..."
3. Ensure the `DOTWEEN` scripting define is added

### Prefabs not appearing

1. Ensure the prefab references are assigned in the `IVXFriendsPanel` component
2. Check that prefabs have the correct slot components attached

### API errors

1. Check the Unity Console for detailed error messages
2. Ensure Nakama backend is properly configured
3. Verify that the user is authenticated before using Friends features

---

## 📄 License

Part of the IntelliVerse-X SDK. See main SDK license for terms.

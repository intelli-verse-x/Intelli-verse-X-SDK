# IVX_Friends Scene Setup Guide

**Version:** 1.0 | **Date:** 2026-02-27

---

## 1. Scene Root Structure

```
IVX_Friends_Scene
│
├── EventSystem
│    └── InputSystemUIInputModule  ✅ (NOT StandaloneInputModule)
│
├── Canvas (Screen Space - Overlay)
│    └── SafeArea
│         └── MainContainer (Vertical Layout Group)
│              ├── HeaderSection
│              ├── SearchSection
│              ├── PendingRequestsSection
│              ├── FriendsListSection
│              └── FooterControls
│
└── IVX_FriendsBootstrap (Empty GameObject)
     ├── IVXFriendsManager
     └── IVXFriendsSceneController
```

---

## 2. Manager Bootstrap Setup

**IVX_FriendsBootstrap** (Empty GameObject) should have:
- **IVXFriendsManager** – Add via Add Component
- **IVXFriendsSceneController** – Add via Add Component

**IVXFriendsSceneController** requires:
- Search: `searchInput` (TMP_InputField), `searchButton` (Button)
- Friends List: `friendsListRoot` (Transform), `friendItemPrefab` (GameObject)
- Pending List: `pendingListRoot` (Transform), `pendingItemPrefab` (GameObject)
- Optional: `statusText`, `loadingOverlay`

---

## 3. Main Container Settings

**Vertical Layout Group**
- Padding: 32
- Spacing: 24
- Child Control Size: ON

**Content Size Fitter** (on MainContainer)
- Vertical: Preferred Size

NO manual positioning.

---

## 4. Search Section

**SearchSection** (Horizontal Layout Group)
- Child: TMP_InputField
- Child: Search Button

**InputField**
- Placeholder: "Search by username"
- Font size: 32
- Height: 100
- Background: Dark grey
- Text color: White
- Caret color: White

---

## 5. Friends List Section

**FriendsListSection**
- Title (TextMeshPro "Friends")
- ScrollView
  - Movement: Vertical
  - Mask: enabled
  - Content (Vertical Layout Group)
    - Spacing: 16

`friendsListRoot` = ScrollView's Content transform

---

## 6. Friend Item Prefab (IVXFriendItem)

**Structure:**
```
FriendItem (Horizontal Layout Group)
├── Avatar (Image 80x80)
├── NameSection (Vertical Layout)
│    ├── DisplayName (TMP 30 Bold)
│    └── Username (TMP 22 Grey)
└── Actions (Horizontal Layout)
     ├── Remove Button (height 80)
     └── Block Button (height 80)
```

**Script:** `IVXFriendItemUI`
- Assign: displayNameText, usernameText, removeButton, blockButton
- For search results: addButton (optional)

---

## 7. Pending Request Prefab (IVXPendingRequestItem)

**Structure:** Same as FriendItem, but:
- Actions: Accept Button, Reject Button

**Script:** `IVXPendingRequestItemUI`
- Assign: displayNameText, usernameText, acceptButton, rejectButton
- Optional: loadingIndicator, buttonsContainer

**Accept** = `AddFriendsAsync(session, [userId])`  
**Reject** = `DeleteFriendsAsync(session, [userId])`

---

## 8. EventSystem

- Use **InputSystemUIInputModule** (NOT StandaloneInputModule)
- Only one EventSystem in scene

---

## 9. Realtime (Optional)

When `ISocket` is passed to `IVXFriendsManager.Initialize()`, the manager subscribes to:

- **ReceivedStatusPresence** → friend presence changes
- **ReceivedNotification** (subject `"friend_request"`) → new friend requests

For notifications, ensure Nakama sends notifications with subject `friend_request`. Minimal Lua:

```lua
nk.notification_send(user_id, "friend_request", { from = sender_id }, 1, sender_id)
```

---

## 10. Test Flow

| Step | Action |
|------|--------|
| 1 | Login with Account A |
| 2 | Search Account B, Add friend |
| 3 | Login with Account B, see pending request, Accept |
| 4 | Go back to Account A, see friend in list |
| 5 | Open Game B project, login same account → Friend appears (global) |

### Edge Case Tests

- Add self → should fail
- Add same user twice
- Remove friend twice
- Block then add (blocked user)
- Logout/login
- Expired token
- Network drop
- 100+ friends pagination

---

## 11. Final Checklist

- [ ] EventSystem uses InputSystemUIInputModule
- [ ] Only one EventSystem
- [ ] No HTTP API calls
- [ ] All calls use Nakama client (IVXFriendsManager)
- [ ] Proper ScrollView layout
- [ ] No overlapping UI
- [ ] Managers initialized once (IVX_FriendsBootstrap)
- [ ] Session valid before calls (IVXNManager initialized first)

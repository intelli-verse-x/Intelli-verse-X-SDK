# Friends Demo

Sample scene demonstrating the friends system.

---

## Scene Overview

**Location:** `Assets/_IntelliVerseXSDK/Samples/Scenes/IVX_Friends.unity`

This sample demonstrates:

- Sending friend requests
- Accepting/rejecting requests
- Viewing friends list
- Removing friends
- Blocking users

---

## Scene Hierarchy

```
Canvas
├── FriendsListPanel
│   ├── FriendsScrollView
│   │   └── FriendEntryPrefab
│   └── RefreshButton
├── RequestsPanel
│   ├── IncomingRequests
│   └── OutgoingRequests
├── AddFriendPanel
│   ├── SearchInput
│   ├── SearchButton
│   └── SearchResults
└── FriendProfilePopup
    ├── Avatar
    ├── DisplayName
    ├── RemoveButton
    └── BlockButton
```

---

## Key Components

### FriendsDemoController.cs

```csharp
using IntelliVerseX.Social;
using System.Collections.Generic;
using UnityEngine;

public class FriendsDemoController : MonoBehaviour
{
    [SerializeField] private Transform _friendsContainer;
    [SerializeField] private Transform _requestsContainer;
    [SerializeField] private GameObject _friendEntryPrefab;
    [SerializeField] private GameObject _requestEntryPrefab;
    
    async void Start()
    {
        await RefreshFriendsList();
        await RefreshRequests();
    }
    
    public async System.Threading.Tasks.Task RefreshFriendsList()
    {
        var friends = await IVXFriendsManager.Instance.GetFriendsAsync();
        
        ClearContainer(_friendsContainer);
        
        foreach (var friend in friends)
        {
            var entry = Instantiate(_friendEntryPrefab, _friendsContainer);
            var display = entry.GetComponent<FriendEntryDisplay>();
            display.Setup(friend, OnFriendSelected);
        }
    }
    
    public async System.Threading.Tasks.Task RefreshRequests()
    {
        var requests = await IVXFriendsManager.Instance.GetPendingRequestsAsync();
        
        ClearContainer(_requestsContainer);
        
        foreach (var request in requests)
        {
            var entry = Instantiate(_requestEntryPrefab, _requestsContainer);
            var display = entry.GetComponent<RequestEntryDisplay>();
            display.Setup(request, OnAcceptRequest, OnRejectRequest);
        }
    }
    
    public async void SearchUser(string username)
    {
        var results = await IVXFriendsManager.Instance.SearchUsersAsync(username);
        DisplaySearchResults(results);
    }
    
    public async void SendFriendRequest(string userId)
    {
        await IVXFriendsManager.Instance.SendFriendRequestAsync(userId);
        ShowMessage("Friend request sent!");
    }
    
    async void OnAcceptRequest(string userId)
    {
        await IVXFriendsManager.Instance.AcceptFriendRequestAsync(userId);
        await RefreshFriendsList();
        await RefreshRequests();
    }
    
    async void OnRejectRequest(string userId)
    {
        await IVXFriendsManager.Instance.RejectFriendRequestAsync(userId);
        await RefreshRequests();
    }
    
    void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
```

### FriendEntryDisplay.cs

```csharp
using IntelliVerseX.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendEntryDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _displayNameText;
    [SerializeField] private Image _statusIndicator;
    [SerializeField] private Button _viewButton;
    
    [SerializeField] private Color _onlineColor = Color.green;
    [SerializeField] private Color _offlineColor = Color.gray;
    
    private FriendInfo _friend;
    private System.Action<FriendInfo> _onSelect;
    
    public void Setup(FriendInfo friend, System.Action<FriendInfo> onSelect)
    {
        _friend = friend;
        _onSelect = onSelect;
        
        _displayNameText.text = friend.DisplayName;
        _statusIndicator.color = friend.IsOnline ? _onlineColor : _offlineColor;
        
        _viewButton.onClick.AddListener(() => _onSelect?.Invoke(_friend));
    }
}
```

---

## How to Use

### Running the Sample

1. Open `IVX_Friends.unity`
2. Ensure authenticated
3. Press **Play**

### Adding Friends

1. Click **"Add Friend"** tab
2. Enter username or user ID
3. Click **Search**
4. Click **Send Request** on result

### Managing Requests

1. Click **"Requests"** tab
2. View incoming/outgoing requests
3. Click **Accept** or **Reject**

### Viewing Friends

1. Click **"Friends"** tab
2. See all friends with online status
3. Click a friend to view profile

---

## Friend States

```csharp
public enum FriendState
{
    None = 0,          // No relationship
    Friend = 1,        // Mutual friends
    RequestSent = 2,   // You sent request
    RequestReceived = 3, // They sent request
    Blocked = 4        // Blocked by you
}
```

---

## Events

```csharp
void Start()
{
    // Subscribe to friend events
    IVXFriendsManager.OnFriendAdded += (friend) =>
    {
        Debug.Log($"New friend: {friend.DisplayName}");
        RefreshFriendsList();
    };
    
    IVXFriendsManager.OnFriendRemoved += (userId) =>
    {
        Debug.Log($"Friend removed: {userId}");
        RefreshFriendsList();
    };
    
    IVXFriendsManager.OnFriendRequestReceived += (request) =>
    {
        ShowNotification($"{request.DisplayName} wants to be friends!");
        RefreshRequests();
    };
}
```

---

## See Also

- [Social Module](../modules/social.md)
- [Backend Integration](../guides/nakama-integration.md)

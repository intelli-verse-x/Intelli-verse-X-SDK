# Social Module

The Social module provides friends management, sharing, referrals, and social features using native Nakama APIs.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Social` |
| **Assembly** | `IntelliVerseX.Social` |
| **Dependencies** | `IntelliVerseX.Backend`, Nakama Unity SDK |

---

## Key Classes

### IVXFriendsManager

100% Nakama-native friends management.

```csharp
public class IVXFriendsManager : MonoBehaviour
{
    public static IVXFriendsManager Instance { get; }
    
    // Initialize
    public void Initialize(IClient client, ISession session, ISocket socket = null);
    public bool InitializeFromNakamaManager();
    
    // Friend operations
    public async Task AddFriendByIdAsync(string userId, CancellationToken ct = default);
    public async Task AddFriendByUsernameAsync(string username, CancellationToken ct = default);
    public async Task RemoveFriendAsync(string userId, CancellationToken ct = default);
    public async Task BlockUserAsync(string userId, CancellationToken ct = default);
    public async Task UnblockUserAsync(string userId, CancellationToken ct = default);
    
    // Friend lists
    public async Task<List<IVXFriend>> GetFriendsAsync(int? state = null, int limit = 100, CancellationToken ct = default);
    public async Task<List<IVXFriend>> GetFriendRequestsAsync(CancellationToken ct = default);
    public async Task<List<IVXFriend>> GetBlockedUsersAsync(CancellationToken ct = default);
    
    // Refresh
    public async Task RefreshFriendsAsync(CancellationToken ct = default);
    
    // Events (via IVXFriendsEvents)
    public event Action<string> OnFriendRequestReceived;
}
```

**Usage:**
```csharp
using IntelliVerseX.Social;

public class FriendsController : MonoBehaviour
{
    private IVXFriendsManager _friendsManager;
    
    void Start()
    {
        _friendsManager = IVXFriendsManager.Instance;
        
        // Initialize from existing Nakama manager
        if (!_friendsManager.InitializeFromNakamaManager())
        {
            Debug.LogError("Failed to initialize friends manager");
            return;
        }
        
        // Subscribe to events
        IVXFriendsEvents.OnFriendAdded += HandleFriendAdded;
        IVXFriendsEvents.OnFriendRemoved += HandleFriendRemoved;
        IVXFriendsEvents.OnFriendRequestReceived += HandleFriendRequest;
    }
    
    async void LoadFriends()
    {
        var friends = await _friendsManager.GetFriendsAsync();
        foreach (var friend in friends)
        {
            Debug.Log($"Friend: {friend.DisplayName} (Online: {friend.IsOnline})");
        }
    }
    
    async void SendFriendRequest(string username)
    {
        try
        {
            await _friendsManager.AddFriendByUsernameAsync(username);
            ShowToast("Friend request sent!");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }
    
    void HandleFriendRequest(string fromUserId)
    {
        ShowFriendRequestUI(fromUserId);
    }
}
```

---

### IVXFriendsEvents

Static event hub for friend-related events.

```csharp
public static class IVXFriendsEvents
{
    public static event Action<string> OnFriendAdded;
    public static event Action<string> OnFriendRemoved;
    public static event Action<string> OnFriendRequestReceived;
    public static event Action<string> OnFriendRequestAccepted;
    public static event Action<string> OnFriendPresenceChanged;
    public static event Action<string> OnUserBlocked;
    public static event Action<string> OnUserUnblocked;
    public static event Action<string> OnFriendsError;
}
```

---

### IVXFriend

Friend data model.

```csharp
public class IVXFriend
{
    public string UserId { get; }
    public string Username { get; }
    public string DisplayName { get; }
    public string AvatarUrl { get; }
    public bool IsOnline { get; }
    public DateTime LastSeen { get; }
    public IVXFriendState State { get; }
}

public enum IVXFriendState
{
    Friend = 0,         // Mutual friends
    InviteSent = 1,     // Friend request sent
    InviteReceived = 2, // Friend request received
    Blocked = 3         // User is blocked
}
```

---

### IVXShareService

Native sharing functionality.

```csharp
public class IVXShareService
{
    public static void ShareText(string text);
    public static void ShareImage(Texture2D image, string text = null);
    public static void ShareScreenshot(string text = null);
    public static void ShareURL(string url, string title = null);
}
```

**Usage:**
```csharp
// Share text
IVXShareService.ShareText("I just scored 1000 points in MyGame!");

// Share with URL
IVXShareService.ShareURL(
    "https://play.google.com/store/apps/details?id=com.mygame",
    "Download MyGame!"
);

// Share screenshot
IVXShareService.ShareScreenshot("Check out my high score!");
```

---

### IVXGRateAppManager

Rate app prompt management.

```csharp
public class IVXGRateAppManager : MonoBehaviour
{
    public static IVXGRateAppManager Instance { get; }
    
    public bool ShouldShowRatePrompt { get; }
    public int SessionCount { get; }
    public int ActionCount { get; }
    
    public void RecordAction();
    public void ShowRatePrompt();
    public void OpenStoreReview();
    public void NeverAskAgain();
}
```

**Usage:**
```csharp
// Track significant actions
IVXGRateAppManager.Instance.RecordAction();

// Check if should prompt (based on session/action count)
if (IVXGRateAppManager.Instance.ShouldShowRatePrompt)
{
    ShowRateAppDialog();
}

// Open native review (iOS/Android)
IVXGRateAppManager.Instance.OpenStoreReview();
```

---

## Friend States

| State | Value | Description |
|-------|-------|-------------|
| Friend | 0 | Mutual friendship |
| InviteSent | 1 | Request sent, pending |
| InviteReceived | 2 | Request received |
| Blocked | 3 | User is blocked |

---

## Usage Examples

### Complete Friends Flow

```csharp
public class FriendsUI : MonoBehaviour
{
    [SerializeField] private Transform friendsListContainer;
    [SerializeField] private Transform requestsListContainer;
    [SerializeField] private TMP_InputField searchInput;
    
    private IVXFriendsManager _manager;
    
    async void Start()
    {
        _manager = IVXFriendsManager.Instance;
        _manager.InitializeFromNakamaManager();
        
        await RefreshAll();
    }
    
    async Task RefreshAll()
    {
        // Get friends
        var friends = await _manager.GetFriendsAsync(
            state: (int)IVXFriendState.Friend
        );
        DisplayFriends(friends);
        
        // Get pending requests
        var requests = await _manager.GetFriendRequestsAsync();
        DisplayRequests(requests);
    }
    
    async void OnSearchSubmit()
    {
        string username = searchInput.text.Trim();
        if (string.IsNullOrEmpty(username)) return;
        
        try
        {
            await _manager.AddFriendByUsernameAsync(username);
            ShowToast($"Friend request sent to {username}");
        }
        catch (ApiResponseException ex)
        {
            if (ex.Message.Contains("not found"))
                ShowToast("User not found");
            else if (ex.Message.Contains("already"))
                ShowToast("Already friends or request pending");
            else
                ShowToast(ex.Message);
        }
    }
    
    async void AcceptRequest(string userId)
    {
        await _manager.AddFriendByIdAsync(userId);
        await RefreshAll();
    }
    
    async void RemoveFriend(string userId)
    {
        await _manager.RemoveFriendAsync(userId);
        await RefreshAll();
    }
}
```

### Referral System

```csharp
public class ReferralManager : MonoBehaviour
{
    private const string REFERRAL_URL = "https://myga.me/ref/";
    
    public string GetReferralCode()
    {
        return IntelliVerseXUserIdentity.UserId;
    }
    
    public string GetReferralLink()
    {
        return REFERRAL_URL + GetReferralCode();
    }
    
    public void ShareReferralLink()
    {
        string message = $"Join me in MyGame! Use my referral code: {GetReferralCode()}";
        IVXShareService.ShareURL(GetReferralLink(), message);
    }
    
    public async Task ApplyReferralCode(string code)
    {
        // Validate and apply via backend RPC
        var response = await nakamaManager.Client.RpcAsync(
            nakamaManager.Session,
            "apply_referral",
            JsonConvert.SerializeObject(new { referralCode = code })
        );
        
        // Handle response
    }
}
```

---

## Realtime Presence

When socket is connected, friends' online status updates in realtime:

```csharp
void Start()
{
    // Subscribe to presence changes
    IVXFriendsEvents.OnFriendPresenceChanged += HandlePresenceChanged;
}

void HandlePresenceChanged(string userId)
{
    // Refresh friend's status in UI
    RefreshFriendStatus(userId);
}
```

---

## Best Practices

1. **Initialize Early** - Initialize friends manager after Nakama connects
2. **Cache Locally** - Use `IVXFriendsCache` for local caching
3. **Handle Errors** - Subscribe to `OnFriendsError` for error handling
4. **Refresh on Resume** - Refresh friends list when app resumes
5. **Validate Input** - Use `IVXFriendsValidator` before operations

---

## Platform Notes

| Feature | Android | iOS | WebGL |
|---------|---------|-----|-------|
| Friends | ✅ | ✅ | ✅ |
| Sharing | ✅ | ✅ | ✅ |
| Rate App | ✅ | ✅ | ❌ |
| Screenshot Share | ✅ | ✅ | ✅ |

---

## Related Documentation

- [Backend Module](backend.md) - Nakama integration
- [Friends Demo](../samples/friends-demo.md) - Sample implementation

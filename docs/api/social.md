# Social API Reference

Complete API reference for the Social module.

---

## IVXFriendsManager

Friends and social relationships manager.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXFriendsManager` | Singleton instance |
| `Friends` | `List<FriendInfo>` | Cached friends list |
| `IsInitialized` | `bool` | Initialization status |

---

### Methods

#### GetFriendsAsync

```csharp
public async Task<List<FriendInfo>> GetFriendsAsync(int limit = 100)
```

Gets the user's friends list.

---

#### SendFriendRequestAsync

```csharp
public async Task SendFriendRequestAsync(string userId)
```

Sends a friend request.

---

#### AcceptFriendRequestAsync

```csharp
public async Task AcceptFriendRequestAsync(string userId)
```

Accepts an incoming friend request.

---

#### RejectFriendRequestAsync

```csharp
public async Task RejectFriendRequestAsync(string userId)
```

Rejects an incoming friend request.

---

#### RemoveFriendAsync

```csharp
public async Task RemoveFriendAsync(string userId)
```

Removes a friend.

---

#### BlockUserAsync

```csharp
public async Task BlockUserAsync(string userId)
```

Blocks a user.

---

#### UnblockUserAsync

```csharp
public async Task UnblockUserAsync(string userId)
```

Unblocks a user.

---

#### GetPendingRequestsAsync

```csharp
public async Task<List<FriendRequest>> GetPendingRequestsAsync()
```

Gets incoming friend requests.

---

#### SearchUsersAsync

```csharp
public async Task<List<UserSearchResult>> SearchUsersAsync(string query, int limit = 20)
```

Searches for users by username.

---

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnFriendAdded` | `Action<FriendInfo>` | Friend added |
| `OnFriendRemoved` | `Action<string>` | Friend removed |
| `OnFriendRequestReceived` | `Action<FriendRequest>` | Request received |
| `OnFriendOnlineStatusChanged` | `Action<string, bool>` | Online status changed |

---

## FriendInfo

Friend data class.

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | `string` | User ID |
| `DisplayName` | `string` | Display name |
| `AvatarUrl` | `string` | Avatar URL |
| `IsOnline` | `bool` | Online status |
| `State` | `FriendState` | Relationship state |

---

## FriendState Enum

```csharp
public enum FriendState
{
    None = 0,
    Friend = 1,
    RequestSent = 2,
    RequestReceived = 3,
    Blocked = 4
}
```

---

## IVXShareManager

Sharing and referral functionality.

### Methods

#### ShareText

```csharp
public void ShareText(string text, string subject = null)
```

Opens system share dialog with text.

---

#### ShareScreenshot

```csharp
public async Task ShareScreenshot(string message = null)
```

Captures and shares a screenshot.

---

#### GenerateReferralCode

```csharp
public string GenerateReferralCode()
```

Generates a referral code for the current user.

---

#### ApplyReferralCode

```csharp
public async Task<bool> ApplyReferralCode(string code)
```

Applies a referral code.

---

## See Also

- [Social Module Guide](../modules/social.md)

# Friends Integration Guide

Learn how to integrate the friends system into your game.

## Prerequisites

- Backend configured with Nakama
- User authentication working

## Setup

### 1. Initialize Friends Manager

```csharp
using IntelliVerseX.Social;

private void Start()
{
    IVXFriendsManager.Instance.OnFriendsListUpdated += HandleFriendsUpdate;
}
```

### 2. Send Friend Request

```csharp
await IVXFriendsManager.Instance.SendFriendRequestAsync(userId);
```

### 3. Accept Friend Request

```csharp
await IVXFriendsManager.Instance.AcceptFriendRequestAsync(requestId);
```

## See Also

- [Social Module](../modules/social.md)
- [Friends Demo](../samples/friends-demo.md)

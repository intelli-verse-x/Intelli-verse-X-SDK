# Social Features Sample

This sample demonstrates the social features of the IntelliVerseX SDK including friends, sharing, and referrals.

## Features Demonstrated

- Friends system (add, remove, list)
- Native sharing (screenshots, text)
- Referral system
- User profiles

## Setup

1. Import this sample via Package Manager
2. Open the `SocialDemoScene` scene
3. Configure backend settings
4. Press Play

## Key Components

### Friends System
```csharp
using IntelliVerseX.Social;

// Search for users
var users = await IVXSocialManager.SearchUsersAsync("username");

// Send friend request
await IVXSocialManager.SendFriendRequestAsync(userId);

// Accept friend request
await IVXSocialManager.AcceptFriendRequestAsync(requestId);

// Get friends list
var friends = await IVXSocialManager.GetFriendsAsync();
```

### Native Sharing
```csharp
using IntelliVerseX.Social;

// Share text
IVXShareManager.ShareText("Check out my score!", "My Game");

// Share screenshot
await IVXShareManager.ShareScreenshotAsync("My high score!");
```

### Referral System
```csharp
using IntelliVerseX.Social;

// Get referral code
string code = IVXReferralManager.GetReferralCode();

// Apply referral code
bool success = await IVXReferralManager.ApplyReferralCodeAsync(code);

// Check referral rewards
var rewards = await IVXReferralManager.GetReferralRewardsAsync();
```

## Dependencies

- IntelliVerseX.Core
- IntelliVerseX.Social
- IntelliVerseX.Backend
- Native Share (external - com.yasirkula.nativeshare)

## UI Prefabs

- `FriendsPanel` - Complete friends management UI
- `FriendSlot` - Individual friend entry
- `FriendRequestSlot` - Pending request entry
- `SharePanel` - Sharing options
- `ReferralPanel` - Referral code display and input

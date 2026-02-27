# Identity API Reference

Complete API reference for the Identity module.

---

## IntelliVerseXUserIdentity

Main identity and authentication manager.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IntelliVerseXUserIdentity` | Singleton instance |
| `CurrentUser` | `IVXUserProfile` | Currently authenticated user |
| `IsAuthenticated` | `bool` | Authentication status |
| `HasValidSession` | `bool` | Session validity |
| `AuthState` | `AuthState` | Current auth state |

---

### Authentication Methods

#### AuthenticateGuestAsync

```csharp
public async Task<IVXUserProfile> AuthenticateGuestAsync()
```

Creates or restores a guest account.

**Returns:** User profile

**Throws:** `AuthException` on failure

**Example:**
```csharp
var user = await IntelliVerseXUserIdentity.Instance.AuthenticateGuestAsync();
Debug.Log($"Guest: {user.DisplayName}");
```

---

#### LoginWithEmailAsync

```csharp
public async Task<IVXUserProfile> LoginWithEmailAsync(string email, string password)
```

Authenticates with email/password.

**Parameters:**
- `email` - User email
- `password` - User password

**Returns:** User profile

**Throws:** 
- `AuthException` - Invalid credentials
- `NetworkException` - Connection failed

**Example:**
```csharp
var user = await IntelliVerseXUserIdentity.Instance
    .LoginWithEmailAsync("user@email.com", "password123");
```

---

#### RegisterWithEmailAsync

```csharp
public async Task<IVXUserProfile> RegisterWithEmailAsync(
    string email, 
    string password, 
    string displayName = null)
```

Creates a new account with email/password.

**Parameters:**
- `email` - User email
- `password` - Password (min 8 characters)
- `displayName` - Optional display name

**Returns:** New user profile

**Throws:** `AuthException` if email exists

---

#### LoginWithGoogleAsync

```csharp
public async Task<IVXUserProfile> LoginWithGoogleAsync()
```

Initiates Google Sign-In flow.

**Returns:** User profile

**Throws:** `AuthException` on failure or cancellation

---

#### LoginWithAppleAsync

```csharp
public async Task<IVXUserProfile> LoginWithAppleAsync()
```

Initiates Apple Sign-In flow (iOS/macOS only).

**Returns:** User profile

**Platform:** iOS, macOS

---

#### LoginWithFacebookAsync

```csharp
public async Task<IVXUserProfile> LoginWithFacebookAsync()
```

Initiates Facebook Login flow.

**Returns:** User profile

---

### Session Methods

#### TryRestoreSessionAsync

```csharp
public async Task<bool> TryRestoreSessionAsync()
```

Attempts to restore a previous session.

**Returns:** `true` if session restored

**Example:**
```csharp
if (await IntelliVerseXUserIdentity.Instance.TryRestoreSessionAsync())
{
    // User is logged in
}
else
{
    ShowLoginScreen();
}
```

---

#### RefreshSessionAsync

```csharp
public async Task RefreshSessionAsync()
```

Refreshes the current session token.

---

#### LogoutAsync

```csharp
public async Task LogoutAsync()
```

Logs out the current user and clears session.

**Example:**
```csharp
await IntelliVerseXUserIdentity.Instance.LogoutAsync();
```

---

### Account Linking

#### LinkEmailAsync

```csharp
public async Task LinkEmailAsync(string email, string password)
```

Links email/password to current account.

**Parameters:**
- `email` - Email to link
- `password` - Password for the email

**Throws:** `AuthException` if email already linked

---

#### LinkGoogleAsync

```csharp
public async Task LinkGoogleAsync()
```

Links Google account to current user.

---

#### LinkAppleAsync

```csharp
public async Task LinkAppleAsync()
```

Links Apple account to current user.

---

### Password Management

#### SendPasswordResetAsync

```csharp
public async Task SendPasswordResetAsync(string email)
```

Sends password reset email.

**Parameters:**
- `email` - Account email

---

#### ChangePasswordAsync

```csharp
public async Task ChangePasswordAsync(string currentPassword, string newPassword)
```

Changes the user's password.

---

### Profile Management

#### UpdateDisplayNameAsync

```csharp
public async Task UpdateDisplayNameAsync(string displayName)
```

Updates the user's display name.

---

#### UpdateAvatarAsync

```csharp
public async Task UpdateAvatarAsync(string avatarUrl)
```

Updates the user's avatar URL.

---

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnAuthStateChanged` | `Action<AuthState>` | Auth state changed |
| `OnUserUpdated` | `Action<IVXUserProfile>` | Profile updated |
| `OnSessionExpired` | `Action` | Session expired |

**Example:**
```csharp
IntelliVerseXUserIdentity.OnAuthStateChanged += (state) =>
{
    if (state == AuthState.Authenticated)
        ShowMainMenu();
    else
        ShowLoginScreen();
};
```

---

## IVXUserProfile

User profile data class.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | `string` | Unique user ID |
| `DisplayName` | `string` | Display name |
| `Email` | `string` | Email (if set) |
| `AvatarUrl` | `string` | Avatar image URL |
| `IsGuest` | `bool` | Guest account flag |
| `CreatedAt` | `DateTime` | Account creation time |
| `LinkedProviders` | `List<string>` | Linked auth providers |

---

## AuthState Enum

```csharp
public enum AuthState
{
    Unknown,
    Unauthenticated,
    Authenticating,
    Authenticated,
    Expired
}
```

---

## AuthException

Authentication-specific exception.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ErrorCode` | `AuthErrorCode` | Error type |
| `Message` | `string` | Error message |

### Error Codes

| Code | Description |
|------|-------------|
| `InvalidCredentials` | Wrong email/password |
| `EmailAlreadyExists` | Email taken |
| `WeakPassword` | Password too weak |
| `AccountDisabled` | Account suspended |
| `NetworkError` | Connection failed |
| `Cancelled` | User cancelled |

---

## See Also

- [Identity Module Guide](../modules/identity.md)
- [Authentication Flow Guide](../guides/auth-flow.md)

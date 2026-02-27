# Authentication Demo

Sample scene demonstrating user authentication flows.

---

## Scene Overview

**Location:** `Assets/_IntelliVerseXSDK/Samples/Scenes/IVX_AuthTest.unity`

This sample demonstrates:

- Guest login
- Email/password registration
- Email/password login
- Social sign-in (Google, Apple, Facebook)
- Account linking
- Logout

---

## Scene Hierarchy

```
Canvas
├── LoginPanel
│   ├── EmailField
│   ├── PasswordField
│   ├── LoginButton
│   ├── RegisterButton
│   └── GuestButton
├── RegisterPanel
│   ├── EmailField
│   ├── PasswordField
│   ├── ConfirmPasswordField
│   └── RegisterButton
├── SocialLoginPanel
│   ├── GoogleButton
│   ├── AppleButton
│   └── FacebookButton
├── UserInfoPanel
│   ├── DisplayNameText
│   ├── UserIdText
│   ├── EmailText
│   └── LogoutButton
└── LoadingOverlay
```

---

## Key Components

### AuthDemoController.cs

```csharp
using IntelliVerseX.Identity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthDemoController : MonoBehaviour
{
    [Header("Login Panel")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private TMP_InputField _loginEmail;
    [SerializeField] private TMP_InputField _loginPassword;
    
    [Header("User Info Panel")]
    [SerializeField] private GameObject _userPanel;
    [SerializeField] private TMP_Text _displayNameText;
    [SerializeField] private TMP_Text _userIdText;
    
    [Header("Loading")]
    [SerializeField] private GameObject _loadingOverlay;
    
    async void Start()
    {
        // Check for existing session
        if (await IntelliVerseXUserIdentity.Instance.TryRestoreSessionAsync())
        {
            ShowUserPanel();
        }
        else
        {
            ShowLoginPanel();
        }
    }
    
    public async void OnLoginClick()
    {
        ShowLoading(true);
        try
        {
            await IntelliVerseXUserIdentity.Instance.LoginWithEmailAsync(
                _loginEmail.text,
                _loginPassword.text
            );
            ShowUserPanel();
        }
        catch (System.Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            ShowLoading(false);
        }
    }
    
    public async void OnGuestClick()
    {
        ShowLoading(true);
        try
        {
            await IntelliVerseXUserIdentity.Instance.AuthenticateGuestAsync();
            ShowUserPanel();
        }
        finally
        {
            ShowLoading(false);
        }
    }
    
    void ShowUserPanel()
    {
        _loginPanel.SetActive(false);
        _userPanel.SetActive(true);
        
        var user = IntelliVerseXUserIdentity.Instance.CurrentUser;
        _displayNameText.text = user.DisplayName;
        _userIdText.text = user.UserId;
    }
}
```

---

## How to Use

### Running the Sample

1. Open `IVX_AuthTest.unity`
2. Ensure `IntelliVerseXConfig` is configured
3. Press **Play**

### Testing Guest Login

1. Click **"Play as Guest"**
2. Observe auto-generated user profile
3. Note: Account is device-bound

### Testing Email Login

1. Click **"Register"** tab
2. Enter email and password
3. Click **"Create Account"**
4. Use same credentials to **"Login"**

### Testing Social Login

1. Configure social providers in `IntelliVerseXConfig`
2. Click **Google/Apple/Facebook** button
3. Complete OAuth flow

---

## Code Walkthrough

### Session Restoration

```csharp
// On app start, try to restore previous session
if (await IntelliVerseXUserIdentity.Instance.TryRestoreSessionAsync())
{
    // User is already logged in
    GoToMainMenu();
}
else
{
    // Show login options
    ShowAuthScreen();
}
```

### Error Handling

```csharp
try
{
    await IntelliVerseXUserIdentity.Instance.LoginWithEmailAsync(email, password);
}
catch (AuthException ex)
{
    switch (ex.ErrorCode)
    {
        case AuthErrorCode.InvalidCredentials:
            ShowError("Wrong email or password");
            break;
        case AuthErrorCode.NetworkError:
            ShowError("Please check your internet connection");
            break;
        default:
            ShowError("Login failed. Please try again.");
            break;
    }
}
```

---

## Customization

### Change Auth Flow

Edit `AuthDemoController.cs` to customize:

- Default panel shown
- Required fields
- Validation rules
- Success/error handling

### Update UI

Modify the Canvas elements to match your game's art style.

---

## See Also

- [Identity Module](../modules/identity.md)
- [Authentication Flow Guide](../guides/auth-flow.md)

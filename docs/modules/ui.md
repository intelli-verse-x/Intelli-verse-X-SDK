# UI Module

The UI module provides pre-built UI components, panels, and utilities for common game screens.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.UI` |
| **Assembly** | `IntelliVerseX.UI` |
| **Framework** | Unity UI (uGUI) + TextMeshPro |

---

## Components

| Component | Purpose |
|-----------|---------|
| `IVXPopupManager` | Modal popup system |
| `IVXToastManager` | Toast notifications |
| `IVXLoadingOverlay` | Loading indicators |
| `IVXScreenManager` | Screen navigation |
| `IVXAnimatedButton` | Animated button component |

---

## IVXPopupManager

Centralized popup/modal management.

```csharp
public static class IVXPopupManager
{
    // Show basic popup
    public static void Show(string title, string message);
    
    // Show with callback
    public static void Show(string title, string message, Action onClose);
    
    // Show confirmation dialog
    public static void ShowConfirm(
        string title, 
        string message,
        Action onConfirm,
        Action onCancel = null,
        string confirmText = "Confirm",
        string cancelText = "Cancel");
    
    // Show custom popup
    public static T ShowCustom<T>(string prefabPath) where T : IVXPopupBase;
    
    // Close
    public static void Close();
    public static void CloseAll();
    
    // State
    public static bool IsPopupOpen { get; }
}
```

**Usage:**
```csharp
// Simple alert
IVXPopupManager.Show("Welcome!", "Thanks for playing!");

// Confirmation
IVXPopupManager.ShowConfirm(
    "Purchase",
    "Buy 100 coins for $0.99?",
    onConfirm: () => ProcessPurchase(),
    onCancel: () => Debug.Log("Cancelled")
);

// Custom popup
var settingsPopup = IVXPopupManager.ShowCustom<SettingsPopup>("UI/SettingsPopup");
settingsPopup.Initialize(currentSettings);
```

---

## IVXToastManager

Non-blocking toast notifications.

```csharp
public static class IVXToastManager
{
    // Show toast
    public static void Show(string message);
    public static void Show(string message, float duration);
    public static void Show(string message, ToastType type);
    
    // With icon
    public static void Show(string message, Sprite icon);
}

public enum ToastType
{
    Default,
    Success,
    Warning,
    Error,
    Info
}
```

**Usage:**
```csharp
// Simple toast
IVXToastManager.Show("Item purchased!");

// Success toast
IVXToastManager.Show("Level complete!", ToastType.Success);

// Error toast
IVXToastManager.Show("Connection failed", ToastType.Error);

// With custom duration
IVXToastManager.Show("Processing...", duration: 5f);
```

---

## IVXLoadingOverlay

Full-screen loading indicator.

```csharp
public static class IVXLoadingOverlay
{
    // Show/hide
    public static void Show();
    public static void Show(string message);
    public static void Hide();
    
    // Update message
    public static void SetMessage(string message);
    
    // Show with timeout
    public static void ShowWithTimeout(float seconds, Action onTimeout);
    
    // State
    public static bool IsShowing { get; }
}
```

**Usage:**
```csharp
// Show loading
IVXLoadingOverlay.Show("Loading game...");

// Do async work
await LoadGameDataAsync();

// Hide loading
IVXLoadingOverlay.Hide();

// With timeout
IVXLoadingOverlay.ShowWithTimeout(30f, () =>
{
    IVXPopupManager.Show("Error", "Loading timed out");
});
```

---

## IVXScreenManager

Scene/screen navigation system.

```csharp
public static class IVXScreenManager
{
    // Events
    public static event Action<string, string> OnScreenChanged;
    
    // Navigation
    public static void GoTo(string screenId);
    public static void GoTo(string screenId, object data);
    public static void GoBack();
    
    // State
    public static string CurrentScreen { get; }
    public static bool CanGoBack { get; }
    
    // Registration
    public static void RegisterScreen(string screenId, GameObject screenPrefab);
}
```

**Usage:**
```csharp
// Navigate to screen
IVXScreenManager.GoTo("MainMenu");

// Navigate with data
IVXScreenManager.GoTo("LevelSelect", new { chapter = 2 });

// Go back
IVXScreenManager.GoBack();
```

---

## IVXAnimatedButton

Enhanced button with animations and audio.

```csharp
public class IVXAnimatedButton : Button
{
    [Header("Animation")]
    [SerializeField] private AnimationType animationType;
    [SerializeField] private float animationDuration = 0.1f;
    [SerializeField] private float scaleAmount = 1.1f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    
    [Header("Haptics")]
    [SerializeField] private bool enableHaptics = true;
}

public enum AnimationType
{
    None,
    Scale,
    Punch,
    Bounce,
    Shake
}
```

---

## Pre-built Panels

### Authentication Panels

| Panel | Purpose |
|-------|---------|
| `IVXLoginPanel` | Email/password login |
| `IVXRegisterPanel` | User registration |
| `IVXForgotPasswordPanel` | Password recovery |
| `IVXSocialLoginPanel` | Social login buttons |

### Game Panels

| Panel | Purpose |
|-------|---------|
| `IVXPauseMenuPanel` | Pause menu overlay |
| `IVXSettingsPanel` | Game settings |
| `IVXProfilePanel` | User profile display |
| `IVXWalletPanel` | Virtual currency display |
| `IVXStorePanel` | In-app purchase store |
| `IVXLeaderboardPanel` | Leaderboard display |
| `IVXFriendsPanel` | Friends list |
| `IVXAchievementsPanel` | Achievement showcase |

### Common Panels

| Panel | Purpose |
|-------|---------|
| `IVXConfirmationPopup` | Yes/no confirmation |
| `IVXInputPopup` | Text input dialog |
| `IVXRateAppPopup` | App rating prompt |
| `IVXUpdateRequiredPopup` | Force update prompt |

---

## Styling

### IVXTheme

Centralized theming system.

```csharp
[CreateAssetMenu(fileName = "UITheme", menuName = "IntelliVerse-X/UI Theme")]
public class IVXTheme : ScriptableObject
{
    [Header("Colors")]
    public Color primaryColor;
    public Color secondaryColor;
    public Color accentColor;
    public Color backgroundColor;
    public Color textColor;
    public Color errorColor;
    public Color successColor;
    
    [Header("Typography")]
    public TMP_FontAsset primaryFont;
    public TMP_FontAsset headerFont;
    public float baseFontSize;
    
    [Header("Buttons")]
    public Sprite buttonNormal;
    public Sprite buttonHighlighted;
    public Sprite buttonPressed;
    public Sprite buttonDisabled;
    
    [Header("Sounds")]
    public AudioClip buttonClick;
    public AudioClip popupOpen;
    public AudioClip popupClose;
}
```

**Apply Theme:**
```csharp
// Set global theme
IVXUIManager.SetTheme(myTheme);

// Or apply to specific component
[RequireComponent(typeof(Button))]
public class ThemedButton : MonoBehaviour
{
    void Start()
    {
        var theme = IVXUIManager.CurrentTheme;
        GetComponent<Image>().color = theme.primaryColor;
    }
}
```

---

## Safe Area Handling

For notched devices:

```csharp
public class IVXSafeAreaPanel : MonoBehaviour
{
    private RectTransform _rectTransform;
    
    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }
    
    void ApplySafeArea()
    {
        var safeArea = Screen.safeArea;
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
    }
}
```

---

## Responsive Layout

```csharp
public class IVXResponsiveLayout : MonoBehaviour
{
    [SerializeField] private float mobileBreakpoint = 800f;
    [SerializeField] private GameObject desktopLayout;
    [SerializeField] private GameObject mobileLayout;
    
    void Start()
    {
        UpdateLayout();
    }
    
    void UpdateLayout()
    {
        bool isMobile = Screen.width < mobileBreakpoint;
        
        desktopLayout?.SetActive(!isMobile);
        mobileLayout?.SetActive(isMobile);
    }
}
```

---

## Animation Utilities

### IVXUIAnimator

```csharp
public static class IVXUIAnimator
{
    // Fade
    public static async Task FadeIn(CanvasGroup group, float duration = 0.3f);
    public static async Task FadeOut(CanvasGroup group, float duration = 0.3f);
    
    // Scale
    public static async Task ScaleIn(Transform target, float duration = 0.3f);
    public static async Task ScaleOut(Transform target, float duration = 0.3f);
    
    // Slide
    public static async Task SlideIn(RectTransform target, SlideDirection direction);
    public static async Task SlideOut(RectTransform target, SlideDirection direction);
    
    // Punch
    public static async Task Punch(Transform target, float scale = 1.2f);
}
```

**Usage:**
```csharp
// Animate panel in
await IVXUIAnimator.FadeIn(panelCanvasGroup);

// Bounce effect
await IVXUIAnimator.Punch(button.transform);

// Slide out
await IVXUIAnimator.SlideOut(panel, SlideDirection.Left);
```

---

## Best Practices

### 1. Use Canvas Groups

```csharp
// Always have CanvasGroup for panels
// Allows easy fade/interactivity control
[RequireComponent(typeof(CanvasGroup))]
public class MyPanel : MonoBehaviour { }
```

### 2. Pool UI Elements

```csharp
// For lists with many items
public class LeaderboardUI : MonoBehaviour
{
    private ObjectPool<LeaderboardEntry> _entryPool;
    
    void Awake()
    {
        _entryPool = new ObjectPool<LeaderboardEntry>(entryPrefab, 20);
    }
    
    void PopulateList(IVXLeaderboardEntry[] entries)
    {
        _entryPool.ReturnAll();
        
        foreach (var entry in entries)
        {
            var ui = _entryPool.Get();
            ui.SetData(entry);
        }
    }
}
```

### 3. Disable Raycasts When Hidden

```csharp
public class Panel : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    
    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    
    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
}
```

---

## Related Documentation

- [UI Demo](../samples/ui-demo.md) - Sample UI implementation
- [Theming Guide](../guides/theming.md) - Custom themes

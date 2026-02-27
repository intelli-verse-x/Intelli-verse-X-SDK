# UI API Reference

Complete API reference for the UI module.

---

## IVXUIManager

Central UI management and navigation.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXUIManager` | Singleton instance |
| `CurrentPanel` | `IVXPanel` | Active panel |
| `IsTransitioning` | `bool` | Panel transition in progress |

---

### Panel Methods

#### ShowPanel

```csharp
public void ShowPanel(string panelId)
```

Shows a panel by ID.

**Example:**
```csharp
IVXUIManager.Instance.ShowPanel("MainMenu");
```

---

#### ShowPanel<T>

```csharp
public T ShowPanel<T>() where T : IVXPanel
```

Shows a panel by type.

---

#### HidePanel

```csharp
public void HidePanel(string panelId)
```

Hides a specific panel.

---

#### HideAllPanels

```csharp
public void HideAllPanels()
```

Hides all open panels.

---

#### GoBack

```csharp
public bool GoBack()
```

Navigates to previous panel.

**Returns:** `true` if navigation occurred

---

### Popup Methods

#### ShowPopup

```csharp
public void ShowPopup(string title, string message, Action onConfirm = null)
```

Shows a simple popup.

---

#### ShowConfirmPopup

```csharp
public async Task<bool> ShowConfirmPopup(string title, string message)
```

Shows a confirmation popup.

**Returns:** `true` if confirmed

**Example:**
```csharp
bool confirmed = await IVXUIManager.Instance.ShowConfirmPopup(
    "Delete Save?",
    "This cannot be undone."
);
if (confirmed)
{
    DeleteSave();
}
```

---

#### ShowInputPopup

```csharp
public async Task<string> ShowInputPopup(
    string title,
    string placeholder = "",
    string defaultValue = "")
```

Shows an input popup.

**Returns:** User input or `null` if cancelled

---

### Loading Methods

#### ShowLoading

```csharp
public void ShowLoading(string message = "Loading...")
```

Shows a loading overlay.

---

#### HideLoading

```csharp
public void HideLoading()
```

Hides the loading overlay.

---

#### ShowLoadingAsync

```csharp
public async Task ShowLoadingAsync(Task task, string message = "Loading...")
```

Shows loading while a task executes.

**Example:**
```csharp
await IVXUIManager.Instance.ShowLoadingAsync(
    LoadDataAsync(),
    "Loading profile..."
);
```

---

### Toast Methods

#### ShowToast

```csharp
public void ShowToast(string message, float duration = 2f)
```

Shows a toast notification.

---

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnPanelShown` | `Action<string>` | Panel displayed |
| `OnPanelHidden` | `Action<string>` | Panel hidden |
| `OnPopupOpened` | `Action` | Popup opened |
| `OnPopupClosed` | `Action` | Popup closed |

---

## IVXPanel

Base class for UI panels.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `PanelId` | `string` | Unique panel identifier |
| `IsVisible` | `bool` | Visibility state |

### Virtual Methods

```csharp
protected virtual void OnShow() { }
protected virtual void OnHide() { }
protected virtual void OnBackPressed() { }
```

### Example

```csharp
public class MainMenuPanel : IVXPanel
{
    protected override void OnShow()
    {
        // Refresh data when panel opens
        RefreshUI();
    }
    
    protected override void OnBackPressed()
    {
        // Show exit confirmation
        ShowExitDialog();
    }
}
```

---

## IVXButton

Enhanced button with loading and cooldown states.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsLoading` | `bool` | Loading state |
| `CooldownRemaining` | `float` | Cooldown time left |

### Methods

#### SetLoading

```csharp
public void SetLoading(bool isLoading)
```

Sets loading state with spinner.

---

#### StartCooldown

```csharp
public void StartCooldown(float duration)
```

Starts a cooldown period.

---

## IVXProgressBar

Progress bar with animations.

### Methods

#### SetProgress

```csharp
public void SetProgress(float value, bool animated = true)
```

Sets progress (0-1).

---

#### SetProgressImmediate

```csharp
public void SetProgressImmediate(float value)
```

Sets progress without animation.

---

## Common UI Patterns

### Loading Pattern

```csharp
public async void LoadData()
{
    IVXUIManager.Instance.ShowLoading("Loading...");
    try
    {
        await LoadDataAsync();
        RefreshUI();
    }
    catch (Exception ex)
    {
        IVXUIManager.Instance.ShowToast("Failed to load data");
    }
    finally
    {
        IVXUIManager.Instance.HideLoading();
    }
}
```

### Confirmation Pattern

```csharp
public async void OnDeleteClick()
{
    bool confirmed = await IVXUIManager.Instance.ShowConfirmPopup(
        "Confirm",
        "Delete this item?"
    );
    
    if (confirmed)
    {
        await DeleteItemAsync();
        IVXUIManager.Instance.ShowToast("Item deleted");
    }
}
```

---

## See Also

- [UI Module Guide](../modules/ui.md)

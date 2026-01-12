# ⛔ Anti-Patterns - What NOT to Do

> **Purpose:** Prevent common mistakes. AI should check this before generating code.
> **Last Updated:** 2026-01-13

---

## 🔴 Critical Anti-Patterns (NEVER DO)

### 1. Find/GetComponent in Update
```csharp
// ❌ WRONG - Performance killer
void Update()
{
    var manager = FindObjectOfType<IVXIdentityManager>();
    var component = GetComponent<Rigidbody>();
}

// ✅ CORRECT - Cache in Awake/Start
private IVXIdentityManager _identityManager;
private Rigidbody _rb;

void Awake()
{
    _identityManager = IVXIdentityManager.Instance;
    _rb = GetComponent<Rigidbody>();
}

void Update()
{
    // Use cached references
}
```

### 2. String Concatenation in Hot Paths
```csharp
// ❌ WRONG - GC allocation every frame
void Update()
{
    _statusText.text = "Status: " + status.ToString();
}

// ✅ CORRECT - Use StringBuilder or SetText
private StringBuilder _sb = new StringBuilder();

void UpdateStatus(string status)
{
    _sb.Clear();
    _sb.Append("Status: ").Append(status);
    _statusText.SetText(_sb);
}
```

### 3. Null Reference Without Check
```csharp
// ❌ WRONG - Will crash if Instance is null
IVXIdentityManager.Instance.SignInAsync();

// ✅ CORRECT - Always null check singletons
if (IVXIdentityManager.Instance != null)
{
    await IVXIdentityManager.Instance.SignInAsync();
}

// ✅ ALSO CORRECT - Null conditional
IVXIdentityManager.Instance?.SignOut();
```

### 4. Async Without Try-Catch
```csharp
// ❌ WRONG - Unhandled exception crashes silently
async Task LoadUserAsync()
{
    var user = await FetchFromServer();
}

// ✅ CORRECT - Always wrap async
async Task LoadUserAsync()
{
    try
    {
        var user = await FetchFromServer();
    }
    catch (Exception ex)
    {
        Debug.LogError($"[{nameof(ClassName)}] Load failed: {ex.Message}");
    }
}
```

### 5. Hardcoded Strings for Keys
```csharp
// ❌ WRONG - Typos cause bugs
PlayerPrefs.SetString("userToken", token);
PlayerPrefs.GetString("userTokn"); // Typo!

// ✅ CORRECT - Use constants
private const string KEY_USER_TOKEN = "userToken";
PlayerPrefs.SetString(KEY_USER_TOKEN, token);
PlayerPrefs.GetString(KEY_USER_TOKEN);
```

### 6. Exposing Internal Types in Public API
```csharp
// ❌ WRONG - Exposes third-party types
public class IVXBackendManager
{
    public Nakama.IClient NakamaClient { get; } // VIOLATION!
}

// ✅ CORRECT - Wrap in SDK types
public class IVXBackendManager
{
    public IVXSession CurrentSession { get; }
    public async Task<IVXUser> GetUserAsync() { }
}
```

### 7. Hardcoded Secrets
```csharp
// ❌ WRONG - Security vulnerability
private const string API_KEY = "sk_live_abc123...";

// ✅ CORRECT - Use configuration
[SerializeField] private IVXConfig _config;
// API keys loaded from secure config at runtime
```

---

## 🟡 Warning Anti-Patterns (Avoid)

### 8. Public Fields Instead of Properties
```csharp
// ❌ AVOID - No encapsulation
public bool isInitialized;

// ✅ PREFER - Use properties
public bool IsInitialized { get; private set; }

// ✅ OR - SerializeField with property
[SerializeField] private bool _isInitialized;
public bool IsInitialized => _isInitialized;
```

### 9. Magic Numbers
```csharp
// ❌ AVOID - What does 5 mean?
if (retryCount > 5) return;

// ✅ PREFER - Named constant
private const int MAX_RETRIES = 5;
if (retryCount > MAX_RETRIES) return;
```

### 10. God Classes
```csharp
// ❌ AVOID - Manager doing everything
public class IVXManager
{
    public void Authenticate() { }
    public void ShowAd() { }
    public void TrackEvent() { }
    public void SaveData() { }
    public void LoadLeaderboard() { }
    // 500+ more methods...
}

// ✅ PREFER - Single responsibility
public class IVXIdentityManager { /* Auth only */ }
public class IVXAdsManager { /* Ads only */ }
public class IVXAnalyticsManager { /* Analytics only */ }
```

### 11. Deep Nesting
```csharp
// ❌ AVOID - Hard to read
if (user != null)
{
    if (user.IsAuthenticated)
    {
        if (user.HasPermission)
        {
            if (user.Session.IsValid)
            {
                // Do something
            }
        }
    }
}

// ✅ PREFER - Early returns
if (user == null) return;
if (!user.IsAuthenticated) return;
if (!user.HasPermission) return;
if (!user.Session.IsValid) return;
// Do something
```

---

## 🟠 SDK-Specific Anti-Patterns

### 12. Wrong Namespace
```csharp
// ❌ WRONG for this SDK
namespace MySDK { }
namespace IntelliVerseXSDK { }
namespace IntelliVerseX.Scripts.Manager { }

// ✅ CORRECT for this SDK
namespace IntelliVerseX.Core { }
namespace IntelliVerseX.Identity { }
namespace IntelliVerseX.Monetization { }
```

### 13. Missing IVX Prefix
```csharp
// ❌ WRONG - Missing prefix
public class IdentityManager { }
public interface IAuthProvider { }

// ✅ CORRECT - IVX prefix
public class IVXIdentityManager { }
public interface IIVXAuthProvider { }
```

### 14. Circular Dependencies
```csharp
// ❌ WRONG - Circular dependency
// In Identity module:
using IntelliVerseX.Monetization; // Identity → Monetization

// In Monetization module:
using IntelliVerseX.Identity; // Monetization → Identity (circular!)

// ✅ CORRECT - Use events or interfaces
// Identity fires event, Monetization subscribes
// Or both depend on shared Core module
```

### 15. Editor Code in Runtime
```csharp
// ❌ WRONG - Editor code in runtime assembly
public class IVXManager : MonoBehaviour
{
    void Start()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.DisplayDialog("Test", "Message", "OK");
        #endif
    }
}

// ✅ CORRECT - Keep editor code in Editor assembly
// Runtime code should work without UnityEditor namespace
```

### 16. Breaking UPM Structure
```csharp
// ❌ WRONG - Resources folder in package
Assets/_IntelliVerseXSDK/Resources/Config.asset

// ✅ CORRECT - Use Addressables or direct references
Assets/_IntelliVerseXSDK/Runtime/Config/DefaultConfig.asset
// Referenced via SerializeField, not Resources.Load
```

---

## Quick Checklist Before Committing

- [ ] No `Find()` or `GetComponent()` in Update/FixedUpdate
- [ ] All singletons null-checked
- [ ] Async methods have try-catch
- [ ] No hardcoded strings for keys
- [ ] No hardcoded secrets
- [ ] Namespace matches folder location
- [ ] IVX prefix on all public types
- [ ] No circular dependencies
- [ ] No Editor code in Runtime assemblies
- [ ] No third-party types in public API

---

*AI should check this file before generating any code.*

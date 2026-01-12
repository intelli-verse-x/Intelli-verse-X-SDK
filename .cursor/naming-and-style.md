# 📝 Naming & Style Guide — IntelliVerseX Unity SDK

> **Authority:** Hard rules for naming conventions and code style
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13

---

## 🏷️ Namespace Conventions

### Namespace Hierarchy

```
IntelliVerseX                      # Root namespace (avoid direct use)
├── IntelliVerseX.Core             # Core utilities, configuration
├── IntelliVerseX.Identity         # Authentication, user identity
├── IntelliVerseX.Backend          # Nakama integration, data
├── IntelliVerseX.Analytics        # Event tracking, telemetry
├── IntelliVerseX.Monetization     # Ads, IAP, offerwall
│   ├── IntelliVerseX.Monetization.Ads
│   └── IntelliVerseX.Monetization.IAP
├── IntelliVerseX.Localization     # Language support
├── IntelliVerseX.Social           # Referrals, sharing
├── IntelliVerseX.Leaderboard      # Rankings
├── IntelliVerseX.Quiz             # Quiz game support
├── IntelliVerseX.Storage          # Cloud save, persistence
├── IntelliVerseX.Networking       # Network utilities
└── IntelliVerseX.Editor           # Editor-only tools
```

### Namespace Rules

| Rule | Example |
|------|---------|
| Match folder structure | `Identity/` → `IntelliVerseX.Identity` |
| No abbreviations | `IntelliVerseX.Monetization` not `IntelliVerseX.Mon` |
| Editor suffix for editor code | `IntelliVerseX.Identity.Editor` |
| Internal suffix for internal APIs | `IntelliVerseX.Core.Internal` |

---

## 📁 Folder Naming

### Folder Convention

| Type | Convention | Example |
|------|------------|---------|
| Module folders | PascalCase | `Identity/`, `Backend/` |
| Sub-feature folders | PascalCase | `Ads/`, `IAP/` |
| Editor folders | `Editor` exactly | `Editor/` |
| Test folders | `Tests` exactly | `Tests/` |
| Sample folders | `Examples` or `Samples~` | `Examples/` |
| Documentation | `Documentation~` | `Documentation~/` |

### Folder Structure Example

```
Assets/_IntelliVerseXSDK/
├── Core/                    # ✅ PascalCase
├── Identity/                # ✅ PascalCase
├── Backend/                 # ✅ PascalCase
├── Monetization/            # ✅ PascalCase
│   ├── Ads/                 # ✅ PascalCase sub-folder
│   └── IAP/                 # ✅ PascalCase sub-folder
├── Editor/                  # ✅ Exact name
├── Tests/                   # ✅ Exact name
└── Documentation~/          # ✅ Exact name with ~
```

---

## 📄 File Naming

### C# File Naming

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase, match class name | `IVXIdentityManager.cs` |
| Interfaces | IPascalCase, match interface | `IIVXAuthProvider.cs` |
| Enums | PascalCase, match enum | `IVXAuthState.cs` |
| ScriptableObjects | PascalCase, match class | `IVXConfig.cs` |
| Editor scripts | PascalCase + Editor suffix | `IVXConfigEditor.cs` |
| Tests | PascalCase + Tests suffix | `IVXIdentityManagerTests.cs` |

### Assembly Definition Naming

| Type | Convention | Example |
|------|------------|---------|
| Runtime asmdef | `IntelliVerseX.{Module}` | `IntelliVerseX.Identity.asmdef` |
| Editor asmdef | `IntelliVerseX.{Module}.Editor` | `IntelliVerseX.Identity.Editor.asmdef` |
| Tests asmdef | `IntelliVerseX.{Module}.Tests` | `IntelliVerseX.Identity.Tests.asmdef` |

---

## 🏷️ Class Naming

### Naming Patterns

| Type | Pattern | Example |
|------|---------|---------|
| Managers (Singleton) | `IVX{Feature}Manager` | `IVXIdentityManager` |
| Services (Stateless) | `IVX{Feature}Service` | `IVXStorageService` |
| Providers (Strategy) | `IVX{Type}Provider` | `IVXAppleAuthProvider` |
| Data/Models | `IVX{Name}Data` or `IVX{Name}` | `IVXUserData`, `IVXAuthResult` |
| Events | `IVX{Name}Event` | `IVXAuthStateChangedEvent` |
| Exceptions | `IVX{Name}Exception` | `IVXAuthenticationException` |
| ScriptableObjects | `IVX{Name}Config` | `IVXAdsConfig` |
| Editor Windows | `IVX{Name}Window` | `IVXSetupWizardWindow` |
| Editor Inspectors | `IVX{Name}Editor` | `IVXConfigEditor` |

### Prefix Convention

All SDK public types use the `IVX` prefix to:
- Avoid naming collisions with consumer code
- Clearly identify SDK types
- Enable easy discovery via autocomplete

```csharp
// ✅ CORRECT: IVX prefix
public class IVXIdentityManager { }
public interface IIVXAuthProvider { }
public struct IVXAuthResult { }

// ❌ WRONG: No prefix
public class IdentityManager { }      // Collision risk
public class IntelliVerseXManager { } // Too long, inconsistent
```

---

## 🔤 Member Naming

### Fields

| Type | Convention | Example |
|------|------------|---------|
| Private fields | `_camelCase` | `_isInitialized` |
| [SerializeField] private | `_camelCase` | `_maxRetries` |
| Public fields | Avoid, use properties | - |
| Constants | `UPPER_SNAKE_CASE` | `MAX_RETRY_COUNT` |
| Static readonly | `PascalCase` | `DefaultTimeout` |

### Properties

| Type | Convention | Example |
|------|------------|---------|
| Public properties | `PascalCase` | `IsInitialized` |
| Internal properties | `PascalCase` | `SessionToken` |
| Private properties | `PascalCase` | `CachedValue` |

### Methods

| Type | Convention | Example |
|------|------------|---------|
| Public methods | `PascalCase` | `InitializeAsync()` |
| Private methods | `PascalCase` | `ValidateConfig()` |
| Async methods | `PascalCase` + `Async` suffix | `SignInAsync()` |
| Event handlers | `On` + `PascalCase` | `OnAuthStateChanged()` |
| Factory methods | `Create` + `PascalCase` | `CreateSession()` |
| Boolean getters | `Is/Has/Can` + `PascalCase` | `IsAuthenticated()` |

### Parameters & Local Variables

| Type | Convention | Example |
|------|------------|---------|
| Parameters | `camelCase` | `userId`, `authToken` |
| Local variables | `camelCase` | `result`, `isValid` |
| Loop variables | Single letter or descriptive | `i`, `item`, `user` |

---

## 📋 Code Examples

### Complete Class Example

```csharp
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Manages user authentication and identity for the IntelliVerseX SDK.
    /// </summary>
    public class IVXIdentityManager : MonoBehaviour
    {
        #region Constants
        
        private const int MAX_RETRY_COUNT = 3;
        private const float RETRY_DELAY_SECONDS = 1.0f;
        
        #endregion
        
        #region Singleton
        
        private static IVXIdentityManager _instance;
        
        /// <summary>
        /// Gets the singleton instance of the identity manager.
        /// </summary>
        public static IVXIdentityManager Instance => _instance;
        
        #endregion
        
        #region Serialized Fields
        
        [Header("Configuration")]
        [SerializeField] private IVXIdentityConfig _config;
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs;
        
        #endregion
        
        #region Private Fields
        
        private bool _isInitialized;
        private IVXUserData _currentUser;
        private IIVXAuthProvider _activeProvider;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Invoked when the authentication state changes.
        /// </summary>
        public event Action<IVXAuthState> OnAuthStateChanged;
        
        /// <summary>
        /// Invoked when the user data is updated.
        /// </summary>
        public event Action<IVXUserData> OnUserUpdated;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets whether the manager is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Gets whether a user is currently authenticated.
        /// </summary>
        public bool IsAuthenticated => _currentUser != null;
        
        /// <summary>
        /// Gets the current user data, or null if not authenticated.
        /// </summary>
        public IVXUserData CurrentUser => _currentUser;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeSingleton();
        }
        
        private void OnDestroy()
        {
            CleanupResources();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initializes the identity manager with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        /// <returns>A task representing the initialization operation.</returns>
        public async Task InitializeAsync(IVXIdentityConfig config = null)
        {
            if (_isInitialized)
            {
                LogWarning("Already initialized");
                return;
            }
            
            _config = config ?? _config;
            await PerformInitializationAsync();
            _isInitialized = true;
        }
        
        /// <summary>
        /// Authenticates the user with the specified provider.
        /// </summary>
        /// <param name="provider">The authentication provider to use.</param>
        /// <returns>The authentication result.</returns>
        public async Task<IVXAuthResult> SignInAsync(IIVXAuthProvider provider)
        {
            ValidateInitialized();
            
            _activeProvider = provider;
            var result = await provider.AuthenticateAsync();
            
            if (result.IsSuccess)
            {
                _currentUser = result.User;
                OnAuthStateChanged?.Invoke(IVXAuthState.Authenticated);
                OnUserUpdated?.Invoke(_currentUser);
            }
            
            return result;
        }
        
        /// <summary>
        /// Signs out the current user.
        /// </summary>
        public void SignOut()
        {
            ValidateInitialized();
            
            _activeProvider?.SignOut();
            _currentUser = null;
            _activeProvider = null;
            
            OnAuthStateChanged?.Invoke(IVXAuthState.SignedOut);
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializeSingleton()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private async Task PerformInitializationAsync()
        {
            Log("Initializing identity manager...");
            
            // Initialization logic here
            await Task.Yield();
            
            Log("Identity manager initialized");
        }
        
        private void ValidateInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "IVXIdentityManager is not initialized. Call InitializeAsync first.");
            }
        }
        
        private void CleanupResources()
        {
            _activeProvider = null;
            _currentUser = null;
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        private void Log(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[IVXIdentityManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[IVXIdentityManager] {message}");
        }
        
        #endregion
    }
}
```

### Interface Example

```csharp
using System.Threading.Tasks;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Defines the contract for authentication providers.
    /// </summary>
    public interface IIVXAuthProvider
    {
        /// <summary>
        /// Gets the unique identifier for this provider.
        /// </summary>
        string ProviderId { get; }
        
        /// <summary>
        /// Gets whether this provider is available on the current platform.
        /// </summary>
        bool IsAvailable { get; }
        
        /// <summary>
        /// Authenticates the user using this provider.
        /// </summary>
        /// <returns>The authentication result.</returns>
        Task<IVXAuthResult> AuthenticateAsync();
        
        /// <summary>
        /// Signs out the user from this provider.
        /// </summary>
        void SignOut();
    }
}
```

### Data Model Example

```csharp
using System;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Represents the result of an authentication attempt.
    /// </summary>
    [Serializable]
    public struct IVXAuthResult
    {
        /// <summary>
        /// Gets whether the authentication was successful.
        /// </summary>
        public bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the authenticated user data, or null if failed.
        /// </summary>
        public IVXUserData User { get; }
        
        /// <summary>
        /// Gets the error message if authentication failed.
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Gets the error code if authentication failed.
        /// </summary>
        public IVXAuthErrorCode ErrorCode { get; }
        
        /// <summary>
        /// Creates a successful authentication result.
        /// </summary>
        public static IVXAuthResult Success(IVXUserData user)
        {
            return new IVXAuthResult(true, user, null, IVXAuthErrorCode.None);
        }
        
        /// <summary>
        /// Creates a failed authentication result.
        /// </summary>
        public static IVXAuthResult Failure(string message, IVXAuthErrorCode code)
        {
            return new IVXAuthResult(false, null, message, code);
        }
        
        private IVXAuthResult(bool isSuccess, IVXUserData user, string error, IVXAuthErrorCode code)
        {
            IsSuccess = isSuccess;
            User = user;
            ErrorMessage = error;
            ErrorCode = code;
        }
    }
}
```

---

## 🎨 Unity Menu Paths

### Menu Path Convention

```
IntelliVerseX/                           # Root menu
├── Setup Wizard                         # Main setup
├── Configuration/                       # Config submenu
│   ├── SDK Settings
│   ├── Identity Settings
│   └── Monetization Settings
├── Tools/                               # Development tools
│   ├── Validate Dependencies
│   ├── Export Package
│   └── Clear Cache
└── Help/                                # Documentation
    ├── Documentation
    ├── API Reference
    └── Support
```

### Menu Item Code

```csharp
[MenuItem("IntelliVerseX/Setup Wizard")]
public static void ShowSetupWizard() { }

[MenuItem("IntelliVerseX/Configuration/SDK Settings")]
public static void ShowSDKSettings() { }

[MenuItem("IntelliVerseX/Tools/Validate Dependencies")]
public static void ValidateDependencies() { }
```

---

## 📦 Public API Exposure

### Visibility Rules

| Visibility | Use Case |
|------------|----------|
| `public` | SDK public API, consumer-facing |
| `internal` | SDK internal, cross-module |
| `protected` | Inheritance extension points |
| `private` | Implementation details |

### API Surface Guidelines

```csharp
// ✅ CORRECT: Minimal public surface
public class IVXIdentityManager
{
    // Public: Consumer API
    public bool IsAuthenticated { get; }
    public Task<IVXAuthResult> SignInAsync(IIVXAuthProvider provider);
    
    // Internal: SDK-only
    internal void RefreshSession() { }
    
    // Private: Implementation
    private void ValidateConfig() { }
}

// ❌ WRONG: Exposing implementation details
public class IVXIdentityManager
{
    public Dictionary<string, object> _internalCache; // VIOLATION!
    public void ProcessInternalQueue() { }            // VIOLATION!
}
```

---

## ✅ Naming Checklist

Before committing code, verify:

- [ ] Namespace matches folder structure
- [ ] Class name matches file name
- [ ] IVX prefix on all public types
- [ ] Private fields use `_camelCase`
- [ ] Public members use `PascalCase`
- [ ] Async methods have `Async` suffix
- [ ] Events use `On` prefix
- [ ] Interfaces use `I` prefix (after IVX)
- [ ] Constants use `UPPER_SNAKE_CASE`
- [ ] Menu paths follow convention

---

*Consistency in naming enables discoverability and reduces cognitive load. Follow these conventions without exception.*

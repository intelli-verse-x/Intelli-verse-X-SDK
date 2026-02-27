# Contributing to IntelliVerseX SDK

Thank you for your interest in contributing to IntelliVerseX SDK! This document provides guidelines and information for contributors.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Documentation](#documentation)

---

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/version/2/1/code_of_conduct/). By participating, you are expected to uphold this code.

**In short:** Be respectful, inclusive, and constructive.

---

## Getting Started

### Types of Contributions

We welcome:

- 🐛 **Bug fixes** — Fix issues in existing code
- ✨ **Features** — Add new functionality
- 📖 **Documentation** — Improve docs, add examples
- 🧪 **Tests** — Add or improve test coverage
- 🌍 **Translations** — Add new languages

### Not Accepted

- Breaking changes without discussion
- Features outside project scope
- Changes to third-party SDKs

---

## Development Setup

### Prerequisites

- Unity 2021.3 LTS or later (2023.3+ recommended)
- Git
- IDE with C# support (VS Code, Rider, Visual Studio)

### Clone Repository

```bash
git clone https://github.com/AhamedAzmi/IntelliVerseX.git
cd IntelliVerseX
```

### Open in Unity

1. Open Unity Hub
2. Click "Add" → Select cloned folder
3. Open project with Unity 2023.3+

### First Run

1. Open `Assets/_IntelliVerseXSDK/Samples/Scenes/IVX_AuthTest.unity`
2. Run the SDK Setup Wizard: `IntelliVerse-X → Setup Wizard`
3. Verify no compilation errors

---

## Making Changes

### Branch Naming

| Type | Format | Example |
|------|--------|---------|
| Feature | `feature/short-description` | `feature/apple-signin` |
| Bug fix | `fix/issue-number-description` | `fix/123-null-reference` |
| Docs | `docs/what-changed` | `docs/api-reference` |
| Refactor | `refactor/what-changed` | `refactor/auth-module` |

### Workflow

1. **Fork** the repository
2. **Create** a feature branch from `main`
3. **Make** your changes
4. **Test** thoroughly
5. **Commit** with clear messages
6. **Push** to your fork
7. **Open** a Pull Request

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat` — New feature
- `fix` — Bug fix
- `docs` — Documentation
- `style` — Formatting (no code change)
- `refactor` — Code restructuring
- `test` — Adding tests
- `chore` — Maintenance tasks

**Examples:**
```
feat(auth): add Apple Sign-In support

Implements Apple Sign-In for iOS builds using the native
Sign In with Apple framework.

Closes #42
```

```
fix(leaderboard): prevent null reference on empty results

Check for null before iterating leaderboard entries.

Fixes #108
```

---

## Pull Request Process

### Before Submitting

- [ ] Code compiles without errors
- [ ] No new warnings introduced
- [ ] Tests pass (if applicable)
- [ ] Documentation updated (if applicable)
- [ ] Follows coding standards
- [ ] Commit messages are clear

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe how you tested the changes

## Checklist
- [ ] Code follows project style
- [ ] Self-reviewed code
- [ ] Added/updated documentation
- [ ] Added/updated tests
```

### Review Process

1. Maintainer reviews PR
2. Requested changes (if any)
3. Approval
4. Merge to `main`

---

## Coding Standards

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXAuthManager` |
| Interfaces | `IIVX` + PascalCase | `IIVXAuthProvider` |
| Private fields | `_camelCase` | `_isInitialized` |
| Public properties | PascalCase | `IsReady` |
| Constants | `UPPER_SNAKE` | `MAX_RETRY_COUNT` |
| Methods | PascalCase | `InitializeAsync()` |
| Events | `On` + PascalCase | `OnAuthChanged` |
| Async methods | + `Async` suffix | `ConnectAsync()` |

### Code Style

```csharp
using System;
using UnityEngine;

namespace IntelliVerseX.ModuleName
{
    /// <summary>
    /// Brief description of the class.
    /// </summary>
    public class IVXExampleClass : MonoBehaviour
    {
        #region Constants
        private const int MAX_ITEMS = 100;
        #endregion

        #region Serialized Fields
        [SerializeField] private float _duration = 1f;
        #endregion

        #region Private Fields
        private bool _isInitialized;
        #endregion

        #region Properties
        public bool IsInitialized => _isInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Implementation
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the component.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
        }
        #endregion

        #region Private Methods
        private void DoSomething()
        {
            // Implementation
        }
        #endregion
    }
}
```

### Do's and Don'ts

**Do:**
```csharp
// Use nullable operators
var result = manager?.GetData();

// Cache component references
private Transform _cachedTransform;
void Awake() => _cachedTransform = transform;

// Use async/await
public async Task<Data> FetchDataAsync() { }
```

**Don't:**
```csharp
// Don't use Find in Update
void Update()
{
    var obj = GameObject.Find("Player"); // ❌
}

// Don't leave TODO comments
// TODO: Fix this later ❌

// Don't ignore null checks
public void Process(Data data)
{
    data.Value = 5; // ❌ Could be null
}
```

---

## Testing

### Running Tests

```bash
# Via Unity Test Framework
# Window → General → Test Runner → Run All
```

### Writing Tests

```csharp
using NUnit.Framework;
using IntelliVerseX.Core;

[TestFixture]
public class IVXLoggerTests
{
    [Test]
    public void Log_WithMessage_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => IVXLogger.Log("Test message"));
    }

    [Test]
    public void SetLevel_ValidLevel_UpdatesLevel()
    {
        IVXLogger.SetLevel(LogLevel.Warning);
        Assert.AreEqual(LogLevel.Warning, IVXLogger.CurrentLevel);
    }
}
```

---

## Documentation

### XML Comments

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Authenticates the user with email and password.
/// </summary>
/// <param name="email">User's email address.</param>
/// <param name="password">User's password.</param>
/// <returns>Authentication result with user data.</returns>
/// <exception cref="AuthException">Thrown when authentication fails.</exception>
public async Task<AuthResult> LoginAsync(string email, string password)
```

### Updating Docs

Documentation lives in `/docs/`. To preview locally:

```bash
pip install mkdocs-material
mkdocs serve
```

---

## Questions?

- 💬 [GitHub Discussions](https://github.com/AhamedAzmi/IntelliVerseX/discussions)
- 📧 [Email](mailto:contribute@intelli-verse-x.ai)

---

Thank you for contributing! 🎉

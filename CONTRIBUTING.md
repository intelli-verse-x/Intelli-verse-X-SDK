# Contributing to IntelliVerseX SDK

Thank you for your interest in contributing to IntelliVerseX SDK! This document provides guidelines for contributors across all supported platforms.

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

- Bug fixes — Fix issues in existing code
- Features — Add new functionality
- Documentation — Improve docs, add examples
- Tests — Add or improve test coverage
- Translations — Add new languages
- **New platform SDKs** — Improve or extend platform support

### Not Accepted

- Breaking changes without discussion
- Features outside project scope
- Changes to third-party SDKs or Nakama client libraries

---

## Supported Platforms

| Platform | Location | Language |
|----------|----------|----------|
| Unity / .NET | `Assets/_IntelliVerseXSDK/` | C# |
| Unreal Engine | `SDKs/unreal/` | C++ |
| Godot Engine | `SDKs/godot/` | GDScript |
| Defold | `SDKs/defold/` | Lua |
| Cocos2d-x | `SDKs/cocos2dx/` | C++ |
| JavaScript | `SDKs/javascript/` | TypeScript |
| C / C++ | `SDKs/cpp/` | C++ |
| Java / Android | `SDKs/java/` | Java |
| Flutter / Dart | `SDKs/flutter/` | Dart |
| Web3 | `SDKs/web3/` | TypeScript |

---

## Development Setup

### Prerequisites (all platforms)

- Git
- A Nakama server instance for testing (see [docker-compose.yml](../docker-compose.yml))

### Unity SDK

- Unity 2023.3 LTS or later
- IDE: VS Code, Rider, or Visual Studio

### Unreal SDK

- Unreal Engine 5.3+
- [Nakama Unreal Plugin](https://github.com/heroiclabs/nakama-unreal)

### Godot SDK

- Godot 4.2+
- [Nakama Godot addon](https://github.com/heroiclabs/nakama-godot)

### JavaScript SDK

- Node.js 18+
- Run `npm install` in `SDKs/javascript/`

### Java SDK

- JDK 11+
- Gradle 7+
- Run `./gradlew build` in `SDKs/java/`

### C/C++ SDK

- CMake 3.14+
- C++17 compiler
- [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp)

### Flutter SDK

- Dart SDK 3.0+
- Run `dart pub get` in `SDKs/flutter/`

### Web3 SDK

- Node.js 18+
- Run `npm install` in `SDKs/web3/`

### Clone Repository

```bash
git clone https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git
cd Intelli-verse-X-Unity-SDK
```

### Unity First Run

1. Open in Unity 2023.3+
2. Run the SDK Setup Wizard: `IntelliVerse-X > Setup Wizard`
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

### Per-Platform Conventions

#### Unity (C#)

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXAuthManager` |
| Interfaces | `IIVX` + PascalCase | `IIVXAuthProvider` |
| Private fields | `_camelCase` | `_isInitialized` |
| Public properties | PascalCase | `IsReady` |
| Constants | `UPPER_SNAKE` | `MAX_RETRY_COUNT` |
| Events | `On` + PascalCase | `OnAuthChanged` |
| Async methods | + `Async` suffix | `ConnectAsync()` |

#### Unreal (C++)

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `UIVX` / `FIVX` prefix | `UIVXManager` |
| Functions | PascalCase | `AuthenticateWithDevice()` |
| Members | `bPascal` (bool) | `bIsInitialized` |
| Delegates | `FOn` prefix | `FOnIVXInitialized` |

#### Godot (GDScript)

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXConfig` |
| Functions | snake_case | `authenticate_device()` |
| Signals | snake_case | `auth_success` |
| Constants | UPPER_SNAKE | `SDK_VERSION` |

#### JavaScript / TypeScript

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXManager` |
| Methods | camelCase | `authenticateDevice()` |
| Interfaces | `IVX` + PascalCase | `IVXProfile` |
| Constants | UPPER_SNAKE | `SDK_VERSION` |

#### Java

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXManager` |
| Methods | camelCase | `authenticateDevice()` |
| Constants | UPPER_SNAKE | `SDK_VERSION` |
| Packages | `com.intelliversex.sdk.*` | `com.intelliversex.sdk.core` |

#### C/C++ Native

| Element | Convention | Example |
|---------|------------|---------|
| Namespace | `ivx` | `ivx::Manager` |
| Classes | PascalCase | `Manager`, `Config` |
| Methods | camelCase | `authDevice()` |
| Members | `_camelCase` | `_initialized` |

#### Defold (Lua)

| Element | Convention | Example |
|---------|------------|---------|
| Module | lowercase | `ivx` |
| Functions | snake_case | `authenticate_device()` |
| Constants | UPPER_SNAKE | `SDK_VERSION` |
| Private | `_` prefix | `_on_auth_success()` |

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

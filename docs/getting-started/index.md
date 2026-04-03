# Getting Started

Welcome to the IntelliVerseX SDK! This guide will help you get up and running quickly.

## Before You Begin

Before installing the SDK, ensure your project meets the requirements:

<div class="grid cards" markdown>

-   :material-unity:{ .lg .middle } __Unity Version__

    ---
    
    - **Minimum:** Unity 2023.3 LTS
    - **Recommended:** Unity 6000.x

-   :material-cog:{ .lg .middle } __API Compatibility__

    ---
    
    - .NET Standard 2.1 or higher
    - IL2CPP or Mono backend

-   :material-git:{ .lg .middle } __Git__

    ---
    
    Required for UPM installation via Git URL

</div>

[:octicons-arrow-right-24: Full Requirements](requirements.md)

---

## Installation Path

Choose your installation method:

=== "Git URL (Recommended)"

    Add to `Packages/manifest.json`:
    ```json
    {
      "dependencies": {
        "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Assets/Intelli-verse-X-SDK"
      }
    }
    ```

=== "Package Manager UI"

    1. Open **Window > Package Manager**
    2. Click **+** > **Add package from git URL...**
    3. Enter the Git URL
    4. Click **Add**

=== "Local Development"

    1. Clone the repository
    2. **Window > Package Manager**
    3. **+** > **Add package from disk...**
    4. Select `package.json`

[:octicons-arrow-right-24: Full Installation Guide](installation.md)

---

## Quick Start

After installation, follow these steps:

### Step 1: Run Project Setup

```
IntelliVerseX > Project Setup & Validation
```

Click **Apply All Required Settings** to configure your project.

### Step 2: Initialize the SDK

Add an `IVXBootstrap` component to a GameObject in your first scene and assign an **IVXBootstrapConfig** (see the [Quick Start](quickstart.md) for creating config assets and optional AI/Discord configs). With **Auto Initialize** enabled, the SDK starts on Play.

```csharp
using UnityEngine;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Core;

public class GameInit : MonoBehaviour
{
    void Start()
    {
        var bootstrap = IVXBootstrap.Instance;
        if (bootstrap == null)
        {
            IVXLogger.LogWarning("Add IVXBootstrap to the scene with a Bootstrap Config assigned.");
            return;
        }

        if (bootstrap.IsInitialized)
        {
            IVXLogger.Log("IntelliVerseX SDK Ready!");
            return;
        }

        bootstrap.OnBootstrapComplete += success =>
        {
            if (success)
                IVXLogger.Log("IntelliVerseX SDK Ready!");
        };
    }
}
```

### Step 3: (Optional) Connect to Backend

`IVXBootstrap` connects to Nakama when the Nakama package is installed and your **Bootstrap Config** has a valid server host/port/key. After bootstrap completes you can run game-specific setup (for example your own services layered on the same session):

```csharp
using UnityEngine;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Core;

public class GameInit : MonoBehaviour
{
    void Start()
    {
        var bootstrap = IVXBootstrap.Instance;
        if (bootstrap == null)
            return;

        if (bootstrap.IsInitialized)
        {
            OnBackendReady(bootstrap.IsInitialized);
            return;
        }

        bootstrap.OnBootstrapComplete += OnBackendReady;
    }

    void OnBackendReady(bool success)
    {
        if (success)
            IVXLogger.Log("Connected to backend!");
    }
}
```

[:octicons-arrow-right-24: Quick Start Guide](quickstart.md)

---

## What's Included

When you install the SDK, you get:

| Module | Description |
|--------|-------------|
| **Core** | Foundation utilities, logging, singletons |
| **Identity** | User authentication and session management |
| **Backend** | Nakama integration (leaderboards, wallets, RPC) |
| **Social** | Friends system, sharing, referrals |
| **Monetization** | IAP, ads (multiple networks), offerwalls |
| **Analytics** | Event tracking via Nakama |
| **Localization** | Multi-language support with RTL |
| **Storage** | Secure local and cloud storage |
| **Leaderboards** | Global and around-player rankings |
| **Quiz** | Complete quiz game framework |
| **UI** | Production-ready UI components |
| **Editor** | Setup wizards and validation tools |

---

## Next Steps

<div class="grid cards" markdown>

-   :material-book-open-page-variant:{ .lg .middle } __Learn the Modules__

    ---
    
    Explore each module's features and APIs.
    
    [:octicons-arrow-right-24: Module Documentation](../modules/index.md)

-   :material-test-tube:{ .lg .middle } __Try the Samples__

    ---
    
    Import and run sample scenes to see features in action.
    
    [:octicons-arrow-right-24: Sample Projects](../samples/index.md)

-   :material-wrench:{ .lg .middle } __Configure Your Game__

    ---
    
    Set up game configuration, backend, and features.
    
    [:octicons-arrow-right-24: Configuration Guide](../configuration/index.md)

</div>

# Installation

This guide covers all installation methods for the IntelliVerseX SDK.

---

## Requirements

Before installing, ensure you have:

| Requirement | Version |
|-------------|---------|
| Unity | 2023.3 LTS or newer |
| Git | Any recent version |
| .NET | Standard 2.1+ |

!!! warning "Unity 2021/2022 Not Supported"
    While the SDK may work on older Unity versions, only Unity 2023.3+ and Unity 6 are officially supported and tested.

Legacy versions (2021.3/2022.x) may work but are not officially supported.

---

## Method 1: Git URL (Recommended)

This is the easiest and recommended method for most users.

### Step 1: Open manifest.json

Navigate to your Unity project and open `Packages/manifest.json` in a text editor.

### Step 2: Add the dependency

Add the IntelliVerseX SDK to the `dependencies` section:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=SDKs/unity/sdk"
  }
}
```

### Step 3: Save and return to Unity

Unity will automatically download and import the package.

### Step 4: Verify installation

Go to **Window > Package Manager** and verify that "IntelliVerseX SDK" appears in the list.

---

## Method 2: Specific Version (Production)

For production builds, always pin to a specific version tag:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=SDKs/unity/sdk#v5.8.0"
  }
}
```

### Available Version Tags

| Version | Release Date | Notes |
|---------|--------------|-------|
| `v5.8.0` | — | Latest stable (recommended pin); matches `package.json` |
| `v5.7.0` | — | See [GitHub Releases](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/releases) for changelog |
| `v5.6.0` | — | See GitHub Releases for changelog |
| `v5.5.0` | — | See GitHub Releases for changelog |
| `v5.2.0` | 2026-04-01 | Clan system, geolocation refactor |
| `v5.1.0` | 2026-03-02 | IP geolocation |
| `v5.0.0` | 2026-02-27 | Friends system |
| `v4.0.0` | 2026-02-23 | Production ready |
| `v3.0.0` | 2026-01-20 | Platform support |

!!! tip "Check Latest Version"
    View all releases at [GitHub Releases](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/releases).

---

## Method 3: Package Manager UI

### Step 1: Open Package Manager

Go to **Window > Package Manager** in Unity.

### Step 2: Add package from Git URL

1. Click the **+** button in the top-left corner
2. Select **Add package from git URL...**
3. Enter:
   ```
   https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=SDKs/unity/sdk#v5.8.0
   ```
4. Click **Add**

### Step 3: Wait for import

Unity will download and import the package. This may take a minute.

---

## Method 4: Local Development

For contributing to the SDK or local modifications:

### Step 1: Clone the repository

```bash
git clone https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git
```

### Step 2: Add package from disk

1. Open Unity and go to **Window > Package Manager**
2. Click **+** > **Add package from disk...**
3. Navigate to the cloned repository
4. Select `Assets/Intelli-verse-X-SDK/package.json`
5. Click **Open**

---

## Post-Installation Setup

After installing the SDK, complete these steps:

### Step 1: Run Project Setup

1. Go to **IntelliVerseX > Project Setup & Validation**
2. Click **Run Validation** to check your project
3. Click **Apply All Required Settings** to fix any issues

This configures:

- Required Tags and Layers
- Scripting Define Symbols
- Assembly Definition references

### Step 2: Install Dependencies

Some features require external dependencies:

| Dependency | Required For | Installation |
|------------|--------------|--------------|
| Newtonsoft.Json | JSON serialization | Auto-installed via UPM |
| Nakama Unity SDK | Backend features | Manual install |
| DOTween | UI animations | Optional (recommended for UI animations); manual install (e.g. Asset Store) |
| Photon PUN2 | Multiplayer | Optional, via Asset Store |

!!! note "Nakama SDK"
    If using backend features, install the Nakama Unity SDK from the [Heroic Labs GitHub](https://github.com/heroiclabs/nakama-unity).

### Step 3: Configure Your Game (Bootstrap)

1. Create a bootstrap asset: **Assets > Create > IntelliVerseX > Bootstrap Config**
2. Assign it where your game bootstrap / initializer expects it (see [Quick Start](quickstart.md) and [Configuration](../configuration/index.md))
3. Configure feature flags and identifiers as needed for your build

---

## Package Contents

When installed via UPM, Unity resolves the package as **com.intelliversex.sdk** (listed in **Window > Package Manager**). The folder layout matches the repository path `Assets/Intelli-verse-X-SDK` (the `package.json` root):

```
com.intelliversex.sdk/          # Package id; content from Assets/Intelli-verse-X-SDK
├── Analytics/                  # Event tracking
├── Auth/                       # Auth UI & flows
├── Backend/                    # Nakama integration
├── Bootstrap/                  # Bootstrap config & editor tooling
├── Core/                       # Foundation & utilities
├── Editor/                     # Editor tools
├── Examples/                   # Code examples
├── IAP/                        # In-app purchase helpers
├── Icons/
├── Identity/                   # Authentication & session
├── IntroScene/
├── Leaderboard/                # Rankings
├── Localization/               # Multi-language
├── Monetization/               # IAP & Ads
│   └── Ads/                    # Ad network wrappers
├── MoreOfUs/                   # Cross-promotion
├── Networking/                 # Network layer
├── Plugins/
├── Prefabs/
├── Quiz/                       # Quiz logic
│   ├── DailyQuiz/
│   └── WeeklyQuiz/
├── QuizUI/                     # Quiz UI components
├── Samples~/                   # Importable samples (Package Manager)
├── Social/                     # Friends, clans & sharing
│   └── Friends/
├── Storage/                    # Data persistence
├── Tests~/                     # Tests (repository / advanced setups)
├── Tools/
├── UI/                         # UI utilities
├── V2/                         # Next-gen features
├── Documentation~/             # In-package docs
├── package.json
├── CHANGELOG.md
└── README.md
```

---

## Updating the SDK

### Via Git URL

Simply change the version tag in `manifest.json`:

```json
"com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=SDKs/unity/sdk#v5.8.0"
```

Use the tag you want (for example `#v5.8.0` for the current recommended release).

### Via Package Manager

1. Open **Window > Package Manager**
2. Select **IntelliVerseX SDK**
3. Click **Update** (if available)

---

## Troubleshooting Installation

### Package not found

**Symptom:** Unity can't find the package.

**Solution:**
1. Ensure Git is installed and in your PATH
2. Check your internet connection
3. Verify the Git URL is correct (including `?path=SDKs/unity/sdk`)

### Compilation errors after install

**Symptom:** Errors about missing types or assemblies.

**Solution:**
1. Delete `Library/` folder and let Unity reimport
2. Run **IntelliVerseX > Project Setup & Validation**
3. Check for missing dependencies

### Assembly definition errors

**Symptom:** Errors about assembly definition files.

**Solution:**
1. Go to **Edit > Project Settings > Player**
2. Ensure **Allow 'unsafe' Code** is enabled
3. Reimport the package

---

## Next Steps

After installation, proceed to:

- [Quick Start Guide](quickstart.md) - Get running quickly
- [Project Setup](project-setup.md) - Configure your project
- [Configuration Guide](../configuration/index.md) - Set up your game

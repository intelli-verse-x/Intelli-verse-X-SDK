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
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
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
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v5.0.0"
  }
}
```

### Available Version Tags

| Version | Release Date | Notes |
|---------|--------------|-------|
| `v5.1.0` | 2026-03 | Latest stable (IP Geolocation) |
| `v5.0.0` | 2026-02 | Friends system |
| `v4.0.0` | 2026-02 | Production ready |
| `v3.0.0` | 2026-01 | Platform support |

!!! tip "Check Latest Version"
    View all releases at [GitHub Releases](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/releases).

---

## Method 3: Package Manager UI

### Step 1: Open Package Manager

Go to **Window > Package Manager** in Unity.

### Step 2: Add package from Git URL

1. Click the **+** button in the top-left corner
2. Select **Add package from git URL...**
3. Enter:
   ```
   https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK
   ```
4. Click **Add**

### Step 3: Wait for import

Unity will download and import the package. This may take a minute.

---

## Method 4: Local Development

For contributing to the SDK or local modifications:

### Step 1: Clone the repository

```bash
git clone https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git
```

### Step 2: Add package from disk

1. Open Unity and go to **Window > Package Manager**
2. Click **+** > **Add package from disk...**
3. Navigate to the cloned repository
4. Select `Assets/_IntelliVerseXSDK/package.json`
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
| DOTween | Animations | Optional, manual install |
| Photon PUN2 | Multiplayer | Optional, via Asset Store |

!!! note "Nakama SDK"
    If using backend features, install the Nakama Unity SDK from the [Heroic Labs GitHub](https://github.com/heroiclabs/nakama-unity).

### Step 3: Configure Your Game

1. Create a game configuration: **Assets > Create > IntelliVerse-X > Game Configuration**
2. Set your **Game ID** (get from IntelliVerse-X admin panel)
3. Configure feature flags as needed

---

## Package Contents

When installed, the SDK includes:

```
Packages/
└── com.intelliversex.sdk/
    ├── Core/                 # Foundation & utilities
    ├── Identity/             # Authentication
    ├── Backend/              # Nakama integration
    ├── Monetization/         # IAP & Ads
    │   └── Ads/              # Ad network wrappers
    ├── Analytics/            # Event tracking
    ├── Localization/         # Multi-language
    ├── Storage/              # Data persistence
    ├── Networking/           # Network layer
    ├── Leaderboard/          # Rankings
    ├── Social/               # Friends & sharing
    │   └── Friends/          # Friends system
    ├── Quiz/                 # Quiz logic
    │   ├── DailyQuiz/        # Daily quiz feature
    │   └── WeeklyQuiz/       # Weekly quiz feature
    ├── QuizUI/               # Quiz UI components
    ├── UI/                   # UI utilities
    ├── V2/                   # Next-gen features
    ├── MoreOfUs/             # Cross-promotion
    ├── Editor/               # Editor tools
    ├── Examples/             # Code examples
    ├── Prefabs/              # Ready-to-use prefabs
    ├── Samples~/             # Importable samples
    ├── Documentation~/       # In-package docs
    └── Icons/                # Package icons
```

---

## Updating the SDK

### Via Git URL

Simply change the version tag in `manifest.json`:

```json
"com.intelliversex.sdk": "...#v5.0.0"
```

Change to:

```json
"com.intelliversex.sdk": "...#v5.1.0"
```

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
3. Verify the Git URL is correct

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

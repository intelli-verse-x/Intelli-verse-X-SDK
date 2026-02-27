# Project Setup

After installing the SDK, configure your Unity project using the built-in setup tools.

---

## Project Setup Wizard

The SDK includes a comprehensive setup wizard to validate and configure your project.

### Opening the Wizard

```
Menu: IntelliVerseX > Project Setup & Validation
```

Or use the keyboard shortcut: `Ctrl+Shift+I` (Windows) / `Cmd+Shift+I` (macOS)

---

## Validation Steps

The wizard checks and configures:

### 1. Tags & Layers

Ensures required tags and layers exist:

| Tag | Purpose |
|-----|---------|
| `Player` | Player identification |
| `Enemy` | Enemy identification |
| `Pickup` | Collectible items |
| `UI` | UI elements |

| Layer | Purpose |
|-------|---------|
| `UI` | UI rendering |
| `Ground` | Physics raycasts |
| `Players` | Player collision |
| `Enemies` | Enemy collision |

### 2. Scripting Define Symbols

Adds required symbols:

```
INTELLIVERSEX_SDK
```

Optional symbols (added when features enabled):

```
IVX_NAKAMA
IVX_PHOTON
IVX_ADS_LEVELPLAY
IVX_ADS_APPODEAL
IVX_ADS_ADMOB
```

### 3. Project Settings

Validates:

- **Api Compatibility Level:** .NET Standard 2.1
- **Allow Unsafe Code:** Enabled (if required)
- **Scripting Backend:** IL2CPP recommended

---

## Setup Wizard Features

### Tabs Overview

The SDK Setup Wizard has multiple tabs:

| Tab | Purpose |
|-----|---------|
| **Overview** | Quick status and actions |
| **Dependencies** | External dependency status |
| **Modules** | Per-module configuration |
| **Features** | Feature toggles |
| **Samples** | Import sample scenes |
| **Version** | SDK version info |

### Module Status

Each module shows:

- :material-check-circle:{ .success } **Complete** - All requirements met
- :material-alert:{ .warning } **Partial** - Some setup needed
- :material-close-circle:{ .error } **Not Ready** - Setup required

---

## Manual Configuration

If you prefer manual setup:

### Add Tags

1. **Edit > Project Settings > Tags and Layers**
2. Add required tags under "Tags"

### Add Layers

1. **Edit > Project Settings > Tags and Layers**
2. Add required layers (use layers 8-31)

### Add Define Symbols

1. **Edit > Project Settings > Player**
2. Go to **Other Settings > Scripting Define Symbols**
3. Add: `INTELLIVERSEX_SDK`

---

## Creating Game Configuration

Create a configuration asset for your game:

### Step 1: Create Asset

```
Assets > Create > IntelliVerse-X > Game Configuration
```

### Step 2: Configure Settings

| Setting | Description | Example |
|---------|-------------|---------|
| **Game ID** | Unique identifier (UUID) | `126bf539-dae2-4bcf-...` |
| **Game Name** | Human-readable name | `My Game` |
| **Use Shared Backend** | Use IntelliVerseX backend | `true` |

### Step 3: Feature Flags

Enable/disable features:

| Flag | Default | Description |
|------|---------|-------------|
| `enableGuestAccounts` | `true` | Allow guest login |
| `enableAutoLogin` | `true` | Auto-login returning users |
| `enableLeaderboards` | `true` | Enable leaderboard features |
| `enableWallets` | `true` | Enable wallet system |
| `enableAds` | `true` | Enable ad monetization |
| `enableIAP` | `true` | Enable in-app purchases |
| `enableMultiplayer` | `false` | Enable Photon multiplayer |
| `enableDebugLogs` | `false` | Verbose logging |

### Step 4: Place in Resources

Move your config to:

```
Resources/IntelliVerseX/GameConfig.asset
```

Or a custom path (configure in your NakamaManager).

---

## Importing Samples

The SDK includes importable sample scenes:

### Via Package Manager

1. **Window > Package Manager**
2. Select **IntelliVerseX SDK**
3. Expand **Samples**
4. Click **Import** next to desired sample

### Available Samples

| Sample | Description |
|--------|-------------|
| Authentication | Login/register flow demo |
| Leaderboard | Rankings display demo |
| Friends | Friends system demo |
| Ads | Ad integration demo |
| Quiz | Quiz game demo |
| Wallet | Currency system demo |
| Localization | Multi-language demo |

### Sample Scene Locations

After import, samples are in:

```
Assets/Samples/IntelliVerseX SDK/<version>/
└── TestScenes/
    ├── IVX_AuthTest.unity
    ├── IVX_LeaderboardTest.unity
    ├── IVX_Friends.unity
    └── ...
```

---

## Verification Checklist

After setup, verify:

- [x] Project Setup wizard shows all green checkmarks
- [x] Game Configuration asset created and configured
- [x] Config asset placed in Resources folder
- [x] Required dependencies installed (Nakama, etc.)
- [x] Test scene runs without errors

### Quick Test

1. Open a sample scene (e.g., `IVX_AuthTest`)
2. Press Play
3. Verify SDK initializes (check Console)

---

## Common Issues

### "Config not found" Error

**Cause:** Game Configuration not in Resources folder.

**Fix:**
1. Move config to `Resources/IntelliVerseX/GameConfig.asset`
2. Or update the config path in your NakamaManager

### "Assembly definition not found"

**Cause:** SDK assemblies not properly imported.

**Fix:**
1. Delete `Library/` folder
2. Reopen Unity project
3. Let Unity reimport all assets

### Tags/Layers missing

**Cause:** Project Setup not run.

**Fix:**
1. Open **IntelliVerseX > Project Setup & Validation**
2. Click **Apply All Required Settings**

---

## Next Steps

- [Configuration Guide](../configuration/index.md) - Detailed configuration options
- [Module Documentation](../modules/index.md) - Learn about each module
- [Sample Projects](../samples/index.md) - Run sample scenes

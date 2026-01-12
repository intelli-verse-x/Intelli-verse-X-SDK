# IntelliVerseX SDK Setup Wizard

## 📋 Overview

The SDK Setup Wizard provides a comprehensive UI for setting up all IntelliVerseX SDK modules in your Unity project. It handles prefab creation, scene setup, and configuration validation.

## 🚀 Quick Start

### Opening the Wizard

**Menu:** `IntelliVerseX → SDK Setup Wizard`

### One-Click Setup

1. Open your target scene
2. Open the SDK Setup Wizard
3. Click "Setup All Modules" in the Quick Setup tab
4. Done!

## 📁 File Structure

```
Assets/_IntelliVerseXSDK/Editor/
├── IVXSDKSetupWizard.cs     # Main wizard window
├── IVXModuleRegistry.cs      # Module definitions & dependencies
├── IVXPrefabBuilder.cs       # Prefab creation utilities
├── IVXSceneSetup.cs          # Scene setup utilities
└── README_WIZARD.md          # This file
```

## 🎯 Features

### 1. Quick Setup Tab
- **One-Click Full Setup**: Sets up all modules automatically
- **Individual Quick Actions**: Setup specific modules

### 2. Core Modules Tab
- Intro Scene setup
- User Session Manager
- Login/Signup configuration
- Wallet Manager
- Leaderboard Manager

### 3. Game Features Tab
- Daily Quiz
- Weekly Quiz Modes (Prediction, Emoji, Fortune)
- Guest to User Conversion
- Social Sharing
- Localization
- Retention Features

### 4. Monetization Tab
- Ads System (LevelPlay/Appodeal)
- In-App Purchases

### 5. Settings Tab
- SDK Configuration
- API Configuration
- Build Settings

## 🎮 Menu Items

### Quick Setup
- `IntelliVerseX → Quick Setup → Setup Intro Scene`
- `IntelliVerseX → Quick Setup → Setup All Managers`

### Prefabs
- `IntelliVerseX → Prefabs → Create All Manager Prefabs`
- `IntelliVerseX → Prefabs → Create NakamaManager Prefab`
- `IntelliVerseX → Prefabs → Create UserData Prefab`

### Scene Setup
- `IntelliVerseX → Scene Setup → Setup Current Scene (Full)`
- `IntelliVerseX → Scene Setup → Setup Current Scene (Minimal)`
- `IntelliVerseX → Scene Setup → Create Intro Scene`
- `IntelliVerseX → Scene Setup → Add All Managers to Scene`
- `IntelliVerseX → Scene Setup → Verify Scene Setup`

## 📦 Created Prefabs

The wizard creates the following prefabs in `Assets/_IntelliVerseXSDK/Prefabs/Managers/`:

| Prefab | Components | Description |
|--------|------------|-------------|
| `NakamaManager` | IVXNManager, GeoLocationService | Backend manager |
| `UserData` | IVXNUserRuntime | User data monitor |
| `SDKManager` | IntelliVerseXManager | SDK coordinator |
| `BackendService` | IVXBackendService | Nakama client |

## 🔧 Scene Setup Options

### Minimal Setup
- NakamaManager
- UserData

### Core Setup
- NakamaManager
- UserData
- BackendService

### Full Setup
- All Core components
- WalletManager
- LeaderboardManager
- AnalyticsManager
- LoadingOverlay

## 📝 Module Checklist

Each module shows a checklist of setup steps:
- ✅ Complete
- 🔶 Partial
- ⬜ Not started

## 🔍 Verification

The wizard includes verification tools:
- **Refresh Status**: Re-checks all module status
- **Verify Setup**: Validates full SDK configuration

## ⚠️ Important Notes

1. **Don't modify `_Shubhanshu` folder** - It's the reference implementation
2. **Always verify after setup** - Use the "Verify Scene Setup" option
3. **Add scenes to Build Settings** - Use Settings tab helper

## 🔗 Dependencies

The wizard respects module dependencies:
```
Core → Identity → Backend
     ↳ Networking
     ↳ Storage
```

## 💡 Tips

1. **Start with Quick Setup** for most projects
2. **Use individual module setup** for selective configuration
3. **Check the Unity Console** for setup logs
4. **Create prefabs first** before adding to scenes

## 📞 Support

- Documentation: `Assets/_IntelliVerseXSDK/Documentation/`
- Integration Guide: `Assets/_IntelliVerseXSDK/INTEGRATION_GUIDE.md`

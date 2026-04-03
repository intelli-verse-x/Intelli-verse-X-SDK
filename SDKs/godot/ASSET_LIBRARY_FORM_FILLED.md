# Godot Asset Library — Submission Form (Copy-Paste)

Use this when submitting at: **https://godotengine.org/asset-library/asset/submit**

---

## Required fields

| Field | Value to paste |
|-------|----------------|
| **Asset Name** | `IntelliVerseX SDK` |
| **Category** | Addons → Scripts (or **Networking** if you prefer) |
| **Godot version** | `4.2` (supports 4.2 up to 4.6.x) |
| **Version** | `5.1.0` |
| **Repository host** | `GitHub` |
| **Repository URL** | `https://github.com/Intelli-verse-X/Intelli-verse-X-SDK` |
| **Issues URL** | `https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/issues` |
| **Download Commit** | *(see below)* |
| **Icon URL** | *(see below)* |
| **License** | `MIT` |
| **Description** | *(see full description below)* |

---

## Download Commit

Use the **full commit hash** of the commit that contains the Godot SDK (e.g. after your PR is merged to `main`, or the tip of `add-godot-sdk`).

- To get it: open your repo on GitHub → go to the branch/commit → copy the 40-character commit hash (e.g. `1f5d42d...` → use the full hash from the commit page).
- Example format: `1f5d42d0a1b2c3d4e5f6789012345678901234ab`

---

## Icon URL

Upload a **square icon** (min 128×128 px) to your repo (e.g. `SDKs/godot/icon.png`), then use the **raw** URL:

- Example: `https://raw.githubusercontent.com/Intelli-verse-X/Intelli-verse-X-SDK/main/SDKs/godot/icon.png`
- Replace `main` with your branch name if the icon is not on `main` yet.

---

## Description (paste this in the Description field)

```
IntelliVerseX SDK provides a complete game backend integration for Godot 4.2+: authentication (device, email, Google, Apple, custom), session restore, profile management, wallet/economy, leaderboards, cloud storage, RPC calls, and real-time socket — all via Nakama.

**Requirements**
• Godot 4.2 or newer (tested up to 4.6.x).
• The official Nakama Godot addon is required for backend features: https://github.com/heroiclabs/nakama-godot — copy the folder addons/com.heroiclabs.nakama into your project's addons/ folder.

**Installation (this repo is a monorepo)**
The addon lives under SDKs/godot/addons/intelliversex/. Either:
• Copy the contents of addons/intelliversex/ from the SDKs/godot folder into your project's addons/ folder, or
• Open SDKs/godot as a Godot project to run the included example (main scene + basic_example.gd).

**Features**
• Auth: device ID, email, Google, Apple, custom; session restore.
• Profile: fetch and update display name, avatar, language.
• Wallet / economy via RPC.
• Leaderboards: submit score, list records.
• Cloud storage: read/write JSON objects.
• RPC: call server functions.
• Real-time socket for live features.

Enable the IntelliVerseX plugin in Project → Project Settings → Plugins, then initialize with an IVXConfig (host, port, server key, SSL). See the example in examples/basic_example.gd.
```

---

## Optional: Support URL

If the form has a **Support URL** or **Documentation URL**:

- `https://github.com/Intelli-verse-X/Intelli-verse-X-SDK#readme`  
  or your docs site if you have one (e.g. `https://intelli-verse-x.github.io/Intelli-verse-X-SDK/platforms/godot/`).

---

## Optional: Preview images

You can add up to 3 image or YouTube URLs (screenshots or short demo). Use direct image URLs (e.g. raw GitHub or a stable host).

---

## Checklist before submitting

- [ ] Download Commit = full hash of the release commit (e.g. merged PR to main).
- [ ] Icon uploaded to repo and Icon URL is the **raw** link (e.g. raw.githubusercontent.com/...).
- [ ] Repository URL and Issues URL match your actual repo (if the repo was renamed to Intelli-verse-X-SDK, use that).
- [ ] Description pasted as-is or adjusted for length limits (some fields have character limits).

If your repo is under **intelli-verse-x/Intelli-verse-X-SDK** (without "Unity"), use:

- **Repository URL:** `https://github.com/intelli-verse-x/Intelli-verse-X-SDK`
- **Issues URL:** `https://github.com/intelli-verse-x/Intelli-verse-X-SDK/issues`
- **Icon URL:** `https://raw.githubusercontent.com/intelli-verse-x/Intelli-verse-X-SDK/main/SDKs/godot/icon.png` (adjust branch if needed)

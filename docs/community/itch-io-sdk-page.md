# itch.io page copy — IntelliVerseX SDK (8 platforms)

Use this for your **8 drafts** on itch.io. Each section is **copy-paste** friendly. Replace `{PLATFORM}` and `{FOLDER}` using the table below.

| Draft title | `{PLATFORM}` | Repo folder |
|-------------|--------------|---------------|
| IntelliVerseX Unity SDK | Unity | `Assets/Intelli-verse-X-SDK` |
| IntelliVerseX Unreal SDK | Unreal | `SDKs/unreal` |
| IntelliVerseX Godot SDK | Godot | `SDKs/godot` |
| IntelliVerseX Defold SDK | Defold | `SDKs/defold` |
| IntelliVerseX Cocos2d-x SDK | Cocos2d-x | `SDKs/cocos2dx` |
| IntelliVerseX JavaScript SDK | JavaScript | `SDKs/javascript` |
| IntelliVerseX C++ SDK | C++ | `SDKs/cpp` |
| IntelliVerseX Java SDK | Java | `SDKs/java` |

**Version:** 5.8.0 (adjust if you bump releases.)

---

## Project title (itch)

```
IntelliVerseX {PLATFORM} SDK
```

Example: `IntelliVerseX Unity SDK`

---

## Short description (itch subtitle / one line)

```
Nakama-powered backend SDK for {PLATFORM}: auth, profile, wallet, leaderboards, storage, RPCs. MIT license.
```

---

## Tags / classification (suggested)

- **Kind:** Tool / Game asset / Dev tool — pick what fits your account (itch often uses **Tool** for libraries).
- **Genre / engine tags:** e.g. `unity`, `unreal`, `godot`, `defold`, `cocos2d`, `javascript`, `cpp`, `java`, `nakama`, `multiplayer`, `backend`, `sdk`.

---

## Long description (Markdown for itch)

Paste into the **description** field (itch supports Markdown).

```markdown
**IntelliVerseX** is a modular client SDK built on [Nakama](https://heroiclabs.com/nakama/). This package is the **{PLATFORM}** edition: same features as our other engine SDKs, with idiomatic APIs for this stack.

### Features

- Authentication (device, email, OAuth providers where supported by the platform SDK)
- Profile & metadata
- Wallet / economy (Hiro-style RPCs on the server)
- Leaderboards & cloud storage
- Custom RPCs to your Nakama Go / Lua runtime

### License

MIT — see `LICENSE` inside the archive.

### Documentation

- **README (this SDK):** included in the zip (`README.md`).
- **Repo & cross-platform overview:** [github.com/intelli-verse-x/Intelli-verse-X-SDK](https://github.com/intelli-verse-x/Intelli-verse-X-SDK)
- **Release notes:** [v5.8.0](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/releases/tag/v5.8.0)

### Server

Use your own Nakama instance or the sample Go server in the repo: [`server/`](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/tree/main/server).

### Support

Issues and feature requests: [GitHub Issues](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/issues).

---

*IntelliVerseX — Intelli-verse-X*
```

Replace `{PLATFORM}` with **Unity**, **Unreal**, **Godot**, **Defold**, **Cocos2d-x**, **JavaScript**, **C++**, or **Java** in each draft.

---

## Uploads

1. Run `.\tools\scripts\package-itch-bundles.ps1 -Version 5.8.0` from the repository root.
2. Upload the matching zip for this platform from `dist/itch/5.8.0/`.
3. Set visibility, pricing (e.g. **$0** or **Pay what you want**), and publish when ready.

---

## Screenshot checklist

- [ ] One screenshot uploaded (editor or folder structure).
- [ ] Optional: second image — architecture or feature list from docs.

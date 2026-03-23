# IntelliVerseX Unreal SDK – Release Guide

How to publish the plugin to **GitHub**, **Unreal Marketplace**, and **GameDev Market**.

---

## 1. GitHub release

### 1.1 What to ship

- The **plugin** lives under `SDKs/unreal/` in the repo (e.g. `Intelli-verse-X-Unity-SDK`).
- For a **release**, tag the repo and attach a **zip of the plugin** so users can drop it into `Plugins/` without cloning the whole repo.

### 1.2 Steps

1. **Version and changelog**
   - Bump **Version** and **VersionName** in `IntelliVerseX.uplugin` (e.g. `5.1.0`).
   - Update the repo’s main **CHANGELOG** (and any `SDKs/unreal/CHANGELOG.md` if you use it).

2. **Clean package**
   - From repo root, create a zip that contains only the plugin folder, e.g.:
     - **Contents of zip:** everything inside `SDKs/unreal/` (e.g. `Source/`, `IntelliVerseX.uplugin`, `README.md`, `RELEASE.md`, etc.).
   - Name the zip e.g. `IntelliVerseX-Unreal-v5.1.0.zip`.

3. **Tag and release**
   - Create a tag, e.g. `unreal-sdk/v5.1.0` or `v5.1.0-unreal`.
   - On GitHub: **Releases → New release** → choose that tag.
   - Title: e.g. `IntelliVerseX Unreal SDK 5.1.0`.
   - Description: short summary + link to docs + “Install: copy contents of the zip to your project’s `Plugins/IntelliVerseX/` (or `Plugins/IntelliVerseX/unreal/`) and enable the plugin.”
   - Attach `IntelliVerseX-Unreal-v5.1.0.zip`.
   - Publish.

4. **README**
   - In `SDKs/unreal/README.md`, add an **Installation** line: “Latest release: [Releases](https://github.com/YourOrg/YourRepo/releases). Download the `IntelliVerseX-Unreal-vX.Y.Z.zip` and extract into `YourProject/Plugins/IntelliVerseX/`.”

---

## 2. Unreal Marketplace

### 2.1 Requirements (Epic’s rules)

- **Epic Developer account** and acceptance of Marketplace terms.
- **Plugin** must be packaged as a **Marketplace-compliant** plugin (correct folder structure, no absolute paths, no forbidden content).
- **Documentation**: description, screenshots, support URL, support contact.
- **Nakama**: you must state that the **Nakama** plugin is a **dependency** and link to its install/source (Epic or GitHub). You cannot redistribute Nakama inside your plugin zip unless you have a license that allows it; typically you document “Install Nakama from …” and depend on it in `.uplugin`.

### 2.2 Plugin packaging

1. **Folder structure for submission**
   - One root folder, e.g. `IntelliVerseX/`, containing:
     - `IntelliVerseX.uplugin`
     - `Source/`
     - `Resources/` (if any)
     - Any **documentation** you want in the pack (e.g. `README.md`, `INSTALL.md`).
   - No `Binaries/`, `Intermediate/`, or project-specific paths.
   - No test-game content (e.g. IVX_Test) inside the marketplace zip.

2. **.uplugin**
   - Set **VersionName** (e.g. `5.1.0`).
   - Set **IsBetaVersion** to `false` for a store release.
   - Fill **MarketplaceURL** when you have the product page.
   - **DocsURL**, **SupportURL**, **CreatedBy**, **CreatedByURL** must be valid.

3. **Dependencies**
   - Keep `"Plugins": [{ "Name": "Nakama", "Enabled": true }]`.
   - In the Marketplace description, clearly say: “Requires the Nakama Unreal plugin. Install from [link] before or after installing IntelliVerseX.”

### 2.3 Submission steps

1. Go to **Unreal Engine → Marketplace → Seller Portal** (or Epic Developer Portal).
2. **Create new product** (plugin).
3. Upload the **zip** of the `IntelliVerseX/` folder (no Binaries/Intermediate).
4. Fill in:
   - **Title:** IntelliVerseX SDK
   - **Short description** and **Full description** (features, requirements, “requires Nakama”).
   - **Category:** e.g. Networking / Blueprint / Code Plugins.
   - **Support URL / Email.**
   - **Screenshots / video** (editor, blueprint or C++ usage, optional: PIE with Output Log).
   - **Pricing** (free or paid).
5. Submit for review. Epic will check packaging and content.

---

## 3. GameDev Market (and similar stores)

### 3.1 What to provide

- A **customer-facing zip** that is the same “clean” plugin as for Marketplace: `IntelliVerseX/` with `.uplugin`, `Source/`, docs, **no** Binaries/Intermediate.
- A **short PDF or README** with:
  - Installation (copy to `Plugins/IntelliVerseX/`, enable plugin, install Nakama).
  - Requirements (UE 5.3+, Nakama, C++17).
  - Quick start (Blueprint + C++ snippet).
  - Link to full docs and support.

### 3.2 Steps

1. **Zip**
   - Same as Marketplace: one folder `IntelliVerseX/`, version in filename, e.g. `IntelliVerseX-Unreal-5.1.0.zip`.

2. **Listing**
   - **Title:** IntelliVerseX SDK for Unreal Engine
   - **Description:** features, dependency on Nakama, link to install Nakama.
   - **Requirements:** Unreal Engine 5.3+, Nakama Unreal plugin.
   - **Support:** email or GitHub issues.

3. **Deliverables**
   - Main: `IntelliVerseX-Unreal-5.1.0.zip`.
   - Optional: `IntelliVerseX-Unreal-QuickStart.pdf` (install + testing steps from `TESTING_SDK.md`).

---

## 4. Pre-release checklist

| Item | GitHub | Marketplace | GameDev |
|------|--------|-------------|---------|
| Version in .uplugin | ✓ | ✓ | ✓ |
| CHANGELOG / release notes | ✓ | (in description) | (in description) |
| Zip = plugin only, no Binaries/Intermediate | ✓ | ✓ | ✓ |
| README / docs in zip | ✓ | Optional | Optional |
| Nakama documented as dependency | ✓ | ✓ | ✓ |
| Support URL / contact | ✓ | ✓ | ✓ |
| IsBetaVersion = false (for store) | - | ✓ | ✓ |

---

## 5. One-command zip (example)

From repo root (PowerShell), create a release zip of the Unreal SDK only:

```powershell
$version = "5.1.0"
$out = "IntelliVerseX-Unreal-v$version.zip"
Compress-Archive -Path "SDKs\unreal\*" -DestinationPath $out -Force
# Result: IntelliVerseX-Unreal-v5.1.0.zip with contents of SDKs/unreal/
```

Then upload that zip to GitHub Release, Unreal Marketplace, or GameDev Market as described above.

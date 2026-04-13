# Publish IntelliVerseX Unity SDK to OpenUPM

Step-by-step to publish **com.intelliversex.sdk** (Unity C# SDK only) on [OpenUPM](https://openupm.com).

---

## Prerequisites

- [ ] Package `com.intelliversex.sdk` is open-source on GitHub (this repo)
- [ ] At least one Git tag exists (e.g. `v5.8.0`) for the build
- [ ] Package is **not** on the official Unity registry (OpenUPM requirement)
- [ ] License is MIT and clearly stated

---

## 1. Register package on OpenUPM

1. Go to **https://openupm.com/packages/add**
2. Fill the form with:

   | Field | Value |
   |-------|-------|
   | Package name | `com.intelliversex.sdk` |
   | Display name | `IntelliVerseX SDK` |
   | Description | Complete modular game development SDK with 15+ packages: Core, Identity, Auth, Backend, Networking, Localization, Storage, Quiz, Monetization (IAP, Ads), Analytics, and more. |
   | Repository URL | `https://github.com/intelli-verse-x/Intelli-verse-X-SDK` |
   | License (SPDX) | `MIT` |
   | Topics | `services`, `network`, `mobile`, `integration`, `frameworks` (choose from [topics list](https://github.com/openupm/openupm/blob/master/data/topics.yml)) |
   | README path | `main:README.md` (or `master:README.md` if your default branch is master) |
   | Hunter | your GitHub username |

3. Click **Submit metadata**.
4. On the opened GitHub page, click **Commit changes...** (keep default message: `Create com.intelliversex.sdk.yml`).
5. Wait for the PR to be merged (test workflow runs first; new contributors may need moderator approval within 24h).
6. After merge, the build pipeline runs (~15–30 min). Package appears at:  
   **https://openupm.com/packages/com.intelliversex.sdk**

---

## 2. Add OpenUPM scoped registry (one-time)

Users must add the OpenUPM registry to `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.intelliversex"
      ]
    }
  ],
  "dependencies": {
    "com.intelliversex.sdk": "5.8.0"
  }
}
```

---

## 3. Install via OpenUPM CLI (alternative)

```bash
openupm add com.intelliversex.sdk
```

(Requires [OpenUPM CLI](https://openupm.com/docs/getting-started.html#installing-openupm-cli) and adds the registry automatically.)

---

## 4. Verify install flow

After the package is published:

1. Create a new Unity project (or use a test project).
2. Add the scoped registry and dependency to `Packages/manifest.json` as above.
3. Save; Unity should resolve and import the package.
4. Or run: `openupm add com.intelliversex.sdk` in the project root.
5. Confirm: **Window → Package Manager** shows IntelliVerseX SDK.

---

## 5. Add install badge to README

After registration, get the badge from your package page:

- Go to https://openupm.com/packages/com.intelliversex.sdk
- Scroll to the badge section
- Copy the Markdown code and add it to the main README

Standard format:
```markdown
[![openupm](https://img.shields.io/npm/v/com.intelliversex.sdk?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.intelliversex.sdk)
```

---

## Notes

- **Unity Asset Store:** OpenUPM disallows publishing Asset Store content without explicit open-source rights. This package is your own SDK with MIT license; ensure no third-party Asset Store assets are included in the UPM package.
- **Version tags:** OpenUPM builds from Git tags. Use semver (e.g. `v5.8.0`). Create tags via GitHub Releases or: `git tag v5.8.0 && git push origin v5.8.0`
- **Path:** If your package lives in a subfolder (e.g. `Assets/_IntelliVerseXSDK` or `Assets/Intelli-verse-X-SDK`), the add form may ask for the path. OpenUPM supports subfolder packages; specify the path where `package.json` lives.

---

## Summary checklist

| Step | Action | Status |
|------|--------|--------|
| 1 | Register on https://openupm.com/packages/add → Submit metadata → Commit PR | You do |
| 2 | Wait for PR merge + build (~15–30 min) | Automated |
| 3 | Verify: `openupm add com.intelliversex.sdk` in a Unity project | You do |
| 4 | Add badge to README (from package page) | Done in README |

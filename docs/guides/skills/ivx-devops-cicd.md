# DevOps & CI/CD

**Skill ID:** `ivx-devops-cicd`

---
name: ivx-devops-cicd
description: >-
  Set up CI/CD pipelines, build automation, and release workflows for IntelliVerseX
  SDK games across all 10 supported platforms. Use when the user says "CI/CD",
  "GitHub Actions", "build pipeline", "automated builds", "release pipeline",
  "version bumping", "asset bundles", "cloud build", "test automation",
  "code signing", "deploy", "continuous integration", or needs help with
  any build, test, or release automation.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---



## Overview

The DevOps module provides ready-to-use CI/CD pipeline templates, build automation scripts, and release workflows for all 10 supported engines. One command generates a complete GitHub Actions pipeline tailored to your engine, target platforms, and release strategy.

```
Source Code Push
      │
      ▼
CI Pipeline (GitHub Actions)
      │
      ├── Lint & Validate ──► Code quality gates
      ├── Build ──► Per-platform binaries
      ├── Test ──► Unit + integration tests
      ├── Asset Bundle ──► CDN upload
      └── Release ──► Store submission / deploy
              │
              ▼
      Release Artifacts + Changelogs
```

---

## 1. Quick Setup

### Generate Pipeline

```csharp
using IntelliVerseX.DevOps;

await IVXPipelineGenerator.Instance.GenerateAsync(new PipelineConfig
{
    Engine = GameEngine.Unity,
    Platforms = new[] { Platform.Android, Platform.iOS, Platform.WebGL },
    TestFramework = TestFramework.UnityTestFramework,
    AssetBundleCDN = "s3://my-bucket/bundles",
    ReleaseStrategy = ReleaseStrategy.SemVer,
    EnableCodeSigning = true,
});
```

Or use the CLI:

```bash
ivx-pipeline init --engine unity --platforms android,ios,webgl
```

---

## 2. GitHub Actions Workflow Templates

### Unity Pipeline

| Job | Steps | Triggers |
|-----|-------|----------|
| **Lint** | EditorConfig check, namespace validation, asmdef verification | Every push |
| **Test** | Unity Test Framework (EditMode + PlayMode) | Every push |
| **Build Android** | Unity batch build, AAB signing, Play Store upload | Tag / manual |
| **Build iOS** | Unity batch build, Xcode archive, TestFlight upload | Tag / manual |
| **Build WebGL** | Unity batch build, deploy to hosting | Tag / manual |
| **Asset Bundles** | Build bundles, upload to CDN, update manifest | Asset changes |

### Unreal Pipeline

| Job | Steps | Triggers |
|-----|-------|----------|
| **Compile** | UnrealBuildTool compile check | Every push |
| **Test** | Unreal Automation Tool tests | Every push |
| **Build** | Cook + Package per platform | Tag / manual |
| **Deploy** | Upload to Steam / Epic / console dev kits | Tag / manual |

### Godot Pipeline

| Job | Steps | Triggers |
|-----|-------|----------|
| **Lint** | GDScript linter (`gdtoolkit`) | Every push |
| **Test** | GUT (Godot Unit Testing) | Every push |
| **Export** | Godot headless export per platform | Tag / manual |
| **Deploy** | Upload to itch.io / Steam / web hosting | Tag / manual |

### JavaScript/Web Pipeline

| Job | Steps | Triggers |
|-----|-------|----------|
| **Lint** | ESLint + TypeScript check | Every push |
| **Test** | Jest / Vitest | Every push |
| **Build** | Webpack / Vite bundle | Every push |
| **Deploy** | CDN / Vercel / Netlify deploy | Tag / manual |

---

## 3. Version Management

### IVXVersionManager

Automated SemVer version bumping across all SDK files:

```csharp
await IVXVersionManager.Instance.BumpAsync(VersionBump.Minor);
```

### Files Updated

| Engine | Version Files |
|--------|--------------|
| **Unity** | `package.json`, `ProjectSettings/ProjectSettings.asset` |
| **Unreal** | `*.uplugin`, `DefaultGame.ini` |
| **Godot** | `project.godot`, `plugin.cfg` |
| **JavaScript** | `package.json` |
| **Java** | `build.gradle` |
| **Flutter** | `pubspec.yaml` |
| **C++** | `CMakeLists.txt`, version header |

### Changelog Generation

```csharp
var changelog = await IVXVersionManager.Instance.GenerateChangelogAsync(
    format: ChangelogFormat.KeepAChangelog,
    sinceTag: "v1.2.0"
);

await changelog.SaveAsync("CHANGELOG.md");
```

Parses conventional commits (`feat:`, `fix:`, `breaking:`) into categorized changelogs.

---

## 4. Asset Bundle Pipeline

### Build and Upload

```csharp
var bundleResult = await IVXAssetPipeline.Instance.BuildBundlesAsync(new BundleBuildConfig
{
    Platforms = new[] { Platform.Android, Platform.iOS },
    CompressionMode = BundleCompression.LZ4,
    OutputPath = "build/bundles",
    IncrementalBuild = true,
});

await IVXAssetPipeline.Instance.UploadToCDNAsync(new CDNUploadConfig
{
    Provider = CDNProvider.S3,
    Bucket = "my-game-assets",
    Region = "us-east-1",
    VersionTag = currentVersion,
});
```

### Manifest Management

```csharp
var manifest = await IVXAssetPipeline.Instance.GetManifestAsync();

Debug.Log($"Bundle count: {manifest.Bundles.Count}");
Debug.Log($"Total size: {manifest.TotalSizeMB:F1} MB");
```

---

## 5. Test Automation

### Test Runner Configuration

```csharp
var testConfig = new IVXTestRunnerConfig
{
    Framework = TestFramework.UnityTestFramework,
    RunEditModeTests = true,
    RunPlayModeTests = true,
    CodeCoverageMinPercent = 60,
    FailOnWarnings = false,
    OutputFormat = TestOutputFormat.JUnit,
    OutputPath = "test-results/",
};

var results = await IVXTestRunner.Instance.RunAsync(testConfig);

Debug.Log($"Pass: {results.PassCount}, Fail: {results.FailCount}");
Debug.Log($"Coverage: {results.CodeCoverage:P1}");
```

### Per-Engine Test Frameworks

| Engine | Framework | CLI Command |
|--------|----------|-------------|
| **Unity** | Unity Test Framework | `unity -batchmode -runTests` |
| **Unreal** | Automation Tool | `RunTests` commandlet |
| **Godot** | GUT | `godot --headless -s gut_cli.gd` |
| **JavaScript** | Jest / Vitest | `npm test` |
| **Java** | JUnit | `./gradlew test` |
| **Flutter** | flutter_test | `flutter test` |
| **C++** | Google Test | `ctest` |

---

## 6. Code Signing

### Signing Configuration

```csharp
var signingConfig = new IVXSigningConfig
{
    Android = new AndroidSigningConfig
    {
        KeystorePath = "secrets/release.keystore",
        KeystorePassword = Environment.GetEnvironmentVariable("KEYSTORE_PASS"),
        KeyAlias = "release",
        KeyPassword = Environment.GetEnvironmentVariable("KEY_PASS"),
    },
    iOS = new iOSSigningConfig
    {
        ProvisioningProfile = "secrets/release.mobileprovision",
        CertificatePath = "secrets/distribution.p12",
        CertificatePassword = Environment.GetEnvironmentVariable("CERT_PASS"),
        TeamId = "ABCDE12345",
    },
    macOS = new macOSSigningConfig
    {
        DeveloperIdApplication = "Developer ID Application: Your Name",
        NotarizeAppleId = Environment.GetEnvironmentVariable("APPLE_ID"),
        NotarizePassword = Environment.GetEnvironmentVariable("APPLE_PASS"),
    },
};
```

All secrets are stored as GitHub Actions secrets and injected via environment variables.

---

## 7. Release Workflows

### Release Strategy Options

| Strategy | Description | Best For |
|----------|------------|----------|
| `SemVer` | Major.Minor.Patch with conventional commits | Libraries, SDKs |
| `CalVer` | Year.Month.Build (e.g. 2026.04.1) | Live service games |
| `Sprint` | Sprint number + build (e.g. S12.45) | Agile teams |
| `BuildNumber` | Auto-incrementing build number | Continuous deployment |

### Store Submission

```csharp
await IVXReleaseManager.Instance.SubmitAsync(new SubmissionConfig
{
    Platform = Platform.Android,
    Track = ReleaseTrack.Internal,
    ArtifactPath = "build/app-release.aab",
    ReleaseNotes = changelogMarkdown,
});
```

### Supported Stores

| Store | Submission Method | Automation |
|-------|------------------|-----------|
| **Google Play** | `fastlane supply` or Play Console API | Full |
| **Apple App Store** | `fastlane deliver` or App Store Connect API | Full |
| **Steam** | `steamcmd` | Full |
| **itch.io** | `butler push` | Full |
| **Epic Games Store** | BuildPatchTool | Semi (manual review) |
| **Meta Quest Store** | CLI upload | Semi |
| **WebGL hosting** | S3/Vercel/Netlify deploy | Full |

---

## 8. Environment Management

### Staging Environments

```yaml
environments:
  development:
    nakama_url: "http://localhost:7350"
    satori_url: "http://localhost:7450"
    asset_cdn: "http://localhost:8080"

  staging:
    nakama_url: "https://staging-nakama.example.com"
    satori_url: "https://staging-satori.example.com"
    asset_cdn: "https://staging-cdn.example.com"

  production:
    nakama_url: "https://nakama.example.com"
    satori_url: "https://satori.example.com"
    asset_cdn: "https://cdn.example.com"
```

```csharp
IVXEnvironmentManager.Instance.SetEnvironment("staging");
```

---

## 9. Cross-Platform Pipeline Matrix

| Engine | Build Runner | Container | Signing |
|--------|-------------|-----------|---------|
| **Unity** | GameCI Docker | `unityci/editor` | Android keystore, iOS cert, macOS notarize |
| **Unreal** | Self-hosted runner | Custom UE image | Platform-specific |
| **Godot** | GitHub-hosted | `barichello/godot-ci` | Android keystore |
| **JavaScript** | GitHub-hosted | `node:20` | npm publish token |
| **Java** | GitHub-hosted | `gradle:jdk17` | Android keystore, Maven GPG |
| **Flutter** | GitHub-hosted | `cirrusci/flutter` | Android keystore, iOS cert |
| **C++** | GitHub-hosted | Ubuntu/Windows/macOS | CMake + platform tools |

---

## Platform-Specific DevOps

### VR Platforms

| Platform | CI/CD Notes |
|----------|------------|
| **Meta Quest** | Use Meta's `ovr-platform-util` CLI for APK upload. Requires Quest Developer Hub. |
| **SteamVR** | Standard Steam pipeline via `steamcmd`. Include VR manifest in depot config. |
| **Vision Pro** | Standard Xcode archive → App Store Connect pipeline. Requires visionOS SDK. |

### Console Platforms

| Platform | CI/CD Notes |
|----------|------------|
| **PlayStation** | Requires Sony's proprietary SDK and dev kits. CI runs on self-hosted runners with NDA tools. |
| **Xbox** | Requires GDK build tools. Partner Center submission via API or CLI. |
| **Switch** | Requires Nintendo SDK on self-hosted runners. Submission via Nintendo Developer Portal. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Deploy** | Deploy to any static hosting (S3, Vercel, Netlify, GitHub Pages). |
| **CDN** | Configure cache headers for asset bundles. Use content-hash filenames for cache busting. |
| **Compression** | Enable Brotli or gzip compression for WebGL builds. |

---

## Checklist

- [ ] GitHub Actions workflow generated for target engine and platforms
- [ ] Build pipeline tested with successful artifact generation
- [ ] Unit tests running in CI with coverage reporting
- [ ] Version bumping configured (SemVer or CalVer)
- [ ] Changelog generation from conventional commits verified
- [ ] Asset bundle pipeline building and uploading to CDN
- [ ] Code signing configured for Android (keystore) and iOS (provisioning)
- [ ] Secrets stored in GitHub Actions secrets (never in code)
- [ ] Environment configs defined for dev/staging/production
- [ ] Store submission automated for target platforms
- [ ] VR platform CLI tools configured (if targeting Quest/Steam)
- [ ] Console self-hosted runners set up (if targeting PS/Xbox/Switch)
- [ ] WebGL deployment with compression configured (if targeting browser)

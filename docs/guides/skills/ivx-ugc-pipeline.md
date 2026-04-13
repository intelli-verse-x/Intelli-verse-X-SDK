# UGC Pipeline

**Skill ID:** `ivx-ugc-pipeline`

---
name: ivx-ugc-pipeline
description: >-
  Set up user-generated content pipelines for IntelliVerseX SDK games including
  content upload, moderation, sharing, and creator tools. Use when the user says
  "user-generated content", "UGC", "level editor", "player-created content",
  "content moderation", "upload system", "content sharing", "custom levels",
  "player creations", "mod support", "skin creator", "content browser",
  or needs help with any user-generated content feature.
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

The UGC Pipeline module provides a complete system for players to create, upload, share, and discover user-generated content. It includes AI-powered moderation (via `IVXAIModerator`), versioned content storage, cross-platform sharing, creator attribution, and an in-game content browser.

```
Creator Tools (in-game editor)
      │
      ▼
IVXContentUploader
      │
      ├── Validation (size, format, rules)
      ├── AI Moderation (text, images)
      ├── Metadata Extraction
      └── Upload to Storage
              │
              ├──► Nakama Storage (metadata + small content)
              └──► S3/CDN (large assets: images, levels, audio)
                      │
                      ▼
              IVXContentBrowser (discovery)
                      │
                      ├── Search & Filter
                      ├── Ratings & Reports
                      └── Download & Play
```

---

## 1. Configuration

### IVXUGCConfig ScriptableObject

Create via **Create > IntelliVerseX > UGC Configuration**.

| Field | Description |
|-------|-------------|
| `EnableUGC` | Master toggle |
| `StorageBackend` | `Nakama`, `S3`, `Both` (Nakama for metadata, S3 for assets) |
| `S3Bucket` | S3 bucket for large content storage |
| `S3Region` | AWS region |
| `MaxContentSizeMB` | Maximum upload size (default 10 MB) |
| `MaxContentPerUser` | Maximum published items per user (default 50) |
| `RequireModeration` | All content goes through moderation before publishing |
| `AutoModerate` | Use AI moderation for automatic approval/rejection |
| `EnableRatings` | Allow players to rate content |
| `EnableReports` | Allow players to report content |

---

## 2. Content Types

### Supported UGC Types

| Type | Data Format | Storage | Example |
|------|------------|---------|---------|
| **Levels** | JSON (tile data, entity placement) | S3 + Nakama | Custom dungeons, race tracks |
| **Skins** | PNG/sprite sheet + JSON metadata | S3 + Nakama | Character skins, weapon skins |
| **Decks** | JSON (card list, rules) | Nakama | Custom card decks |
| **Quizzes** | JSON (questions, answers, media) | Nakama | Player-created trivia sets |
| **Playlists** | JSON (level IDs, order) | Nakama | Curated level collections |
| **Mods** | ZIP (scripts, assets, manifest) | S3 + Nakama | Gameplay modifications |
| **Replays** | Binary (input recording) | S3 | Match recordings |
| **Screenshots** | PNG/JPEG | S3 | In-game photography |

### Defining Content Types

```csharp
using IntelliVerseX.UGC;

IVXUGCManager.Instance.RegisterContentType(new UGCContentType
{
    TypeId = "custom_level",
    DisplayName = "Custom Level",
    MaxSizeMB = 5,
    AllowedFormats = new[] { ".json" },
    RequiredMetadata = new[] { "title", "difficulty", "theme" },
    ValidationRules = new[] { new LevelValidationRule() },
});
```

---

## 3. Uploading Content

### IVXContentUploader

```csharp
var upload = await IVXContentUploader.Instance.UploadAsync(new UGCUploadRequest
{
    ContentType = "custom_level",
    Title = "Dragon's Lair",
    Description = "A challenging dungeon with fire puzzles and a dragon boss.",
    Tags = new[] { "dungeon", "hard", "boss", "fire" },
    Thumbnail = thumbnailTexture,
    ContentData = levelJsonBytes,
    Visibility = UGCVisibility.Public,
});

if (upload.Status == UploadStatus.PendingModeration)
{
    Debug.Log("Content submitted for review.");
}
else if (upload.Status == UploadStatus.Published)
{
    Debug.Log($"Content published! ID: {upload.ContentId}");
}
```

### Upload Validation

Before upload, content is validated:

| Check | Action on Fail |
|-------|---------------|
| **File size** | Reject (over MaxSizeMB) |
| **Format** | Reject (not in AllowedFormats) |
| **Required metadata** | Reject (missing fields) |
| **Custom rules** | Reject (level not completable, invalid data) |
| **AI text moderation** | Flag for review or reject |
| **AI image moderation** | Flag for review or reject (thumbnail) |

---

## 4. AI Moderation

### Automatic Moderation

Content passes through `IVXAIModerator` before publishing:

```csharp
IVXUGCConfig.Instance.AutoModerate = true;
IVXUGCConfig.Instance.RequireModeration = true;
```

### Moderation Pipeline

```
Upload → Text Moderation → Image Moderation → Custom Rules → Decision
              │                    │                │
              ▼                    ▼                ▼
         Title, Desc,        Thumbnail,        Game-specific
         Tags, In-content    Screenshots        checks
         text
```

### Moderation Results

| Decision | Trigger | Action |
|----------|---------|--------|
| **Approved** | All checks pass | Content published immediately |
| **Flagged** | AI confidence < threshold | Queued for manual review |
| **Rejected** | Clear policy violation | Content blocked, creator notified |

### Manual Review Queue

```csharp
var queue = await IVXUGCModeration.Instance.GetReviewQueueAsync(limit: 20);

foreach (var item in queue.Items)
{
    Debug.Log($"[{item.Status}] {item.Title} by {item.CreatorName}");
    Debug.Log($"  Flags: {string.Join(", ", item.ModerationFlags)}");
}

await IVXUGCModeration.Instance.ApproveAsync(contentId);
await IVXUGCModeration.Instance.RejectAsync(contentId, reason: "Inappropriate thumbnail");
```

---

## 5. Content Browser

### IVXContentBrowser

In-game content discovery:

```csharp
var results = await IVXContentBrowser.Instance.SearchAsync(new UGCSearchRequest
{
    ContentType = "custom_level",
    Query = "dragon",
    Tags = new[] { "hard" },
    SortBy = UGCSortOrder.MostPopular,
    PageSize = 20,
    Page = 1,
});

foreach (var item in results.Items)
{
    Debug.Log($"{item.Title} by {item.CreatorName}");
    Debug.Log($"  Rating: {item.AverageRating:F1}/5 ({item.RatingCount} votes)");
    Debug.Log($"  Downloads: {item.DownloadCount}");
}
```

### Sort Options

| Sort | Description |
|------|------------|
| `Newest` | Most recently published |
| `MostPopular` | Most downloads |
| `HighestRated` | Best average rating |
| `Trending` | Fastest growing in last 7 days |
| `FriendsPlayed` | Played by friends |
| `StaffPicks` | Featured by moderators |

### Content Details

```csharp
var details = await IVXContentBrowser.Instance.GetDetailsAsync(contentId);

Debug.Log($"Title: {details.Title}");
Debug.Log($"Creator: {details.CreatorProfile.DisplayName}");
Debug.Log($"Published: {details.PublishedAt}");
Debug.Log($"Version: {details.Version}");
Debug.Log($"Size: {details.SizeMB:F1} MB");
```

---

## 6. Downloading and Playing UGC

```csharp
var content = await IVXContentBrowser.Instance.DownloadAsync(contentId);

if (content.ContentType == "custom_level")
{
    var levelData = JsonUtility.FromJson<LevelData>(content.DataAsString);
    LevelLoader.Instance.LoadCustomLevel(levelData);
}
```

### Caching

```csharp
IVXUGCCache.Instance.MaxCacheSizeMB = 100;
IVXUGCCache.Instance.CacheTTLDays = 7;

bool isCached = IVXUGCCache.Instance.IsCached(contentId);
```

---

## 7. Ratings and Reports

### Rating Content

```csharp
await IVXContentBrowser.Instance.RateAsync(contentId, stars: 4);
```

### Reporting Content

```csharp
await IVXContentBrowser.Instance.ReportAsync(contentId, new UGCReport
{
    Reason = ReportReason.InappropriateContent,
    Details = "Thumbnail contains offensive imagery.",
});
```

Reports are stored in Nakama under `ivx_ugc_reports` and trigger review queue entries.

---

## 8. Creator Profiles

### IVXCreatorProfile

Track creator statistics and reputation:

```csharp
var profile = await IVXCreatorProfile.Instance.GetAsync(creatorUserId);

Debug.Log($"Creator: {profile.DisplayName}");
Debug.Log($"Published: {profile.PublishedCount}");
Debug.Log($"Total downloads: {profile.TotalDownloads}");
Debug.Log($"Average rating: {profile.AverageRating:F1}");
Debug.Log($"Creator level: {profile.CreatorLevel}");
```

### Creator Rewards

```csharp
IVXCreatorRewards.Instance.Configure(new CreatorRewardConfig
{
    RewardPerDownload = new CurrencyReward("coins", 1),
    RewardPerFeatured = new CurrencyReward("gems", 50),
    MilestoneRewards = new Dictionary<int, CurrencyReward>
    {
        { 100, new CurrencyReward("gems", 10) },
        { 1000, new CurrencyReward("gems", 100) },
        { 10000, new CurrencyReward("gems", 500) },
    },
});
```

---

## 9. Content Versioning

### Updating Published Content

```csharp
await IVXContentUploader.Instance.UpdateAsync(new UGCUpdateRequest
{
    ContentId = existingContentId,
    ContentData = updatedLevelJsonBytes,
    ChangelogEntry = "Fixed impossible jump in room 3, added new secret area.",
});
```

Previous versions are retained for rollback and players can choose which version to play.

---

## 10. Cross-Platform API

| Engine | Class / Module | Core API |
|--------|---------------|----------|
| **Unity** | `IVXUGCManager` | Full feature set + editor tools |
| **Unreal** | `UIVXUGCSubsystem` | Upload, browse, download, ratings |
| **Godot** | `IVXUGC` autoload | Upload, browse, download |
| **JavaScript** | `IVXUGC` | Upload, browse, download (web UGC) |
| **Roblox** | `IVX.UGC` | Browse and download (Roblox has its own creation tools) |
| **Java** | `IVXUGC` | Upload, browse, download |
| **Flutter** | `IvxUGC` | Browse and download |
| **C++** | `IVXUGC` | Upload, browse, download |

---

## Platform-Specific UGC

### VR Platforms

| Feature | VR Guidance |
|---------|------------|
| **Content browser** | 3D world-space gallery. Players walk through a virtual showcase of creations. |
| **Level editor** | VR-native creation tools (grab, place, scale with controllers). |
| **Thumbnails** | Auto-capture 360-degree screenshots for VR content previews. |

### Console Platforms

| Platform | UGC Notes |
|----------|----------|
| **PlayStation** | Sony requires UGC moderation disclosure. Include in Terms of Service. |
| **Xbox** | Xbox Live requires content reporting and moderation for certification. |
| **Switch** | UGC sharing may require Nintendo approval depending on content type. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Upload** | Use `File` API for browser-based content uploads. |
| **Storage** | Content served from CDN for fast web delivery. |
| **Sharing** | Generate shareable URLs for UGC items (e.g. `game.com/play/level_id`). |

---

## Checklist

- [ ] `EnableUGC` toggled on in `IVXBootstrapConfig`
- [ ] Content types registered with size limits and validation rules
- [ ] Upload flow tested end-to-end (create → upload → moderate → publish)
- [ ] AI moderation configured for text and image content
- [ ] Content browser UI implemented with search, sort, and pagination
- [ ] Download and play flow verified for all content types
- [ ] Rating system integrated with UI feedback
- [ ] Report system integrated with moderation queue
- [ ] Creator profiles displaying statistics
- [ ] Creator rewards configured (if incentivizing creators)
- [ ] Content versioning tested (update + rollback)
- [ ] S3/CDN configured for large asset storage
- [ ] VR content browser and editor adapted for room-scale (if targeting VR)
- [ ] Console UGC moderation requirements met (if targeting consoles)
- [ ] WebGL shareable URLs configured (if targeting browser)

# Guides

Step-by-step tutorials for common integration scenarios.

---

## Getting Started Guides

<div class="grid cards" markdown>

-   :material-login:{ .lg .middle } **Authentication Flow**

    ---

    Implement complete user authentication with multiple providers

    [:octicons-arrow-right-24: Auth Guide](auth-flow.md)

-   :material-server:{ .lg .middle } **Nakama Integration**

    ---

    Set up backend connection and make RPC calls

    [:octicons-arrow-right-24: Backend Guide](nakama-integration.md)

</div>

---

## Feature Integration Guides

<div class="grid cards" markdown>

-   :material-advertisements:{ .lg .middle } **Ad Integration**

    ---

    Configure and display rewarded, interstitial, and banner ads

    [:octicons-arrow-right-24: Ads Guide](ad-integration.md)

-   :material-translate:{ .lg .middle } **Localization Setup**

    ---

    Add multi-language support to your game

    [:octicons-arrow-right-24: Localization Guide](localization-setup.md)

-   :material-account-group:{ .lg .middle } **Friends System**

    ---

    Implement social features with real-time presence

    [:octicons-arrow-right-24: Friends Guide](friends-integration.md)

-   :material-trophy:{ .lg .middle } **Leaderboards**

    ---

    Set up global and friends leaderboards

    [:octicons-arrow-right-24: Leaderboards Guide](leaderboard-integration.md)

</div>

---

## Advanced Guides

<div class="grid cards" markdown>

-   :material-arrow-up-bold:{ .lg .middle } **Migration Guide**

    ---

    Upgrade from previous SDK versions

    [:octicons-arrow-right-24: Migration Guide](migration.md)

-   :material-puzzle:{ .lg .middle } **Custom Modules**

    ---

    Create your own SDK extensions

    [:octicons-arrow-right-24: Custom Modules](custom-modules.md)

-   :material-test-tube:{ .lg .middle } **Testing Guide**

    ---

    Best practices for testing SDK integration

    [:octicons-arrow-right-24: Testing Guide](testing.md)

</div>

---

## Quick Integration Recipes

### 5-Minute Auth Setup

```csharp
// 1. Initialize SDK (in your entry scene)
await IntelliVerseXSDK.InitializeAsync();

// 2. Add auth buttons
// Guest login
guestButton.onClick.AddListener(async () =>
{
    await IVXAuthService.LoginWithDeviceIdAsync();
    SceneManager.LoadScene("MainMenu");
});

// Email login
loginButton.onClick.AddListener(async () =>
{
    string email = emailField.text;
    string password = passwordField.text;
    await IVXAuthService.LoginAsync(email, password);
    SceneManager.LoadScene("MainMenu");
});

// 3. Check auth on protected screens
void Start()
{
    if (!IVXAuthService.IsAuthenticated)
    {
        SceneManager.LoadScene("Login");
    }
}
```

### 5-Minute Leaderboard Setup

```csharp
// 1. Submit score
await IVXLeaderboardManager.SubmitScoreAsync("main_leaderboard", score);

// 2. Display leaderboard
var entries = await IVXLeaderboardManager.GetTopScoresAsync("main_leaderboard", 10);

foreach (var entry in entries)
{
    var row = Instantiate(rowPrefab, container);
    row.GetComponent<LeaderboardRow>().SetData(
        entry.Rank,
        entry.Username,
        entry.Score
    );
}
```

### 5-Minute Ad Integration

```csharp
// 1. Initialize ads (in your bootstrap)
IVXAdsManager.Initialize();

// 2. Show rewarded ad
void OnWatchAdButton()
{
    if (IVXAdsManager.IsRewardedAdReady())
    {
        IVXAdsManager.ShowRewardedAd(
            onRewarded: () => GiveReward(100),
            onFailed: (err) => ShowError("Ad not available")
        );
    }
}
```

---

## Integration Checklist

### Basic Integration (Day 1)

- [ ] Install SDK via Package Manager
- [ ] Create configuration file
- [ ] Initialize SDK in entry scene
- [ ] Add device ID authentication
- [ ] Test backend connection

### Core Features (Week 1)

- [ ] Implement email registration/login
- [ ] Add social login (Google/Apple)
- [ ] Set up leaderboards
- [ ] Integrate basic analytics
- [ ] Add rewarded ads

### Polish (Week 2)

- [ ] Add localization
- [ ] Implement friends system
- [ ] Add achievement tracking
- [ ] Set up push notifications
- [ ] Add error handling UI

### Launch Prep

- [ ] Switch to production backend
- [ ] Replace test ad IDs
- [ ] Enable crash reporting
- [ ] Verify all deep links
- [ ] Test on target devices

---

## Video Tutorials

!!! info "Coming Soon"
    Video tutorials are in development. Subscribe to our YouTube channel for updates.

- Getting Started with IntelliVerseX
- Complete Auth Flow Tutorial
- Monetization Deep Dive
- Backend RPC Patterns

---

## Community Guides

Guides contributed by the community:

!!! tip "Contribute"
    Have a guide to share? Submit a PR to our documentation repository.

---

## Support

Need help with integration?

- 📖 [Documentation](../index.md)
- 💬 [Discord Community](https://discord.gg/intelliversex)
- 🐛 [Report Issues](https://github.com/AhamedAzmi/IntelliVerseX/issues)
- 📧 [Email Support](mailto:support@intelli-verse-x.ai)

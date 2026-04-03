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

-   :material-robot:{ .lg .middle } **AI & LLM Stack**

    ---

    Add AI NPCs, moderation, content generation, profiling, and voice to your game

    [:octicons-arrow-right-24: AI Getting Started](ai-getting-started.md)

</div>

---

## Monetization Guides

<div class="grid cards" markdown>

-   :material-currency-usd:{ .lg .middle } **Monetization Strategy**

    ---

    Choose the right revenue model for your game genre — ads, IAP, offerwall, season pass

    [:octicons-arrow-right-24: Monetization Strategy](monetization-strategy.md)

-   :material-gift:{ .lg .middle } **Offerwall Integration**

    ---

    Set up Pubscale and Xsolla offerwalls with reward callbacks

    [:octicons-arrow-right-24: Offerwall Guide](offerwall-integration.md)

-   :material-cart:{ .lg .middle } **IAP Integration**

    ---

    Apple and Google in-app purchases with server-side receipt validation

    [:octicons-arrow-right-24: IAP Guide](iap-integration.md)

</div>

---

## AI Agent Skills

<div class="grid cards" markdown>

-   :material-robot-happy:{ .lg .middle } **AI Agent Skills Reference**

    ---

    7 purpose-built skills for Cursor, Windsurf, Claude Code, and more — automate SDK integration with natural language

    [:octicons-arrow-right-24: AI Agent Skills](ai-agent-skills.md)

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

### 5-Minute Bootstrap Setup

```csharp
// 1. Add IVX_Bootstrap.prefab to your first scene
// 2. Configure IVXBootstrapConfig with your Nakama server details
// 3. Listen for ready event:
IVXBootstrap.Instance.OnBootstrapComplete += () =>
{
    Debug.Log($"SDK ready! Player: {IVXBootstrap.Instance.UserId}");
    SceneManager.LoadScene("MainMenu");
};
```

### 5-Minute AI NPC Setup

```csharp
// 1. Create IVXAIConfig asset (Assets > Create > IntelliVerseX > AI > Configuration)
// 2. Enable Mock Mode for development (no API key needed)
// 3. Initialize and register an NPC:
var npc = IVXAINPCDialogManager.Instance;
npc.Initialize(aiConfig);
npc.RegisterNPC(new IVXAINPCProfile
{
    NpcId = "merchant",
    DisplayName = "Elara the Merchant",
    PersonaPrompt = "Friendly elven merchant who loves to haggle.",
    MaxTurns = 20
});
npc.StartDialog("merchant", playerId, null, session =>
    npc.SendMessage(session.SessionId, "What's for sale?", r => ShowDialog(r.Text)));
```

### 5-Minute Leaderboard Setup

```csharp
// After IVXBootstrap is ready:
var hiro = IVXBootstrap.Instance.HiroCoordinator;
var result = await hiro.Leaderboards.SubmitScoreAsync(score);
var top = await hiro.Leaderboards.GetTopScoresAsync(10);
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
- 💬 [Discord Community](https://discord.gg/YVPxPFftMQ)
- 🐛 [Report Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/issues)
- 📧 [Email Support](mailto:support@intelli-verse-x.ai)

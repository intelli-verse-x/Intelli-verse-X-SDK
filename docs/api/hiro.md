# Hiro Live-Ops API Reference

Complete API reference for the Hiro module providing server-authoritative metagame systems via Nakama RPCs.

---

## IVXHiroCoordinator

Singleton entry point that creates, initializes, and provides access to all 33 Hiro sub-systems.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXHiroCoordinator` | Singleton instance |
| `IsInitialized` | `bool` | Whether all systems have been initialized |
| `RpcClient` | `IVXHiroRpcClient` | Low-level RPC client for direct calls |
| `Economy` | `IVXEconomySystem` | Currency operations |
| `Inventory` | `IVXInventorySystem` | Item management |
| `Achievements` | `IVXAchievementsSystem` | Hierarchical achievements |
| `Progression` | `IVXProgressionSystem` | XP, leveling, prestige |
| `Energy` | `IVXEnergySystem` | Time-gated energy |
| `Stats` | `IVXStatsSystem` | Arbitrary numeric player stats |
| `Streaks` | `IVXStreaksSystem` | Daily streaks with milestones |
| `EventLeaderboards` | `IVXEventLeaderboardSystem` | Time-limited competitive events |
| `Store` | `IVXStoreSystem` | Sectioned store with limits |
| `Challenges` | `IVXChallengesSystem` | Multi-player competitive challenges |
| `Teams` | `IVXTeamsSystem` | Team wallets and stats |
| `Tutorials` | `IVXTutorialsSystem` | Tutorial tracking with rewards |
| `Unlockables` | `IVXUnlockablesSystem` | Time-gated unlock slots |
| `Auctions` | `IVXAuctionsSystem` | Player-to-player auction house |
| `Incentives` | `IVXIncentivesSystem` | Referrals and return bonuses |
| `Mailbox` | `IVXMailboxSystem` | In-game mail with attachments |
| `RewardBuckets` | `IVXRewardBucketSystem` | Tiered point-based reward tracks |
| `Personalizer` | `IVXPersonalizerSystem` | Per-player config overrides |
| `Base` | `IVXBaseSystem` | IAP validation and purchase history |
| `Leaderboards` | `IVXLeaderboardsSystem` | Persistent leaderboards |
| `SpinWheel` | `IVXSpinWheelSystem` | Lucky wheel spins |
| `SocialPressure` | `IVXSocialPressureSystem` | Social proof feeds |
| `Retention` | `IVXRetentionSystem` | Session depth and churn risk |
| `StreakShield` | `IVXStreakShieldSystem` | Streak protection shields |
| `SessionBoosters` | `IVXSessionBoosterSystem` | Time-limited multipliers |
| `Appointments` | `IVXAppointmentSystem` | Scheduled reward windows |
| `DailyContent` | `IVXLimitedDailyContentSystem` | Rotating daily content |
| `IAPTriggers` | `IVXIAPTriggerSystem` | Context-aware purchase offers |
| `SmartAdTimer` | `IVXSmartAdTimerSystem` | Ad cooldowns and caps |
| `AdRevenueOptimizer` | `IVXAdRevenueOptimizerSystem` | Per-segment ad configs |
| `Offerwall` | `IVXOfferwallSystem` | Third-party offerwall integration |
| `FriendQuests` | `IVXFriendQuestSystem` | Cooperative friend quests |
| `FriendStreaks` | `IVXFriendStreakSystem` | Bilateral daily streaks |
| `FriendBattles` | `IVXFriendBattleSystem` | Async 1v1 challenges |

### Methods

#### InitializeSystems

```csharp
public void InitializeSystems(IClient client, ISession session)
```

Creates all 33 system instances and injects the shared RPC client. Call once after Nakama authentication.

**Parameters:**
- `client` - Nakama `IClient`
- `session` - Authenticated Nakama `ISession`

**Example:**
```csharp
IVXHiroCoordinator.Instance.InitializeSystems(nakamaClient, nakamaSession);
```

---

#### RefreshSession

```csharp
public void RefreshSession(ISession newSession)
```

Propagates a refreshed Nakama session to all systems after token renewal.

**Parameters:**
- `newSession` - New authenticated `ISession`

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnInitialized` | `Action<bool>` | Fired after `InitializeSystems` completes (`true` on success) |

---

## IVXHiroRpcClient

Low-level RPC wrapper around `IClient.RpcAsync` with typed JSON serialization and error handling.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `EnableDebugLogs` | `bool` (static) | Toggle RPC debug logging |

### Methods

#### CallAsync<T>

```csharp
public async Task<HiroRpcResponse<T>> CallAsync<T>(string rpcId, object payload = null)
```

Typed RPC call with automatic serialization and deserialization.

**Parameters:**
- `rpcId` - Server RPC identifier
- `payload` - Request payload (serialized to JSON)

**Returns:** `HiroRpcResponse<T>` containing `success`, `data`, and `error`

**Example:**
```csharp
var response = await rpcClient.CallAsync<IVXSpinWheelResult>("hiro_spin_wheel_spin", new { spinType = "free" });
if (response.success)
    ShowResult(response.data);
```

---

#### CallVoidAsync

```csharp
public async Task<bool> CallVoidAsync(string rpcId, object payload = null)
```

Fire-and-forget RPC returning success status.

**Parameters:**
- `rpcId` - Server RPC identifier
- `payload` - Request payload (optional)

**Returns:** `bool` indicating success

---

#### UpdateSession

```csharp
public void UpdateSession(ISession session)
```

Swaps the session after token refresh.

**Parameters:**
- `session` - New authenticated session

---

## IVXSpinWheelSystem

Server-authoritative lucky wheel with free, ad-gated, and currency spins.

### Methods

#### GetAsync

```csharp
public async Task<IVXSpinWheelConfig> GetAsync(string wheelId = null)
```

Fetches wheel configuration including segments, free spin counts, and costs.

**Parameters:**
- `wheelId` - Wheel identifier (optional, uses default)

**Returns:** `IVXSpinWheelConfig`

---

#### SpinAsync

```csharp
public async Task<IVXSpinWheelResult> SpinAsync(string spinType, string wheelId = null)
```

Executes a spin and receives the server-determined winning segment.

**Parameters:**
- `spinType` - `"free"`, `"ad"`, or `"currency"`
- `wheelId` - Wheel identifier (optional)

**Returns:** `IVXSpinWheelResult`

**Example:**
```csharp
var result = await IVXHiroCoordinator.Instance.SpinWheel.SpinAsync("free");
if (result != null)
    ShowRewardPopup(result.reward);
```

---

## IVXStreaksSystem

Daily engagement streaks with configurable milestone rewards.

### Methods

#### GetAsync

```csharp
public async Task<IVXStreaksGetResponse> GetAsync(string gameId = null)
```

Fetches all streaks with current and best counts.

**Returns:** `IVXStreaksGetResponse`

---

#### UpdateAsync

```csharp
public async Task<IVXStreak> UpdateAsync(string streakId, string gameId = null)
```

Increments or maintains a streak.

**Parameters:**
- `streakId` - Streak identifier (e.g. `"daily_login"`)

**Returns:** Updated `IVXStreak`

---

#### ClaimMilestoneAsync

```csharp
public async Task<IVXStreakClaimResponse> ClaimMilestoneAsync(string streakId, int milestone, string gameId = null)
```

Claims a milestone reward for a streak.

**Parameters:**
- `streakId` - Streak identifier
- `milestone` - Milestone day number

**Returns:** `IVXStreakClaimResponse`

---

## IVXRetentionSystem

Tracks session depth, churn risk, onboarding progress, and comeback bonuses.

### Methods

#### GetAsync

```csharp
public async Task<IVXRetentionState> GetAsync()
```

Fetches the full retention state for the player.

**Returns:** `IVXRetentionState`

---

#### HeartbeatAsync

```csharp
public async Task<IVXRetentionHeartbeatResponse> HeartbeatAsync()
```

Sends a session heartbeat; may return time-gated rewards.

**Returns:** `IVXRetentionHeartbeatResponse`

**Example:**
```csharp
var heartbeat = await IVXHiroCoordinator.Instance.Retention.HeartbeatAsync();
if (heartbeat?.reward != null)
    ShowRewardPopup(heartbeat.reward);
```

---

#### CompleteOnboardingStepAsync

```csharp
public async Task<IVXRetentionState> CompleteOnboardingStepAsync(int step)
```

Advances onboarding and awards step rewards.

**Parameters:**
- `step` - Onboarding step index

**Returns:** Updated `IVXRetentionState`

---

#### ClaimComebackBonusAsync

```csharp
public async Task<IVXRetentionHeartbeatResponse> ClaimComebackBonusAsync()
```

Claims the absence bonus if the player is eligible.

**Returns:** `IVXRetentionHeartbeatResponse`

---

## IVXStreakShieldSystem

Consumable shields that protect daily streaks from breaking.

### Methods

#### GetAsync

```csharp
public async Task<IVXStreakShieldState> GetAsync()
```

Returns shield count, active status, and expiry.

---

#### ActivateAsync

```csharp
public async Task<IVXStreakShieldActivateResponse> ActivateAsync()
```

Consumes one shield to protect the current streak.

---

#### ReplenishAsync

```csharp
public async Task<IVXStreakShieldReplenishResponse> ReplenishAsync(string source, string receiptOrId = null)
```

Adds shields via `"ad"`, `"iap"`, or `"currency"`.

**Parameters:**
- `source` - Replenish source
- `receiptOrId` - Receipt data or item ID (optional)

---

## IVXSessionBoosterSystem

Time-limited multiplier bonuses that incentivize longer play sessions.

### Methods

#### GetAsync

```csharp
public async Task<IVXSessionBoosterState> GetAsync()
```

Returns active and available boosters.

---

#### ActivateAsync

```csharp
public async Task<IVXSessionBoosterActivateResponse> ActivateAsync(string boosterId, string source = null)
```

Activates a booster from `"inventory"`, `"ad"`, or `"iap"`.

**Parameters:**
- `boosterId` - Booster identifier
- `source` - Activation source (optional)

---

#### ClaimFreeAsync

```csharp
public async Task<IVXSessionBoosterActivateResponse> ClaimFreeAsync()
```

Claims the next free time-gated booster.

---

## IVXAppointmentSystem

Scheduled reward windows that create habitual return behavior.

### Methods

#### GetAsync

```csharp
public async Task<IVXAppointmentState> GetAsync()
```

Returns all appointments (active, upcoming, expired).

---

#### ClaimAsync

```csharp
public async Task<IVXAppointmentClaimResponse> ClaimAsync(string appointmentId)
```

Claims a reward within the active window.

**Parameters:**
- `appointmentId` - Appointment identifier

---

## IVXLimitedDailyContentSystem

Rotating daily content slots with server-controlled reset cadence.

### Methods

#### GetAsync

```csharp
public async Task<IVXDailyContentState> GetAsync()
```

Returns all slots with claimed and locked states.

---

#### ClaimAsync

```csharp
public async Task<IVXDailyContentClaimResponse> ClaimAsync(string slotId, string actionPayload = null)
```

Claims a slot, optionally passing proof-of-action.

**Parameters:**
- `slotId` - Content slot identifier
- `actionPayload` - Proof-of-action JSON (optional)

---

## IVXIAPTriggerSystem

Context-aware purchase offer triggers based on player behavior signals.

### Methods

#### EvaluateAsync

```csharp
public async Task<IVXIAPTriggerEvalResponse> EvaluateAsync(string context, string contextValue = null)
```

Evaluates context and returns triggered offers.

**Parameters:**
- `context` - Context type (`"post_game"`, `"store_visit"`, `"low_currency"`, `"win_streak"`, `"level_up"`)
- `contextValue` - Optional context value

**Returns:** `IVXIAPTriggerEvalResponse`

---

#### DismissAsync

```csharp
public async Task<IVXIAPTriggerDismissResponse> DismissAsync(string triggerId)
```

Records offer dismissal and updates cooldown timers.

---

#### RecordConversionAsync

```csharp
public async Task<bool> RecordConversionAsync(string triggerId, string receipt)
```

Records a successful IAP conversion with receipt validation.

**Parameters:**
- `triggerId` - Trigger identifier
- `receipt` - IAP receipt data

**Returns:** `bool` indicating success

---

## IVXSmartAdTimerSystem

Manages interstitial cooldowns, rewarded ad caps, and banner eligibility.

### Methods

#### GetAsync

```csharp
public async Task<IVXSmartAdTimerState> GetAsync()
```

Returns cooldowns, daily caps, and banner status.

---

#### CanShowAsync

```csharp
public async Task<IVXSmartAdTimerState> CanShowAsync(string adType)
```

Checks if an ad type can be shown now.

**Parameters:**
- `adType` - `"interstitial"`, `"rewarded"`, or `"banner"`

---

#### RecordImpressionAsync

```csharp
public async Task<IVXSmartAdTimerRecordResponse> RecordImpressionAsync(string adType, string placementId = null)
```

Records an ad impression; returns associated reward for rewarded ads.

**Parameters:**
- `adType` - Ad type
- `placementId` - Placement identifier (optional)

---

## IVXAdRevenueOptimizerSystem

Per-segment placement configurations with remote-configurable settings.

### Methods

#### GetConfigAsync

```csharp
public async Task<IVXAdRevenueConfig> GetConfigAsync()
```

Returns personalized ad placement config for the player.

---

#### RecordImpressionAsync

```csharp
public async Task<IVXAdImpressionResponse> RecordImpressionAsync(string placementId, string adNetwork = null, float? revenue = null)
```

Records an impression with optional revenue attribution.

**Parameters:**
- `placementId` - Placement identifier
- `adNetwork` - Ad network name (optional)
- `revenue` - Revenue amount (optional)

---

## IVXOfferwallSystem

Server-authoritative offerwall integration with reward validation and deduplication.

### Methods

#### GetAsync

```csharp
public async Task<IVXOfferwallState> GetAsync()
```

Returns available, pending, and completed offers.

---

#### CompleteOfferAsync

```csharp
public async Task<IVXOfferwallCompleteResponse> CompleteOfferAsync(string offerId, string provider, string transactionId)
```

Records offer completion from a provider callback.

**Parameters:**
- `offerId` - Offer identifier
- `provider` - Provider name (e.g. `"tapjoy"`)
- `transactionId` - Provider transaction ID

---

#### ClaimPendingAsync

```csharp
public async Task<IVXOfferwallCompleteResponse> ClaimPendingAsync()
```

Credits pending offerwall rewards to the player wallet.

---

## IVXSocialPressureSystem

Social proof feeds and online counters.

### Methods

#### GetAsync

```csharp
public async Task<IVXSocialPressureState> GetAsync()
```

Returns social pressure data including friends online count and activity feeds.

---

## IVXFriendQuestSystem

Cooperative quests between friends with shared progress tracking.

### Methods

#### GetAsync

```csharp
public async Task<IVXFriendQuestState> GetAsync()
```

Returns active, available, and completed quests.

---

#### AcceptAsync

```csharp
public async Task<IVXFriendQuestAcceptResponse> AcceptAsync(string questId, string partnerId)
```

Accepts a quest with a friend (both must accept).

**Parameters:**
- `questId` - Quest identifier
- `partnerId` - Friend's user ID

---

#### ReportProgressAsync

```csharp
public async Task<IVXFriendQuestProgressResponse> ReportProgressAsync(string questId, int amount)
```

Reports progress; server aggregates both players.

**Parameters:**
- `questId` - Quest identifier
- `amount` - Progress increment

---

## IVXFriendStreakSystem

Bilateral daily interaction streaks where both friends contribute each day.

### Methods

#### GetAsync

```csharp
public async Task<IVXFriendStreakState> GetAsync()
```

Returns all friend streaks for the player.

---

#### InteractAsync

```csharp
public async Task<IVXFriendStreakInteractResponse> InteractAsync(string friendId)
```

Records a daily interaction to maintain or advance the streak.

**Parameters:**
- `friendId` - Friend's user ID

---

#### ClaimMilestoneAsync

```csharp
public async Task<IVXFriendStreakInteractResponse> ClaimMilestoneAsync(string streakId, int day)
```

Claims a milestone reward (e.g. day 3, 7, 14, 30).

**Parameters:**
- `streakId` - Streak identifier
- `day` - Milestone day number

---

## IVXFriendBattleSystem

Asynchronous 1v1 challenges with optional wagers and server-validated scoring.

### Methods

#### GetAsync

```csharp
public async Task<IVXFriendBattleState> GetAsync()
```

Returns pending, active, and recent battles.

---

#### SendChallengeAsync

```csharp
public async Task<IVXFriendBattleSendResponse> SendChallengeAsync(string friendId, string gameMode, int? score = null)
```

Sends a challenge to a friend.

**Parameters:**
- `friendId` - Friend's user ID
- `gameMode` - Game mode identifier
- `score` - Optional initial score (if challenger plays first)

---

#### AcceptChallengeAsync

```csharp
public async Task<IVXFriendBattleSendResponse> AcceptChallengeAsync(string challengeId)
```

Accepts a pending challenge.

**Parameters:**
- `challengeId` - Challenge identifier

---

#### SubmitScoreAsync

```csharp
public async Task<IVXFriendBattleSubmitResponse> SubmitScoreAsync(string challengeId, int score)
```

Submits a score; the winner is determined when both players submit.

**Parameters:**
- `challengeId` - Challenge identifier
- `score` - Player's score

---

## Data Models

### HiroRpcResponse<T>

| Property | Type | Description |
|----------|------|-------------|
| `success` | `bool` | Whether the RPC succeeded |
| `data` | `T` | Deserialized response data |
| `error` | `string` | Error message on failure |

### IVXReward

Shared reward container used across all systems.

| Property | Type | Description |
|----------|------|-------------|
| `currencies` | `Dictionary<string, float>` | Currency rewards (e.g. `{ "coins": 500 }`) |
| `items` | `Dictionary<string, int>` | Item rewards (e.g. `{ "shield_token": 1 }`) |

### IVXSpinWheelConfig

| Property | Type | Description |
|----------|------|-------------|
| `wheelId` | `string` | Wheel identifier |
| `name` | `string` | Display name |
| `segments` | `List<IVXSpinWheelSegment>` | Wheel segments |
| `freeSpinsRemaining` | `int` | Free spins left today |
| `maxFreeSpinsPerDay` | `int` | Daily free spin cap |
| `nextFreeSpinAt` | `long` | Unix timestamp for next free spin |
| `adSpinsRemaining` | `int` | Ad spins left today |
| `maxAdSpinsPerDay` | `int` | Daily ad spin cap |
| `spinCost` | `IVXReward` | Cost for a currency spin |

### IVXSpinWheelSegment

| Property | Type | Description |
|----------|------|-------------|
| `segmentId` | `string` | Segment identifier |
| `label` | `string` | Display label |
| `reward` | `IVXReward` | Segment reward |
| `weight` | `float` | Server-side weight |
| `color` | `string` | Hex color code |
| `isJackpot` | `bool` | Whether this is the jackpot segment |

### IVXStreak

| Property | Type | Description |
|----------|------|-------------|
| `streakId` | `string` | Streak identifier |
| `name` | `string` | Display name |
| `currentCount` | `int` | Current streak count |
| `bestCount` | `int` | All-time best |
| `lastUpdateSec` | `long` | Last update timestamp |
| `resetAt` | `long` | Reset deadline timestamp |
| `claimedMilestones` | `List<int>` | Already claimed milestones |

### IVXRetentionState

| Property | Type | Description |
|----------|------|-------------|
| `userId` | `string` | Player identifier |
| `firstSessionAt` | `long` | First session timestamp |
| `lastSessionAt` | `long` | Last session timestamp |
| `totalSessions` | `int` | Total session count |
| `currentSessionDepth` | `int` | Current session depth |
| `daysSinceLastSession` | `int` | Days since last visit |
| `churnRisk` | `string` | `"low"`, `"medium"`, or `"high"` |
| `onboardingComplete` | `bool` | Whether onboarding is finished |
| `onboardingStep` | `int` | Current onboarding step |
| `comebackBonusAvailable` | `bool` | Whether a comeback bonus is claimable |
| `comebackBonusReward` | `IVXReward` | The comeback bonus reward |

### IVXIAPTrigger

| Property | Type | Description |
|----------|------|-------------|
| `triggerId` | `string` | Trigger identifier |
| `offerSku` | `string` | Store SKU for the offer |
| `triggerType` | `string` | Trigger context type |
| `displayTitle` | `string` | Offer title |
| `displayMessage` | `string` | Offer message |
| `expiresAt` | `long` | Expiry timestamp |
| `priority` | `int` | Display priority |
| `metadata` | `Dictionary<string, string>` | Additional metadata |

### IVXSmartAdTimerState

| Property | Type | Description |
|----------|------|-------------|
| `interstitialCooldownSec` | `int` | Interstitial cooldown seconds |
| `nextInterstitialAt` | `long` | Next allowed interstitial timestamp |
| `rewardedAdsToday` | `int` | Rewarded ads shown today |
| `maxRewardedAdsPerDay` | `int` | Daily rewarded ad cap |
| `bannerEnabled` | `bool` | Whether banners are enabled |
| `sessionAdCount` | `int` | Total ads in current session |

### IVXFriendQuest

| Property | Type | Description |
|----------|------|-------------|
| `questId` | `string` | Quest identifier |
| `title` | `string` | Quest title |
| `description` | `string` | Quest description |
| `targetValue` | `int` | Goal value |
| `currentValue` | `int` | Current progress |
| `partnerId` | `string` | Partner's user ID |
| `partnerName` | `string` | Partner's display name |
| `partnerProgress` | `int` | Partner's contribution |
| `reward` | `IVXReward` | Quest reward |
| `status` | `string` | `"available"`, `"active"`, or `"completed"` |
| `expiresAt` | `long` | Expiry timestamp |

### IVXFriendStreak

| Property | Type | Description |
|----------|------|-------------|
| `streakId` | `string` | Streak identifier |
| `friendId` | `string` | Friend's user ID |
| `friendName` | `string` | Friend's display name |
| `currentStreak` | `int` | Current streak count |
| `longestStreak` | `int` | All-time best |
| `lastInteractionAt` | `long` | Last interaction timestamp |
| `myContributionToday` | `bool` | Whether the player contributed today |
| `friendContributionToday` | `bool` | Whether the friend contributed today |
| `expiresAt` | `long` | Expiry timestamp |

### IVXFriendBattleChallenge

| Property | Type | Description |
|----------|------|-------------|
| `challengeId` | `string` | Challenge identifier |
| `challengerId` | `string` | Challenger's user ID |
| `challengerName` | `string` | Challenger's display name |
| `challengerScore` | `int` | Challenger's score |
| `opponentId` | `string` | Opponent's user ID |
| `opponentName` | `string` | Opponent's display name |
| `opponentScore` | `int` | Opponent's score |
| `gameMode` | `string` | Game mode |
| `status` | `string` | `"pending"`, `"active"`, or `"completed"` |
| `wager` | `IVXReward` | Optional wager |
| `winnerReward` | `IVXReward` | Winner's reward |
| `expiresAt` | `long` | Expiry timestamp |
| `winnerId` | `string` | Winner's user ID |

---

## See Also

- [Hiro Module Guide](../modules/hiro.md)
- [Backend Module](../modules/backend.md)
- [Social Module](../modules/social.md)
- [Monetization Module](../modules/monetization.md)

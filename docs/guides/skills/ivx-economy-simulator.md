# Economy Simulator

**Skill ID:** `ivx-economy-simulator`

---
name: ivx-economy-simulator
description: >-
  Design, simulate, and monitor game economies for IntelliVerseX SDK games.
  Use when the user says "economy design", "currency flow", "balance economy",
  "inflation detection", "reward curves", "economy simulator", "Monte Carlo",
  "pricing optimization", "Gini coefficient", "economy health", "currency sinks",
  "currency sources", "exchange rates", "loot tables", or needs help with
  any virtual economy design or balancing.
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

The Economy Simulator provides tools for designing, testing, and monitoring virtual game economies. It builds on top of the Hiro economy system to add currency flow modeling, Monte Carlo simulation, inflation detection, reward curve design, and real-time economy health monitoring.

```
Economy Design (offline)          Live Economy (runtime)
┌──────────────────────┐         ┌──────────────────────┐
│ Currency Flow Model  │         │ IVXEconomyMonitor    │
│ Reward Curve Design  │  ────►  │ Health Dashboard     │
│ Monte Carlo Sim      │  deploy │ Inflation Alerts     │
│ Pricing Optimizer    │         │ Player Wealth Dist.  │
└──────────────────────┘         └──────────────────────┘
         │                                │
         ▼                                ▼
  Hiro Economy Config              Satori Metrics
  (hiro_configs/economy)           (economy events)
```

---

## 1. Currency Flow Model

### Defining Sources and Sinks

```csharp
using IntelliVerseX.Economy;

var model = new IVXCurrencyFlowModel("coins");

model.AddSource(new CurrencySource
{
    Name = "level_reward",
    AveragePerDay = 500,
    Variance = 100,
    PlayerSegment = "all",
});

model.AddSource(new CurrencySource
{
    Name = "daily_login",
    AveragePerDay = 200,
    Variance = 0,
    PlayerSegment = "retained",
});

model.AddSource(new CurrencySource
{
    Name = "rewarded_ad",
    AveragePerDay = 300,
    Variance = 200,
    PlayerSegment = "ad_viewers",
});

model.AddSink(new CurrencySink
{
    Name = "store_purchase",
    AveragePerDay = 400,
    Variance = 150,
});

model.AddSink(new CurrencySink
{
    Name = "energy_refill",
    AveragePerDay = 150,
    Variance = 50,
});

model.AddSink(new CurrencySink
{
    Name = "gacha_pull",
    AveragePerDay = 200,
    Variance = 300,
});
```

### Flow Analysis

```csharp
var analysis = model.Analyze();

Debug.Log($"Daily net flow: {analysis.NetFlowPerDay:+#;-#;0}");
Debug.Log($"Source total: {analysis.TotalSourcesPerDay}/day");
Debug.Log($"Sink total: {analysis.TotalSinksPerDay}/day");
Debug.Log($"Balance ratio: {analysis.SinkToSourceRatio:F2}");
Debug.Log($"Inflation risk: {analysis.InflationRisk}");
```

Recommended sink-to-source ratio: **0.8–1.2** for a healthy economy. Below 0.8 indicates inflation risk.

---

## 2. Monte Carlo Simulation

### Running Simulations

```csharp
var sim = new IVXEconomySimulator();

sim.AddCurrency(model);
sim.SetPlayerCount(1000);
sim.SetDurationDays(90);
sim.SetIterations(100);

sim.SetPlayerBehavior(new PlayerBehaviorProfile
{
    DailySessionProbability = 0.6f,
    PurchaseProbability = 0.05f,
    AdViewProbability = 0.3f,
    ChurnCurve = ChurnCurve.Exponential(halfLifeDays: 14),
});

var results = await sim.RunAsync();

Debug.Log($"Day 30 median balance: {results.GetMedianBalance(30)}");
Debug.Log($"Day 30 P90 balance: {results.GetPercentileBalance(30, 90)}");
Debug.Log($"Day 60 Gini coefficient: {results.GetGiniCoefficient(60):F3}");
Debug.Log($"Day 90 inflation rate: {results.GetInflationRate(90):P1}");
```

### Simulation Output

| Metric | Description | Healthy Range |
|--------|-------------|--------------|
| **Median Balance** | Middle player's currency holdings | Game-specific |
| **Gini Coefficient** | Wealth inequality (0 = equal, 1 = one player has all) | 0.3–0.6 |
| **Inflation Rate** | Average balance growth rate vs day 1 | < 5%/month |
| **Deflation Rate** | Average balance decline rate | < 3%/month |
| **Velocity** | Currency turnover rate (spend/balance) | 0.5–2.0/week |
| **Sink Coverage** | % of earned currency that gets spent | 60–90% |

---

## 3. Reward Curve Designer

### Defining Progression Rewards

```csharp
var curve = new IVXRewardCurve("level_rewards");

curve.SetBaseReward(100);
curve.SetGrowthFunction(RewardGrowth.Logarithmic);
curve.SetGrowthRate(1.15f);
curve.SetMaxLevel(100);
curve.SetMilestones(new Dictionary<int, int>
{
    { 10, 500 },
    { 25, 2000 },
    { 50, 10000 },
    { 100, 50000 },
});

for (int level = 1; level <= 20; level++)
{
    int reward = curve.GetReward(level);
    Debug.Log($"Level {level}: {reward} coins");
}
```

### Growth Functions

| Function | Formula | Use Case |
|----------|---------|----------|
| `Linear` | `base * level` | Simple, predictable |
| `Logarithmic` | `base * log(level) * rate` | Diminishing returns (recommended) |
| `Exponential` | `base * rate^level` | Aggressive progression |
| `Polynomial` | `base * level^exponent` | Balanced middle ground |
| `SCurve` | Sigmoid function | Slow start, fast middle, slow end |
| `Custom` | User-defined function | Game-specific tuning |

---

## 4. Store Pricing Optimization

### A/B Testing Prices

```csharp
using IntelliVerseX.Economy;

var optimizer = new IVXPricingOptimizer("gem_pack_small");

optimizer.SetBasePrice("coins", 500);
optimizer.SetVariants(new[]
{
    new PriceVariant { Name = "control", Price = 500 },
    new PriceVariant { Name = "discount", Price = 350 },
    new PriceVariant { Name = "premium", Price = 750 },
});

optimizer.LinkToExperiment("store_pricing_test");
```

This creates a Satori experiment that assigns players to price variants and tracks conversion and revenue per variant.

### Elasticity Analysis

```csharp
var elasticity = await optimizer.GetElasticityAsync();

Debug.Log($"Price elasticity: {elasticity.Coefficient:F2}");
Debug.Log($"Revenue-maximizing price: {elasticity.OptimalPrice}");
Debug.Log($"Expected conversion at optimal: {elasticity.OptimalConversion:P1}");
```

---

## 5. Economy Health Monitor

### Real-Time Monitoring

```csharp
var monitor = IVXEconomyMonitor.Instance;

monitor.StartMonitoring(new MonitorConfig
{
    Currencies = new[] { "coins", "gems" },
    SampleIntervalMinutes = 60,
    AlertOnInflation = true,
    InflationThresholdPercent = 10.0f,
    AlertOnDeflation = true,
    DeflationThresholdPercent = 5.0f,
});

monitor.OnAlert += (alert) =>
{
    Debug.LogWarning($"Economy alert: {alert.Currency} - {alert.Type}: {alert.Message}");
};
```

### Health Dashboard Metrics

| Metric | Query | Alert Threshold |
|--------|-------|----------------|
| **Average balance** | Per currency, rolling 24h | > 2x baseline |
| **Median balance** | Per currency, rolling 24h | > 2x baseline |
| **Gini coefficient** | Wealth distribution | > 0.7 |
| **Velocity** | Spend rate / total supply | < 0.3/week |
| **Source breakdown** | % of income by source | Any source > 60% |
| **Sink breakdown** | % of spend by sink | Any sink > 60% |
| **New currency created** | Daily minting total | > 1.5x daily average |

### Querying Health

```csharp
var health = await IVXEconomyMonitor.Instance.GetHealthAsync("coins");

Debug.Log($"Status: {health.Status}");
Debug.Log($"Avg balance: {health.AverageBalance}");
Debug.Log($"Gini: {health.GiniCoefficient:F3}");
Debug.Log($"Velocity: {health.Velocity:F2}/week");
Debug.Log($"Inflation rate: {health.InflationRate:P1}");
```

---

## 6. Server-Side Economy Config

The economy simulator reads and writes to the Hiro economy config:

```
hiro_configs/economy
├── currencies[]          — currency definitions
│   ├── id, name, max_count
│   └── initial_amount
├── exchange_rates[]      — currency conversion rates
├── store_items[]         — purchasable items
└── donation_config       — team/guild economy settings
```

### Deploying Simulation Results

```csharp
var deployConfig = model.ToHiroConfig();
await IVXHiroConfigManager.Instance.SetAsync("economy", deployConfig);
```

---

## 7. Cross-Platform API

| Engine | Class / Module | Simulation API |
|--------|---------------|---------------|
| **Unity** | `IVXEconomySimulator` | Full simulation + monitor |
| **Unreal** | `UIVXEconomySubsystem` | Monitor only (sim via Editor tools) |
| **Godot** | `IVXEconomy` autoload | Monitor only |
| **JavaScript** | `IVXEconomySimulator` | Full simulation (Node.js) + monitor |
| **All others** | Server-side via Nakama RPC | Monitor via analytics events |

The full Monte Carlo simulator is available in Unity Editor and Node.js for offline design. Runtime monitoring works on all platforms.

---

## Platform-Specific Economy Design

### VR Platforms

| Consideration | Guidance |
|--------------|----------|
| **Session economics** | VR sessions are 15-45 min. Design earn rates for satisfying rewards in short windows. |
| **Premium pricing** | VR users expect premium experiences. Higher price points with fewer IAP prompts. |
| **Physical comfort** | Don't require grind-heavy economies that incentivize extended sessions. |

### Console Platforms

| Platform | Economy Notes |
|----------|-------------|
| **Game Pass (Xbox)** | Players didn't pay upfront. More aggressive but fair monetization is acceptable. |
| **PS Plus** | Similar to Game Pass. Balance generosity with engagement. |
| **Switch** | Handheld mode = shorter sessions. Mobile-like economy pacing. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Session persistence** | Browser sessions may be interrupted. Auto-save economy state frequently. |
| **Currency display** | Use `Intl.NumberFormat` for locale-appropriate currency display in web builds. |

---

## Checklist

- [ ] Currency flow model defined for each currency (sources + sinks)
- [ ] Sink-to-source ratio within 0.8–1.2 range
- [ ] Monte Carlo simulation run with 90-day projections
- [ ] Gini coefficient below 0.7 in simulations
- [ ] Inflation rate below 5%/month in simulations
- [ ] Reward curves defined for all progression systems
- [ ] Store pricing A/B test configured via Satori
- [ ] Economy health monitor active with alert thresholds
- [ ] Hiro economy config deployed from simulation results
- [ ] VR earn rates tuned for short session lengths (if targeting VR)
- [ ] Console economy reviewed for platform monetization norms (if targeting consoles)
- [ ] Browser session persistence tested (if targeting WebGL)

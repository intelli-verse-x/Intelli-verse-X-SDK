# IntelliVerseX SDK — Pricing & Marketplace Strategy

**Date:** April 2, 2026
**Scope:** Every viable distribution channel, pricing model, and revenue strategy
**Status:** Strategic analysis — decisions required before execution

---

## Table of Contents

1. [Pricing Philosophy](#1-pricing-philosophy)
2. [Tier Structure](#2-tier-structure)
3. [Marketplace-by-Marketplace Analysis](#3-marketplace-by-marketplace-analysis)
4. [Revenue Projections](#4-revenue-projections)
5. [Competitor Pricing Comparison](#5-competitor-pricing-comparison)
6. [What Goes Free vs. Paid](#6-what-goes-free-vs-paid)
7. [Licensing Model](#7-licensing-model)
8. [Launch Sequence & Timing](#8-launch-sequence--timing)
9. [Anti-Patterns to Avoid](#9-anti-patterns-to-avoid)
10. [Decision Matrix](#10-decision-matrix)

---

## 1. Pricing Philosophy

### The Core Principle: Open Core + Managed Services

The most successful developer tool companies in 2025-2026 follow the **open-core** model:

```
FREE (Open Source)              PAID (Managed + Premium)
┌─────────────────────┐        ┌──────────────────────────┐
│ Core SDK             │        │ Managed cloud backend    │
│ All 22 AI skills     │        │ Priority support (SLA)   │
│ Nakama self-hosted   │        │ Enterprise dashboards    │
│ Basic tools          │        │ Advanced AI quotas       │
│ Community support    │        │ White-label / OEM        │
│ Documentation        │        │ Custom integrations      │
│ CI/CD templates      │        │ Compliance certification │
└─────────────────────┘        └──────────────────────────┘
         │                                  │
    Distribution moat               Revenue engine
    (adoption + trust)              (conversion at scale)
```

**Why free core:**
- 95% of game developers will never pay for a backend SDK if a free alternative exists
- Open source builds trust — studios can audit code before depending on it
- Skills as distribution — free skills get embedded in thousands of AI workflows
- Self-hosted Nakama is already free — competing on price against "free" is futile
- Network effects — more users = more community fixes, content, and reputation

**Why paid services:**
- Self-hosting Nakama at scale requires DevOps expertise most studios lack
- AI features (voice, NPC, content gen) have real per-call costs (OpenAI/Azure/Anthropic)
- Enterprise studios need SLAs, compliance audits, and dedicated support
- Managed dashboards, analytics visualization, and real-time monitoring have ongoing costs
- White-label and OEM deals are high-value, low-volume

---

## 2. Tier Structure

### Recommended Tiers

| Tier | Price | Target | Includes |
|------|-------|--------|----------|
| **Community** | **Free forever** | Solo devs, students, jam participants, hobbyists | Full SDK (all engines), all 22 skills, self-hosted Nakama, tools, CI templates, community Discord, docs |
| **Indie** | **$0/mo** (usage-based AI) | Indie studios < $200K revenue | Everything in Community + managed Nakama (up to 1K MAU free), 10K AI calls/mo free, basic dashboard, email support |
| **Studio** | **$99/mo** | Mid-size studios (5-50 people) | Everything in Indie + managed Nakama (up to 50K MAU), 100K AI calls/mo, analytics dashboard, priority email support, remote config UI |
| **Enterprise** | **$499/mo** (or custom) | AAA studios, publishers, 50+ people | Everything in Studio + unlimited MAU (usage-based above 500K), unlimited AI calls, SLA (99.9%), dedicated Slack, compliance reports, white-label, custom integrations |
| **OEM / Publisher** | **Custom** | Publishers embedding IVX in multiple titles | Volume licensing, revenue share option, co-branded, dedicated infrastructure, on-prem option |

### Why These Prices

| Tier | Rationale |
|------|-----------|
| **Free** | Matches Nakama self-hosted (free). Must be free to compete. Skills distribution requires zero friction. |
| **$0 + usage** | Mimics Supabase/Vercel model. Gets studios using managed infra with no commitment. Converts when they scale. |
| **$99/mo** | Sweet spot for mid-size studios. Less than one junior developer hour per day. Comparable to LootLocker Developer tier. Under PlayFab Essentials. |
| **$499/mo** | Below AccelByte/PlayFab enterprise pricing ($5K-$50K/mo). Attractive to studios spending $10K+/mo on fragmented tools. |
| **Custom** | Publishers like Scopely, Zynga, or Supercell negotiate volume deals. Revenue share (2-5% of game revenue) is common. |

---

## 3. Marketplace-by-Marketplace Analysis

### Category A: Free Distribution (Zero Cost to Publish, Maximum Reach)

---

#### 1. GitHub Repository (Primary)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** (MIT license) |
| **Revenue share** | N/A |
| **Reach** | Global developer community (100M+ GitHub users) |
| **What to list** | Full SDK, all skills, all tools, documentation |
| **Strategic value** | Foundation of everything. SEO. Trust. Stars = social proof. |
| **Action** | Already live. Optimize README (done). Add topics/tags. Enable Sponsors. |
| **Verdict** | **FREE — non-negotiable** |

---

#### 2. GitHub Sponsors

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | Tiered sponsorship ($5 / $15 / $50 / $100+ /mo) |
| **Revenue share** | **0%** (GitHub takes nothing; first-year matching available) |
| **Reach** | GitHub users who value the project |
| **What to list** | Sponsor tiers with benefits (logo in README, priority issues, 1:1 calls, feature requests) |
| **Strategic value** | Low effort, passive income, community goodwill. Avg $1,850/mo across surveyed devs. |
| **Revenue potential** | $500-$5,000/mo depending on star count and community size |
| **Action** | Set up tiers: $5 (thanks + badge), $15 (priority issues), $50 (monthly office hours), $100 (logo in README + priority feature requests) |
| **Verdict** | **SPONSOR TIERS — do immediately** |

---

#### 3. Cursor Skills Registry

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A (no marketplace commerce yet) |
| **Reach** | All Cursor users (Pro: $20/mo, Pro+: $60/mo, Ultra: $200/mo) — high-value developers |
| **What to list** | All 22 SKILL.md files |
| **Strategic value** | Very high — Cursor is the leading AI code editor. Skills auto-activate. Direct workflow integration. |
| **Action** | Submit all 22 skills to Cursor's skill directory |
| **Verdict** | **FREE — critical distribution channel** |

---

#### 4. Claude Code Plugin Marketplace

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A (2,787 skills / 416 plugins available, no paid model yet) |
| **Reach** | All Claude Code users — growing rapidly |
| **What to list** | Plugin with all 22 skills as `@intelliversex` namespace |
| **Strategic value** | High — Claude Code's skill marketplace is actively growing. Early presence = discoverability. |
| **Action** | Package as plugin, submit to marketplace |
| **Verdict** | **FREE — early mover advantage** |

---

#### 5. Windsurf / Codeium Extensions

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A |
| **Reach** | Windsurf users |
| **What to list** | SKILL.md compatible files (same format as Cursor) |
| **Strategic value** | Medium — smaller user base than Cursor, but growing. Zero incremental effort since format is identical. |
| **Action** | Submit to Windsurf extension/skill registry |
| **Verdict** | **FREE — zero effort since format matches Cursor** |

---

#### 6. Smithery (MCP Registry)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** (MCP server listing) |
| **Revenue share** | N/A |
| **Reach** | 125,000+ skills, 4,700+ MCP servers on platform |
| **What to list** | IntelliVerseX MCP server (50+ Nakama/Hiro/Satori tools) |
| **Strategic value** | High — MCP is becoming the standard for AI-tool integration. Already referenced in README. |
| **Action** | `smithery mcp publish` — submit the MCP server |
| **Verdict** | **FREE — publish MCP server immediately** |

---

#### 7. npm (JavaScript SDK)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A |
| **Reach** | 17M+ npm developers |
| **What to list** | `@intelliversex/sdk` package + `@intelliversex/mcp-server` |
| **Strategic value** | Package manager presence is a trust signal. Enables `npx` one-liner installs. |
| **Action** | Publish JavaScript SDK as scoped npm package |
| **Verdict** | **FREE — standard distribution** |

---

#### 8. PyPI (Python Tools)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A |
| **Reach** | Python developers |
| **What to list** | `intelliversex-tools` — asset pipeline CLI (scaffold, spritesheet, validate) |
| **Strategy** | `pip install intelliversex-tools` makes the asset pipeline accessible to any developer regardless of engine |
| **Action** | Package `tools/asset-pipeline/` as a PyPI distribution |
| **Verdict** | **FREE — extends reach beyond game engines** |

---

#### 9. OpenUPM (Unity Package Registry)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A |
| **Reach** | Unity developers who prefer UPM over Asset Store |
| **What to list** | `com.intelliversex.sdk` |
| **Strategic value** | Already listed. Maintains trust with Unity community. |
| **Action** | Already live — keep updated |
| **Verdict** | **FREE — already active** |

---

#### 10. pub.dev (Flutter/Dart)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A |
| **Reach** | Flutter developers |
| **What to list** | `intelliversex_sdk` Dart package |
| **Action** | Publish Dart SDK package |
| **Verdict** | **FREE** |

---

#### 11. Maven Central / Gradle (Java/Android)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | **Free** |
| **Revenue share** | N/A |
| **Reach** | Android/Java developers |
| **What to list** | `com.intelliversex:sdk` |
| **Action** | Publish Java SDK |
| **Verdict** | **FREE** |

---

#### 12. Awesome Lists (GitHub)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free (PR submission) |
| **Price to charge** | N/A (link listing) |
| **Reach** | High for niche communities |
| **What to list** | Links to SDK repo |
| **Target lists** | `awesome-gamedev`, `awesome-unity`, `awesome-nakama`, `awesome-godot`, `awesome-mcp`, `awesome-ai-tools` |
| **Strategic value** | SEO backlinks + discoverability in curated lists |
| **Action** | Submit PRs to 6+ awesome lists |
| **Verdict** | **FREE — high ROI for effort** |

---

### Category B: Free-to-List, Revenue from Conversions

---

#### 13. Product Hunt

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | N/A (awareness channel) |
| **Reach** | 10,000+ visitors on #1 day, 1,000-3,000 email captures |
| **What to list** | "IntelliVerseX: 22 AI Skills for Game Development — GDD to Live Ops" |
| **Strategic value** | Very high for launch. Creates press, backlinks, and social proof. Drives signups for managed service. |
| **Timing** | Tuesday-Thursday launch at 12:01 AM PT. Need 400+ pre-launch supporters. |
| **Action** | Prepare assets (5-6 screenshots, 1-2 min video, maker story). Build waitlist 4-6 weeks before. |
| **Verdict** | **FREE — one-time high-impact launch event** |

---

#### 14. Hacker News (Show HN)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | N/A |
| **Reach** | Highly technical audience, venture capitalists, CTOs |
| **What to list** | "Show HN: IntelliVerseX — Open-source game SDK with 22 AI agent skills" |
| **Strategic value** | High for credibility and investor visibility. HN front page = instant legitimacy in dev tools. |
| **Action** | Post on a weekday morning. Engage in comments. Be honest about what's shipped vs planned. |
| **Verdict** | **FREE — critical for investor/VC visibility** |

---

#### 15. Dev.to / Hashnode / Medium

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | N/A (content marketing) |
| **Reach** | Dev.to: ~500K daily readers; Medium: broader tech audience |
| **What to list** | Tutorial series, launch announcement, deep-dive articles |
| **Revenue** | Medium Partner Program: $150-$450/article for well-performing dev content |
| **Article ideas** | "How to build a game backend in 10 minutes", "AI NPC dialog in Unity with 20 lines", "From GDD to live ops: automating game development with AI skills" |
| **Action** | Publish 1 article/week for 6 weeks around launch |
| **Verdict** | **FREE — content marketing engine** |

---

#### 16. YouTube

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free (production cost: time) |
| **Price to charge** | Ad revenue + awareness |
| **Reach** | Gamedev YouTube is underserved — high potential |
| **What to list** | Skill walkthrough series (22 videos, one per skill), quickstart tutorials, live coding |
| **Revenue potential** | $500-$5,000/mo at scale via AdSense + managed service conversions |
| **Action** | Record 4 launch videos: (1) Overview, (2) 5-min setup, (3) AI NPC demo, (4) Live ops in 10 min |
| **Verdict** | **FREE — high long-term ROI, medium effort** |

---

#### 17. Discord (Community Server)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | N/A (community channel) |
| **Reach** | Targeted — developers who actively use the SDK |
| **What to list** | Support channels, showcase, feedback, announcements |
| **Strategic value** | Retention + feedback loop. Reduces support burden. Community-generated content. |
| **Action** | Already exists (discord.gg/YVPxPFftMQ). Activate with regular updates. |
| **Verdict** | **FREE — essential community infrastructure** |

---

#### 18. Reddit

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Price to charge** | N/A |
| **Reach** | r/gamedev (2.2M), r/unity3d (350K), r/unrealengine (450K), r/indiedev (400K), r/godot (200K) |
| **What to list** | Launch posts, tutorials, "how we built X" stories |
| **Rules** | Must provide value, not just self-promote. Use Show-off Saturday or Free Talk Friday threads. |
| **Action** | Post in 3-5 subreddits at launch with different angles per community |
| **Verdict** | **FREE — high reach, must respect community norms** |

---

### Category C: Paid Marketplaces (Revenue Generating)

---

#### 19. Unity Asset Store

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free to list |
| **Revenue share** | **~70/30** (publisher keeps ~70%, Unity takes ~30%) |
| **Reach** | Massive — primary discovery channel for Unity devs |
| **Price recommendation** | **FREE listing (Community tier)** with **paid "Pro" add-on ($49.99)** |
| **What goes free** | Core SDK: Bootstrap, Auth, Backend, Economy, Leaderboards, Storage, basic analytics |
| **What goes paid ($49.99)** | "IntelliVerseX Pro Pack": 16 demo UIs, all Hiro system prefabs, AI integration prefabs, multiplayer templates, season pass/fortune wheel/tournament prefabs, all sample scenes |
| **Strategic value** | Very high. Unity Asset Store is where Unity devs discover tools. A free listing drives adoption; a paid add-on generates revenue. |
| **Revenue potential** | At 100 sales/mo × $49.99 × 70% = **$3,500/mo**. Top SDK tools do 500+ sales/mo. |
| **Action** | Create two listings: (1) Free core SDK, (2) Paid Pro Pack |
| **Verdict** | **FREE core + $49.99 Pro Pack** |

**Pricing justification for $49.99:**
- Odin Inspector: $55 (serialization only)
- Photon PUN2: Free core, paid cloud
- Mirror Networking: Free (no comparable premium)
- DOTween Pro: $15 (animation only)
- NaughtyAttributes: $15 (editor only)
- IntelliVerseX at $49.99 includes 33 live-ops systems, 7 AI subsystems, 16 demo UIs — dramatically more value per dollar than any comparable asset.

---

#### 20. Unreal Marketplace

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free to list |
| **Revenue share** | **88/12** (publisher keeps 88%, Epic takes 12%) |
| **Reach** | Large — primary discovery for Unreal devs |
| **Price recommendation** | **FREE** (until Unreal parity is achieved) |
| **What to list** | Unreal SDK plugin (Nakama wrapper + Hiro RPC) |
| **Timing** | List as free now. Convert to freemium when native C++ Hiro client and Blueprints exist (Q3 2026 target). |
| **Revenue potential (future)** | At $59.99 × 50 sales/mo × 88% = **$2,640/mo** (when parity exists) |
| **Action** | List free now to build reviews/ratings. Switch to paid tier when feature-complete. |
| **Verdict** | **FREE now → $59.99 after Unreal parity (Q3 2026)** |

---

#### 21. Fab (Epic's new unified marketplace)

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free to list |
| **Revenue share** | **88/12** (same as Epic Store) |
| **Reach** | Unreal + Fortnite + MetaHuman ecosystem |
| **Price recommendation** | **Same as Unreal Marketplace** — cross-list |
| **Action** | Submit to Fab when Unreal plugin is feature-complete |
| **Verdict** | **FREE now → paid later** |

---

#### 22. itch.io

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Revenue share** | **Creator chooses** (default 10% to itch.io, you keep 90%) |
| **Reach** | 30M monthly visitors, strong indie community |
| **Price recommendation** | **"Pay what you want"** with $0 minimum |
| **What to list** | Per-engine bundles (Unity, Unreal, Godot, JS, etc.) + all-in-one bundle |
| **Strategic value** | Indie community visibility. Pay-what-you-want captures voluntary revenue. Game jam exposure. |
| **Revenue potential** | Modest — $200-$1,000/mo from voluntary payments. High awareness value. |
| **Action** | Create page, upload engine bundles, set PWYW with suggested $10 |
| **Verdict** | **PAY WHAT YOU WANT ($0 minimum, $10 suggested)** |

---

#### 23. Gumroad / Polar / Lemon Squeezy

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free to list |
| **Revenue share** | Gumroad: 10%, Polar: 5%, Lemon Squeezy: 5% + Stripe |
| **Reach** | Digital product buyers, developers |
| **Price recommendation** | **$0+ (pay what you want)** for SDK bundles; **$29-$99** for premium content packs |
| **What to sell** | Premium GDD template pack ($29), Economy simulation toolkit ($49), Complete starter kit with all demos ($99) |
| **Strategic value** | Direct sales channel with no marketplace dependency. Email capture. |
| **Revenue potential** | $500-$3,000/mo depending on audience size |
| **Action** | Create Polar storefront (developer-focused, GitHub-integrated, lowest fees at 5%) |
| **Verdict** | **FREEMIUM — free SDK + paid premium packs** |

---

#### 24. AWS Marketplace

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free to list |
| **Revenue share** | **3%** of TCV (lowest of any marketplace) |
| **Reach** | Enterprise buyers with AWS budgets |
| **Price recommendation** | **$499/mo** for managed IntelliVerseX backend (Nakama + Hiro + Satori on ECS/EKS) |
| **What to list** | SaaS: Managed IntelliVerseX backend, pre-configured, auto-scaling, with dashboard |
| **Strategic value** | Very high for enterprise. AWS customers can use committed spend / credits. Procurement teams prefer marketplace purchases. |
| **Revenue potential** | 10 enterprise customers × $499/mo = **$4,990/mo** (minus 3% = $4,840) |
| **Timing** | Requires managed backend product to be built first. Target Q4 2026 or Q1 2027. |
| **Action** | Build managed backend offering → list on AWS Marketplace |
| **Verdict** | **$499/mo SaaS — high-value enterprise channel (future)** |

---

#### 25. GCP Marketplace

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free to list |
| **Revenue share** | **3%** (same as AWS) |
| **Reach** | GCP enterprise buyers |
| **Price recommendation** | **Same as AWS** — cross-list |
| **Action** | List alongside AWS Marketplace launch |
| **Verdict** | **$499/mo SaaS (future, cross-list with AWS)** |

---

#### 26. Azure Marketplace

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free to list |
| **Revenue share** | **3%** for SaaS |
| **Reach** | Microsoft enterprise ecosystem |
| **Price recommendation** | **Same as AWS/GCP** |
| **Action** | Third cloud marketplace to target |
| **Verdict** | **$499/mo SaaS (future)** |

---

### Category D: Content & Community Platforms

---

#### 27. Udemy / Skillshare

| Attribute | Detail |
|-----------|--------|
| **Cost to publish** | Free |
| **Revenue share** | Udemy: 37% (organic), 97% (instructor coupon); Skillshare: minutes-based |
| **Price recommendation** | **$49.99-$99.99** per course |
| **What to sell** | "Complete Game Development with IntelliVerseX" course (10+ hours) |
| **Revenue potential** | $1,000-$10,000/mo for well-marketed courses |
| **Action** | Create comprehensive video course covering SDK setup through live ops |
| **Verdict** | **$49.99-$99.99 per course — high effort, high long-term return** |

---

#### 28. Game Jam Partnerships

| Attribute | Detail |
|-----------|--------|
| **Cost** | Sponsor fee ($500-$5,000 per jam) or free tool sponsorship |
| **Price to charge** | **Free** (provided as tool for jam participants) |
| **Reach** | Targeted — game developers actively building |
| **Strategic value** | High conversion potential. Developers who use IVX in a jam adopt it for real projects. |
| **Target jams** | Ludum Dare, Global Game Jam, GMTK Game Jam, itch.io jams |
| **Action** | Create "IVX Jam Starter Kit" (pre-configured project, 5-min setup, cheat sheet) |
| **Verdict** | **FREE — sponsor or provide as tool. Track conversions.** |

---

#### 29. Academic / Student Licenses

| Attribute | Detail |
|-----------|--------|
| **Cost** | Free to distribute |
| **Price to charge** | **Free** for .edu emails |
| **Reach** | Game development programs at universities |
| **Strategic value** | Long-term adoption pipeline. Students become professional developers who recommend IVX. |
| **Action** | Create academic program page. Partner with 5-10 game dev university programs. |
| **Verdict** | **FREE — long-term investment** |

---

### Category E: Enterprise & Conference Channels

---

#### 30. GDC / Unite / Unreal Fest / XR conferences

| Attribute | Detail |
|-----------|--------|
| **Cost** | $5,000-$50,000 (booth/talk depending on conference) |
| **Price to charge** | N/A (awareness + enterprise leads) |
| **Reach** | Industry professionals, studio decision-makers |
| **What to present** | "AI-Native Game Development: 22 Skills From GDD to Live Ops" |
| **Strategic value** | Enterprise customer acquisition. Partnership deals. Press coverage. |
| **ROI** | 1 enterprise deal ($499/mo × 12 = $6K/yr) covers a conference talk. |
| **Timing** | GDC 2027 submission deadline: ~September 2026. Unite 2026: TBD. |
| **Action** | Submit talk proposals for GDC 2027 and Unite 2026. Start with virtual talks (free). |
| **Verdict** | **INVESTMENT — submit talks first (free), booth later when revenue supports it** |

---

#### 31. Heroic Labs Partner Program

| Attribute | Detail |
|-----------|--------|
| **Cost** | Free to apply |
| **Price to charge** | N/A (co-marketing) |
| **Reach** | All Nakama/Hiro/Satori users |
| **Strategic value** | Very high — official partner status = trust signal. Co-marketing reaches their customer base. |
| **Action** | Apply for integration partner status. Offer to be featured case study. |
| **Verdict** | **FREE — apply immediately** |

---

## 4. Revenue Projections

### Year 1 (Conservative)

| Revenue Stream | Monthly | Annual | Confidence |
|---------------|--------:|-------:|:----------:|
| GitHub Sponsors | $1,500 | $18,000 | Medium |
| Unity Asset Store (Pro Pack, $49.99) | $3,500 | $42,000 | Medium |
| Gumroad/Polar premium packs | $1,000 | $12,000 | Low-Medium |
| itch.io voluntary payments | $300 | $3,600 | Low |
| Managed backend (Indie tier) | $500 | $6,000 | Low |
| Dev.to / Medium articles | $400 | $4,800 | Low |
| **Year 1 Total** | **$7,200** | **$86,400** | |

### Year 2 (Growth)

| Revenue Stream | Monthly | Annual | Confidence |
|---------------|--------:|-------:|:----------:|
| GitHub Sponsors | $3,000 | $36,000 | Medium |
| Unity Asset Store | $7,000 | $84,000 | Medium |
| Unreal Marketplace ($59.99) | $2,640 | $31,680 | Medium |
| Managed backend (Studio tier × 20) | $1,980 | $23,760 | Medium |
| Managed backend (Enterprise × 5) | $2,495 | $29,940 | Low-Medium |
| AWS/GCP/Azure Marketplace | $2,000 | $24,000 | Low |
| Udemy courses | $3,000 | $36,000 | Low-Medium |
| Gumroad/Polar | $2,000 | $24,000 | Medium |
| itch.io | $500 | $6,000 | Low |
| **Year 2 Total** | **$24,615** | **$295,380** | |

### Year 3 (Scale)

| Revenue Stream | Monthly | Annual | Confidence |
|---------------|--------:|-------:|:----------:|
| Managed backend (all tiers) | $25,000 | $300,000 | Medium |
| Asset Store + Unreal Marketplace | $15,000 | $180,000 | Medium |
| Enterprise / OEM deals | $10,000 | $120,000 | Low |
| Cloud Marketplaces | $8,000 | $96,000 | Low-Medium |
| Courses + content | $5,000 | $60,000 | Medium |
| Sponsorships + community | $5,000 | $60,000 | Medium |
| **Year 3 Total** | **$68,000** | **$816,000** | |

---

## 5. Competitor Pricing Comparison

### Backend-as-a-Service Pricing

| Provider | Free Tier | Indie | Studio | Enterprise | Self-Host |
|----------|-----------|-------|--------|-----------|-----------|
| **PlayFab** | 100K MAU | $99/mo | $299/mo | $5K-$50K/mo | No |
| **AccelByte** | PCCU-based free | Contact | Contact | $10K+/mo | Yes (paid) |
| **Beamable** | Free tier | Contact | Contact | Contact | No |
| **LootLocker** | 30-day trial | Contact | Contact | Contact | No |
| **Nakama (Heroic Labs)** | Unlimited (self-host) | $99/mo cloud | $499/mo | Custom | Yes (free) |
| **IntelliVerseX (proposed)** | Unlimited (self-host) | $0 + usage | $99/mo | $499/mo | Yes (free) |

### Key Insight

IntelliVerseX is **cheaper than every competitor** at every tier because:
1. Self-hosted is truly free (Nakama is open source)
2. The managed service competes on price with Nakama Cloud
3. The SDK + skills + tools are entirely free — competitors charge for similar tooling
4. No per-MAU fees at the free/indie tier

**This is the right strategy.** Win on price at the bottom, win on features at the top.

### Asset Store Price Comparisons

| Asset | Price | What You Get |
|-------|------:|-------------|
| Odin Inspector | $55 | Serialization + editor tools |
| Rewired | $45 | Input management |
| Easy Save | $25 | Save/load system |
| Dialogue System for Unity | $85 | Dialog trees only |
| Master Audio | $55 | Audio management |
| **IntelliVerseX Pro Pack** | **$49.99** | 33 live-ops systems, 7 AI subsystems, 16 demo UIs, multiplayer templates, 11 engine support |

At $49.99 the Pro Pack is **the best value per feature** on the entire Asset Store.

---

## 6. What Goes Free vs. Paid

### Free Everywhere (Community Tier)

| Component | Why Free |
|-----------|---------|
| Core SDK (all 11 engines) | Adoption. Must match Nakama client libraries (already free). |
| All 22 AI agent skills | Distribution moat. Skills embedded in workflows = long-term lock-in. |
| Asset pipeline tools (Python CLI) | Developer experience. Low cost to provide. Drives trust. |
| CI/CD workflow templates | Standard practice for open-source tools. |
| Documentation (MkDocs site) | Table stakes. |
| Self-hosted Nakama integration | Nakama is already free. Can't charge for what's free upstream. |
| Community Discord support | Community management cost is low. |
| Basic dashboard (web) | Teaser for managed service. |

### Paid (Indie / Studio / Enterprise)

| Component | Tier | Why Paid |
|-----------|------|---------|
| Managed Nakama hosting | Indie+ | Real infrastructure cost. DevOps expertise. |
| AI call quotas (above free tier) | Indie+ | Real per-call cost (OpenAI/Azure). |
| Analytics dashboard (visual) | Studio+ | Build + maintenance cost. Differentiated value. |
| Remote config UI | Studio+ | Build cost. Enterprise feature. |
| Priority email support | Studio+ | Human time cost. |
| SLA (99.9% uptime) | Enterprise | Operational commitment. |
| Dedicated Slack channel | Enterprise | Human time cost. |
| Compliance reports (GDPR/COPPA) | Enterprise | Legal and audit cost. |
| White-label / OEM rights | Enterprise | Strategic value. Exclusive benefit. |
| Custom integrations | Enterprise | Engineering time cost. |

### Premium Content Packs (one-time purchase)

| Pack | Price | Contents | Channel |
|------|------:|----------|---------|
| **GDD Template Pack** | $29 | 15 pre-filled GDD templates, economy model spreadsheets, pitch deck template | Gumroad/Polar |
| **Demo UI Pack** | $49.99 | 16 production-ready demo UIs, prefabs, animations | Unity Asset Store |
| **Economy Toolkit** | $49 | Monte Carlo simulator scripts, reward curve spreadsheets, pricing A/B templates | Gumroad/Polar |
| **Complete Starter Kit** | $99 | All packs + pre-configured project for Unity/Unreal/Godot | Gumroad/Polar |
| **Live Ops Quickstart** | $39 | Pre-configured daily rewards, streaks, spin wheel, season pass Hiro configs | Gumroad/Polar |

---

## 7. Licensing Model

### Recommended License Structure

| Component | License | Reason |
|-----------|---------|--------|
| Core SDK (all engines) | **MIT** | Maximum adoption. No friction. Industry standard. |
| AI Agent Skills (.cursor/skills/) | **MIT** | Must be freely embeddable in any workflow. |
| Asset Pipeline Tools | **MIT** | Encourage use across any project. |
| CI/CD Templates | **MIT** | Standard for workflow templates. |
| Premium Content Packs | **Commercial** (per-seat or per-studio) | Revenue generation. Not open source. |
| Managed Service | **SaaS Terms of Service** | Standard for hosted services. |
| Documentation Content | **CC BY 4.0** | Allows sharing with attribution. |

### License Compatibility

MIT is compatible with virtually every game engine license:
- Unity: Proprietary (MIT is fine as a dependency)
- Unreal: Custom (MIT is compatible)
- Godot: MIT (perfect match)
- Roblox: Proprietary (MIT is fine)
- All others: MIT is the most permissive option

### What NOT to Do

- Do NOT use AGPL or GPL — this scares game studios who fear license contamination
- Do NOT use BSL (Business Source License) — confusing, deters contributors
- Do NOT use SSPL — designed for databases, inappropriate for SDKs
- Do NOT require CLA (Contributor License Agreement) for now — reduces contributions
- Do NOT add "Commons Clause" — creates legal ambiguity

---

## 8. 4-Tier Launch Plan — Platform-by-Platform Checklist

---

### TIER 1: Week 1-2 — Foundation & AI Tool Registries

**Goal:** Establish presence on every free platform. Zero cost. Maximum surface area.

**Cost: $0 | Expected monthly revenue after Tier 1: $500-$1,500**

---

#### 1.1 GitHub Repository (Day 1)

- [ ] **Verify README** is up to date with all 22 skills, wiring instructions, MCP config
- [ ] **Add GitHub Topics:** `game-sdk`, `ai-skills`, `nakama`, `hiro`, `satori`, `game-development`, `unity`, `unreal`, `godot`, `roblox`, `live-ops`, `mcp`, `cursor-skills`
- [ ] **Enable GitHub Discussions** for community Q&A
- [ ] **Pin an Issue:** "Getting Started — Which skill should I use first?"
- [ ] **Add social preview image** (1280×640, shows lifecycle diagram)
- [ ] **Verify LICENSE** (MIT) is at repo root

**Platform details:**
- URL: `github.com/Intelli-verse-X/Intelli-verse-X-SDK`
- Submission process: Already live — optimize metadata
- Approval time: Instant
- Listing format: README.md is the landing page
- Requirements: Public repository, LICENSE file

---

#### 1.2 GitHub Sponsors (Day 1)

- [ ] **Apply** at `github.com/sponsors` (requires payout method)
- [ ] **Create 4 tiers:**

| Tier | Price | Benefits |
|------|------:|---------|
| Supporter | $5/mo | Name in SPONSORS.md, sponsor badge |
| Builder | $15/mo | Priority GitHub issues, monthly newsletter |
| Studio | $50/mo | Monthly 30-min office hours call, logo in README |
| Partner | $100/mo | Logo in README + docs site, priority feature requests, quarterly roadmap input |

- [ ] **Write sponsor profile** with pitch, project stats, roadmap
- [ ] **Add funding.yml** to `.github/FUNDING.yml`:
  ```yaml
  github: [Intelli-verse-X]
  ```

**Platform details:**
- URL: `github.com/sponsors/Intelli-verse-X`
- Submission process: Apply → bank/Stripe verification → 1-3 day approval
- Revenue share: **0%** (GitHub takes nothing)
- First-year matching: GitHub may match up to $1,000
- Payout: Monthly via Stripe

---

#### 1.3 Cursor Skills Registry (Day 1)

- [ ] **Verify all 22 SKILL.md** files have correct YAML frontmatter:
  - `name`, `description`, `version`, `author`, `allowed-tools`
- [ ] **Test each skill** in a fresh Cursor workspace:
  - Clone repo → open in Cursor → ask trigger phrase → verify skill activates
- [ ] **Submit to Cursor Skills Directory** (if registry exists) or ensure discoverability via GitHub
- [ ] **Create a `.cursor/skills/README.md`** index file listing all 22 skills

**Platform details:**
- URL: Cursor app → Settings → Skills
- Submission process: Skills auto-detect from `.cursor/skills/` in workspace
- Approval time: Instant (no review needed — file-based)
- Listing format: Each `SKILL.md` with YAML frontmatter
- Requirements: Valid YAML frontmatter, `allowed-tools` array
- Reach: All Cursor Pro ($20/mo), Pro+ ($60/mo), Ultra ($200/mo) users
- Revenue: Free (skills are distribution, not revenue)

---

#### 1.4 Claude Code Plugin Marketplace (Day 1)

- [ ] **Package skills as Claude Code plugin:**
  - Create `plugin.json` manifest with all 22 skills
  - Map each skill to intent triggers
- [ ] **Create CLAUDE.md** in repo root (already documented in README wiring guide)
- [ ] **Submit to marketplace:**
  ```bash
  /plugin marketplace add https://github.com/Intelli-verse-X/Intelli-verse-X-SDK
  ```
- [ ] **Test** with Claude Code CLI: trigger each skill via natural language

**Platform details:**
- URL: `claudecodeplugins.io` / CLI marketplace
- Submission process: Submit GitHub URL → automated review → publish
- Approval time: Usually < 24 hours
- Listing format: Plugin manifest + SKILL.md files
- Current marketplace size: 2,787 skills across 416 plugins
- Requirements: Public GitHub repo, plugin manifest
- Revenue: Free (no paid plugins yet)
- Namespace: `@intelliversex`

---

#### 1.5 Windsurf / Codeium (Day 1)

- [ ] **Verify SKILL.md compatibility** (same format as Cursor — should work without changes)
- [ ] **Test in Windsurf:** open project → use Cascade → trigger skill phrases
- [ ] **Submit to Windsurf extension registry** (if available) or document in README

**Platform details:**
- URL: Windsurf app → Extensions / Skills
- Submission process: Same `.cursor/skills/` format auto-detected
- Approval time: Instant (file-based detection)
- Requirements: Same as Cursor
- Revenue: Free

---

#### 1.6 SkillsGate (Day 1-2)

- [ ] **Register** at SkillsGate platform
- [ ] **Submit all 22 skills** as `@intelliversex/*` namespace:
  ```bash
  skillsgate add @intelliversex/ivx-sdk-setup @intelliversex/ivx-live-ops @intelliversex/ivx-monetization ...
  ```
- [ ] **Add badges** to README for SkillsGate listing

**Platform details:**
- URL: `skillsgate.dev` (or similar)
- Submission process: CLI submission + metadata
- Approval time: Varies
- Listing format: SKILL.md per skill
- Reach: 18+ AI agent tools
- Revenue: Free

---

#### 1.7 Smithery MCP Registry (Day 2)

- [ ] **Package the IntelliVerseX MCP server** for Smithery
- [ ] **Publish:**
  ```bash
  smithery mcp publish
  ```
  Or submit via `smithery.ai/new`
- [ ] **Write listing description** emphasizing 50+ tools: Nakama, Hiro, Satori, player ops, analytics
- [ ] **Add Smithery badge** to README

**Platform details:**
- URL: `smithery.ai`
- Submission process: CLI `smithery mcp publish` or web form
- Approval time: Instant (self-serve)
- Listing format: MCP server descriptor + README
- Current platform size: 4,700+ MCP servers, 125,000+ skills
- Requirements: Working MCP server endpoint
- Revenue: Free to publish
- Analytics: Built-in usage analytics

---

#### 1.8 npm — JavaScript SDK (Day 3)

- [ ] **Prepare package:** `@intelliversex/sdk`
  - Verify `package.json` in `SDKs/javascript/`
  - Add `README.md` with quick start
  - Add `keywords` for npm search
- [ ] **Publish:**
  ```bash
  cd SDKs/javascript && npm publish --access public
  ```
- [ ] **Publish MCP package:** `@intelliversex/mcp-server`
- [ ] **Verify** install works: `npm install @intelliversex/sdk`

**Platform details:**
- URL: `npmjs.com/package/@intelliversex/sdk`
- Submission process: `npm publish` (requires npm account + 2FA)
- Approval time: Instant
- Requirements: Valid `package.json`, scoped package name, npm account
- Reach: 17M+ npm developers
- Revenue: Free

---

#### 1.9 PyPI — Python Tools (Day 3)

- [ ] **Create `setup.py` / `pyproject.toml`** for `tools/asset-pipeline/`
  - Package name: `intelliversex-tools`
  - Include: `scaffold_character`, `generate_spritesheet`, `validate_specs`, `validate_character`, `validate_sound_manifest`
  - Dependencies: `Pillow`, `jsonschema`
- [ ] **Build and publish:**
  ```bash
  python -m build && twine upload dist/*
  ```
- [ ] **Verify** install: `pip install intelliversex-tools`

**Platform details:**
- URL: `pypi.org/project/intelliversex-tools`
- Submission process: `twine upload` (requires PyPI account + API token)
- Approval time: Instant
- Requirements: `pyproject.toml`, README, license classifier
- Reach: Python developers across all game engines
- Revenue: Free

---

#### 1.10 Awesome Lists (Day 2-3)

- [ ] **Submit PR** to each list with link, description, and category:

| List | Category | PR Description |
|------|----------|---------------|
| `awesome-gamedev` | Tools / SDKs | "AI-native 11-engine game SDK with 22 agent skills" |
| `awesome-unity` | Frameworks | "Complete game SDK: live-ops, AI, monetization, analytics" |
| `awesome-nakama` | Integrations | "Full Hiro + Satori integration with 22 AI skills" |
| `awesome-godot` | Addons | "Cross-platform game SDK with Nakama backend" |
| `awesome-mcp-servers` | Game Development | "50+ MCP tools for Nakama, Hiro, and Satori" |
| `awesome-ai-tools` | Developer Tools | "22 AI agent skills for game development lifecycle" |

- [ ] **Follow each repo's CONTRIBUTING.md** format

**Platform details:**
- Submission: GitHub Pull Request
- Approval time: Days to weeks (maintainer review)
- Requirements: Varies per list — usually: project must be active, have README, be useful
- Revenue: Free (SEO + backlinks)

---

#### 1.11 Polar Storefront (Day 5)

- [ ] **Create account** at `polar.sh`
- [ ] **Connect GitHub** for automatic project linking
- [ ] **Create premium packs:**

| Product | Price | Contents |
|---------|------:|---------|
| GDD Template Pack | $29 | 15 templates, economy spreadsheets, pitch deck |
| Economy Toolkit | $49 | Monte Carlo scripts, reward curves, pricing templates |
| Complete Starter Kit | $99 | All packs + pre-configured projects |
| Live Ops Quickstart | $39 | Hiro configs for daily rewards, streaks, spin wheel, season pass |

- [ ] **Enable license key** delivery for premium packs
- [ ] **Add Polar badge** to README

**Platform details:**
- URL: `polar.sh/intelliversex`
- Submission: Self-serve storefront creation
- Revenue share: **5%** (lowest of any platform)
- Payment: Stripe-based, global
- Features: GitHub-integrated, license keys, subscriptions, digital products

---

#### 1.12 itch.io (Day 7)

- [ ] **Create tool page** at `itch.io`
- [ ] **Upload per-engine bundles:** Unity, Unreal, Godot, JS, All-in-One
- [ ] **Set pricing:** Pay What You Want, $0 minimum, **$10 suggested**
- [ ] **Write page description** with screenshots, feature list, quick start
- [ ] **Add tags:** game-development, sdk, ai, unity, godot, unreal, tools, backend
- [ ] **Enable "Tools" category** (not "Games")

**Platform details:**
- URL: `itch.io/intelliversex`
- Submission: Self-serve page creation, instant publish
- Revenue share: **Creator chooses** (default 10% to itch.io → you keep 90%)
- Reach: 30M monthly visitors, strong indie community
- Payment: PayPal + Stripe

---

#### 1.13 Heroic Labs Partner Program (Day 7)

- [ ] **Email** Heroic Labs partnership team
- [ ] **Pitch:** "IntelliVerseX adds 22 AI agent skills and 33 Hiro live-ops system wrappers on top of Nakama. We want to be an official integration partner."
- [ ] **Offer:** featured case study, co-marketing blog post, joint webinar

**Platform details:**
- URL: `heroiclabs.com/partners`
- Submission: Email / contact form
- Approval time: 1-4 weeks
- Requirements: Active Nakama integration, production usage
- Revenue: Free (co-marketing value)

---

#### 1.14 pub.dev — Flutter/Dart SDK (Day 3)

- [ ] **Publish** `intelliversex_sdk` Dart package from `SDKs/flutter/`
- [ ] **Verify** `dart pub publish --dry-run` passes

**Platform details:**
- URL: `pub.dev/packages/intelliversex_sdk`
- Submission: `dart pub publish`
- Approval time: Instant (automated analysis + scoring)
- Requirements: `pubspec.yaml`, README, CHANGELOG, LICENSE, passing `dart analyze`

---

#### 1.15 Maven Central — Java/Android SDK (Day 3)

- [ ] **Publish** `com.intelliversex:sdk` from `SDKs/java/`
- [ ] **Sign with GPG** (Maven Central requirement)

**Platform details:**
- URL: `search.maven.org` → `com.intelliversex:sdk`
- Submission: Gradle `publishToMavenCentral` or Sonatype OSSRH
- Approval time: First publish requires manual review (1-3 days), subsequent instant
- Requirements: GPG signing, Javadoc, sources JAR, POM metadata

---

**Tier 1 Completion Checklist:**

- [ ] All 15 platforms above are live
- [ ] All skills verified working in Cursor, Claude Code, Windsurf
- [ ] npm, PyPI, pub.dev, Maven packages published
- [ ] MCP server on Smithery
- [ ] Polar storefront with 4 premium packs
- [ ] itch.io page with PWYW bundles
- [ ] GitHub Sponsors active with 4 tiers
- [ ] Awesome list PRs submitted (6+)
- [ ] Heroic Labs partner application sent

---

### TIER 2: Week 3-4 — Public Launch

**Goal:** High-visibility launch across developer communities. Capture emails. Generate first revenue.

**Cost: $0 | Expected monthly revenue after Tier 2: $2,000-$5,000**

---

#### 2.1 Product Hunt Launch (Week 3 — Tuesday)

**Pre-launch (start Week 1):**
- [ ] **Build waitlist** of 400+ supporters (email, social, Discord)
- [ ] **Prepare assets:**
  - [ ] Thumbnail (240×240) — distinctive, not generic
  - [ ] Gallery: 5-6 screenshots/GIFs showing lifecycle (Design → Build → Ship → Grow)
  - [ ] Demo video: 60-90 seconds, showing "describe game → get assets + infrastructure"
  - [ ] Maker video: 30s authentic story (why you built this)
- [ ] **Write listing:**
  - Tagline: "22 AI Skills for Game Development — GDD to Live Ops, Any Engine"
  - Description: Problem → Solution → How it works → Key features (22 skills, 11 engines, free + open source)
- [ ] **Recruit hunter** (optional but recommended — established PH community member)
- [ ] **Draft maker comment** (first comment after launch — your authentic story)

**Launch day (Tuesday 12:01 AM PT):**
- [ ] **Publish listing** at exactly 12:01 AM Pacific Time
- [ ] **Post maker comment** immediately
- [ ] **Notify waitlist** via email
- [ ] **Share on social** (Twitter/X, LinkedIn, Discord)
- [ ] **Respond to every comment** within 30 minutes
- [ ] **Monitor all day** — engage, thank, answer questions
- [ ] **DO NOT ask for upvotes** (violates TOS, triggers penalties)

**Platform details:**
- URL: `producthunt.com/posts/intelliversex`
- Submission: `producthunt.com/posts/new`
- Approval time: Instant (self-serve posting)
- Expected results: 10,000+ visitors, 1,000-3,000 email signups
- Best days: Tuesday-Thursday (maximum traffic)
- Requirements: Product page, assets, maker account
- Revenue: Indirect (awareness → signups → conversions)

---

#### 2.2 Hacker News — Show HN (Week 3, day after PH)

- [ ] **Write post title:** "Show HN: IntelliVerseX — Open-source game SDK with 22 AI agent skills (GDD to Live Ops)"
- [ ] **Write top comment** with technical details, honest status (what's shipped vs planned), links
- [ ] **Post on weekday morning** (US East Coast time for max visibility)
- [ ] **Respond to every comment** — HN rewards detailed, honest answers
- [ ] **Be transparent** about stubs/planned features — HN detects hype instantly

**Platform details:**
- URL: `news.ycombinator.com/show`
- Submission: Submit link with "Show HN:" prefix
- Approval time: Instant (self-serve), but reaching front page requires upvotes
- Expected results: 5,000-50,000 visitors if front page
- Reach: VCs, CTOs, senior engineers — high-quality audience
- Requirements: Authentic, technical, honest
- Revenue: Indirect (VC visibility + developer signups)

---

#### 2.3 Reddit Posts (Week 3)

- [ ] **Write unique posts** for each subreddit (not cross-posts):

| Subreddit | Members | Angle | Post Type |
|-----------|--------:|-------|-----------|
| r/gamedev | 2.2M | "I built 22 AI skills that automate game development from GDD to live ops" | Show-off Saturday / self-promo thread |
| r/unity3d | 350K | "Open-source SDK with 33 Hiro live-ops systems, 7 AI subsystems, free" | Tutorial / tool announcement |
| r/unrealengine | 450K | "11-engine game SDK — what Unreal features would you prioritize?" | Discussion + feedback request |
| r/indiedev | 400K | "Solo dev? Here's how I generate game assets + wire backend in 30 minutes" | Tutorial / workflow |
| r/godot | 200K | "Cross-platform SDK with Nakama backend for Godot — free, MIT licensed" | Tool announcement |

- [ ] **Provide value first** — each post should teach something, not just promote
- [ ] **Respond to all comments** within 2 hours
- [ ] **Respect subreddit rules** — check self-promotion policies for each

**Platform details:**
- Submission: Direct post (respect each sub's rules)
- Approval time: Varies (some subs require mod approval)
- Requirements: Account karma threshold varies, follow community guidelines
- Revenue: Indirect

---

#### 2.4 Dev.to Launch Article (Week 2-3)

- [ ] **Write launch article:** "How I Built 22 AI Agent Skills for Game Development"
  - Hook: personal story
  - Problem: game dev requires 10+ fragmented tools
  - Solution: one SDK, 22 skills, any engine
  - Demo: GDD → Assets → Backend → Ship (with screenshots)
  - CTA: GitHub repo + Discord
- [ ] **Add tags:** `gamedev`, `ai`, `opensource`, `sdk`
- [ ] **Cross-post** to Hashnode and Medium (canonical URL to Dev.to)
- [ ] **Plan 5 more articles** (weekly):
  1. "AI NPC Dialog in Unity with 20 Lines of Code"
  2. "From Idea to Daily Rewards in 10 Minutes"
  3. "Economy Simulation: Monte Carlo for Game Designers"
  4. "Ship to 11 Engines from One Codebase"
  5. "22 AI Skills That Know How to Build Your Game"

**Platform details:**
- URL: `dev.to/intelliversex`
- Submission: Self-serve publishing
- Revenue: Medium Partner Program pays $150-$450/article for good performance
- SEO value: High (Dev.to ranks well on Google)

---

#### 2.5 Discord Community Activation (Week 3)

- [ ] **IntelliVerseX Discord** (`discord.gg/YVPxPFftMQ`):
  - [ ] Create channels: `#welcome`, `#showcase`, `#sdk-support`, `#skills-help`, `#feature-requests`, `#announcements`
  - [ ] Post launch announcement
  - [ ] Pin getting-started guide
- [ ] **Post in external gamedev Discords** (where permitted):
  - Brackeys, Game Dev League, Godot official, Unity official, Nakama official
  - Always follow server rules on self-promotion

**Platform details:**
- Submission: Server setup (already exists), external server posts
- Revenue: Indirect (community retention, feedback loop)

---

#### 2.6 Unity Asset Store (Week 3)

- [ ] **Create publisher account** at `publisher.unity.com`
- [ ] **Prepare two listings:**

**Listing 1: IntelliVerseX SDK (FREE)**
- [ ] Title: "IntelliVerseX SDK — Game Backend, Live-Ops, AI, Analytics (11 Engines)"
- [ ] Description: Full feature list, quick start, links to docs
- [ ] Key images: 5 screenshots (setup wizard, demo UIs, architecture diagram, skill list, platform matrix)
- [ ] Category: Tools / Integration
- [ ] Keywords: backend, nakama, live-ops, analytics, monetization, multiplayer, ai, cross-platform
- [ ] Minimum Unity version: 2023.3

**Listing 2: IntelliVerseX Pro Pack ($49.99)**
- [ ] Title: "IntelliVerseX Pro — 16 Demo UIs, Hiro Systems, AI Templates, Multiplayer"
- [ ] Description: Everything in free + 16 production UIs, all Hiro prefabs, AI integration templates, sample scenes
- [ ] Key images: 5 screenshots of demo UIs in action
- [ ] Category: Complete Projects / Tools
- [ ] Price: $49.99

- [ ] **Submit both** for review

**Platform details:**
- URL: `assetstore.unity.com`
- Submission: Publisher portal → Create Package → Upload → Submit for review
- Approval time: **5-15 business days** (Unity reviews all submissions)
- Revenue share: **~70/30** (you keep ~70%)
- Requirements: Unity package, valid metadata, screenshots, description, support email
- SEO: Title and keywords are critical — 80% of Unity devs discover via Asset Store search
- Payments: Monthly, 30-day hold, via PayPal or bank transfer

---

**Tier 2 Completion Checklist:**

- [ ] Product Hunt launched with 400+ pre-launch supporters
- [ ] Hacker News Show HN posted and engaged
- [ ] Reddit posts in 5 subreddits
- [ ] Dev.to launch article published
- [ ] Discord community activated with channels
- [ ] Unity Asset Store free + paid listings submitted
- [ ] All comments/questions answered within 2 hours

---

### TIER 3: Month 2 — Content & Package Ecosystems

**Goal:** Build long-tail discovery through content and package manager listings.

**Cost: $0 (time investment) | Expected monthly revenue after Tier 3: $5,000-$8,000**

---

#### 3.1 YouTube Tutorial Series (Week 5-8)

- [ ] **Record 8 videos** (one per lifecycle skill area):

| # | Video Title | Length | Skill Covered |
|---|------------|--------|---------------|
| 1 | "IntelliVerseX in 5 Minutes — Setup to Running Game" | 5 min | ivx-sdk-setup |
| 2 | "AI NPC Dialog — Add a Talking Character in 20 Lines" | 10 min | ivx-ai-integration |
| 3 | "Daily Rewards, Streaks, Season Pass — Config-Driven Live Ops" | 12 min | ivx-live-ops |
| 4 | "Monetize with Ads + IAP + Offerwalls" | 10 min | ivx-monetization |
| 5 | "Generate Game Characters with AI (Sprites, Emotions, Skins)" | 15 min | ivx-character-factory |
| 6 | "Economy Design: Monte Carlo Simulation for Game Balance" | 12 min | ivx-economy-simulator |
| 7 | "Ship to 11 Engines from One Backend" | 8 min | ivx-cross-platform |
| 8 | "Full Lifecycle Demo: Idea → Assets → Backend → Store" | 20 min | All skills |

- [ ] **Optimize each video:** title, description with timestamps, tags, end screens, cards
- [ ] **Add links** to GitHub repo, Discord, docs in every description
- [ ] **Create playlists:** "Getting Started", "Advanced Features", "Full Tutorials"

**Platform details:**
- URL: `youtube.com/@intelliversex`
- Submission: Upload (instant publish)
- Revenue: YouTube Partner Program (1,000 subs + 4,000 watch hours → ad revenue)
- Expected revenue: $500-$5,000/mo at scale
- SEO value: Very high (YouTube is the #2 search engine)

---

#### 3.2 Dev.to Weekly Articles (Week 5-10)

- [ ] **Publish 6 articles** (1/week) as planned in Tier 2
- [ ] **Republish** to Hashnode and Medium with canonical URLs
- [ ] **Engage in comments** on each platform
- [ ] **Track** which articles drive the most GitHub stars and signups

---

#### 3.3 Unreal Marketplace (Week 8-10)

- [ ] **Create Unreal Marketplace publisher account**
- [ ] **Package Unreal SDK plugin** from `SDKs/unreal/`
- [ ] **Submit free listing:**
  - Title: "IntelliVerseX SDK — Nakama Backend, Live-Ops, Analytics for Unreal"
  - Blueprint support if available
  - C++ source included
- [ ] **Note:** Convert to paid ($59.99) when native Hiro client ships (Q3 2026)

**Platform details:**
- URL: `unrealengine.com/marketplace`
- Submission: Publisher portal → Upload → Submit
- Approval time: **1-4 weeks** (Epic reviews all submissions)
- Revenue share: **88/12** (you keep 88%)
- Requirements: Compiled plugin, documentation, example project, screenshots
- Note: Also submit to **Fab** (Epic's unified marketplace) when available

---

#### 3.4 Smithery MCP (Week 5 — if not done in Tier 1)

- [ ] **Verify MCP server is published** and discoverable
- [ ] **Add usage examples** to Smithery listing
- [ ] **Track analytics** via Smithery dashboard

---

#### 3.5 Udemy Course Production (Week 10-12)

- [ ] **Plan course:** "Complete Game Development with IntelliVerseX — Backend to Live Ops"
  - 10+ hours of content
  - Section per lifecycle phase (Design, Build, Ship, Grow)
  - Hands-on project: build a complete game using IVX
- [ ] **Record first 3 sections** to submit for Udemy review
- [ ] **Price:** $49.99-$99.99

**Platform details:**
- URL: `udemy.com`
- Submission: Instructor application → course upload → review (1-2 weeks)
- Revenue share: **37%** (Udemy organic) / **97%** (your coupon)
- Requirements: Video quality standards, 30+ min content minimum
- Revenue potential: $1,000-$10,000/mo for well-marketed courses

---

**Tier 3 Completion Checklist:**

- [ ] 8 YouTube videos published
- [ ] 6 Dev.to articles published
- [ ] Unreal Marketplace listing submitted
- [ ] Udemy course in production
- [ ] Smithery MCP live with analytics
- [ ] YouTube channel setup (branding, playlists, links)

---

### TIER 4: Month 3+ — Enterprise, Marketplaces & Conferences

**Goal:** Revenue-generating channels. Enterprise presence. Industry recognition.

**Cost: Server infrastructure + time | Expected monthly revenue after Tier 4: $10,000-$15,000**

---

#### 4.1 AWS Marketplace (Month 4)

- [ ] **Build managed backend offering:**
  - Nakama + Hiro + Satori on ECS/EKS
  - Auto-scaling, monitoring, backups
  - Dashboard for config management
- [ ] **Register as AWS Marketplace Seller** (`aws.amazon.com/marketplace/management`)
- [ ] **Create SaaS listing:**
  - Title: "IntelliVerseX — Managed Game Backend with Live-Ops & Analytics"
  - Pricing: $499/mo flat + usage above 500K MAU
  - Free trial: 14 days
- [ ] **Integrate AWS Marketplace Metering API** for usage tracking
- [ ] **Submit for review**

**Platform details:**
- URL: `aws.amazon.com/marketplace`
- Submission: Seller registration → product listing → AWS review (2-4 weeks)
- Revenue share: **3%** of TCV (you keep 97%)
- Requirements: SaaS product, metering API integration, support plan
- Reach: Enterprise buyers with AWS committed spend
- Revenue potential: $2,000-$10,000/mo from enterprise accounts

---

#### 4.2 GCP Marketplace (Month 4-5)

- [ ] **Cross-list** same managed backend on GCP Marketplace
- [ ] **Integrate GCP Marketplace API** for billing
- [ ] **Same pricing** as AWS

**Platform details:**
- URL: `console.cloud.google.com/marketplace`
- Revenue share: **3%**
- Submission: Similar to AWS — partner registration + listing review

---

#### 4.3 Azure Marketplace (Month 5-6)

- [ ] **Cross-list** on Azure Marketplace
- [ ] **Integrate Azure SaaS fulfillment API**

**Platform details:**
- URL: `azuremarketplace.microsoft.com`
- Revenue share: **3%**

---

#### 4.4 GDC 2027 Submission (Month 5 — September 2026 deadline)

- [ ] **Submit talk proposal:**
  - Title: "AI-Native Game Development: From GDD to Live Ops with 22 Agent Skills"
  - Track: Tools & Tech or Production & Leadership
  - Abstract: How AI skills cover the full game development lifecycle
- [ ] **Prepare backup:** Virtual talk at GDC Vault if not accepted for in-person
- [ ] **Budget for booth** only if revenue supports it ($10,000-$50,000)

**Platform details:**
- URL: `gdconf.com/call-for-submissions`
- Submission deadline: ~September 2026 for GDC 2027 (March)
- Approval time: 2-3 months
- Cost: Speaker pass free if talk accepted; booth $5,000-$50,000
- Reach: 28,000+ game industry professionals
- ROI: 1 enterprise deal covers the cost

---

#### 4.5 Unite Conference (Month 5-6)

- [ ] **Submit talk proposal** for Unite 2026/2027
- [ ] **Focus:** Unity-specific integration, demo of SDK + live-ops

**Platform details:**
- URL: `unity.com/events/unite`
- Similar submission process to GDC

---

#### 4.6 Academic Partnerships (Month 4+)

- [ ] **Create academic program page** on docs site
- [ ] **Offer free access** to all tiers for .edu email addresses
- [ ] **Contact 5-10 game development programs:**
  - DigiPen, USC Games, NYU Game Center, Abertay, Full Sail
- [ ] **Create "Jam Starter Kit"** — pre-configured project for game jams
  - [ ] Target: Ludum Dare, Global Game Jam, GMTK Game Jam

**Platform details:**
- Submission: Direct email to program directors
- Cost: Free (tool sponsorship)
- Revenue: Long-term pipeline (students → professional developers)

---

#### 4.7 Unreal Marketplace Paid Upgrade (Month 6 — when parity ships)

- [ ] **Update Unreal plugin** with native Hiro C++ client and Blueprints
- [ ] **Convert listing** from free to **$59.99**
- [ ] **Maintain free tier** (basic Nakama wrapper) alongside paid (full Hiro + AI)

**Platform details:**
- Revenue share: **88/12** (you keep 88%)
- Revenue potential: $2,640/mo at 50 sales/mo

---

**Tier 4 Completion Checklist:**

- [ ] Managed backend live and deployed
- [ ] AWS Marketplace listing approved and active
- [ ] GCP Marketplace cross-listed
- [ ] Azure Marketplace cross-listed
- [ ] GDC 2027 talk submitted
- [ ] Unite talk submitted
- [ ] 5+ academic partnerships initiated
- [ ] Game jam starter kit created
- [ ] Unreal paid listing (if parity achieved)

---

### Full Launch Timeline (Visual)

```
WEEK  1 ─── 2 ─── 3 ─── 4 ─── 5 ─── 6 ─── 7 ─── 8 ─── 9 ── 10 ── 11 ── 12 ── M4 ── M5 ── M6
  │         │     │           │                 │               │     │     │     │
  ├─ Cursor  ├─ PH ├─ Unity    ├─ YT starts      ├─ Unreal       ├─ AWS ├─ GDC  ├─ Azure
  ├─ Claude  ├─ HN │  Asset    ├─ Dev.to          │  Marketplace  │     │  talk  │
  ├─ Wind    ├─ Reddit Store   │  series          │               │     │       ├─ Academic
  ├─ npm     ├─ Discord        │                  ├─ Udemy start  ├─ GCP│       │
  ├─ PyPI    │                 │                  │               │     │       ├─ Unreal $
  ├─ pub.dev │                 │                  │               │     │       │
  ├─ Maven   │                 │                  │               │     │       │
  ├─ Smithery│                 │                  │               │     │       │
  ├─ itch.io │                 │                  │               │     │       │
  ├─ Polar   │                 │                  │               │     │       │
  ├─ Awesome │                 │                  │               │     │       │
  ├─ Sponsors│                 │                  │               │     │       │
  └─ Heroic  │                 │                  │               │     │       │
     Labs    │                 │                  │               │     │       │
             │                 │                  │               │     │       │
TIER 1 ──────┘  TIER 2 ───────┘   TIER 3 ────────┘  TIER 4 ─────┘─────┘───────┘
(15 platforms)  (6 launches)       (5 channels)      (7 enterprise channels)
$0 cost         $0 cost            $0 (time)         Server costs
$500-1.5K/mo    $2-5K/mo           $5-8K/mo          $10-15K/mo
```

---

### Quarterly Roadmap Through 2027

#### Q2 2026 (April — June): Foundation

| Month | Tier | Milestones | Revenue Target |
|-------|------|-----------|---------------:|
| April | 1 + 2 | All 15 free platforms live. PH + HN launched. Unity Asset Store submitted. | $2,000/mo |
| May | 2 + 3 | YouTube series started. Dev.to articles running. Reddit engaged. | $4,000/mo |
| June | 3 | Unreal Marketplace free listing. Udemy course in production. Smithery analytics tracking. | $6,000/mo |

#### Q3 2026 (July — September): Growth

| Month | Tier | Milestones | Revenue Target |
|-------|------|-----------|---------------:|
| July | 3 + 4 | 8 YouTube videos live. Udemy course submitted. Academic outreach started. | $8,000/mo |
| August | 4 | Managed backend beta. AWS Marketplace prep. GDC talk submitted. | $10,000/mo |
| September | 4 | AWS Marketplace live. GCP cross-listed. First enterprise leads. | $12,000/mo |

#### Q4 2026 (October — December): Enterprise

| Month | Tier | Milestones | Revenue Target |
|-------|------|-----------|---------------:|
| October | 4 | Azure cross-listed. Unreal paid listing (if parity). 3+ enterprise trials. | $15,000/mo |
| November | 4 | First enterprise conversion. Academic partnerships active. Game jam sponsor. | $18,000/mo |
| December | All | Year 1 wrap: 31 platforms, $86K+ annual revenue. Plan Year 2. | $20,000/mo |

#### Q1 2027 (January — March): Scale

| Month | Milestones | Revenue Target |
|-------|-----------|---------------:|
| January | GDC 2027 prep. Content-factory integration launch. 30+ skills. | $22,000/mo |
| February | Pre-GDC PR push. Enterprise pipeline building. | $25,000/mo |
| March | **GDC 2027**: Talk + demo. Enterprise deals closing. Managed backend scaling. | $30,000/mo |

#### Q2-Q4 2027: Platform

| Quarter | Milestones | Revenue Target |
|---------|-----------|---------------:|
| Q2 2027 | Community skill marketplace. Full Unreal + Godot parity. 50+ skills. | $40,000/mo |
| Q3 2027 | Managed cloud offering GA. White-label deals. 10,000 studios. | $55,000/mo |
| Q4 2027 | $864K annual run rate. Series A positioning. Industry standard status. | $72,000/mo |

---

## 9. Anti-Patterns to Avoid

### Pricing Anti-Patterns

| Anti-Pattern | Why It Fails | What to Do Instead |
|-------------|-------------|-------------------|
| **Charging for the SDK itself** | Nakama clients are free. PlayFab SDK is free. Nobody pays for client SDKs. | Charge for managed services + premium content |
| **Charging for skills** | Skills need zero-friction distribution to build the moat. A paid skill never gets embedded. | Free forever. Skills are marketing. |
| **Per-MAU pricing at low tiers** | Indie devs can't predict MAU. Creates anxiety and churn. | Flat tiers with generous included MAU |
| **Hiding features behind enterprise tier** | Creates resentment. Devs tweet about it negatively. | Free core covers 90% of needs. Paid = scale + support + compliance. |
| **Free trial with hard cutoff** | Devs abandon at trial end, never convert. | Free forever + usage-based growth |
| **Requiring credit card for free tier** | 50%+ drop-off at CC entry. | No CC for Community or Indie tier |
| **Discounting regularly** | Trains customers to wait for sales. Devalues product. | Fixed prices. Launch discount once. |
| **Complex pricing calculator** | Developers hate surprises. | Simple, predictable tiers. |

### Distribution Anti-Patterns

| Anti-Pattern | Why It Fails | What to Do Instead |
|-------------|-------------|-------------------|
| **Launching on all platforms simultaneously** | Spreads attention thin. No single platform gets enough momentum. | Staged launch over 6 months |
| **Ignoring Asset Store SEO** | Asset Store search is how 80% of Unity devs discover tools. | Optimize title, description, keywords, screenshots |
| **Skipping Product Hunt** | Missing a free 10K-visitor day | Launch properly with preparation |
| **Not engaging in comments** | Abandoned product perception | Respond to every PH/HN/Reddit comment within 30 min |
| **Marketing before product is solid** | Negative first impressions are permanent | Audit aspirational APIs before major launch |

---

## 10. Decision Matrix

### Summary: What to Price Where

| # | Platform | Price | Revenue Share You Keep | Priority | Timeline |
|---|----------|-------|:----------------------:|:--------:|:--------:|
| 1 | GitHub Repository | Free | N/A | Immediate | Done |
| 2 | GitHub Sponsors | $5-$100/mo tiers | **100%** | Immediate | Week 1 |
| 3 | Cursor Skills Registry | Free | N/A | Immediate | Week 1 |
| 4 | Claude Code Plugins | Free | N/A | Immediate | Week 1 |
| 5 | Windsurf | Free | N/A | Immediate | Week 1 |
| 6 | Smithery (MCP) | Free | N/A | Immediate | Week 1 |
| 7 | npm | Free | N/A | Immediate | Week 1 |
| 8 | PyPI | Free | N/A | Immediate | Week 1 |
| 9 | OpenUPM | Free | N/A | Active | Done |
| 10 | pub.dev | Free | N/A | Immediate | Week 1 |
| 11 | Maven Central | Free | N/A | Immediate | Week 1 |
| 12 | Awesome Lists | Free | N/A | Immediate | Week 1 |
| 13 | Product Hunt | Free (awareness) | N/A | High | Week 3 |
| 14 | Hacker News | Free (awareness) | N/A | High | Week 3 |
| 15 | Dev.to / Medium | Free (content) | N/A | High | Week 2+ |
| 16 | YouTube | Free (content) | Ad revenue | Medium | Month 2 |
| 17 | Discord | Free (community) | N/A | Active | Done |
| 18 | Reddit | Free (awareness) | N/A | High | Week 3 |
| 19 | **Unity Asset Store** | Free core + **$49.99** Pro | **~70%** | **High** | Week 3 |
| 20 | **Unreal Marketplace** | Free now → **$59.99** later | **88%** | Medium | Free: Week 4, Paid: Q3 |
| 21 | Fab | Same as Unreal | **88%** | Medium | With Unreal |
| 22 | **itch.io** | **PWYW ($0+, suggest $10)** | **90%** | Medium | Week 1 |
| 23 | **Polar / Gumroad** | **$29-$99** (premium packs) | **95%** (Polar) | Medium | Week 1 |
| 24 | **AWS Marketplace** | **$499/mo** managed | **97%** | Future | Q4 2026 |
| 25 | GCP Marketplace | **$499/mo** managed | **97%** | Future | Q4 2026 |
| 26 | Azure Marketplace | **$499/mo** managed | **97%** | Future | Q1 2027 |
| 27 | Udemy | **$49.99-$99.99** courses | **37-97%** | Medium | Month 3 |
| 28 | Game Jams | Free (sponsorship) | N/A | Medium | Ongoing |
| 29 | Academic | Free (.edu) | N/A | Low | Month 4 |
| 30 | GDC / Conferences | Cost (sponsorship) | Leads | Future | Q3 2026 |
| 31 | Heroic Labs Partner | Free (co-marketing) | N/A | Immediate | Week 1 |

### Revenue Summary by Channel Type

| Channel Type | Year 1 Revenue | Year 2 Revenue | Year 3 Revenue |
|-------------|---------------:|---------------:|---------------:|
| **Free distribution** (GitHub, npm, skills) | $0 | $0 | $0 |
| **Sponsorship** (GitHub Sponsors) | $18,000 | $36,000 | $60,000 |
| **Marketplace** (Asset Store, Unreal, itch) | $42,000 | $120,000 | $180,000 |
| **Direct sales** (Polar, Gumroad) | $12,000 | $24,000 | $48,000 |
| **Managed services** (SaaS) | $6,000 | $78,000 | $300,000 |
| **Cloud marketplaces** (AWS/GCP/Azure) | $0 | $24,000 | $96,000 |
| **Content** (Udemy, articles) | $5,000 | $36,000 | $60,000 |
| **Enterprise / OEM** | $0 | $0 | $120,000 |
| **Total** | **$83,000** | **$318,000** | **$864,000** |

---

### The One-Line Strategy

> **Give away everything that creates adoption. Charge for everything that requires infrastructure, support, or exclusive content.**

Skills are free because they are marketing. The SDK is free because Nakama is free. Premium packs are $29-$99 because they save time. Managed services are $99-$499/mo because they replace a DevOps hire. Enterprise is custom because the value is custom.

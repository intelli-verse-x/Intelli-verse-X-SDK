# Java / Android

> IntelliVerseX **Java library** for JVM desktop services, tooling, and **Android** games. It wraps the Nakama Java client with auth, economy, storage, RPC, modular Hiro live-ops, AI stubs, Discord social surfaces, Satori stubs, and game-mode helpers.

## Requirements

- **Java 11+** (`sourceCompatibility` / `targetCompatibility` in `SDKs/java/build.gradle`)
- **Android API 21+** when targeting Android (validate against your `minSdk` in the app module)
- **[Nakama Java client](https://github.com/heroiclabs/nakama-java)** — pulled as `com.github.heroiclabs:nakama-java` (JitPack); see `SDKs/java/build.gradle` for the pinned tag
- **Gradle 7+** for building the SDK; Android Gradle Plugin version must match your studio

## Installation

### Gradle (Maven coordinates)

The SDK’s Gradle project uses **`group = 'ai.intelli-verse-x'`** and **`artifactId = 'sdk'`** (see `SDKs/java/build.gradle`). When published or consumed from a local Maven repository:

```groovy
repositories {
    mavenCentral()
    maven { url 'https://jitpack.io' }  // required for nakama-java transitive
}

dependencies {
    implementation 'ai.intelli-verse-x:sdk:5.8.0'
}
```

### Composite / local module

From a monorepo checkout:

```groovy
include ':intelliversex-sdk'
project(':intelliversex-sdk').projectDir = new File(settingsDir, '../Intelli-verse-X-Unity-SDK/SDKs/java')
```

Then:

```groovy
dependencies {
    implementation project(':intelliversex-sdk')
}
```

### Maven (`pom.xml`)

```xml
<repositories>
    <repository>
        <id>jitpack.io</id>
        <url>https://jitpack.io</url>
    </repository>
</repositories>

<dependency>
    <groupId>ai.intelli-verse-x</groupId>
    <artifactId>sdk</artifactId>
    <version>5.8.0</version>
</dependency>
```

Older docs sometimes referenced `com.intelliversex`; align with **`ai.intelli-verse-x`** for builds that track this repository.

## Quick Start

```java
import com.intelliversex.sdk.core.IVXConfig;
import com.intelliversex.sdk.core.IVXManager;

IVXManager ivx = IVXManager.getInstance();

IVXConfig config = IVXConfig.builder()
    .nakamaHost("127.0.0.1")
    .nakamaPort(7350)
    .nakamaServerKey("defaultkey")
    .enableDebugLogs(true)
    .build();

ivx.initialize(config);

ivx.on("authSuccess", userId -> {
    System.out.println("Logged in: " + userId);
    var profile = ivx.fetchProfile();
    var wallet = ivx.fetchWallet();
});

if (!ivx.restoreSession()) {
    ivx.authenticateDevice(null);
}
```

### Android device ID

```java
String deviceId = Settings.Secure.getString(
    getContentResolver(), Settings.Secure.ANDROID_ID);
ivx.authenticateDevice(deviceId);
```

Prefer a **stable, privacy-compliant** device identifier strategy per Google Play policy; `ANDROID_ID` is a common default but may not be unique across all devices or resets.

## API Surface

Packages are under **`com.intelliversex.sdk.*`**. The table lists the main entry types developers touch.

| Type | Package / role |
|------|----------------|
| `IVXManager` | `core` — singleton facade: init, events (`on`/`emit` pattern), auth, profile, wallet, leaderboards, storage, RPC, session restore |
| `IVXConfig` | `core` — builder for Nakama host/port/key, TLS, debug, optional `gameId` |
| `IVXGameModeManager` | `gamemodes` — mode selection, slots, rooms, match lifecycle (stub depth varies) |
| `IVXLobbyManager` | `gamemodes` — lobby-oriented flows alongside game modes |
| `IVXHiroSystems` | `hiro` — typed Hiro subsystems: `spinWheel()`, `streaks()`, `offerwall()`, `retention()`, `friendQuests()`, `friendBattles()`, `iapTrigger()`, `smartAdTimer()` |
| `IVXDiscordSocial` | `discord` — Rich Presence, friends, invites, voice/chat models (stubs) |
| `IVXDiscordSettings`, `IVXDiscordMessages`, `IVXDiscordModeration`, `IVXDiscordLinkedChannels`, `IVXDiscordDebug` | `discord` — supporting Discord features |
| `IVXSatori` | `analytics` — Satori-style events, flags, experiments (stub) |
| `IVXAIClient`, `IVXAIAssistant`, `IVXAIContentGenerator`, `IVXAIModerator`, `IVXAINPCDialogManager`, `IVXAIProfiler`, `IVXAIVoiceServices` | `ai` — AI integration surfaces |
| `IVXDeepLinks` | `platform` — deep link parsing / campaign attribution hooks |

**Naming note:** There is no single class named **`IVXMultiplayer`** in Java; use **`IVXGameModeManager`** and **`IVXLobbyManager`** for multiplayer-oriented flows. Documentation may refer to “multiplayer” conceptually.

## Feature Coverage

Legend: **Y** = usable in this SDK layer, **S** = stub or partial, **—** = rely on raw Nakama or custom code.

| Area | Java / Android |
|------|----------------|
| Device / email / Google / Apple / custom auth | Y |
| Session restore, profile, wallet, leaderboards, storage, RPC | Y |
| Real-time socket (Nakama socket API) | Y via `IVXManager.getClient()` + Nakama APIs |
| AI: init / assistant / host-style flows | Y (surface); voice / heavy LLM | S |
| Game modes abstraction (`IVXGameModeManager`) | S |
| Lobby / matchmaking helpers (`IVXLobbyManager`) | Y |
| Hiro subsystems (spin, streaks, offerwall, friends, retention, monetization helpers) | Y |
| Discord social stack | S |
| Satori (`IVXSatori`) | S |
| Deep links (`IVXDeepLinks`) | Y |

## Android integration

### Activity lifecycle

- Call **`IVXManager.initialize(IVXConfig)`** once when your application or **first game Activity** is ready — typically from `Application.onCreate()` or the main `Activity` after `super.onCreate()`, before any auth UI.
- **Do not** re-initialize on every `Activity` unless you explicitly tear down the SDK; the manager is a process-wide singleton.
- On **process death**, call **`restoreSession()`** early; if it fails, fall back to **`authenticateDevice`** (or your chosen auth).
- On **logout**, use **`clearSession()`** (or equivalent API on manager) and clear UI state; unregister listeners if you registered many lambdas to avoid leaks on rotating activities (prefer a single ViewModel-owned subscription pattern).

### Session persistence and `Preferences`

`IVXManager` defaults to **`java.util.prefs.Preferences`**, which is **not reliable on all Android devices**. Before **`initialize`**, call:

```java
ivx.setPreferences(mySharedPreferencesBackedPreferences);
```

Implement a small adapter from **`SharedPreferences`** to **`Preferences`** or use a community-backed bridge so tokens survive restarts consistently.

### ProGuard / R8

The SDK uses **Gson** and **Nakama** models. Typical keep rules:

```proguard
-keepattributes Signature, InnerClasses, EnclosingMethod
-keepclassmembers class * {
    @com.google.gson.annotations.SerializedName <fields>;
}
-keep class com.heroiclabs.nakama.** { *; }
-keep class com.intelliversex.sdk.** { *; }
```

Adjust after shrinking passes if reflection-based RPC parsing fails in release builds.

### AndroidManifest permissions

Minimum set depends on your features:

- **`INTERNET`** — required for Nakama and backend calls.
- Optional: **`ACCESS_NETWORK_STATE`** for connectivity checks in your own code (not strictly required by the SDK JAR itself).

Do **not** add dangerous permissions unless your game uses related features (e.g. microphone for voice — not enforced by the SDK alone).

## Advanced examples

### Hiro: “league-style” progression (quests + leaderboards)

There is no single **`IVXHiroLeagues`** class in this JAR; league-style UX is usually **leaderboards + Hiro quests/battles** on the server. Client-side, combine **`IVXHiroSystems.friendQuests()`** with **`IVXManager.fetchLeaderboard`**:

```java
IVXHiroSystems hiro = new IVXHiroSystems(ivx.getClient(), ivx.getSession());

hiro.friendQuests().list().thenAccept(quests -> {
    // bind to UI on main thread
});

List<Map<String, Object>> standings = ivx.fetchLeaderboard("season_rank", 50);
// render standings
```

Align RPC ids, quest payloads, and leaderboard ids with your Nakama/Hiro deployment.

### Authentication flow with Android lifecycle

```java
public class GameActivity extends AppCompatActivity {
    private final IVXManager ivx = IVXManager.getInstance();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (!ivx.isInitialized()) {
            IVXConfig cfg = IVXConfig.builder()
                .nakamaHost(BuildConfig.NAKAMA_HOST)
                .nakamaPort(BuildConfig.NAKAMA_PORT)
                .nakamaServerKey(BuildConfig.NAKAMA_KEY)
                .enableDebugLogs(BuildConfig.DEBUG)
                .build();
            ivx.initialize(cfg);
        }
        ivx.on("authSuccess", uid -> runOnUiThread(() -> showMainMenu()));
        if (!ivx.restoreSession()) {
            String did = Settings.Secure.getString(
                getContentResolver(), Settings.Secure.ANDROID_ID);
            ivx.authenticateDevice(did);
        }
    }
}
```

Use **`runOnUiThread`** (or main-thread `Handler`) for UI updates from callbacks.

## Troubleshooting

### JitPack / Nakama resolve failures

- Ensure **`maven { url 'https://jitpack.io' }`** is present; nakama-java is consumed as **`com.github.heroiclabs:nakama-java:...`**.
- If resolution is slow, mirror artifacts to an internal Nexus/Artifactory.

### Gradle dependency conflicts

- **Gson**: SDK pins a Gson version; force a single version with `resolutionStrategy` if another library pulls an ancient Gson.
- **OkHttp / Netty**: Nakama Java brings its own stack; avoid duplicate older OkHttp on the classpath for Android (use BOM or align versions in the app `build.gradle`).

### ProGuard stripping Gson or Nakama models

- Symptom: `null` fields after RPC or crash in reflective adapters.
- Fix: broaden **`-keep`** rules for your DTO packages and Nakama generated types; test **release** builds on device.

### Android permissions / cleartext

- Cleartext HTTP to a dev server requires **`android:usesCleartextTraffic="true"`** in the manifest or a **network security config** — only for debug.

### `Preferences` / session not restored on device

- Symptom: user logs in every cold start.
- Fix: **`setPreferences`** with **SharedPreferences**-backed storage before **`initialize`**.

## Nakama client

Built on **[nakama-java](https://github.com/heroiclabs/nakama-java)**. Use **`IVXManager.getClient()`** and **`getSession()`** for advanced APIs (realtime socket, groups, tournaments) beyond the IVX facade.

## Source

- **[SDKs/java/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/java)** — Gradle library project, sources under `src/main/java/com/intelliversex/sdk/`.

## Further reading

- [Nakama Java client documentation](https://heroiclabs.com/docs/nakama/client-libraries/java/) — sessions, sockets, and API coverage.
- [Android data safety and identifiers](https://developer.android.com/training/articles/user-data-ids) — policy guidance for device IDs.
- IntelliVerseX skills: **`.cursor/skills/ivx-sdk-setup/SKILL.md`**, **`.cursor/skills/ivx-live-ops/SKILL.md`**, **`.cursor/skills/ivx-ai-integration/SKILL.md`** — higher-level product patterns (engine-agnostic backend assumptions).
- In-repo **Java README**: `SDKs/java/README.md` — module map and stub vs implemented notes.

# IntelliVerseX Java / Android SDK

> Complete modular game development SDK for Java and Android — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.8.0

### AI Voice & Host (`IVXAIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```java
import com.intelliversex.sdk.ai.IVXAIClient;

IVXAIClient ai = IVXAIClient.getInstance();
ai.initialize("https://ai.intelli-verse-x.ai", "your-key");

ai.startVoiceSession("persona-1", userId).thenAccept(session -> {
    System.out.println("Session: " + session.getSessionId());
});
ai.getPersonas().thenAccept(personas -> { ... });
ai.checkEntitlement(userId).thenAccept(result -> { ... });
```

### Multiplayer & Game Modes (`IVXGameModeManager`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```java
import com.intelliversex.sdk.gamemodes.IVXGameModeManager;
import com.intelliversex.sdk.gamemodes.IVXGameModeManager.IVXGameMode;

IVXGameModeManager gm = IVXGameModeManager.getInstance();
gm.selectMode(IVXGameMode.ONLINE_VERSUS, 4);
gm.addPlayer("Alice", true);
gm.setPlayerReady(0, true);
if (gm.canStartMatch()) gm.startMatch();
```

### Hiro Live-Ops Systems (`IVXHiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```java
import com.intelliversex.sdk.hiro.IVXHiroSystems;

IVXHiroSystems hiro = new IVXHiroSystems(nakamaClient, session);
hiro.spinWheel().spin("daily_wheel").thenAccept(result -> { ... });
hiro.streaks().getState().thenAccept(state -> { ... });
hiro.streaks().claim().thenAccept(state -> { ... });
hiro.offerwall().getState().thenAccept(offers -> { ... });
```

## What's New in v5.8.0

- Discord Social SDK integration (Rich Presence, friends, lobbies, voice, invites, DMs, moderation)
- Satori Analytics (events, feature flags, A/B experiments, live events)
- Hiro parity: retention, IAP triggers, smart ad timer (Unreal/C++/Cocos/Godot/Defold)

### Discord Social SDK (`IVXDiscordSocial`)

- Rich Presence, friends list, lobbies, voice chat
- Game invites, DMs, moderation tools

```java
import com.intelliversex.sdk.social.IVXDiscordSocial;

IVXDiscordSocial discord = IVXDiscordSocial.getInstance();
discord.initialize(IVXDiscordConfig.builder()
    .applicationId("YOUR_APP_ID")
    .clientId("YOUR_CLIENT_ID")
    .build());

discord.updatePresence("In Match", "Round 3 of 5");
discord.getFriends().thenAccept(friends -> { /* ... */ });
```

### Satori Analytics (`IVXSatori`)

- Event capture, feature flags, A/B experiments, live events

```java
import com.intelliversex.sdk.analytics.IVXSatori;

IVXSatori satori = IVXSatori.getInstance();
satori.initialize(IVXSatoriConfig.builder()
    .satoriUrl("https://satori.example.com")
    .apiKey("your-satori-key")
    .build());

satori.captureEvents(List.of(new IVXEvent("level_complete", "5")));
var flags = satori.getFeatureFlags();
```

## Requirements

- Java 11+ / Android API 21+
- [Nakama Java Client](https://github.com/heroiclabs/nakama-java) (via [JitPack](https://jitpack.io/#heroiclabs/nakama-java), e.g. `v2.5.3`)
- Gradle 7+

### Platform Support

- **Java**: Android (API 21+), Desktop Java 11+, Kotlin compatible

## Installation

Your `repositories` must include **JitPack** (transitive Nakama client):

```groovy
repositories {
    mavenCentral()
    maven { url 'https://jitpack.io' }
}
```

### Gradle

```groovy
dependencies {
    implementation 'ai.intelli-verse-x:sdk:5.8.0'
}
```

### Maven

```xml
<dependency>
    <groupId>ai.intelli-verse-x</groupId>
    <artifactId>sdk</artifactId>
    <version>5.8.0</version>
</dependency>
```

### Local Build

```bash
cd SDKs/java
./gradlew build
```

## Setting Up Nakama Server

The SDK requires a [Nakama](https://heroiclabs.com/nakama/) game server for backend features.

**Quick start with Docker:**

```bash
docker run -d --name nakama -p 7349:7349 -p 7350:7350 -p 7351:7351 heroiclabs/nakama
```

**Heroic Labs Cloud:** For production, use [Heroic Labs Cloud](https://heroiclabs.com/) for managed hosting.

See [Nakama documentation](https://heroiclabs.com/docs/nakama/) for full setup instructions.

## Quick Start

```java
import com.intelliversex.sdk.core.IVXConfig;
import com.intelliversex.sdk.core.IVXManager;

public class Main {
    public static void main(String[] args) {
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
            System.out.println("Profile: " + profile);

            var wallet = ivx.fetchWallet();
            System.out.println("Wallet: " + wallet);
        });

        ivx.on("error", error -> {
            System.err.println("Error: " + error);
        });

        if (!ivx.restoreSession()) {
            ivx.authenticateDevice(null);
        }
    }
}
```

### Android

```java
public class GameActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        IVXManager ivx = IVXManager.getInstance();

        IVXConfig config = IVXConfig.builder()
            .nakamaHost("your-server.com")
            .nakamaPort(7350)
            .useSSL(true)
            .enableDebugLogs(BuildConfig.DEBUG)
            .build();

        ivx.initialize(config);

        // Use Android device ID
        String deviceId = Settings.Secure.getString(
            getContentResolver(), Settings.Secure.ANDROID_ID);
        ivx.authenticateDevice(deviceId);
    }
}
```

## Features

| Feature | Status |
|---------|--------|
| Device Auth | ✅ Supported |
| Email Auth | ✅ Supported |
| Google Auth | ✅ Supported |
| Apple Auth | ✅ Supported |
| Custom Auth | ✅ Supported |
| Profile Management | ✅ Supported |
| Wallet / Economy | ✅ Supported |
| Leaderboards | ✅ Supported |
| Cloud Storage | ✅ Supported |
| RPC Calls | ✅ Supported |
| AI Voice & Host | 🔶 Stub |
| Multiplayer & Game Modes | 🔶 Stub |
| Hiro Live-Ops Systems | 🔶 Stub |
| Analytics | ✅ Supported |
| Discord Social SDK | 🔶 Stub |
| Satori Analytics | 🔶 Stub |
| Android | ✅ Supported |
| Desktop Java | ✅ Supported |

> 🔶 **Stub** = Full API surface exists. Methods log warnings and return empty/mock data. Zero code changes needed when backend support ships.

## Project Structure

```
src/main/java/com/intelliversex/sdk/
├── core/
│   ├── IVXConfig.java
│   └── IVXManager.java
├── ai/
│   ├── IVXAIClient.java        # AI voice & host client
│   └── IVXAIModels.java        # AI data models
├── gamemodes/
│   ├── IVXGameModeManager.java # Game mode & lobby manager
│   ├── IVXGameModeModels.java  # Game mode data models
│   └── IVXLobbyManager.java    # Room/lobby management
└── hiro/
    └── IVXHiroSystems.java     # Hiro live-ops typed wrappers
```

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/platforms/java/).

## Nakama Client Library

This SDK wraps the official [Nakama Java Client](https://github.com/heroiclabs/nakama-java) (37 stars, 22 forks).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection timeout | Verify Nakama server is running and accessible at the configured host:port |
| Auth failed | Check server key matches your Nakama configuration |
| AI features not working | Verify AI API endpoint and key are set in config |
| Discord not connecting | Ensure `applicationId` and `clientId` are valid and Discord app is approved |
| Satori events not captured | Check `satoriUrl` and `apiKey` are correctly configured |

## License

MIT License — see [LICENSE](../../LICENSE)

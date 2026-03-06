# IntelliVerseX Java / Android SDK

> Complete modular game development SDK for Java and Android — Auth, Backend (Nakama), Analytics, Social, Monetization, and more.

## Requirements

- Java 11+ / Android API 21+
- [Nakama Java Client](https://github.com/heroiclabs/nakama-java) v2.9+
- Gradle 7+

## Installation

### Gradle

```groovy
dependencies {
    implementation 'com.intelliversex:sdk:5.1.0'
}
```

### Maven

```xml
<dependency>
    <groupId>com.intelliversex</groupId>
    <artifactId>sdk</artifactId>
    <version>5.1.0</version>
</dependency>
```

### Local Build

```bash
cd SDKs/java
./gradlew build
```

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
| Device Auth | Supported |
| Email Auth | Supported |
| Google Auth | Supported |
| Apple Auth | Supported |
| Custom Auth | Supported |
| Profile Management | Supported |
| Wallet / Economy | Supported |
| Leaderboards | Supported |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| Hiro Systems | Via RPC |
| Android | Supported |
| Desktop Java | Supported |

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/java/).

## Nakama Client Library

This SDK wraps the official [Nakama Java Client](https://github.com/heroiclabs/nakama-java) (37 stars, 22 forks).

## License

MIT License — see [LICENSE](../../LICENSE)

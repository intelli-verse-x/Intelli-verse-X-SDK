# Java / Android

> IntelliVerseX Java library for desktop and Android applications, built with Gradle.

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

### Android

```java
String deviceId = Settings.Secure.getString(
    getContentResolver(), Settings.Secure.ANDROID_ID);
ivx.authenticateDevice(deviceId);
```

## Nakama Client

Built on [nakama-java](https://github.com/heroiclabs/nakama-java) (37 stars, 22 forks).

## Source

[SDKs/java/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/java)

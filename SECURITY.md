# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 6.0.x   | :white_check_mark: |
| 5.11.x  | :white_check_mark: (security fixes only) |
| 5.10.x  | :x:                |
| 5.8.x   | :x:                |
| < 5.8   | :x:                |

## Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability, please report it responsibly.

### How to Report

**Do NOT create a public GitHub issue for security vulnerabilities.**

Instead:

1. **Email:** security@intelli-verse-x.ai
2. **Subject:** `[SECURITY] Brief description`
3. **Include:**
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

### What to Expect

| Timeframe | Action |
|-----------|--------|
| 24 hours | Acknowledgment of your report |
| 72 hours | Initial assessment |
| 7 days | Detailed response with timeline |
| 30-90 days | Fix released (depending on severity) |

### Severity Levels

| Level | Description | Response Time |
|-------|-------------|---------------|
| Critical | RCE, data breach, auth bypass | 24-48 hours |
| High | Privilege escalation, data leak | 7 days |
| Medium | Limited impact vulnerabilities | 30 days |
| Low | Minor issues, hardening | 90 days |

## Security Best Practices

When using IntelliVerseX SDK:

### Configuration Security

```csharp
// Don't hardcode secrets in ScriptableObjects or tooltips
public string apiKey = "sk_live_xxxx"; // BAD

// Use config/keys.json (gitignored) or IVXSecureStorage
// Copy config/keys.example.json → config/keys.json; never commit keys.json
var apiKey = IVXSecureStorage.GetString("api_key");
```

### Data Encryption

The SDK encrypts sensitive PlayerPrefs values with AES-256-CBC (PBKDF2-derived per-device key):

```csharp
IVXSecureStorage.SetObject("user_data", sensitiveData);
```

WebGL / browser builds cannot provide the same secrecy as native — treat client storage as untrusted.

### Network Security

- All backend communication uses TLS 1.2+
- Token refresh reduces long-lived session risk
- **Certificate pinning is not currently enforced in the Unity package** — rely on platform TLS and pin at the reverse-proxy if required for your threat model

### Input Validation

```csharp
// The SDK validates many inputs; still sanitize untrusted user content before display or RPC
var sanitizedInput = SanitizeUserInput(rawInput);
```

## Known Security Considerations

### Shared cloud defaults

Shared Nakama hosts may use platform default server keys for the managed SaaS. For self-hosting, set a strong server key in `IVXBootstrapConfig` and never ship production secrets in client tooltips or source.

### WebGL / Browser Limitations

WebGL and JavaScript browser builds have reduced security:
- No native encryption guarantees
- Data stored in IndexedDB / localStorage
- Session tokens accessible via browser dev tools
- Prefer server-side validation for economy and competitive scores

### Platform-Specific Notes

| Platform | Session Storage | Encryption |
|----------|----------------|------------|
| Unity (Mobile) | PlayerPrefs (encrypted via IVXSecureStorage) | AES-256 |
| Unity (WebGL) | IndexedDB / PlayerPrefs | Limited |
| Unreal | GConfig (Game.ini) | File-system level |
| Godot | ConfigFile (user://) | None by default |
| Defold | sys.save | None by default |
| JavaScript | localStorage | None |
| C/C++ | File-based | None by default |
| Java | java.util.prefs | None by default |
| Flutter/Dart | SharedPreferences | None by default |
| Web3 | Browser wallet + localStorage | TLS only |

### Debug Builds

Disable verbose logging in production:

```csharp
#if !DEVELOPMENT_BUILD
    IVXLogger.SetLevel(LogLevel.Error);
#endif
```

## Security Changelog

### v6.0.0
- Removed shared Photon App IDs from consumer-visible tooltips
- Replaced fixed secure-storage fallback secrets with install-scoped seeds
- Documented that certificate pinning is not enforced in-package
- Added CodeQL workflow for C# / JS / Python surfaces

### v5.1.0 (2026-03-02)
- Added Flutter/Dart and Web3/TypeScript SDK security considerations
- Web3 wallet signature authentication (EIP-191)
- Secure device ID caching in Flutter SDK

### v5.0.0 (2026-02-27)
- Upgraded encryption to AES-256
- Improved token storage security
- Added secure device ID generation

---

Questions about security? Email security@intelli-verse-x.ai

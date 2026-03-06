# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 5.0.x   | :white_check_mark: |
| 4.2.x   | :white_check_mark: |
| 4.1.x   | :x:                |
| < 4.1   | :x:                |

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
// ❌ Don't hardcode secrets
public string apiKey = "sk_live_xxxx";

// ✅ Use secure storage
var apiKey = IVXSecureStorage.GetString("api_key");
```

### Data Encryption

The SDK encrypts sensitive data by default:

```csharp
// Encrypted storage (default)
IVXSecureStorage.SetObject("user_data", sensitiveData);

// Verify encryption is enabled
Debug.Assert(IntelliVerseXSDK.Config.encryptLocalData);
```

### Network Security

- All backend communication uses TLS 1.2+
- SSL certificate pinning available for mobile
- Token refresh prevents session hijacking

### Input Validation

```csharp
// The SDK validates inputs, but always sanitize user data
var sanitizedInput = SanitizeUserInput(rawInput);
```

## Known Security Considerations

### WebGL / Browser Limitations

WebGL and JavaScript browser builds have reduced security:
- No native encryption
- Data stored in IndexedDB / localStorage (not encrypted)
- Session tokens accessible via browser dev tools
- Consider additional server-side validation

### Platform-Specific Notes

| Platform | Session Storage | Encryption |
|----------|----------------|------------|
| Unity (Mobile) | PlayerPrefs (encrypted) | AES-256 |
| Unity (WebGL) | IndexedDB | None |
| Unreal | GConfig (Game.ini) | File-system level |
| Godot | ConfigFile (user://) | None by default |
| Defold | sys.save | None by default |
| JavaScript | localStorage | None |
| C/C++ | File-based | None by default |
| Java | java.util.prefs | None by default |

For all non-Unity platforms, sensitive data should be protected at the OS/filesystem level. SSL/TLS is enforced for all server communication.

### Debug Builds

Debug builds may expose sensitive information. Disable debug logging in production:

```csharp
// Unity
#if !DEVELOPMENT_BUILD
    IVXLogger.SetLevel(LogLevel.Error);
#endif
```

```typescript
// JavaScript
ivx.initialize({ enableDebugLogs: false });
```

```java
// Java
IVXConfig.builder().enableDebugLogs(false).build();
```

## Security Changelog

### v5.0.0 (2025-01-13)
- Upgraded encryption to AES-256
- Added SSL certificate pinning
- Improved token storage security
- Added secure device ID generation

### v4.2.0 (2024-11-15)
- Fixed potential token exposure in logs
- Added session timeout enforcement

## Acknowledgments

We thank the following security researchers:

- (Your name could be here!)

---

Questions about security? Email security@intelli-verse-x.ai

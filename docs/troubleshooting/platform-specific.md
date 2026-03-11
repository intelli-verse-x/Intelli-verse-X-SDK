# Platform-Specific Issues

Platform-specific problems and solutions for Android, iOS, and WebGL.

---

## Android

### Permissions

#### Missing Runtime Permissions

**Symptom:** Features fail silently on Android 6+

**Solution:**
```csharp
// Check and request permissions
if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
{
    Permission.RequestUserPermission(Permission.ExternalStorageWrite);
}
```

#### Android 13+ Notification Permission

**Symptom:** Push notifications don't appear

**Solution:**
Add to `AndroidManifest.xml`:
```xml
<uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
```

Request at runtime:
```csharp
#if UNITY_ANDROID && !UNITY_EDITOR
if (Application.platform == RuntimePlatform.Android)
{
    Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
}
#endif
```

---

### Google Play Services

#### Google Sign-In Fails

**Error:**
```
GoogleSignIn failed: DEVELOPER_ERROR
```

**Solutions:**
1. Check SHA-1 fingerprint in Firebase/Google Cloud Console
2. Verify `google-services.json` is in `Assets/Plugins/Android/`
3. Ensure package name matches exactly

**Get SHA-1:**
```bash
# Debug key
keytool -list -v -keystore ~/.android/debug.keystore -alias androiddebugkey -storepass android

# Release key
keytool -list -v -keystore your-release.keystore -alias your-alias
```

---

### Deep Links

**Symptom:** App links don't open the app

**Solution:**
1. Add intent filter to `AndroidManifest.xml`:
```xml
<intent-filter android:autoVerify="true">
    <action android:name="android.intent.action.VIEW"/>
    <category android:name="android.intent.category.DEFAULT"/>
    <category android:name="android.intent.category.BROWSABLE"/>
    <data android:scheme="https" 
          android:host="yourdomain.com" 
          android:pathPrefix="/app"/>
</intent-filter>
```

2. Host `.well-known/assetlinks.json` on your domain

---

### Back Button

**Symptom:** Back button closes app instead of navigating

**Solution:**
```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (CanGoBack())
        {
            GoBack();
        }
        else
        {
            ShowExitConfirmation();
        }
    }
}
```

---

## iOS

### Code Signing

#### Provisioning Profile Issues

**Error:**
```
Code signing is required for product type 'Application'
```

**Solution:**
1. Open in Xcode → Signing & Capabilities
2. Select correct team
3. Enable "Automatically manage signing"

---

### Capabilities

#### Sign in with Apple Not Working

**Symptom:** Apple login button doesn't respond

**Solutions:**
1. Enable capability in Xcode: Signing & Capabilities → + → Sign in with Apple
2. Ensure provisioning profile includes the capability
3. Check Apple Developer portal has Sign in with Apple enabled

---

#### Push Notifications Not Arriving

**Checklist:**
1. Push Notifications capability enabled in Xcode
2. Push certificate uploaded to backend
3. Device is registered for remote notifications
4. Not in sandbox/production mismatch

```csharp
// Register for notifications
UnityEngine.iOS.NotificationServices.RegisterForNotifications(
    UnityEngine.iOS.NotificationType.Alert |
    UnityEngine.iOS.NotificationType.Badge |
    UnityEngine.iOS.NotificationType.Sound
);
```

---

### App Store

#### IAP Products Not Loading

**Symptom:** Products array is empty

**Solutions:**
1. Products configured in App Store Connect
2. App in "Ready to Submit" or later state
3. Product IDs match exactly
4. Using sandbox tester account

```csharp
// Debug IAP loading
IVXIAPManager.OnProductsLoaded += (products) =>
{
    Debug.Log($"Loaded {products.Count} products");
    foreach (var p in products)
    {
        Debug.Log($"  - {p.productId}: {p.price}");
    }
};
```

---

### Keychain

#### Data Lost After Reinstall

**Cause:** Keychain not persisting across installs

**Solution:**
Use Keychain with appropriate accessibility:
```csharp
// SDK handles this automatically, but ensure
// kSecAttrAccessibleAfterFirstUnlock is used
```

---

## WebGL

### Browser Compatibility

#### Not Working in Safari

**Symptom:** Features fail in Safari

**Solutions:**
1. Enable SharedArrayBuffer (requires COOP/COEP headers)
2. Check IndexedDB is available
3. Disable certain features for Safari

```csharp
#if UNITY_WEBGL
if (IsSafari())
{
    // Use fallback implementations
}
#endif
```

---

### CORS Issues

**Error:**
```
Access to XMLHttpRequest blocked by CORS policy
```

**Solution:**
Configure server to allow CORS:
```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization
```

---

### IndexedDB Errors

**Error:**
```
QuotaExceededError: The quota has been exceeded
```

**Solutions:**
1. Reduce data stored
2. Clear old data periodically
3. Handle quota errors gracefully

```csharp
try
{
    IVXSecureStorage.SetObject("large_data", data);
}
catch (StorageException ex) when (ex.Message.Contains("quota"))
{
    // Clear old data and retry
    IVXSecureStorage.ClearOldData();
    IVXSecureStorage.SetObject("large_data", data);
}
```

---

### Memory Limits

**Symptom:** Page crashes or freezes

**Solutions:**
1. Increase memory in Player Settings (512MB+)
2. Use compression for textures
3. Unload unused assets

---

### Fullscreen Issues

**Symptom:** Fullscreen doesn't work or breaks input

**Solution:**
```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
[DllImport("__Internal")]
private static extern void RequestFullscreen();

public void GoFullscreen()
{
    RequestFullscreen();
}
#endif
```

---

### Audio Autoplay

**Symptom:** Audio doesn't play

**Cause:** Browsers block autoplay

**Solution:**
```csharp
// Start audio on user interaction
public void OnUserClick()
{
    // Now audio can play
    AudioSource.PlayClipAtPoint(clip, Vector3.zero);
}
```

---

## macOS

### Notarization

**Error:** App damaged or from unidentified developer

**Solution:**
1. Sign with Developer ID certificate
2. Notarize with Apple
3. Staple notarization ticket

```bash
xcrun notarytool submit build.zip --apple-id email --team-id TEAM --password @keychain:notarize
```

---

### Sandbox Issues

**Symptom:** Can't access files or network

**Solution:**
Adjust `*.entitlements`:
```xml
<key>com.apple.security.network.client</key>
<true/>
<key>com.apple.security.files.user-selected.read-write</key>
<true/>
```

---

## Windows

### Windows Firewall

**Symptom:** Network calls blocked

**Solution:**
Add exception for your app or use standard HTTPS ports.

---

### Long Path Names

**Error:** Path too long exceptions

**Solution:**
Enable long paths in Windows 10+:
```
Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem
LongPathsEnabled = 1
```

---

## Cross-Platform Tips

### Platform-Specific Code

```csharp
#if UNITY_ANDROID
    // Android-specific code
#elif UNITY_IOS
    // iOS-specific code
#elif UNITY_WEBGL
    // WebGL-specific code
#else
    // Desktop/other
#endif
```

### Runtime Platform Check

```csharp
switch (Application.platform)
{
    case RuntimePlatform.Android:
        AndroidSpecificSetup();
        break;
    case RuntimePlatform.IPhonePlayer:
        IOSSpecificSetup();
        break;
    case RuntimePlatform.WebGLPlayer:
        WebGLSpecificSetup();
        break;
}
```

---

## Still Stuck?

- Check [GitHub Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/issues) for platform-specific bugs
- Post on [GitHub Discussions](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/discussions) with platform tag

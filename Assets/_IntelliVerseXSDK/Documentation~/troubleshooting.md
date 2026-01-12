# Troubleshooting Guide

This guide covers common issues and their solutions when using the IntelliVerseX SDK.

## Installation Issues

### Package not appearing in Package Manager

**Symptoms**: After adding the Git URL, the package doesn't appear.

**Solutions**:
1. Check your `manifest.json` syntax is valid JSON
2. Ensure Git is installed and accessible from command line
3. Verify the Git URL is correct:
   ```
   https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK
   ```
4. Try closing and reopening Unity
5. Delete the `Library/PackageCache` folder and reopen Unity

### "Could not resolve package" error

**Solutions**:
1. Check your internet connection
2. Verify the repository is public and accessible
3. If using a specific version tag, ensure it exists
4. Try without the version tag first

## Compilation Errors

### "Assembly not found" errors

**Symptoms**: Errors like `The referenced assembly 'IntelliVerseX.Core' could not be resolved`

**Solutions**:
1. Run **IntelliVerseX > Project Setup & Validation**
2. Ensure all assembly definitions have correct references
3. Check that package was installed completely
4. Reimport the package: Right-click package folder > Reimport

### "Newtonsoft.Json not found"

**Solutions**:
1. The package should install automatically, but if not:
2. Add manually via Package Manager: `com.unity.nuget.newtonsoft-json`
3. Or run **IntelliVerseX > Setup Wizard** and follow steps

### "Nakama not found"

**Solutions**:
1. Nakama must be installed manually
2. Download from: https://github.com/heroiclabs/nakama-unity/releases
3. Import the `.unitypackage` file
4. Restart Unity after import

## Runtime Issues

### "NullReferenceException" on SDK managers

**Symptoms**: Errors when accessing `Instance` on SDK managers

**Solutions**:
1. Ensure the SDK is initialized in `Awake()` or `Start()`
2. Check execution order - SDK managers should initialize first
3. Verify the manager GameObject exists in the scene
4. Use null checks: `if (IVXBackendService.Instance != null)`

### Backend connection fails

**Solutions**:
1. Verify Nakama server is running and accessible
2. Check server URL, port, and key in config
3. Ensure SSL settings match server configuration
4. Test with a simple curl/Postman request first
5. Check firewall settings

### Localization not loading

**Solutions**:
1. Ensure localization CSV files are in correct location
2. Check file encoding (UTF-8 recommended)
3. Verify language codes match expected values
4. Call `IVXLocalizationService.Instance.LoadLanguage()` after initialization

## Build Issues

### Android build fails

**Solutions**:
1. Check minimum API level is 21+
2. Resolve Gradle conflicts (check for duplicate libraries)
3. Ensure NDK is properly configured
4. Check for conflicting package manifests

### iOS build fails

**Solutions**:
1. Ensure minimum iOS version is 12.0+
2. Check Xcode version compatibility
3. Verify signing certificates are valid
4. For Apple Sign-In, enable capability in Xcode

### WebGL build fails or doesn't work

**Solutions**:
1. Some features are not available on WebGL (file system, native share)
2. Configure CORS on your backend server
3. Use WebGL-specific ad providers
4. Check browser console for errors

## Performance Issues

### High memory usage

**Solutions**:
1. Use object pooling for frequently instantiated objects
2. Unload unused assets with `Resources.UnloadUnusedAssets()`
3. Check for memory leaks in Update loops
4. Profile with Unity Profiler

### Slow initialization

**Solutions**:
1. Use async initialization methods
2. Lazy-load modules that aren't needed immediately
3. Profile to identify bottlenecks
4. Consider spreading initialization across multiple frames

## Common Error Messages

### "INTELLIVERSEX_SDK not defined"

**Solution**: Run **IntelliVerseX > Project Setup & Validation** and click "Apply All Required Settings"

### "Missing .meta file"

**Solution**: This usually happens when files are moved outside Unity. Reimport the affected files or restore .meta files from version control.

### "Assembly has reference to non-existent assembly"

**Solutions**:
1. Install missing dependencies
2. Check that all SDK modules are present
3. Reimport the package

## Getting Help

If your issue isn't covered here:

1. **Search existing issues**: https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK/issues
2. **Create a new issue** with:
   - Unity version
   - SDK version
   - Platform (Editor/Android/iOS/WebGL)
   - Error messages (full stack trace)
   - Steps to reproduce
3. **Contact support**: sdk@intelliversex.com

## Diagnostic Tools

### Project Validation
```
IntelliVerseX > Project Setup & Validation
```

### Dependency Check
```
IntelliVerseX > Check Dependencies
```

### Console Logs
Enable verbose logging:
```csharp
IVXLogger.SetLogLevel(IVXLogLevel.Debug);
```

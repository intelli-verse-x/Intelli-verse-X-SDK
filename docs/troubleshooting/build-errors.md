# Build Errors

Common build-time errors and solutions.

---

## Compilation Errors

### Missing Assembly Reference

**Error:**
```
error CS0246: The type or namespace name 'IVXLogger' could not be found
```

**Solution:**
1. Ensure your assembly references `IntelliVerseX.Core`
2. Check your `.asmdef` file includes the dependency

```json
{
  "name": "MyGame",
  "references": [
    "IntelliVerseX.Core",
    "IntelliVerseX.Backend"
  ]
}
```

---

### Duplicate Assembly Definition

**Error:**
```
Assembly with name 'IntelliVerseX.Core' already exists
```

**Solution:**
- Remove duplicate SDK installations
- Check `Packages/` folder for conflicts
- Clear `Library/` folder and reimport

---

### Missing TextMeshPro

**Error:**
```
error CS0246: The type or namespace name 'TMP_Text' could not be found
```

**Solution:**
1. Window → TextMeshPro → Import TMP Essential Resources
2. Restart Unity after import

---

## Android Build Errors

### Gradle Build Failed

**Error:**
```
Gradle build failed. See the Console for details.
```

**Common Causes:**

1. **Duplicate classes:**
   - Multiple copies of same JAR/AAR
   - Use `gradleTemplate.properties` to resolve

2. **minSdkVersion too low:**
   ```groovy
   // In mainTemplate.gradle
   minSdkVersion 21  // Minimum for SDK
   ```

3. **Missing dependencies:**
   - Ensure Google Play Services is included
   - Check for missing AAR files

---

### MultiDex Required

**Error:**
```
Cannot fit requested classes in a single dex file
```

**Solution:**
In `mainTemplate.gradle`:
```groovy
android {
    defaultConfig {
        multiDexEnabled true
    }
}

dependencies {
    implementation 'androidx.multidex:multidex:2.0.1'
}
```

---

### ProGuard/R8 Stripping

**Error:**
Runtime crashes after obfuscation.

**Solution:**
Add to `proguard-user.txt`:
```
-keep class com.intelliversex.** { *; }
-keep class io.nakama.** { *; }
```

---

## iOS Build Errors

### Missing Capabilities

**Error:**
```
Code signing error: Provisioning profile doesn't include capability
```

**Solution:**
Enable required capabilities in Xcode:
- Sign in with Apple (if using Apple auth)
- Push Notifications (if using push)
- In-App Purchase (if using IAP)

---

### Bitcode Error

**Error:**
```
ld: bitcode bundle could not be generated
```

**Solution:**
1. Build Settings → Enable Bitcode → No
2. Or ensure all libraries support bitcode

---

### Cocoapods Issues

**Error:**
```
Unable to resolve dependency
```

**Solution:**
```bash
cd ios_build_folder
pod repo update
pod install --repo-update
```

---

## WebGL Build Errors

### Stripping Level Too High

**Error:**
```
Build exception: ArgumentException: No matching methods
```

**Solution:**
1. Player Settings → Other Settings → Managed Stripping Level → Minimal
2. Add `link.xml`:

```xml
<linker>
  <assembly fullname="IntelliVerseX.Core" preserve="all"/>
  <assembly fullname="IntelliVerseX.Backend" preserve="all"/>
  <!-- Add other assemblies as needed -->
</linker>
```

---

### Memory Size

**Error:**
```
Out of memory
```

**Solution:**
Player Settings → Publishing Settings:
- Memory Size: 512 (or higher)
- Enable Exception: Full With Stacktrace (for debugging)

---

## IL2CPP Errors

### Type Stripping

**Error:**
```
ExecutionEngineException: Attempting to call method that was not preserved
```

**Solution:**
Create `link.xml` in `Assets/`:

```xml
<linker>
  <assembly fullname="System.Core">
    <type fullname="System.Linq.Expressions.Interpreter.LightLambda" preserve="all" />
  </assembly>
  <assembly fullname="IntelliVerseX.Core" preserve="all"/>
</linker>
```

---

### Generic Instantiation

**Error:**
```
ExecutionEngineException: Attempting to call method with generic parameter
```

**Solution:**
Force AOT compilation by referencing types:

```csharp
// Add to a MonoBehaviour that exists in build
void AOTHints()
{
    // Force generic instantiation
    var _ = new List<MyCustomType>();
}
```

---

## Package Conflicts

### Version Mismatch

**Error:**
```
Package conflict detected: Package X requires Y version Z
```

**Solution:**
1. Check `Packages/manifest.json` for version conflicts
2. Update all packages to compatible versions
3. Use `Packages/packages-lock.json` to lock versions

---

### Conflicting Third-Party SDKs

If using multiple SDKs with shared dependencies:

1. **Use External Dependency Manager:**
   ```
   Window → Google → Play Resolver → Resolve
   ```

2. **Remove duplicate DLLs manually**

3. **Check for namespace conflicts**

---

## Still Stuck?

1. Clear `Library/` folder and reimport
2. Check Unity console for full error
3. Search [GitHub Issues](https://github.com/AhamedAzmi/IntelliVerseX/issues)
4. Post on [GitHub Discussions](https://github.com/AhamedAzmi/IntelliVerseX/discussions)

# IntelliVerseX Editor surfaces (P3)

## First run

**Menu:** `IntelliVerseX → Control Center`

Paste Game ID → add Bootstrap → Play.

## Advanced

**Menu:** `IntelliVerseX → Advanced Setup`

Three tabs:

1. **Dependencies** — core/optional UPM status, install, Asset Store links  
2. **Project** — project validation + define symbols  
3. **Extras** — demo scenes, wipe local data  

Implementation: `IVXAdvancedSetup.cs` + `IVXDependencies.cs`.

## Maintainers

Export, docs, emoji tools, define-symbol helpers — under `IntelliVerseX → Maintainers`.

The old fat `IVXSDKSetupWizard` EditorWindow was removed; only an obsolete static forwarder remains.

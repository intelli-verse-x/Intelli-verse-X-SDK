# IntelliVerseX SDKs

One folder per engine (except Unity’s UPM package, which lives at repo `Packages/` for Git URL installs).

| Folder | Platform | Install |
|--------|----------|---------|
| `../Packages/com.intelliversex.sdk` | Unity UPM | `?path=Packages/com.intelliversex.sdk` |
| `unity/editor` | Unity Hub (sandbox, not for consumers) | Open this folder in Hub |
| `javascript` | npm / TypeScript | `npm install @intelliversex/sdk` |
| `web3` | JS + wallet | same family as javascript |
| `unreal` | Unreal plugin | add this folder as a plugin |
| `godot` | Godot addon | copy into `res://addons/` |
| `flutter` | Dart | `path: SDKs/flutter` in pubspec |
| `java` | Android / JVM | Gradle project in this folder |
| `cpp` | Native | CMake in this folder |
| `defold` | Defold | this folder |
| `roblox` | Roblox | this folder |
| `cocos2dx` | Cocos2d-x | this folder |
| `visionos` | visionOS | this folder |

Details: [docs/architecture/REPO_LAYOUT.md](../docs/architecture/REPO_LAYOUT.md) · Optional Unity add-ons: [docs/OPTIONAL_MODULES.md](../docs/OPTIONAL_MODULES.md)

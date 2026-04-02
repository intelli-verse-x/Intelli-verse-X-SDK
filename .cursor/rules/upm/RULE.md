# UPM Package Rules

## Applies To
Files related to Unity Package Manager packaging.

## Rules

1. **Version**: Follow SemVer (MAJOR.MINOR.PATCH) in `package.json`
2. **Assembly Definitions**: Every module needs a `.asmdef` file
3. **Samples**: Place in `Samples~/` (excluded from package by default)
4. **Dependencies**: Declare in `package.json` dependencies section
5. **External Dependencies**: Document in `_ivx_externalDependencies` section
6. **Changelog**: Update `CHANGELOG.md` for every version bump
7. **Min Unity Version**: `2023.3` (Unity 2023 LTS)
8. **Package Name**: `com.intelliversex.sdk`

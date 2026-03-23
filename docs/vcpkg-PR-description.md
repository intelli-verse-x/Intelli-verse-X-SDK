# vcpkg PR description (for opening and PR review)

Use this as the PR description when opening a pull request to [microsoft/vcpkg](https://github.com/microsoft/vcpkg). It is written for both submitters and reviewers.

---

## PR title

**Add intelliversex-cpp port (1.5.0)**

---

## Description

New port for the **IntelliVerseX C/C++ SDK** (intelliversex-cpp): auth, backend via Nakama, analytics, social, and monetization for game development.

| Field | Value |
|-------|--------|
| **Upstream** | https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK |
| **Version** | 1.5.0 (tag `v1.5.0`) |
| **License** | MIT |

### Dependency: nakama-sdk

This port depends on **nakama-sdk**, which is **not** in the main vcpkg registry. The port README documents how to obtain it:

- **[Heroic Labs nakama-vcpkg-registry](https://github.com/heroiclabs/nakama-vcpkg-registry)** — add as a custom registry in `vcpkg-configuration.json`, or  
- An overlay port that provides `nakama-sdk`.

Until nakama-sdk is in the main registry, consumers must configure the Heroic registry or use an overlay (see port README).

---

## New port checklist (for PR review)

- [x] Changes comply with the [maintainer guide](https://github.com/microsoft/vcpkg-docs/blob/main/vcpkg/contributing/maintainer-guide.md).
- [x] The packaged project shows strong association with the chosen port name (GitHub: [Intelli-verse-X/Intelli-verse-X-Unity-SDK](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK)).
- [x] Optional dependencies of the build are controlled by the port (tests/examples disabled via CMake options).
- [x] The versioning scheme in `vcpkg.json` matches upstream (1.5.0).
- [x] The license declaration in `vcpkg.json` matches upstream (MIT).
- [x] The installed "copyright" file matches upstream (LICENSE from repo root).
- [x] The source code is from the authoritative upstream (GitHub release tag v1.5.0).
- [x] The version database was updated with `vcpkg x-add-version intelliversex-cpp`.
- [x] Exactly one version is added in the modified versions file.

---

## Reviewer notes

- **Build:** CMake with `IVX_BUILD_TESTS=OFF`, `IVX_BUILD_EXAMPLES=OFF`. C++17.
- **Testing:** `vcpkg install intelliversex-cpp:x64-windows` requires nakama-sdk (Heroic registry or overlay); see port README.
- **CI:** If baseline entries are needed for this port, they can be added in a follow-up or as requested by maintainers.

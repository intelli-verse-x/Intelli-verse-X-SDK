# vcpkg PR — Preview (copy into the PR when opening)

Use the text below as the PR description when you open a pull request to [microsoft/vcpkg](https://github.com/microsoft/vcpkg) for the `intelliversex-cpp` port.

---

## Title

**Add intelliversex-cpp port (1.5.0)**

---

## Body (paste below the line)

---

Adds the **intelliversex-cpp** port for the IntelliVerseX C/C++ SDK (auth, backend via Nakama, analytics, social, monetization for game development).

- **Upstream:** https://github.com/Intelli-verse-X/Intelli-verse-X-SDK  
- **Version:** 1.5.0 (tag `v1.5.0`)  
- **License:** MIT  

### Dependency: nakama-sdk

This port depends on **nakama-sdk**, which is not in the main vcpkg registry. The port README documents how to obtain it:

- **[Heroic Labs nakama-vcpkg-registry](https://github.com/heroiclabs/nakama-vcpkg-registry)** — add as a custom registry in `vcpkg-configuration.json`, or  
- An overlay port that provides `nakama-sdk`.

Until/unless nakama-sdk is added to the main registry, consumers must configure the Heroic registry or use an overlay (see port README).

---

### New port checklist

- [x] Changes comply with the [maintainer guide](https://github.com/microsoft/vcpkg-docs/blob/main/vcpkg/contributing/maintainer-guide.md).
- [x] The packaged project shows strong association with the chosen port name (GitHub: [Intelli-verse-X/Intelli-verse-X-SDK](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK)).
- [x] Optional dependencies of the build are controlled by the port (tests/examples disabled via `-DIVX_BUILD_TESTS=OFF` / `-DIVX_BUILD_EXAMPLES=OFF`).
- [x] The versioning scheme in `vcpkg.json` matches upstream (1.5.0).
- [x] The license declaration in `vcpkg.json` matches upstream (MIT).
- [x] The installed "copyright" file matches upstream (LICENSE from repo root).
- [x] The source code is from the authoritative upstream (GitHub release tag v1.5.0).
- [x] The version database was updated with `vcpkg x-add-version intelliversex-cpp`.
- [x] Exactly one version is added in the modified versions file.

---

### Testing

- `vcpkg install intelliversex-cpp:x64-windows` (with nakama-sdk from Heroic registry or overlay) completes successfully.
- Port README includes instructions for the Heroic Labs registry and overlay.

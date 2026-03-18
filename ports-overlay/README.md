# vcpkg overlay ports

Use this overlay so vcpkg can build **nakama-sdk** and **intelliversex-cpp** without adding external registries.

## What’s here

- **nakama-sdk** — Builds [heroiclabs/nakama-cpp](https://github.com/heroiclabs/nakama-cpp) v2.9.0 with a small patch so it uses vcpkg’s toolchain when built as a port.
- **intelliversex-cpp** — Use the port from the main repo: copy `ports/intelliversex-cpp/` from this repo into your vcpkg clone (or use your vcpkg fork that already has it).

## How to use

From your vcpkg clone (or project that uses vcpkg):

```powershell
# Path to this overlay (adjust if your repo is elsewhere)
$overlay = "D:\work\Unityprojects\Intelli-verse-X-Unity-SDK\ports-overlay"

# Install nakama-sdk from the overlay
.\vcpkg install nakama-sdk:x64-windows --overlay-ports=$overlay

# Install intelliversex-cpp (needs nakama-sdk; use same overlay so nakama-sdk is found)
.\vcpkg install intelliversex-cpp:x64-windows --overlay-ports=$overlay
```

If intelliversex-cpp is in your vcpkg fork under `ports/intelliversex-cpp/`, you only need the overlay for **nakama-sdk**. Then:

```powershell
.\vcpkg install intelliversex-cpp:x64-windows --overlay-ports=$overlay
```

vcpkg will build nakama-sdk from the overlay first, then intelliversex-cpp.

## Can we PR intelliversex-cpp to the main vcpkg?

**No, not with this overlay alone.** The main [microsoft/vcpkg](https://github.com/microsoft/vcpkg) registry does not accept ports that depend on packages that are not in the **main** registry. So:

- This overlay is for **local/private use** (your machine or your team).
- To get **intelliversex-cpp** into the main vcpkg repo, **nakama-sdk** must be in the main registry first (e.g. Heroic Labs or someone else submits it). After that, you can open a PR to add intelliversex-cpp and it can depend on nakama-sdk.
- Until then: use this overlay, or use [Heroic’s registry](https://github.com/heroiclabs/nakama-vcpkg-registry) + a custom registry for intelliversex-cpp, as in the earlier guidance.

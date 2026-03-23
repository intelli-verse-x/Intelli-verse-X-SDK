# vcpkg: Port, PR, and usage

## What’s in the repo

The vcpkg port lives under:

- **`ports/intelliversex-cpp/`**
  - `vcpkg.json` – name, version, description, dependency on `nakama-sdk`
  - `portfile.cmake` – fetch from GitHub, configure/build/install the C++ SDK
  - `README.md` – port-level notes and dependency options

## Option B: Fork vcpkg and submit a PR

### 1. Fork and clone vcpkg

- Fork [microsoft/vcpkg](https://github.com/microsoft/vcpkg).
- Clone your fork and create a branch, e.g. `add-intelliversex-cpp`.

### 2. Add the port

- Copy the contents of **`ports/intelliversex-cpp/`** from this repo into your vcpkg clone:

  ```text
  vcpkg/
  └── ports/
      └── intelliversex-cpp/
          ├── vcpkg.json
          ├── portfile.cmake
          └── README.md
  ```

### 3. Version and SHA512 (required for PR)

- The port uses version **1.5.0** (tag `v1.5.0`). Ensure this repo has that tag (see below).
- From the **root of your vcpkg clone** run:

  ```bash
  vcpkg x-add-version intelliversex-cpp
  ```

- This updates the versions database (and SHA512). Commit any new/updated files under `ports/intelliversex-cpp/` and `versions/`.

**Creating the tag `v1.5.0` in this repo (if not already present):**

```bash
git tag v1.5.0
git push origin v1.5.0
```

If you prefer the tag to point at a specific commit, use `git tag v1.5.0 <commit-hash>` first. The vcpkg port will fetch the source from this tag.

### 4. Handle `nakama-sdk` dependency

- **nakama-sdk** is not in the main vcpkg registry; it’s in [heroiclabs/nakama-vcpkg-registry](https://github.com/heroiclabs/nakama-vcpkg-registry).
- In your PR description, state that:
  - intelliversex-cpp depends on **nakama-sdk**.
  - Users can get it via Heroic’s registry or an overlay until/if it’s added to the main vcpkg registry.
- Optionally, in the same PR or a follow-up, document in the port’s README how to add Heroic’s registry or use an overlay for nakama-sdk.

### 5. Test locally

- Add Heroic’s registry (or an overlay with nakama-sdk), then:

  ```bash
  vcpkg install intelliversex-cpp
  ```

- Confirm build and install succeed on at least one triplet (e.g. `x64-windows`).

### 6. Open the PR

- Push your branch and open a pull request to **microsoft/vcpkg**.
- Fill out the PR template and link to this SDK repo and, if useful, to the nakama-vcpkg-registry.

## Using the port (after it’s available)

- With Heroic’s registry (or overlay) configured so `nakama-sdk` is available:

  ```bash
  vcpkg install intelliversex-cpp
  ```

- In a CMake project:

  ```cmake
  find_package(nakama-sdk CONFIG REQUIRED)
  find_package(intelliversex CONFIG REQUIRED)  # if we add config in the future; else use find_path/find_library
  target_link_libraries(your_app PRIVATE intelliversex nakama-sdk)
  ```

- If the port doesn’t install a CMake config, use the include and library paths reported by vcpkg and link `intelliversex` and `nakama-sdk` as above.

## Summary checklist

| Step | Action |
|------|--------|
| 1 | Fork vcpkg, create branch |
| 2 | Copy `ports/intelliversex-cpp/` into the fork |
| 3 | Ensure tag `v1.5.0` exists (see below), run `vcpkg x-add-version intelliversex-cpp`, commit |
| 4 | Document nakama-sdk (Heroic registry or overlay) in PR and port README |
| 5 | Test `vcpkg install intelliversex-cpp` |
| 6 | Open PR to microsoft/vcpkg |

## Results and remaining work

- **Task 4 (done):** Port README in the vcpkg repo documents nakama-sdk and includes a copy-paste `vcpkg-configuration.json` example (Heroic registry). A PR description template is in **`docs/vcpkg-PR-description.md`** — paste it into the GitHub PR when opening.
- **Task 5 (local test):** To test `vcpkg install intelliversex-cpp`, add Heroic’s registry (see port README or the example above). In a fork with the port committed, use a `vcpkg-configuration.json` that sets `default-registry` (e.g. `kind: "builtin"`, baseline = your commit that includes the port) and adds the Heroic registry; then run `vcpkg install intelliversex-cpp:x64-windows`. Build can take a while (abseil, nakama-sdk, etc.).
- **Task 6 (you do this):** Push your vcpkg branch (e.g. `add-intelliversex-cpp`) and open a pull request to **microsoft/vcpkg**. Use the body from `docs/vcpkg-PR-description.md` and add any links (e.g. this SDK repo, nakama-vcpkg-registry).

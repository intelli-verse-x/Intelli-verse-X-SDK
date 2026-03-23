# Conan recipe — task list and how-to

Use this list to create, test, and (optionally) submit the IntelliVerseX C++ SDK as a Conan package.

---

## Files added (for quick reference)

| Path | Purpose |
|------|--------|
| `SDKs/cpp/conanfile.py` | Conan 2 recipe (name: intelliversex-cpp, version: 1.5.0, requires nakama-sdk) |
| `SDKs/cpp/recipes/nakama-sdk/conanfile.py` | Local recipe for nakama-sdk (pre-built from Heroic releases; not for Conan Center) |
| `SDKs/cpp/test_package/conanfile.py` | Test consumer used by `conan create` |
| `SDKs/cpp/test_package/CMakeLists.txt` | CMake for test_package |
| `SDKs/cpp/test_package/src/example.cpp` | Minimal app that includes ivx.h and links intelliversex |
| `docs/conan-recipe-tasks.md` | This file — task list and how-to |
| `docs/conan-center-submission.md` | Step-by-step Conan Center submission guide |

---

## Task checklist

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | Add Conan recipe (`conanfile.py`) in repo | ✅ Done | `SDKs/cpp/conanfile.py` |
| 2 | Add `test_package` for `conan create` | ✅ Done | `SDKs/cpp/test_package/` |
| 3 | Document dependency on `nakama-sdk` | ✅ Done | See "Dependency: nakama-sdk" below |
| 4 | Test locally: `conan create .` | ⬜ You do | Requires `nakama-sdk` in Conan cache or remotes |
| 5 | (Optional) Submit to Conan Center | ⬜ You do | Fork conan-center-index, add recipe, open PR |

---

## Where the recipe lives

- **In this repo:** `SDKs/cpp/conanfile.py` (and `SDKs/cpp/test_package/`).
- **For Conan Center:** You would copy/adapt the recipe into [conan-center-index](https://github.com/conan-io/conan-center-index) (e.g. `recipes/intelliversex-cpp/all/conanfile.py`) and use `source()` to fetch from GitHub tag instead of `exports_sources`.

---

## Dependency: nakama-sdk

The C++ SDK requires **nakama-sdk**. It is **not** in Conan Center (as of this writing). You have two options:

1. **Use a Conan recipe for nakama-sdk**  
   If you have a Conan package named `nakama-sdk` (e.g. from a custom remote or a recipe you export locally), add that remote or run `conan create` for nakama-sdk first so `intelliversex-cpp` can resolve `requires = "nakama-sdk/2.8.4"` (or the version you use).

2. **Build nakama-sdk yourself and export**  
   Use Heroic’s nakama-cpp or a wrapper recipe to create a Conan package `nakama-sdk`, then build/export it so it’s in your Conan cache before building `intelliversex-cpp`.

Until `nakama-sdk` is in Conan Center, document in any PR that **intelliversex-cpp** depends on it and that users must get it from a custom remote or a separate recipe.

---

## Commands (quick reference)

**1. Create nakama-sdk (local pre-built recipe; run once):**

```powershell
cd D:\work\Unityprojects\Intelli-verse-X-Unity-SDK
conan create SDKs/cpp/recipes/nakama-sdk --version=2.9.0 -s build_type=Release
```

**2. Create intelliversex-cpp (use C++17 for nakama headers):**

```powershell
conan create SDKs/cpp --version=1.5.0 -s build_type=Release -s compiler.cppstd=17
```

Or with options:

```powershell
conan create SDKs/cpp --version=1.5.0 -s build_type=Release -s compiler.cppstd=17 -o intelliversex-cpp/*:shared=False
```

**Note:** If `conan` is not on PATH, use the full path to Conan (e.g. after `pip install conan` on Windows):  
`& "C:\Users\<You>\AppData\Local\Python\pythoncore-3.14-64\Scripts\conan.exe" create ...`  
Or run `conan profile detect` once to create the default profile.

**List the package:**

```bash
conan list "intelliversex-cpp/*"
```

**Use in a consumer project (conanfile.txt or conanfile.py):**

```ini
[requires]
intelliversex-cpp/1.5.0
nakama-sdk/2.8.4
```

---

## Task details

### Task 1 – Recipe in repo ✅

- File: `SDKs/cpp/conanfile.py`
- Name: `intelliversex-cpp`, version: `1.5.0` (aligned with vcpkg port).
- Build: CMake, tests/examples off; optional `shared` and `fPIC`.
- Requires: `nakama-sdk/2.8.4` (or compatible); see "Dependency: nakama-sdk" above.

### Task 2 – test_package ✅

- Folder: `SDKs/cpp/test_package/`
- Contains a minimal app that `#include`s IntelliVerseX and links `intelliversex` and `nakama-sdk`.
- Used automatically by `conan create .` to verify the package.

### Task 3 – Document nakama-sdk ✅

- This doc and comments in `conanfile.py` describe the `nakama-sdk` requirement and that it is not in Conan Center.

### Task 4 – Test locally ✅ (done in this session)

1. Conan 2 installed (`pip install conan` or `py -m pip install conan`). Run `conan profile detect` once if needed.
2. **nakama-sdk**: Use the local pre-built recipe:  
   `conan create SDKs/cpp/recipes/nakama-sdk --version=2.9.0 -s build_type=Release`
3. Create intelliversex-cpp (use C++17):  
   `conan create SDKs/cpp --version=1.5.0 -s build_type=Release -s compiler.cppstd=17`
4. Build and test_package should succeed; you should see `intelliversex-cpp test_package: OK`.

### Task 5 – Submit to Conan Center (optional)

1. Read [Conan Center contribution guidelines](https://github.com/conan-io/conan-center-index/blob/master/docs/how_to_add_packages.md).
2. Fork conan-center-index, create a branch.
3. Add a recipe under e.g. `recipes/intelliversex-cpp/all/` that:
   - Uses `source()` to download from GitHub (e.g. tag `v1.5.0`).
   - Keeps the same CMake build and options as in-repo.
4. Handle `nakama-sdk`: either add it to Conan Center in a separate PR or document in the intelliversex-cpp PR that it is required and not yet in Center.
5. Open a PR; CI will build and test.

---

## Summary

| Step | Action |
|------|--------|
| 1 | Recipe and test_package are in `SDKs/cpp/`. |
| 2 | Get `nakama-sdk` into your Conan cache (remote or local recipe). |
| 3 | Run `conan create . --version=1.5.0` from `SDKs/cpp`. |
| 4 | Optionally submit to Conan Center (fork, add recipe, open PR). |

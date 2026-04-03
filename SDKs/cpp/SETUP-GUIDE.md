# Easiest setup: IntelliVerseX C++ SDK (Windows)

**No MSYS2. No building nakama from source.** Use pre-built nakama + Visual Studio.

---

## What you need

| What | Where to get it |
|------|-----------------|
| **Visual Studio 2022** (or 2019) | [Download](https://visualstudio.microsoft.com/downloads/) — install **Desktop development with C++** |
| **CMake** | Install with VS, or [cmake.org](https://cmake.org/download/) |

---

## Where everything lives

| What | Path |
|------|------|
| **Your repo** | `D:\work\Unityprojects\Intelli-verse-X-SDK` |
| **Pre-built nakama (you extract here)** | `D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\nakama-cpp-install` |
| **IntelliVerseX C++ SDK** | `D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\cpp` |

---

## Which app to use

| Task | Use |
|------|-----|
| **Edit code** | VS Code or Cursor |
| **Run build commands** | **Developer PowerShell for VS 2022** (Start menu) |

Do **not** use MSYS2 or VS Code terminal. Use **Developer PowerShell** only.

---

## Step 1: Download pre-built nakama (no build)

1. Open: https://github.com/heroiclabs/nakama-cpp/releases
2. Download **win-x64-MinSizeRel.zip** (about 5 MB)
3. Extract the zip. You get a folder (e.g. `win-x64-MinSizeRel`).
4. Rename or move that folder to:
   ```
   D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\nakama-cpp-install
   ```
5. Check that this path exists:
   ```
   D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\nakama-cpp-install\lib
   D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\nakama-cpp-install\include\nakama-cpp
   ```

---

## Step 2: Build IntelliVerseX C++ SDK

1. Open **Developer PowerShell for VS 2022** from the Start menu.
2. Run these commands one by one:

```powershell
cd D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\cpp
```

```powershell
cmake -B build -DCMAKE_BUILD_TYPE=Release -DIVX_NAKAMA_DIR="D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\nakama-cpp-install"
```

```powershell
cmake --build build --config Release
```

Done. If there are no errors, the SDK compiled correctly.

---

## Single-line (copy-paste in Developer PowerShell)

```powershell
cd D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\cpp; cmake -B build -DCMAKE_BUILD_TYPE=Release -DIVX_NAKAMA_DIR="D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\nakama-cpp-install"; cmake --build build --config Release
```

---

## If the zip has a different structure

Some nakama zips extract to a folder like `win-x64-MinSizeRel`. If yours does:

- Put the **contents** of that folder (the `lib` and `include` folders) directly into:
  ```
  D:\work\Unityprojects\Intelli-verse-X-SDK\SDKs\nakama-cpp-install\
  ```
- So you have `nakama-cpp-install\lib\` and `nakama-cpp-install\include\nakama-cpp\`.

---

## Quick reference

| Do this | Use this |
|--------|----------|
| Edit code | VS Code or Cursor |
| Build | **Developer PowerShell for VS 2022** |
| nakama location | `SDKs\nakama-cpp-install` |
| No MSYS2 | Use Visual Studio tools only |

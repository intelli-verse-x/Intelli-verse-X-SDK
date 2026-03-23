# Runbook: Push server config + 5.2.0 by SDK branch (Approach A)

**Goal:** Update default server (host, port, key, SSL) and set version 5.2.0 on each SDK **only on its own branch**, so existing PRs get the right changes and future devs use the same branch names.

**Your setup:** Fork = `hasanraza31/Intelli-verse-X-Unity-SDK`, PRs into `intelli-verse-x/Intelli-verse-X-Unity-SDK`. You (or senior) merge after review.

**Reminder (do later):** After all branches are merged, do “Option 1” publish steps on each site where SDKs are published (npm, Maven Central, Godot Asset Library, etc.). Skipped for now.

---

## Prerequisites

- All server-config and 5.2.0 changes are in your **current working tree** (SDKs + ports).
- Remote `origin` = your fork (`https://github.com/hasanraza31/Intelli-verse-X-Unity-SDK.git`). If `origin` points to the org repo, add your fork as a remote, e.g. `git remote add fork https://github.com/hasanraza31/Intelli-verse-X-Unity-SDK.git` and use `fork` instead of `origin` below.
- You are on branch `add-java-sdk` with uncommitted changes.

---

## Step 0: Save all changes on a single ref (so we can copy per-SDK files)

We need one commit that contains every server-config and 5.2.0 change, so we can “pluck” only the right files onto each branch.

```powershell
cd "d:\work\Unityprojects\Intelli-verse-X-Unity-SDK"

# Create a temporary branch from current and commit everything (server config + 5.2.0)
git checkout -b wip-server-config-5.2.0
git add SDKs/cpp/ SDKs/defold/ SDKs/godot/ SDKs/java/ SDKs/javascript/ SDKs/unreal/ ports/
git status
# Confirm only SDK + ports changes (no unwanted Assets/ or .github/ if you don't want them)
git add SDKs/ ports/
git commit -m "chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; version 5.2.0 (all SDKs, for branch split)"
```

Do **not** push `wip-server-config-5.2.0` unless you want a backup on the remote. You will use it only locally to copy files.

---

## Step 1: add-cpp-sdk

Only C++ SDK + vcpkg port; existing PR gets this commit.

```powershell
git checkout add-cpp-sdk
git pull origin add-cpp-sdk
git checkout wip-server-config-5.2.0 -- SDKs/cpp/include/intelliversex/ivx_config.h SDKs/cpp/README.md SDKs/cpp/examples/main.cpp SDKs/cpp/CMakeLists.txt ports/intelliversex-cpp/portfile.cmake ports/intelliversex-cpp/vcpkg.json
git status
git add SDKs/cpp/ ports/
git commit -m "chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; release 5.2.0"
git push origin add-cpp-sdk
```

---

## Step 2: add-defold-sdk

Only Defold SDK files.

```powershell
git checkout add-defold-sdk
git pull origin add-defold-sdk
git checkout wip-server-config-5.2.0 -- SDKs/defold/intelliversex/ivx.lua SDKs/defold/examples/basic_example.lua SDKs/defold/tests/test_ivx.lua
git add SDKs/defold/
git commit -m "chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; release 5.2.0"
git push origin add-defold-sdk
```

---

## Step 3: add-godot-sdk

Only Godot SDK files.

```powershell
git checkout add-godot-sdk
git pull origin add-godot-sdk
git checkout wip-server-config-5.2.0 -- SDKs/godot/examples/basic_example.gd SDKs/godot/addons/intelliversex/core/ivx_manager.gd SDKs/godot/addons/intelliversex/tests/test_ivx.gd
git add SDKs/godot/
git commit -m "chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; release 5.2.0"
git push origin add-godot-sdk
```

---

## Step 4: add-java-sdk

Only Java SDK files. This branch already has an open PR.

```powershell
git checkout add-java-sdk
git pull origin add-java-sdk
git checkout wip-server-config-5.2.0 -- SDKs/java/src/main/java/com/intelliversex/sdk/core/IVXConfig.java SDKs/java/examples/BasicExample.java SDKs/java/README.md SDKs/java/build.gradle
git add SDKs/java/
git commit -m "chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; release 5.2.0"
git push origin add-java-sdk
```

---

## Step 5: add-javascript-sdk (branch does not exist yet)

Create the branch from `main`, then apply only JavaScript SDK changes.

```powershell
git checkout main
git pull origin main
git checkout -b add-javascript-sdk
git checkout wip-server-config-5.2.0 -- SDKs/javascript/src/IVXConfig.ts SDKs/javascript/src/types.ts SDKs/javascript/README.md SDKs/javascript/examples/node-example.ts SDKs/javascript/examples/browser.html SDKs/javascript/package.json
git add SDKs/javascript/
git commit -m "chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; release 5.2.0"
git push origin add-javascript-sdk
```

Then open a new PR: `hasanraza31/add-javascript-sdk` → `intelli-verse-x/main`.

---

## Step 6: add-unreal-sdk (branch does not exist yet)

Create the branch from `main`, then apply only Unreal SDK changes.

```powershell
git checkout main
git pull origin main
git checkout -b add-unreal-sdk
git checkout wip-server-config-5.2.0 -- SDKs/unreal/Source/IntelliVerseX/Public/IVXConfig.h SDKs/unreal/Examples/ExampleGameMode.cpp SDKs/unreal/IntelliVerseX.uplugin
git add SDKs/unreal/
git commit -m "chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; release 5.2.0"
git push origin add-unreal-sdk
```

Then open a new PR: `hasanraza31/add-unreal-sdk` → `intelli-verse-x/main`.

---

## After pushing

| Branch            | PR exists? | Action |
|-------------------|------------|--------|
| add-cpp-sdk       | Yes        | PR will show the new commit; get review, then merge (you or senior). |
| add-defold-sdk    | No (you didn’t list) | Open PR from `add-defold-sdk` → `main` if you want it merged. |
| add-godot-sdk     | Yes        | Same as add-cpp-sdk. |
| add-java-sdk      | Yes        | Same as add-cpp-sdk. |
| add-javascript-sdk| No (new branch) | Open new PR → `main`. |
| add-unreal-sdk    | No (new branch) | Open new PR → `main`. |

---

## Optional: delete local temp branch

After all branches are pushed and you don’t need the ref anymore:

```powershell
git checkout add-java-sdk
git branch -D wip-server-config-5.2.0
```

---

## Reminder (later): Option 1 – publish 5.2.0 on each site

When you’re ready to publish the new version on each site (npm, Maven Central, Godot Asset Library, etc.), we’ll do that in a separate step. Skipped for now as requested.

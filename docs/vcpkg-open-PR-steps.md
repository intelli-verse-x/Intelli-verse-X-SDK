# Open the vcpkg PR (Task 6)

## Branches in your vcpkg repo

- **`add-intelliversex-cpp`** — correct branch (has the port and version files). Use this for the PR.
- **`my-branch`** — created by mistake; you can delete it locally (and on remote if you pushed it) after the PR is opened.

## What’s already done

- On **`add-intelliversex-cpp`**: port and version files are committed (`Add intelliversex-cpp port (1.5.0)`).
- Port README documents nakama-sdk and Heroic registry.
- **`add-intelliversex-cpp`** is already pushed to `origin` (remotes/origin/add-intelliversex-cpp exists).

---

## 1. Verify install finished (optional)

To confirm `vcpkg install intelliversex-cpp:x64-windows` completed successfully:

**Option A – Check if the port is installed**
```powershell
cd D:\work\Unityprojects\vcpkg
.\vcpkg.exe list intelliversex-cpp:x64-windows
```
If you see `intelliversex-cpp:x64-windows@1.5.0`, the install finished.

**Option B – Run install again**
```powershell
cd D:\work\Unityprojects\vcpkg
.\vcpkg.exe install intelliversex-cpp:x64-windows
```
- If it says “already installed” or completes quickly, the port was already built.
- If it starts building (abseil, nakama-sdk, etc.), let it run until you see a line like `Total install time: ...` and `intelliversex-cpp:x64-windows` in the list.

---

## 2. Push the vcpkg branch

Use the **correct** branch: **`add-intelliversex-cpp`**.

```powershell
cd D:\work\Unityprojects\vcpkg
git checkout add-intelliversex-cpp
git push -u origin add-intelliversex-cpp
```
If you have new commits only on this branch, a simple `git push origin add-intelliversex-cpp` is enough. If your fork’s remote isn’t `origin`, use that remote name instead.

**To remove the mistaken branch (optional):**
- **Local only:**  
  `git branch -d my-branch`
- **If you already pushed `my-branch` and want to remove it from GitHub:**  
  `git push origin --delete my-branch`

---

## 3. What you do next (open PR)

2. **Open the PR on GitHub**
   - Go to https://github.com/microsoft/vcpkg
   - Use “Compare & pull request” for `add-intelliversex-cpp`, or “New pull request” and choose your fork and that branch.

3. **Fill the PR description**
   - **Quick copy:** Use **`docs/vcpkg-PR-preview.md`** — copy the "Body" section into the PR description; use that file's title for the PR title. **Full reference:** **`docs/vcpkg-PR-description.md`** has the same content structured for PR review (including reviewer notes).
   - Links to the SDK and Heroic registry are included in the PR description.

4. **Submit** (or create as Draft if you’re still testing).

## Note

- **Do not commit** `vcpkg-configuration.json` in the vcpkg repo (it’s for local testing and is in `.gitignore`).
- If the local install (`vcpkg install intelliversex-cpp:x64-windows`) is still running, you can let it finish to confirm the port builds; the PR can be opened either way.

# Push SDK Branches to Your Fork

Run these from the **repo root** on your machine (where you are logged into GitHub).  
Push from this environment failed with authentication, so use your own machine (GitHub CLI, credential manager, or PAT).

**Remote:** Use your fork remote name (e.g. `fork`). If you don’t have it:

```bash
git remote add fork https://github.com/hasanraza31/Intelli-verse-X-Unity-SDK.git
```

## Push each SDK branch

```bash
git push fork add-cpp-sdk
git push fork add-defold-sdk
git push fork add-godot-sdk
git push fork add-java-sdk
git push fork add-javascript-sdk
git push fork add-unreal-sdk
```

If a branch already exists on the fork and you’ve merged main locally, a normal push is enough. Use `--force-with-lease` only if you’re sure (e.g. after a rebase).

If you get "rejected (non-fast-forward)", pull first, e.g.:

```bash
git checkout add-cpp-sdk
git pull fork add-cpp-sdk --rebase
git push fork add-cpp-sdk
```

## Then create or update PRs

- Go to **https://github.com/hasanraza31/Intelli-verse-X-Unity-SDK**.
- For each branch, open a PR **into** `intelli-verse-x/Intelli-verse-X-Unity-SDK` → `main`.
- Base: `main`, compare: your branch (e.g. `add-unreal-sdk`).

# Push SDK Branches to Your Fork

Run these from the **repo root** on your machine (where you are logged into GitHub).  
Use your fork remote name (e.g. `fork` if you added it as in the runbook).

## One-time: ensure fork remote

```bash
git remote -v
```

If you don’t have your fork as a remote:

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

If a branch already exists on the fork and you’ve merged main locally:

```bash
git push fork <branch-name>
```

Use `--force-with-lease` only if you’re sure you want to overwrite the remote branch (e.g. after a rebase); otherwise a normal push is enough after merging main.

## Then create or update PRs

- Go to **https://github.com/hasanraza31/Intelli-verse-X-Unity-SDK**.
- For each branch, open a PR **into** `intelli-verse-x/Intelli-verse-X-Unity-SDK` `main`.
- Base: `main`, compare: your branch (e.g. `add-unreal-sdk`).

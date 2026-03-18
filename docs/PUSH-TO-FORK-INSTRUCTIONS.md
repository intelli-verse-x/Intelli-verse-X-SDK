# Push to your fork (hasanraza31) — run these on your machine

Git **push** from this environment failed with:  
`Authentication failed for 'https://github.com/hasanraza31/Intelli-verse-X-Unity-SDK.git/'`  
So you need to push from your own machine where you are logged in (GitHub CLI, credential manager, or PAT).

**Remote:** Your fork is already added as `fork`.  
If you use a different remote name for your fork, replace `fork` with that name below.

---

## Commands to run (in repo root)

```powershell
cd "d:\work\Unityprojects\Intelli-verse-X-Unity-SDK"

# Push each branch to your fork (you can run these one by one)
git push fork add-cpp-sdk
git push fork add-defold-sdk
git push fork add-godot-sdk
git push fork add-java-sdk
git push fork add-javascript-sdk
git push fork add-unreal-sdk
```

If any branch does not exist on the fork yet, Git will create it. If you get "rejected (non-fast-forward)", pull first, e.g.:

```powershell
git checkout add-cpp-sdk
git pull fork add-cpp-sdk --rebase
git push fork add-cpp-sdk
```

Then open (or update) PRs from **hasanraza31/Intelli-verse-X-Unity-SDK** → **intelli-verse-x/Intelli-verse-X-Unity-SDK** for each branch.

---

## Note about add-javascript-sdk

`add-javascript-sdk` was created from the branch that was checked out when `git checkout main` failed (due to local changes). So it may be based on **add-java-sdk** instead of **main**. When you open a PR from `add-javascript-sdk` into `main`, the diff will still show only the JavaScript SDK commit if the rest of the history matches main; if the PR shows extra files, create a new branch from `main` and cherry-pick only the JS commit, then push that branch and open the PR from it.

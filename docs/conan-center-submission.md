# Submitting intelliversex-cpp to Conan Center

Step-by-step guide to submit the IntelliVerseX C++ SDK to [Conan Center](https://conan.io/center) so users can install it with `conan install intelliversex-cpp/1.5.0`.

---

## Important: nakama-sdk dependency

**intelliversex-cpp** depends on **nakama-sdk**, which is **not** in Conan Center. You have two paths:

1. **Submit nakama-sdk first** (recommended): Add a recipe for nakama-sdk to conan-center-index; then submit intelliversex-cpp with `requires = "nakama-sdk/2.9.0"`.
2. **Submit intelliversex-cpp and explain**: Open the PR and state in the description that it depends on nakama-sdk. Be prepared to submit nakama-sdk in a separate PR if asked.

---

## Prerequisites

- [ ] Git and a GitHub account.
- [ ] Sign the [Conan Center CLA](https://github.com/conan-io/conan-center-index/blob/master/docs/how_to_add_packages.md#contributor-license-agreement-cla) (required on first PR).

The SDK and nakama-sdk require **C++17**; the recipe is set up accordingly.

---

## Fork actions (step by step)

Do these in order. Replace **YOUR_USERNAME** with your GitHub username.

---

### 1. Fork on GitHub

- Go to **https://github.com/conan-io/conan-center-index**
- Click **Fork** (top right).
- You get: `https://github.com/YOUR_USERNAME/conan-center-index`

---

### 2. Clone your fork and add upstream

```bash
git clone https://github.com/YOUR_USERNAME/conan-center-index.git
cd conan-center-index
git remote add upstream https://github.com/conan-io/conan-center-index.git
git fetch upstream
```

---

### 3. Create the branch from upstream

```bash
git checkout -b add-intelliversex-cpp upstream/master
```

---

### 4. Copy the recipe files from this repo

The Conan Center–ready recipe is in this repository:

- **From:** `docs/conan-center-recipe/all/`
- **To (in your fork):** `recipes/intelliversex-cpp/all/`

**Do this:**

1. Create the folder in your fork:
   ```bash
   mkdir -p recipes/intelliversex-cpp/all
   ```
2. Copy the two files from this repo into that folder:
   - `docs/conan-center-recipe/all/conanfile.py` → `recipes/intelliversex-cpp/all/conanfile.py`
   - `docs/conan-center-recipe/all/conandata.yml` → `recipes/intelliversex-cpp/all/conandata.yml`

**Or on Windows (PowerShell), from this repo root:**

```powershell
$src = "D:\work\Unityprojects\Intelli-verse-X-SDK\docs\conan-center-recipe\all"
$dst = "D:\path\to\conan-center-index\recipes\intelliversex-cpp\all"
New-Item -ItemType Directory -Force -Path $dst
Copy-Item "$src\conanfile.py" "$dst\"
Copy-Item "$src\conandata.yml" "$dst\"
```

(Replace `D:\path\to\conan-center-index` with the path where you cloned your fork.)

---

### 5. Set the SHA256 in conandata.yml

The tarball checksum is required. After the tag **v1.5.0** is on GitHub:

**Linux/macOS:**

```bash
curl -sL -o v1.5.0.tar.gz "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/archive/refs/tags/v1.5.0.tar.gz"
sha256sum v1.5.0.tar.gz
```

**Windows (PowerShell):**

```powershell
Invoke-WebRequest -Uri "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/archive/refs/tags/v1.5.0.tar.gz" -OutFile "v1.5.0.tar.gz" -UseBasicParsing
Get-FileHash -Path "v1.5.0.tar.gz" -Algorithm SHA256 | Select-Object -ExpandProperty Hash
```

Open `recipes/intelliversex-cpp/all/conandata.yml` and replace `REPLACE_WITH_SHA256` with the computed hash (lowercase, no spaces).

---

### 6. Commit and push

```bash
git add recipes/intelliversex-cpp/
git commit -m "Add intelliversex-cpp 1.5.0"
git push -u origin add-intelliversex-cpp
```

---

### 7. Open the PR on GitHub

1. Go to **https://github.com/conan-io/conan-center-index**
2. Use **Compare & pull request** for your branch `add-intelliversex-cpp`, or **New pull request** → base: `conan-io/conan-center-index` **master**, compare: your fork, branch `add-intelliversex-cpp`.
3. **PR description** (copy and adjust):
   - Title: `Add intelliversex-cpp 1.5.0`
   - Body:
     - This adds the **IntelliVerseX C/C++ SDK** (auth, backend, analytics for games).
     - **Dependency:** It requires **nakama-sdk**, which is not yet in Conan Center. [Choose one: "We are submitting nakama-sdk in a separate PR." / "We request consideration to add intelliversex-cpp; we can submit nakama-sdk in a follow-up PR."]
     - Repository: https://github.com/Intelli-verse-X/Intelli-verse-X-SDK
4. Submit (or create as Draft). Conan Center CI will run; if nakama-sdk is missing, the build will fail until it is added.

---

## After acceptance

- The package will appear on Conan Center after the next index update.
- Users: `conan install intelliversex-cpp/1.5.0` (plus nakama-sdk if not pulled as a dependency).
- To add more versions later: update the recipe and `conandata.yml`, then open a new PR.

---

## If the repo was prepared for you (local clone at `conan-center-index`)

A clone at **`D:\work\Unityprojects\conan-center-index`** may already have branch **`add-intelliversex-cpp`** with the recipe committed. In that case:

1. **Fork** https://github.com/conan-io/conan-center-index on GitHub (your account).
2. **Add your fork** and push:
   ```bash
   cd D:\work\Unityprojects\conan-center-index
   git remote add myfork https://github.com/YOUR_USERNAME/conan-center-index.git
   git push -u myfork add-intelliversex-cpp
   ```
3. **Fill SHA256**: Edit `recipes/intelliversex-cpp/all/conandata.yml`, replace `REPLACE_WITH_SHA256` with the real hash (see Step 5 above; tag v1.5.0 must be on GitHub). Then `git add`, `git commit --amend --no-edit`, `git push myfork add-intelliversex-cpp --force`.
4. **Open the PR** on GitHub (Compare & pull request for `add-intelliversex-cpp` → conan-io/conan-center-index master), paste the PR description from above.

---

## Summary checklist

| # | Action |
|---|--------|
| 1 | Fork conan-center-index on GitHub |
| 2 | Clone your fork, add upstream, create branch `add-intelliversex-cpp` (or use existing clone) |
| 3 | Copy `docs/conan-center-recipe/all/*` → `recipes/intelliversex-cpp/all/` (or already done) |
| 4 | Generate SHA256 for v1.5.0 tarball and put it in `conandata.yml` |
| 5 | Commit, push, open PR with description (and nakama-sdk note) |

---

## References

- [Conan Center – How to add packages](https://github.com/conan-io/conan-center-index/blob/master/docs/how_to_add_packages.md)
- [Adding packages (overview)](https://github.com/conan-io/conan-center-index/blob/master/docs/adding_packages/README.md)
- [Developing recipes locally](https://github.com/conan-io/conan-center-index/blob/master/docs/developing_recipes_locally.md)

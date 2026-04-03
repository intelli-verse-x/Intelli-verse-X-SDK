# SDK Branches and Changes — Team Reference

This document explains how the **IntelliVerseX** repo is organized for multi-SDK work and what was done to fix "ahead/behind" branch confusion.

## Branch strategy

- **`main`** — Upstream default. Contains Unity SDK (under `Assets/Intelli-verse-X-SDK/`), docs, and CI. Other SDKs live under `SDKs/` and are developed on **per-SDK branches**.
- **One branch per SDK** — Each platform has its own branch so PRs stay focused and reviewable.

| Branch | SDK | Location |
|--------|-----|----------|
| `add-cpp-sdk` | C++ | `SDKs/cpp/`, `ports/intelliversex-cpp/` |
| `add-defold-sdk` | Defold | `SDKs/defold/` |
| `add-godot-sdk` | Godot | `SDKs/godot/` |
| `add-java-sdk` | Java (Gradle) | `SDKs/java/` |
| `add-javascript-sdk` | JavaScript/TypeScript (npm) | `SDKs/javascript/` |
| `add-unreal-sdk` | Unreal Engine 5 | `SDKs/unreal/` |

## What “one step ahead, one step behind” meant

- SDK branches were created from an older point in history, then **`main`** moved forward (e.g. Unity package layout changes, workflow updates).
- So each branch was **ahead** of `main` (its SDK commits) and **behind** `main` (missing latest `main` commits).

## What we did to fix it

1. **Merged `origin/main` into each SDK branch**  
   So each branch is now: **main + that branch’s SDK commits**. No more “behind main.”
2. **Single config/version commit per branch**  
   Each branch has a commit like:  
   `chore(config): default server nakama-rest.intelli-verse-x.ai:443, SSL on; release 5.2.0`  
   so default Nakama host/port/key/SSL and SDK version **5.2.0** are consistent.
3. **Unreal-only follow-up**  
   On `add-unreal-sdk` only: an extra commit for RPC-not-found handling (wallet/grant/sync non-fatal), auth error hint, and README updates.

## Default server config (all SDKs)

- **Host:** `nakama-rest.intelli-verse-x.ai`  
- **Port:** `443`  
- **SSL:** enabled  
- **Version:** 5.2.0  

## Your next steps (push and PRs)

- **Push** each branch from your machine (where you’re logged into GitHub) to your fork.  
  See **docs/PUSH-TO-FORK-INSTRUCTIONS.md** for the exact `git push` commands.
- **Open (or update) PRs** from your fork’s branch into the org repo’s `main` (e.g. `hasanraza31/Intelli-verse-X-SDK` → `intelli-verse-x/Intelli-verse-X-SDK`).

## Summary

- Each SDK has its own branch; each branch has been **synced with `main`** via a merge.  
- Default server is **nakama-rest.intelli-verse-x.ai:443** with SSL; SDK version **5.2.0**.  
- Your work is committed; push the branches from your machine and create/update PRs as needed.

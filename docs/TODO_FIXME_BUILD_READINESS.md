# TODO/FIXME Build Readiness — Task List & Backend Scripting Updation

**Purpose:** Complete all IntelliVerseX SDK TODOs so the build is feature-ready and context validation passes. Align backend/scripting with the **Unity SDK (Asset Store)** where applicable.

**Context validator:** Scans `Assets/_IntelliVerseXSDK` for TODO/FIXME. Baseline: `tools/context/baselines/todo_fixme_cs.tsv`.  
**Note:** Package code lives in `Assets/Intelli-verse-X-SDK`; validator path may need to include that folder so baseline matches current files.

---

## 1. IntelliVerseX SDK TODOs (Unity — In Scope)

Only SDK-owned code is listed. Third-party (Nakama, Photon, VSCode, TextMesh Pro, etc.) are out of scope.

| # | File | Line | TODO / FIXME | What’s Needed |
|---|------|------|--------------|----------------|
| 1 | **IVXLoginController.cs** | 295 | Implement Apple Sign In | Wire to existing Apple Auth: use `IVXPanelLogin.SignInWithApple()` / AppleAuth plugin; get token and call Identity/Nakama or APIManager social login. Reference: Auth UI `IVXPanelLogin.cs`, `AppleAuth` in project. |
| 2 | **IVXLoginController.cs** | 325 | Implement Google Sign In | Wire to Google Sign-In (Unity plugin or REST). Get ID token, call backend (e.g. `APIManager` social login or Nakama `AuthenticateGoogle`). Reference: Auth UI `IVXPanelLogin.SignInWithGoogle()`, backend `APIManager` auth-v2. |
| 3 | **IVXLoginController.cs** | 382 | Implement Cognito email/password authentication | Replace device-id fallback with real Cognito: call `APIManager` login (e.g. `/api/user/auth-v2/login`) or existing Cognito flow from `IVXPanelLogin` / `APIManager` (Auth V2: Login). |
| 4 | **IVXQuizSessionManager.cs** | 297 | Implement backend saving if needed | When `saveToBackend == true`: persist quiz session/result via Nakama (Storage or RPC). Define RPC or collection keys; call from `CompleteSessionAsync`. Reference: Unity SDK storage/RPC usage. |
| 5 | **IVXAdsManager.cs** | 1607 | Implement hide banner logic | **Resolve by cleanup:** Implementation already exists below (AdMob, Appodeal, IronSource). Remove the TODO comment only. |
| 6 | **IVXAPIClient.cs** | 438 | Implement when wallet API is available in APIManager | Add wallet balance (or similar) endpoint in backend; implement in `APIManager`; call it from `GetWalletBalanceAsync`. Or document “requires backend wallet API” and keep stub until API exists. |
| 7 | **APIManager.cs** | 3046–3047 | profilePicture: null / socialId: null | Optional enhancement: when caller has avatar URL or provider userId, pass them into social login payload. Can stay as null until UI/flow provides them. |
| 8 | **IVXSubscriptionManager.cs** | 368–369 | Show paywall UI | Add a paywall flow: scene or prefab + simple UI (title, products, purchase button). Call from `ShowPaywall()`; on purchase use existing IAP/subscription APIs. Reference: Unity SDK IAP samples. |

**Baseline-only (CompleteGameBootstrap):**  
The baseline lists two TODOs in `Assets/_IntelliVerseXSDK/Examples/CompleteGameBootstrap.cs` (“ShowInterstitial not yet implemented”, “ShowRewarded not yet implemented”). In the current codebase, `CompleteGameBootstrap` already calls `IVXAdsManager.ShowInterstitialAd` and `ShowRewardedAd`. So either: (a) that file lives under `Intelli-verse-X-SDK` and the baseline path is outdated, or (b) the example under `_IntelliVerseXSDK` was removed. **Action:** Update baseline to match current paths and remove these two entries if the example no longer contains those TODOs.

---

## 2. What “Backend Scripting Updation” Means Here

- **Unity:** Backend = your game backend (Nakama, Cognito, custom API). “Scripting” = C# (Identity, APIManager, Ads, IAP, Quiz, etc.).
- **Other platforms (Defold, Godot, JS, etc.):** Backend = same servers (Nakama/Cognito/API). “Scripting” = Lua/GDScript/TS/etc. that call the same RPCs and APIs.

So **Backend Scripting Updation** = keep all platform SDKs in sync with:
- The same **backend contracts** (Nakama RPCs, REST endpoints, auth flows).
- The **feature set** of the **Unity SDK (Asset Store)** where it makes sense per platform.

---

## 3. What’s Needed for Backend Scripting Updation (With Unity SDK as Reference)

### 3.1 Backend / API alignment

| Area | Unity SDK (reference) | What other platforms need |
|------|------------------------|---------------------------|
| **Auth** | Device, Email, Google, Apple, Cognito (APIManager auth-v2) | Defold/Godot/JS: same endpoints and payloads; implement same flows (device, email, social tokens) and map to Nakama/Cognito. |
| **Profile** | APIManager + Nakama; profilePicture, socialId optional | Backend accepts profilePicture/socialId; all SDKs pass them when available. |
| **Wallet** | IVXAPIClient.GetWalletBalanceAsync (stub until API exists) | Backend: implement wallet API; then Unity + other SDKs call it. |
| **Quiz** | IVXQuizSessionManager.CompleteSessionAsync(saveToBackend) | Backend: RPC or Storage schema for quiz sessions; Unity implements; Defold/others add same RPC/storage calls. |
| **IAP / Paywall** | IVXSubscriptionManager + paywall UI (TODO) | Unity: add paywall UI; backend: product/entitlement checks; other platforms: same entitlements/RPCs where applicable. |

### 3.2 Defold (and other non-Unity SDKs)

- **Auth:** Defold `ivx.lua` already has `authenticate_device`, `authenticate_email`, `authenticate_google`, `authenticate_apple`. Ensure they use the **same** backend URLs and payloads as Unity (Nakama + any REST auth-v2).
- **RPC / Storage:** Add any new RPCs or storage keys that Unity uses (e.g. quiz save, wallet) so Defold can call the same RPCs.
- **No Unity-only features:** Ads, IAP, full Cognito UI are Unity-specific; Defold can stay with “RPC only” for those if that’s the design.

**Concrete Defold tasks:**

1. Compare `SDKs/defold/intelliversex/ivx.lua` with Unity’s auth and RPC usage; add any missing RPC wrappers (e.g. quiz save, wallet) that the backend will support.
2. Document which backend RPCs/endpoints each platform uses (single “backend contract” doc or table).
3. After Unity paywall/wallet/quiz backend is defined, add matching Lua (or other) helpers in Defold.

### 3.3 Reference: Unity SDK (Asset Store)

Use the **Unity SDK** as the source of truth for:

- **Auth flows:** `IVXPanelLogin`, `APIManager` (auth-v2), `IntelliVerseXIdentity`, Cognito config.
- **Ads:** `IVXAdsManager` (ShowInterstitial, ShowRewarded, HideBanner); no change needed for “not yet implemented” in examples if the manager already implements them.
- **IAP / Paywall:** `IVXSubscriptionManager`, IAP samples; add paywall UI and hook to same backend.
- **Quiz:** `IVXQuizSessionManager`; define and implement backend save, then mirror RPC from other SDKs.
- **Wallet:** `IVXAPIClient.GetWalletBalanceAsync` + `APIManager` once backend has wallet API.

---

## 4. Suggested Order of Work

1. **Baseline & validator**  
   - Update `todo_fixme_cs.tsv` to current paths (`Assets/Intelli-verse-X-SDK` if that’s where the code lives).  
   - Optionally make the validator scan both `Assets/_IntelliVerseXSDK` and `Assets/Intelli-verse-X-SDK` so one baseline covers all SDK code.

2. **Quick wins**  
   - Remove the stale TODO in `IVXAdsManager.cs` (HideBannerAd).  
   - Resolve CompleteGameBootstrap baseline entries (update or remove).

3. **Auth (IVXLoginController)**  
   - Implement Apple Sign In (wire to Apple Auth plugin + backend).  
   - Implement Google Sign In (token + backend).  
   - Implement Cognito email/password (use existing APIManager/Cognito flow).

4. **Backend + scripting**  
   - Wallet: backend wallet API + `APIManager` + `IVXAPIClient.GetWalletBalanceAsync`.  
   - Quiz: backend save (RPC/Storage) + `IVXQuizSessionManager.CompleteSessionAsync`.  
   - APIManager: optional profilePicture/socialId when available.

5. **Paywall**  
   - Add paywall UI and hook `IVXSubscriptionManager.ShowPaywall()` to it and to IAP/subscription logic.

6. **Defold / other platforms**  
   - Add any new RPCs/storage to match Unity; document backend contract; keep auth/payloads aligned.

---

## 5. Summary Table: TODO → Action

| TODO | Action | Depends On |
|------|--------|------------|
| Apple Sign In (IVXLoginController) | Wire to Apple Auth + backend | Apple Auth plugin, backend social login |
| Google Sign In (IVXLoginController) | Wire to Google token + backend | Google plugin or REST, backend |
| Cognito email/password (IVXLoginController) | Use APIManager/Cognito login | Existing Cognito in Unity SDK |
| Quiz backend saving | Nakama RPC/Storage + call in CompleteSessionAsync | Backend RPC/schema |
| Hide banner (IVXAdsManager) | Remove TODO comment | None |
| Wallet API (IVXAPIClient) | Implement in APIManager + call from client | Backend wallet endpoint |
| profilePicture/socialId (APIManager) | Pass when available | Optional; UI/flow |
| Paywall UI (IVXSubscriptionManager) | Add scene/prefab + ShowPaywall() | IAP/subscription backend |
| CompleteGameBootstrap baseline | Update/remove baseline entries | None |
| Validator path | Scan Intelli-verse-X-SDK and/or fix baseline paths | None |

Completing these and updating the baseline will get the build ready with all TODO-driven features and keep backend scripting aligned with the Unity SDK (Asset Store) and other platforms.

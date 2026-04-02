# Unity WebGL Platform

> IntelliVerseX runs natively in the browser via Unity's WebGL build target. All HTTP/WebSocket-based modules work out of the box; a few require WebGL-specific adapters shipped with the SDK.

---

## Unity WebGL Build Pipeline

### Player Settings

Configure these in **Edit → Project Settings → Player → WebGL**:

| Setting | Recommended Value | Why |
|---------|-------------------|-----|
| WebGL Template | Custom (with IntelliVerseX head tags) | Inject ad scripts, meta tags |
| Compression Format | **Brotli** (release) / Gzip (dev) | Brotli is ~15-20 % smaller |
| Data Caching | Enabled | Avoids re-downloading assets |
| Initial Memory Size | 512 MB | 256 MB default is too small for most games |
| Maximum Memory Size | 2048 MB | Prevents OOM on complex scenes |
| Run In Background | Enabled | Keeps WebSocket alive when tab loses focus |
| Decompression Fallback | Enabled | Fallback for hosts that lack Brotli support |

### Compile Defines

IntelliVerseX gates WebGL-specific code behind preprocessor defines:

```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
    IVXWebGLBridge.InitializeJSPlugins();
    IVXWebGLAdsManager.Instance.LoadAdSenseBanner();
#endif
```

`UNITY_WEBGL` is set automatically by Unity when the active build target is WebGL. The SDK checks for `!UNITY_EDITOR` to avoid calling `.jslib` functions during Play Mode.

### Build Size Optimization

!!! tip "Target < 30 MB compressed for fast page loads"

- **Strip Engine Code** — Enable in Player Settings → Other Settings → Strip Engine Code
- **Managed Stripping Level** — Set to **High** (or Minimal if reflection issues arise)
- **Disable unused modules** — Uncheck Physics, Cloth, etc. in Player Settings → WebGL → Modules
- **Compress textures** — Use ASTC / ETC2 with crunch compression
- **Addressables** — Move non-critical assets to remote bundles loaded on demand
- **IL2CPP code generation** — Set to "Faster (smaller) builds" for release

```
Build command (CI):
  Unity -quit -batchmode -buildTarget WebGL \
        -executeMethod BuildScript.PerformWebGLBuild \
        -logFile build.log
```

---

## WebGL-Specific Limitations

Understanding these constraints is critical for a stable WebGL deployment.

### Threading

`System.Threading.Thread` is **not available**. Unity's WebGL backend runs single-threaded on the browser's main thread.

| What works | What doesn't |
|------------|--------------|
| `async` / `await` (compiles to coroutines) | `new Thread(...)` |
| `Task.Run` (executes synchronously) | `ThreadPool.QueueUserWorkItem` |
| Coroutines | `Mutex`, `Semaphore`, `Monitor` |

IntelliVerseX avoids `System.Threading` internally; all async operations use `UniTask` or coroutines on WebGL.

### Networking

`System.Net.Sockets` is **not available**. All network calls must go through:

- **`UnityWebRequest`** — HTTP calls (used by Nakama REST client, Hiro RPCs, Satori)
- **Browser `WebSocket`** — Real-time connections (Nakama socket uses the native browser API automatically)
- **`.jslib` interop** — For custom JavaScript APIs (e.g., Applixir ads, AdSense, WebXR)

```csharp
// Nakama auto-selects the correct transport on WebGL
var client = new Client("http", "nakama.example.com", 7350, "serverkey");
var socket = client.NewSocket(useMainThread: true);  // browser WebSocket
```

### Audio

Unity's FMOD-based `AudioSource` is replaced by the **WebAudio API** on WebGL. Differences:

- Playback must start from a **user gesture** (click/tap) — autoplay is blocked by browsers
- Compressed audio uses browser decoding — prefer `.ogg` (wide support) or `.mp3`
- `AudioSource.time` seeking can be less precise

### File System

There is no direct file system. Unity maps `Application.persistentDataPath` to **IndexedDB**:

```csharp
// This works — Unity abstracts to IndexedDB
string path = Path.Combine(Application.persistentDataPath, "save.json");
File.WriteAllText(path, json);

// PlayerPrefs also persists to IndexedDB
PlayerPrefs.SetString("session_token", token);
```

!!! warning
    IndexedDB has per-origin storage limits (varies by browser, typically 50-80 % of available disk). Avoid storing large blobs.

### Memory

WebGL runs inside a **WebAssembly linear memory** buffer. The default 256 MB is often insufficient.

- Set **Initial Memory** to 512 MB for typical games
- Set **Maximum Memory** to 2048 MB (browsers cap at ~4 GB)
- Monitor with `Profiler.GetTotalAllocatedMemoryLong()` in debug builds
- Memory cannot be freed back to the OS once allocated — avoid spikes

---

## SDK Module Compatibility

| Module | WebGL | Notes |
|--------|:-----:|-------|
| **Identity** | :white_check_mark: | Fingerprint-based guest ID via `DeviceInfoHelper` WebGL branch; no native keychain |
| **Ads** | :white_check_mark: | `IVXWebGLAdsManager` with AdSense + Applixir via `.jslib` plugin |
| **Monetization** | :white_check_mark: | `IVXWebGLMonetizationManager` for web payment flows (Stripe, PayPal) |
| **AI Voice/Host** | :white_check_mark: | WebSocket via `ConnectWebGL()` coroutine; TTS uses Web Speech API fallback |
| **Multiplayer** | :white_check_mark: | Nakama WebSocket works natively in browser |
| **Hiro / Economy** | :white_check_mark: | All RPC-based, works over HTTP via `UnityWebRequest` |
| **Satori Analytics** | :white_check_mark: | HTTP-based event capture |
| **Quiz** | :white_check_mark: | S3 content fetch works via `UnityWebRequest` |
| **Storage** | :white_check_mark: | Nakama cloud storage over HTTP |
| **Social / Discord** | :material-approximately-equal: Partial | API calls work; Rich Presence not available in browser context |
| **Leaderboards** | :white_check_mark: | HTTP-based, fully functional |
| **Localization** | :white_check_mark: | Local JSON bundles load from StreamingAssets |

### WebGL Ad Integration

The SDK ships a `.jslib` bridge for web ads:

=== "AdSense (Banner/Interstitial)"

    ```javascript
    // Assets/_IntelliVerseXSDK/Plugins/WebGL/IVXAdSense.jslib
    mergeInto(LibraryManager.library, {
        IVXAdSense_ShowBanner: function(adSlotPtr) {
            var adSlot = UTF8ToString(adSlotPtr);
            // injects <ins class="adsbygoogle"> into the page
        }
    });
    ```

=== "Applixir (Rewarded Video)"

    ```javascript
    // Assets/_IntelliVerseXSDK/Plugins/WebGL/IVXApplixir.jslib
    mergeInto(LibraryManager.library, {
        IVXApplixir_ShowRewarded: function(callbackObjPtr, callbackMethodPtr) {
            invokeApplixirVideo({
                zoneId: window.IVX_APPLIXIR_ZONE,
                accountId: window.IVX_APPLIXIR_ACCOUNT,
                callback: function(result) {
                    SendMessage(UTF8ToString(callbackObjPtr),
                                UTF8ToString(callbackMethodPtr),
                                result);
                }
            });
        }
    });
    ```

---

## Hosting Requirements

### HTTPS

WebGL builds **must** be served over HTTPS in production. Browsers block mixed content, which means:

- WebSocket connections to Nakama (`wss://`) require the page itself to be `https://`
- Non-secure origins cannot access `navigator.mediaDevices` (needed for voice)

### CORS Headers

If quiz content or remote assets are hosted on a different origin (e.g., S3), the bucket must return proper CORS headers:

```
Access-Control-Allow-Origin: https://yourgame.example.com
Access-Control-Allow-Methods: GET, HEAD
Access-Control-Allow-Headers: Content-Type
```

### SharedArrayBuffer Headers

If you enable Unity's multi-threaded WebAssembly (experimental), the host must set:

```
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Embedder-Policy: require-corp
```

!!! note
    These headers break third-party iframes (ads, OAuth popups). Only enable if you need threads and have ad-free pages or use a cross-origin isolation workaround.

### Recommended Hosting Providers

| Provider | Header Config | Notes |
|----------|:------------:|-------|
| **Cloudflare Pages** | `_headers` file | Free tier, global CDN, easy custom headers |
| **Vercel** | `vercel.json` | Free tier, edge network |
| **Netlify** | `_headers` file | Free tier, form handling |
| **AWS S3 + CloudFront** | Lambda@Edge | Most control, more setup |
| **itch.io** | Limited | Good for jams; no custom header control |

Example `_headers` file (Cloudflare Pages / Netlify):

```
/*
  Cross-Origin-Opener-Policy: same-origin
  Cross-Origin-Embedder-Policy: require-corp
  X-Content-Type-Options: nosniff

/Build/*
  Content-Encoding: br
  Content-Type: application/wasm
```

---

## WebGL Template

IntelliVerseX provides a custom WebGL template at `Assets/_IntelliVerseXSDK/WebGLTemplates/IntelliVerseX/`:

```
WebGLTemplates/IntelliVerseX/
├── index.html          # Main page with ad containers and loader
├── style.css           # Responsive canvas styling
├── TemplateData/
│   ├── progress.js     # Custom loading bar
│   └── favicon.ico
└── ivx-bridge.js       # SDK ↔ browser bridge (ads, payments, analytics)
```

Select it in **Player Settings → WebGL → Resolution and Presentation → WebGL Template**.

---

## CI/CD

WebGL builds are already configured in the project CI pipeline:

```yaml
# .github/workflows/unity-tests.yml (excerpt)
- name: Build WebGL
  uses: game-ci/unity-builder@v4
  with:
    targetPlatform: WebGL
    buildMethod: BuildScript.PerformWebGLBuild
```

### Post-Build Steps

1. **Compress** — Ensure Brotli `.br` files are generated (Unity does this automatically)
2. **Deploy** — Upload `Build/` folder to your static host
3. **Verify headers** — `curl -I https://yourgame.example.com/Build/game.wasm` should return `Content-Encoding: br`
4. **Smoke test** — Open in Chrome, Firefox, Safari; check console for WebSocket connection

---

## Debugging

### Browser DevTools

- **Console** — Unity `Debug.Log` maps to `console.log`; filter by `[IVX]` prefix
- **Network** — Inspect Nakama HTTP/WebSocket traffic
- **Memory** — Chrome's Memory tab shows WASM heap usage
- **Performance** — Profile frame drops; look for JS ↔ WASM interop overhead

### Development Build

Enable **Development Build** + **Autoconnect Profiler** in Build Settings for:

- Source maps (readable stack traces)
- Unity Profiler connection over WebSocket
- `Debug.Assert` is active

!!! warning
    Development builds are significantly larger. Never ship a dev build to production.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Blank screen, no errors | MIME type wrong for `.wasm` | Ensure server returns `application/wasm` |
| `ReferenceError: SharedArrayBuffer` | Missing isolation headers | Add COOP/COEP headers or disable threads |
| `Mixed Content` blocked | HTTP page + WSS socket | Serve page over HTTPS |
| `Out of memory` | Default 256 MB too low | Increase Initial Memory in Player Settings |
| Ads not showing | Ad blocker or missing `.jslib` | Check browser console; whitelist in ad blocker |
| Audio won't play | No user gesture | Call `AudioContext.resume()` on first click |
| `IndexedDB` quota error | Too much data in `persistentDataPath` | Clean up old saves; reduce stored data |

# IVX × Pico XR (Unity)

The IVX Unity multiplayer adapter runs unchanged on Pico Neo 3 / Pico 4
because Pico's runtime is plain Android XR + OpenXR. The Pico-specific
glue is just the bootstrapper in this folder, which:

1. Detects the headset via `PXR_Plugin.System.UPxr_GetProductName()`
   (when the `IVX_HAS_PICO_XR` define is on).
2. Tags the spatial frame with `provider_hint = "pico-xr"` so the
   kernel telemetry can split metrics by device family.
3. Defaults to a 72Hz publish rate (Pico 4 native refresh).
4. Picks LiveKit voice (Pico's Android stack supports Opus + WebRTC).

## Setup

1. Install the Pico XR Unity package
   (`com.unity.xr.pico` from the Pico developer portal).
2. Add the scripting define `IVX_HAS_PICO_XR` to the Android player
   settings.
3. Import the IVX SDK and drop `IVXPicoXRBootstrapper` onto a scene
   GameObject. Wire it to your `IVXNakamaMultiplayer` and (optionally)
   the Unity avatar replicator.
4. Enable OpenXR + Pico XR loader in `Project Settings → XR Plug-in
   Management → Android`.
5. Build with `Target API Level = 32+` and IL2CPP backend.

## Anchors

Pico devices speak OpenXR XR_MSFT_spatial_anchor (the same extension
the Hololens uses) so `IVXOpenXrMsftSpatialAnchorProvider.cs` works
unchanged. Cloud anchors require Pico's XR Cloud Anchor service if
you want cross-device persistence; the IVX provider falls back to
match-local synthetic frames if the device's cloud anchor service is
unavailable.

## Sample scene

`Samples/PicoPartyScene/PicoPartyScene.unity` (this folder) wires:

* `IVXNakamaMultiplayer` (transport).
* `IVXLiveKitVoiceProvider` (voice).
* `IVXAvatarReplicator` (RealityKit-equivalent ECS port for Unity).
* `IVXPicoXRBootstrapper` (Pico-specific tweaks).

After building, sideload to the headset:

```
adb install -r build/IVXPicoParty.apk
```

## Cert checklist

| Concern | Pico Neo 3 | Pico 4 |
|---------|:---------:|:------:|
| 6DOF tracking | ✅ | ✅ |
| Hand tracking | ⚠ (via add-on) | ✅ |
| Spatial anchors (XR_MSFT) | ✅ | ✅ |
| LiveKit Android Opus | ✅ | ✅ |
| Network sign-in flow | ✅ | ✅ |
| Voice attestation | ✅ | ✅ |

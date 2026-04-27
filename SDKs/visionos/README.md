# IVXMultiplayer — visionOS / iOS / macOS Swift Package

Targets visionOS 1.0+, iOS 17+, macOS 14+. Bridges:

* **Transport** — `nakama-cpp` xcframework wrapped by
  `IVXNakamaTransport`. Replace the stubbed `_sendBytes` /
  `_onIncoming` shims with calls into the real `CIVXNakama`
  Objective-C++ shim once the xcframework is dropped under
  `SDKs/nakama-cpp/`.
* **Voice** — `IVXLiveKitVoiceProvider` implements
  `IVXVoiceProviderProtocol` over the upstream LiveKit Swift SDK.
  Spatial audio is enabled when `IVXVoiceSessionToken.spatial` is `true`.
* **Avatar replication** — `IVXAvatarReplicator` attaches to a
  RealityKit `Entity` root, publishes local poses at 60Hz, lerps peer
  entities into place, and respects server-driven LOD changes.
* **Spatial frames** — `IVXRealityKitSpatialFrame` implements the
  kernel `ISpatialFrame` translator. Hosts call `rebase(toAnchorWorldTransform:)`
  whenever a cloud anchor relocalizes.

## File layout

```
Sources/IVXMultiplayer/
├── API/
│   ├── IVXMultiplayer.swift   # client + session protocols, op codes
│   └── IVXVoice.swift          # IVXVoiceProviderProtocol
├── Transport/
│   └── IVXNakamaTransport.swift
├── Voice/
│   └── IVXLiveKitVoiceProvider.swift
├── Avatar/
│   └── IVXAvatarReplicator.swift
└── Spatial/
    └── IVXSpatialFrame.swift

Examples/PartyRoomVisionOS/
└── IVXPartyRoomApp.swift       # Vision Pro demo (RealityView immersive)
```

## Quick start

```swift
import IVXMultiplayer

let transport = IVXNakamaTransport(host: "nakama.example.com")
try await transport.connect()
try await transport.authenticate(deviceId: UIDevice.current.identifierForVendor!.uuidString)
let session = try await transport.createMatch(
    templateId: "avatar-replication-v1",
    gameId: "ivx.party"
)
let replicator = IVXAvatarReplicator(session: session, root: rootEntity)
replicator.attach { (camera.position, camera.orientation) }
```

## Wire compatibility

All op-codes and quantization formats match the kernel exactly:

| Op | Code   | Direction |
|----|--------|-----------|
| `pose_update` | `0xC101` | server → client |
| `pose_submit` | `0xC102` | client → server |
| `lod_change`  | `0xC103` | server → client |

`IVXPoseQuantized` uses the same smallest-three quaternion packing as
`AvatarReplicationMatch.go` and `webxr/adapter.ts`, so a Vision Pro
client interoperates with WebXR + Unity Quest + Unreal PC clients in the
same room.

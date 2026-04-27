// IVXPartyRoomApp — visionOS sample wiring the multiplayer kernel
// (Nakama transport), LiveKit voice provider, RealityKit avatar
// replicator, and an ISpatialFrame translator together.
//
// Build: open the visionOS package in Xcode 15+, link this target as an
// App, set the deployment target to visionOS 1.0, and run on Vision Pro
// simulator or device.

import SwiftUI
import RealityKit
import IVXMultiplayer

@main
struct IVXPartyRoomApp: App {
    var body: some Scene {
        WindowGroup { ContentView() }
        ImmersiveSpace(id: "ivx-party") {
            PartyImmersiveView()
        }
    }
}

@MainActor
final class IVXPartyRoom: ObservableObject {
    @Published var connected = false
    @Published var matchId: String = ""

    let transport: IVXNakamaTransport
    var session: IVXMultiplayerSession?
    var voice: IVXLiveKitVoiceProvider?
    var avatar: IVXAvatarReplicator?
    let spatial = IVXRealityKitSpatialFrame()

    init() {
        self.transport = IVXNakamaTransport(host: "nakama.intelliverse.example")
    }

    func connect(deviceId: String) async {
        do {
            try await transport.connect()
            try await transport.authenticate(deviceId: deviceId)
            let s = try await transport.createMatch(
                templateId: "avatar-replication-v1",
                gameId: "ivx.party",
                init_: nil
            )
            self.session = s
            self.matchId = s.matchId
            self.connected = true
        } catch {
            NSLog("[IVXPartyRoom] connect failed: \(error)")
        }
    }

    func attach(to root: Entity, localProvider: @escaping () -> (SIMD3<Float>, simd_quatf)) {
        guard let s = session else { return }
        let rep = IVXAvatarReplicator(session: s, root: root)
        rep.attach(localPoseProvider: localProvider)
        self.avatar = rep
        self.spatial.setActive(IVXSpatialFrame(
            frameId: "match-\(s.matchId)",
            kind: .localMatch
        ))
    }
}

struct ContentView: View {
    @StateObject private var room = IVXPartyRoom()

    var body: some View {
        VStack(spacing: 16) {
            Text("IVX Party Room").font(.largeTitle)
            Text(room.connected ? "match: \(room.matchId)" : "disconnected")
            Button("Connect") {
                Task { await room.connect(deviceId: ProcessInfo.processInfo.globallyUniqueString) }
            }
            .buttonStyle(.borderedProminent)
        }
        .padding()
        .environmentObject(room)
    }
}

struct PartyImmersiveView: View {
    @EnvironmentObject var room: IVXPartyRoom

    var body: some View {
        RealityView { content in
            let root = Entity()
            content.add(root)
            // Local pose provider hooks into the visionOS world tracking.
            // This stub returns identity; the real app should sample the
            // ARSession's worldTransform.
            room.attach(to: root) {
                (SIMD3<Float>(0, 1.6, 0), simd_quatf(angle: 0, axis: [0, 1, 0]))
            }
        }
    }
}

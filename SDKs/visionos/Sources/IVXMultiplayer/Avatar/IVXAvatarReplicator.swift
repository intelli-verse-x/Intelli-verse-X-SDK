// IVXAvatarReplicator — RealityKit ECS adapter.
//
// Attaches to a RealityKit Scene; for each remote presence it:
//   * Spawns a placeholder Entity (Model or imported GLB).
//   * Receives quantized poses from avatar-replication-v1 match.
//   * Lerps Entity transform on the main actor.
//
// The local viewer publishes poses at 60Hz (configurable) by sampling
// the active ARView / Scene's camera (or a host-supplied transform).
// On Apple Vision Pro this is the ARSession's worldOrigin transform.
//
// Wire layout matches Go AvatarReplicationMatch + JS WebXR adapter.

import Foundation
import RealityKit
import simd

#if canImport(Combine)
import Combine
#endif

public enum IVXAvatarOp: UInt32 {
    case poseUpdate = 0xC101  // server → client
    case poseSubmit = 0xC102  // client → server
    case lodChange  = 0xC103
}

public struct IVXPosePayload: Codable {
    public let user_id: String?
    public let pose: IVXPoseQuantized
}

@MainActor
public final class IVXAvatarReplicator {
    private let session: IVXMultiplayerSession
    private weak var rootEntity: Entity?
    private var peers: [String: Entity] = [:]
    private var sub: IVXSubscription?
    private var publishTask: Task<Void, Never>?
    private var localProvider: (() -> (SIMD3<Float>, simd_quatf))?

    public var publishHz: Double = 60

    public init(session: IVXMultiplayerSession, root: Entity) {
        self.session = session
        self.rootEntity = root
    }

    public func attach(localPoseProvider: @escaping () -> (SIMD3<Float>, simd_quatf)) {
        self.localProvider = localPoseProvider
        self.sub = session.subscribe(IVXAvatarOp.poseUpdate.rawValue) { [weak self] (env: IVXEnvelope<IVXPosePayload>) in
            guard let self = self, let p = env.payload else { return }
            Task { @MainActor in self.applyPeer(p) }
        }
        self.publishTask = Task { [weak self] in
            guard let self = self else { return }
            let interval = UInt64(1_000_000_000.0 / max(1.0, self.publishHz))
            while !Task.isCancelled {
                if let provider = self.localProvider {
                    let (pos, rot) = provider()
                    let q = IVXPoseQuantized.quantize(position: pos, rotation: rot)
                    let payload = IVXPosePayload(user_id: nil, pose: q)
                    do { try await self.session.send(IVXAvatarOp.poseSubmit.rawValue, payload) } catch { }
                }
                try? await Task.sleep(nanoseconds: interval)
            }
        }
    }

    public func detach() {
        sub?.cancel(); sub = nil
        publishTask?.cancel(); publishTask = nil
        for (_, e) in peers { e.removeFromParent() }
        peers.removeAll()
    }

    private func applyPeer(_ p: IVXPosePayload) {
        guard let root = rootEntity, let userId = p.user_id else { return }
        let entity = peers[userId] ?? makePeerEntity(for: userId, parent: root)
        peers[userId] = entity
        let pos = p.pose.position
        var t = entity.transform
        t.translation = pos
        entity.transform = t
    }

    private func makePeerEntity(for userId: String, parent: Entity) -> Entity {
        let mesh = MeshResource.generateBox(size: SIMD3<Float>(0.3, 0.5, 0.3))
        let material = SimpleMaterial(color: .systemTeal, isMetallic: false)
        let entity = ModelEntity(mesh: mesh, materials: [material])
        entity.name = "ivx-peer-\(userId)"
        parent.addChild(entity)
        return entity
    }
}

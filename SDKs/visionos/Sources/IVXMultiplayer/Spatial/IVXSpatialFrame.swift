// IVXSpatialFrame — visionOS / iOS implementation of the kernel
// ISpatialFrame contract. Backed by RealityKit + ARKit world maps where
// available. On platforms without world tracking, falls back to a
// match-local synthetic frame.
//
// Wire contract: schemas/multiplayer/services/spatial_frame.proto.

import Foundation
import simd
#if canImport(ARKit)
import ARKit
#endif
#if canImport(RealityKit)
import RealityKit
#endif

public enum IVXSpatialFrameKind: Int32, Sendable {
    case unspecified  = 0
    case localMatch   = 1
    case anchorShared = 2
    case worldMap     = 3
}

public struct IVXSpatialFrame: Sendable, Codable {
    public let frameId: String
    public let kind: IVXSpatialFrameKind
    public let originUserId: String?
    public let createdMs: Int64
    public let providerHint: String?

    public init(frameId: String, kind: IVXSpatialFrameKind, originUserId: String? = nil,
                createdMs: Int64 = Int64(Date().timeIntervalSince1970 * 1000),
                providerHint: String? = nil) {
        self.frameId = frameId
        self.kind = kind
        self.originUserId = originUserId
        self.createdMs = createdMs
        self.providerHint = providerHint
    }

    enum CodingKeys: String, CodingKey {
        case frameId = "frame_id"
        case kind
        case originUserId = "origin_user_id"
        case createdMs = "created_ms"
        case providerHint = "provider_hint"
    }

    public func encode(to encoder: Encoder) throws {
        var c = encoder.container(keyedBy: CodingKeys.self)
        try c.encode(frameId, forKey: .frameId)
        try c.encode(kind.rawValue, forKey: .kind)
        try c.encodeIfPresent(originUserId, forKey: .originUserId)
        try c.encode(createdMs, forKey: .createdMs)
        try c.encodeIfPresent(providerHint, forKey: .providerHint)
    }

    public init(from decoder: Decoder) throws {
        let c = try decoder.container(keyedBy: CodingKeys.self)
        self.frameId = try c.decode(String.self, forKey: .frameId)
        let raw = try c.decode(Int32.self, forKey: .kind)
        self.kind = IVXSpatialFrameKind(rawValue: raw) ?? .unspecified
        self.originUserId = try c.decodeIfPresent(String.self, forKey: .originUserId)
        self.createdMs = try c.decode(Int64.self, forKey: .createdMs)
        self.providerHint = try c.decodeIfPresent(String.self, forKey: .providerHint)
    }
}

public protocol IVXSpatialFrameTranslator: AnyObject {
    var activeFrame: IVXSpatialFrame? { get }
    func setActive(_ frame: IVXSpatialFrame)
    func toLocal(_ pose: IVXPoseQuantized) -> (SIMD3<Float>, simd_quatf)
    func fromLocal(position: SIMD3<Float>, rotation: simd_quatf) -> IVXPoseQuantized
}

@MainActor
public final class IVXRealityKitSpatialFrame: IVXSpatialFrameTranslator {
    public private(set) var activeFrame: IVXSpatialFrame?
    private var rootTransform: simd_float4x4 = matrix_identity_float4x4

    public init() {}

    public func setActive(_ frame: IVXSpatialFrame) {
        self.activeFrame = frame
        // Reset the root transform; the host should call rebase() when an
        // anchor relocalizes.
        self.rootTransform = matrix_identity_float4x4
    }

    public func rebase(toAnchorWorldTransform t: simd_float4x4) {
        self.rootTransform = t
    }

    public func toLocal(_ pose: IVXPoseQuantized) -> (SIMD3<Float>, simd_quatf) {
        let p = pose.position
        let v = simd_float4(p.x, p.y, p.z, 1)
        let local = rootTransform * v
        let pos = SIMD3<Float>(local.x, local.y, local.z)
        return (pos, simd_quatf(angle: 0, axis: SIMD3<Float>(0, 1, 0)))
    }

    public func fromLocal(position: SIMD3<Float>, rotation: simd_quatf) -> IVXPoseQuantized {
        let inv = simd_inverse(rootTransform)
        let v = simd_float4(position.x, position.y, position.z, 1)
        let frame = inv * v
        return IVXPoseQuantized.quantize(
            position: SIMD3<Float>(frame.x, frame.y, frame.z),
            rotation: rotation
        )
    }
}

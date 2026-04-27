// IVXMultiplayer — Swift surface mirroring the C# IIVXMultiplayer
// contract. The actual transport is nakama-cpp (delivered as an
// xcframework) wrapped by IVXNakamaTransport.swift.
//
// Goals:
//   * Type-safe wire constants (mirror schemas/multiplayer/*.proto).
//   * Async/await match join + envelope subscribe.
//   * RealityKit ECS adapter (IVXAvatarReplicator) so a host scene
//     just attaches `IVXAvatarReplicator` and pose updates flow.

import Foundation
import RealityKit

// MARK: - Wire constants (mirror IVXWireConstants.cs / constants.ts)

public enum IVXKernelOp: UInt32 {
    case clientHello              = 0x0001
    case serverHello              = 0x0002
    case heartbeat                = 0x0003
    case playerJoined             = 0x0004
    case playerLeft               = 0x0005
    case playerKicked             = 0x0006
    case matchEnded               = 0x0007
    case error                    = 0x0008
    case matchResume              = 0x0009
    case matchResumeAck           = 0x000A
    case latencyWarning           = 0x000B
    case tickRateChanged          = 0x000C
    case voiceCapabilityChanged   = 0x000D
    case voiceUnavailable         = 0x000E
    case voiceModeChanged         = 0x000F
    case lowBandwidthRequest      = 0x0010
    case networkClockPing         = 0x0011
    case networkClockPong         = 0x0012
    case warnRateLimited          = 0x0013
    case warnTickOverrun          = 0x0014
}

public enum IVXErrorCode: Int32 {
    case unspecified         = 0
    case schemaTooOld        = 1
    case serverTooOld        = 2
    case badPayload          = 3
    case seqGap              = 4
    case unknownOpcode       = 5
    case clockSkewExtreme    = 7
    case matchFull           = 20
    case matchNotFound       = 21
    case notAMember          = 22
    case rateLimited         = 23
    case flapping            = 24
    case matchEnded          = 25
    case sessionReplaced     = 26
    case permissionDenied    = 30
    case kicked              = 31
    case banned              = 32
    case notAuthorized       = 33
    case voiceUnavailable    = 60
    case anchorIncompat      = 50
    case anchorLost          = 51
    case `internal`          = 999
}

// MARK: - Envelope

public struct IVXEnvelope<Payload: Decodable>: Decodable {
    public let seq:    UInt64
    public let ts_ms:  Int64
    public let op:     UInt32
    public let payload: Payload?
}

public struct IVXErrorPayload: Decodable {
    public let code:   Int32
    public let detail: String
}

// MARK: - Multiplayer client + session protocols

public protocol IVXMultiplayerSession: AnyObject {
    var matchId: String { get }
    func send<P: Encodable>(_ op: UInt32, _ payload: P) async throws
    func subscribe<P: Decodable>(_ op: UInt32, _ handler: @escaping (IVXEnvelope<P>) -> Void) -> IVXSubscription
    func leave() async throws
}

public protocol IVXSubscription: AnyObject {
    func cancel()
}

public protocol IVXMultiplayerClient: AnyObject {
    func connect() async throws
    func authenticate(deviceId: String) async throws
    func createMatch(templateId: String, gameId: String, init_: Encodable?) async throws -> IVXMultiplayerSession
    func joinMatch(matchId: String) async throws -> IVXMultiplayerSession
    func disconnect() async
}

// MARK: - Quantized pose mirror

public struct IVXPoseQuantized: Codable, Hashable {
    public let px_mm: Int32
    public let py_mm: Int32
    public let pz_mm: Int32
    public let rot_packed: UInt32
    public let ts_ms: Int64

    public init(px_mm: Int32, py_mm: Int32, pz_mm: Int32, rot_packed: UInt32, ts_ms: Int64) {
        self.px_mm = px_mm; self.py_mm = py_mm; self.pz_mm = pz_mm
        self.rot_packed = rot_packed; self.ts_ms = ts_ms
    }

    public static func quantize(position: SIMD3<Float>, rotation: simd_quatf) -> IVXPoseQuantized {
        let clampMM: Int32 = 32_767
        let px = max(-clampMM, min(clampMM, Int32((position.x * 1000).rounded())))
        let py = max(-clampMM, min(clampMM, Int32((position.y * 1000).rounded())))
        let pz = max(-clampMM, min(clampMM, Int32((position.z * 1000).rounded())))
        // Smallest-three packing.
        let qv = rotation.vector
        let q: [Float] = [qv.x, qv.y, qv.z, qv.w]
        var dropIdx = 0
        var maxAbs: Float = 0
        for (i, v) in q.enumerated() {
            if abs(v) > maxAbs { maxAbs = abs(v); dropIdx = i }
        }
        let sign: Float = q[dropIdx] >= 0 ? 1 : -1
        var packed: UInt32 = UInt32(dropIdx & 0x3)
        var slot = 0
        for i in 0..<4 where i != dropIdx {
            let scaled = max(-1, min(1, q[i] * sign * Float(2.0).squareRoot()))
            let bits = UInt32(((scaled + 1) * 0.5 * 511).rounded())
            packed |= (bits & 0x1FF) << UInt32(2 + slot * 9)
            slot += 1
        }
        return IVXPoseQuantized(
            px_mm: px, py_mm: py, pz_mm: pz, rot_packed: packed,
            ts_ms: Int64(Date().timeIntervalSince1970 * 1000)
        )
    }

    public var position: SIMD3<Float> {
        SIMD3<Float>(Float(px_mm) / 1000, Float(py_mm) / 1000, Float(pz_mm) / 1000)
    }
}

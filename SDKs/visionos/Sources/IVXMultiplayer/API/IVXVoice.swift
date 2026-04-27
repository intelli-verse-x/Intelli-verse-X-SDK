// IVXVoice — Swift mirror of IIVXVoice for visionOS / iOS.
//
// Wire contract: schemas/multiplayer/services/voice.proto.

import Foundation

public enum IVXVoiceProvider: Int, Sendable {
    case unspecified = 0
    case livekit     = 1
    case agora       = 2
    case twilio      = 3
    case dolby       = 4
    case none        = 5
}

public enum IVXVoiceCodec: Int, Sendable {
    case unspecified = 0
    case opus        = 1
    case aac         = 2
}

public enum IVXVoiceMode: Int, Sendable {
    case off       = 0
    case broadcast = 1
    case spatial   = 2
    case ptt       = 3
}

public struct IVXVoiceSessionToken: Sendable {
    public let provider: IVXVoiceProvider
    public let token: String
    public let roomId: String
    public let identity: String
    public let url: String
    public let expiresAtMs: Int64
    public let canPublish: Bool
    public let canSubscribe: Bool
    public let spatial: Bool
    public let region: String
    public let providerOpts: [String: String]

    public init(provider: IVXVoiceProvider, token: String, roomId: String, identity: String,
                url: String, expiresAtMs: Int64, canPublish: Bool, canSubscribe: Bool,
                spatial: Bool, region: String, providerOpts: [String: String] = [:]) {
        self.provider = provider; self.token = token
        self.roomId = roomId; self.identity = identity
        self.url = url; self.expiresAtMs = expiresAtMs
        self.canPublish = canPublish; self.canSubscribe = canSubscribe
        self.spatial = spatial; self.region = region
        self.providerOpts = providerOpts
    }
}

public struct IVXVoiceCapability: Sendable {
    public var canPublish: Bool
    public var canSubscribe: Bool
    public var canSpatial: Bool
    public var codecs: [IVXVoiceCodec]
    public var maxPublishers: UInt32
    public var canChangeProvider: Bool
    public var canPassthroughExternal: Bool
    public var pttSupported: Bool
    public var broadcastSupported: Bool
    public var spatialSupported: Bool

    public static func ivxCanonical() -> IVXVoiceCapability {
        IVXVoiceCapability(
            canPublish: true, canSubscribe: true, canSpatial: true,
            codecs: [.opus],
            maxPublishers: 16,
            canChangeProvider: true, canPassthroughExternal: true,
            pttSupported: true, broadcastSupported: true, spatialSupported: true
        )
    }
}

public struct IVXSpeakerStateChanged: Sendable {
    public let userId: String
    public let granted: Bool
    public let mutedBySelf: Bool
    public let mutedByKernel: Bool
    public let floorSecondsRemaining: UInt32
    public let reason: String
}

public struct IVXVoiceLevelsSample: Sendable {
    public let userId: String
    public let talkingPct: UInt32
    public let silent: Bool
}

public struct IVXVoiceLevels: Sendable {
    public let samples: [IVXVoiceLevelsSample]
    public let tsMs: Int64
}

public struct IVXPoseFrameRef: Sendable {
    public let frameId: String
    public let tsMs: Int64

    public init(frameId: String, tsMs: Int64) {
        self.frameId = frameId
        self.tsMs = tsMs
    }
}

public protocol IVXVoiceProviderProtocol: AnyObject, Sendable {
    var provider: IVXVoiceProvider { get }
    var capability: IVXVoiceCapability { get }
    var currentMode: IVXVoiceMode { get }
    var isConnected: Bool { get }
    var isLocallyMuted: Bool { get }
    var hasFloor: Bool { get }

    var onConnectionChanged: (@Sendable (Bool) -> Void)? { get set }
    var onSpeakerStateChanged: (@Sendable (IVXSpeakerStateChanged) -> Void)? { get set }
    var onVoiceLevels: (@Sendable (IVXVoiceLevels) -> Void)? { get set }
    var onVoiceModeChanged: (@Sendable (IVXVoiceMode) -> Void)? { get set }
    var onProviderFailover: (@Sendable (IVXVoiceProvider) -> Void)? { get set }
    var onVoiceUnavailable: (@Sendable (String) -> Void)? { get set }

    func connect(token: IVXVoiceSessionToken) async throws
    func disconnect() async
    func setLocalMute(_ muted: Bool) async
    func requestSpeaker(topicHint: String?) async
    func releaseSpeaker() async
    func publishSpatialPosition(frameRef: IVXPoseFrameRef, x: Float, y: Float, z: Float, yawDeg: Float) async
    func setVoiceMode(_ mode: IVXVoiceMode) async
}

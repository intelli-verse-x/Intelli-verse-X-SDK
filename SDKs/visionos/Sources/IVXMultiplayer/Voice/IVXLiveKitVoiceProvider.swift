// IVXLiveKitVoiceProvider — visionOS / iOS / macOS implementation of
// IVXVoiceProviderProtocol against client-sdk-swift (LiveKit).
//
// On visionOS the renderer attaches incoming audio tracks to RealityKit
// SpatialAudioComponent for true positional audio. The kernel's
// SpatialFrame translates remote pose to RealityKit anchor space at the
// adapter boundary (see Spatial/IVXSpatialFrameRealityKit.swift).

import Foundation
import LiveKit

@MainActor
public final class IVXLiveKitVoiceProvider: IVXVoiceProviderProtocol, @unchecked Sendable {

    public let provider: IVXVoiceProvider = .livekit
    public let capability: IVXVoiceCapability
    public private(set) var currentMode: IVXVoiceMode = .off
    public private(set) var isConnected: Bool = false
    public private(set) var isLocallyMuted: Bool = false
    public private(set) var hasFloor: Bool = false

    public var onConnectionChanged:    (@Sendable (Bool) -> Void)?
    public var onSpeakerStateChanged:  (@Sendable (IVXSpeakerStateChanged) -> Void)?
    public var onVoiceLevels:          (@Sendable (IVXVoiceLevels) -> Void)?
    public var onVoiceModeChanged:     (@Sendable (IVXVoiceMode) -> Void)?
    public var onProviderFailover:     (@Sendable (IVXVoiceProvider) -> Void)?
    public var onVoiceUnavailable:     (@Sendable (String) -> Void)?

    private var room: Room?

    public init(capability: IVXVoiceCapability = .ivxCanonical()) {
        self.capability = capability
    }

    public func connect(token: IVXVoiceSessionToken) async throws {
        guard !token.token.isEmpty, !token.url.isEmpty else {
            onVoiceUnavailable?("livekit_token_missing")
            return
        }

        let room = Room()
        self.room = room
        room.add(delegate: self)

        let connectOptions = ConnectOptions(autoSubscribe: token.canSubscribe)
        let roomOptions = RoomOptions(
            adaptiveStream: true,
            dynacast: true
        )
        do {
            try await room.connect(url: token.url, token: token.token, connectOptions: connectOptions, roomOptions: roomOptions)
        } catch {
            onVoiceUnavailable?("livekit_connect_failed: \(error)")
            return
        }

        isConnected = true
        onConnectionChanged?(true)

        if token.canPublish {
            do {
                try await room.localParticipant.setMicrophone(enabled: true)
            } catch {
                onVoiceUnavailable?("livekit_mic_failed: \(error)")
            }
        }
    }

    public func disconnect() async {
        if let room = room {
            await room.disconnect()
            self.room = nil
        }
        isConnected = false
        onConnectionChanged?(false)
    }

    public func setLocalMute(_ muted: Bool) async {
        isLocallyMuted = muted
        guard let room = room else { return }
        do {
            try await room.localParticipant.setMicrophone(enabled: !muted)
        } catch {
            // Mute failures are surfaced via OnVoiceUnavailable so the
            // game UI can prompt for OS permissions if needed.
            onVoiceUnavailable?("livekit_mute_failed: \(error)")
        }
    }

    public func requestSpeaker(topicHint: String?) async {
        // Floor authority is in the kernel; this is a no-op at the SFU layer.
    }

    public func releaseSpeaker() async {
        // No-op at SFU layer; kernel revokes via SpeakerStateChanged.
    }

    public func publishSpatialPosition(frameRef: IVXPoseFrameRef, x: Float, y: Float, z: Float, yawDeg: Float) async {
        guard let room = room else { return }
        let payload: [String: Any] = [
            "frame": frameRef.frameId,
            "x": x, "y": y, "z": z,
            "yaw": yawDeg, "ts": frameRef.tsMs
        ]
        guard let bytes = try? JSONSerialization.data(withJSONObject: payload) else { return }
        try? await room.localParticipant.publish(data: bytes, options: DataPublishOptions(reliability: .lossy))
    }

    public func setVoiceMode(_ mode: IVXVoiceMode) async {
        currentMode = mode
        onVoiceModeChanged?(mode)
    }

    // ---- Kernel-driven hooks ----
    public func onKernelSpeakerStateChanged(_ ev: IVXSpeakerStateChanged) {
        hasFloor = ev.granted
        onSpeakerStateChanged?(ev)
    }

    public func onKernelProviderFailover(_ next: IVXVoiceProvider) {
        onProviderFailover?(next)
        Task { await disconnect() }
    }
}

extension IVXLiveKitVoiceProvider: RoomDelegate {
    nonisolated public func room(_ room: Room, didUpdateConnectionState newState: ConnectionState, from oldState: ConnectionState) {
        Task { @MainActor in
            self.isConnected = newState == .connected
            self.onConnectionChanged?(self.isConnected)
            if newState == .disconnected {
                self.onVoiceUnavailable?("livekit_disconnected")
            }
        }
    }

    nonisolated public func room(_ room: Room, didUpdateSpeakers speakers: [Participant]) {
        let samples = speakers.map { p in
            IVXVoiceLevelsSample(
                userId: p.identity?.stringValue ?? "",
                talkingPct: UInt32((p.audioLevel * 100).rounded()),
                silent: false
            )
        }
        let levels = IVXVoiceLevels(samples: samples, tsMs: Int64(Date().timeIntervalSince1970 * 1000))
        Task { @MainActor in self.onVoiceLevels?(levels) }
    }
}

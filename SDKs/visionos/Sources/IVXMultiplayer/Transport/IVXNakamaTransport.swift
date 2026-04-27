// IVXNakamaTransport — thin Swift wrapper around the nakama-cpp client.
//
// nakama-cpp is delivered as a binary xcframework (see
// SDKs/nakama-cpp). The C++ surface is exposed to Swift via a
// CIVXNakama Objective-C++ shim that wraps the few methods we care about:
//
//   * createDefaultClient(host, port, serverKey, ssl)
//   * authenticateDevice(deviceId)
//   * createMatch(templateId, params)
//   * joinMatch(matchId)
//   * sendMatchData(matchId, opCode, dataBytes)
//   * onMatchData(callback)
//
// This file ONLY contains the Swift-side glue. The shim header lives at
// SDKs/visionos/Sources/CIVXNakama/include/CIVXNakama.h. Build is
// configured by SwiftPM via the xcframework binary target — see Package.swift.

import Foundation

public final class IVXNakamaTransport: IVXMultiplayerClient {
    private let host: String
    private let port: Int
    private let serverKey: String
    private let useSsl: Bool

    private var connected = false
    private var sessionToken: String?

    public init(host: String, port: Int = 7350, serverKey: String = "defaultkey", useSsl: Bool = false) {
        self.host = host; self.port = port; self.serverKey = serverKey; self.useSsl = useSsl
    }

    public func connect() async throws {
        // The real implementation calls into CIVXNakama.createDefaultClient.
        // Here we mark connected; the runtime hooks up via the binary target.
        self.connected = true
    }

    public func authenticate(deviceId: String) async throws {
        guard connected else { throw IVXTransportError.notConnected }
        // Real path: CIVXNakama.authenticateDevice(deviceId)
        self.sessionToken = "mock-session-" + deviceId
    }

    public func createMatch(templateId: String, gameId: String, init_: Encodable? = nil) async throws -> IVXMultiplayerSession {
        guard sessionToken != nil else { throw IVXTransportError.notAuthenticated }
        let matchId = "match-\(UUID().uuidString.prefix(8))"
        return IVXNakamaSession(transport: self, matchId: matchId)
    }

    public func joinMatch(matchId: String) async throws -> IVXMultiplayerSession {
        guard sessionToken != nil else { throw IVXTransportError.notAuthenticated }
        return IVXNakamaSession(transport: self, matchId: matchId)
    }

    public func disconnect() async {
        self.connected = false
        self.sessionToken = nil
    }

    /// Internal: send raw bytes for a given match. Replace with the real
    /// CIVXNakama.sendMatchData call once the xcframework is wired.
    func _sendBytes(matchId: String, op: UInt32, bytes: Data) async throws {
        guard connected else { throw IVXTransportError.notConnected }
        // CIVXNakama.sendMatchData(matchId, op, bytes.bytes, bytes.count)
    }

    /// Internal: register a callback for inbound match data. The real
    /// xcframework dispatches into a C++ callback; we re-marshal here.
    func _onIncoming(matchId: String, handler: @escaping (UInt32, Data, String) -> Void) -> IVXSubscription {
        let token = IVXNakamaSubscription { /* stop */ }
        // CIVXNakama.onMatchData(matchId, { op, data, sender in handler(op, data, sender) })
        return token
    }
}

public enum IVXTransportError: Error {
    case notConnected
    case notAuthenticated
    case sendFailed(String)
    case decodeFailed(String)
}

final class IVXNakamaSubscription: IVXSubscription {
    private let stop: () -> Void
    init(stop: @escaping () -> Void) { self.stop = stop }
    func cancel() { stop() }
}

final class IVXNakamaSession: IVXMultiplayerSession {
    let matchId: String
    private weak var transport: IVXNakamaTransport?
    private var handlers: [UInt32: [(Data, String) -> Void]] = [:]

    init(transport: IVXNakamaTransport, matchId: String) {
        self.transport = transport
        self.matchId = matchId
        _ = transport._onIncoming(matchId: matchId) { [weak self] op, data, sender in
            self?.handlers[op]?.forEach { $0(data, sender) }
        }
    }

    func send<P: Encodable>(_ op: UInt32, _ payload: P) async throws {
        guard let t = transport else { throw IVXTransportError.notConnected }
        let env = IVXOutgoingEnvelope(seq: 0, ts_ms: Int64(Date().timeIntervalSince1970 * 1000), op: op, payload: payload)
        let data = try JSONEncoder().encode(env)
        try await t._sendBytes(matchId: matchId, op: op, bytes: data)
    }

    func subscribe<P: Decodable>(_ op: UInt32, _ handler: @escaping (IVXEnvelope<P>) -> Void) -> IVXSubscription {
        let h: (Data, String) -> Void = { data, _sender in
            do {
                let env = try JSONDecoder().decode(IVXEnvelope<P>.self, from: data)
                handler(env)
            } catch {
                NSLog("[IVXSession] decode failed op=\(op) err=\(error)")
            }
        }
        handlers[op, default: []].append(h)
        return IVXNakamaSubscription { [weak self] in
            self?.handlers[op]?.removeAll(where: { _ in true })
        }
    }

    func leave() async throws {
        // CIVXNakama.leaveMatch(matchId)
        handlers.removeAll()
    }
}

private struct IVXOutgoingEnvelope<P: Encodable>: Encodable {
    let seq: UInt64
    let ts_ms: Int64
    let op: UInt32
    let payload: P
}

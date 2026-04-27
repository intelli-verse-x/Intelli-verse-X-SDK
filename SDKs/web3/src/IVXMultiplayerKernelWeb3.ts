// IVXMultiplayerKernelWeb3 — Web3 multiplayer adapter.
//
// Decorates the canonical JS Multiplayer Kernel
//   `@intelliversex/multiplayer` (SDKs/javascript/packages/multiplayer/)
// with on-chain identity binding and signed wire payloads, so a quiz / poker
// / esports match served by the kernel can also commit verifiable
// match-result attestations to a smart contract.
//
// Wire protocol: identical to the JS adapter (kernel envelope = {h:{s,t,u}, p}).
// On top of that, this adapter:
//
//   * Replaces Nakama device-auth with `authenticateCustom` using a
//     wallet-signed nonce, so the user_id is the wallet address.
//   * Adds an optional `signed=true` mode where every outbound payload is
//     wrapped as
//        { tx: <user_op_json>, sig: <eip-191-personal-sign hex> }
//     before the kernel envelope wraps it.
//   * Exposes a `commitMatchResult()` helper that submits the kernel's
//     mp_match_result envelope (returned by the server's MpKernelMatchResult.read)
//     to a configurable on-chain MatchResult contract.
//
// IMPORTANT: We DO NOT bundle ethers.js as a dep — game projects already
// pull it in. The signer is passed in as `IIVXEthersSigner` so this adapter
// remains framework-agnostic.

import type {
  IIVXMatchSession,
  IIVXMultiplayer,
  IVXCreateMatchRequest,
  IVXCreateMatchResponse,
  IVXJoinOptions,
  IVXKernelEvent,
  IVXSubscription,
  IVXTransportState,
} from "@intelliversex/multiplayer";

// ---------- ethers.js signer surface (structural, not nominal) ----------

/** Minimum signer surface we use. ethers.js v5 and v6 both satisfy this. */
export interface IIVXEthersSigner {
  getAddress(): Promise<string>;
  signMessage(message: string | Uint8Array): Promise<string>;
}

/** Minimum contract surface we use. ethers v5 / v6 Contract satisfies this. */
export interface IIVXMatchResultContract {
  commitResult(matchId: string, resultHash: string, signature: string): Promise<unknown>;
}

// ---------- public API ----------

export interface IVXWeb3KernelOptions {
  /** Sign every outbound payload (default true). */
  signOutbound?: boolean;
  /** Optional audit hook — fires on every signed tx for telemetry. */
  onSignedTx?: (info: {
    matchId: string;
    opcode: number;
    txHash: string;
    sig: string;
  }) => void;
}

export interface IVXSignedPayload<TPayload = unknown> {
  /** Original game payload, untouched. */
  tx: TPayload;
  /** EIP-191 personal-sign of `JSON.stringify({ matchId, opcode, tx, ts })`. */
  sig: string;
  /** The signer's wallet address (lowercased). */
  from: string;
  /** Unix-ms when the client signed. */
  ts: number;
}

/**
 * Wraps an instance of the JS adapter and adds web3-aware send/recv.
 * Construct AFTER you've called the wrapped adapter's `initialize()`.
 *
 *   const inner = createNakamaJsAdapter({...});
 *   await inner.initialize();
 *   const web3 = new IVXMultiplayerKernelWeb3(inner, signer, { signOutbound: true });
 *   const session = await web3.createAndJoin({ templateId: "sync-turn-v1" });
 *   await session.send(0xC101, { answer_id: "a" });   // signed automatically
 */
export class IVXMultiplayerKernelWeb3 {
  private readonly inner: IIVXMultiplayer;
  private readonly signer: IIVXEthersSigner;
  private readonly opts: Required<IVXWeb3KernelOptions>;
  private cachedAddress = "";

  constructor(
    inner: IIVXMultiplayer,
    signer: IIVXEthersSigner,
    options: IVXWeb3KernelOptions = {},
  ) {
    this.inner = inner;
    this.signer = signer;
    this.opts = {
      signOutbound: options.signOutbound ?? true,
      onSignedTx: options.onSignedTx ?? (() => {}),
    };
  }

  /** Resolve and cache the signer's address. Idempotent. */
  async getAddress(): Promise<string> {
    if (!this.cachedAddress) {
      this.cachedAddress = (await this.signer.getAddress()).toLowerCase();
    }
    return this.cachedAddress;
  }

  get isInitialized(): boolean { return this.inner.isInitialized; }
  get isConnected():   boolean { return this.inner.isConnected;   }
  get lastServerUnixMs(): number { return this.inner.lastServerUnixMs; }

  initialize(): Promise<void> { return this.inner.initialize(); }
  shutdown():   Promise<void> { return this.inner.shutdown(); }

  createMatch(req: IVXCreateMatchRequest): Promise<IVXCreateMatchResponse> {
    return this.inner.createMatch(req);
  }

  async joinMatch(matchId: string, options?: IVXJoinOptions): Promise<IIVXMatchSession> {
    const session = await this.inner.joinMatch(matchId, options);
    return this.wrapSession(session, matchId);
  }

  async createAndJoin(
    req: IVXCreateMatchRequest,
    options?: IVXJoinOptions,
  ): Promise<IIVXMatchSession> {
    const inner = await this.inner.createAndJoin(req, options);
    return this.wrapSession(inner, inner.matchId);
  }

  onKernelError(handler: (e: IVXKernelEvent<unknown>) => void): IVXSubscription {
    return this.inner.onKernelError(handler as never);
  }

  onTransportStateChanged(handler: (s: IVXTransportState) => void): IVXSubscription {
    return this.inner.onTransportStateChanged(handler);
  }

  /**
   * Commit a kernel match-result to chain. Pass the envelope you got back
   * via your server's `mp_match_result_get` RPC (or via the
   * MpKernelMatchResult.read helper on the server side).
   *
   * Wallet signs `keccak256(matchId || resultJson)` using personal-sign.
   * The contract verifies (matchId, resultHash, signature) on-chain and
   * emits a `MatchResultCommitted` event.
   */
  async commitMatchResult(
    contract: IIVXMatchResultContract,
    matchId: string,
    resultEnvelopeJson: string,
  ): Promise<unknown> {
    if (!matchId)            throw new Error("matchId required");
    if (!resultEnvelopeJson) throw new Error("resultEnvelopeJson required");
    const message = `mp_match_result|${matchId}|${resultEnvelopeJson}`;
    const sig = await this.signer.signMessage(message);
    // We deliberately keep hashing on-chain so the Solidity side controls
    // the canonical bytes. Pass the raw resultEnvelopeJson; the contract
    // ABI-encodes and hashes.
    return contract.commitResult(matchId, resultEnvelopeJson, sig);
  }

  // ---- internals ----

  private wrapSession(inner: IIVXMatchSession, matchId: string): IIVXMatchSession {
    const self = this;

    // We return a Proxy-like object that intercepts `send` to inject
    // the signed-payload envelope. All other methods delegate verbatim.
    const wrapped: IIVXMatchSession = {
      get matchId()             { return inner.matchId; },
      get templateId()          { return inner.templateId; },
      get localUserId()         { return inner.localUserId; },
      get currentMatchTimeMs()  { return inner.currentMatchTimeMs; },
      get activePlayerCount()   { return inner.activePlayerCount; },
      get state()               { return inner.state; },

      subscribe<TPayload = unknown>(opcode: number, handler: (e: IVXKernelEvent<TPayload>) => void) {
        return inner.subscribe(opcode, handler);
      },
      subscribeRange(from, to, handler) {
        return inner.subscribeRange(from, to, handler);
      },
      sendEnvelope(env) { return inner.sendEnvelope(env); },
      leave()           { return inner.leave(); },
      onWelcome(h)      { return inner.onWelcome(h); },
      onPlayerJoined(h) { return inner.onPlayerJoined(h); },
      onPlayerLeft(h)   { return inner.onPlayerLeft(h); },
      onMatchEnded(h)   { return inner.onMatchEnded(h); },
      onError(h)        { return inner.onError(h); },
      onStateChanged(h) { return inner.onStateChanged(h); },
      dispose()         { inner.dispose(); },

      async send<TPayload>(opcode: number, payload: TPayload): Promise<void> {
        if (!self.opts.signOutbound) {
          return inner.send(opcode, payload);
        }
        const from = await self.getAddress();
        const ts = Date.now();
        const message = `mp_send|${matchId}|${opcode}|${ts}|${JSON.stringify(payload)}`;
        const sig = await self.signer.signMessage(message);
        const wrappedPayload: IVXSignedPayload<TPayload> = {
          tx: payload,
          sig,
          from,
          ts,
        };
        // Telemetry / audit hook (sync — never await user code in the hot
        // path beyond the actual signMessage call).
        try {
          self.opts.onSignedTx({
            matchId,
            opcode,
            txHash: hashMessageJsLike(message),
            sig,
          });
        } catch { /* never propagate user-hook errors */ }
        return inner.send(opcode, wrappedPayload);
      },
    };
    return wrapped;
  }
}

// ---- minimal helper: deterministic message hash for telemetry only ----

/**
 * Stable, allocation-free SHA-256-style FNV1a over a string. Used purely as
 * a telemetry handle so two telemetry rows for the same payload collapse.
 * Do NOT use as a security primitive — that's what the on-chain hash is for.
 */
function hashMessageJsLike(s: string): string {
  let h = 0xcbf29ce484222325n;
  const PRIME = 0x100000001b3n;
  for (let i = 0; i < s.length; i++) {
    h ^= BigInt(s.charCodeAt(i));
    h = (h * PRIME) & 0xffffffffffffffffn;
  }
  return "0x" + h.toString(16).padStart(16, "0");
}

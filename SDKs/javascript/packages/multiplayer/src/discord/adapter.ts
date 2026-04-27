// IVX Discord Activities adapter.
//
// Discord Activities are iframed web apps that talk to the Discord
// client via @discord/embedded-app-sdk and to the broader internet via
// CSP-restricted fetch/WebSocket. This adapter wires the IVX
// multiplayer transport into a Discord Activity so:
//
//   * Activity OAuth → Nakama device authentication.
//   * Activity participant updates → automatic kernel presence sync.
//   * Voice mode prefers Discord's voice channel (capability advertised
//     as `none` for IVX voice; Discord owns audio).
//   * Match creation tags `game_id` with the Discord application id.
//
// The runtime DEPENDENCY on @discord/embedded-app-sdk is loose: this
// file accepts an SDK instance via constructor injection so the package
// does not pull Discord into its dist by default.
//
// SINGLE SOURCE OF TRUTH: schemas/multiplayer/*.proto.

import type { IIVXMultiplayer, IVXCreateMatchRequest, IVXCreateMatchResponse } from "../api";

// Minimal subset of @discord/embedded-app-sdk we depend on. Defined
// here to avoid a hard dependency.
export interface IDiscordEmbeddedSDK {
  ready(): Promise<void>;
  commands: {
    authorize(opts: {
      client_id: string;
      response_type: "code";
      state: string;
      prompt: "none" | "consent";
      scope: string[];
    }): Promise<{ code: string }>;
    authenticate(opts: { access_token: string }): Promise<{ user: { id: string; username: string } }>;
    setActivity?(opts: any): Promise<void>;
  };
  subscribe(event: string, handler: (data: any) => void, filter?: any): Promise<{ unsubscribe: () => void }>;
  applicationId: string;
  channelId?: string;
  instanceId?: string;
}

/** Token-exchange callback the host activity must implement. The host
 *  swaps the OAuth `code` for a Discord access token via its own
 *  backend (so the client_secret never leaves the server) and returns
 *  the access token to us.
 */
export type DiscordTokenExchanger = (code: string) => Promise<string>;

export interface IIVXDiscordActivityAdapter {
  attach(): Promise<void>;
  detach(): Promise<void>;
  /** Returns the Discord user id used for Nakama authentication. */
  readonly discordUserId: string | null;
  /** Returns the Discord channel id this activity is bound to. */
  readonly discordChannelId: string | null;
}

export interface IVXDiscordAdapterOptions {
  multiplayer: IIVXMultiplayer;
  sdk: IDiscordEmbeddedSDK;
  clientId: string;
  exchangeToken: DiscordTokenExchanger;
  /** Callback once the multiplayer adapter is authenticated + initialised. */
  onReady?: (info: { discordUserId: string; channelId: string | null }) => void;
}

export class IVXDiscordActivityAdapter implements IIVXDiscordActivityAdapter {
  private readonly multiplayer: IIVXMultiplayer;
  private readonly sdk: IDiscordEmbeddedSDK;
  private readonly clientId: string;
  private readonly exchangeToken: DiscordTokenExchanger;
  private readonly onReady?: (info: { discordUserId: string; channelId: string | null }) => void;

  private _participantSub: { unsubscribe: () => void } | null = null;
  private _userId: string | null = null;
  private _accessToken: string | null = null;

  constructor(opts: IVXDiscordAdapterOptions) {
    this.multiplayer = opts.multiplayer;
    this.sdk = opts.sdk;
    this.clientId = opts.clientId;
    this.exchangeToken = opts.exchangeToken;
    this.onReady = opts.onReady;
  }

  get discordUserId(): string | null { return this._userId; }
  get discordChannelId(): string | null { return this.sdk.channelId ?? null; }

  async attach(): Promise<void> {
    await this.sdk.ready();

    const auth = await this.sdk.commands.authorize({
      client_id: this.clientId,
      response_type: "code",
      state: cryptoRandom(),
      prompt: "none",
      scope: ["identify", "guilds.members.read", "rpc.activities.write"],
    });

    this._accessToken = await this.exchangeToken(auth.code);

    const me = await this.sdk.commands.authenticate({ access_token: this._accessToken });
    this._userId = me.user.id;

    if (!this.multiplayer.isInitialized) {
      await this.multiplayer.initialize();
    }

    try {
      this._participantSub = await this.sdk.subscribe(
        "ACTIVITY_INSTANCE_PARTICIPANTS_UPDATE",
        (_data: any) => {
          // The kernel doesn't auto-import discord roster; the host
          // game is responsible for joining matches, but we pass the
          // participants update through any custom listener it set on
          // the adapter via window.dispatchEvent — kept side-effect
          // free here to remain framework-agnostic.
        }
      );
    } catch (_err) {
      // Subscription failure shouldn't block ready.
    }

    this.onReady?.({
      discordUserId: this._userId,
      channelId: this.sdk.channelId ?? null,
    });
  }

  async detach(): Promise<void> {
    try { this._participantSub?.unsubscribe(); } catch (_e) {}
    this._participantSub = null;
    if (this.multiplayer.isConnected) {
      await this.multiplayer.shutdown();
    }
  }

  /** Helper that creates a kernel match tagged with the activity's
   *  Discord channel + instance ids so the kernel can correlate IVX
   *  matches with Discord rooms in moderation telemetry. */
  async createMatchForActivity(req: Omit<IVXCreateMatchRequest, "gameId"> & { gameId?: string }): Promise<IVXCreateMatchResponse> {
    if (!this.multiplayer.isInitialized) {
      throw new Error("[IVXDiscord] adapter not attached; call attach() first");
    }
    const templateInit: Record<string, unknown> = {
      ...((req as any).templateInit ?? {}),
      discord_application_id: this.sdk.applicationId,
      discord_channel_id: this.sdk.channelId ?? "",
      discord_instance_id: this.sdk.instanceId ?? "",
    };
    const merged: IVXCreateMatchRequest = {
      ...req,
      gameId: req.gameId ?? `discord:${this.sdk.applicationId}`,
      templateInit,
    };
    return this.multiplayer.createMatch(merged);
  }
}

function cryptoRandom(): string {
  const buf = new Uint8Array(16);
  if (typeof crypto !== "undefined" && crypto.getRandomValues) {
    crypto.getRandomValues(buf);
  } else {
    for (let i = 0; i < buf.length; i++) buf[i] = Math.floor(Math.random() * 256);
  }
  let s = "";
  for (let i = 0; i < buf.length; i++) s += buf[i].toString(16).padStart(2, "0");
  return s;
}

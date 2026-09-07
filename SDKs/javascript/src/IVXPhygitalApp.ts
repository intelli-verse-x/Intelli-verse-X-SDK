// Copyright (c) 2026 Intelliverse. MIT License.

/** Places where a host application can expose the same app. These are adapter labels, not bundled integrations. */
export type IVXAppChannel = 'mobile' | 'web' | 'kiosk' | 'whatsapp' | 'chat' | 'voice';
/** An action a registered channel adapter can deliver. */
export type IVXAppCapability = 'quest' | 'ad' | 'conversation';

/** A portable app definition. Enable only channels and actions your deployment supports. */
export interface IVXPhygitalManifest {
  readonly appId: string;
  readonly name: string;
  readonly channels: Readonly<Partial<Record<IVXAppChannel, readonly IVXAppCapability[]>>>;
}

/** Intent sent to a host-owned adapter. A quest submission requests verification; it does not grant a reward. */
export type IVXAppIntent =
  | { readonly kind: 'quest'; readonly action: 'start'; readonly questId: string }
  | { readonly kind: 'quest'; readonly action: 'submit'; readonly questId: string; readonly evidenceId: string }
  | { readonly kind: 'ad'; readonly campaignId: string; readonly placementId: string }
  | { readonly kind: 'conversation'; readonly text: string; readonly conversationId?: string };

/** One operation, with a host-supplied idempotency key reused for retries. No credentials or user PII belong here. */
export interface IVXPhygitalRequest {
  readonly appId: string;
  readonly operationId: string;
  readonly channel: IVXAppChannel;
  readonly intent: IVXAppIntent;
}

/** Delivery acknowledgement only. Accepted is not proof of quest completion, an ad impression, or a reward grant. */
export interface IVXPhygitalResult {
  readonly operationId: string;
  readonly status: 'accepted' | 'pending' | 'rejected';
  readonly referenceId?: string;
}

/** Host-owned bridge to a verified backend/provider. Implement authentication, authorization and durable deduplication there. */
export interface IVXPhygitalAdapter {
  readonly channel: IVXAppChannel;
  readonly capabilities: readonly IVXAppCapability[];
  handle(request: IVXPhygitalRequest): Promise<IVXPhygitalResult>;
}

const CHANNELS: readonly IVXAppChannel[] = ['mobile', 'web', 'kiosk', 'whatsapp', 'chat', 'voice'];
const CAPABILITIES: readonly IVXAppCapability[] = ['quest', 'ad', 'conversation'];

function identifier(value: unknown, field: string): asserts value is string {
  if (typeof value !== 'string' || !/^[A-Za-z0-9][A-Za-z0-9._:-]{0,127}$/.test(value)) {
    throw new Error(`IVXPhygitalApp: ${field} must be a 1–128 character identifier`);
  }
}

function capabilities(value: readonly IVXAppCapability[]): readonly IVXAppCapability[] {
  if (!Array.isArray(value) || value.length === 0 || value.some(v => !CAPABILITIES.includes(v))) {
    throw new Error('IVXPhygitalApp: declare at least one supported capability');
  }
  return Object.freeze([...new Set(value)]);
}

function intentCopy(intent: IVXAppIntent): IVXAppIntent {
  if (!intent || typeof intent !== 'object') throw new Error('IVXPhygitalApp: intent is required');
  switch (intent.kind) {
    case 'quest':
      identifier(intent.questId, 'questId');
      if (intent.action === 'start') return Object.freeze({ kind: 'quest', action: 'start', questId: intent.questId });
      if (intent.action === 'submit') {
        identifier(intent.evidenceId, 'evidenceId');
        return Object.freeze({ kind: 'quest', action: 'submit', questId: intent.questId, evidenceId: intent.evidenceId });
      }
      throw new Error('IVXPhygitalApp: unsupported quest action');
    case 'ad':
      identifier(intent.campaignId, 'campaignId');
      identifier(intent.placementId, 'placementId');
      return Object.freeze({ kind: 'ad', campaignId: intent.campaignId, placementId: intent.placementId });
    case 'conversation': {
      if (typeof intent.text !== 'string' || !intent.text.trim() || intent.text.length > 4096) {
        throw new Error('IVXPhygitalApp: conversation text must contain 1–4096 characters');
      }
      if (intent.conversationId !== undefined) identifier(intent.conversationId, 'conversationId');
      return Object.freeze({ kind: 'conversation', text: intent.text, ...(intent.conversationId === undefined ? {} : { conversationId: intent.conversationId }) });
    }
    default:
      throw new Error('IVXPhygitalApp: unsupported intent');
  }
}

/**
 * Routes App Quests, App Ads and conversations through explicitly registered adapters.
 * This helper performs no network requests, stores no identity and never changes wallets.
 * The JavaScript reference implementation can run in a kiosk/web app or a server-side channel bridge.
 */
export class IVXPhygitalApp {
  /** Immutable, validated app definition. */
  readonly manifest: IVXPhygitalManifest;
  private readonly _adapters = new Map<IVXAppChannel, IVXPhygitalAdapter>();

  /** Build a router using only the host's declared and installed capabilities. Throws for invalid configuration. */
  constructor(manifest: IVXPhygitalManifest, adapters: readonly IVXPhygitalAdapter[] = []) {
    if (!manifest || typeof manifest !== 'object') throw new Error('IVXPhygitalApp: manifest is required');
    identifier(manifest.appId, 'appId');
    if (typeof manifest.name !== 'string' || !manifest.name.trim() || manifest.name.length > 120) {
      throw new Error('IVXPhygitalApp: name must contain 1–120 characters');
    }
    if (!manifest.channels || typeof manifest.channels !== 'object' || Array.isArray(manifest.channels)) {
      throw new Error('IVXPhygitalApp: channels are required');
    }
    const channels: Partial<Record<IVXAppChannel, readonly IVXAppCapability[]>> = {};
    for (const [channel, values] of Object.entries(manifest.channels)) {
      if (!CHANNELS.includes(channel as IVXAppChannel)) throw new Error('IVXPhygitalApp: unsupported channel');
      channels[channel as IVXAppChannel] = capabilities(values);
    }
    if (Object.keys(channels).length === 0) throw new Error('IVXPhygitalApp: declare at least one channel');
    this.manifest = Object.freeze({ appId: manifest.appId, name: manifest.name, channels: Object.freeze(channels) });
    for (const adapter of adapters) {
      if (!adapter || !CHANNELS.includes(adapter.channel) || !channels[adapter.channel] || typeof adapter.handle !== 'function') {
        throw new Error('IVXPhygitalApp: adapter must handle a declared channel');
      }
      if (this._adapters.has(adapter.channel)) throw new Error('IVXPhygitalApp: duplicate channel adapter');
      this._adapters.set(adapter.channel, Object.freeze({
        channel: adapter.channel,
        capabilities: capabilities(adapter.capabilities),
        handle: adapter.handle.bind(adapter),
      }));
    }
  }

  /** True only when both the manifest and an installed adapter support this action. */
  supports(channel: IVXAppChannel, capability: IVXAppCapability): boolean {
    return Boolean(CHANNELS.includes(channel) && CAPABILITIES.includes(capability) && this.manifest.channels[channel]?.includes(capability) && this._adapters.get(channel)?.capabilities.includes(capability));
  }

  /**
   * Validate and dispatch a single operation. Unknown fields are stripped; provider errors propagate.
   * Reuse operationId when retrying. The trusted backend must deduplicate and validate evidence,
   * consent, campaign eligibility, and identity before any reward or advertising side effect.
   */
  async dispatch(channel: IVXAppChannel, operationId: string, intent: IVXAppIntent): Promise<IVXPhygitalResult> {
    identifier(operationId, 'operationId');
    const copy = intentCopy(intent);
    if (!this.supports(channel, copy.kind)) throw new Error(`IVXPhygitalApp: ${copy.kind} is unavailable on ${channel}`);
    const request = Object.freeze({ appId: this.manifest.appId, operationId, channel, intent: copy });
    const result = await this._adapters.get(channel)!.handle(request);
    if (!result || result.operationId !== operationId || !['accepted', 'pending', 'rejected'].includes(result.status)) {
      throw new Error('IVXPhygitalApp: invalid or mismatched adapter acknowledgement');
    }
    if (result.referenceId !== undefined) identifier(result.referenceId, 'referenceId');
    return Object.freeze({ operationId, status: result.status, ...(result.referenceId === undefined ? {} : { referenceId: result.referenceId }) });
  }
}

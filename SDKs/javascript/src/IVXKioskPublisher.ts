/** One hosted display and optional phone controller. Keep operator credentials out of both URLs. */
export interface IVXKioskExperience {
  readonly schemaVersion: 1;
  readonly appId: string;
  readonly name: string;
  readonly kioskUrl: string;
  readonly controllerUrl?: string | null;
  readonly sessionSeconds: number;
}

/** A mapping acknowledgement, not proof that a device rendered the app. */
export interface IVXKioskPublication {
  readonly machineNo: string;
  readonly revision: number;
  readonly enabled: boolean;
  readonly manifest: IVXKioskExperience | null;
  readonly playerReady: boolean;
  readonly delivery: 'available-on-next-open' | 'disabled';
  readonly detail: string;
}

/** Server-side credential provider. Use an operator-owned key with machines:read/write. */
export interface IVXKioskPublisherOptions {
  readonly apiKey: () => string | Promise<string>;
  readonly baseUrl?: string;
  readonly fetch?: typeof fetch;
}

function httpsUrl(value: string): string {
  if (typeof value !== 'string' || value.length > 2048) throw new Error('Invalid HTTPS URL');
  const url = new URL(value);
  if (url.protocol !== 'https:' || url.username || url.password || url.port ||
      !url.hostname.includes('.') || url.hostname.endsWith('.') || /[\\\s]/.test(value) ||
      /^[\d.]+$/.test(url.hostname) || /\.(?:localhost|local|internal)$/.test(url.hostname)) {
    throw new Error('Use a public HTTPS hostname on port 443 without credentials');
  }
  return value;
}

/** Validate the v1 KioskX wire manifest before attempting a publish. */
export function validateIVXKioskExperience(value: IVXKioskExperience): IVXKioskExperience {
  if (!value || value.schemaVersion !== 1 || typeof value.appId !== 'string' || !/^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$/.test(value.appId) ||
      typeof value.name !== 'string' || !value.name.trim() || value.name.length > 80 ||
      !Number.isInteger(value.sessionSeconds) || value.sessionSeconds < 30 || value.sessionSeconds > 900) {
    throw new Error('Invalid v1 kiosk experience manifest');
  }
  return Object.freeze({ schemaVersion: 1, appId: value.appId, name: value.name.trim(),
    kioskUrl: httpsUrl(value.kioskUrl), controllerUrl: value.controllerUrl == null ? null : httpsUrl(value.controllerUrl),
    sessionSeconds: value.sessionSeconds });
}

/**
 * Server-side KioskX publishing client. Read a revision, then explicitly publish or disable.
 * No automatic retries: a timeout may follow a committed write; read again before retrying.
 * Requires the matching KioskX API and experiencePlayerV1 ZHZN release.
 */
export class IVXKioskPublisher {
  private readonly _options: IVXKioskPublisherOptions;
  private readonly _base: string;
  constructor(options: IVXKioskPublisherOptions) {
    if (typeof window !== 'undefined') throw new Error('Kiosk publishing belongs on your server; never bundle operator keys');
    if (!options || typeof options.apiKey !== 'function') throw new Error('A server-side API key provider is required');
    this._options = options;
    this._base = httpsUrl(options.baseUrl || 'https://api.kiosk-x.ai').replace(/\/$/, '');
    const url = new URL(this._base);
    if (url.pathname !== '/' || url.search || url.hash) throw new Error('baseUrl must be an HTTPS origin');
  }

  /** Fetch current mapping and device readiness. */
  get(machineNo: string): Promise<IVXKioskPublication> { return this.request(machineNo); }

  /** Publish a mapping using the revision from get(). A concurrent edit returns 409. */
  publish(machineNo: string, expectedRevision: number, manifest: IVXKioskExperience, enabled = true): Promise<IVXKioskPublication> {
    return this.request(machineNo, { expectedRevision, enabled, manifest: validateIVXKioskExperience(manifest) });
  }

  /** Disable playback and clear the mapping. Existing bounded sessions finish or can be closed locally. */
  disable(machineNo: string, expectedRevision: number): Promise<IVXKioskPublication> {
    return this.request(machineNo, { expectedRevision, enabled: false, manifest: null });
  }

  private async request(machineNo: string, body?: {expectedRevision: number; enabled: boolean; manifest: IVXKioskExperience | null}): Promise<IVXKioskPublication> {
    if (!/^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$/.test(machineNo)) throw new Error('Invalid machine number');
    if (body && (!Number.isSafeInteger(body.expectedRevision) || body.expectedRevision < 0 || typeof body.enabled !== 'boolean')) throw new Error('Invalid publication revision or enabled flag');
    const key = await this._options.apiKey();
    if (!key || /[\r\n]/.test(key)) throw new Error('API key provider returned an invalid key');
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 15000);
    try {
      const response = await (this._options.fetch || fetch)(`${this._base}/api/v1/machines/${encodeURIComponent(machineNo)}/experience`, {
        method: body ? 'PUT' : 'GET', headers: {'X-API-Key': key, 'Content-Type': 'application/json'},
        ...(body ? {body: JSON.stringify(body)} : {}), signal: controller.signal, redirect: 'error',
      });
      if (!response.ok) throw new Error(`KioskX publishing failed (${response.status}); reload the mapping before retrying`);
      const envelope = await response.json();
      const data = envelope?.data;
      if (envelope?.code !== 200 || !data || data.machineNo !== machineNo || !Number.isSafeInteger(data.revision) || data.revision < 0 || typeof data.enabled !== 'boolean' || typeof data.playerReady !== 'boolean' || !['available-on-next-open', 'disabled'].includes(data.delivery) || typeof data.detail !== 'string') {
        throw new Error('Invalid KioskX publication acknowledgement');
      }
      return Object.freeze({...data, manifest: data.manifest === null ? null : validateIVXKioskExperience(data.manifest)});
    } finally { clearTimeout(timer); }
  }
}

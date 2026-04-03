import { IVXSatori, type IVXSatoriEvent } from '../sdk-stub';
import { IVX_CONFIG } from '../config';

let started = false;

export async function initSatori(): Promise<void> {
  if (started) return;
  started = true;
  const url = import.meta.env.VITE_SATORI_URL || `https://${IVX_CONFIG.serverHost}/v1/satori`;
  const apiKey = import.meta.env.VITE_SATORI_API_KEY || '';
  if (!apiKey) {
    console.warn('[{{game_id}}] VITE_SATORI_API_KEY missing; Satori init skipped.');
    return;
  }
  try {
    IVXSatori.getInstance().initialize({ satoriUrl: url, apiKey });
  } catch (e) {
    console.warn('Satori init failed', e);
  }
}

export async function identifySatoriUser(userId: string): Promise<void> {
  if (!userId) return;
  try {
    const s = IVXSatori.getInstance();
    if (!s.isInitialized) return;
    await s.authenticate(userId, { game_id: '{{game_id}}' }, { platform: 'web' });
  } catch {
    /* non-fatal */
  }
}

export async function trackEvent(name: string, metadata?: Record<string, string>): Promise<void> {
  const gameId = '{{game_id}}';
  const ev: IVXSatoriEvent = {
    name,
    metadata: { game_id: gameId, ...Object.fromEntries(Object.entries(metadata ?? {}).map(([k, v]) => [k, String(v)])) },
  };
  try {
    const s = IVXSatori.getInstance();
    if (s.isInitialized) await s.captureEvents([ev]);
    else if (import.meta.env.DEV) console.debug('[analytics]', name, ev.metadata);
  } catch {
    if (import.meta.env.DEV) console.debug('[analytics]', name, ev.metadata);
  }
}

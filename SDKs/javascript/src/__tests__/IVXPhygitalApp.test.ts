import { describe, expect, it, vi } from 'vitest';
import { IVXPhygitalApp, type IVXPhygitalRequest, type IVXAppChannel, type IVXAppIntent, type IVXPhygitalAdapter, type IVXPhygitalManifest } from '../IVXPhygitalApp';

const manifest: IVXPhygitalManifest = { appId: 'trivia-app', name: 'Trivia', channels: { kiosk: ['quest', 'ad'], whatsapp: ['conversation'], voice: ['conversation'] } };
const handler = () => vi.fn(async (r: IVXPhygitalRequest) => ({ operationId: r.operationId, status: 'pending' as const, referenceId: 'delivery-1' }));

describe('IVXPhygitalApp', () => {
  it('fails closed for a declared channel without an adapter', async () => {
    const app = new IVXPhygitalApp(manifest);
    expect(app.supports('kiosk', 'quest')).toBe(false);
    expect(app.supports('__proto__' as IVXAppChannel, 'quest')).toBe(false);
    await expect(app.dispatch('kiosk', 'op-1', { kind: 'quest', action: 'start', questId: 'q-1' })).rejects.toThrow('unavailable');
  });
  it('requires capability support in both manifest and adapter', async () => {
    const handle = handler();
    const app = new IVXPhygitalApp(manifest, [{ channel: 'kiosk', capabilities: ['quest', 'conversation'], handle }]);
    expect(app.supports('kiosk', 'ad')).toBe(false);
    expect(app.supports('kiosk', 'conversation')).toBe(false);
    await expect(app.dispatch('kiosk', 'op-1', { kind: 'ad', campaignId: 'c-1', placementId: 'p-1' })).rejects.toThrow();
    expect(handle).not.toHaveBeenCalled();
  });
  it.each(['mobile', 'web', 'kiosk', 'whatsapp', 'chat', 'voice'] as IVXAppChannel[])('routes a conversation on %s when explicitly installed', async channel => {
    const handle = handler();
    const app = new IVXPhygitalApp({ appId: 'app', name: 'App', channels: { [channel]: ['conversation'] } }, [{ channel, capabilities: ['conversation'], handle }]);
    expect(await app.dispatch(channel, 'message-1', { kind: 'conversation', text: 'Find a quest', conversationId: 'thread-1' })).toEqual({ operationId: 'message-1', status: 'pending', referenceId: 'delivery-1' });
    expect(handle.mock.calls[0][0]).toEqual({ appId: 'app', operationId: 'message-1', channel, intent: { kind: 'conversation', text: 'Find a quest', conversationId: 'thread-1' } });
  });
  it('passes quest evidence references and campaign placement IDs without inventing rewards', async () => {
    const handle = handler();
    const app = new IVXPhygitalApp(manifest, [{ channel: 'kiosk', capabilities: ['quest', 'ad'], handle }]);
    await app.dispatch('kiosk', 'quest-op', { kind: 'quest', action: 'submit', questId: 'q-1', evidenceId: 'receipt-1' });
    await app.dispatch('kiosk', 'ad-op', { kind: 'ad', campaignId: 'c-1', placementId: 'screen-1' });
    expect(handle.mock.calls[0][0].intent).toMatchObject({ evidenceId: 'receipt-1' });
    expect(handle.mock.calls[1][0].intent).toMatchObject({ placementId: 'screen-1' });
  });
  it('rejects missing quest evidence before the adapter is called', async () => {
    const handle = handler();
    const app = new IVXPhygitalApp(manifest, [{ channel: 'kiosk', capabilities: ['quest'], handle }]);
    await expect(app.dispatch('kiosk', 'op-1', { kind: 'quest', action: 'submit', questId: 'q-1' } as IVXAppIntent)).rejects.toThrow('evidenceId');
    expect(handle).not.toHaveBeenCalled();
  });
  it('rejects malformed IDs, unsupported operations and oversized conversation input', async () => {
    const app = new IVXPhygitalApp(manifest, [{ channel: 'whatsapp', capabilities: ['conversation'], handle: handler() }]);
    await expect(app.dispatch('whatsapp', '', { kind: 'conversation', text: 'hello' })).rejects.toThrow('operationId');
    await expect(app.dispatch('whatsapp', 'op', { kind: 'conversation', text: ' '.repeat(3) })).rejects.toThrow('text');
    await expect(app.dispatch('whatsapp', 'op', { kind: 'conversation', text: 'a'.repeat(4097) })).rejects.toThrow('text');
    await expect(app.dispatch('whatsapp', 'op', { kind: 'grant-reward' } as unknown as IVXAppIntent)).rejects.toThrow('unsupported');
  });
  it('strips unrecognized fields rather than forwarding a client reward amount or identity', async () => {
    const handle = handler();
    const app = new IVXPhygitalApp(manifest, [{ channel: 'kiosk', capabilities: ['quest'], handle }]);
    await app.dispatch('kiosk', 'op', { kind: 'quest', action: 'start', questId: 'q', rewardAmount: 1000, userId: 'spoofed' } as IVXAppIntent);
    expect(handle.mock.calls[0][0].intent).toEqual({ kind: 'quest', action: 'start', questId: 'q' });
  });
  it('snapshots configuration so callers cannot enable a channel by mutating input', () => {
    const channels = { kiosk: ['quest' as const] };
    const adapter: IVXPhygitalAdapter = { channel: 'kiosk', capabilities: ['quest'], handle: handler() };
    const app = new IVXPhygitalApp({ appId: 'app', name: 'App', channels }, [adapter]);
    channels.kiosk.splice(0);
    expect(app.supports('kiosk', 'quest')).toBe(true);
    expect(Object.isFrozen(app.manifest.channels.kiosk)).toBe(true);
  });
  it('rejects unknown channels, duplicate adapters and invalid configuration', () => {
    const adapter: IVXPhygitalAdapter = { channel: 'kiosk', capabilities: ['quest'], handle: handler() };
    expect(() => new IVXPhygitalApp({ ...manifest, channels: { fax: ['quest'] } } as unknown as IVXPhygitalManifest)).toThrow('channel');
    expect(() => new IVXPhygitalApp(manifest, [adapter, adapter])).toThrow('duplicate');
    expect(() => new IVXPhygitalApp({ ...manifest, channels: {} })).toThrow('channel');
    expect(() => new IVXPhygitalApp({ ...manifest, appId: 'bad/id' })).toThrow('appId');
  });
  it('propagates provider failure and rejects mismatched acknowledgements', async () => {
    const app = new IVXPhygitalApp(manifest, [{ channel: 'kiosk', capabilities: ['quest'], handle: async () => { throw new Error('provider unavailable'); } }]);
    await expect(app.dispatch('kiosk', 'op', { kind: 'quest', action: 'start', questId: 'q' })).rejects.toThrow('provider unavailable');
    const bad = new IVXPhygitalApp(manifest, [{ channel: 'kiosk', capabilities: ['quest'], handle: async () => ({ operationId: 'different', status: 'accepted' }) }]);
    await expect(bad.dispatch('kiosk', 'op', { kind: 'quest', action: 'start', questId: 'q' })).rejects.toThrow('mismatched');
  });
});

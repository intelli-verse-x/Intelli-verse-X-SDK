import { describe, it, expect, vi } from 'vitest';
import { IVXKioskPublisher, validateIVXKioskExperience } from '../IVXKioskPublisher';
const manifest = {schemaVersion: 1 as const, appId:'demo', name:'Demo', kioskUrl:'https://games.example.com/display', sessionSeconds:180};
const data = {machineNo:'M1', revision:1, enabled:true, manifest, playerReady:true, delivery:'available-on-next-open', detail:'Not proof of playback'};
describe('kiosk publishing', () => {
  it('uses the existing machine API and keeps keys in headers', async () => {
    const transport = vi.fn().mockResolvedValue(new Response(JSON.stringify({code:200,data})));
    const client = new IVXKioskPublisher({apiKey:()=> 'test-key', fetch:transport});
    expect((await client.publish('M1',0,manifest)).revision).toBe(1);
    const [url, options] = transport.mock.calls[0];
    expect(url).toBe('https://api.kiosk-x.ai/api/v1/machines/M1/experience');
    expect(options.headers['X-API-Key']).toBe('test-key');
    expect(options.redirect).toBe('error');
    expect(JSON.parse(options.body)).toMatchObject({expectedRevision:0, enabled:true});
    expect(options.body).not.toContain('test-key');
  });
  it('does not retry a conflict or pretend publishing succeeded', async () => {
    const transport = vi.fn().mockResolvedValue(new Response('{}', {status:409}));
    const client = new IVXKioskPublisher({apiKey:()=> 'test-key', fetch:transport});
    await expect(client.publish('M1',0,manifest)).rejects.toThrow('409');
    expect(transport).toHaveBeenCalledTimes(1);
  });
  it('rejects acknowledgements for another machine', async () => {
    const transport = vi.fn().mockResolvedValue(new Response(JSON.stringify({code:200,data:{...data,machineNo:'M2'}})));
    await expect(new IVXKioskPublisher({apiKey:()=> 'test-key',fetch:transport}).get('M1')).rejects.toThrow('acknowledgement');
  });
  it.each(['http://games.example.com','https://127.0.0.1','https://localhost','https://device.local','https://user:pass@games.example.com','file:///tmp/app','https://games.example.com:8443'])('rejects unsafe URL %s', url => {
    expect(()=>validateIVXKioskExperience({...manifest,kioskUrl:url})).toThrow();
  });
  it('validates revisions before sending and clears mapping on disable', async () => {
    const transport = vi.fn().mockResolvedValue(new Response(JSON.stringify({code:200,data:{...data,enabled:false,manifest:null,delivery:'disabled'}})));
    const client = new IVXKioskPublisher({apiKey:()=> 'test-key',fetch:transport});
    await expect(client.disable('M1',-1)).rejects.toThrow('revision');
    expect(transport).not.toHaveBeenCalled();
    await client.disable('M1',1);
    expect(JSON.parse(transport.mock.calls[0][1].body)).toEqual({expectedRevision:1,enabled:false,manifest:null});
  });
});

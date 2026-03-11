import { describe, it, expect, beforeEach, vi } from 'vitest';
import { IVXManager } from '../IVXManager';
import { SDK_VERSION } from '../types';
import { validateConfig } from '../IVXConfig';

describe('IVXManager', () => {
  beforeEach(() => {
    IVXManager.resetInstance();
  });

  it('should return a singleton instance', () => {
    const a = IVXManager.getInstance();
    const b = IVXManager.getInstance();
    expect(a).toBe(b);
  });

  it('should reset the singleton', () => {
    const a = IVXManager.getInstance();
    IVXManager.resetInstance();
    const b = IVXManager.getInstance();
    expect(a).not.toBe(b);
  });

  it('should not be initialized before calling initialize()', () => {
    const mgr = IVXManager.getInstance();
    expect(mgr.isInitialized).toBe(false);
    expect(mgr.userId).toBe('');
    expect(mgr.username).toBe('');
    expect(mgr.hasValidSession).toBe(false);
  });

  it('should initialize with valid config', () => {
    const mgr = IVXManager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1', nakamaPort: 7350, nakamaServerKey: 'testkey' });
    expect(mgr.isInitialized).toBe(true);
    expect(mgr.client).not.toBeNull();
  });

  it('should emit initialized event', () => {
    const mgr = IVXManager.getInstance();
    const handler = vi.fn();
    mgr.on('initialized', handler);
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    expect(handler).toHaveBeenCalledOnce();
  });

  it('should throw on authenticateDevice before init', async () => {
    const mgr = IVXManager.getInstance();
    await expect(mgr.authenticateDevice()).rejects.toMatchObject({
      code: -1,
      message: expect.stringContaining('not initialized'),
    });
  });

  it('should throw on fetchProfile without session', async () => {
    const mgr = IVXManager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    await expect(mgr.fetchProfile()).rejects.toMatchObject({
      code: -1,
      message: expect.stringContaining('No valid session'),
    });
  });

  it('should throw on callRpc without session', async () => {
    const mgr = IVXManager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    await expect(mgr.callRpc('test_rpc')).rejects.toMatchObject({
      code: -1,
      message: expect.stringContaining('No valid session'),
    });
  });

  it('should clear session and socket on clearSession', () => {
    const mgr = IVXManager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    mgr.clearSession();
    expect(mgr.hasValidSession).toBe(false);
    expect(mgr.userId).toBe('');
  });

  it('should handle event on/off', () => {
    const mgr = IVXManager.getInstance();
    const handler = vi.fn();
    mgr.on('error', handler);
    mgr.off('error', handler);
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    expect(handler).not.toHaveBeenCalled();
  });

  it('should return false from restoreSession with no stored tokens', () => {
    const mgr = IVXManager.getInstance();
    expect(mgr.restoreSession()).toBe(false);
  });
});

describe('validateConfig', () => {
  it('should accept valid config', () => {
    expect(() => validateConfig({ nakamaHost: 'localhost', nakamaPort: 7350 })).not.toThrow();
  });

  it('should reject invalid port', () => {
    expect(() => validateConfig({ nakamaPort: 0 })).toThrow('Invalid port');
    expect(() => validateConfig({ nakamaPort: 99999 })).toThrow('Invalid port');
  });

  it('should reject empty host', () => {
    expect(() => validateConfig({ nakamaHost: '  ' })).toThrow('cannot be empty');
  });

  it('should reject empty server key', () => {
    expect(() => validateConfig({ nakamaServerKey: '' })).toThrow('cannot be empty');
  });
});

describe('SDK_VERSION', () => {
  it('should be a valid semver string', () => {
    expect(SDK_VERSION).toMatch(/^\d+\.\d+\.\d+$/);
  });
});

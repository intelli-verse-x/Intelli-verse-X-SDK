import { describe, it, expect, beforeEach, vi } from 'vitest';
import { IVXWeb3Manager } from '../IVXWeb3Manager';
import { SDK_VERSION, validateWeb3Config } from '../types';

describe('IVXWeb3Manager', () => {
  beforeEach(() => {
    IVXWeb3Manager.resetInstance();
  });

  it('should return a singleton instance', () => {
    const a = IVXWeb3Manager.getInstance();
    const b = IVXWeb3Manager.getInstance();
    expect(a).toBe(b);
  });

  it('should reset the singleton', () => {
    const a = IVXWeb3Manager.getInstance();
    IVXWeb3Manager.resetInstance();
    const b = IVXWeb3Manager.getInstance();
    expect(a).not.toBe(b);
  });

  it('should not be initialized before calling initialize()', () => {
    const mgr = IVXWeb3Manager.getInstance();
    expect(mgr.isInitialized).toBe(false);
    expect(mgr.userId).toBe('');
    expect(mgr.walletAddress).toBe('');
    expect(mgr.isWalletConnected).toBe(false);
  });

  it('should initialize with valid config', () => {
    const mgr = IVXWeb3Manager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1', nakamaPort: 7350, chainId: 137 });
    expect(mgr.isInitialized).toBe(true);
    expect(mgr.client).not.toBeNull();
  });

  it('should emit initialized event', () => {
    const mgr = IVXWeb3Manager.getInstance();
    const handler = vi.fn();
    mgr.on('initialized', handler);
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    expect(handler).toHaveBeenCalledOnce();
  });

  it('should throw on connectWallet before init', async () => {
    const mgr = IVXWeb3Manager.getInstance();
    await expect(mgr.connectWallet()).rejects.toMatchObject({
      code: -1,
      message: expect.stringContaining('not initialized'),
    });
  });

  it('should throw on authenticateDevice before init', async () => {
    const mgr = IVXWeb3Manager.getInstance();
    await expect(mgr.authenticateDevice()).rejects.toMatchObject({
      code: -1,
      message: expect.stringContaining('not initialized'),
    });
  });

  it('should throw on fetchProfile without session', async () => {
    const mgr = IVXWeb3Manager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    await expect(mgr.fetchProfile()).rejects.toMatchObject({
      code: -1,
      message: expect.stringContaining('No valid session'),
    });
  });

  it('should throw on callRpc without session', async () => {
    const mgr = IVXWeb3Manager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    await expect(mgr.callRpc('test')).rejects.toMatchObject({
      code: -1,
      message: expect.stringContaining('No valid session'),
    });
  });

  it('should clear session', () => {
    const mgr = IVXWeb3Manager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    mgr.clearSession();
    expect(mgr.hasValidSession).toBe(false);
  });

  it('should handle event on/off', () => {
    const mgr = IVXWeb3Manager.getInstance();
    const handler = vi.fn();
    mgr.on('error', handler);
    mgr.off('error', handler);
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    expect(handler).not.toHaveBeenCalled();
  });

  it('should disconnect wallet', () => {
    const mgr = IVXWeb3Manager.getInstance();
    mgr.initialize({ nakamaHost: '127.0.0.1' });
    const handler = vi.fn();
    mgr.on('walletDisconnected', handler);
    mgr.disconnectWallet();
    expect(mgr.isWalletConnected).toBe(false);
    expect(handler).toHaveBeenCalledOnce();
  });
});

describe('validateWeb3Config', () => {
  it('should accept valid config', () => {
    expect(() => validateWeb3Config({ nakamaHost: 'localhost', nakamaPort: 7350, chainId: 137 })).not.toThrow();
  });

  it('should reject invalid port', () => {
    expect(() => validateWeb3Config({ nakamaPort: 0 })).toThrow('Invalid port');
    expect(() => validateWeb3Config({ nakamaPort: 99999 })).toThrow('Invalid port');
  });

  it('should reject empty host', () => {
    expect(() => validateWeb3Config({ nakamaHost: '  ' })).toThrow('cannot be empty');
  });

  it('should reject empty server key', () => {
    expect(() => validateWeb3Config({ nakamaServerKey: '' })).toThrow('cannot be empty');
  });
});

describe('SDK_VERSION', () => {
  it('should be a valid semver string', () => {
    expect(SDK_VERSION).toMatch(/^\d+\.\d+\.\d+$/);
  });
});

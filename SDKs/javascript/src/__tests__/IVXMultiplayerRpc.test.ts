import { afterEach, expect, it, vi } from 'vitest';
import type { Client, Session } from '@heroiclabs/nakama-js';
import { IVXMultiplayer } from '../IVXMultiplayer';

afterEach(() => IVXMultiplayer.resetInstance());
it('passes an object to Nakama RPC and accepts its decoded object response', async () => {
  const lobby = { lobby_id: 'lobby-1' };
  const rpc = vi.fn(async () => ({ payload: lobby }));
  const session = {} as Session;
  const client = IVXMultiplayer.getInstance();
  client.initialize({ rpc } as unknown as Client, session);
  expect(await client.lobby.createLobby('Test', 4, true)).toEqual(lobby);
  expect(rpc).toHaveBeenCalledWith(session, 'create_lobby', { name: 'Test', max_players: 4, is_public: true });
});

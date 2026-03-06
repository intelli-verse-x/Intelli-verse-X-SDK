/**
 * IntelliVerseX SDK - Node.js Example
 *
 * Run: npx ts-node examples/node-example.ts
 */
import { IVXManager } from '../src';

async function main() {
  const ivx = IVXManager.getInstance();

  ivx.on('authSuccess', (userId) => console.log('Authenticated:', userId));
  ivx.on('error', (err) => console.error('Error:', err.message));

  ivx.initialize({
    nakamaHost: process.env.NAKAMA_HOST || '127.0.0.1',
    nakamaPort: parseInt(process.env.NAKAMA_PORT || '7350'),
    nakamaServerKey: process.env.NAKAMA_KEY || 'defaultkey',
    enableDebugLogs: true,
  });

  console.log('Authenticating with device ID...');
  await ivx.authenticateDevice('node-example-device-001');

  console.log('Fetching profile...');
  const profile = await ivx.fetchProfile();
  console.log('Profile:', JSON.stringify(profile, null, 2));

  console.log('Fetching wallet...');
  const wallet = await ivx.fetchWallet();
  console.log('Wallet:', JSON.stringify(wallet, null, 2));

  console.log('Submitting leaderboard score...');
  await ivx.submitScore('weekly_leaderboard', 1234);

  console.log('Fetching leaderboard...');
  const records = await ivx.fetchLeaderboard('weekly_leaderboard', 5);
  console.log('Leaderboard:', JSON.stringify(records, null, 2));

  console.log('Writing to storage...');
  await ivx.writeStorage('game_saves', 'slot1', { level: 5, score: 1234, lastPlayed: Date.now() });

  console.log('Reading from storage...');
  const save = await ivx.readStorage('game_saves', 'slot1');
  console.log('Save data:', JSON.stringify(save, null, 2));

  console.log('Calling RPC...');
  const rpcResult = await ivx.callRpc('hiro_achievements_list', '{}');
  console.log('RPC result:', JSON.stringify(rpcResult, null, 2));

  console.log('Done!');
}

main().catch(console.error);

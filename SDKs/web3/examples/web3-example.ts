/**
 * IntelliVerseX Web3 SDK - Browser Example
 *
 * This example demonstrates wallet connection, authentication, NFT queries,
 * token gating, and standard game features via the Web3 SDK.
 *
 * Build with: npx tsup examples/web3-example.ts --format esm
 * Include in an HTML page with MetaMask or another EIP-1193 wallet installed.
 */
import { IVXWeb3Manager } from '../src';

async function main() {
  const ivx = IVXWeb3Manager.getInstance();

  ivx.on('walletConnected', (info) => {
    console.log('Wallet connected:', info.address, 'on chain', info.chainId);
    console.log('Balance:', info.balance, 'ETH');
  });
  ivx.on('authSuccess', (userId) => console.log('Nakama authenticated:', userId));
  ivx.on('nftsFetched', (nfts) => console.log('NFTs:', nfts.length));
  ivx.on('error', (err) => console.error('Error:', err.message));

  ivx.initialize({
    nakamaHost: '127.0.0.1',
    nakamaPort: 7350,
    nakamaServerKey: 'defaultkey',
    chainId: 137,
    enableDebugLogs: true,
  });

  console.log('Connecting wallet...');
  const walletInfo = await ivx.connectWallet();
  console.log('Wallet:', walletInfo);

  console.log('Authenticating with wallet signature...');
  await ivx.authenticateWallet();

  console.log('Fetching profile...');
  const profile = await ivx.fetchProfile();
  console.log('Profile:', profile);

  console.log('Checking NFT ownership...');
  const nfts = await ivx.fetchNfts('0x1234...contract');
  console.log('Owned NFTs:', nfts);

  console.log('Checking token gate...');
  const hasAccess = await ivx.checkTokenGate('0x1234...contract', '1');
  console.log('Token gate access:', hasAccess);

  console.log('Fetching token balances...');
  const tokens = await ivx.fetchTokenBalances();
  console.log('Tokens:', tokens);

  console.log('Submitting leaderboard score...');
  await ivx.submitScore('weekly_leaderboard', 2500);

  console.log('Fetching leaderboard...');
  const records = await ivx.fetchLeaderboard('weekly_leaderboard', 5);
  console.log('Leaderboard:', records);

  console.log('Done!');
}

main().catch(console.error);

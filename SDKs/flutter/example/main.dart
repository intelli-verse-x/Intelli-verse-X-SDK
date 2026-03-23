/// IntelliVerseX SDK - Flutter/Dart Example
///
/// Run: dart run example/main.dart
import 'package:intelliversex_sdk/intelliversex_sdk.dart';

Future<void> main() async {
  final ivx = IVXManager.instance;

  ivx.on(IVXEvent.authSuccess, (userId) => print('Authenticated: $userId'));
  ivx.on(IVXEvent.error, (err) => print('Error: $err'));

  ivx.initialize(const IVXConfig(
    nakamaHost: '127.0.0.1',
    nakamaPort: 7350,
    nakamaServerKey: 'defaultkey',
    enableDebugLogs: true,
  ));

  print('Authenticating with device ID...');
  await ivx.authenticateDevice('flutter-example-device-001');

  print('Fetching profile...');
  final profile = await ivx.fetchProfile();
  print('Profile: $profile');

  print('Fetching wallet...');
  final wallet = await ivx.fetchWallet();
  print('Wallet: $wallet');

  print('Submitting leaderboard score...');
  await ivx.submitScore('weekly_leaderboard', 1500);

  print('Fetching leaderboard...');
  final records = await ivx.fetchLeaderboard('weekly_leaderboard', limit: 5);
  print('Leaderboard: $records');

  print('Writing to storage...');
  await ivx.writeStorage('game_saves', 'slot1', {
    'level': 5,
    'score': 1500,
    'lastPlayed': DateTime.now().toIso8601String(),
  });

  print('Reading from storage...');
  final save = await ivx.readStorage('game_saves', 'slot1');
  print('Save data: $save');

  print('Calling RPC...');
  final rpcResult = await ivx.callRpc('hiro_achievements_list');
  print('RPC result: $rpcResult');

  print('Done!');
}

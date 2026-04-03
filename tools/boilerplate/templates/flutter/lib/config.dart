/// Template-expanded runtime configuration for IntelliVerseX + Nakama.
class IvxConfig {
  IvxConfig._();

  static const String gameId = '{{game_id}}';
  static const String serverHost = '{{server_host}}';
  static final int serverPort = int.parse('{{server_port}}');
  static const String serverKey = '{{server_key}}';
  static const String bundleId = '{{bundle_id}}';
  static const String packageName = '{{package_name}}';
  static const String companyName = '{{company_name}}';
  static const String tagline = '{{tagline}}';
  static const int maxEnergy = {{max_energy}};
  static const int energyRefillMinutes = {{energy_refill_minutes}};
  static const int initialCoins = {{initial_coins}};
  static const int initialGems = {{initial_gems}};
}

class IVXConfig {
  final String nakamaHost;
  final int nakamaPort;
  final String nakamaServerKey;
  final bool useSSL;
  final bool enableAnalytics;
  final bool enableDebugLogs;
  final bool verboseLogging;

  const IVXConfig({
    this.nakamaHost = '127.0.0.1',
    this.nakamaPort = 7350,
    this.nakamaServerKey = 'defaultkey',
    this.useSSL = false,
    this.enableAnalytics = true,
    this.enableDebugLogs = false,
    this.verboseLogging = false,
  });

  void validate() {
    if (nakamaPort < 1 || nakamaPort > 65535) {
      throw ArgumentError('Invalid port: $nakamaPort. Must be 1-65535.');
    }
    if (nakamaHost.trim().isEmpty) {
      throw ArgumentError('nakamaHost cannot be empty.');
    }
    if (nakamaServerKey.trim().isEmpty) {
      throw ArgumentError('nakamaServerKey cannot be empty.');
    }
  }
}

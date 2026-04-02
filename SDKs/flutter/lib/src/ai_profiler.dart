// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

enum IVXPlayerCohort {
  casual,
  social,
  competitive,
  explorer,
  achiever,
  whale,
  atRisk,
  newPlayer,
  veteran,
  lapsed,
}

class IVXPlayerProfile {
  String playerId = '';
  IVXPlayerCohort cohort = IVXPlayerCohort.casual;
  double engagementScore = 0;
  double churnRiskScore = 0;
  double monetizationPropensity = 0;
  int totalSessionCount = 0;
  double avgSessionDurationMinutes = 0;
  List<String> preferredGameModes = const [];
  List<String> preferredFeatures = const [];
  int lastActiveTimestamp = 0;
  Map<String, double> customMetrics = const {};
}

class IVXPersonalizationHint {
  String hintType = '';
  String targetFeature = '';
  String message = '';
  double priority = 0;
  Map<String, String>? parameters;
}

/// Player profiling — stub matching Unity [IVXAIProfiler].
class IVXAIProfiler {
  IVXAIProfiler._();
  static final IVXAIProfiler instance = IVXAIProfiler._();

  bool get isTracking => false;
  IVXPlayerProfile? get cachedProfile => null;

  void initialize(Object? config, String playerId) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void trackEvent(String eventName, [Map<String, Object?>? data]) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void flushEvents() {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<IVXPlayerProfile?> getPlayerProfile() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<List<IVXPersonalizationHint>> getPersonalizationHints() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<IVXPlayerCohort> classifyPlayer() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<({double score, List<String> factors})> predictChurn() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void startAutoTracking() {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void stopAutoTracking() {
    throw UnimplementedError('Not yet implemented — stub only');
  }
}

// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

/// Satori client configuration.
class IVXSatoriConfig {
  final String apiKey;
  final String endpoint;
  final String? namespace;

  const IVXSatoriConfig({
    required this.apiKey,
    required this.endpoint,
    this.namespace,
  });
}

/// Analytics/event payload for Satori.
class IVXSatoriEvent {
  final String name;
  final Map<String, dynamic> properties;
  final int? timestamp;

  const IVXSatoriEvent({
    required this.name,
    this.properties = const {},
    this.timestamp,
  });
}

/// Feature flag value from Satori.
class IVXSatoriFlag {
  final String name;
  final String value;
  final String? variant;

  const IVXSatoriFlag({
    required this.name,
    required this.value,
    this.variant,
  });
}

/// A/B experiment assignment.
class IVXSatoriExperiment {
  final String name;
  final String variant;
  final Map<String, dynamic>? metadata;

  const IVXSatoriExperiment({
    required this.name,
    required this.variant,
    this.metadata,
  });
}

/// Scheduled or live ops event from Satori.
class IVXSatoriLiveEvent {
  final String id;
  final String name;
  final DateTime? startTime;
  final DateTime? endTime;
  final Map<String, dynamic>? metadata;

  const IVXSatoriLiveEvent({
    required this.id,
    required this.name,
    this.startTime,
    this.endTime,
    this.metadata,
  });
}

/// Satori analytics, flags, and live ops — stub until native/backend wiring exists.
class IVXSatori {
  IVXSatori._();
  static final IVXSatori instance = IVXSatori._();

  Future<void> initialize(IVXSatoriConfig config) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<void> authenticate({
    required String id,
    Map<String, String>? properties,
  }) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<void> updateIdentity(Map<String, String> properties) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<void> captureEvents(List<IVXSatoriEvent> events) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<Map<String, IVXSatoriFlag>> getAllFlags() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<IVXSatoriFlag?> getFlag(String name) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<String?> getExperimentVariant(String experimentName) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<List<IVXSatoriExperiment>> getAllExperiments() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<List<IVXSatoriLiveEvent>> getLiveEvents() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<void> logout() async {
    throw UnimplementedError('Not yet implemented — stub only');
  }
}

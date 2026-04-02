// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

enum IVXContentCategory {
  clean,
  toxic,
  spam,
  pii,
  harassment,
  hateSpeech,
  selfHarm,
  sexual,
  violence,
  custom,
}

enum IVXModerationSeverity { none, low, medium, high, critical }

enum IVXModerationActionType { allow, warn, replace, block, flag }

class IVXModerationResult {
  final IVXContentCategory category;
  final IVXModerationSeverity severity;
  final double confidence;
  final IVXModerationActionType suggestedAction;
  final String replacement;
  final String originalText;

  const IVXModerationResult({
    required this.category,
    required this.severity,
    required this.confidence,
    required this.suggestedAction,
    required this.replacement,
    required this.originalText,
  });
}

class IVXModerationRule {
  final String pattern;
  final IVXContentCategory category;
  final IVXModerationActionType action;
  final String? replacementText;

  const IVXModerationRule({
    required this.pattern,
    required this.category,
    required this.action,
    this.replacementText,
  });
}

/// AI text moderation — stub matching Unity [IVXAIModerator].
class IVXAIModerator {
  IVXAIModerator._();
  static final IVXAIModerator instance = IVXAIModerator._();

  bool get isEnabled => false;
  List<IVXModerationRule> get customRules => const [];

  void initialize(Object? config) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<IVXModerationResult> classifyText(String text) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<String> filterMessage(String text) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<List<IVXModerationResult>> scanBatch(List<String> messages) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void addCustomRule(IVXModerationRule rule) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void removeCustomRule(String pattern) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void setCustomRules(List<IVXModerationRule> rules) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void clearCustomRules() {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  IVXModerationResult checkLocalRules(String text) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Map<String, String> getDiscordModerationMetadata(IVXModerationResult result) {
    throw UnimplementedError('Not yet implemented — stub only');
  }
}

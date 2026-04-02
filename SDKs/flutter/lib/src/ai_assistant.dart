// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

class IVXAIGameContext {
  String? currentLevel;
  String? currentObjective;
  String? gamePhase;
  List<String>? inventory;
  Map<String, double>? playerStats;
  String? customContext;
}

class IVXAIAssistantResponse {
  final String response;
  final List<String>? sources;
  final double? confidence;
  final bool? isStreaming;

  const IVXAIAssistantResponse({
    required this.response,
    this.sources,
    this.confidence,
    this.isStreaming,
  });
}

class IVXAIHintResponse {
  final String hint;
  final String? difficultyLevel;
  final bool? nextHintAvailable;

  const IVXAIHintResponse({
    required this.hint,
    this.difficultyLevel,
    this.nextHintAvailable,
  });
}

class IVXAITutorialStep {
  final int stepNumber;
  final String title;
  final String description;
  final String? actionRequired;

  const IVXAITutorialStep({
    required this.stepNumber,
    required this.title,
    required this.description,
    this.actionRequired,
  });
}

class IVXAITutorialResponse {
  final String featureId;
  final List<IVXAITutorialStep> steps;
  final int? estimatedTimeSeconds;

  const IVXAITutorialResponse({
    required this.featureId,
    required this.steps,
    this.estimatedTimeSeconds,
  });
}

/// In-game AI assistant — stub matching Unity [IVXAIAssistant].
class IVXAIAssistant {
  IVXAIAssistant._();
  static final IVXAIAssistant instance = IVXAIAssistant._();

  String? systemPrompt;

  bool get isProcessing => false;
  bool get isInitialized => false;

  void initialize(Object? config) {
    throw UnimplementedError('IVXAIAssistant.initialize');
  }

  void setAuthToken(String? token) {
    throw UnimplementedError('IVXAIAssistant.setAuthToken');
  }

  void clearHistory() {
    throw UnimplementedError('IVXAIAssistant.clearHistory');
  }

  void setSystemPrompt(String prompt) {
    throw UnimplementedError('IVXAIAssistant.setSystemPrompt');
  }

  Future<IVXAIAssistantResponse?> ask(
    String question, [
    IVXAIGameContext? context,
  ]) async {
    throw UnimplementedError('IVXAIAssistant.ask');
  }

  Future<IVXAIHintResponse?> getHint(
    String levelId,
    String objectiveId, [
    IVXAIGameContext? context,
  ]) async {
    throw UnimplementedError('IVXAIAssistant.getHint');
  }

  Future<IVXAITutorialResponse?> getTutorial(String featureId) async {
    throw UnimplementedError('IVXAIAssistant.getTutorial');
  }

  Future<List<String>> searchKnowledgeBase(String query) async {
    throw UnimplementedError('IVXAIAssistant.searchKnowledgeBase');
  }
}

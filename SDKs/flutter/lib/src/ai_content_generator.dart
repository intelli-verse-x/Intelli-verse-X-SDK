// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

class IVXQuestTemplate {
  String? genre;
  String? difficulty;
  List<String>? requiredElements;
  int estimatedDurationMinutes = 0;
  String? customPrompt;
}

class IVXGeneratedQuest {
  String? title;
  String? description;
}

class IVXGeneratedStory {
  String? title;
  String? body;
}

class IVXGeneratedItem {
  String? name;
  String? description;
}

class IVXGeneratedDialogue {
  String? rawJson;
}

/// Procedural content — stub matching Unity [IVXAIContentGenerator].
class IVXAIContentGenerator {
  IVXAIContentGenerator._();
  static final IVXAIContentGenerator instance = IVXAIContentGenerator._();

  bool get isGenerating => false;

  void initialize(Object? config) {
    throw UnimplementedError('IVXAIContentGenerator.initialize');
  }

  Future<IVXGeneratedQuest?> generateQuest(
    IVXQuestTemplate? template, [
    String? playerContext,
  ]) async {
    throw UnimplementedError('IVXAIContentGenerator.generateQuest');
  }

  Future<IVXGeneratedStory?> generateStory(
    String prompt, [
    String genre = 'fantasy',
    int maxWords = 500,
  ]) async {
    throw UnimplementedError('IVXAIContentGenerator.generateStory');
  }

  Future<IVXGeneratedItem?> generateItemDescription(
    String itemName,
    String itemType,
    String rarity,
  ) async {
    throw UnimplementedError('IVXAIContentGenerator.generateItemDescription');
  }

  Future<IVXGeneratedDialogue?> generateDialogue(
    String scenario,
    List<String>? characters,
  ) async {
    throw UnimplementedError('IVXAIContentGenerator.generateDialogue');
  }

  Future<String?> generateFromTemplate(
    String template,
    Map<String, String>? variables,
  ) async {
    throw UnimplementedError('IVXAIContentGenerator.generateFromTemplate');
  }

  void cancelGeneration() {
    throw UnimplementedError('IVXAIContentGenerator.cancelGeneration');
  }
}

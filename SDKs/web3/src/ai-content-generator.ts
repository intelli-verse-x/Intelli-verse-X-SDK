// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/** Quest template (stub). */
export interface IVXQuestTemplate {
  genre?: string;
  difficulty?: string;
  requiredElements?: string[];
  estimatedDurationMinutes?: number;
  customPrompt?: string;
}

export interface IVXGeneratedQuest {
  title?: string;
  description?: string;
}

export interface IVXGeneratedStory {
  title?: string;
  body?: string;
}

export interface IVXGeneratedItem {
  name?: string;
  description?: string;
}

export interface IVXGeneratedDialogue {
  lines?: { speaker: string; text: string }[];
}

/**
 * Procedural content: quests, stories, items, dialogue (Unity IVXAIContentGenerator).
 */
export class IVXAIContentGenerator {
  get isGenerating(): boolean {
    return false;
  }

  initialize(_config: unknown): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async generateQuest(
    _template: IVXQuestTemplate | null,
    _playerContext?: string
  ): Promise<IVXGeneratedQuest | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async generateStory(
    _prompt: string,
    _genre?: string,
    _maxWords?: number
  ): Promise<IVXGeneratedStory | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async generateItemDescription(
    _itemName: string,
    _itemType: string,
    _rarity: string
  ): Promise<IVXGeneratedItem | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async generateDialogue(
    _scenario: string,
    _characters?: string[]
  ): Promise<IVXGeneratedDialogue | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async generateFromTemplate(
    _template: string,
    _variables?: Record<string, string>
  ): Promise<string | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  cancelGeneration(): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }
}

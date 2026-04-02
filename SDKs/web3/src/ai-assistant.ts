// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export interface IVXAIGameContext {
  currentLevel?: string;
  currentObjective?: string;
  gamePhase?: string;
  inventory?: string[];
  playerStats?: Record<string, number>;
  customContext?: string;
}

export interface IVXAIAssistantResponse {
  response: string;
  sources?: string[];
  confidence?: number;
  isStreaming?: boolean;
}

export interface IVXAIHintResponse {
  hint: string;
  difficultyLevel?: string;
  nextHintAvailable?: boolean;
}

export interface IVXAITutorialStep {
  stepNumber: number;
  title: string;
  description: string;
  actionRequired?: string;
}

export interface IVXAITutorialResponse {
  featureId: string;
  steps: IVXAITutorialStep[];
  estimatedTimeSeconds?: number;
}

/**
 * In-game AI assistant: hints, tutorials, Q&A, knowledge search (Unity IVXAIAssistant).
 */
export class IVXAIAssistant {
  systemPrompt: string | undefined;

  get isProcessing(): boolean {
    return false;
  }

  get isInitialized(): boolean {
    return false;
  }

  initialize(_config: unknown): void {
    throw new Error('Not implemented');
  }

  setAuthToken(_token: string | null): void {
    throw new Error('Not implemented');
  }

  clearHistory(): void {
    throw new Error('Not implemented');
  }

  setSystemPrompt(_prompt: string): void {
    throw new Error('Not implemented');
  }

  async ask(
    _question: string,
    _context?: IVXAIGameContext
  ): Promise<IVXAIAssistantResponse | null> {
    throw new Error('Not implemented');
  }

  async getHint(
    _levelId: string,
    _objectiveId: string,
    _context?: IVXAIGameContext
  ): Promise<IVXAIHintResponse | null> {
    throw new Error('Not implemented');
  }

  async getTutorial(
    _featureId: string
  ): Promise<IVXAITutorialResponse | null> {
    throw new Error('Not implemented');
  }

  async searchKnowledgeBase(_query: string): Promise<string[]> {
    throw new Error('Not implemented');
  }
}

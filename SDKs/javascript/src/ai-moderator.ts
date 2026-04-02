// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export enum IVXContentCategory {
  Clean = 'Clean',
  Toxic = 'Toxic',
  Spam = 'Spam',
  PII = 'PII',
  Harassment = 'Harassment',
  HateSpeech = 'HateSpeech',
  SelfHarm = 'SelfHarm',
  Sexual = 'Sexual',
  Violence = 'Violence',
  Custom = 'Custom',
}

export enum IVXModerationSeverity {
  None = 'None',
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical',
}

export enum IVXModerationActionType {
  Allow = 'Allow',
  Warn = 'Warn',
  Replace = 'Replace',
  Block = 'Block',
  Flag = 'Flag',
}

export interface IVXModerationResult {
  category: IVXContentCategory;
  severity: IVXModerationSeverity;
  confidence: number;
  suggestedAction: IVXModerationActionType;
  replacement: string;
  originalText: string;
}

export interface IVXModerationRule {
  pattern: string;
  category: IVXContentCategory;
  action: IVXModerationActionType;
  replacementText?: string;
}

/**
 * Text moderation: local rules + remote classify / filter / batch (Unity IVXAIModerator).
 */
export class IVXAIModerator {
  get isEnabled(): boolean {
    return false;
  }

  get customRules(): readonly IVXModerationRule[] {
    return [];
  }

  initialize(_config: unknown): void {
    throw new Error('Not implemented');
  }

  async classifyText(_text: string): Promise<IVXModerationResult> {
    throw new Error('Not implemented');
  }

  async filterMessage(_text: string): Promise<string> {
    throw new Error('Not implemented');
  }

  async scanBatch(_messages: string[]): Promise<IVXModerationResult[]> {
    throw new Error('Not implemented');
  }

  addCustomRule(_rule: IVXModerationRule): void {
    throw new Error('Not implemented');
  }

  removeCustomRule(_pattern: string): void {
    throw new Error('Not implemented');
  }

  setCustomRules(_rules: IVXModerationRule[]): void {
    throw new Error('Not implemented');
  }

  clearCustomRules(): void {
    throw new Error('Not implemented');
  }

  checkLocalRules(_text: string): IVXModerationResult {
    throw new Error('Not implemented');
  }

  getDiscordModerationMetadata(
    _result: IVXModerationResult
  ): Record<string, string> {
    throw new Error('Not implemented');
  }
}

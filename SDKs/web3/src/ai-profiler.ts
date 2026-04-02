// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export enum IVXPlayerCohort {
  Casual = 'Casual',
  Social = 'Social',
  Competitive = 'Competitive',
  Explorer = 'Explorer',
  Achiever = 'Achiever',
  Whale = 'Whale',
  AtRisk = 'AtRisk',
  NewPlayer = 'NewPlayer',
  Veteran = 'Veteran',
  Lapsed = 'Lapsed',
}

export interface IVXPlayerProfile {
  playerId: string;
  cohort: IVXPlayerCohort;
  engagementScore: number;
  churnRiskScore: number;
  monetizationPropensity: number;
  totalSessionCount: number;
  avgSessionDurationMinutes: number;
  preferredGameModes: string[];
  preferredFeatures: string[];
  lastActiveTimestamp: number;
  customMetrics: Record<string, number>;
}

export interface IVXPersonalizationHint {
  hintType: string;
  targetFeature: string;
  message: string;
  priority: number;
  parameters?: Record<string, string>;
}

/**
 * Player profiling events, profile fetch, personalization, churn (Unity IVXAIProfiler).
 */
export class IVXAIProfiler {
  get isTracking(): boolean {
    return false;
  }

  get cachedProfile(): IVXPlayerProfile | null {
    return null;
  }

  initialize(_config: unknown, _playerId: string): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  trackEvent(_eventName: string, _data?: Record<string, unknown>): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  flushEvents(): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async getPlayerProfile(): Promise<IVXPlayerProfile | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async getPersonalizationHints(): Promise<IVXPersonalizationHint[]> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async classifyPlayer(): Promise<IVXPlayerCohort> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async predictChurn(): Promise<{ score: number; factors: string[] }> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  startAutoTracking(): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  stopAutoTracking(): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }
}

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
    throw new Error('Not implemented');
  }

  trackEvent(_eventName: string, _data?: Record<string, unknown>): void {
    throw new Error('Not implemented');
  }

  flushEvents(): void {
    throw new Error('Not implemented');
  }

  async getPlayerProfile(): Promise<IVXPlayerProfile | null> {
    throw new Error('Not implemented');
  }

  async getPersonalizationHints(): Promise<IVXPersonalizationHint[]> {
    throw new Error('Not implemented');
  }

  async classifyPlayer(): Promise<IVXPlayerCohort> {
    throw new Error('Not implemented');
  }

  async predictChurn(): Promise<{ score: number; factors: string[] }> {
    throw new Error('Not implemented');
  }

  startAutoTracking(): void {
    throw new Error('Not implemented');
  }

  stopAutoTracking(): void {
    throw new Error('Not implemented');
  }
}

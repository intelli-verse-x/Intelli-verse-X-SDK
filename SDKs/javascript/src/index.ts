// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export { IVXManager } from './IVXManager';
export { DEFAULT_CONFIG, validateConfig } from './IVXConfig';
export { SDK_VERSION } from './types';
export type {
  IVXConfig,
} from './IVXConfig';
export type {
  IVXProfile,
  IVXLeaderboardRecord,
  IVXError,
  IVXEventMap,
} from './types';

// AI Client
export { IVXAIClient } from './IVXAIClient';
export type {
  IVXAIConfig,
  IVXAIPersona,
  IVXAIMessage,
  IVXAISessionResponse,
  IVXAIEntitlement,
  IVXAIHostProfile,
  IVXAIEventMap,
} from './IVXAIClient';

// Game Modes
export { IVXGameModes, IVXGameMode } from './IVXGameModes';
export type {
  IVXPlayerSlot,
  IVXMatchConfig,
  IVXRoomConfig,
  IVXRoomInfo,
  IVXRoomFilter,
  IVXMatchResult,
  IVXGameModesEventMap,
} from './IVXGameModes';

// Hiro Systems
export { IVXHiroSystems } from './IVXHiroSystems';
export type {
  IVXSpinWheelResult,
  IVXSpinWheelState,
  IVXStreakState,
  IVXStreakMilestone,
  IVXOffer,
  IVXOfferWallState,
  IVXRetentionState,
  IVXFriendQuest,
  IVXFriendBattle,
  IVXIapTriggerResult,
  IVXSmartAdResult,
} from './IVXHiroSystems';

// Discord Social
export { IVXDiscordSocial } from './IVXDiscordSocial';
export type {
  IVXDiscordConfig,
  IVXUnifiedFriend,
  IVXGameInvite,
  IVXDiscordLobbyInfo,
  IVXVoiceParticipant,
  IVXDiscordSocialEventMap,
} from './IVXDiscordSocial';

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

// Discord DMs & moderation
export { IVXDiscordMessages } from './discord-messages';
export type { IVXDirectMessage, IVXDMSummary } from './discord-messages';
export { IVXDiscordModeration, IVXModerationAction } from './discord-moderation';
export type { IVXModerationDecision } from './discord-moderation';
export { IVXDiscordSettings, type IVXDiscordSettingsState } from './discord-settings';

// AI LLM stack
export { IVXAINPCDialogManager } from './ai-npc';
export type { IVXAINPCProfile, IVXAINPCDialogSession } from './ai-npc';
export { IVXAIAssistant } from './ai-assistant';
export type {
  IVXAIGameContext,
  IVXAIAssistantResponse,
  IVXAIHintResponse,
  IVXAITutorialStep,
  IVXAITutorialResponse,
} from './ai-assistant';
export {
  IVXAIModerator,
  IVXContentCategory,
  IVXModerationSeverity,
  IVXModerationActionType,
} from './ai-moderator';
export type { IVXModerationResult, IVXModerationRule } from './ai-moderator';
export { IVXAIContentGenerator } from './ai-content-generator';
export type {
  IVXQuestTemplate,
  IVXGeneratedQuest,
  IVXGeneratedStory,
  IVXGeneratedItem,
  IVXGeneratedDialogue,
} from './ai-content-generator';
export { IVXAIProfiler, IVXPlayerCohort } from './ai-profiler';
export type { IVXPlayerProfile, IVXPersonalizationHint } from './ai-profiler';
export { IVXAIVoiceServices } from './ai-voice-services';
export type { IVXAIVoice, IVXTranscriptionResult } from './ai-voice-services';

// Discord Linked Channels & Debug
export { IVXDiscordLinkedChannels } from './discord-linked-channels';
export type { IVXLinkedChannel } from './discord-linked-channels';
export { IVXDiscordDebug, IVXDiscordLogLevel } from './discord-debug';
export type { IVXDiscordLogEntry, IVXDiscordLogCallback } from './discord-debug';

// Satori Analytics
export { IVXSatori } from './ivx-satori';
export type {
  IVXSatoriConfig,
  IVXSatoriEvent,
  IVXSatoriFlag,
  IVXSatoriExperiment,
  IVXSatoriLiveEvent,
} from './ivx-satori';

// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/**
 * Discord Social Settings — notification preferences, privacy, DND mode.
 *
 * Stub: requires Discord Social SDK native integration.
 * API shape matches Unity IVXDiscordSettings for zero-code-change upgrade.
 */

/** Player's Discord social settings. */
export interface IVXDiscordSettingsState {
  notificationsEnabled: boolean;
  friendRequestsEnabled: boolean;
  doNotDisturb: boolean;
  showOnlineStatus: boolean;
  allowDirectMessages: boolean;
}

/** Default settings state. */
const DEFAULT_SETTINGS: IVXDiscordSettingsState = {
  notificationsEnabled: true,
  friendRequestsEnabled: true,
  doNotDisturb: false,
  showOnlineStatus: true,
  allowDirectMessages: true,
};

export class IVXDiscordSettings {
  private _settings: IVXDiscordSettingsState = { ...DEFAULT_SETTINGS };

  /** Get the current social settings. */
  getSettings(): IVXDiscordSettingsState {
    return { ...this._settings };
  }

  /** Update one or more social settings. */
  updateSettings(partial: Partial<IVXDiscordSettingsState>): void {
    this._settings = { ...this._settings, ...partial };
  }

  /** Enable Do Not Disturb mode. */
  enableDoNotDisturb(): void {
    this._settings.doNotDisturb = true;
  }

  /** Disable Do Not Disturb mode. */
  disableDoNotDisturb(): void {
    this._settings.doNotDisturb = false;
  }

  /** Reset all settings to defaults. */
  resetToDefaults(): void {
    this._settings = { ...DEFAULT_SETTINGS };
  }
}

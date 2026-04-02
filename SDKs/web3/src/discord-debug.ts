// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/**
 * Discord Social SDK — Debug & Logging: route Discord SDK logs to custom sinks.
 * Stub surface; integrate with the native Discord Social SDK.
 */

export enum IVXDiscordLogLevel {
  NONE = 0,
  ERROR = 1,
  WARN = 2,
  INFO = 3,
  DEBUG = 4,
}

export interface IVXDiscordLogEntry {
  level: IVXDiscordLogLevel;
  message: string;
  timestamp: number;
  source: string;
}

export type IVXDiscordLogCallback = (entry: IVXDiscordLogEntry) => void;

export class IVXDiscordDebug {
  private _logLevel: IVXDiscordLogLevel = IVXDiscordLogLevel.WARN;
  private _callbacks: IVXDiscordLogCallback[] = [];
  private _logHistory: IVXDiscordLogEntry[] = [];
  private static readonly MAX_HISTORY = 500;

  /** Set the minimum log level for Discord SDK output. */
  setLogLevel(level: IVXDiscordLogLevel): void {
    this._logLevel = level;
  }

  /** Get the current log level. */
  getLogLevel(): IVXDiscordLogLevel {
    return this._logLevel;
  }

  /** Register a log callback to receive Discord SDK log messages. */
  addLogCallback(callback: IVXDiscordLogCallback): void {
    this._callbacks.push(callback);
  }

  /** Remove a previously registered log callback. */
  removeLogCallback(callback: IVXDiscordLogCallback): void {
    const idx = this._callbacks.indexOf(callback);
    if (idx >= 0) this._callbacks.splice(idx, 1);
  }

  /** Retrieve the buffered log history. */
  getLogHistory(limit = 100): IVXDiscordLogEntry[] {
    return this._logHistory.slice(-limit);
  }

  /** Clear buffered log history. */
  clearLogHistory(): void {
    this._logHistory = [];
  }

  /** Internal: emit a log entry (called by the SDK bridge layer). */
  _emitLog(level: IVXDiscordLogLevel, message: string, source = 'discord'): void {
    if (level > this._logLevel) return;
    const entry: IVXDiscordLogEntry = { level, message, timestamp: Date.now(), source };
    this._logHistory.push(entry);
    if (this._logHistory.length > IVXDiscordDebug.MAX_HISTORY) {
      this._logHistory.shift();
    }
    for (const cb of this._callbacks) {
      try { cb(entry); } catch { /* swallow callback errors */ }
    }
  }
}

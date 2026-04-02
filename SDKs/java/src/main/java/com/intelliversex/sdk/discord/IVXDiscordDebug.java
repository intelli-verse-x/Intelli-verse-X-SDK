// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.discord;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Objects;
import java.util.concurrent.CopyOnWriteArrayList;

/**
 * Discord Social SDK debug logging: levels, callbacks, and retained history.
 * <p>
 * Thread-safe for concurrent callback registration and log history updates.
 */
public final class IVXDiscordDebug {

    private static final IVXDiscordDebug INSTANCE = new IVXDiscordDebug();

    /** Maximum number of entries retained when appending to history (ring buffer cap). */
    private static final int MAX_HISTORY = 10_000;

    private volatile LogLevel logLevel = LogLevel.INFO;
    private final CopyOnWriteArrayList<LogCallback> callbacks = new CopyOnWriteArrayList<>();
    private final List<LogEntry> history = Collections.synchronizedList(new ArrayList<>());

    private IVXDiscordDebug() {}

    public static IVXDiscordDebug getInstance() {
        return INSTANCE;
    }

    /**
     * Log verbosity for Discord SDK diagnostics.
     */
    public enum LogLevel {
        /** No log output. */
        NONE,
        /** Errors only. */
        ERROR,
        /** Warnings and errors. */
        WARN,
        /** Informational messages and below. */
        INFO,
        /** Verbose debug trace. */
        DEBUG
    }

    /**
     * A single log line emitted by the Discord integration.
     */
    public static final class LogEntry {
        /** Severity. */
        public final LogLevel level;
        /** Message text. */
        public final String message;
        /** Unix epoch millis. */
        public final long timestamp;
        /** Optional subsystem or tag (e.g. class name). */
        public final String source;

        /**
         * @param level     severity
         * @param message   message text
         * @param timestamp unix epoch milliseconds
         * @param source    optional source tag; may be {@code null}
         */
        public LogEntry(LogLevel level, String message, long timestamp, String source) {
            this.level = Objects.requireNonNull(level, "level");
            this.message = message;
            this.timestamp = timestamp;
            this.source = source;
        }
    }

    /**
     * Receives {@link LogEntry} instances as they are recorded.
     */
    public interface LogCallback {
        /**
         * @param entry the log entry; never {@code null}
         */
        void onLog(LogEntry entry);
    }

    /**
     * Sets the minimum level that is recorded and dispatched to callbacks.
     *
     * @param level new threshold; must not be {@code null}
     */
    public void setLogLevel(LogLevel level) {
        this.logLevel = Objects.requireNonNull(level, "level");
    }

    /**
     * @return current minimum log level
     */
    public LogLevel getLogLevel() {
        return logLevel;
    }

    /**
     * Registers a callback invoked for each log entry that passes the current level.
     *
     * @param callback listener; must not be {@code null}
     */
    public void addLogCallback(LogCallback callback) {
        Objects.requireNonNull(callback, "callback");
        callbacks.addIfAbsent(callback);
    }

    /**
     * Unregisters a previously added callback.
     *
     * @param callback listener to remove
     */
    public void removeLogCallback(LogCallback callback) {
        if (callback != null) {
            callbacks.remove(callback);
        }
    }

    /**
     * Returns up to {@code limit} most recent log entries, oldest first.
     *
     * @param limit maximum entries (clamped to non-negative)
     * @return a defensive copy; never {@code null}
     */
    public List<LogEntry> getLogHistory(int limit) {
        int n = Math.max(0, limit);
        synchronized (history) {
            int size = history.size();
            if (size == 0 || n == 0) {
                return Collections.emptyList();
            }
            int from = Math.max(0, size - n);
            return new ArrayList<>(history.subList(from, size));
        }
    }

    /**
     * Clears retained log history. Does not affect registered callbacks or log level.
     */
    public void clearLogHistory() {
        synchronized (history) {
            history.clear();
        }
    }

    /**
     * Records a log entry if it meets {@link #getLogLevel()}, notifies callbacks, and appends to history.
     * Intended for internal / future Discord SDK use.
     *
     * @param level   severity
     * @param message message text
     * @param source  optional source tag
     */
    /**
     * Records a log entry when it passes {@link #getLogLevel()}, notifies callbacks, and retains history.
     *
     * @param level   severity (never {@link LogLevel#NONE} for normal use)
     * @param message message text
     * @param source  optional source tag
     */
    public void log(LogLevel level, String message, String source) {
        Objects.requireNonNull(level, "level");
        if (level == LogLevel.NONE || !shouldLog(level)) {
            return;
        }
        LogEntry entry = new LogEntry(level, message, System.currentTimeMillis(), source);
        appendHistory(entry);
        for (LogCallback cb : callbacks) {
            try {
                cb.onLog(entry);
            } catch (RuntimeException ignored) {
                // Callbacks must not break logging; swallow to match typical observer patterns.
            }
        }
    }

    private boolean shouldLog(LogLevel level) {
        LogLevel min = this.logLevel;
        if (min == LogLevel.NONE) {
            return false;
        }
        // Enum order: NONE &lt; ERROR &lt; WARN &lt; INFO &lt; DEBUG (verbosity increases).
        // Threshold includes this level and all more-severe (lower-ordinal) levels.
        return level.ordinal() <= min.ordinal();
    }

    private void appendHistory(LogEntry entry) {
        synchronized (history) {
            history.add(entry);
            while (history.size() > MAX_HISTORY) {
                history.remove(0);
            }
        }
    }
}

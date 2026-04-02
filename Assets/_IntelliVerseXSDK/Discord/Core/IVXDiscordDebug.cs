using System;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Log verbosity for Discord Social SDK integration diagnostics.
    /// </summary>
    public enum IVXDiscordLogLevel
    {
        /// <summary>Most verbose tracing.</summary>
        Verbose,
        /// <summary>Developer-oriented detail.</summary>
        Debug,
        /// <summary>Informational messages.</summary>
        Info,
        /// <summary>Recoverable issues.</summary>
        Warning,
        /// <summary>Failures and errors.</summary>
        Error
    }

    /// <summary>
    /// Routes Discord SDK log output through Unity and optional custom sinks.
    /// With <c>INTELLIVERSEX_HAS_DISCORD</c>, wires to native log configuration; otherwise logs via <see cref="Debug.Log"/>.
    /// </summary>
    public sealed class IVXDiscordDebug : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordDebug]";

        #endregion

        #region Private Fields

        private static IVXDiscordDebug _instance;
        private IVXDiscordLogLevel _currentLogLevel = IVXDiscordLogLevel.Info;
        private Action<IVXDiscordLogLevel, string> _customLogCallback;

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordDebug Instance => _instance;

        /// <summary>Current minimum log level for Discord integration output.</summary>
        public IVXDiscordLogLevel CurrentLogLevel
        {
            get => _currentLogLevel;
            set => SetLogLevel(value);
        }

        #endregion

        #region Events

        /// <summary>Raised for each Discord log line after level filtering.</summary>
        public event Action<IVXDiscordLogLevel, string> OnLogMessage;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _customLogCallback = null;
                _instance = null;
            }
        }

        private void Start()
        {
#if INTELLIVERSEX_HAS_DISCORD
            ApplyDiscordLogLevel();
            RegisterDiscordLogCallback();
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the minimum Discord log level and applies it to the native SDK when available.
        /// </summary>
        /// <param name="level">Minimum level to emit.</param>
        public void SetLogLevel(IVXDiscordLogLevel level)
        {
            _currentLogLevel = level;

#if INTELLIVERSEX_HAS_DISCORD
            ApplyDiscordLogLevel();
#else
            Debug.Log($"{LOG_TAG} Log level set to {level} (stub uses Unity logging).");
#endif
        }

        /// <summary>
        /// Registers an optional callback to receive every Discord log line (e.g. file or analytics sink).
        /// </summary>
        /// <param name="callback">Invoked with level and message; replaces any previous custom callback.</param>
        public void SetLogCallback(Action<IVXDiscordLogLevel, string> callback)
        {
            _customLogCallback = callback;

#if INTELLIVERSEX_HAS_DISCORD
            RegisterDiscordLogCallback();
#endif
        }

        /// <summary>
        /// Removes the custom log callback registered with <see cref="SetLogCallback"/>.
        /// </summary>
        public void ClearLogCallback()
        {
            _customLogCallback = null;

#if INTELLIVERSEX_HAS_DISCORD
            RegisterDiscordLogCallback();
#endif
        }

        #endregion

        #region Private Methods

        private bool ShouldLog(IVXDiscordLogLevel level)
        {
            return level >= _currentLogLevel;
        }

        private void RouteLog(IVXDiscordLogLevel level, string message)
        {
            if (!ShouldLog(level))
            {
                return;
            }

            OnLogMessage?.Invoke(level, message);
            _customLogCallback?.Invoke(level, message);

#if INTELLIVERSEX_HAS_DISCORD
            // Native SDK forwards here from RegisterDiscordLogCallback; optional Unity mirror below if needed.
#else
            var text = $"{LOG_TAG} [{level}] {message}";
            switch (level)
            {
                case IVXDiscordLogLevel.Warning:
                    Debug.LogWarning(text);
                    break;
                case IVXDiscordLogLevel.Error:
                    Debug.LogError(text);
                    break;
                default:
                    Debug.Log(text);
                    break;
            }
#endif
        }

#if INTELLIVERSEX_HAS_DISCORD
        private discordpp.Client Client => IVXDiscordManager.Instance?.DiscordClient;

        private void ApplyDiscordLogLevel()
        {
            var client = Client;
            if (client == null) return;
            try
            {
                var nativeLevel = _currentLogLevel switch
                {
                    IVXDiscordLogLevel.Verbose => discordpp.LoggingSeverity.Verbose,
                    IVXDiscordLogLevel.Debug => discordpp.LoggingSeverity.Debug,
                    IVXDiscordLogLevel.Info => discordpp.LoggingSeverity.Info,
                    IVXDiscordLogLevel.Warning => discordpp.LoggingSeverity.Warning,
                    IVXDiscordLogLevel.Error => discordpp.LoggingSeverity.Error,
                    _ => discordpp.LoggingSeverity.Info
                };
                client.SetLogLevel(nativeLevel);
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} ApplyDiscordLogLevel error: {e.Message}"); }
        }

        private void RegisterDiscordLogCallback()
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.SetLogCallback((severity, message) =>
                {
                    var level = severity switch
                    {
                        discordpp.LoggingSeverity.Verbose => IVXDiscordLogLevel.Verbose,
                        discordpp.LoggingSeverity.Debug => IVXDiscordLogLevel.Debug,
                        discordpp.LoggingSeverity.Info => IVXDiscordLogLevel.Info,
                        discordpp.LoggingSeverity.Warning => IVXDiscordLogLevel.Warning,
                        discordpp.LoggingSeverity.Error => IVXDiscordLogLevel.Error,
                        _ => IVXDiscordLogLevel.Info
                    };
                    RouteLog(level, message);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} RegisterDiscordLogCallback error: {e.Message}"); }
        }
#endif

        #endregion
    }
}

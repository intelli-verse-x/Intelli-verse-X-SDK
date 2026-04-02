using System;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Manages Discord Rich Presence for the game. Auto-updates presence
    /// from IntelliVerseX game state (game mode, match status, streaks,
    /// leaderboard rank, lobby party info).
    /// </summary>
    public sealed class IVXDiscordPresence : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordPresence]";

        #endregion

        #region Private Fields

        private static IVXDiscordPresence _instance;
        private float _nextUpdateTime;
        private string _currentDetails;
        private string _currentState;
        private string _partyId;
        private int _partySize;
        private int _partyMax;
        private string _joinSecret;
        private long _startTimestamp;

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordPresence Instance => _instance;
        /// <summary>Current Rich Presence details line.</summary>
        public string CurrentDetails => _currentDetails;
        /// <summary>Current Rich Presence state line.</summary>
        public string CurrentState => _currentState;

        #endregion

        #region Events

        /// <summary>Fired after a Rich Presence update is sent to Discord.</summary>
        public event Action OnPresenceUpdated;

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
                ClearPresence();
                _instance = null;
            }
        }

        private void Update()
        {
            var mgr = IVXDiscordManager.Instance;
            if (mgr == null || !mgr.IsInitialized || !mgr.Config.AutoPresence)
                return;

            if (Time.time >= _nextUpdateTime)
            {
                _nextUpdateTime = Time.time + mgr.Config.PresenceUpdateInterval;
                PushPresenceToDiscord();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the main activity details shown in Discord.
        /// Example: "Ranked Match on Arena" or "Exploring World 3".
        /// </summary>
        /// <param name="details">Primary activity description.</param>
        /// <param name="state">Secondary status text (e.g. "In Queue", "Score: 1500").</param>
        public void SetActivity(string details, string state = null)
        {
            _currentDetails = details;
            _currentState = state;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Set party/lobby information for the Rich Presence.
        /// Shows "In a group (2 of 4)" and enables Join functionality.
        /// </summary>
        /// <param name="partyId">Unique party/lobby identifier.</param>
        /// <param name="currentSize">Current number of party members.</param>
        /// <param name="maxSize">Maximum party capacity.</param>
        /// <param name="joinSecret">Secret token for others to join via Discord.</param>
        public void SetParty(string partyId, int currentSize, int maxSize, string joinSecret = null)
        {
            _partyId = partyId;
            _partySize = currentSize;
            _partyMax = maxSize;
            _joinSecret = joinSecret;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Clear party information from Rich Presence.
        /// </summary>
        public void ClearParty()
        {
            _partyId = null;
            _partySize = 0;
            _partyMax = 0;
            _joinSecret = null;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Start the activity timer. Shows "Playing for 0:15:32" in Discord.
        /// </summary>
        public void StartTimer()
        {
            _startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Stop and clear the activity timer.
        /// </summary>
        public void StopTimer()
        {
            _startTimestamp = 0;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Convenience: update presence from IVX game mode state.
        /// Automatically reads current game mode, match phase, and player count.
        /// </summary>
        public void SyncFromGameState()
        {
#if INTELLIVERSEX_HAS_DISCORD
            SyncFromIVXSystems();
#else
            Debug.Log($"{LOG_TAG} [Stub] SyncFromGameState — would read IVXGameModeManager state.");
#endif
        }

        /// <summary>
        /// Update presence to show a Hiro live-ops event.
        /// </summary>
        /// <param name="eventName">Name of the event (e.g. "Daily Spin", "Day 5 Streak").</param>
        /// <param name="result">Optional result text (e.g. "Won Legendary Chest!").</param>
        public void SetLiveOpsEvent(string eventName, string result = null)
        {
            _currentDetails = eventName;
            _currentState = result;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Update presence to show leaderboard rank.
        /// </summary>
        /// <param name="leaderboardName">Name of the leaderboard.</param>
        /// <param name="rank">Player's current rank.</param>
        /// <param name="score">Player's score.</param>
        public void SetLeaderboardRank(string leaderboardName, int rank, long score)
        {
            _currentDetails = $"Rank #{rank} on {leaderboardName}";
            _currentState = $"Score: {score:N0}";
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Clear all Rich Presence data from Discord.
        /// </summary>
        public void ClearPresence()
        {
            _currentDetails = null;
            _currentState = null;
            _partyId = null;
            _partySize = 0;
            _partyMax = 0;
            _joinSecret = null;
            _startTimestamp = 0;

#if INTELLIVERSEX_HAS_DISCORD
            ClearDiscordPresence();
#else
            Debug.Log($"{LOG_TAG} [Stub] Presence cleared.");
#endif
            OnPresenceUpdated?.Invoke();
        }

        #endregion

        #region Private Methods

        private void PushPresenceToDiscord()
        {
            var mgr = IVXDiscordManager.Instance;
            if (mgr == null || !mgr.IsInitialized) return;

#if INTELLIVERSEX_HAS_DISCORD
            UpdateDiscordActivity(mgr.Config);
#else
            Debug.Log($"{LOG_TAG} [Stub] Presence → details=\"{_currentDetails}\" " +
                      $"state=\"{_currentState}\" party={_partySize}/{_partyMax}");
#endif
            OnPresenceUpdated?.Invoke();
        }

#if INTELLIVERSEX_HAS_DISCORD
        private void UpdateDiscordActivity(IVXDiscordConfig config)
        {
            // Wire to: discordpp::Activity
            // activity.SetType(ActivityTypes::Playing)
            // activity.SetDetails(_currentDetails)
            // activity.SetState(_currentState)
            //
            // if (_startTimestamp > 0):
            //   timestamps.SetStart(_startTimestamp)
            //   activity.SetTimestamps(timestamps)
            //
            // if (_partyId != null):
            //   party.SetId(_partyId)
            //   party.SetCurrentSize(_partySize)
            //   party.SetMaxSize(_partyMax)
            //   activity.SetParty(party)
            //
            // if (_joinSecret != null):
            //   secrets.SetJoin(_joinSecret)
            //   activity.SetSecrets(secrets)
            //
            // assets.SetLargeImage(config.LargeImageAssetKey)
            // assets.SetLargeText(config.LargeImageText)
            // activity.SetAssets(assets)
            //
            // if (!string.IsNullOrEmpty(config.StorePageUrl)):
            //   button1.SetLabel("Play Now")
            //   button1.SetUrl(config.StorePageUrl)
            //   activity.AddButton(button1)
            //
            // if (!string.IsNullOrEmpty(config.CommunityInviteUrl)):
            //   button2.SetLabel("Join Community")
            //   button2.SetUrl(config.CommunityInviteUrl)
            //   activity.AddButton(button2)
            //
            // client->UpdateRichPresence(activity, callback)
        }

        private void ClearDiscordPresence()
        {
            // Wire to: client->ClearRichPresence()
        }

        private void SyncFromIVXSystems()
        {
            // Read IVXGameModeManager.Instance for current mode + phase
            // Read IVXLobbyManager.Instance for room info (party)
            // Read IVXHiroCoordinator.Instance for streak/spin state
            // Call SetActivity / SetParty accordingly
        }
#endif

        #endregion
    }
}

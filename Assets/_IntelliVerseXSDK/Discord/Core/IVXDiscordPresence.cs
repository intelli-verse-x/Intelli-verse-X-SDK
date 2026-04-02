using System;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Selects which activity field Discord uses as the primary status line.
    /// </summary>
    public enum IVXStatusDisplayType
    {
        /// <summary>Display the activity name.</summary>
        Name,
        /// <summary>Display the state line.</summary>
        State,
        /// <summary>Display the details line.</summary>
        Details
    }

    /// <summary>
    /// Bitmask of client surfaces where Rich Presence join and related UI may appear.
    /// </summary>
    [Flags]
    public enum IVXActivityPlatforms
    {
        /// <summary>Desktop Discord client.</summary>
        Desktop = 1,
        /// <summary>Mobile Discord clients.</summary>
        Mobile = 2,
        /// <summary>Console Discord surfaces.</summary>
        Console = 4,
        /// <summary>All supported platforms.</summary>
        All = Desktop | Mobile | Console
    }

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

        private string _stateUrl;
        private string _detailsUrl;
        private string _largeUrl;
        private string _smallUrl;
        private IVXStatusDisplayType _statusDisplayType = IVXStatusDisplayType.Name;
        private IVXActivityPlatforms _supportedPlatforms = IVXActivityPlatforms.All;
        private string _button1Label;
        private string _button1Url;
        private string _button2Label;
        private string _button2Url;
        private string _inviteCoverImage;
        private string _smallImageAssetKey;
        private string _smallImageText;
        private bool _rpcOnlyMode;

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordPresence Instance => _instance;
        /// <summary>Current Rich Presence details line.</summary>
        public string CurrentDetails => _currentDetails;
        /// <summary>Current Rich Presence state line.</summary>
        public string CurrentState => _currentState;
        /// <summary>Which field is used as the primary status text (name, state, or details).</summary>
        public IVXStatusDisplayType StatusDisplayType => _statusDisplayType;
        /// <summary>Platforms on which join-related presence UI is supported.</summary>
        public IVXActivityPlatforms SupportedPlatforms => _supportedPlatforms;
        /// <summary>Whether Rich Presence is running in RPC-only mode (no full SDK auth / connect).</summary>
        public bool IsRPCOnlyMode => _rpcOnlyMode;

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

        #region Advanced Rich Presence

        /// <summary>
        /// Sets optional clickable URLs for the state and details text in the Discord client.
        /// Pass null to clear a field.
        /// </summary>
        /// <param name="stateUrl">URL opened when the state line is activated.</param>
        /// <param name="detailsUrl">URL opened when the details line is activated.</param>
        public void SetFieldUrls(string stateUrl = null, string detailsUrl = null)
        {
            _stateUrl = stateUrl;
            _detailsUrl = detailsUrl;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Sets optional clickable URLs for the large and small Rich Presence images.
        /// Pass null to clear a field.
        /// </summary>
        /// <param name="largeUrl">URL for the large image asset.</param>
        /// <param name="smallUrl">URL for the small image asset.</param>
        public void SetAssetUrls(string largeUrl = null, string smallUrl = null)
        {
            _largeUrl = largeUrl;
            _smallUrl = smallUrl;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Controls which activity field Discord shows as the primary status text.
        /// </summary>
        /// <param name="type">Whether to emphasize name, state, or details.</param>
        public void SetStatusDisplayType(IVXStatusDisplayType type)
        {
            _statusDisplayType = type;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Controls which platforms expose join and related Rich Presence actions.
        /// </summary>
        /// <param name="platforms">Bitmask of desktop, mobile, and/or console.</param>
        public void SetSupportedPlatforms(IVXActivityPlatforms platforms)
        {
            _supportedPlatforms = platforms;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Adds a Rich Presence button (label + URL). At most two buttons are supported;
        /// if two already exist, the second slot is replaced.
        /// </summary>
        /// <param name="label">Button label shown in Discord.</param>
        /// <param name="url">HTTPS URL opened when the button is clicked.</param>
        public void AddButton(string label, string url)
        {
            var hasFirst = !string.IsNullOrEmpty(_button1Label) || !string.IsNullOrEmpty(_button1Url);
            var hasSecond = !string.IsNullOrEmpty(_button2Label) || !string.IsNullOrEmpty(_button2Url);

            if (!hasFirst)
            {
                _button1Label = label;
                _button1Url = url;
            }
            else if (!hasSecond)
            {
                _button2Label = label;
                _button2Url = url;
            }
            else
            {
                _button2Label = label;
                _button2Url = url;
            }

            PushPresenceToDiscord();
        }

        /// <summary>
        /// Removes all custom Rich Presence buttons.
        /// </summary>
        public void ClearButtons()
        {
            _button1Label = null;
            _button1Url = null;
            _button2Label = null;
            _button2Url = null;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Sets the cover image asset key used for game invites in Discord.
        /// </summary>
        /// <param name="assetKey">Registered asset key for the invite cover art.</param>
        public void SetInviteCoverImage(string assetKey)
        {
            _inviteCoverImage = assetKey;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Sets the small image overlay and optional hover text on Rich Presence.
        /// </summary>
        /// <param name="assetKey">Registered small image asset key.</param>
        /// <param name="text">Optional tooltip / small text line.</param>
        public void SetSmallImage(string assetKey, string text = null)
        {
            _smallImageAssetKey = assetKey;
            _smallImageText = text;
            PushPresenceToDiscord();
        }

        /// <summary>
        /// Initializes Rich Presence in RPC-only mode: sets the Discord application ID and
        /// enables presence updates without requiring <see cref="IVXDiscordManager"/> authentication
        /// or <c>Connect()</c>. Intended for desktop clients only.
        /// </summary>
        /// <param name="applicationId">Discord application (snowflake) id.</param>
        public void InitializeRPCOnly(long applicationId)
        {
            _rpcOnlyMode = true;
#if INTELLIVERSEX_HAS_DISCORD
            InitializeRPCOnlyMode(applicationId);
#else
            Debug.Log($"{LOG_TAG} [Stub] InitializeRPCOnly({applicationId}) — RPC-only Rich Presence.");
#endif
            OnPresenceUpdated?.Invoke();
        }

        #endregion

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

            _stateUrl = null;
            _detailsUrl = null;
            _largeUrl = null;
            _smallUrl = null;
            _statusDisplayType = IVXStatusDisplayType.Name;
            _supportedPlatforms = IVXActivityPlatforms.All;
            _button1Label = null;
            _button1Url = null;
            _button2Label = null;
            _button2Url = null;
            _inviteCoverImage = null;
            _smallImageAssetKey = null;
            _smallImageText = null;

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
            if (mgr == null)
                return;

            if (!_rpcOnlyMode && !mgr.IsInitialized)
                return;

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
            // activity.SetStateUrl(_stateUrl)
            // activity.SetDetailsUrl(_detailsUrl)
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
            // assets.SetLargeUrl(_largeUrl)
            // assets.SetSmallUrl(_smallUrl)
            // assets.SetSmallImage(_smallImageAssetKey)
            // assets.SetSmallText(_smallImageText)
            // assets.SetInviteCoverImage(_inviteCoverImage)
            // activity.SetAssets(assets)
            //
            // activity.SetStatusDisplayType(...) // map _statusDisplayType to SDK enum (Name / State / Details)
            // activity.SetSupportedPlatforms(...) // map _supportedPlatforms bitmask to SDK flags
            //
            // if (!string.IsNullOrEmpty(_button1Label) && !string.IsNullOrEmpty(_button1Url)):
            //   customButton1.SetLabel(_button1Label)
            //   customButton1.SetUrl(_button1Url)
            //   activity.AddButton(customButton1)
            // if (!string.IsNullOrEmpty(_button2Label) && !string.IsNullOrEmpty(_button2Url)):
            //   customButton2.SetLabel(_button2Label)
            //   customButton2.SetUrl(_button2Url)
            //   activity.AddButton(customButton2)
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

        private void InitializeRPCOnlyMode(long applicationId)
        {
            // Wire to: client->SetApplicationId(applicationId) without Connect(), then UpdateRichPresence
            // (same activity assembly as UpdateDiscordActivity; RPC-only / desktop path)
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

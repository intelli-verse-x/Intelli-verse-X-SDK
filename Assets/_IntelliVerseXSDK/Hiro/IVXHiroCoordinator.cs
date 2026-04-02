using System;
using Nakama;
using UnityEngine;
using IntelliVerseX.Hiro.Systems;

namespace IntelliVerseX.Hiro
{
    /// <summary>
    /// Central hub for all Hiro metagame systems.
    /// Attach to a persistent GameObject and call <see cref="InitializeSystems"/>
    /// after successful Nakama authentication.
    /// </summary>
    [HelpURL("https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/modules/hiro/")]
    public sealed class IVXHiroCoordinator : MonoBehaviour
    {
        private static IVXHiroCoordinator _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;

        #region System Properties

        /// <summary>Singleton instance of the Hiro coordinator.</summary>
        public static IVXHiroCoordinator Instance => _instance;
        /// <summary>Whether all Hiro systems have been initialized.</summary>
        public bool IsInitialized => _initialized;
        /// <summary>The shared RPC client used by all Hiro systems.</summary>
        public IVXHiroRpcClient RpcClient => _rpcClient;

        /// <summary>Virtual-currency and wallet management system.</summary>
        public IVXEconomySystem Economy { get; private set; }
        /// <summary>Player inventory management system.</summary>
        public IVXInventorySystem Inventory { get; private set; }
        /// <summary>Achievements and trophy tracking system.</summary>
        public IVXAchievementsSystem Achievements { get; private set; }
        /// <summary>Player progression and leveling system.</summary>
        public IVXProgressionSystem Progression { get; private set; }
        /// <summary>Energy / stamina gating system.</summary>
        public IVXEnergySystem Energy { get; private set; }
        /// <summary>Player statistics tracking system.</summary>
        public IVXStatsSystem Stats { get; private set; }
        /// <summary>Daily / weekly streak tracking system.</summary>
        public IVXStreaksSystem Streaks { get; private set; }
        /// <summary>Time-limited event leaderboard system.</summary>
        public IVXEventLeaderboardSystem EventLeaderboards { get; private set; }
        /// <summary>In-game store and catalog system.</summary>
        public IVXStoreSystem Store { get; private set; }
        /// <summary>Challenges and quest tracking system.</summary>
        public IVXChallengesSystem Challenges { get; private set; }
        /// <summary>Teams / guilds management system.</summary>
        public IVXTeamsSystem Teams { get; private set; }
        /// <summary>Tutorial step-tracking system.</summary>
        public IVXTutorialsSystem Tutorials { get; private set; }
        /// <summary>Unlockable content management system.</summary>
        public IVXUnlockablesSystem Unlockables { get; private set; }
        /// <summary>Auction house / marketplace system.</summary>
        public IVXAuctionsSystem Auctions { get; private set; }
        /// <summary>Player incentive and reward-trigger system.</summary>
        public IVXIncentivesSystem Incentives { get; private set; }
        /// <summary>In-game mailbox and messaging system.</summary>
        public IVXMailboxSystem Mailbox { get; private set; }
        /// <summary>Randomized reward-bucket system.</summary>
        public IVXRewardBucketSystem RewardBuckets { get; private set; }
        /// <summary>AI-driven player personalization system.</summary>
        public IVXPersonalizerSystem Personalizer { get; private set; }
        /// <summary>Base-building / homestead system.</summary>
        public IVXBaseSystem Base { get; private set; }
        /// <summary>Global and segmented leaderboard system.</summary>
        public IVXLeaderboardsSystem Leaderboards { get; private set; }

        // Retention
        /// <summary>Core retention metrics and lifecycle system.</summary>
        public IVXRetentionSystem Retention { get; private set; }
        /// <summary>Streak-shield protection system.</summary>
        public IVXStreakShieldSystem StreakShield { get; private set; }
        /// <summary>Time-limited session booster system.</summary>
        public IVXSessionBoosterSystem SessionBoosters { get; private set; }
        /// <summary>Scheduled appointment / callback system.</summary>
        public IVXAppointmentSystem Appointments { get; private set; }
        /// <summary>Daily-limited content rotation system.</summary>
        public IVXLimitedDailyContentSystem DailyContent { get; private set; }

        // Monetization Optimization
        /// <summary>Contextual IAP trigger system.</summary>
        public IVXIAPTriggerSystem IAPTriggers { get; private set; }
        /// <summary>Intelligent ad-pacing timer system.</summary>
        public IVXSmartAdTimerSystem SmartAdTimer { get; private set; }
        /// <summary>Ad-revenue optimization and waterfall system.</summary>
        public IVXAdRevenueOptimizerSystem AdRevenueOptimizer { get; private set; }
        /// <summary>Offerwall integration and tracking system.</summary>
        public IVXOfferwallSystem Offerwall { get; private set; }

        // Engagement
        /// <summary>Spin-the-wheel reward system.</summary>
        public IVXSpinWheelSystem SpinWheel { get; private set; }
        /// <summary>Social-pressure engagement system.</summary>
        public IVXSocialPressureSystem SocialPressure { get; private set; }

        // Social Extension
        /// <summary>Cooperative friend-quest system.</summary>
        public IVXFriendQuestSystem FriendQuests { get; private set; }
        /// <summary>Shared friend-streak tracking system.</summary>
        public IVXFriendStreakSystem FriendStreaks { get; private set; }
        /// <summary>Head-to-head friend-battle system.</summary>
        public IVXFriendBattleSystem FriendBattles { get; private set; }

        #endregion

        #region Events

        /// <summary>Fired after all systems are initialized. Bool indicates success.</summary>
        public event Action<bool> OnInitialized;

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
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialize all Hiro systems. Call after Nakama authentication succeeds.
        /// </summary>
        /// <param name="client">The Nakama client instance.</param>
        /// <param name="session">An authenticated Nakama session.</param>
        public void InitializeSystems(IClient client, ISession session)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (session == null) throw new ArgumentNullException(nameof(session));

            _rpcClient = new IVXHiroRpcClient(client, session);

            Economy = new IVXEconomySystem(_rpcClient);
            Inventory = new IVXInventorySystem(_rpcClient);
            Achievements = new IVXAchievementsSystem(_rpcClient);
            Progression = new IVXProgressionSystem(_rpcClient);
            Energy = new IVXEnergySystem(_rpcClient);
            Stats = new IVXStatsSystem(_rpcClient);
            Streaks = new IVXStreaksSystem(_rpcClient);
            EventLeaderboards = new IVXEventLeaderboardSystem(_rpcClient);
            Store = new IVXStoreSystem(_rpcClient);
            Challenges = new IVXChallengesSystem(_rpcClient);
            Teams = new IVXTeamsSystem(_rpcClient);
            Tutorials = new IVXTutorialsSystem(_rpcClient);
            Unlockables = new IVXUnlockablesSystem(_rpcClient);
            Auctions = new IVXAuctionsSystem(_rpcClient);
            Incentives = new IVXIncentivesSystem(_rpcClient);
            Mailbox = new IVXMailboxSystem(_rpcClient);
            RewardBuckets = new IVXRewardBucketSystem(_rpcClient);
            Personalizer = new IVXPersonalizerSystem(_rpcClient);
            Base = new IVXBaseSystem(_rpcClient);
            Leaderboards = new IVXLeaderboardsSystem(_rpcClient);

            // Retention
            Retention = new IVXRetentionSystem(_rpcClient);
            StreakShield = new IVXStreakShieldSystem(_rpcClient);
            SessionBoosters = new IVXSessionBoosterSystem(_rpcClient);
            Appointments = new IVXAppointmentSystem(_rpcClient);
            DailyContent = new IVXLimitedDailyContentSystem(_rpcClient);

            // Monetization Optimization
            IAPTriggers = new IVXIAPTriggerSystem(_rpcClient);
            SmartAdTimer = new IVXSmartAdTimerSystem(_rpcClient);
            AdRevenueOptimizer = new IVXAdRevenueOptimizerSystem(_rpcClient);
            Offerwall = new IVXOfferwallSystem(_rpcClient);

            // Engagement
            SpinWheel = new IVXSpinWheelSystem(_rpcClient);
            SocialPressure = new IVXSocialPressureSystem(_rpcClient);

            // Social Extension
            FriendQuests = new IVXFriendQuestSystem(_rpcClient);
            FriendStreaks = new IVXFriendStreakSystem(_rpcClient);
            FriendBattles = new IVXFriendBattleSystem(_rpcClient);

            _initialized = true;
            Debug.Log("[IVXHiro] All 33 systems initialized.");
            OnInitialized?.Invoke(true);
        }

        /// <summary>
        /// Update the session on all system RPC clients (e.g. after token refresh).
        /// </summary>
        /// <param name="session">The refreshed Nakama session.</param>
        public void RefreshSession(ISession session)
        {
            if (_rpcClient == null) return;
            _rpcClient.UpdateSession(session);
        }

        #endregion
    }
}

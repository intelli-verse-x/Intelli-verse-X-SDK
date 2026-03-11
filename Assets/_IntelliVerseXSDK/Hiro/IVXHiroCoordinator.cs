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
    public sealed class IVXHiroCoordinator : MonoBehaviour
    {
        private static IVXHiroCoordinator _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;

        #region System Properties

        public static IVXHiroCoordinator Instance => _instance;
        public bool IsInitialized => _initialized;
        public IVXHiroRpcClient RpcClient => _rpcClient;

        public IVXEconomySystem Economy { get; private set; }
        public IVXInventorySystem Inventory { get; private set; }
        public IVXAchievementsSystem Achievements { get; private set; }
        public IVXProgressionSystem Progression { get; private set; }
        public IVXEnergySystem Energy { get; private set; }
        public IVXStatsSystem Stats { get; private set; }
        public IVXStreaksSystem Streaks { get; private set; }
        public IVXEventLeaderboardSystem EventLeaderboards { get; private set; }
        public IVXStoreSystem Store { get; private set; }
        public IVXChallengesSystem Challenges { get; private set; }
        public IVXTeamsSystem Teams { get; private set; }
        public IVXTutorialsSystem Tutorials { get; private set; }
        public IVXUnlockablesSystem Unlockables { get; private set; }
        public IVXAuctionsSystem Auctions { get; private set; }
        public IVXIncentivesSystem Incentives { get; private set; }
        public IVXMailboxSystem Mailbox { get; private set; }
        public IVXRewardBucketSystem RewardBuckets { get; private set; }
        public IVXPersonalizerSystem Personalizer { get; private set; }
        public IVXBaseSystem Base { get; private set; }
        public IVXLeaderboardsSystem Leaderboards { get; private set; }

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

            _initialized = true;
            Debug.Log("[IVXHiro] All 20 systems initialized.");
            OnInitialized?.Invoke(true);
        }

        /// <summary>
        /// Update the session on all system RPC clients (e.g. after token refresh).
        /// </summary>
        public void RefreshSession(ISession session)
        {
            if (_rpcClient == null) return;
            _rpcClient.UpdateSession(session);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Progression
{
    /// <summary>
    /// Manages collectible badges including unlocking, equipping, and tier tracking.
    /// </summary>
    public class IVXBadgeManager : MonoBehaviour
    {
        #region Singleton

        private static IVXBadgeManager _instance;

        /// <summary>
        /// Singleton instance of the badge manager.
        /// </summary>
        public static IVXBadgeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXBadgeManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when a badge is unlocked.</summary>
        public event Action<IVXBadge> OnBadgeUnlocked;

        /// <summary>Raised when a badge is equipped.</summary>
        public event Action<IVXBadge> OnBadgeEquipped;

        #endregion

        #region Private Fields

        private IVXHiroRpcClient _rpcClient;
        private bool _isInitialized;

        #endregion

        #region Properties

        /// <summary>Whether the manager has been initialized.</summary>
        public bool IsInitialized => _isInitialized;

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
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the badge manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXBadgeManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves all badges for the current player.
        /// Prod RPC: <c>badges_get_all</c> (requires <c>game_id</c> on session bootstrap / payload).
        /// </summary>
        public async Task<List<IVXBadge>> GetAllBadgesAsync(string gameId = null)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXBadgeManager)}] Not initialized. Call Initialize() first."); return new List<IVXBadge>(); }
            object payload = string.IsNullOrEmpty(gameId)
                ? new { }
                : new { game_id = gameId, gameId };
            var rpc = await _rpcClient.CallAsync<IVXBadgeListResponse>("badges_get_all", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "badges_get_all"))
                return new List<IVXBadge>();
            return envelope?.badges ?? new List<IVXBadge>();
        }

        /// <summary>
        /// Resolves badge unlock state from the prod catalog (<c>badges_get_all</c>).
        /// Prod has no <c>badges_check_unlock</c> RPC — unlock is server-driven.
        /// </summary>
        public async Task<IVXBadge> CheckUnlockAsync(string badgeId, string gameId = null)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXBadgeManager)}] Not initialized. Call Initialize() first."); return null; }
            if (string.IsNullOrEmpty(badgeId)) return null;

            var all = await GetAllBadgesAsync(gameId);
            var badge = all?.Find(b => b != null && b.badgeId == badgeId);
            if (badge != null && badge.unlocked)
                OnBadgeUnlocked?.Invoke(badge);
            return badge;
        }

        /// <summary>
        /// Marks a badge as displayed/equipped locally after confirming it exists unlocked in prod catalog.
        /// Prod currently exposes no <c>badges_equip</c> RPC; prefer UI "displayed" from <c>badges_get_all</c>.
        /// </summary>
        public async Task<IVXBadge> EquipBadgeAsync(string badgeId, string gameId = null)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXBadgeManager)}] Not initialized. Call Initialize() first."); return null; }
            var badge = await CheckUnlockAsync(badgeId, gameId);
            if (badge == null || !badge.unlocked)
            {
                Debug.LogWarning($"[{nameof(IVXBadgeManager)}] Cannot equip '{badgeId}' — not unlocked on prod.");
                return null;
            }

            badge.displayed = true;
            OnBadgeEquipped?.Invoke(badge);
            return badge;
        }

        #endregion
    }
}

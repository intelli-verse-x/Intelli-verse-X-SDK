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
        /// </summary>
        /// <returns>A list of badges.</returns>
        public async Task<List<IVXBadge>> GetAllBadgesAsync()
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXBadgeManager)}] Not initialized. Call Initialize() first."); return new List<IVXBadge>(); }
            var rpc = await _rpcClient.CallAsync<IVXBadgeListResponse>("badges_get_all");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "badges_get_all"))
                return new List<IVXBadge>();
            return envelope?.badges ?? new List<IVXBadge>();
        }

        /// <summary>
        /// Checks and attempts to unlock a badge.
        /// </summary>
        /// <param name="badgeId">The badge identifier.</param>
        /// <returns>The badge if unlocked.</returns>
        public async Task<IVXBadge> CheckUnlockAsync(string badgeId)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXBadgeManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new IVXBadgeRequest { badgeId = badgeId };
            var rpc = await _rpcClient.CallAsync<IVXBadge>("badges_check_unlock", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var badge, "badges_check_unlock"))
                return null;
            if (badge != null && badge.unlocked)
                OnBadgeUnlocked?.Invoke(badge);
            return badge;
        }

        /// <summary>
        /// Equips a badge for the current player.
        /// </summary>
        /// <param name="badgeId">The badge identifier.</param>
        /// <returns>The equipped badge.</returns>
        public async Task<IVXBadge> EquipBadgeAsync(string badgeId)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXBadgeManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new IVXBadgeRequest { badgeId = badgeId };
            var rpc = await _rpcClient.CallAsync<IVXBadge>("badges_equip", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var badge, "badges_equip"))
                return null;
            if (badge != null)
                OnBadgeEquipped?.Invoke(badge);
            return badge;
        }

        #endregion
    }
}

using System;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Manages the season pass lifecycle including XP progression, reward claiming, and premium upgrades.
    /// </summary>
    public class IVXSeasonPassManager : MonoBehaviour
    {
        #region Singleton

        private static IVXSeasonPassManager _instance;

        /// <summary>
        /// Singleton instance of the season pass manager.
        /// </summary>
        public static IVXSeasonPassManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXSeasonPassManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when the player levels up on the season pass.</summary>
        public event Action<IVXSeasonPassState> OnLevelUp;

        /// <summary>Raised when a season pass reward is claimed.</summary>
        public event Action<IVXSeasonPassReward> OnRewardClaimed;

        /// <summary>Raised when the premium pass is purchased.</summary>
        public event Action<IVXSeasonPassState> OnPremiumPurchased;

        /// <summary>Raised when the current season ends.</summary>
        public event Action<IVXSeasonPassState> OnSeasonEnded;

        #endregion

        #region Private Fields

        private IVXHiroRpcClient _rpcClient;
        private bool _isInitialized;
        private int _previousLevel;

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
        /// Initializes the season pass manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXSeasonPassManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves the current season pass state for the player.
        /// </summary>
        /// <returns>The season pass state.</returns>
        public async Task<IVXSeasonPassState> GetStateAsync()
        {
            var response = await _rpcClient.CallAsync<IVXSeasonPassStateResponse>("season_pass_get_state");
            return response?.state;
        }

        /// <summary>
        /// Claims a reward at the specified level from either the free or premium track.
        /// </summary>
        /// <param name="level">The season pass level.</param>
        /// <param name="isPremiumTrack">Whether to claim from the premium track.</param>
        /// <returns>The claimed reward.</returns>
        public async Task<IVXSeasonPassReward> ClaimRewardAsync(int level, bool isPremiumTrack)
        {
            var payload = new IVXSeasonPassClaimRequest
            {
                level = level,
                isPremiumTrack = isPremiumTrack
            };
            var reward = await _rpcClient.CallAsync<IVXSeasonPassReward>("season_pass_claim_reward", payload);
            if (reward != null)
                OnRewardClaimed?.Invoke(reward);
            return reward;
        }

        /// <summary>
        /// Purchases the premium season pass upgrade for the current player.
        /// </summary>
        /// <returns>The updated season pass state.</returns>
        public async Task<IVXSeasonPassState> PurchasePremiumAsync()
        {
            var state = await _rpcClient.CallAsync<IVXSeasonPassState>("season_pass_purchase_premium");
            if (state != null)
                OnPremiumPurchased?.Invoke(state);
            return state;
        }

        /// <summary>
        /// Adds XP to the season pass and checks for level-ups.
        /// </summary>
        /// <param name="amount">The amount of XP to add.</param>
        /// <returns>The updated season pass state.</returns>
        public async Task<IVXSeasonPassState> AddXpAsync(int amount)
        {
            var payload = new IVXSeasonPassXpRequest { amount = amount };
            var state = await _rpcClient.CallAsync<IVXSeasonPassState>("season_pass_add_xp", payload);
            if (state != null && state.currentLevel > _previousLevel)
            {
                OnLevelUp?.Invoke(state);
                _previousLevel = state.currentLevel;
            }
            return state;
        }

        #endregion
    }
}

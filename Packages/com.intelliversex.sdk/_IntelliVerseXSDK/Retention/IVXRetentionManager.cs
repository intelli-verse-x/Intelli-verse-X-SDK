using System;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Retention
{
    /// <summary>
    /// Manages player retention tracking, daily check-ins, and winback offers for lapsed players.
    /// </summary>
    public class IVXRetentionManager : MonoBehaviour
    {
        #region Singleton

        private static IVXRetentionManager _instance;

        /// <summary>
        /// Singleton instance of the retention manager.
        /// </summary>
        public static IVXRetentionManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXRetentionManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when the retention state is updated.</summary>
        public event Action<IVXRetentionState> OnRetentionStateUpdated;

        /// <summary>Raised when a daily check-in is completed.</summary>
        public event Action<IVXRetentionState> OnCheckInCompleted;

        /// <summary>Raised when a winback offer becomes available.</summary>
        public event Action<IVXWinbackOffer> OnWinbackOfferAvailable;

        /// <summary>Raised when a winback offer is claimed.</summary>
        public event Action<IVXWinbackOffer> OnWinbackClaimed;

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
        /// Initializes the retention manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXRetentionManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves the current retention state for the player.
        /// </summary>
        /// <returns>The player's retention state.</returns>
        public async Task<IVXRetentionState> GetStateAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXRetentionStateResponse>("retention_get_state");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "retention_get_state"))
                return null;
            var state = envelope?.state;
            if (state != null)
                OnRetentionStateUpdated?.Invoke(state);
            return state;
        }

        /// <summary>
        /// Records a daily check-in for the player.
        /// </summary>
        /// <returns>The updated retention state.</returns>
        public async Task<IVXRetentionState> CheckInAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXRetentionState>("retention_check_in");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var state, "retention_check_in"))
                return null;
            if (state != null)
                OnCheckInCompleted?.Invoke(state);
            return state;
        }

        /// <summary>
        /// Retrieves an available winback offer for the player.
        /// </summary>
        /// <returns>The winback offer, or null if none available.</returns>
        public async Task<IVXWinbackOffer> GetWinbackOfferAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXWinbackOffer>("winback_get_offer");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var offer, "winback_get_offer"))
                return null;
            if (offer != null)
                OnWinbackOfferAvailable?.Invoke(offer);
            return offer;
        }

        /// <summary>
        /// Claims a winback offer by its identifier.
        /// </summary>
        /// <param name="offerId">The winback offer identifier.</param>
        /// <returns>The claimed winback offer.</returns>
        public async Task<IVXWinbackOffer> ClaimWinbackAsync(string offerId)
        {
            var payload = new IVXWinbackClaimRequest { offerId = offerId };
            var rpc = await _rpcClient.CallAsync<IVXWinbackOffer>("winback_claim_offer", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var offer, "winback_claim_offer"))
                return null;
            if (offer != null)
                OnWinbackClaimed?.Invoke(offer);
            return offer;
        }

        #endregion
    }
}

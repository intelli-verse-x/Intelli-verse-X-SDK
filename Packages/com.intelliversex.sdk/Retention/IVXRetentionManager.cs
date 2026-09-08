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
        /// Prod RPC: <c>hiro_retention_get</c>.
        /// </summary>
        public async Task<IVXRetentionState> GetStateAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXRetentionState>("hiro_retention_get");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var state, "hiro_retention_get"))
                return null;
            if (state != null)
                OnRetentionStateUpdated?.Invoke(state);
            return state;
        }

        /// <summary>
        /// Records a retention heartbeat / daily check-in.
        /// Prod RPC: <c>hiro_retention_heartbeat</c>, then refreshes state.
        /// </summary>
        public async Task<IVXRetentionState> CheckInAsync()
        {
            var rpc = await _rpcClient.CallAsync<object>("hiro_retention_heartbeat");
            if (rpc == null || !rpc.success)
            {
                if (rpc != null && !string.IsNullOrEmpty(rpc.error))
                    Debug.LogWarning($"[{nameof(IVXRetentionManager)}] hiro_retention_heartbeat: {rpc.error}");
                return null;
            }

            var state = await GetStateAsync();
            if (state != null)
                OnCheckInCompleted?.Invoke(state);
            return state;
        }

        /// <summary>
        /// Retrieves an available return / winback bonus for the player.
        /// Prod RPC: <c>hiro_incentives_return_bonus</c>.
        /// </summary>
        public async Task<IVXWinbackOffer> GetWinbackOfferAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXWinbackOffer>("hiro_incentives_return_bonus");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var offer, "hiro_incentives_return_bonus"))
                return null;
            if (offer != null)
                OnWinbackOfferAvailable?.Invoke(offer);
            return offer;
        }

        /// <summary>
        /// Claims a comeback / winback bonus.
        /// Prod RPC: <c>hiro_retention_claim_comeback</c>.
        /// </summary>
        public async Task<IVXWinbackOffer> ClaimWinbackAsync(string offerId)
        {
            var payload = new IVXWinbackClaimRequest
            {
                offerId = offerId,
                offerIdCamel = offerId
            };
            var rpc = await _rpcClient.CallAsync<IVXWinbackOffer>("hiro_retention_claim_comeback", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var offer, "hiro_retention_claim_comeback"))
                return null;
            if (offer != null)
                OnWinbackClaimed?.Invoke(offer);
            return offer;
        }

        #endregion
    }
}

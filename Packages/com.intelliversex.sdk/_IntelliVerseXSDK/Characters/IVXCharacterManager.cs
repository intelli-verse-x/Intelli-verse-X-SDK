using System;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Characters
{
    /// <summary>
    /// Manages the character roster including unlocking, selecting, and viewing character state.
    /// </summary>
    public class IVXCharacterManager : MonoBehaviour
    {
        #region Singleton

        private static IVXCharacterManager _instance;

        /// <summary>
        /// Singleton instance of the character manager.
        /// </summary>
        public static IVXCharacterManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXCharacterManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when a character is unlocked.</summary>
        public event Action<IVXCharacter> OnCharacterUnlocked;

        /// <summary>Raised when the active character is changed.</summary>
        public event Action<IVXCharacterState> OnActiveCharacterChanged;

        /// <summary>Raised when the full character state is updated.</summary>
        public event Action<IVXCharacterState> OnCharacterStateUpdated;

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
        /// Initializes the character manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXCharacterManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves the full character roster state for the player.
        /// </summary>
        /// <returns>The character state.</returns>
        public async Task<IVXCharacterState> GetStateAsync()
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXCharacterManager)}] Not initialized. Call Initialize() first."); return null; }
            var rpc = await _rpcClient.CallAsync<IVXCharacterStateResponse>("character_get_state");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "character_get_state"))
                return null;
            var state = envelope?.state;
            if (state != null)
                OnCharacterStateUpdated?.Invoke(state);
            return state;
        }

        /// <summary>
        /// Unlocks a character by its identifier.
        /// </summary>
        /// <param name="characterId">The character identifier.</param>
        /// <returns>The unlocked character.</returns>
        public async Task<IVXCharacter> UnlockCharacterAsync(string characterId)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXCharacterManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new IVXCharacterRequest { characterId = characterId };
            var rpc = await _rpcClient.CallAsync<IVXCharacter>("character_unlock", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var character, "character_unlock"))
                return null;
            if (character != null)
                OnCharacterUnlocked?.Invoke(character);
            return character;
        }

        /// <summary>
        /// Sets the active character for the player.
        /// </summary>
        /// <param name="characterId">The character identifier to activate.</param>
        /// <returns>The updated character state.</returns>
        public async Task<IVXCharacterState> SetActiveAsync(string characterId)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXCharacterManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new IVXCharacterRequest { characterId = characterId };
            var rpc = await _rpcClient.CallAsync<IVXCharacterState>("character_set_active", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var state, "character_set_active"))
                return null;
            if (state != null)
                OnActiveCharacterChanged?.Invoke(state);
            return state;
        }

        #endregion
    }
}

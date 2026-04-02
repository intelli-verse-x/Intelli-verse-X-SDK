using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Represents a participant in a Discord voice call.
    /// </summary>
    [Serializable]
    public sealed class IVXVoiceParticipant
    {
        /// <summary>Discord user ID of the participant.</summary>
        public string UserId;
        /// <summary>Display name of the participant.</summary>
        public string DisplayName;
        /// <summary>Whether the participant has self-muted.</summary>
        public bool IsMuted;
        /// <summary>Whether the participant has self-deafened.</summary>
        public bool IsDeafened;
        /// <summary>Whether the participant is currently speaking.</summary>
        public bool IsSpeaking;
        /// <summary>Volume level (0-200, default 100).</summary>
        public float Volume;
    }

    /// <summary>
    /// Manages Discord voice chat within game lobbies.
    /// Provides mute/deafen controls, per-participant volume,
    /// speaking indicators, and integration with external audio pipelines.
    /// </summary>
    public sealed class IVXDiscordVoice : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordVoice]";

        #endregion

        #region Private Fields

        private static IVXDiscordVoice _instance;
        private bool _inCall;
        private bool _selfMuted;
        private bool _selfDeafened;
        private float _inputVolume = 100f;
        private float _outputVolume = 100f;
        private readonly List<IVXVoiceParticipant> _participants = new();

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordVoice Instance => _instance;
        /// <summary>Whether the player is in an active voice call.</summary>
        public bool IsInCall => _inCall;
        /// <summary>Whether the local player is muted.</summary>
        public bool IsSelfMuted => _selfMuted;
        /// <summary>Whether the local player is deafened.</summary>
        public bool IsSelfDeafened => _selfDeafened;
        /// <summary>Current microphone volume (0-200).</summary>
        public float InputVolume => _inputVolume;
        /// <summary>Current speaker volume (0-200).</summary>
        public float OutputVolume => _outputVolume;
        /// <summary>List of voice call participants.</summary>
        public IReadOnlyList<IVXVoiceParticipant> Participants => _participants;

        #endregion

        #region Events

        /// <summary>Fired when joining a voice call.</summary>
        public event Action OnCallJoined;
        /// <summary>Fired when leaving a voice call.</summary>
        public event Action OnCallLeft;
        /// <summary>Fired when a participant starts speaking. Provides user ID.</summary>
        public event Action<string> OnParticipantSpeaking;
        /// <summary>Fired when the participant list changes.</summary>
        public event Action<IReadOnlyList<IVXVoiceParticipant>> OnParticipantsChanged;

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
                if (_inCall) LeaveCall();
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start or join a voice call in the specified Discord lobby.
        /// </summary>
        /// <param name="lobbyId">The Discord lobby ID to start voice in.</param>
        public void JoinCall(ulong lobbyId)
        {
            if (_inCall)
            {
                Debug.LogWarning($"{LOG_TAG} Already in a voice call.");
                return;
            }

            if (!(IVXDiscordManager.Instance?.Config?.EnableVoiceChat ?? false))
            {
                Debug.LogWarning($"{LOG_TAG} Voice chat is disabled in config.");
                return;
            }

            Debug.Log($"{LOG_TAG} Joining voice call in lobby {lobbyId}...");

#if INTELLIVERSEX_HAS_DISCORD
            StartDiscordCall(lobbyId);
#else
            _inCall = true;
            _participants.Clear();
            _participants.Add(new IVXVoiceParticipant
            {
                UserId = "self",
                DisplayName = "You",
                Volume = 100f
            });
            Debug.Log($"{LOG_TAG} [Stub] Voice call joined.");
            OnCallJoined?.Invoke();
            OnParticipantsChanged?.Invoke(_participants);
#endif
        }

        /// <summary>
        /// Leave the current voice call.
        /// </summary>
        public void LeaveCall()
        {
            if (!_inCall) return;

            Debug.Log($"{LOG_TAG} Leaving voice call...");

#if INTELLIVERSEX_HAS_DISCORD
            EndDiscordCall();
#endif

            _inCall = false;
            _selfMuted = false;
            _selfDeafened = false;
            _participants.Clear();
            OnCallLeft?.Invoke();
        }

        /// <summary>
        /// Toggle self-mute (microphone).
        /// </summary>
        /// <param name="muted">True to mute, false to unmute.</param>
        public void SetSelfMute(bool muted)
        {
            _selfMuted = muted;
#if INTELLIVERSEX_HAS_DISCORD
            SetDiscordSelfMute(muted);
#else
            Debug.Log($"{LOG_TAG} [Stub] Self mute: {muted}");
#endif
        }

        /// <summary>
        /// Toggle self-deafen (speaker + microphone).
        /// </summary>
        /// <param name="deafened">True to deafen, false to undeafen.</param>
        public void SetSelfDeafen(bool deafened)
        {
            _selfDeafened = deafened;
            if (deafened) _selfMuted = true;
#if INTELLIVERSEX_HAS_DISCORD
            SetDiscordSelfDeafen(deafened);
#else
            Debug.Log($"{LOG_TAG} [Stub] Self deafen: {deafened}");
#endif
        }

        /// <summary>
        /// Set the microphone input volume.
        /// </summary>
        /// <param name="volume">Volume level (0-200, 100 = normal).</param>
        public void SetInputVolume(float volume)
        {
            _inputVolume = Mathf.Clamp(volume, 0f, 200f);
#if INTELLIVERSEX_HAS_DISCORD
            SetDiscordInputVolume(_inputVolume);
#endif
        }

        /// <summary>
        /// Set the speaker output volume.
        /// </summary>
        /// <param name="volume">Volume level (0-200, 100 = normal).</param>
        public void SetOutputVolume(float volume)
        {
            _outputVolume = Mathf.Clamp(volume, 0f, 200f);
#if INTELLIVERSEX_HAS_DISCORD
            SetDiscordOutputVolume(_outputVolume);
#endif
        }

        /// <summary>
        /// Set the volume for a specific participant.
        /// </summary>
        /// <param name="userId">Discord user ID of the participant.</param>
        /// <param name="volume">Volume level (0-200).</param>
        public void SetParticipantVolume(string userId, float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 200f);
            for (int i = 0; i < _participants.Count; i++)
            {
                if (_participants[i].UserId == userId)
                {
                    _participants[i].Volume = volume;
                    break;
                }
            }
#if INTELLIVERSEX_HAS_DISCORD
            SetDiscordParticipantVolume(userId, volume);
#endif
        }

        /// <summary>
        /// Convenience: auto-join voice when entering an IVX Discord lobby.
        /// </summary>
        public void AutoJoinFromLobby()
        {
            var lobby = IVXDiscordLobby.Instance;
            if (lobby != null && lobby.IsInLobby)
            {
                JoinCall(lobby.CurrentLobbyId);
            }
        }

        #endregion

        #region Private Methods

#if INTELLIVERSEX_HAS_DISCORD
        private void StartDiscordCall(ulong lobbyId)
        {
            // Wire to: client->StartCall(lobbyId)
            // Set up participant tracking callbacks
        }

        private void EndDiscordCall()
        {
            // Wire to: client->EndCalls(callback)
        }

        private void SetDiscordSelfMute(bool muted)
        {
            // Wire to: client->SetSelfMuteAll(muted)
        }

        private void SetDiscordSelfDeafen(bool deafened)
        {
            // Wire to: client->SetSelfDeafAll(deafened)
        }

        private void SetDiscordInputVolume(float volume)
        {
            // Wire to: client->SetInputVolume(volume)
        }

        private void SetDiscordOutputVolume(float volume)
        {
            // Wire to: client->SetOutputVolume(volume)
        }

        private void SetDiscordParticipantVolume(string userId, float volume)
        {
            // Wire to: call.SetParticipantVolume(userId, volume)
        }
#endif

        #endregion
    }
}

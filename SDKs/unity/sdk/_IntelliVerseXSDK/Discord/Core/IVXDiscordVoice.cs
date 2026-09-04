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
    /// Invoked for each decoded remote voice frame. Set <paramref name="shouldMuteData"/> to true to drop this frame in your pipeline.
    /// </summary>
    public delegate void IVXAudioReceivedCallback(ulong userId, short[] data, int samplesPerChannel, int sampleRate, int channels, ref bool shouldMuteData);

    /// <summary>
    /// Invoked for each captured local microphone frame (PCM16) for custom routing (e.g. FMOD/Wwise).
    /// </summary>
    public delegate void IVXAudioCapturedCallback(short[] data, int samplesPerChannel, int sampleRate, int channels);

    /// <summary>
    /// Manages Discord voice chat within game lobbies.
    /// Provides mute/deafen controls, per-participant volume,
    /// speaking indicators, and integration with external audio pipelines.
    /// </summary>
    public sealed class IVXDiscordVoice : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordVoice]";
        private const float DEFAULT_VAD_THRESHOLD_DB = -30f;

        #endregion

        #region Private Fields

        private static IVXDiscordVoice _instance;
        private bool _inCall;
        private bool _selfMuted;
        private bool _selfDeafened;
        private float _inputVolume = 100f;
        private float _outputVolume = 100f;
        private readonly List<IVXVoiceParticipant> _participants = new();
        private bool _isVadCustom;
        private float _vadThresholdDb = DEFAULT_VAD_THRESHOLD_DB;
        private bool _globalMuteAll;
        private bool _globalDeafenAll;
        private IVXAudioReceivedCallback _audioReceivedCallback;
        private IVXAudioCapturedCallback _audioCapturedCallback;

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
        /// <summary>Whether a custom voice-activity-detection threshold is active.</summary>
        public bool IsVADCustom => _isVadCustom;
        /// <summary>Current VAD threshold in dB (meaningful when <see cref="IsVADCustom"/> is true).</summary>
        public float VADThreshold => _vadThresholdDb;

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
        /// <summary>Fired when a participant's mute state changes. Provides user ID and muted flag.</summary>
        public event Action<string, bool> OnParticipantMuteChanged;
        /// <summary>Fired when a participant's deafen state changes. Provides user ID and deafened flag.</summary>
        public event Action<string, bool> OnParticipantDeafenChanged;

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
            StartDiscordCall(lobbyId, null, null);
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
            EndDiscordCallForLeave();
#endif

            _inCall = false;
            _selfMuted = false;
            _selfDeafened = false;
            _audioReceivedCallback = null;
            _audioCapturedCallback = null;
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

        #region Advanced Voice

        /// <summary>
        /// Set voice-activity-detection threshold for capturing speech.
        /// </summary>
        /// <param name="useCustom">When true, applies <paramref name="thresholdDb"/>; when false, restores default SDK VAD.</param>
        /// <param name="thresholdDb">Sensitivity in dB (typical range roughly -60 to 0).</param>
        public void SetVADThreshold(bool useCustom, float thresholdDb = -30f)
        {
            _isVadCustom = useCustom;
            _vadThresholdDb = thresholdDb;

#if INTELLIVERSEX_HAS_DISCORD
            try { Client?.SetVoiceActivityDetection(useCustom, thresholdDb); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetVADThreshold error: {e.Message}"); }
#else
            Debug.Log($"{LOG_TAG} [Stub] VAD custom={useCustom}, threshold={thresholdDb} dB");
#endif
        }

        /// <summary>
        /// Join a voice call with raw PCM callbacks for custom audio engines (FMOD, Wwise, etc.).
        /// </summary>
        /// <param name="lobbyId">Discord lobby ID.</param>
        /// <param name="onReceived">Per-remote-user decoded frames.</param>
        /// <param name="onCaptured">Local capture frames.</param>
        public void JoinCallWithAudioCallbacks(ulong lobbyId, IVXAudioReceivedCallback onReceived, IVXAudioCapturedCallback onCaptured)
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

            _audioReceivedCallback = onReceived;
            _audioCapturedCallback = onCaptured;

#if INTELLIVERSEX_HAS_DISCORD
            StartDiscordCall(lobbyId, onReceived, onCaptured);
#else
            _inCall = true;
            _participants.Clear();
            _participants.Add(new IVXVoiceParticipant { UserId = "self", DisplayName = "You", Volume = 100f });
            Debug.Log($"{LOG_TAG} [Stub] Voice call with audio callbacks joined.");
            OnCallJoined?.Invoke();
            OnParticipantsChanged?.Invoke(_participants);
#endif
        }

        /// <summary>
        /// Apply mute across every active Discord voice session (global).
        /// </summary>
        public void SetSelfMuteAll(bool muted)
        {
            _globalMuteAll = muted;
            _selfMuted = muted;

#if INTELLIVERSEX_HAS_DISCORD
            try { Client?.SetSelfMute(muted); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetSelfMuteAll error: {e.Message}"); }
#else
            Debug.Log($"{LOG_TAG} [Stub] Global self mute all: {muted}");
#endif
        }

        /// <summary>
        /// Apply deafen across every active Discord voice session (global).
        /// </summary>
        public void SetSelfDeafenAll(bool deafened)
        {
            _globalDeafenAll = deafened;
            _selfDeafened = deafened;
            if (deafened) _selfMuted = true;

#if INTELLIVERSEX_HAS_DISCORD
            try { Client?.SetSelfDeaf(deafened); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetSelfDeafenAll error: {e.Message}"); }
#else
            Debug.Log($"{LOG_TAG} [Stub] Global self deafen all: {deafened}");
#endif
        }

        /// <summary>
        /// End every active voice call and clear local state.
        /// </summary>
        /// <param name="onComplete">Invoked after teardown (main thread).</param>
        public void EndAllCalls(Action onComplete = null)
        {
#if INTELLIVERSEX_HAS_DISCORD
            EndDiscordCall(() =>
            {
                if (_inCall)
                {
                    _inCall = false;
                    _selfMuted = false;
                    _selfDeafened = false;
                    _audioReceivedCallback = null;
                    _audioCapturedCallback = null;
                    _participants.Clear();
                    OnCallLeft?.Invoke();
                }
                onComplete?.Invoke();
            });
#else
            Debug.Log($"{LOG_TAG} [Stub] EndAllCalls");
            if (_inCall)
            {
                _inCall = false;
                _selfMuted = false;
                _selfDeafened = false;
                _audioReceivedCallback = null;
                _audioCapturedCallback = null;
                _participants.Clear();
                OnCallLeft?.Invoke();
            }
            onComplete?.Invoke();
#endif
        }

        /// <summary>
        /// Read mute/deafen flags for a participant by user ID.
        /// </summary>
        public (bool isMuted, bool isDeafened) GetParticipantVoiceState(string userId)
        {
            for (int i = 0; i < _participants.Count; i++)
            {
                if (_participants[i].UserId == userId)
                    return (_participants[i].IsMuted, _participants[i].IsDeafened);
            }

#if INTELLIVERSEX_HAS_DISCORD
#endif
            return (false, false);
        }

        #endregion

        #region Private Methods

#if INTELLIVERSEX_HAS_DISCORD
        private discordpp.Client Client => IVXDiscordManager.Instance?.DiscordClient;

        private void StartDiscordCall(ulong lobbyId, IVXAudioReceivedCallback onReceived, IVXAudioCapturedCallback onCaptured)
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.StartCall(lobbyId, (result) =>
                {
                    _inCall = true;
                    _participants.Clear();
                    _participants.Add(new IVXVoiceParticipant { UserId = "self", DisplayName = "You", Volume = 100f });
                    Debug.Log($"{LOG_TAG} Voice call started for lobby {lobbyId}");
                    OnCallJoined?.Invoke();
                    OnParticipantsChanged?.Invoke(_participants);
                });

                if (onReceived != null)
                    client.SetAudioReceivedCallback((userId, data, samplesPerChannel, sampleRate, channels) =>
                    {
                        bool mute = false;
                        onReceived(userId, data, samplesPerChannel, sampleRate, channels, ref mute);
                    });

                if (onCaptured != null)
                    client.SetAudioCapturedCallback((data, samplesPerChannel, sampleRate, channels) =>
                        onCaptured(data, samplesPerChannel, sampleRate, channels));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} StartDiscordCall error: {e.Message}"); }
        }

        private void EndDiscordCallForLeave()
        {
            try { Client?.EndCalls((r) => Debug.Log($"{LOG_TAG} Call ended (leave).")); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} EndDiscordCallForLeave error: {e.Message}"); }
        }

        private void EndDiscordCall(Action onComplete)
        {
            try { Client?.EndCalls((r) => onComplete?.Invoke()); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} EndDiscordCall error: {e.Message}"); onComplete?.Invoke(); }
        }

        private void SetDiscordSelfMute(bool muted)
        {
            try { Client?.SetSelfMute(muted); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetSelfMute error: {e.Message}"); }
        }

        private void SetDiscordSelfDeafen(bool deafened)
        {
            try { Client?.SetSelfDeaf(deafened); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetSelfDeafen error: {e.Message}"); }
        }

        private void SetDiscordInputVolume(float volume)
        {
            try { Client?.SetInputVolume(volume); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetInputVolume error: {e.Message}"); }
        }

        private void SetDiscordOutputVolume(float volume)
        {
            try { Client?.SetOutputVolume(volume); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetOutputVolume error: {e.Message}"); }
        }

        private void SetDiscordParticipantVolume(string userId, float volume)
        {
            if (ulong.TryParse(userId, out var id))
            {
                try { Client?.SetParticipantVolume(id, volume); }
                catch (Exception e) { Debug.LogError($"{LOG_TAG} SetParticipantVolume error: {e.Message}"); }
            }
        }
#endif

        #endregion
    }
}

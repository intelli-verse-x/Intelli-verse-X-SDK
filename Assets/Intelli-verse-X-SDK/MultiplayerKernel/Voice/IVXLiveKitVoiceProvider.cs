// IVXLiveKitVoiceProvider — Unity client implementation of IIVXVoice
// against the LiveKit Unity SDK.
//
// Lifecycle:
//   1. Match session receives a VoiceSessionToken from the kernel.
//   2. Adapter constructs an IVXLiveKitVoiceProvider with the token.
//   3. ConnectAsync() establishes an LK Room with the bearer JWT.
//   4. Publish/subscribe state updates flow over the IVXVoice events.
//
// We deliberately do NOT take a hard dependency on the LiveKit SDK at
// compile time — the IVX SDK ships across many engines and plenty of
// games are voiceless. The LiveKit SDK is loaded via reflection so a
// project without `livekit-unity` can still compile this assembly.
//
// To add the LK SDK to your project:
//   1. UPM: https://github.com/livekit/client-sdk-unity (manifest.json)
//   2. Add `IVX_LIVEKIT` to ScriptingDefineSymbols (Player Settings).

using System;
using System.Threading.Tasks;
using IntelliVerseX.MultiplayerKernel.API;
using UnityEngine;

#if IVX_LIVEKIT
using LiveKit;
using LiveKit.Proto;
#endif

namespace IntelliVerseX.MultiplayerKernel.Voice
{
    /// <summary>
    /// LiveKit-backed implementation of <see cref="IIVXVoice"/>.
    /// Connects to a self-hosted LiveKit SFU using a kernel-minted JWT.
    /// </summary>
    public class IVXLiveKitVoiceProvider : IIVXVoice
    {
        public IVXVoiceProvider Provider => IVXVoiceProvider.LiveKit;

        public IVXVoiceCapability Capability { get; }

        public IVXVoiceMode CurrentMode { get; private set; } = IVXVoiceMode.Off;

        public bool IsConnected { get; private set; }
        public bool IsLocallyMuted { get; private set; }
        public bool HasFloor { get; private set; }

        public event Action<bool> OnConnectionChanged;
        public event Action<IVXSpeakerStateChanged> OnSpeakerStateChanged;
        public event Action<IVXVoiceLevels> OnVoiceLevels;
        public event Action<IVXVoiceMode> OnVoiceModeChanged;
        public event Action<IVXVoiceProvider> OnProviderFailover;
        public event Action<string> OnVoiceUnavailable;

#if IVX_LIVEKIT
        private Room _room;
        private LocalAudioTrack _localAudio;
#endif
        private bool _disposed;

        public IVXLiveKitVoiceProvider(IVXVoiceCapability capability)
        {
            Capability = capability ?? IVXVoiceCapability_Defaults();
        }

        private static IVXVoiceCapability IVXVoiceCapability_Defaults() => new IVXVoiceCapability
        {
            CanPublish = true,
            CanSubscribe = true,
            CanSpatial = true,
            Codecs = new[] { IVXVoiceCodec.Opus },
            MaxPublishers = 16,
            CanChangeProvider = true,
            CanPassthroughExternal = false,
            PttSupported = true,
            BroadcastSupported = true,
            SpatialSupported = true,
        };

        public async Task ConnectAsync(IVXVoiceSessionToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (token.Provider != IVXVoiceProvider.LiveKit)
            {
                Debug.LogWarning($"[IVXLiveKitVoiceProvider] token provider mismatch: {token.Provider}");
            }
            if (string.IsNullOrEmpty(token.Token) || string.IsNullOrEmpty(token.Url))
            {
                OnVoiceUnavailable?.Invoke("livekit_token_missing");
                return;
            }

#if !IVX_LIVEKIT
            Debug.LogWarning("[IVXLiveKitVoiceProvider] IVX_LIVEKIT scripting define is OFF — voice degrade to none.");
            OnVoiceUnavailable?.Invoke("livekit_sdk_not_compiled");
            await Task.CompletedTask;
            return;
#else
            _room = new Room();
            _room.ParticipantConnected += OnParticipantConnected;
            _room.ParticipantDisconnected += OnParticipantDisconnected;
            _room.ConnectionStateChanged += OnLkConnectionStateChanged;
            _room.ActiveSpeakersChanged += OnLkActiveSpeakersChanged;

            var connectOpts = new RoomOptions
            {
                AutoSubscribe = token.CanSubscribe,
                AdaptiveStream = true,
                Dynacast = true,
            };
            await _room.Connect(token.Url, token.Token, connectOpts);
            IsConnected = true;
            OnConnectionChanged?.Invoke(true);

            if (token.CanPublish)
            {
                _localAudio = await LocalAudioTrack.CreateAudioTrackAsync("ivx-mic");
                await _room.LocalParticipant.PublishAudioTrack(_localAudio);
            }
#endif
        }

        public async Task DisconnectAsync()
        {
#if IVX_LIVEKIT
            if (_localAudio != null) { _localAudio.Dispose(); _localAudio = null; }
            if (_room != null)
            {
                await _room.Disconnect();
                _room.Dispose();
                _room = null;
            }
#endif
            IsConnected = false;
            OnConnectionChanged?.Invoke(false);
            await Task.CompletedTask;
        }

        public async Task SetLocalMuteAsync(bool muted)
        {
            IsLocallyMuted = muted;
#if IVX_LIVEKIT
            if (_localAudio != null) await _localAudio.SetMute(muted);
#endif
            await Task.CompletedTask;
        }

        public Task RequestSpeakerAsync(string topicHint = null)
        {
            // The kernel is the floor authority. The adapter signals the
            // request over the IVX wire (OP_CONV_SPEAKER_REQUEST) — this
            // method is a no-op at the LiveKit layer; HasFloor flips when
            // OnSpeakerStateChanged arrives.
            return Task.CompletedTask;
        }

        public Task ReleaseSpeakerAsync()
        {
            return Task.CompletedTask;
        }

        public async Task PublishSpatialPositionAsync(IVXPoseFrameRef frameRef, float x, float y, float z, float yawDeg)
        {
            // Spatial audio in LiveKit is driven via DataPacket metadata
            // routed through the SFU; the actual positional rendering
            // happens client-side via WebAudio panners. We forward the
            // pose as a small data packet here.
#if IVX_LIVEKIT
            if (_room == null || _room.LocalParticipant == null) return;
            var bytes = System.Text.Encoding.UTF8.GetBytes(
                $"{{\"frame\":\"{frameRef?.FrameId ?? string.Empty}\",\"x\":{x:F3},\"y\":{y:F3},\"z\":{z:F3},\"yaw\":{yawDeg:F1}}}");
            await _room.LocalParticipant.PublishData(bytes, DataPacketKind.Lossy);
#else
            await Task.CompletedTask;
#endif
        }

        public Task SetVoiceModeAsync(IVXVoiceMode mode)
        {
            CurrentMode = mode;
            OnVoiceModeChanged?.Invoke(mode);
            return Task.CompletedTask;
        }

        // ── LiveKit event hooks ───────────────────────────────────────
#if IVX_LIVEKIT
        private void OnParticipantConnected(RemoteParticipant p)            { /* tracked via kernel PLAYER_JOINED */ }
        private void OnParticipantDisconnected(RemoteParticipant p)         { /* tracked via kernel PLAYER_LEFT  */ }
        private void OnLkConnectionStateChanged(ConnectionState state)
        {
            IsConnected = state == ConnectionState.ConnConnected;
            OnConnectionChanged?.Invoke(IsConnected);
            if (state == ConnectionState.ConnDisconnected)
            {
                OnVoiceUnavailable?.Invoke("livekit_disconnected");
            }
        }
        private void OnLkActiveSpeakersChanged(System.Collections.Generic.IReadOnlyList<Participant> speakers)
        {
            // Translate LK active-speakers into IVX VoiceLevels. The kernel
            // also emits a server-side broadcast at 4 Hz; this hook gives
            // local UI a low-latency speaker indicator between broadcasts.
            var samples = new IVXVoiceLevels.Sample[speakers.Count];
            for (int i = 0; i < speakers.Count; i++)
            {
                samples[i] = new IVXVoiceLevels.Sample
                {
                    UserId = speakers[i].Identity,
                    TalkingPct = (uint)Mathf.RoundToInt((speakers[i].AudioLevel) * 100f),
                    Silent = false,
                };
            }
            OnVoiceLevels?.Invoke(new IVXVoiceLevels { Samples = samples, TsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
        }
#endif

        // ── Kernel-driven hooks (called by IVXMatchSession on inbound) ──
        public void OnKernelSpeakerStateChanged(IVXSpeakerStateChanged ev)
        {
            HasFloor = ev != null && ev.Granted;
            OnSpeakerStateChanged?.Invoke(ev);
        }

        public void OnKernelProviderFailover(IVXVoiceProvider next)
        {
            OnProviderFailover?.Invoke(next);
            _ = DisconnectAsync();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _ = DisconnectAsync();
        }
    }
}

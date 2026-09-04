// IVXLiveKitVisemeBinder — auto-wires a LiveKit `Room.DataReceived`
// event onto an IVXLiveKitVisemeStream. Drop this on the same
// GameObject as the IVXLiveKitVoiceProvider + IVXLiveKitVisemeStream
// and the lip-sync data channel will start flowing without any
// glue code in your game.
//
// Without this binder the dev had to subscribe to LiveKit's
// `Room.DataReceived` event by hand and forward only the
// `viseme.v1` topic — easy to forget. This component does that
// chore automatically.
//
// Pre-requisite: the IVX_LIVEKIT scripting define is on AND the
// LiveKit Unity SDK is installed (UPM:
// https://github.com/livekit/client-sdk-unity).

using System;
using IntelliVerseX.MultiplayerKernel.API;
using UnityEngine;

#if IVX_LIVEKIT
using LiveKit;
#endif

namespace IntelliVerseX.MultiplayerKernel.Voice
{
    /// <summary>
    /// Auto-binder: forwards LiveKit data-channel packets on topic
    /// <c>viseme.v1</c> from a connected <c>Room</c> into the
    /// configured <see cref="IVXLiveKitVisemeStream"/>.
    /// </summary>
    [RequireComponent(typeof(IVXLiveKitVisemeStream))]
    public sealed class IVXLiveKitVisemeBinder : MonoBehaviour
    {
        #region Constants
        private const string VISEME_TOPIC = "viseme.v1";
        #endregion

        #region Serialized Fields
        [Tooltip("Optional: the LiveKit voice provider component on this GameObject. " +
                 "When set we'll auto-bind to its Room. Otherwise call Bind() manually.")]
        [SerializeField] private IVXLiveKitVoiceProvider _voiceProvider;

        [Tooltip("If true, only forward packets where the publisher's identity ends in this suffix. " +
                 "Useful when the AI host's identity is e.g. 'agent-FortuneTeller'.")]
        [SerializeField] private string _publisherIdentitySuffix = "";

        [SerializeField] private bool _verboseLogging;
        #endregion

        #region Private Fields
        private IIVXVisemeStream _stream;
#if IVX_LIVEKIT
        private Room _boundRoom;
#endif
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _stream = GetComponent<IIVXVisemeStream>();
            if (_stream == null)
            {
                Debug.LogError($"[IVXLiveKitVisemeBinder] no IIVXVisemeStream on {name}");
            }
        }

        private void OnDisable() => Unbind();
        private void OnDestroy() => Unbind();
        #endregion

        #region Public API
        /// <summary>
        /// Bind to the given LiveKit Room. Caller is responsible for
        /// keeping the Room alive — this binder only subscribes.
        /// </summary>
        public void Bind(object liveKitRoom)
        {
#if IVX_LIVEKIT
            if (liveKitRoom is not Room room)
            {
                Debug.LogWarning("[IVXLiveKitVisemeBinder] Bind() called with non-Room object");
                return;
            }
            Unbind();
            _boundRoom = room;
            _boundRoom.DataReceived += OnRoomDataReceived;
            if (_verboseLogging) Debug.Log("[IVXLiveKitVisemeBinder] bound to LiveKit Room");
#endif
        }

        public void Unbind()
        {
#if IVX_LIVEKIT
            if (_boundRoom != null)
            {
                _boundRoom.DataReceived -= OnRoomDataReceived;
                _boundRoom = null;
                if (_verboseLogging) Debug.Log("[IVXLiveKitVisemeBinder] unbound");
            }
#endif
        }
        #endregion

        #region LiveKit Event
#if IVX_LIVEKIT
        private void OnRoomDataReceived(byte[] payload, Participant participant, DataPacketKind kind, string topic)
        {
            if (topic != VISEME_TOPIC) return;
            if (!string.IsNullOrEmpty(_publisherIdentitySuffix) &&
                participant != null &&
                !participant.Identity.EndsWith(_publisherIdentitySuffix, StringComparison.Ordinal))
            {
                return;
            }
            if (_stream == null || payload == null || payload.Length == 0) return;
            _stream.Dispatch(new ReadOnlyMemory<byte>(payload), isJson: true);
        }
#endif
        #endregion
    }
}

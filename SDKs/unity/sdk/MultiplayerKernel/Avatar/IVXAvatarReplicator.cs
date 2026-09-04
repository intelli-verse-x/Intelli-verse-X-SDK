// IVXAvatarReplicator — Unity MonoBehaviour that drives remote human
// avatars from the canonical avatar-replication wire (XR_POSE range
// 0xF000-0xF008) and publishes the local player's head/hand/blendshape
// poses on the same channel.
//
// Wire contract:
//   * Server template:  data/modules/avatar_replication/main.go
//   * Schema:           Intelli-verse-X-SDK/schemas/multiplayer/templates/avatar_replication.proto
//   * Reference:        SDKs/visionos/Sources/IVXMultiplayer/Avatar/IVXAvatarReplicator.swift
//   * JS sibling:       SDKs/javascript/.../webxr/adapter.ts
//
// Responsibilities:
//   1. Subscribe to HEAD_POSE / LEFT_HAND_POSE / RIGHT_HAND_POSE /
//      BLENDSHAPES / FINGER_CURLS / AVATAR_DESCRIPTOR / LOD_HINT /
//      PEER_LEFT / AVATAR_FALLBACK on a given IIVXMatchSession.
//   2. Maintain a per-userId map; either:
//        a) drive a registered IIVXAvatar adapter (humanoid skeleton +
//           blendshapes), or
//        b) emit C# events so games using their own scene-graph wiring
//           (Animation Rigging, Final IK, custom puppets) can consume
//           poses directly.
//   3. Publish the local player's head + per-controller poses at a
//      configurable Hz (default 30). Idle suppression + 1Hz heartbeat
//      keeps server-state warm without flooding the SFU.
//
// AI avatars do NOT need this component: their poses come from the
// LiveKit Agents worker, not from avatar_replication.proto. This class
// is the single piece that unlocks REMOTE HUMAN replication on Unity.
//
// What this MonoBehaviour does NOT do:
//   * Does NOT mint Nakama matches — call IIVXMultiplayer.JoinMatchAsync
//     yourself and pass the resulting IIVXMatchSession to Attach().
//   * Does NOT load avatar meshes — supply an avatar factory delegate.
//   * Does NOT handle voice / lip-sync — use IVXLiveKitVisemeStream.
//
// Drop on a GameObject with optional Transform refs for head + hands.
// Most users only need head + hands; body/finger publishing is
// game-supplied via IIVXAvatarPosePublisher. See README in this folder.

using System;
using System.Collections.Generic;
using IntelliVerseX.MultiplayerKernel.API;
using IntelliVerseX.MultiplayerKernel.Wire;
using Newtonsoft.Json;
using UnityEngine;

namespace IntelliVerseX.MultiplayerKernel.Avatar
{
    #region Wire DTOs (mirror avatar_replication.proto)

    /// <summary>
    /// Inbound shape for HEAD_POSE / LEFT_HAND_POSE / RIGHT_HAND_POSE.
    /// Server stamps <c>user_id</c>; clients publish only <c>pose</c>.
    /// </summary>
    [Serializable]
    public class IVXAvatarPoseEnvelope
    {
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty("pose")]
        public IVXPoseQuantized Pose { get; set; }

        [JsonProperty("grip_pct", NullValueHandling = NullValueHandling.Ignore)]
        public uint? GripPct { get; set; }

        [JsonProperty("trigger_pct", NullValueHandling = NullValueHandling.Ignore)]
        public uint? TriggerPct { get; set; }
    }

    [Serializable]
    public class IVXAvatarBlendshapeEnvelope
    {
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty("blendshapes")]
        public byte[] Blendshapes { get; set; }

        [JsonProperty("quant_profile", NullValueHandling = NullValueHandling.Ignore)]
        public uint QuantProfile { get; set; } = 1;
    }

    [Serializable]
    public class IVXAvatarFingerEnvelope
    {
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty("is_left")]
        public bool IsLeft { get; set; }

        [JsonProperty("finger_curls")]
        public byte[] FingerCurls { get; set; }
    }

    [Serializable]
    public class IVXAvatarLodEnvelope
    {
        [JsonProperty("user_id")] public string UserId { get; set; }
        [JsonProperty("lod")]     public uint   Lod    { get; set; }
        [JsonProperty("reason")]  public string Reason { get; set; }
    }

    [Serializable]
    public class IVXAvatarLifecycleEnvelope
    {
        [JsonProperty("user_id")] public string UserId { get; set; }
        [JsonProperty("reason")]  public string Reason { get; set; }
    }

    #endregion

    #region Peer events (for games that don't implement IIVXAvatar)

    public struct IVXPeerPoseEvent
    {
        public string UserId;
        public IVXAvatarBone Bone;
        public Vector3 Position;
        public Quaternion Rotation;
        public uint GripPct;
        public uint TriggerPct;
        public long TsMs;
    }

    public enum IVXAvatarBone
    {
        Head      = 0,
        LeftHand  = 1,
        RightHand = 2
    }

    public struct IVXPeerBlendshapeEvent
    {
        public string UserId;
        public byte[] Weights;
        public IVXBlendshapeProfile Profile;
    }

    public struct IVXPeerFingerEvent
    {
        public string UserId;
        public bool   IsLeft;
        public byte[] Curls;
    }

    public struct IVXPeerLifecycleEvent
    {
        public string UserId;
        public string Reason;
    }

    #endregion

    /// <summary>
    /// Drives remote-human avatars from a match running the
    /// <c>avatar-replication-v1</c> template, and publishes the local
    /// player's poses on the same wire.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class IVXAvatarReplicator : MonoBehaviour
    {
        #region Constants
        private const string LOG_PREFIX = "[IVXAvatarReplicator]";
        private const float  DEFAULT_POS_EPSILON_M    = 0.001f; // 1 mm
        private const float  DEFAULT_ROT_EPSILON_DEG  = 0.5f;
        private const int    DEFAULT_HEARTBEAT_MS     = 1000;
        #endregion

        #region Serialized Fields
        [Header("Local pose sources (optional)")]
        [Tooltip("Local head transform — usually the XR camera. If null, head publishing is disabled.")]
        [SerializeField] private Transform _localHead;

        [Tooltip("Local left-hand transform — usually the XR left controller. If null, left-hand publishing is disabled.")]
        [SerializeField] private Transform _localLeftHand;

        [Tooltip("Local right-hand transform — usually the XR right controller. If null, right-hand publishing is disabled.")]
        [SerializeField] private Transform _localRightHand;

        [Tooltip("Optional anchor root — poses are quantized in this transform's local space. " +
                 "Leave null to use world space (only correct when all peers share the same world origin).")]
        [SerializeField] private Transform _anchorRoot;

        [Header("Publish rates")]
        [Range(1, 90)]
        [Tooltip("Head pose publish rate. Typical: 30 mobile, 60 standalone VR, 72 Quest, 90 PCVR.")]
        [SerializeField] private int _headHz = 30;

        [Range(1, 90)]
        [Tooltip("Per-hand publish rate.")]
        [SerializeField] private int _handHz = 30;

        [Header("Idle suppression + heartbeat")]
        [Tooltip("Skip publishing when local position changes less than this many metres.")]
        [SerializeField] private float _posEpsilonM = DEFAULT_POS_EPSILON_M;

        [Tooltip("Skip publishing when local rotation changes less than this many degrees.")]
        [SerializeField] private float _rotEpsilonDeg = DEFAULT_ROT_EPSILON_DEG;

        [Tooltip("Even if the local pose is idle, publish at least once every N ms so the server keeps the peer warm.")]
        [SerializeField] private int _heartbeatMs = DEFAULT_HEARTBEAT_MS;

        [Header("Debug")]
        [SerializeField] private bool _verboseLogging;
        #endregion

        #region Private Fields
        private IIVXMatchSession _session;
        private readonly List<IDisposable> _subs = new List<IDisposable>();

        // Per-peer state: either an IIVXAvatar adapter (if a factory is set)
        // or null (events-only mode).
        private readonly Dictionary<string, IIVXAvatar> _peerAvatars =
            new Dictionary<string, IIVXAvatar>(StringComparer.Ordinal);

        // Optional avatar factory — set via SetAvatarFactory().
        private Func<IVXAvatarDescriptor, IIVXAvatar> _avatarFactory;

        // Last-published pose state for idle suppression.
        private Vector3    _lastHeadPos,    _lastLeftPos,  _lastRightPos;
        private Quaternion _lastHeadRot,    _lastLeftRot,  _lastRightRot;
        private float      _lastHeadPubMs,  _lastLeftPubMs, _lastRightPubMs;
        private bool       _hasHeadBaseline, _hasLeftBaseline, _hasRightBaseline;

        private bool _attached;
        #endregion

        #region Public events (events-only mode)

        /// <summary>
        /// Fires when a remote peer's head/hand pose update arrives. Use
        /// when your game does NOT implement IIVXAvatar and wants to drive
        /// its own scene graph directly.
        /// </summary>
        public event Action<IVXPeerPoseEvent> OnPeerPose;

        public event Action<IVXPeerBlendshapeEvent> OnPeerBlendshapes;
        public event Action<IVXPeerFingerEvent>     OnPeerFinger;
        public event Action<IVXAvatarDescriptor>    OnPeerAvatarDescriptor;
        public event Action<IVXPeerLifecycleEvent>  OnPeerLeft;
        public event Action<IVXPeerLifecycleEvent>  OnPeerFallback;

        #endregion

        #region Public properties

        /// <summary>True between Attach() and Detach().</summary>
        public bool IsAttached => _attached;

        /// <summary>Snapshot of currently tracked remote user ids.</summary>
        public IReadOnlyCollection<string> RemoteUserIds => _peerAvatars.Keys;

        /// <summary>
        /// Currently-bound match session. Null until Attach() succeeds.
        /// </summary>
        public IIVXMatchSession Session => _session;

        #endregion

        #region Public API

        /// <summary>
        /// Bind to a match session that runs the <c>avatar-replication-v1</c>
        /// template. Subscribes to all 9 avatar opcodes and starts the
        /// publish loop on the next Update().
        /// </summary>
        public void Attach(IIVXMatchSession session)
        {
            if (session == null)
            {
                Debug.LogError($"{LOG_PREFIX} Attach called with null session");
                return;
            }
            if (_attached)
            {
                Debug.LogWarning($"{LOG_PREFIX} already attached; detaching previous session first");
                Detach();
            }

            _session = session;

            _subs.Add(session.Subscribe<IVXAvatarPoseEnvelope>(IVXAvatarOp.HEAD_POSE,
                ev => HandlePose(ev, IVXAvatarBone.Head)));
            _subs.Add(session.Subscribe<IVXAvatarPoseEnvelope>(IVXAvatarOp.LEFT_HAND_POSE,
                ev => HandlePose(ev, IVXAvatarBone.LeftHand)));
            _subs.Add(session.Subscribe<IVXAvatarPoseEnvelope>(IVXAvatarOp.RIGHT_HAND_POSE,
                ev => HandlePose(ev, IVXAvatarBone.RightHand)));
            _subs.Add(session.Subscribe<IVXAvatarBlendshapeEnvelope>(IVXAvatarOp.BLENDSHAPES,
                HandleBlendshapes));
            _subs.Add(session.Subscribe<IVXAvatarFingerEnvelope>(IVXAvatarOp.FINGER_CURLS,
                HandleFingers));
            _subs.Add(session.Subscribe<IVXAvatarDescriptor>(IVXAvatarOp.AVATAR_DESCRIPTOR,
                HandleDescriptor));
            _subs.Add(session.Subscribe<IVXAvatarLodEnvelope>(IVXAvatarOp.LOD_HINT,
                HandleLodHint));
            _subs.Add(session.Subscribe<IVXAvatarLifecycleEnvelope>(IVXAvatarOp.PEER_LEFT,
                HandlePeerLeft));
            _subs.Add(session.Subscribe<IVXAvatarLifecycleEnvelope>(IVXAvatarOp.AVATAR_FALLBACK,
                HandleFallback));

            _attached = true;
            if (_verboseLogging) Debug.Log($"{LOG_PREFIX} attached match={session.MatchId} local={session.LocalUserId}");
        }

        /// <summary>
        /// Stop publishing, drop subscriptions, and dispose all per-peer
        /// IIVXAvatar adapters.
        /// </summary>
        public void Detach()
        {
            foreach (var sub in _subs)
            {
                try { sub?.Dispose(); } catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} sub dispose: {e.Message}"); }
            }
            _subs.Clear();

            foreach (var kv in _peerAvatars)
            {
                try { kv.Value?.Dispose(); } catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} avatar dispose: {e.Message}"); }
            }
            _peerAvatars.Clear();

            _session = null;
            _attached = false;
            ResetIdleBaseline();
        }

        /// <summary>
        /// Plug in an IIVXAvatar factory. Called once per remote peer the
        /// first time a HEAD_POSE / AVATAR_DESCRIPTOR arrives. The
        /// returned adapter receives all subsequent ApplyHeadPose /
        /// ApplyHandPose / ApplyBlendshapes / ApplyFingerCurls / SetLOD
        /// / FallbackToBillboard calls.
        ///
        /// Pass null to switch to events-only mode (your game listens
        /// to OnPeerPose / OnPeerBlendshapes / etc).
        /// </summary>
        public void SetAvatarFactory(Func<IVXAvatarDescriptor, IIVXAvatar> factory)
        {
            _avatarFactory = factory;
        }

        /// <summary>
        /// Manually register an IIVXAvatar for a known user id (e.g. for
        /// your own local first-person avatar, or for tests). Replaces
        /// any existing adapter.
        /// </summary>
        public void RegisterAvatar(string userId, IIVXAvatar avatar)
        {
            if (string.IsNullOrEmpty(userId) || avatar == null) return;
            if (_peerAvatars.TryGetValue(userId, out var existing) && existing != null && !ReferenceEquals(existing, avatar))
            {
                try { existing.Dispose(); } catch { /* swallow */ }
            }
            _peerAvatars[userId] = avatar;
        }

        /// <summary>
        /// Look up a known peer's adapter (null if unknown).
        /// </summary>
        public IIVXAvatar GetAvatar(string userId)
            => userId != null && _peerAvatars.TryGetValue(userId, out var a) ? a : null;

        /// <summary>
        /// Force-publish a face blendshape frame. Useful when your face
        /// tracker (ARKit blendshapes, Persona, Avatar SDK) ticks on a
        /// different cadence than the head transform.
        /// </summary>
        public void PublishBlendshapes(byte[] arkit52Weights, IVXBlendshapeProfile profile = IVXBlendshapeProfile.Arkit52)
        {
            if (!_attached || _session == null || arkit52Weights == null || arkit52Weights.Length == 0) return;
            var payload = new IVXAvatarBlendshapeEnvelope
            {
                Blendshapes  = arkit52Weights,
                QuantProfile = (uint)profile
            };
            _ = _session.SendAsync(IVXAvatarOp.BLENDSHAPES, payload);
        }

        /// <summary>
        /// Force-publish finger curls (5 fingers × 3 joints, uint8 0..255).
        /// </summary>
        public void PublishFingerCurls(bool isLeft, byte[] curls)
        {
            if (!_attached || _session == null || curls == null || curls.Length == 0) return;
            var payload = new IVXAvatarFingerEnvelope { IsLeft = isLeft, FingerCurls = curls };
            _ = _session.SendAsync(IVXAvatarOp.FINGER_CURLS, payload);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDisable() => Detach();

        private void OnDestroy() => Detach();

        private void Update()
        {
            if (!_attached || _session == null) return;
            float nowMs = Time.unscaledTime * 1000f;

            if (_localHead     != null) TryPublishBone(_localHead,      IVXAvatarOp.HEAD_POSE,       _headHz, nowMs, ref _lastHeadPos,  ref _lastHeadRot,  ref _lastHeadPubMs,  ref _hasHeadBaseline);
            if (_localLeftHand != null) TryPublishBone(_localLeftHand,  IVXAvatarOp.LEFT_HAND_POSE,  _handHz, nowMs, ref _lastLeftPos,  ref _lastLeftRot,  ref _lastLeftPubMs,  ref _hasLeftBaseline);
            if (_localRightHand!= null) TryPublishBone(_localRightHand, IVXAvatarOp.RIGHT_HAND_POSE, _handHz, nowMs, ref _lastRightPos, ref _lastRightRot, ref _lastRightPubMs, ref _hasRightBaseline);
        }

        #endregion

        #region Outbound publishing

        private void TryPublishBone(
            Transform t, int opcode, int hz, float nowMs,
            ref Vector3 lastPos, ref Quaternion lastRot, ref float lastPubMs,
            ref bool hasBaseline)
        {
            float minIntervalMs = 1000f / Mathf.Max(1, hz);
            if (nowMs - lastPubMs < minIntervalMs) return;

            // Sample in anchor-local space so all peers share the same frame.
            Vector3    pos = _anchorRoot != null ? _anchorRoot.InverseTransformPoint(t.position)
                                                  : t.position;
            Quaternion rot = _anchorRoot != null ? Quaternion.Inverse(_anchorRoot.rotation) * t.rotation
                                                  : t.rotation;

            bool moved = !hasBaseline
                || (pos - lastPos).sqrMagnitude    > _posEpsilonM * _posEpsilonM
                || Quaternion.Angle(rot, lastRot) > _rotEpsilonDeg;
            bool heartbeatDue = (nowMs - lastPubMs) >= _heartbeatMs;
            if (!moved && !heartbeatDue) return;

            var quant   = QuantizePose(pos, rot, nowMs);
            var payload = new IVXAvatarPoseEnvelope { Pose = quant };
            _ = _session.SendAsync(opcode, payload);

            lastPos = pos; lastRot = rot; lastPubMs = nowMs; hasBaseline = true;
        }

        private void ResetIdleBaseline()
        {
            _hasHeadBaseline = _hasLeftBaseline = _hasRightBaseline = false;
            _lastHeadPubMs = _lastLeftPubMs = _lastRightPubMs = 0f;
        }

        #endregion

        #region Inbound dispatch

        private bool IsLocal(string userId) =>
            !string.IsNullOrEmpty(userId) &&
            _session != null &&
            string.Equals(userId, _session.LocalUserId, StringComparison.Ordinal);

        private string ResolveSender(string payloadUserId, string headerSender) =>
            !string.IsNullOrEmpty(payloadUserId) ? payloadUserId : headerSender;

        private IIVXAvatar EnsureAvatar(string userId, IVXAvatarDescriptor descriptor = null)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            if (_peerAvatars.TryGetValue(userId, out var existing) && existing != null) return existing;
            if (_avatarFactory == null) return null;

            var desc = descriptor ?? new IVXAvatarDescriptor
            {
                UserId            = userId,
                Source            = IVXAvatarSource.FallbackBillboard,
                SkeletonProfile   = "ivx_humanoid_v1",
                BlendshapeProfile = "arkit_52"
            };
            try
            {
                var av = _avatarFactory(desc);
                if (av != null) _peerAvatars[userId] = av;
                return av;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} avatar factory threw for {userId}: {e.Message}");
                return null;
            }
        }

        private void HandlePose(IVXKernelEvent<IVXAvatarPoseEnvelope> ev, IVXAvatarBone bone)
        {
            if (ev?.Payload?.Pose == null) return;
            string userId = ResolveSender(ev.Payload.UserId, ev.SenderId);
            if (IsLocal(userId)) return; // never echo our own pose back to ourselves

            // Adapter path
            var avatar = EnsureAvatar(userId);
            if (avatar != null)
            {
                switch (bone)
                {
                    case IVXAvatarBone.Head:
                        avatar.ApplyHeadPose(ev.Payload.Pose); break;
                    case IVXAvatarBone.LeftHand:
                        avatar.ApplyHandPose(true,  ev.Payload.Pose,
                            ev.Payload.GripPct ?? 0, ev.Payload.TriggerPct ?? 0); break;
                    case IVXAvatarBone.RightHand:
                        avatar.ApplyHandPose(false, ev.Payload.Pose,
                            ev.Payload.GripPct ?? 0, ev.Payload.TriggerPct ?? 0); break;
                }
            }

            // Events-only path
            if (OnPeerPose != null)
            {
                DequantizePose(ev.Payload.Pose, out var pos, out var rot);
                OnPeerPose.Invoke(new IVXPeerPoseEvent
                {
                    UserId     = userId,
                    Bone       = bone,
                    Position   = pos,
                    Rotation   = rot,
                    GripPct    = ev.Payload.GripPct ?? 0,
                    TriggerPct = ev.Payload.TriggerPct ?? 0,
                    TsMs       = ev.Payload.Pose.TsMs
                });
            }
        }

        private void HandleBlendshapes(IVXKernelEvent<IVXAvatarBlendshapeEnvelope> ev)
        {
            if (ev?.Payload?.Blendshapes == null || ev.Payload.Blendshapes.Length == 0) return;
            string userId = ResolveSender(ev.Payload.UserId, ev.SenderId);
            if (IsLocal(userId)) return;

            var profile = (IVXBlendshapeProfile)Math.Min(3u, Math.Max(0u, ev.Payload.QuantProfile));

            var avatar = EnsureAvatar(userId);
            avatar?.ApplyBlendshapes(ev.Payload.Blendshapes, profile);

            OnPeerBlendshapes?.Invoke(new IVXPeerBlendshapeEvent
            {
                UserId  = userId,
                Weights = ev.Payload.Blendshapes,
                Profile = profile
            });
        }

        private void HandleFingers(IVXKernelEvent<IVXAvatarFingerEnvelope> ev)
        {
            if (ev?.Payload?.FingerCurls == null || ev.Payload.FingerCurls.Length == 0) return;
            string userId = ResolveSender(ev.Payload.UserId, ev.SenderId);
            if (IsLocal(userId)) return;

            var avatar = EnsureAvatar(userId);
            avatar?.ApplyFingerCurls(ev.Payload.IsLeft, ev.Payload.FingerCurls);

            OnPeerFinger?.Invoke(new IVXPeerFingerEvent
            {
                UserId = userId,
                IsLeft = ev.Payload.IsLeft,
                Curls  = ev.Payload.FingerCurls
            });
        }

        private void HandleDescriptor(IVXKernelEvent<IVXAvatarDescriptor> ev)
        {
            var desc = ev?.Payload;
            if (desc == null || string.IsNullOrEmpty(desc.UserId)) return;
            if (IsLocal(desc.UserId)) return;

            var avatar = EnsureAvatar(desc.UserId, desc);
            if (avatar != null)
            {
                try { _ = avatar.LoadAsync(desc); }
                catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} LoadAsync({desc.UserId}): {e.Message}"); }
            }
            OnPeerAvatarDescriptor?.Invoke(desc);
        }

        private void HandleLodHint(IVXKernelEvent<IVXAvatarLodEnvelope> ev)
        {
            var p = ev?.Payload;
            if (p == null || string.IsNullOrEmpty(p.UserId)) return;
            if (IsLocal(p.UserId)) return;
            if (!_peerAvatars.TryGetValue(p.UserId, out var avatar) || avatar == null) return;

            var lod = (IVXAvatarLOD)Math.Min(3u, p.Lod);
            avatar.SetLOD(lod, p.Reason ?? string.Empty);
        }

        private void HandlePeerLeft(IVXKernelEvent<IVXAvatarLifecycleEnvelope> ev)
        {
            var p = ev?.Payload;
            if (p == null || string.IsNullOrEmpty(p.UserId)) return;
            if (_peerAvatars.TryGetValue(p.UserId, out var avatar) && avatar != null)
            {
                try { avatar.Dispose(); } catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} dispose {p.UserId}: {e.Message}"); }
                _peerAvatars.Remove(p.UserId);
            }
            OnPeerLeft?.Invoke(new IVXPeerLifecycleEvent { UserId = p.UserId, Reason = p.Reason });
        }

        private void HandleFallback(IVXKernelEvent<IVXAvatarLifecycleEnvelope> ev)
        {
            var p = ev?.Payload;
            if (p == null || string.IsNullOrEmpty(p.UserId)) return;
            if (_peerAvatars.TryGetValue(p.UserId, out var avatar) && avatar != null)
            {
                try { avatar.FallbackToBillboard(p.Reason ?? "server"); }
                catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} fallback {p.UserId}: {e.Message}"); }
            }
            OnPeerFallback?.Invoke(new IVXPeerLifecycleEvent { UserId = p.UserId, Reason = p.Reason });
        }

        #endregion

        #region Quantization (mirrors JS adapter + avatar_v1.proto PoseQuantized)

        private const int POS_RANGE_MM = 32_767; // ±32.767 m clamp

        /// <summary>
        /// Pack a Unity pose into the canonical PoseQuantized wire form
        /// (smallest-three quaternion + fixed-point millimetres). Bit-for-bit
        /// compatible with the JS WebXR adapter and Go avatar_replication
        /// template. Visible for tests.
        /// </summary>
        public static IVXPoseQuantized QuantizePose(Vector3 pos, Quaternion rot, float nowMs)
        {
            int pxMm = Mathf.Clamp(Mathf.RoundToInt(pos.x * 1000f), -POS_RANGE_MM, POS_RANGE_MM);
            int pyMm = Mathf.Clamp(Mathf.RoundToInt(pos.y * 1000f), -POS_RANGE_MM, POS_RANGE_MM);
            int pzMm = Mathf.Clamp(Mathf.RoundToInt(pos.z * 1000f), -POS_RANGE_MM, POS_RANGE_MM);

            float qx = rot.x, qy = rot.y, qz = rot.z, qw = rot.w;
            float ax = Mathf.Abs(qx), ay = Mathf.Abs(qy), az = Mathf.Abs(qz), aw = Mathf.Abs(qw);

            int dropIdx; float sign;
            if (aw >= ax && aw >= ay && aw >= az) { dropIdx = 3; sign = qw < 0f ? -1f : 1f; }
            else if (ax >= ay && ax >= az)        { dropIdx = 0; sign = qx < 0f ? -1f : 1f; }
            else if (ay >= az)                    { dropIdx = 1; sign = qy < 0f ? -1f : 1f; }
            else                                  { dropIdx = 2; sign = qz < 0f ? -1f : 1f; }

            float a = dropIdx == 0 ? qy : qx;
            float b = dropIdx == 1 ? qz : (dropIdx == 0 ? qz : qy);
            float c = dropIdx == 2 ? qw : (dropIdx == 0 ? qw : (dropIdx == 1 ? qw : qz));
            uint pa = Pack9(a * sign), pb = Pack9(b * sign), pc = Pack9(c * sign);
            uint rotPacked = ((uint)dropIdx & 0x3u) | (pa << 2) | (pb << 11) | (pc << 20);

            return new IVXPoseQuantized
            {
                PxMm          = pxMm,
                PyMm          = pyMm,
                PzMm          = pzMm,
                RotPacked     = rotPacked,
                QuantProfile  = 1,
                TsMs          = (long)nowMs,
                ConfidencePct = 100
            };
        }

        /// <summary>
        /// Unpack a quantized pose to Unity Vector3/Quaternion. Visible for tests
        /// and for game code consuming OnPeerPose-style events.
        /// </summary>
        public static void DequantizePose(IVXPoseQuantized q, out Vector3 pos, out Quaternion rot)
        {
            pos = new Vector3(q.PxMm / 1000f, q.PyMm / 1000f, q.PzMm / 1000f);
            uint rp = q.RotPacked;
            int dropIdx = (int)(rp & 0x3u);
            float a = Unpack9((rp >> 2)  & 0x1FFu);
            float b = Unpack9((rp >> 11) & 0x1FFu);
            float c = Unpack9((rp >> 20) & 0x1FFu);

            float dropMag = Mathf.Sqrt(Mathf.Max(0f, 1f - (a * a + b * b + c * c)));
            float qx = 0f, qy = 0f, qz = 0f, qw = 0f;
            switch (dropIdx)
            {
                case 0: qx = dropMag; qy = a; qz = b; qw = c; break;
                case 1: qy = dropMag; qx = a; qz = b; qw = c; break;
                case 2: qz = dropMag; qx = a; qy = b; qw = c; break;
                case 3: qw = dropMag; qx = a; qy = b; qz = c; break;
            }
            rot = new Quaternion(qx, qy, qz, qw);
            // Normalize against rounding error.
            float n2 = rot.x * rot.x + rot.y * rot.y + rot.z * rot.z + rot.w * rot.w;
            if (n2 > 1e-6f)
            {
                float inv = 1f / Mathf.Sqrt(n2);
                rot = new Quaternion(rot.x * inv, rot.y * inv, rot.z * inv, rot.w * inv);
            }
            else
            {
                rot = Quaternion.identity;
            }
        }

        private static uint Pack9(float v)
        {
            float scaled = Mathf.Clamp(v * 1.41421356f, -1f, 1f); // sqrt(2)
            return (uint)Mathf.RoundToInt((scaled + 1f) * 0.5f * 511f) & 0x1FFu;
        }

        private static float Unpack9(uint p)
        {
            float norm = (p / 511f) * 2f - 1f;
            return norm / 1.41421356f;
        }

        #endregion
    }
}

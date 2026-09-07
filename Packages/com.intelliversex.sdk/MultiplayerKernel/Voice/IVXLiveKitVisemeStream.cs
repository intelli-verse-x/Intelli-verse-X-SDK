// IVXLiveKitVisemeStream — Unity client implementation of
// IIVXVisemeStream. Wired to the LiveKit data channel `viseme.v1`
// from the server-rendered avatar (or another peer running the same
// protocol). Drives a SkinnedMeshRenderer's BlendShape weights so the
// Unity avatar lipsyncs without ever subscribing to the agent's video
// track — saving ~3 Mbps per remote participant.

using System;
using System.Collections.Generic;
using IntelliVerseX.MultiplayerKernel.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IntelliVerseX.MultiplayerKernel.Voice
{
    /// <summary>
    /// LiveKit-bound viseme/blendshape receiver. Drop on the same
    /// GameObject as <see cref="IVXLiveKitVoiceProvider"/> and pass
    /// each <c>DataReceived</c> payload to <see cref="Dispatch"/>.
    /// </summary>
    public sealed class IVXLiveKitVisemeStream : MonoBehaviour, IIVXVisemeStream
    {
        #region Constants
        private const string TopicLabel = "viseme.v1";
        #endregion

        #region Public State
        public bool IsActive => _isActive;
        public ulong CurrentLineId => _currentLineId;
        public uint LastIntensityPct => _lastIntensityPct;
        #endregion

        #region Events
        public event Action<IVXVisemeStreamHeader> OnHeader;
        public event Action<IVXVisemeFrame>        OnFrame;
        public event Action<IVXPhonemeFrame>       OnPhoneme;
        public event Action<IVXFacialExpressionFrame> OnExpression;
        public event Action<IVXVisemeStreamFooter> OnFooter;
        #endregion

        #region Private Fields
        [SerializeField] private bool _verboseLogging;

        // Avatar binding — caller wires the SkinnedMeshRenderer + a
        // map from ARKit blendshape index to the local mesh index.
        [SerializeField] private SkinnedMeshRenderer _faceMesh;
        private Dictionary<int, int> _arkitToMeshIndex = new Dictionary<int, int>();

        private bool  _isActive;
        private ulong _currentLineId;
        private uint  _lastIntensityPct;
        private uint  _lastFrameSeq;
        private int   _droppedFrames;
        #endregion

        #region Lifecycle
        public void BindFaceMesh(SkinnedMeshRenderer mesh, Dictionary<int, int> arkitIndexToMeshBlendshape)
        {
            _faceMesh = mesh;
            _arkitToMeshIndex = arkitIndexToMeshBlendshape ?? new Dictionary<int, int>();
        }

        public void Reset(string reason)
        {
            _isActive = false;
            _currentLineId = 0;
            _lastIntensityPct = 0;
            _lastFrameSeq = 0;
            _droppedFrames = 0;
            ResetMeshWeights();
            if (_verboseLogging) Debug.Log($"[IVXVisemeStream] reset reason={reason}");
        }

        public void Dispose()
        {
            Reset("dispose");
            OnHeader = null;
            OnFrame = null;
            OnPhoneme = null;
            OnExpression = null;
            OnFooter = null;
        }
        #endregion

        #region Dispatch
        public void Dispatch(ReadOnlyMemory<byte> bytes, bool isJson)
        {
            if (bytes.IsEmpty) return;
            if (isJson)
            {
                DispatchJson(bytes);
            }
            else
            {
                // Phase-4 ships JSON; proto-binary is wired in Phase-4.1
                // once protoc-generated C# bindings are added to the SDK.
                DispatchJson(bytes);
            }
        }

        private void DispatchJson(ReadOnlyMemory<byte> bytes)
        {
            string text;
            try
            {
                text = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
            }
            catch (Exception e)
            {
                if (_verboseLogging) Debug.LogWarning($"[IVXVisemeStream] utf8 decode failed: {e.Message}");
                return;
            }

            JObject env;
            try
            {
                env = JObject.Parse(text);
            }
            catch (Exception)
            {
                return;
            }

            var kind = env.Value<string>("kind");
            switch (kind)
            {
                case "header":     HandleHeader(env["header"] as JObject); break;
                case "frame":      HandleFrame(env["frame"] as JObject); break;
                case "phoneme":    HandlePhoneme(env["phoneme"] as JObject); break;
                case "expression": HandleExpression(env["expression"] as JObject); break;
                case "footer":     HandleFooter(env["footer"] as JObject); break;
            }
        }
        #endregion

        #region Handlers
        private void HandleHeader(JObject body)
        {
            if (body == null) return;
            var header = body.ToObject<IVXVisemeStreamHeader>();
            if (header == null) return;
            _isActive = true;
            _currentLineId = header.LineId;
            _lastFrameSeq = 0;
            _droppedFrames = 0;
            OnHeader?.Invoke(header);
            if (_verboseLogging)
                Debug.Log($"[IVXVisemeStream] header line={header.LineId} expected={header.ExpectedFrames} fhz={header.FrameHz}");
        }

        private void HandleFrame(JObject body)
        {
            if (body == null) return;
            var frame = body.ToObject<IVXVisemeFrame>();
            if (frame == null) return;
            // Drop out-of-order frames (LiveKit data channel is best-effort
            // when reliable=false, but we still want monotonic playback).
            if (frame.FrameSeq < _lastFrameSeq)
            {
                _droppedFrames++;
                return;
            }
            _lastFrameSeq = frame.FrameSeq;
            _lastIntensityPct = frame.IntensityPct;
            ApplyToMesh(frame);
            OnFrame?.Invoke(frame);
        }

        private void HandlePhoneme(JObject body)
        {
            if (body == null) return;
            var p = body.ToObject<IVXPhonemeFrame>();
            if (p == null) return;
            OnPhoneme?.Invoke(p);
        }

        private void HandleExpression(JObject body)
        {
            if (body == null) return;
            var e = body.ToObject<IVXFacialExpressionFrame>();
            if (e == null) return;
            OnExpression?.Invoke(e);
        }

        private void HandleFooter(JObject body)
        {
            if (body == null) return;
            var f = body.ToObject<IVXVisemeStreamFooter>();
            if (f == null) return;
            OnFooter?.Invoke(f);
            _isActive = false;
            ResetMeshWeights();
            if (_verboseLogging)
                Debug.Log($"[IVXVisemeStream] footer line={f.LineId} sent={f.FramesSent} dropped={_droppedFrames}");
        }
        #endregion

        #region Mesh I/O
        private void ApplyToMesh(IVXVisemeFrame frame)
        {
            if (_faceMesh == null || frame.Blendshapes == null || frame.Blendshapes.Length == 0) return;
            var weights = frame.Blendshapes;
            for (int arkitIdx = 0; arkitIdx < weights.Length; arkitIdx++)
            {
                if (!_arkitToMeshIndex.TryGetValue(arkitIdx, out var meshIdx)) continue;
                if (meshIdx < 0 || meshIdx >= _faceMesh.sharedMesh.blendShapeCount) continue;
                var w = weights[arkitIdx] / 255f * 100f; // ARKit is 0..1; Unity blend weights are 0..100
                _faceMesh.SetBlendShapeWeight(meshIdx, w);
            }
        }

        private void ResetMeshWeights()
        {
            if (_faceMesh == null) return;
            for (int i = 0; i < _faceMesh.sharedMesh.blendShapeCount; i++)
            {
                _faceMesh.SetBlendShapeWeight(i, 0f);
            }
        }
        #endregion

        #region Diagnostics
        public string DiagnosticsJson()
        {
            return JsonConvert.SerializeObject(new
            {
                topic = TopicLabel,
                isActive = _isActive,
                currentLineId = _currentLineId,
                lastFrameSeq = _lastFrameSeq,
                lastIntensityPct = _lastIntensityPct,
                droppedFrames = _droppedFrames,
            });
        }
        #endregion
    }
}

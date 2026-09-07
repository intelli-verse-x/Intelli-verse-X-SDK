// IVXSyncTurnEchoSample — minimal sample demonstrating the multiplayer
// kernel adapter against the `sync-turn-v1` template. Drop on a GameObject
// in any scene where IVXNakamaManager has finished initializing, press the
// "Run Echo" button (or call from code) and watch the console.
//
// Flow:
//   1. Adapter wraps IVXNakamaManager.Instance.
//   2. CreateAndJoin a sync-turn match with 1 player + 1 turn (echo cfg).
//   3. Server emits TURN_START -> sample submits a JSON echo response.
//   4. Server emits TURN_RESOLVED + MATCH_ENDED.
//
// This is a sample, not a unit test. It is wrapped in
// `UNITY_INCLUDE_TESTS` so it is excluded from production builds by
// default.

#if UNITY_EDITOR || INTELLIVERSEX_MP_SAMPLES

using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.MultiplayerKernel.Adapters;
using IntelliVerseX.MultiplayerKernel.Templates.SyncTurn;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IntelliVerseX.MultiplayerKernel.Samples
{
    /// <summary>
    /// Minimal sync-turn smoke test driven from a MonoBehaviour. Runs once
    /// when <see cref="RunOnce"/> is invoked.
    /// </summary>
    public class IVXSyncTurnEchoSample : MonoBehaviour
    {
        private const string LOG = "[SyncTurnEchoSample]";

        [Header("Provider — leave null to find IVXNakamaManager in scene")]
        [SerializeField] private MonoBehaviour _providerBehaviour;

        [Header("Match settings")]
        [SerializeField] private string _gameId    = "demo";
        [SerializeField] private int    _minPlayers = 1;
        [SerializeField] private int    _maxPlayers = 1;
        [SerializeField] private int    _maxTurns   = 1;
        [SerializeField] private int    _inputWindowMs = 5000;

        [Header("Run automatically on Start (after a short grace period)")]
        [SerializeField] private bool  _autoRun = false;
        [SerializeField] private float _autoRunDelaySec = 2f;

        private IVXNakamaMultiplayer _adapter;
        private IIVXMatchSession     _session;
        private IVXSyncTurnClient    _syncTurn;
        private bool                 _running;

        private async void Start()
        {
            if (!_autoRun) return;
            await Task.Delay((int)(_autoRunDelaySec * 1000));
            await RunOnce();
        }

        [ContextMenu("Run Echo")]
        public async void RunEchoMenu() => await RunOnce();

        public async Task RunOnce()
        {
            if (_running)
            {
                Debug.LogWarning($"{LOG} already running — ignoring");
                return;
            }
            _running = true;
            try
            {
                var provider = ResolveProvider();
                if (provider == null || !provider.IsInitialized)
                {
                    Debug.LogError($"{LOG} no initialized IIVXNakamaRealtimeProvider available");
                    return;
                }

                _adapter = new IVXNakamaMultiplayer(provider);
                _adapter.OnKernelError += e =>
                    Debug.LogError($"{LOG} kernel error code={e.Payload?.Code} detail={e.Payload?.Detail}");
                _adapter.OnTransportStateChanged += s =>
                    Debug.Log($"{LOG} transport={s}");
                await _adapter.InitializeAsync();

                var req = new IVXCreateMatchRequest("sync-turn-v1", _gameId)
                {
                    TemplateInit = new Dictionary<string, object>
                    {
                        { "min_players",            _minPlayers },
                        { "max_players",            _maxPlayers },
                        { "default_input_window_ms", _inputWindowMs },
                        { "max_turns",              _maxTurns },
                        { "generator_id",           "echo" },
                    },
                };
                Debug.Log($"{LOG} creating match...");
                _session = await _adapter.CreateAndJoinAsync(req, new IVXJoinOptions
                {
                    ClientBuildId = Application.version,
                });
                Debug.Log($"{LOG} joined match={_session.MatchId} template={_session.TemplateId}");

                _syncTurn = new IVXSyncTurnClient(_session);
                _syncTurn.OnTurnStart += async ev =>
                {
                    Debug.Log($"{LOG} TURN_START turn={ev.Payload?.TurnIndex} window={ev.Payload?.InputWindowMs}ms");
                    var submission = JToken.FromObject(new { echo = "hello-from-unity" });
                    await _syncTurn.SubmitInputAsync(
                        ev.Payload?.TurnIndex ?? 0,
                        submission,
                        clientResponseMs: 250);
                };
                _syncTurn.OnTurnResolved += ev =>
                    Debug.Log($"{LOG} TURN_RESOLVED turn={ev.Payload?.TurnIndex}");
                _syncTurn.OnScoreUpdate += ev =>
                    Debug.Log($"{LOG} SCORE_UPDATE entries={(ev.Payload?.Totals?.Count ?? 0)}");

                _session.OnMatchEnded += ev =>
                {
                    Debug.Log($"{LOG} MATCH_ENDED reason={ev.Payload?.Reason}");
                    Cleanup();
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{LOG} run failed: {e}");
                Cleanup();
            }
        }

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            _running = false;
            _syncTurn?.Dispose(); _syncTurn = null;
            try { _ = _session?.LeaveAsync(); } catch { }
            _session = null;
            try { _ = _adapter?.ShutdownAsync(); } catch { }
            _adapter = null;
        }

        private IIVXNakamaRealtimeProvider ResolveProvider()
        {
            if (_providerBehaviour is IIVXNakamaRealtimeProvider asInterface) return asInterface;
            return UnityEngine.Object.FindObjectOfType<IVXNakamaManager>();
        }
    }
}

#endif

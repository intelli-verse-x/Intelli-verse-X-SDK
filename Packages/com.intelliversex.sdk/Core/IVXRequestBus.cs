using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Canonical RPC request bus (revamp P2 / control-plane Phase B).
    /// Timeout, exponential backoff + jitter, and honor of <c>retry_after_ms</c> in payloads.
    /// Domain managers should invoke Nakama through this instead of ad-hoc retry loops.
    /// </summary>
    public static class IVXRequestBus
    {
        public const int DefaultRpcTimeoutMs = 8000;
        public const int DefaultAuthTimeoutMs = 15000;
        public const int MaxTrafficHistory = 50;

        public struct Result
        {
            public bool Success;
            public string Payload;
            public string Error;
            public string ErrorCode;
            public int? RetryAfterMs;
            public int Attempts;
        }

        /// <summary>One Traffic-tab row (Control Center).</summary>
        public struct TrafficEntry
        {
            public DateTime Utc;
            public string RpcId;
            public int Attempt;
            public string Status;
            public long LatencyMs;
            public string Error;
            public int? RetryAfterMs;
            public bool InFlight;
        }

        private static readonly object TrafficLock = new object();
        private static readonly List<TrafficEntry> TrafficHistory = new List<TrafficEntry>(MaxTrafficHistory);
        private static int InFlightCount;

        /// <summary>In-flight RPC count for the Traffic tab.</summary>
        public static int GetInFlightCount()
        {
            lock (TrafficLock)
            {
                return InFlightCount;
            }
        }

        /// <summary>Newest-first copy of the last <see cref="MaxTrafficHistory"/> traffic rows.</summary>
        public static TrafficEntry[] GetRecentTraffic()
        {
            lock (TrafficLock)
            {
                var copy = new TrafficEntry[TrafficHistory.Count];
                for (int i = 0; i < TrafficHistory.Count; i++)
                    copy[i] = TrafficHistory[TrafficHistory.Count - 1 - i];
                return copy;
            }
        }

        public static void ClearTrafficHistory()
        {
            lock (TrafficLock)
            {
                TrafficHistory.Clear();
            }
        }

        /// <summary>
        /// Executes an async RPC invoke with timeout and retries.
        /// Retries on timeout, exceptions, and when the payload reports <c>retry_after_ms</c>.
        /// </summary>
        /// <param name="rpcId">RPC id for logging / Traffic tab.</param>
        /// <param name="invoke">Returns the raw payload string. Throw on transport failure.</param>
        /// <param name="isSuccessPayload">Optional: return false to treat a 200 payload as a soft failure.</param>
        /// <param name="shouldRetrySoftFailure">Optional: when soft-failed, return false to stop retrying (non-retryable server codes).</param>
        public static async Task<Result> ExecuteAsync(
            string rpcId,
            Func<CancellationToken, Task<string>> invoke,
            int maxAttempts = 3,
            int timeoutMs = DefaultRpcTimeoutMs,
            float baseDelaySeconds = 1f,
            Func<string, bool> isSuccessPayload = null,
            Func<string, bool> shouldRetrySoftFailure = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(rpcId))
                throw new ArgumentException("rpcId is required", "rpcId");
            if (invoke == null)
                throw new ArgumentNullException("invoke");
            if (maxAttempts < 1)
                maxAttempts = 1;

            Result last = new Result
            {
                Success = false,
                Error = "No attempts",
                ErrorCode = "NO_ATTEMPT",
                Attempts = 0
            };

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                BeginInFlight();
                var sw = Stopwatch.StartNew();

                using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutCts.CancelAfter(timeoutMs);
                    try
                    {
                        string payload = await invoke(timeoutCts.Token).ConfigureAwait(false);
                        sw.Stop();
                        int? retryAfter = TryParseRetryAfterMs(payload);
                        bool ok = isSuccessPayload == null || isSuccessPayload(payload);

                        last = new Result
                        {
                            Success = ok,
                            Payload = payload,
                            Error = ok ? null : "RPC soft failure",
                            ErrorCode = ok ? null : "RPC_SOFT_FAIL",
                            RetryAfterMs = retryAfter,
                            Attempts = attempt
                        };

                        RecordTraffic(rpcId, attempt, ok ? "ok" : "soft_fail", sw.ElapsedMilliseconds, last.Error, retryAfter);
                        EndInFlight();

                        if (ok)
                            return last;

                        bool canRetry = shouldRetrySoftFailure == null || shouldRetrySoftFailure(payload);
                        if (!canRetry)
                            return last;

                        if (retryAfter.HasValue && attempt < maxAttempts)
                        {
                            await DelayWithJitterAsync(retryAfter.Value / 1000f, cancellationToken)
                                .ConfigureAwait(false);
                            continue;
                        }

                        if (attempt < maxAttempts)
                        {
                            float delay = baseDelaySeconds * (float)Math.Pow(2, attempt - 1);
                            await DelayWithJitterAsync(delay, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        sw.Stop();
                        last = new Result
                        {
                            Success = false,
                            Error = "RPC timed out after " + timeoutMs + "ms",
                            ErrorCode = "TIMEOUT",
                            Attempts = attempt
                        };
                        RecordTraffic(rpcId, attempt, "timeout", sw.ElapsedMilliseconds, last.Error, null);
                        EndInFlight();
                        if (attempt < maxAttempts)
                        {
                            float delay = baseDelaySeconds * (float)Math.Pow(2, attempt - 1);
                            await DelayWithJitterAsync(delay, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        last = new Result
                        {
                            Success = false,
                            Error = ex.Message,
                            ErrorCode = "EXCEPTION",
                            Attempts = attempt
                        };
                        RecordTraffic(rpcId, attempt, "error", sw.ElapsedMilliseconds, last.Error, null);
                        EndInFlight();
                        if (attempt >= maxAttempts)
                            return last;

                        float delay = baseDelaySeconds * (float)Math.Pow(2, attempt - 1);
                        await DelayWithJitterAsync(delay, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return last;
        }

        private static void BeginInFlight()
        {
            lock (TrafficLock)
            {
                InFlightCount++;
            }
        }

        private static void EndInFlight()
        {
            lock (TrafficLock)
            {
                if (InFlightCount > 0)
                    InFlightCount--;
            }
        }

        private static void RecordTraffic(
            string rpcId,
            int attempt,
            string status,
            long latencyMs,
            string error,
            int? retryAfterMs)
        {
            var entry = new TrafficEntry
            {
                Utc = DateTime.UtcNow,
                RpcId = rpcId,
                Attempt = attempt,
                Status = status,
                LatencyMs = latencyMs,
                Error = error,
                RetryAfterMs = retryAfterMs,
                InFlight = false
            };

            lock (TrafficLock)
            {
                TrafficHistory.Add(entry);
                while (TrafficHistory.Count > MaxTrafficHistory)
                    TrafficHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Best-effort parse of <c>retry_after_ms</c> from a JSON payload (control-plane envelope).
        /// </summary>
        public static int? TryParseRetryAfterMs(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return null;

            const string key = "\"retry_after_ms\"";
            int idx = payload.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return null;

            int colon = payload.IndexOf(':', idx + key.Length);
            if (colon < 0)
                return null;

            int i = colon + 1;
            while (i < payload.Length && (payload[i] == ' ' || payload[i] == '\t'))
                i++;

            int start = i;
            while (i < payload.Length && char.IsDigit(payload[i]))
                i++;

            if (i == start)
                return null;

            int ms;
            if (!int.TryParse(payload.Substring(start, i - start), out ms) || ms <= 0)
                return null;

            return ms;
        }

        private static async Task DelayWithJitterAsync(float delaySeconds, CancellationToken ct)
        {
            if (delaySeconds < 0.05f)
                delaySeconds = 0.05f;

            // ±20% jitter
            float jitter = 1f + (UnityEngine.Random.value * 0.4f - 0.2f);
            int ms = Mathf.Max(50, Mathf.RoundToInt(delaySeconds * jitter * 1000f));
            await Task.Delay(ms, ct).ConfigureAwait(false);
        }
    }
}

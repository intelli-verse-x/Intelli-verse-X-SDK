using System;
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

        public struct Result
        {
            public bool Success;
            public string Payload;
            public string Error;
            public string ErrorCode;
            public int? RetryAfterMs;
            public int Attempts;
        }

        /// <summary>
        /// Executes an async RPC invoke with timeout and retries.
        /// Retries on timeout, exceptions, and when the payload reports <c>retry_after_ms</c>.
        /// </summary>
        /// <param name="rpcId">RPC id for logging / future Traffic tab.</param>
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

                using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutCts.CancelAfter(timeoutMs);
                    try
                    {
                        string payload = await invoke(timeoutCts.Token).ConfigureAwait(false);
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
                        last = new Result
                        {
                            Success = false,
                            Error = "RPC timed out after " + timeoutMs + "ms",
                            ErrorCode = "TIMEOUT",
                            Attempts = attempt
                        };
                        if (attempt < maxAttempts)
                        {
                            float delay = baseDelaySeconds * (float)Math.Pow(2, attempt - 1);
                            await DelayWithJitterAsync(delay, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        last = new Result
                        {
                            Success = false,
                            Error = ex.Message,
                            ErrorCode = "EXCEPTION",
                            Attempts = attempt
                        };
                        if (attempt >= maxAttempts)
                            return last;

                        float delay = baseDelaySeconds * (float)Math.Pow(2, attempt - 1);
                        await DelayWithJitterAsync(delay, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return last;
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

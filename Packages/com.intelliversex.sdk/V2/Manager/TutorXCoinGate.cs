using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// TutorX AI Coin Gate - manages daily free tier + coin gating for DeepTutor AI features.
    /// 
    /// Flow:
    /// 1. Before AI message, call CheckAllowanceAsync() to verify user can send
    /// 2. If allowed, send the AI message  
    /// 3. After successful AI response, call RecordUsageAsync() to track usage
    /// 4. If free tier exhausted, deduct coins via IVXNWalletManager
    /// 
    /// Usage:
    ///   TutorXCoinGate.GameId = "your-game-uuid";
    ///   var status = await TutorXCoinGate.CheckAllowanceAsync();
    ///   if (status.CanUse) { ... }
    /// </summary>
    public static class TutorXCoinGate
    {
        #region Constants & Config

        private const string LogPrefix = "[TutorX-CoinGate]";
        
        public static int FreeMessagesPerDay { get; set; } = 3;
        public static int CostPerMessage { get; set; } = 5;
        public static string GameId { get; set; } = "126bf539-dae2-4bcf-964d-316c0fa1f92b";
        public static bool EnableDebugLogs { get; set; } = true;

        #endregion

        #region State
        
        private static TutorXAllowanceStatus _cachedStatus;
        private static DateTime _lastCheckUtc = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

        #endregion

        #region Events

        public static event Action<TutorXAllowanceStatus> OnAllowanceChanged;
        public static event Action<int, int> OnUsageRecorded;

        #endregion

        #region Data Models

        [Serializable]
        public struct TutorXAllowanceStatus
        {
            public bool CanUse;
            public int FreeRemaining;
            public long CoinBalance;
            public int CostPerMsg;
            public int UsedToday;
            public DateTime CheckedAtUtc;

            public bool IsFreeTier => FreeRemaining > 0;
            public bool RequiresCoins => !IsFreeTier && CoinBalance >= CostPerMsg;
            public bool IsBlocked => !IsFreeTier && CoinBalance < CostPerMsg;

            public override string ToString()
            {
                return $"[TutorXStatus] CanUse={CanUse}, Free={FreeRemaining}, Coins={CoinBalance}, Cost={CostPerMsg}, UsedToday={UsedToday}";
            }
        }

        [Serializable]
        private class AllowanceRpcResponse
        {
            public bool success;
            public bool canUse;
            public int freeRemaining;
            public long coinBalance;
            public int costPerMsg;
            public int usedToday;
            public string userId;
            public string gameId;
            public string timestamp;
        }

        [Serializable]
        private class UsageRpcResponse
        {
            public bool success;
            public int usedToday;
            public int freeRemaining;
            public string timestamp;
        }

        #endregion

        #region Backend Delegates

        public static Func<string, CancellationToken, Task<string>> CallRpcAsync { get; set; }

        #endregion

        #region Public API

        public static TutorXAllowanceStatus CachedStatus => _cachedStatus;
        public static bool HasValidCache => (DateTime.UtcNow - _lastCheckUtc) < CacheDuration;

        public static async Task<TutorXAllowanceStatus> CheckAllowanceAsync(
            bool forceRefresh = false,
            CancellationToken cancellationToken = default)
        {
            if (!forceRefresh && HasValidCache)
            {
                Log($"{LogPrefix} Returning cached status: {_cachedStatus}");
                return _cachedStatus;
            }

            if (CallRpcAsync == null)
            {
                Log($"{LogPrefix} CallRpcAsync delegate not set. Using local fallback.", isWarning: true);
                return CreateLocalStatus();
            }

            try
            {
                var payload = JsonUtility.ToJson(new { gameId = GameId });
                var response = await CallRpcAsync("tutorx_check_allowance", cancellationToken);

                var parsed = JsonUtility.FromJson<AllowanceRpcResponse>(response);
                if (parsed == null || !parsed.success)
                {
                    Log($"{LogPrefix} RPC returned failure: {response}", isWarning: true);
                    return CreateLocalStatus();
                }

                _cachedStatus = new TutorXAllowanceStatus
                {
                    CanUse = parsed.canUse,
                    FreeRemaining = parsed.freeRemaining,
                    CoinBalance = parsed.coinBalance,
                    CostPerMsg = parsed.costPerMsg,
                    UsedToday = parsed.usedToday,
                    CheckedAtUtc = DateTime.UtcNow
                };
                _lastCheckUtc = DateTime.UtcNow;

                Log($"{LogPrefix} Allowance checked: {_cachedStatus}");
                SafeRaiseAllowanceChanged(_cachedStatus);

                return _cachedStatus;
            }
            catch (Exception ex)
            {
                Log($"{LogPrefix} CheckAllowanceAsync failed: {ex.Message}", isError: true);
                return CreateLocalStatus();
            }
        }

        public static async Task<bool> RecordUsageAsync(CancellationToken cancellationToken = default)
        {
            if (CallRpcAsync == null)
            {
                Log($"{LogPrefix} CallRpcAsync delegate not set. Cannot record usage.", isWarning: true);
                return false;
            }

            try
            {
                var payload = JsonUtility.ToJson(new { gameId = GameId });
                var response = await CallRpcAsync("tutorx_record_usage", cancellationToken);

                var parsed = JsonUtility.FromJson<UsageRpcResponse>(response);
                if (parsed == null || !parsed.success)
                {
                    Log($"{LogPrefix} Record usage RPC returned failure: {response}", isWarning: true);
                    return false;
                }

                _cachedStatus.UsedToday = parsed.usedToday;
                _cachedStatus.FreeRemaining = parsed.freeRemaining;
                _cachedStatus.CanUse = parsed.freeRemaining > 0 || 
                                       _cachedStatus.CoinBalance >= _cachedStatus.CostPerMsg;

                Log($"{LogPrefix} Usage recorded: usedToday={parsed.usedToday}, freeRemaining={parsed.freeRemaining}");
                SafeRaiseUsageRecorded(parsed.usedToday, parsed.freeRemaining);

                return true;
            }
            catch (Exception ex)
            {
                Log($"{LogPrefix} RecordUsageAsync failed: {ex.Message}", isError: true);
                return false;
            }
        }

        public static async Task<bool> ConsumeAIMessageAsync(CancellationToken cancellationToken = default)
        {
            var status = await CheckAllowanceAsync(forceRefresh: true, cancellationToken);
            
            if (!status.CanUse)
            {
                Log($"{LogPrefix} Cannot consume AI message - blocked: {status}", isWarning: true);
                return false;
            }

            if (!status.IsFreeTier)
            {
                bool spent = await IVXNWalletManager.TrySpendGameAsync(
                    status.CostPerMsg, 
                    "TutorX AI message",
                    null, 
                    cancellationToken);

                if (!spent)
                {
                    Log($"{LogPrefix} Failed to deduct coins for AI message", isWarning: true);
                    return false;
                }

                Log($"{LogPrefix} Deducted {status.CostPerMsg} coins for AI message");
            }

            await RecordUsageAsync(cancellationToken);
            return true;
        }

        public static void InvalidateCache()
        {
            _lastCheckUtc = DateTime.MinValue;
            Log($"{LogPrefix} Cache invalidated");
        }

        #endregion

        #region Helpers

        private static TutorXAllowanceStatus CreateLocalStatus()
        {
            var gameBalance = IVXNWalletManager.IsInitialized ? IVXNWalletManager.GameBalance : 0;
            return new TutorXAllowanceStatus
            {
                CanUse = true,
                FreeRemaining = FreeMessagesPerDay,
                CoinBalance = gameBalance,
                CostPerMsg = CostPerMessage,
                UsedToday = 0,
                CheckedAtUtc = DateTime.UtcNow
            };
        }

        private static void SafeRaiseAllowanceChanged(TutorXAllowanceStatus status)
        {
            try { OnAllowanceChanged?.Invoke(status); }
            catch (Exception ex) { Log($"{LogPrefix} OnAllowanceChanged error: {ex.Message}", isError: true); }
        }

        private static void SafeRaiseUsageRecorded(int usedToday, int freeRemaining)
        {
            try { OnUsageRecorded?.Invoke(usedToday, freeRemaining); }
            catch (Exception ex) { Log($"{LogPrefix} OnUsageRecorded error: {ex.Message}", isError: true); }
        }

        private static void Log(string message, bool isWarning = false, bool isError = false)
        {
            if (!EnableDebugLogs && !isError) return;

            if (isError) Debug.LogError(message);
            else if (isWarning) Debug.LogWarning(message);
            else Debug.Log(message);
        }

        #endregion
    }
}

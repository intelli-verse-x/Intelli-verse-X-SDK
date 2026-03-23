using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Central static wallet manager for the IntelliVerseX SDK.
    /// 
    /// Responsibilities:
    /// - Keep an in-memory snapshot of game & global wallet balances.
    /// - Provide a single event for balance changes (for UI, etc.).
    /// - Provide async operations to refresh and mutate wallet balances.
    /// - Delegate actual backend work via pluggable funcs.
    /// 
    /// Usage:
    /// 1. At SDK init (after auth), call:
    ///      IVXNWalletManager.Initialize(initialGame, initialGlobal);
    ///    and plug in your backend:
    ///      IVXNWalletManager.RefreshFromServerAsync = MyBackend.RefreshBalancesAsync;
    ///      IVXNWalletManager.ApplyOperationOnServerAsync = MyBackend.ApplyWalletOperationAsync;
    /// 
    /// 2. In UI (e.g. IVXWalletDisplay), subscribe to:
    ///      IVXNWalletManager.OnWalletBalanceChanged += (game, global) => { ... };
    /// 
    /// 3. From gameplay / economy code, call:
    ///      await IVXNWalletManager.TrySpendGameAsync(50, "Buy chest");
    ///      await IVXNWalletManager.CreditGlobalAsync(100, "Ad reward");
    /// </summary>
    public static class IVXNWalletManager
    {
        #region Nested types

        /// <summary>
        /// Which wallet we are operating on.
        /// </summary>
        public enum WalletKind
        {
            Game,
            Global
        }

        /// <summary>
        /// Snapshot of wallet state used by the manager.
        /// This is the "truth" that UIs and systems read.
        /// </summary>
        [Serializable]
        public struct WalletSnapshot
        {
            public long GameBalance;
            public long GlobalBalance;
            public DateTime LastUpdatedUtc;

            public WalletSnapshot(long game, long global)
            {
                GameBalance = game;
                GlobalBalance = global;
                LastUpdatedUtc = DateTime.UtcNow;
            }

            public override string ToString()
            {
                return $"[Snapshot] Game={GameBalance}, Global={GlobalBalance}, UpdatedUtc={LastUpdatedUtc:O}";
            }
        }

        /// <summary>
        /// Generic wallet delta operation that will be sent to the backend.
        /// Delta > 0 : credit, Delta &lt; 0 : debit.
        /// </summary>
        [Serializable]
        public struct WalletOperation
        {
            public WalletKind Wallet;
            public long Delta;
            public string Reason;
            public string MetadataJson;

            public override string ToString()
            {
                return $"{Wallet} {(Delta >= 0 ? "+" : "")}{Delta} ({Reason})";
            }
        }

        #endregion

        #region Fields & properties

        private const string LogPrefix = "[IVX-WALLET]";

        private static readonly object SyncRoot = new object();

        private static bool _initialized;
        private static bool _isRefreshing;
        private static WalletSnapshot _snapshot;

        // Track active operations for debugging/monitoring.
        private static int _activeOperationsCount;

        /// <summary>
        /// Toggle for non-error logs. Errors / exceptions always log.
        /// Can be disabled in production builds if desired.
        /// </summary>
        public static bool EnableDebugLogs { get; set; } = true;

        /// <summary>
        /// Called whenever balances change (after server-confirmed operations or forced sync).
        /// Args: (gameBalance, globalBalance).
        /// </summary>
        public static event Action<long, long> OnWalletBalanceChanged;

        /// <summary>
        /// Called after a wallet operation (credit/debit/refresh).
        /// bool success, string errorMessage (null if success).
        /// </summary>
        public static event Action<bool, string> OnWalletOperationCompleted;

        /// <summary>
        /// Delegate used by RefreshBalancesAsync to talk to backend (e.g. Nakama).
        /// Should return the latest server snapshot.
        /// </summary>
        public static Func<CancellationToken, Task<WalletSnapshot>> RefreshFromServerAsync { get; set; }

        /// <summary>
        /// Delegate used by credit/debit operations to talk to backend.
        /// Should apply the delta on the server and return the updated snapshot.
        /// </summary>
        public static Func<WalletOperation, CancellationToken, Task<WalletSnapshot>> ApplyOperationOnServerAsync { get; set; }

        /// <summary>Current game wallet balance.</summary>
        public static long GameBalance
        {
            get { EnsureInitialized(); return _snapshot.GameBalance; }
        }

        /// <summary>Current global wallet balance.</summary>
        public static long GlobalBalance
        {
            get { EnsureInitialized(); return _snapshot.GlobalBalance; }
        }

        /// <summary>True when the manager has been initialized at least once.</summary>
        public static bool IsInitialized => _initialized;

        /// <summary>True when a refresh operation is currently running.</summary>
        public static bool IsRefreshing => _isRefreshing;

        /// <summary>Number of active wallet operations (useful for debugging).</summary>
        public static int ActiveOperationsCount => _activeOperationsCount;

        /// <summary>Latest full snapshot (copy).</summary>
        public static WalletSnapshot Snapshot
        {
            get
            {
                EnsureInitialized();
                return _snapshot;
            }
        }

        #endregion

        #region Initialization & sync

        /// <summary>
        /// Initialize the wallet manager with a starting snapshot (usually from login response).
        /// Safe to call multiple times; subsequent calls override the snapshot.
        /// </summary>
        public static void Initialize(long initialGameBalance = 0, long initialGlobalBalance = 0)
        {
            lock (SyncRoot)
            {
                _snapshot = new WalletSnapshot(initialGameBalance, initialGlobalBalance);
                _initialized = true;
            }

            SafeRaiseBalanceChanged();
            Log($"{LogPrefix} Initialized. Game={initialGameBalance}, Global={initialGlobalBalance}");
        }

        /// <summary>
        /// For backwards compatibility with IntelliVerseXIdentity or other systems that already
        /// know the balances. You can call this from existing identity events.
        /// </summary>
        public static void ForceSetBalances(long gameBalance, long globalBalance, bool suppressEvent = false)
        {
            EnsureInitialized();

            lock (SyncRoot)
            {
                _snapshot = new WalletSnapshot(gameBalance, globalBalance);
            }

            if (!suppressEvent)
            {
                SafeRaiseBalanceChanged();
            }

            Log($"{LogPrefix} ForceSetBalances. Game={gameBalance}, Global={globalBalance}");
        }

        /// <summary>
        /// Refresh balances from backend (e.g. via Nakama RPC).
        /// Uses RefreshFromServerAsync delegate. Returns false if delegate is not set or on error.
        /// </summary>
        public static async Task<bool> RefreshBalancesAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (RefreshFromServerAsync == null)
            {
                Log($"{LogPrefix} RefreshFromServerAsync delegate is null. " +
                    "Backend is not configured; skipping refresh.", isWarning: true);
                SafeRaiseOperationCompleted(false, "Wallet backend not configured.");
                return false;
            }

            if (_isRefreshing)
            {
                // Avoid concurrent refresh storms; this is a soft guard.
                Log($"{LogPrefix} Refresh already in progress, ignoring additional request.");
                return false;
            }

            _isRefreshing = true;
            var opId = Guid.NewGuid().ToString("N");
            Log($"{LogPrefix} [Refresh:{opId}] Starting wallet refresh... CurrentSnapshot={Snapshot}");

            try
            {
                var newSnapshot = await RefreshFromServerAsync(cancellationToken);

                lock (SyncRoot)
                {
                    _snapshot = newSnapshot;
                }

                SafeRaiseBalanceChanged();
                SafeRaiseOperationCompleted(true, null);

                Log($"{LogPrefix} [Refresh:{opId}] Success. NewSnapshot={_snapshot}");
                return true;
            }
            catch (OperationCanceledException)
            {
                Log($"{LogPrefix} [Refresh:{opId}] RefreshBalancesAsync cancelled.", isWarning: true);
                SafeRaiseOperationCompleted(false, "Refresh cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                Log($"{LogPrefix} [Refresh:{opId}] RefreshBalancesAsync failed: {ex.Message}", isError: true);
                Debug.LogException(ex);
                SafeRaiseOperationCompleted(false, ex.Message);
                return false;
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        #endregion

        #region Public high-level operations

        /// <summary>
        /// Credit game wallet (e.g. ad reward, quest reward).
        /// Positive amount only.
        /// </summary>
        public static Task<bool> CreditGameAsync(long amount, string reason = null, string metadataJson = null,
                                                 CancellationToken cancellationToken = default)
        {
            return ApplyDeltaAsync(WalletKind.Game, Math.Abs(amount), reason, metadataJson, cancellationToken);
        }

        /// <summary>
        /// Credit global wallet (IVX tokens, etc).
        /// Positive amount only.
        /// </summary>
        public static Task<bool> CreditGlobalAsync(long amount, string reason = null, string metadataJson = null,
                                                   CancellationToken cancellationToken = default)
        {
            return ApplyDeltaAsync(WalletKind.Global, Math.Abs(amount), reason, metadataJson, cancellationToken);
        }

        /// <summary>
        /// Try to spend from the game wallet.
        /// Returns false if not enough local balance OR backend fails.
        /// </summary>
        public static async Task<bool> TrySpendGameAsync(long amount, string reason = null, string metadataJson = null,
                                                         CancellationToken cancellationToken = default)
        {
            amount = Math.Abs(amount);

            // Local check to avoid obviously invalid spends.
            if (!HasSufficientBalance(WalletKind.Game, amount))
            {
                Log($"{LogPrefix} TrySpendGameAsync failed locally: insufficient funds. " +
                    $"Balance={GameBalance}, Required={amount}", isWarning: true);
                SafeRaiseOperationCompleted(false, "Insufficient game balance.");
                return false;
            }

            return await ApplyDeltaAsync(WalletKind.Game, -amount, reason, metadataJson, cancellationToken);
        }

        /// <summary>
        /// Try to spend from the global wallet.
        /// Returns false if not enough local balance OR backend fails.
        /// </summary>
        public static async Task<bool> TrySpendGlobalAsync(long amount, string reason = null, string metadataJson = null,
                                                           CancellationToken cancellationToken = default)
        {
            amount = Math.Abs(amount);

            if (!HasSufficientBalance(WalletKind.Global, amount))
            {
                Log($"{LogPrefix} TrySpendGlobalAsync failed locally: insufficient funds. " +
                    $"Balance={GlobalBalance}, Required={amount}", isWarning: true);
                SafeRaiseOperationCompleted(false, "Insufficient global balance.");
                return false;
            }

            return await ApplyDeltaAsync(WalletKind.Global, -amount, reason, metadataJson, cancellationToken);
        }

        /// <summary>
        /// Example convenience: transfer between game and global wallet.
        /// Implemented as a single backend operation (semantics defined by ApplyOperationOnServerAsync).
        /// </summary>
        public static async Task<bool> TransferGameToGlobalAsync(long amount, string reason = null, string metadataJson = null,
                                                                 CancellationToken cancellationToken = default)
        {
            var combinedReason = reason ?? "Game->Global transfer";

            var op = new WalletOperation
            {
                Wallet = WalletKind.Game,
                Delta = -Math.Abs(amount),              // withdraw from game
                Reason = combinedReason,
                MetadataJson = metadataJson ?? string.Empty
            };

            return await ApplyOperationAsync(op, cancellationToken);
        }

        /// <summary>
        /// Example convenience: transfer between global and game wallet.
        /// </summary>
        public static async Task<bool> TransferGlobalToGameAsync(long amount, string reason = null, string metadataJson = null,
                                                                 CancellationToken cancellationToken = default)
        {
            var combinedReason = reason ?? "Global->Game transfer";

            var op = new WalletOperation
            {
                Wallet = WalletKind.Global,
                Delta = -Math.Abs(amount),              // withdraw from global
                Reason = combinedReason,
                MetadataJson = metadataJson ?? string.Empty
            };

            return await ApplyOperationAsync(op, cancellationToken);
        }

        #endregion

        #region Backend wiring helpers

        private static async Task<bool> ApplyDeltaAsync(WalletKind wallet, long delta,
                                                        string reason, string metadataJson,
                                                        CancellationToken cancellationToken)
        {
            EnsureInitialized();

            if (delta == 0)
            {
                Log($"{LogPrefix} ApplyDeltaAsync called with 0 delta for {wallet}. Ignoring.", isWarning: true);
                SafeRaiseOperationCompleted(true, null);
                return true;
            }

            var op = new WalletOperation
            {
                Wallet = wallet,
                Delta = delta,
                Reason = reason ?? string.Empty,
                MetadataJson = metadataJson ?? string.Empty
            };

            return await ApplyOperationAsync(op, cancellationToken);
        }

        private static async Task<bool> ApplyOperationAsync(WalletOperation operation, CancellationToken cancellationToken)
        {
            if (ApplyOperationOnServerAsync == null)
            {
                Log($"{LogPrefix} ApplyOperationOnServerAsync delegate is null. " +
                    "Backend is not configured; skipping operation.", isWarning: true);
                SafeRaiseOperationCompleted(false, "Wallet backend not configured.");
                return false;
            }

            EnsureInitialized();

            var opId = Guid.NewGuid().ToString("N");
            var snapshotBefore = Snapshot;

            var running = Interlocked.Increment(ref _activeOperationsCount);

            Log($"{LogPrefix} [Op:{opId}] Begin operation: {operation}. " +
                $"SnapshotBefore={snapshotBefore}. ActiveOpsNow={running}");

            bool success = false;
            string errorMsg = null;

            try
            {
                var newSnapshot = await ApplyOperationOnServerAsync(operation, cancellationToken);

                lock (SyncRoot)
                {
                    _snapshot = newSnapshot;
                }

                SafeRaiseBalanceChanged();
                SafeRaiseOperationCompleted(true, null);

                success = true;
                Log($"{LogPrefix} [Op:{opId}] Success: {operation}. SnapshotAfter={_snapshot}");
                return true;
            }
            catch (OperationCanceledException)
            {
                errorMsg = "Wallet operation cancelled.";
                Log($"{LogPrefix} [Op:{opId}] ApplyOperationAsync cancelled: {operation}.", isWarning: true);
                SafeRaiseOperationCompleted(false, errorMsg);
                return false;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log($"{LogPrefix} [Op:{opId}] ApplyOperationAsync failed: {operation}. Error={ex.Message}", isError: true);
                Debug.LogException(ex);
                SafeRaiseOperationCompleted(false, errorMsg);
                return false;
            }
            finally
            {
                var remaining = Interlocked.Decrement(ref _activeOperationsCount);
                Log($"{LogPrefix} [Op:{opId}] End operation. Success={success}, Error={errorMsg ?? "null"}, " +
                    $"ActiveOpsNow={remaining}");
            }
        }

        #endregion

        #region Helpers

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (SyncRoot)
            {
                if (_initialized) return;
                _snapshot = new WalletSnapshot(0, 0);
                _initialized = true;
            }

            Log($"{LogPrefix} Used before explicit Initialize(). " +
                "Initialized with zero balances.", isWarning: true);
        }

        private static bool HasSufficientBalance(WalletKind wallet, long amount)
        {
            if (amount < 0) amount = Math.Abs(amount);

            switch (wallet)
            {
                case WalletKind.Game:
                    return GameBalance >= amount;
                case WalletKind.Global:
                    return GlobalBalance >= amount;
                default:
                    return false;
            }
        }

        private static void SafeRaiseBalanceChanged()
        {
            try
            {
                OnWalletBalanceChanged?.Invoke(_snapshot.GameBalance, _snapshot.GlobalBalance);
            }
            catch (Exception ex)
            {
                Log($"{LogPrefix} Exception in OnWalletBalanceChanged subscriber: {ex.Message}", isError: true);
                Debug.LogException(ex);
            }
        }

        private static void SafeRaiseOperationCompleted(bool success, string error)
        {
            try
            {
                OnWalletOperationCompleted?.Invoke(success, error);
            }
            catch (Exception ex)
            {
                Log($"{LogPrefix} Exception in OnWalletOperationCompleted subscriber: {ex.Message}", isError: true);
                Debug.LogException(ex);
            }
        }

        private static void Log(string message, bool isWarning = false, bool isError = false)
        {
            if (!EnableDebugLogs && !isError) return;

            if (isError)
                Debug.LogError(message);
            else if (isWarning)
                Debug.LogWarning(message);
            else
                Debug.Log(message);
        }

        #endregion
    }
}

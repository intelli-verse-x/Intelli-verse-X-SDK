using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Backend
{
    /// <summary>
    /// Wallet manager for IntelliVerse-X SDK.
    /// Handles dual-wallet system (game + global wallets).
    /// 
    /// Usage:
    ///   int gameBalance = IVXWalletManager.GetGameBalance();
    ///   int globalBalance = IVXWalletManager.GetGlobalBalance();
    ///   
    ///   await IVXWalletManager.RefreshWalletsAsync();
    ///   
    ///   bool canBuy = IVXWalletManager.CanAfford(100); // Check game wallet
    ///   bool canBuyGlobal = IVXWalletManager.CanAfford(50, useGlobal: true);
    /// </summary>
    public static class IVXWalletManager
    {
        // Events
        public static event Action<int, int> OnBalanceChanged; // gameBalance, globalBalance
        public static event Action<string> OnWalletError;

        /// <summary>
        /// Get cached game wallet balance (no network call)
        /// </summary>
        public static int GetGameBalance()
        {
            return Core.IntelliVerseXIdentity.GameWalletBalance;
        }

        /// <summary>
        /// Get cached global wallet balance (no network call)
        /// </summary>
        public static int GetGlobalBalance()
        {
            return Core.IntelliVerseXIdentity.GlobalWalletBalance;
        }

        /// <summary>
        /// Get game wallet ID
        /// </summary>
        public static string GetGameWalletId()
        {
            return Core.IntelliVerseXIdentity.GameWalletId;
        }

        /// <summary>
        /// Get global wallet ID
        /// </summary>
        public static string GetGlobalWalletId()
        {
            return Core.IntelliVerseXIdentity.GlobalWalletId;
        }

        /// <summary>
        /// Check if user has both wallet IDs assigned
        /// </summary>
        public static bool HasWalletIds()
        {
            return Core.IntelliVerseXIdentity.HasWalletIds;
        }

        /// <summary>
        /// Check if user can afford a purchase
        /// </summary>
        /// <param name="cost">Cost of item</param>
        /// <param name="useGlobal">Use global wallet instead of game wallet</param>
        public static bool CanAfford(int cost, bool useGlobal = false)
        {
            int balance = useGlobal ? GetGlobalBalance() : GetGameBalance();
            return balance >= cost;
        }

        /// <summary>
        /// Add coins to game wallet (local update, sync to server separately)
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <param name="source">Source of coins (for tracking)</param>
        public static void AddCoins(int amount, string source = "unknown")
        {
            int currentBalance = GetGameBalance();
            int newBalance = currentBalance + amount;
            UpdateBalances(newBalance, GetGlobalBalance());
            Debug.Log($"[IVXWalletManager] Added {amount} coins from {source}. New balance: {newBalance}");
        }

        /// <summary>
        /// Refresh wallet balances from server
        /// Calls create_or_get_wallet RPC for both wallets
        /// </summary>
        public static async Task<bool> RefreshWalletsAsync()
        {
            try
            {
                if (!HasWalletIds())
                {
                    Debug.LogWarning("[IVXWalletManager] No wallet IDs found. Call create_or_sync_user first.");
                    return false;
                }

                // This will be implemented when IVXNakamaClient is ready
                // For now, return cached values
                await Task.CompletedTask;
                Debug.Log($"[IVXWalletManager] Wallet balances - Game: {GetGameBalance()}, Global: {GetGlobalBalance()}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXWalletManager] Failed to refresh wallets: {ex.Message}");
                OnWalletError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Internal: Update wallet balances (called by Nakama client)
        /// </summary>
        /// <summary>
        /// Update wallet balance UI
        /// </summary>
        public static void UpdateBalances(int gameBalance, int globalBalance, string gameCurrency = "coins", string globalCurrency = "gems")
        {
            Core.IntelliVerseXIdentity.UpdateWalletBalances(gameBalance, globalBalance, gameCurrency, globalCurrency);
            OnBalanceChanged?.Invoke(gameBalance, globalBalance);
        }

        /// <summary>
        /// Internal: Set wallet IDs (called by Nakama client after create_or_sync_user)
        /// </summary>
        internal static void SetWalletIds(string gameWalletId, string globalWalletId)
        {
            Core.IntelliVerseXIdentity.SetWalletIds(gameWalletId, globalWalletId);
        }
    }
}

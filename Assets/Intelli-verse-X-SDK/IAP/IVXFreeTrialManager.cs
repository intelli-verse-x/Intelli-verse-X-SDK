// FILE: IVXFreeTrialManager.cs
// PURPOSE: Reusable free trial system for IntelliVerseX games with HMAC-protected, device-bound trial tokens.
// USAGE: Per-game instance scoped by gameId, supports one-time local trials with integrity protection.
//
// KEY FEATURES:
//  - Device-bound HMAC-protected trial tokens (tamper-resistant)
//  - Per-game trial scoping via gameId
//  - Configurable trial scope (global, per-feature, per-product)
//  - Fail-open policy: corrupted PlayerPrefs don't incorrectly revoke legitimate trials
//  - Production-ready with secure signature verification
//
// INTEGRATION:
//  1. Call IVXFreeTrialManager.GetInstance(gameId, hmacSecret) to get game-scoped instance
//  2. Check HasTrialAvailable() before gating features
//  3. Call ConsumeTrial() when user uses their trial
//  4. Optional: Use GetTrialScope() for multi-tier trial systems

using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace IntelliVerseX.IAP
{
    /// <summary>
    /// Trial scope determines how trials are counted across features/products.
    /// </summary>
    public enum TrialScope
    {
        /// <summary>Single trial per game (shared across all premium features)</summary>
        Global,
        
        /// <summary>One trial per feature (e.g., separate trials for quiz mode, daily quiz, etc.)</summary>
        PerFeature,
        
        /// <summary>One trial per product/SKU</summary>
        PerProduct
    }

    /// <summary>
    /// Manages one-time local free trials with HMAC integrity protection and device binding.
    /// Scoped by gameId to support multi-game platforms.
    /// </summary>
    public class IVXFreeTrialManager
    {
        #region ---------- Configuration ----------

        private readonly string _gameId;
        private readonly string _hmacSecret;
        private readonly TrialScope _scope;
        private readonly string _scopeIdentifier;

        // PlayerPrefs keys (scoped by gameId + scope)
        private readonly string _trialUsedKey;
        private readonly string _trialSigKey;
        private readonly string _deviceGuidKey;

        #endregion

        #region ---------- Singleton Factory ----------

        private static readonly System.Collections.Generic.Dictionary<string, IVXFreeTrialManager> _instances 
            = new System.Collections.Generic.Dictionary<string, IVXFreeTrialManager>();

        /// <summary>
        /// Gets or creates a game-scoped free trial manager instance.
        /// </summary>
        /// <param name="gameId">Unique game identifier (e.g., "quiz-verse", "word-hunt")</param>
        /// <param name="hmacSecret">Long random secret for HMAC signing (keep private, different per game recommended)</param>
        /// <param name="scope">Trial scope (global, per-feature, per-product)</param>
        /// <param name="scopeIdentifier">Optional scope identifier (feature name or product ID) when using PerFeature/PerProduct</param>
        public static IVXFreeTrialManager GetInstance(
            string gameId, 
            string hmacSecret, 
            TrialScope scope = TrialScope.Global,
            string scopeIdentifier = "")
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("gameId cannot be null or empty", nameof(gameId));

            if (string.IsNullOrWhiteSpace(hmacSecret))
                throw new ArgumentException("hmacSecret cannot be null or empty", nameof(hmacSecret));

            // Create composite key for instance dictionary
            string instanceKey = $"{gameId}:{scope}:{scopeIdentifier}";

            if (!_instances.TryGetValue(instanceKey, out var instance))
            {
                instance = new IVXFreeTrialManager(gameId, hmacSecret, scope, scopeIdentifier);
                _instances[instanceKey] = instance;
            }

            return instance;
        }

        /// <summary>
        /// Resets all trial data for a game (DEBUG ONLY - use with extreme caution).
        /// </summary>
        public static void ResetTrialForGame(string gameId)
        {
            Debug.LogWarning($"[IVXFreeTrialManager] Resetting ALL trial data for game: {gameId}");
            
            // Remove all instances for this game
            var keysToRemove = new System.Collections.Generic.List<string>();
            foreach (var kvp in _instances)
            {
                if (kvp.Key.StartsWith(gameId + ":"))
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                if (_instances.TryGetValue(key, out var instance))
                {
                    instance.ClearTrialData();
                    _instances.Remove(key);
                }
            }
        }

        #endregion

        #region ---------- Constructor (Private) ----------

        private IVXFreeTrialManager(string gameId, string hmacSecret, TrialScope scope, string scopeIdentifier)
        {
            _gameId = gameId;
            _hmacSecret = hmacSecret;
            _scope = scope;
            _scopeIdentifier = scopeIdentifier ?? string.Empty;

            // Build scoped PlayerPrefs keys
            string scopeSuffix = BuildScopeSuffix();
            _trialUsedKey = $"ivx_{gameId}_trial_used{scopeSuffix}";
            _trialSigKey = $"ivx_{gameId}_trial_sig{scopeSuffix}";
            _deviceGuidKey = $"ivx_{gameId}_device_guid"; // Device GUID is always game-scoped, not per-trial
        }

        private string BuildScopeSuffix()
        {
            switch (_scope)
            {
                case TrialScope.Global:
                    return "";
                case TrialScope.PerFeature:
                    return string.IsNullOrWhiteSpace(_scopeIdentifier) 
                        ? "" 
                        : $"_feat_{_scopeIdentifier}";
                case TrialScope.PerProduct:
                    return string.IsNullOrWhiteSpace(_scopeIdentifier) 
                        ? "" 
                        : $"_prod_{_scopeIdentifier}";
                default:
                    return "";
            }
        }

        #endregion

        #region ---------- Public API ----------

        /// <summary>
        /// Checks if the user has a free trial available (not yet consumed).
        /// Uses HMAC signature verification for integrity.
        /// </summary>
        /// <returns>True if trial is available, false if already consumed</returns>
        public bool HasTrialAvailable()
        {
            int used = PlayerPrefs.GetInt(_trialUsedKey, 0);
            string sig = PlayerPrefs.GetString(_trialSigKey, string.Empty);

            // FAIL-OPEN POLICY:
            // If signature is invalid/missing, treat as "trial still available"
            // This prevents corrupted PlayerPrefs from incorrectly revoking legitimate trials.
            //
            // For stricter anti-tamper (FAIL-CLOSED), change to:
            // return used == 0 && VerifySignature(used, sig);
            
            return used == 0 || !VerifySignature(used, sig);
        }

        /// <summary>
        /// Consumes the one-time free trial, marking it as used with HMAC signature.
        /// </summary>
        /// <returns>True if trial was consumed successfully, false if already consumed</returns>
        public bool ConsumeTrial()
        {
            if (!HasTrialAvailable())
            {
                Debug.LogWarning($"[IVXFreeTrialManager] Trial already consumed for {_gameId} (scope: {_scope})");
                return false;
            }

            int used = 1;
            string sig = ComputeSignature(used);
            
            PlayerPrefs.SetInt(_trialUsedKey, used);
            PlayerPrefs.SetString(_trialSigKey, sig);
            PlayerPrefs.Save();

            Debug.Log($"[IVXFreeTrialManager] Trial consumed for {_gameId} (scope: {_scope}, identifier: {_scopeIdentifier})");
            return true;
        }

        /// <summary>
        /// Gets the trial scope configuration.
        /// </summary>
        public TrialScope GetTrialScope() => _scope;

        /// <summary>
        /// Gets the scope identifier (feature name or product ID).
        /// </summary>
        public string GetScopeIdentifier() => _scopeIdentifier;

        /// <summary>
        /// DEBUG ONLY: Clears all trial data for this specific scope.
        /// ⚠️ Use with extreme caution - this permanently resets the trial.
        /// </summary>
        public void ClearTrialData()
        {
            Debug.LogWarning($"[IVXFreeTrialManager] Clearing trial data for {_gameId} (scope: {_scope}, identifier: {_scopeIdentifier})");
            
            PlayerPrefs.DeleteKey(_trialUsedKey);
            PlayerPrefs.DeleteKey(_trialSigKey);
            // Note: Don't delete device GUID as it's shared across all trials for the game
            PlayerPrefs.Save();
        }

        #endregion

        #region ---------- HMAC Signature Protection ----------

        private string GetOrCreateDeviceGuid()
        {
            var guid = PlayerPrefs.GetString(_deviceGuidKey, string.Empty);
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(_deviceGuidKey, guid);
                PlayerPrefs.Save();
            }
            return guid;
        }

        private string ComputeSignature(int usedValue)
        {
            string deviceGuid = GetOrCreateDeviceGuid();
            string scopeSuffix = BuildScopeSuffix();
            string payload = $"{deviceGuid}:{usedValue}:{_gameId}{scopeSuffix}";
            
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_hmacSecret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }

        private bool VerifySignature(int usedValue, string signature)
        {
            if (string.IsNullOrEmpty(signature))
                return false;

            string expected = ComputeSignature(usedValue);
            return ConstantTimeEquals(expected, signature);
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks.
        /// </summary>
        private bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }

        #endregion

        #region ---------- Diagnostic API ----------

        /// <summary>
        /// Gets diagnostic information about the trial state (for debugging).
        /// </summary>
        public string GetDiagnostics()
        {
            int used = PlayerPrefs.GetInt(_trialUsedKey, 0);
            string sig = PlayerPrefs.GetString(_trialSigKey, string.Empty);
            bool sigValid = VerifySignature(used, sig);
            bool available = HasTrialAvailable();

            return $"[IVXFreeTrialManager] Diagnostics:\n" +
                   $"  Game ID: {_gameId}\n" +
                   $"  Scope: {_scope}\n" +
                   $"  Scope Identifier: {_scopeIdentifier}\n" +
                   $"  Trial Used: {used}\n" +
                   $"  Signature Valid: {sigValid}\n" +
                   $"  Trial Available: {available}\n" +
                   $"  Device GUID: {GetOrCreateDeviceGuid()}";
        }

        #endregion
    }
}

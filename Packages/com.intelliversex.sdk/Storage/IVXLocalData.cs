using System;
using System.IO;
using UnityEngine;

namespace IntelliVerseX.Storage
{
    /// <summary>
    /// Single entry point for SDK local data: remember-me prefs, key wipe, and plaintext→secure migration.
    /// Auth session JSON is owned by <c>UserSessionManager</c> / <c>IVXUserSession</c> on top of
    /// <see cref="IVXSecureStorage"/>.
    /// </summary>
    public static class IVXLocalData
    {
        private static bool _migrated;

        /// <summary>
        /// Ensures secure storage is initialized and migrates known plaintext keys once per process.
        /// </summary>
        public static void EnsureInitialized()
        {
            IVXSecureStorage.Initialize();
            if (_migrated)
                return;
            _migrated = true;
            MigratePlaintextKeys();
            MigrateRememberMeFlags();
        }

        /// <summary>Whether the user asked to persist login (remember me). Default true.</summary>
        public static bool GetRememberMe(bool defaultValue = true)
        {
            EnsureInitialized();
            if (IVXSecureStorage.HasKey(IVXLocalDataKeys.RememberMe))
                return IVXSecureStorage.GetBool(IVXLocalDataKeys.RememberMe, defaultValue);
            if (PlayerPrefs.HasKey(IVXLocalDataKeys.RememberMeLegacy))
                return PlayerPrefs.GetInt(IVXLocalDataKeys.RememberMeLegacy, defaultValue ? 1 : 0) == 1;
            return defaultValue;
        }

        public static void SetRememberMe(bool remember)
        {
            EnsureInitialized();
            IVXSecureStorage.SetBool(IVXLocalDataKeys.RememberMe, remember);
            // Keep legacy int in sync for older builds reading PlayerPrefs directly.
            PlayerPrefs.SetInt(IVXLocalDataKeys.RememberMeLegacy, remember ? 1 : 0);
            PlayerPrefs.Save();

            if (!remember)
                ClearRememberedEmail();
        }

        public static string GetLastEmail()
        {
            EnsureInitialized();
            string email = IVXSecureStorage.GetString(IVXLocalDataKeys.LastEmail, "");
            if (!string.IsNullOrWhiteSpace(email))
                return email.Trim();

            // Legacy plaintext / previously-migrated keys — always decrypt via secure storage.
            if (PlayerPrefs.HasKey(IVXLocalDataKeys.SavedEmailLegacy))
            {
                email = GetSecureOrMigrate(IVXLocalDataKeys.SavedEmailLegacy, "");
                if (!string.IsNullOrWhiteSpace(email))
                {
                    IVXSecureStorage.SetString(IVXLocalDataKeys.LastEmail, email.Trim());
                    PlayerPrefs.DeleteKey(IVXLocalDataKeys.SavedEmailLegacy);
                    PlayerPrefs.Save();
                    return email.Trim();
                }
            }

            return "";
        }

        public static void SetLastEmail(string email)
        {
            EnsureInitialized();
            string trimmed = (email ?? "").Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                ClearRememberedEmail();
                return;
            }

            IVXSecureStorage.SetString(IVXLocalDataKeys.LastEmail, trimmed);
            // Drop legacy plaintext duplicate only — do not delete LastEmail (that is the secure slot).
            PlayerPrefs.DeleteKey(IVXLocalDataKeys.SavedEmailLegacy);
            PlayerPrefs.Save();
        }

        public static void ClearRememberedEmail()
        {
            IVXSecureStorage.DeleteKey(IVXLocalDataKeys.LastEmail);
            PlayerPrefs.DeleteKey(IVXLocalDataKeys.SavedEmailLegacy);
            PlayerPrefs.Save();
        }

        public static void SetPersistFlag(bool persisted)
        {
            EnsureInitialized();
            IVXSecureStorage.SetBool(IVXLocalDataKeys.PersistFlag, persisted);
        }

        public static bool GetPersistFlag()
        {
            EnsureInitialized();
            return IVXSecureStorage.GetBool(IVXLocalDataKeys.PersistFlag, false);
        }

        public static void SetAuthUserHint(string userId, string loginType)
        {
            EnsureInitialized();
            if (!string.IsNullOrWhiteSpace(userId))
                IVXSecureStorage.SetString(IVXLocalDataKeys.AuthUserId, userId.Trim());
            if (!string.IsNullOrWhiteSpace(loginType))
                IVXSecureStorage.SetString(IVXLocalDataKeys.AuthLoginType, loginType.Trim());
        }

        /// <summary>
        /// Deletes auth/session/token keys. Keeps device id, game id, and remember-me email unless requested.
        /// </summary>
        public static void ClearAuthRelatedKeys(bool clearRememberMe = false)
        {
            EnsureInitialized();
            DeleteKeys(IVXLocalDataKeys.AuthSessionKeys);
            if (clearRememberMe)
                DeleteKeys(IVXLocalDataKeys.RememberMeKeys);
        }

        /// <summary>
        /// Deletes every registered SDK local key (dev wipe / GDPR). Does not call PlayerPrefs.DeleteAll().
        /// </summary>
        public static void ClearAllRegisteredKeys()
        {
            EnsureInitialized();
            DeleteKeys(IVXLocalDataKeys.AllSdkKeys);
            // Also scrub any leftover plaintext copies.
            foreach (string key in IVXLocalDataKeys.PlaintextMigrationKeys)
            {
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
        }

        public static void DeleteKeys(params string[] keys)
        {
            if (keys == null)
                return;
            foreach (string key in keys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;
                IVXSecureStorage.DeleteKey(key);
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
        }

        public static void DeleteLegacySessionFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXLocalData] Could not delete legacy session file: {ex.Message}");
            }
        }

        /// <summary>Read a string that may still be plaintext PlayerPrefs; migrate on read.</summary>
        public static string GetSecureOrMigrate(string key, string defaultValue = "")
        {
            EnsureInitialized();
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;

            // Prefer decrypt path (handles already-encrypted values).
            string value = IVXSecureStorage.GetString(key, defaultValue);
            if (!string.IsNullOrEmpty(value) && value != defaultValue)
                return value;

            // If GetString returned default because decrypt failed / empty, try one plaintext migrate.
            string plain = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(plain))
                return defaultValue;

            // MigrateFromPlayerPrefs only rewrites when value is not Base64; GetString already tried decrypt.
            IVXSecureStorage.MigrateFromPlayerPrefs(key);
            return IVXSecureStorage.GetString(key, defaultValue);
        }

        public static void SetSecureString(string key, string value)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(key))
                return;

            if (string.IsNullOrEmpty(value))
            {
                IVXSecureStorage.DeleteKey(key);
                return;
            }

            // SetString writes the encrypted value into PlayerPrefs under the same key — do not DeleteKey after.
            IVXSecureStorage.SetString(key, value);
        }

        private static void MigratePlaintextKeys()
        {
            foreach (string key in IVXLocalDataKeys.PlaintextMigrationKeys)
                IVXSecureStorage.MigrateFromPlayerPrefs(key);
        }

        private static void MigrateRememberMeFlags()
        {
            if (!IVXSecureStorage.HasKey(IVXLocalDataKeys.RememberMe)
                && PlayerPrefs.HasKey(IVXLocalDataKeys.RememberMeLegacy))
            {
                bool on = PlayerPrefs.GetInt(IVXLocalDataKeys.RememberMeLegacy, 1) == 1;
                IVXSecureStorage.SetBool(IVXLocalDataKeys.RememberMe, on);
            }

            // Promote legacy saved-email into the canonical secure LastEmail slot.
            if (string.IsNullOrEmpty(IVXSecureStorage.GetString(IVXLocalDataKeys.LastEmail, ""))
                && PlayerPrefs.HasKey(IVXLocalDataKeys.SavedEmailLegacy))
            {
                IVXSecureStorage.MigrateFromPlayerPrefs(IVXLocalDataKeys.SavedEmailLegacy);
                string email = IVXSecureStorage.GetString(IVXLocalDataKeys.SavedEmailLegacy, "");
                if (!string.IsNullOrWhiteSpace(email))
                {
                    IVXSecureStorage.SetString(IVXLocalDataKeys.LastEmail, email.Trim());
                    PlayerPrefs.DeleteKey(IVXLocalDataKeys.SavedEmailLegacy);
                    PlayerPrefs.Save();
                }
            }
            else if (PlayerPrefs.HasKey(IVXLocalDataKeys.LastEmail))
            {
                // In-place migrate if LastEmail is still plaintext.
                IVXSecureStorage.MigrateFromPlayerPrefs(IVXLocalDataKeys.LastEmail);
            }
        }
    }
}

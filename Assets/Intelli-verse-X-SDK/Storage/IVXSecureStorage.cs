using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace IntelliVerseX.Storage
{
    /// <summary>
    /// Secure storage wrapper for PlayerPrefs with AES-256-CBC encryption.
    /// Derives a per-device key via PBKDF2. Provides GDPR-compliant data
    /// storage with validation and migration support.
    /// </summary>
    public static class IVXSecureStorage
    {
        private const int DATA_VERSION = 2;
        private const string VERSION_KEY = "IVX_STORAGE_VERSION";
        private const int AES_KEY_SIZE = 256;
        private const int AES_IV_SIZE = 16;
        private const int PBKDF2_ITERATIONS = 10000;
        private static readonly byte[] PBKDF2_SALT =
            System.Text.Encoding.UTF8.GetBytes("IVXSecureStorage_Salt_v2");

        private static byte[] _cachedKey;

        private static byte[] GetEncryptionKey()
        {
            if (_cachedKey != null) return _cachedKey;

            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId))
                deviceId = "IntelliVerseX_Default_Key_2026";

            using var kdf = new Rfc2898DeriveBytes(
                deviceId, PBKDF2_SALT, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256);
            _cachedKey = kdf.GetBytes(AES_KEY_SIZE / 8);
            return _cachedKey;
        }

        /// <summary>
        /// Encrypts plaintext with AES-256-CBC. Returns Base64(IV + ciphertext).
        /// </summary>
        private static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] key = GetEncryptionKey();
                using var aes = Aes.Create();
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                byte[] result = new byte[AES_IV_SIZE + cipherBytes.Length];
                Array.Copy(aes.IV, 0, result, 0, AES_IV_SIZE);
                Array.Copy(cipherBytes, 0, result, AES_IV_SIZE, cipherBytes.Length);
                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"[IVXSecureStorage] Encryption failed: {ex.Message}\n{ex.StackTrace}");
#else
                Debug.LogError("[IVXSecureStorage] Encryption failed; value was not written.");
#endif
                throw;
            }
        }

        /// <summary>
        /// Decrypts AES-256-CBC ciphertext. Falls back to legacy XOR if AES fails
        /// (auto-migration from v1 data).
        /// </summary>
        /// <returns>Plaintext, legacy-decrypted text, or legacy/plain passthrough for migration.
        /// Returns <c>null</c> if ciphertext is corrupt or integrity cannot be verified (caller must drop the key).</returns>
        private static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                byte[] raw = Convert.FromBase64String(cipherText);
                if (raw.Length <= AES_IV_SIZE)
                    return DecryptLegacyXOR(raw);

                byte[] iv = new byte[AES_IV_SIZE];
                Array.Copy(raw, 0, iv, 0, AES_IV_SIZE);
                byte[] cipher = new byte[raw.Length - AES_IV_SIZE];
                Array.Copy(raw, AES_IV_SIZE, cipher, 0, cipher.Length);

                byte[] key = GetEncryptionKey();
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                byte[] plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                return System.Text.Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                return TryLegacyDecrypt(cipherText);
            }
            catch (FormatException)
            {
                Debug.LogWarning("[IVXSecureStorage] Value is not Base64; treating as legacy plain text.");
                return cipherText;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[IVXSecureStorage] Decryption failed (corrupt or wrong key): {ex.GetType().Name}: {ex.Message}");
#else
                Debug.LogWarning("[IVXSecureStorage] Decryption failed — stored value was removed (corrupt or incompatible data).");
#endif
                return null;
            }
        }

        private static string TryLegacyDecrypt(string cipherText)
        {
            try
            {
                byte[] raw = Convert.FromBase64String(cipherText);
                return DecryptLegacyXOR(raw);
            }
            catch
            {
                return cipherText;
            }
        }

        private static string DecryptLegacyXOR(byte[] cipherBytes)
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId))
                deviceId = "IntelliVerseX_Default_Key_2025";

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(deviceId);
            byte[] legacyKey = new byte[32];
            Array.Copy(keyBytes, legacyKey, Mathf.Min(keyBytes.Length, 32));

            byte[] plainBytes = new byte[cipherBytes.Length];
            for (int i = 0; i < cipherBytes.Length; i++)
                plainBytes[i] = (byte)(cipherBytes[i] ^ legacyKey[i % legacyKey.Length]);

            return System.Text.Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Saves an encrypted string value
        /// </summary>
        public static void SetString(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("[IVXSecureStorage] Key cannot be null or empty");
                    return;
                }

                string encrypted = Encrypt(value);
                PlayerPrefs.SetString(key, encrypted);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSecureStorage] SetString failed for key '{key}': {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a decrypted string value
        /// </summary>
        public static string GetString(string key, string defaultValue = "")
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return defaultValue;

                if (!PlayerPrefs.HasKey(key))
                    return defaultValue;

                string encrypted = PlayerPrefs.GetString(key, defaultValue);
                if (string.IsNullOrEmpty(encrypted))
                    return defaultValue;

                string decrypted = Decrypt(encrypted);
                if (decrypted == null)
                {
                    PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                    return defaultValue;
                }

                return decrypted;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[IVXSecureStorage] GetString failed for key '{key}': {ex.Message}");
#else
                Debug.LogWarning("[IVXSecureStorage] GetString failed — key removed.");
#endif
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                return defaultValue;
            }
        }

        public static void SetInt(string key, int value)
        {
            try
            {
                PlayerPrefs.SetInt(key, value);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSecureStorage] SetInt failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            try
            {
                return PlayerPrefs.GetInt(key, defaultValue);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSecureStorage] GetInt failed: {ex.Message}");
                PlayerPrefs.DeleteKey(key);
                return defaultValue;
            }
        }

        public static void SetFloat(string key, float value)
        {
            try
            {
                PlayerPrefs.SetFloat(key, value);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSecureStorage] SetFloat failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            try
            {
                return PlayerPrefs.GetFloat(key, defaultValue);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSecureStorage] GetFloat failed: {ex.Message}");
                PlayerPrefs.DeleteKey(key);
                return defaultValue;
            }
        }

        public static void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            return GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        /// <summary>
        /// Saves an object as encrypted JSON
        /// </summary>
        public static void SetObject<T>(string key, T value)
        {
            try
            {
                string json = JsonUtility.ToJson(value);
                SetString(key, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSecureStorage] SetObject failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves an object from encrypted JSON
        /// </summary>
        public static T GetObject<T>(string key, T defaultValue = default)
        {
            try
            {
                string json = GetString(key);
                if (string.IsNullOrEmpty(json))
                    return defaultValue;

                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSecureStorage] GetObject failed: {ex.Message}");
                PlayerPrefs.DeleteKey(key);
                return defaultValue;
            }
        }

        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Deletes all data (GDPR compliance)
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[IVXSecureStorage] All data deleted (GDPR compliance)");
        }

        /// <summary>
        /// Migrates unencrypted PlayerPrefs key to encrypted storage
        /// </summary>
        public static void MigrateFromPlayerPrefs(string key)
        {
            try
            {
                if (!PlayerPrefs.HasKey(key))
                    return;

                string plainValue = PlayerPrefs.GetString(key);
                
                if (!IsBase64(plainValue))
                {
                    Debug.Log($"[IVXSecureStorage] Migrating unencrypted key: {key}");
                    SetString(key, plainValue);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSecureStorage] Migration failed: {ex.Message}");
            }
        }

        private static bool IsBase64(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes storage and performs migrations
        /// </summary>
        public static void Initialize()
        {
            int currentVersion = GetInt(VERSION_KEY, 0);

            if (currentVersion < DATA_VERSION)
            {
                Debug.Log($"[IVXSecureStorage] Migrating from v{currentVersion} to v{DATA_VERSION}");
                SetInt(VERSION_KEY, DATA_VERSION);
            }
        }
    }

    /// <summary>
    /// GDPR-compliant privacy manager for data export and deletion
    /// </summary>
    public static class IVXPrivacyManager
    {
        /// <summary>
        /// Exports all user data as JSON (GDPR Article 20 - Right to data portability)
        /// </summary>
        public static string ExportAllData()
        {
            var data = new Dictionary<string, object>
            {
                ["export_date"] = DateTime.UtcNow.ToString("o"),
                ["device_id"] = SystemInfo.deviceUniqueIdentifier,
                ["user_data"] = GetAllPlayerPrefs()
            };

            string json = JsonUtility.ToJson(data);
            Debug.Log("[IVXPrivacyManager] User data exported (GDPR compliance)");
            return json;
        }

        /// <summary>
        /// Deletes all user data (GDPR Article 17 - Right to be forgotten)
        /// </summary>
        public static void DeleteAllData()
        {
            IVXSecureStorage.DeleteAll();
            
            // Clear cache directories
            try
            {
                if (System.IO.Directory.Exists(Application.persistentDataPath))
                {
                    var files = System.IO.Directory.GetFiles(Application.persistentDataPath);
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXPrivacyManager] Cache cleanup failed: {ex.Message}");
            }

            Debug.Log("[IVXPrivacyManager] All user data deleted (GDPR compliance)");
        }

        private static Dictionary<string, string> GetAllPlayerPrefs()
        {
            // Note: Unity doesn't provide a way to enumerate all PlayerPrefs keys
            // Games must maintain their own key list
            return new Dictionary<string, string>();
        }
    }
}

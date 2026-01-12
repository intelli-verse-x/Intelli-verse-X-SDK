// File: IVXLocalizationService.cs
// Purpose: Core localization service with provider pattern for multi-language support
// Package: IntelliVerseX.Localization
// Dependencies: IntelliVerseX.Core

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using IntelliVerseX.Core;

namespace IntelliVerseX.Localization
{
    /// <summary>
    /// Supported language codes (12 languages matching QuizVerse)
    /// </summary>
    public enum IVXLanguageCode
    {
        Spanish_LatAm,  // es-419
        Arabic,         // ar
        Chinese,        // zh-CN
        English,        // en
        French,         // fr
        German,         // de
        Hindi,          // hi
        Indonesian,     // id
        Japanese,       // ja
        Korean,         // ko
        Portuguese,     // pt
        Russian,        // ru
        Spanish_Spain,  // es
        Zulu            // zu (deprecated but kept for compatibility)
    }

    /// <summary>
    /// Localization provider interface for loading translations
    /// </summary>
    public interface IIVXLocalizationProvider
    {
        Task<bool> LoadLanguageAsync(string languageCode);
        string GetString(string key, string defaultValue = "");
        bool HasKey(string key);
        List<string> GetSupportedLanguages();
    }

    /// <summary>
    /// Localization service result wrapper
    /// </summary>
    public class IVXLocalizationResult
    {
        public bool Success { get; set; }
        public string Value { get; set; }
        public string ErrorMessage { get; set; }

        public static IVXLocalizationResult Ok(string value) => new IVXLocalizationResult { Success = true, Value = value };
        public static IVXLocalizationResult Error(string error) => new IVXLocalizationResult { Success = false, ErrorMessage = error };
    }

    /// <summary>
    /// Core localization service with provider pattern.
    /// Replaces QuizVerse LocalizationManager with SDK version.
    /// 
    /// Usage:
    ///   IVXLocalizationService.Instance.SetProvider(new IVXCSVLocalizationProvider());
    ///   await IVXLocalizationService.Instance.SetLanguageAsync("en");
    ///   string text = IVXLocalizationService.Instance.GetString("Button_Play");
    /// </summary>
    public class IVXLocalizationService : IVXSafeSingleton<IVXLocalizationService>
    {
        #region Constants

        private const string LANGUAGE_PREF_KEY = "IVX_SelectedLanguage";
        private const string DEFAULT_LANGUAGE_CODE = "en";

        // Language code mappings (enum to ISO code)
        private static readonly Dictionary<IVXLanguageCode, string> LanguageCodeMap = new Dictionary<IVXLanguageCode, string>
        {
            { IVXLanguageCode.Spanish_LatAm, "es-419" },
            { IVXLanguageCode.Arabic, "ar" },
            { IVXLanguageCode.Chinese, "zh-CN" },
            { IVXLanguageCode.English, "en" },
            { IVXLanguageCode.French, "fr" },
            { IVXLanguageCode.German, "de" },
            { IVXLanguageCode.Hindi, "hi" },
            { IVXLanguageCode.Indonesian, "id" },
            { IVXLanguageCode.Japanese, "ja" },
            { IVXLanguageCode.Korean, "ko" },
            { IVXLanguageCode.Portuguese, "pt" },
            { IVXLanguageCode.Russian, "ru" },
            { IVXLanguageCode.Spanish_Spain, "es" },
            { IVXLanguageCode.Zulu, "zu" }
        };

        #endregion

        #region Events

        /// <summary>
        /// Fired when the current language changes
        /// </summary>
        public static event Action<string> OnLanguageChanged;

        #endregion

        #region Private Fields

        private IIVXLocalizationProvider _provider;
        private string _currentLanguageCode = DEFAULT_LANGUAGE_CODE;
        private bool _isInitialized = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current language code (e.g., "en", "es-419", "ar")
        /// </summary>
        public string CurrentLanguage => _currentLanguageCode;

        /// <summary>
        /// True if localization is initialized and ready
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// List of supported language codes
        /// </summary>
        public List<string> SupportedLanguages => _provider?.GetSupportedLanguages() ?? new List<string>();

        /// <summary>
        /// Alias for SupportedLanguages (adapter compatibility)
        /// </summary>
        public List<string> AvailableLanguages => SupportedLanguages;

        #endregion

        #region Initialization

        /// <summary>
        /// Set the localization provider (CSV, JSON, Unity Localization, etc.)
        /// </summary>
        public void SetProvider(IIVXLocalizationProvider provider)
        {
            if (provider == null)
            {
                Debug.LogError("[IVXLocalizationService] Provider cannot be null");
                return;
            }

            _provider = provider;
            Debug.Log($"[IVXLocalizationService] Provider set: {provider.GetType().Name}");
        }

        /// <summary>
        /// Initialize with auto-detected device language
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (_provider == null)
            {
                Debug.LogError("[IVXLocalizationService] No provider set. Call SetProvider() first.");
                return false;
            }

            // Load saved language or auto-detect
            string savedLanguage = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, "");
            string languageToLoad = string.IsNullOrEmpty(savedLanguage) 
                ? IVXLanguageDetector.DetectDeviceLanguage() 
                : savedLanguage;

            bool success = await SetLanguageAsync(languageToLoad);
            if (success)
            {
                _isInitialized = true;
                Debug.Log($"[IVXLocalizationService] Initialized with language: {_currentLanguageCode}");
            }
            else
            {
                Debug.LogWarning($"[IVXLocalizationService] Failed to load {languageToLoad}, falling back to {DEFAULT_LANGUAGE_CODE}");
                success = await SetLanguageAsync(DEFAULT_LANGUAGE_CODE);
                _isInitialized = success;
            }

            return _isInitialized;
        }

        #endregion

        #region Language Management

        /// <summary>
        /// Set current language by code (e.g., "en", "es-419", "ar")
        /// </summary>
        public async Task<bool> SetLanguageAsync(string languageCode)
        {
            if (_provider == null)
            {
                Debug.LogError("[IVXLocalizationService] No provider set");
                return false;
            }

            if (string.IsNullOrEmpty(languageCode))
            {
                Debug.LogWarning("[IVXLocalizationService] Invalid language code, using default");
                languageCode = DEFAULT_LANGUAGE_CODE;
            }

            // Validate language is supported
            var supportedLanguages = _provider.GetSupportedLanguages();
            if (!supportedLanguages.Contains(languageCode))
            {
                Debug.LogWarning($"[IVXLocalizationService] Language {languageCode} not supported, using default");
                languageCode = DEFAULT_LANGUAGE_CODE;
            }

            Debug.Log($"[IVXLocalizationService] Loading language: {languageCode}");

            bool success = await _provider.LoadLanguageAsync(languageCode);
            if (success)
            {
                _currentLanguageCode = languageCode;
                PlayerPrefs.SetString(LANGUAGE_PREF_KEY, languageCode);
                PlayerPrefs.Save();

                // Notify listeners
                OnLanguageChanged?.Invoke(_currentLanguageCode);
                Debug.Log($"[IVXLocalizationService] Language changed to: {_currentLanguageCode}");
                return true;
            }
            else
            {
                Debug.LogError($"[IVXLocalizationService] Failed to load language: {languageCode}");
                return false;
            }
        }

        /// <summary>
        /// Set current language synchronously (for compatibility with legacy code)
        /// Note: This starts async operation but doesn't wait for completion.
        /// For guaranteed language loading, use SetLanguageAsync().
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            _ = SetLanguageAsync(languageCode);
        }

        /// <summary>
        /// Set current language by enum
        /// </summary>
        public async Task<bool> SetLanguageAsync(IVXLanguageCode language)
        {
            if (LanguageCodeMap.TryGetValue(language, out string code))
            {
                return await SetLanguageAsync(code);
            }
            else
            {
                Debug.LogError($"[IVXLocalizationService] Unknown language enum: {language}");
                return false;
            }
        }

        /// <summary>
        /// Get ISO code from enum
        /// </summary>
        public static string GetLanguageCode(IVXLanguageCode language)
        {
            return LanguageCodeMap.TryGetValue(language, out string code) ? code : DEFAULT_LANGUAGE_CODE;
        }

        /// <summary>
        /// Get enum from ISO code
        /// </summary>
        public static IVXLanguageCode GetLanguageEnum(string code)
        {
            foreach (var kvp in LanguageCodeMap)
            {
                if (kvp.Value.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }
            return IVXLanguageCode.English; // Default
        }

        #endregion

        #region String Retrieval

        /// <summary>
        /// Get localized string by key
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            if (_provider == null)
            {
                Debug.LogError($"[IVXLocalizationService] No provider set for key: {key}");
                return defaultValue;
            }

            return _provider.GetString(key, defaultValue);
        }

        /// <summary>
        /// Get localized string with result wrapper
        /// </summary>
        public IVXLocalizationResult GetStringResult(string key)
        {
            if (_provider == null)
            {
                return IVXLocalizationResult.Error("No provider set");
            }

            if (!_provider.HasKey(key))
            {
                return IVXLocalizationResult.Error($"Key not found: {key}");
            }

            string value = _provider.GetString(key, "");
            return IVXLocalizationResult.Ok(value);
        }

        /// <summary>
        /// Check if a key exists in current language
        /// </summary>
        public bool HasKey(string key)
        {
            return _provider?.HasKey(key) ?? false;
        }

        /// <summary>
        /// Get localized string with format arguments
        /// </summary>
        public string GetStringFormatted(string key, params object[] args)
        {
            string format = GetString(key);
            if (string.IsNullOrEmpty(format) || args == null || args.Length == 0)
                return format;

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException e)
            {
                Debug.LogError($"[IVXLocalizationService] Format error for key {key}: {e.Message}");
                return format;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if current language is RTL (Arabic, Hebrew)
        /// </summary>
        public bool IsRTL()
        {
            return _currentLanguageCode == "ar" || _currentLanguageCode == "he";
        }

        /// <summary>
        /// Reload current language (useful after CSV/JSON updates)
        /// </summary>
        public async Task<bool> ReloadCurrentLanguageAsync()
        {
            if (_provider == null || string.IsNullOrEmpty(_currentLanguageCode))
            {
                Debug.LogError("[IVXLocalizationService] Cannot reload - not initialized");
                return false;
            }

            Debug.Log($"[IVXLocalizationService] Reloading language: {_currentLanguageCode}");
            return await _provider.LoadLanguageAsync(_currentLanguageCode);
        }

        #endregion
    }
}

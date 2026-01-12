// IVXLocalizationHelper.cs
// Localization helper utilities for IntelliVerseX SDK
// Provides language detection, RTL support, and common localization utilities

using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Localization
{
    /// <summary>
    /// Localization helper utilities that can be used across all games.
    /// Does NOT contain game-specific localization data - only utilities.
    /// </summary>
    public static class IVXLocalizationHelper
    {
        // RTL language codes
        private static readonly HashSet<string> RTL_LANGUAGES = new HashSet<string>
        {
            "ar",    // Arabic
            "he",    // Hebrew
            "fa",    // Persian (Farsi)
            "ur",    // Urdu
            "yi"     // Yiddish
        };

        // Language code mappings (Unity locale codes → standard ISO codes)
        private static readonly Dictionary<string, string> LANGUAGE_CODE_MAPPINGS = new Dictionary<string, string>
        {
            { "es-419", "es-419" },  // Latin American Spanish
            { "zh-CN", "zh-Hans" },  // Simplified Chinese
            { "zh-TW", "zh-Hant" },  // Traditional Chinese
            { "pt-BR", "pt-BR" },    // Brazilian Portuguese
            { "pt-PT", "pt-PT" },    // European Portuguese
        };

        /// <summary>
        /// Detects the device's default language and returns ISO code.
        /// Falls back to English if language is not recognized.
        /// </summary>
        /// <returns>ISO language code (e.g., "en", "es", "zh-Hans")</returns>
        public static string DetectDeviceLanguage()
        {
            SystemLanguage systemLang = Application.systemLanguage;
            
            string langCode = systemLang switch
            {
                SystemLanguage.English => "en",
                SystemLanguage.Spanish => "es-419",
                SystemLanguage.Chinese => "zh-Hans",
                SystemLanguage.ChineseSimplified => "zh-Hans",
                SystemLanguage.ChineseTraditional => "zh-Hant",
                SystemLanguage.Arabic => "ar",
                SystemLanguage.French => "fr",
                SystemLanguage.German => "de",
                SystemLanguage.Hindi => "hi",
                SystemLanguage.Indonesian => "id",
                SystemLanguage.Japanese => "ja",
                SystemLanguage.Korean => "ko",
                SystemLanguage.Portuguese => "pt-BR",
                SystemLanguage.Russian => "ru",
                SystemLanguage.Italian => "it",
                SystemLanguage.Dutch => "nl",
                SystemLanguage.Polish => "pl",
                SystemLanguage.Turkish => "tr",
                SystemLanguage.Vietnamese => "vi",
                SystemLanguage.Thai => "th",
                SystemLanguage.Hebrew => "he",
                // Note: SystemLanguage doesn't have Persian/Urdu enums, handle separately
                _ => "en" // Default to English
            };

            Debug.Log($"[IVXLocalizationHelper] Detected device language: {systemLang} → {langCode}");
            return langCode;
        }

        /// <summary>
        /// Maps Unity Localization locale codes to standard ISO codes.
        /// Example: "es-419" stays "es-419", "zh-CN" → "zh-Hans"
        /// </summary>
        public static string MapLanguageCode(string unityLocaleCode)
        {
            if (string.IsNullOrEmpty(unityLocaleCode))
                return "en";

            if (LANGUAGE_CODE_MAPPINGS.TryGetValue(unityLocaleCode, out string mappedCode))
                return mappedCode;

            // Return as-is if no mapping needed
            return unityLocaleCode;
        }

        /// <summary>
        /// Checks if a language code uses Right-to-Left (RTL) writing system.
        /// </summary>
        /// <param name="langCode">ISO language code (e.g., "ar", "he")</param>
        /// <returns>True if RTL language, false otherwise</returns>
        public static bool IsRTL(string langCode)
        {
            if (string.IsNullOrEmpty(langCode))
                return false;

            // Check base language code (e.g., "ar" from "ar-SA")
            string baseLang = langCode.Split('-')[0].ToLower();
            return RTL_LANGUAGES.Contains(baseLang);
        }

        /// <summary>
        /// Gets the native (localized) name of a language.
        /// Example: GetLanguageName("es", "es") → "Español"
        /// </summary>
        /// <param name="langCode">Language code to get name for</param>
        /// <param name="displayLangCode">Language to display name in (defaults to same language)</param>
        /// <returns>Native language name</returns>
        public static string GetLanguageName(string langCode, string displayLangCode = null)
        {
            displayLangCode = displayLangCode ?? langCode;

            // If asking for native name (display same as lang), return native names
            if (displayLangCode == langCode)
            {
                return langCode switch
                {
                    "en" => "English",
                    "es" or "es-419" => "Español",
                    "zh-Hans" or "zh-CN" => "简体中文",
                    "zh-Hant" or "zh-TW" => "繁體中文",
                    "ar" => "العربية",
                    "fr" => "Français",
                    "de" => "Deutsch",
                    "hi" => "हिन्दी",
                    "id" => "Bahasa Indonesia",
                    "ja" => "日本語",
                    "ko" => "한국어",
                    "pt-BR" => "Português (Brasil)",
                    "pt-PT" => "Português (Portugal)",
                    "ru" => "Русский",
                    "it" => "Italiano",
                    "nl" => "Nederlands",
                    "pl" => "Polski",
                    "tr" => "Türkçe",
                    "vi" => "Tiếng Việt",
                    "th" => "ไทย",
                    "he" => "עברית",
                    "fa" => "فارسی",
                    "ur" => "اردو",
                    _ => langCode
                };
            }

            // If asking for English names
            if (displayLangCode == "en")
            {
                return langCode switch
                {
                    "en" => "English",
                    "es" or "es-419" => "Spanish",
                    "zh-Hans" or "zh-CN" => "Chinese (Simplified)",
                    "zh-Hant" or "zh-TW" => "Chinese (Traditional)",
                    "ar" => "Arabic",
                    "fr" => "French",
                    "de" => "German",
                    "hi" => "Hindi",
                    "id" => "Indonesian",
                    "ja" => "Japanese",
                    "ko" => "Korean",
                    "pt-BR" => "Portuguese (Brazil)",
                    "pt-PT" => "Portuguese (Portugal)",
                    "ru" => "Russian",
                    "it" => "Italian",
                    "nl" => "Dutch",
                    "pl" => "Polish",
                    "tr" => "Turkish",
                    "vi" => "Vietnamese",
                    "th" => "Thai",
                    "he" => "Hebrew",
                    "fa" => "Persian",
                    "ur" => "Urdu",
                    _ => langCode
                };
            }

            // For other display languages, return the code
            return langCode;
        }

        /// <summary>
        /// Gets the current Unity Localization selected locale code.
        /// </summary>
        /// <returns>Current locale code or "en" if not available</returns>
        public static string GetCurrentUnityLocaleCode()
        {
            try
            {
#if UNITY_LOCALIZATION
                var locale = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale;
                return locale?.Identifier.Code ?? "en";
#else
                Debug.LogWarning("[IVXLocalizationHelper] Unity Localization package not installed, using device language");
                return DetectDeviceLanguage();
#endif
            }
            catch
            {
                Debug.LogWarning("[IVXLocalizationHelper] Unity Localization not initialized, defaulting to 'en'");
                return "en";
            }
        }

        /// <summary>
        /// Gets the current language code with standard mapping applied.
        /// </summary>
        public static string GetCurrentLanguageCode()
        {
            string unityCode = GetCurrentUnityLocaleCode();
            return MapLanguageCode(unityCode);
        }

        /// <summary>
        /// Validates if a language code is supported.
        /// Override this in game-specific code to check against your supported languages.
        /// </summary>
        public static bool IsLanguageSupported(string langCode, string[] supportedLanguages)
        {
            if (string.IsNullOrEmpty(langCode) || supportedLanguages == null)
                return false;

            foreach (string supported in supportedLanguages)
            {
                if (supported.Equals(langCode, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}

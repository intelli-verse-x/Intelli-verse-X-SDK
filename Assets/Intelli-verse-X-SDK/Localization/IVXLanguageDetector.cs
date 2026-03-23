// File: IVXLanguageDetector.cs
// Purpose: Auto-detect device language with fallback logic
// Package: IntelliVerseX.Localization

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Localization
{
    /// <summary>
    /// Auto-detects device language with fallback to English.
    /// Supports 12 languages matching QuizVerse.
    /// 
    /// Usage:
    ///   string deviceLang = IVXLanguageDetector.DetectDeviceLanguage();
    ///   // Returns: "en", "es-419", "ar", etc.
    /// </summary>
    public static class IVXLanguageDetector
    {
        #region Constants

        private const string DEFAULT_LANGUAGE = "en";

        // Map Unity SystemLanguage to our language codes
        private static readonly Dictionary<SystemLanguage, string> LanguageMap = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "en" },
            { SystemLanguage.Spanish, "es-419" }, // Default to LatAm Spanish
            { SystemLanguage.Arabic, "ar" },
            { SystemLanguage.Chinese, "zh-CN" },
            { SystemLanguage.ChineseSimplified, "zh-CN" },
            { SystemLanguage.French, "fr" },
            { SystemLanguage.German, "de" },
            { SystemLanguage.Hindi, "hi" },
            { SystemLanguage.Indonesian, "id" },
            { SystemLanguage.Japanese, "ja" },
            { SystemLanguage.Korean, "ko" },
            { SystemLanguage.Portuguese, "pt" },
            { SystemLanguage.Russian, "ru" }
        };

        // Supported language codes (12 languages)
        private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
        {
            "en", "es-419", "ar", "zh-CN", "fr", "de", 
            "hi", "id", "ja", "ko", "pt", "ru", "es", "zu"
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Detect device language with fallback to English
        /// </summary>
        public static string DetectDeviceLanguage()
        {
            // Get Unity's detected system language
            SystemLanguage systemLang = Application.systemLanguage;

            // Map to our language code
            if (LanguageMap.TryGetValue(systemLang, out string languageCode))
            {
                Debug.Log($"[IVXLanguageDetector] Detected device language: {systemLang} -> {languageCode}");
                return languageCode;
            }
            else
            {
                Debug.LogWarning($"[IVXLanguageDetector] Unsupported system language: {systemLang}, defaulting to {DEFAULT_LANGUAGE}");
                return DEFAULT_LANGUAGE;
            }
        }

        /// <summary>
        /// Check if a language code is supported
        /// </summary>
        public static bool IsLanguageSupported(string languageCode)
        {
            return !string.IsNullOrEmpty(languageCode) && SupportedLanguages.Contains(languageCode);
        }

        /// <summary>
        /// Validate and fallback to default if unsupported
        /// </summary>
        public static string ValidateLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                Debug.LogWarning($"[IVXLanguageDetector] Empty language code, using default: {DEFAULT_LANGUAGE}");
                return DEFAULT_LANGUAGE;
            }

            if (!IsLanguageSupported(languageCode))
            {
                Debug.LogWarning($"[IVXLanguageDetector] Unsupported language: {languageCode}, using default: {DEFAULT_LANGUAGE}");
                return DEFAULT_LANGUAGE;
            }

            return languageCode;
        }

        /// <summary>
        /// Get all supported language codes
        /// </summary>
        public static List<string> GetSupportedLanguages()
        {
            return new List<string>(SupportedLanguages);
        }

        /// <summary>
        /// Get language name from code (for display in UI)
        /// </summary>
        public static string GetLanguageName(string languageCode)
        {
            switch (languageCode)
            {
                case "en": return "English";
                case "es-419": return "Español (Latinoamérica)";
                case "es": return "Español (España)";
                case "ar": return "العربية";
                case "zh-CN": return "中文 (简体)";
                case "fr": return "Français";
                case "de": return "Deutsch";
                case "hi": return "हिन्दी";
                case "id": return "Bahasa Indonesia";
                case "ja": return "日本語";
                case "ko": return "한국어";
                case "pt": return "Português";
                case "ru": return "Русский";
                case "zu": return "isiZulu";
                default: return languageCode;
            }
        }

        /// <summary>
        /// Detect if device language is RTL
        /// </summary>
        public static bool IsDeviceLanguageRTL()
        {
            string deviceLang = DetectDeviceLanguage();
            return IsRTL(deviceLang);
        }

        /// <summary>
        /// Check if a language is RTL
        /// </summary>
        public static bool IsRTL(string languageCode)
        {
            return languageCode == "ar" || languageCode == "he"; // Arabic, Hebrew
        }

        #endregion
    }
}

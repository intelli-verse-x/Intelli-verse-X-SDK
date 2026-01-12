using System;
using UnityEngine;

namespace IntelliVerseX.Localization
{
    /// <summary>
    /// Language manager for IntelliVerse-X SDK.
    /// Handles multi-language support with auto-detection.
    /// 
    /// Usage:
    ///   string lang = IVXLanguageManager.CurrentLanguage; // "en", "es", etc.
    ///   
    ///   IVXLanguageManager.SetLanguage("es");
    ///   
    ///   IVXLanguageManager.DetectAndSetLanguage();
    /// </summary>
    public static class IVXLanguageManager
    {
        private const string PREF_LANGUAGE = "IVX_Language";
        
        private static Core.IntelliVerseXConfig _config;
        private static string _currentLanguage = "en";
        
        // Events
        public static event Action<string> OnLanguageChanged; // new language code

        /// <summary>
        /// Get current language code (e.g., "en", "es", "fr")
        /// </summary>
        public static string CurrentLanguage => _currentLanguage;

        /// <summary>
        /// Get supported languages from config
        /// </summary>
        public static string[] SupportedLanguages
        {
            get
            {
                if (_config == null || _config.supportedLanguages == null)
                    return new[] { "en" };

                var codes = new string[_config.supportedLanguages.Length];
                for (int i = 0; i < _config.supportedLanguages.Length; i++)
                {
                    codes[i] = LanguageToCode(_config.supportedLanguages[i]);
                }
                return codes;
            }
        }

        /// <summary>
        /// Initialize language manager
        /// </summary>
        public static void Initialize(Core.IntelliVerseXConfig config)
        {
            _config = config;

            // Try to load saved language
            if (PlayerPrefs.HasKey(PREF_LANGUAGE))
            {
                string savedLang = PlayerPrefs.GetString(PREF_LANGUAGE);
                if (IsSupportedLanguage(savedLang))
                {
                    _currentLanguage = savedLang;
                    Debug.Log($"[IVXLanguageManager] Loaded saved language: {_currentLanguage}");
                    return;
                }
            }

            // Auto-detect language
            DetectAndSetLanguage();
        }

        /// <summary>
        /// Auto-detect device language and set if supported
        /// Falls back to default language if not supported
        /// </summary>
        public static void DetectAndSetLanguage()
        {
            string detectedCode = LanguageToCode(Application.systemLanguage);
            
            Debug.Log($"[IVXLanguageManager] Detected system language: {Application.systemLanguage} ({detectedCode})");

            if (IsSupportedLanguage(detectedCode))
            {
                SetLanguage(detectedCode);
            }
            else
            {
                string defaultCode = LanguageToCode(_config?.defaultLanguage ?? SystemLanguage.English);
                Debug.Log($"[IVXLanguageManager] Detected language not supported, using default: {defaultCode}");
                SetLanguage(defaultCode);
            }
        }

        /// <summary>
        /// Set language
        /// </summary>
        /// <param name="languageCode">Language code (e.g., "en", "es", "fr")</param>
        public static void SetLanguage(string languageCode)
        {
            if (!IsSupportedLanguage(languageCode))
            {
                Debug.LogWarning($"[IVXLanguageManager] Language '{languageCode}' not supported");
                return;
            }

            _currentLanguage = languageCode;
            PlayerPrefs.SetString(PREF_LANGUAGE, languageCode);
            PlayerPrefs.Save();

            Debug.Log($"[IVXLanguageManager] Language set to: {languageCode}");
            OnLanguageChanged?.Invoke(languageCode);
        }

        /// <summary>
        /// Check if language is supported
        /// </summary>
        public static bool IsSupportedLanguage(string languageCode)
        {
            foreach (string supportedCode in SupportedLanguages)
            {
                if (supportedCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Convert SystemLanguage to 2-letter ISO 639-1 code
        /// </summary>
        public static string LanguageToCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English: return "en";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return "zh";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Swedish: return "sv";
                case SystemLanguage.Norwegian: return "no";
                case SystemLanguage.Danish: return "da";
                case SystemLanguage.Finnish: return "fi";
                case SystemLanguage.Greek: return "el";
                case SystemLanguage.Hebrew: return "he";
                case SystemLanguage.Hindi: return "hi";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Vietnamese: return "vi";
                default: return "en"; // Fallback to English
            }
        }

        /// <summary>
        /// Convert 2-letter ISO 639-1 code to SystemLanguage
        /// </summary>
        public static SystemLanguage CodeToLanguage(string code)
        {
            switch (code.ToLower())
            {
                case "en": return SystemLanguage.English;
                case "es": return SystemLanguage.Spanish;
                case "fr": return SystemLanguage.French;
                case "de": return SystemLanguage.German;
                case "pt": return SystemLanguage.Portuguese;
                case "zh": return SystemLanguage.Chinese;
                case "ja": return SystemLanguage.Japanese;
                case "ko": return SystemLanguage.Korean;
                case "ru": return SystemLanguage.Russian;
                case "it": return SystemLanguage.Italian;
                case "nl": return SystemLanguage.Dutch;
                case "pl": return SystemLanguage.Polish;
                case "ar": return SystemLanguage.Arabic;
                case "tr": return SystemLanguage.Turkish;
                case "sv": return SystemLanguage.Swedish;
                case "no": return SystemLanguage.Norwegian;
                case "da": return SystemLanguage.Danish;
                case "fi": return SystemLanguage.Finnish;
                case "el": return SystemLanguage.Greek;
                case "he": return SystemLanguage.Hebrew;
                case "hi": return SystemLanguage.Hindi;
                case "id": return SystemLanguage.Indonesian;
                case "th": return SystemLanguage.Thai;
                case "vi": return SystemLanguage.Vietnamese;
                default: return SystemLanguage.English;
            }
        }

        /// <summary>
        /// Get language display name
        /// </summary>
        public static string GetLanguageName(string code)
        {
            switch (code.ToLower())
            {
                case "en": return "English";
                case "es": return "Español";
                case "fr": return "Français";
                case "de": return "Deutsch";
                case "pt": return "Português";
                case "zh": return "中文";
                case "ja": return "日本語";
                case "ko": return "한국어";
                case "ru": return "Русский";
                case "it": return "Italiano";
                case "nl": return "Nederlands";
                case "pl": return "Polski";
                case "ar": return "العربية";
                case "tr": return "Türkçe";
                default: return code.ToUpper();
            }
        }
    }
}

// File: IVXCSVLocalizationProvider.cs
// Purpose: CSV-based localization provider for QuizVerse format compatibility
// Package: IntelliVerseX.Localization
// Dependencies: IntelliVerseX.Core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Localization
{
    /// <summary>
    /// CSV-based localization provider compatible with QuizVerse CSV format.
    /// Loads translations from TextAsset CSVs (one per language).
    /// 
    /// CSV Format:
    ///   Key,Value
    ///   Button_Play,Play
    ///   Button_Exit,Exit
    /// 
    /// Usage:
    ///   var provider = new IVXCSVLocalizationProvider();
    ///   provider.AddLanguage("en", englishCSV);
    ///   await provider.LoadLanguageAsync("en");
    /// </summary>
    public class IVXCSVLocalizationProvider : IIVXLocalizationProvider
    {
        #region Private Fields

        // Language code -> CSV TextAsset mapping
        private Dictionary<string, TextAsset> _languageAssets = new Dictionary<string, TextAsset>();

        // Current language code
        private string _currentLanguage = "en";

        // Current translations (key -> value)
        private Dictionary<string, string> _translations = new Dictionary<string, string>();

        // Cache parsed CSVs to avoid re-parsing
        private Dictionary<string, Dictionary<string, string>> _cache = new Dictionary<string, Dictionary<string, string>>();

        #endregion

        #region Configuration

        /// <summary>
        /// Add a language CSV asset
        /// </summary>
        public void AddLanguage(string languageCode, TextAsset csvAsset)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                Debug.LogError("[IVXCSVLocalizationProvider] Invalid language code");
                return;
            }

            if (csvAsset == null)
            {
                Debug.LogError($"[IVXCSVLocalizationProvider] CSV asset is null for language: {languageCode}");
                return;
            }

            _languageAssets[languageCode] = csvAsset;
            Debug.Log($"[IVXCSVLocalizationProvider] Added language: {languageCode} ({csvAsset.name})");
        }

        /// <summary>
        /// Add multiple languages at once
        /// </summary>
        public void AddLanguages(Dictionary<string, TextAsset> languages)
        {
            if (languages == null) return;

            foreach (var kvp in languages)
            {
                AddLanguage(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region IIVXLocalizationProvider Implementation

        public async Task<bool> LoadLanguageAsync(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                Debug.LogError("[IVXCSVLocalizationProvider] Invalid language code");
                return false;
            }

            // Check if language is registered
            if (!_languageAssets.ContainsKey(languageCode))
            {
                Debug.LogError($"[IVXCSVLocalizationProvider] Language not registered: {languageCode}");
                return false;
            }

            // Check cache first
            if (_cache.TryGetValue(languageCode, out var cachedTranslations))
            {
                _translations = cachedTranslations;
                _currentLanguage = languageCode;
                Debug.Log($"[IVXCSVLocalizationProvider] Loaded from cache: {languageCode} ({_translations.Count} keys)");
                return true;
            }

            // Parse CSV (async to avoid blocking)
            await Task.Run(() =>
            {
                var csvAsset = _languageAssets[languageCode];
                var parsed = ParseCSV(csvAsset.text);
                _cache[languageCode] = parsed;
                _translations = parsed;
            });

            _currentLanguage = languageCode;
            Debug.Log($"[IVXCSVLocalizationProvider] Loaded: {languageCode} ({_translations.Count} keys)");
            return true;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            return _translations.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public bool HasKey(string key)
        {
            return !string.IsNullOrEmpty(key) && _translations.ContainsKey(key);
        }

        public List<string> GetSupportedLanguages()
        {
            return _languageAssets.Keys.ToList();
        }

        #endregion

        #region CSV Parsing

        /// <summary>
        /// Parse CSV text into key-value dictionary
        /// Format: Key,Value (with optional quotes)
        /// </summary>
        private Dictionary<string, string> ParseCSV(string csvText)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(csvText))
            {
                Debug.LogWarning("[IVXCSVLocalizationProvider] CSV text is empty");
                return result;
            }

            // Split by lines
            string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Skip header row (Key,Value)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue; // Skip empty lines and comments

                // Parse CSV line (handles quoted values with commas)
                var parts = ParseCSVLine(line);
                if (parts.Count >= 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (!string.IsNullOrEmpty(key))
                    {
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parse a single CSV line (handles quoted values)
        /// Example: "Button_Play","Click to Play" -> ["Button_Play", "Click to Play"]
        /// </summary>
        private List<string> ParseCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentValue = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentValue);
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }

            // Add last value
            if (!string.IsNullOrEmpty(currentValue) || result.Count > 0)
            {
                result.Add(currentValue);
            }

            return result;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clear all cached translations
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
            Debug.Log("[IVXCSVLocalizationProvider] Cache cleared");
        }

        /// <summary>
        /// Get all translation keys for current language
        /// </summary>
        public List<string> GetAllKeys()
        {
            return _translations.Keys.ToList();
        }

        /// <summary>
        /// Get translation count for current language
        /// </summary>
        public int GetTranslationCount()
        {
            return _translations.Count;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Provides production-safe emoji conversion utilities for TextMeshPro.
    /// Converts Unicode emoji sequences to TMP sprite tags using emoji-style sprite names (hex or hex-joined).
    /// </summary>
    public static class IVXEmojiTextUtility
    {
        private const int VARIATION_SELECTOR_15 = 0xFE0E;
        private const int VARIATION_SELECTOR_16 = 0xFE0F;
        private const int ZERO_WIDTH_JOINER = 0x200D;
        private const int COMBINING_KEYCAP = 0x20E3;
        private const int TAG_SEQUENCE_START = 0xE0020;
        private const int TAG_SEQUENCE_END = 0xE007F;
        private const int MAX_CACHE_ENTRIES = 1024;

        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, string> ConversionCache = new Dictionary<string, string>(256);

        /// <summary>
        /// Converts Unicode emojis to TMP sprite tags and caches the result.
        /// </summary>
        /// <param name="text">Input text that may contain emojis.</param>
        /// <returns>Text with emojis replaced by TMP sprite tags.</returns>
        public static string ConvertUnicodeToSpriteTags(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            lock (CacheLock)
            {
                if (ConversionCache.TryGetValue(text, out string cached))
                {
                    return cached;
                }
            }

            string converted = ConvertUnicodeToSpriteTagsInternal(
                text,
                availableSpriteNames: null,
                keepUnsupportedUnicode: true);

            lock (CacheLock)
            {
                if (ConversionCache.Count > MAX_CACHE_ENTRIES)
                {
                    ConversionCache.Clear();
                }

                ConversionCache[text] = converted;
            }

            return converted;
        }

        /// <summary>
        /// Converts Unicode emojis to TMP sprite tags and validates against a sprite asset set.
        /// </summary>
        /// <param name="text">Input text that may contain emojis.</param>
        /// <param name="spriteAsset">Primary sprite asset used for lookup.</param>
        /// <param name="includeFallbackAssets">Whether fallback sprite assets should be considered.</param>
        /// <param name="keepUnsupportedUnicode">If true, unsupported emojis remain Unicode; otherwise replaced by replacementCharacter.</param>
        /// <param name="replacementCharacter">Character used when keepUnsupportedUnicode is false and emoji is unsupported.</param>
        /// <returns>Text with supported emojis converted to TMP sprite tags.</returns>
        public static string ConvertUnicodeToSpriteTags(
            string text,
            TMP_SpriteAsset spriteAsset,
            bool includeFallbackAssets = true,
            bool keepUnsupportedUnicode = true,
            char replacementCharacter = '□')
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            HashSet<string> availableNames = BuildSpriteNameSet(spriteAsset, includeFallbackAssets);
            string converted = ConvertUnicodeToSpriteTagsInternal(text, availableNames, keepUnsupportedUnicode, replacementCharacter);
            return converted;
        }

        /// <summary>
        /// Removes FE0E and FE0F variation selectors from text.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>Text without variation selectors.</returns>
        public static string StripVariationSelectors(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            StringBuilder sb = new StringBuilder(text.Length);
            int index = 0;

            while (index < text.Length)
            {
                if (!TryReadCodePoint(text, index, out int codePoint, out int consumedChars))
                {
                    break;
                }

                if (codePoint != VARIATION_SELECTOR_15 && codePoint != VARIATION_SELECTOR_16)
                {
                    sb.Append(text, index, consumedChars);
                }

                index += consumedChars;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if the text contains at least one emoji sequence.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>True when an emoji sequence is found.</returns>
        public static bool ContainsEmoji(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            int index = 0;
            while (index < text.Length)
            {
                if (TryReadEmojiSequence(text, index, out int consumedChars, out _, out _))
                {
                    return true;
                }

                if (!TryReadCodePoint(text, index, out _, out int cpChars))
                {
                    break;
                }

                index += cpChars;
            }

            return false;
        }

        /// <summary>
        /// Gets sprite names that are required by the text but missing in the provided sprite asset set.
        /// </summary>
        /// <param name="text">Input text containing emojis.</param>
        /// <param name="spriteAsset">Primary sprite asset to validate against.</param>
        /// <param name="includeFallbackAssets">Whether fallback sprite assets should be considered.</param>
        /// <returns>Unique missing sprite names.</returns>
        public static List<string> GetMissingSpriteNames(
            string text,
            TMP_SpriteAsset spriteAsset,
            bool includeFallbackAssets = true)
        {
            List<string> missing = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                return missing;
            }

            HashSet<string> availableNames = BuildSpriteNameSet(spriteAsset, includeFallbackAssets);
            if (availableNames.Count == 0)
            {
                return missing;
            }

            HashSet<string> uniqueMissing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int index = 0;

            while (index < text.Length)
            {
                if (TryReadEmojiSequence(text, index, out int consumedChars, out string spriteName, out string unicodeEmoji))
                {
                    if (!HasMatchingSpriteName(unicodeEmoji, spriteName, availableNames))
                    {
                        uniqueMissing.Add(spriteName);
                    }

                    index += consumedChars;
                    continue;
                }

                if (!TryReadCodePoint(text, index, out _, out int cpChars))
                {
                    break;
                }

                index += cpChars;
            }

            foreach (string item in uniqueMissing)
            {
                missing.Add(item);
            }

            return missing;
        }

        /// <summary>
        /// Clears the conversion cache.
        /// </summary>
        public static void ClearCache()
        {
            lock (CacheLock)
            {
                ConversionCache.Clear();
            }
        }

        /// <summary>
        /// Gets current number of cache entries.
        /// </summary>
        public static int CacheSize
        {
            get
            {
                lock (CacheLock)
                {
                    return ConversionCache.Count;
                }
            }
        }

        private static string ConvertUnicodeToSpriteTagsInternal(
            string text,
            HashSet<string> availableSpriteNames,
            bool keepUnsupportedUnicode,
            char replacementCharacter = '□')
        {
            StringBuilder result = new StringBuilder(text.Length + 16);
            int index = 0;

            while (index < text.Length)
            {
                if (TryReadEmojiSequence(text, index, out int consumedChars, out string spriteName, out string unicodeEmoji))
                {
                    bool isAvailable;
                    string resolvedSpriteName = spriteName;

                    if (availableSpriteNames == null)
                    {
                        isAvailable = true;
                    }
                    else
                    {
                        resolvedSpriteName = ResolveSpriteName(unicodeEmoji, spriteName, availableSpriteNames);
                        isAvailable = !string.IsNullOrEmpty(resolvedSpriteName);
                    }

                    if (isAvailable)
                    {
                        result.Append("<sprite name=\"");
                        result.Append(resolvedSpriteName);
                        result.Append("\">");
                    }
                    else
                    {
                        if (keepUnsupportedUnicode)
                        {
                            result.Append(unicodeEmoji);
                        }
                        else
                        {
                            result.Append(replacementCharacter);
                        }
                    }

                    index += consumedChars;
                    continue;
                }

                if (!TryReadCodePoint(text, index, out _, out int cpChars))
                {
                    break;
                }

                result.Append(text, index, cpChars);
                index += cpChars;
            }

            return result.ToString();
        }

        private static bool HasMatchingSpriteName(string unicodeEmoji, string normalizedSpriteName, HashSet<string> availableNames)
        {
            if (availableNames.Contains(normalizedSpriteName))
            {
                return true;
            }

            List<string> candidates = BuildSpriteNameCandidates(unicodeEmoji, normalizedSpriteName);
            int count = candidates.Count;
            for (int i = 0; i < count; i++)
            {
                if (availableNames.Contains(candidates[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveSpriteName(string unicodeEmoji, string normalizedSpriteName, HashSet<string> availableNames)
        {
            if (availableNames.Contains(normalizedSpriteName))
            {
                return normalizedSpriteName;
            }

            List<string> candidates = BuildSpriteNameCandidates(unicodeEmoji, normalizedSpriteName);
            int count = candidates.Count;
            for (int i = 0; i < count; i++)
            {
                string candidate = candidates[i];
                if (availableNames.Contains(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static List<string> BuildSpriteNameCandidates(string unicodeEmoji, string normalizedSpriteName)
        {
            List<string> candidates = new List<string>(4);
            HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(normalizedSpriteName) && unique.Add(normalizedSpriteName))
            {
                candidates.Add(normalizedSpriteName);
            }

            List<int> codePoints = ParseCodePoints(unicodeEmoji);
            if (codePoints.Count == 0)
            {
                return candidates;
            }

            string noVariationKeepZwJ = BuildSpriteName(codePoints, includeVariationSelectors: false, includeZwJ: true);
            if (!string.IsNullOrWhiteSpace(noVariationKeepZwJ) && unique.Add(noVariationKeepZwJ))
            {
                candidates.Add(noVariationKeepZwJ);
            }

            string fullRaw = BuildSpriteName(codePoints, includeVariationSelectors: true, includeZwJ: true);
            if (!string.IsNullOrWhiteSpace(fullRaw) && unique.Add(fullRaw))
            {
                candidates.Add(fullRaw);
            }

            string keepVariationNoZwJ = BuildSpriteName(codePoints, includeVariationSelectors: true, includeZwJ: false);
            if (!string.IsNullOrWhiteSpace(keepVariationNoZwJ) && unique.Add(keepVariationNoZwJ))
            {
                candidates.Add(keepVariationNoZwJ);
            }

            return candidates;
        }

        private static HashSet<string> BuildSpriteNameSet(TMP_SpriteAsset spriteAsset, bool includeFallbackAssets)
        {
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (spriteAsset == null)
            {
                return names;
            }

            HashSet<int> visited = new HashSet<int>();
            CollectSpriteNames(spriteAsset, includeFallbackAssets, names, visited);
            return names;
        }

        private static void CollectSpriteNames(
            TMP_SpriteAsset spriteAsset,
            bool includeFallbackAssets,
            HashSet<string> names,
            HashSet<int> visited)
        {
            if (spriteAsset == null)
            {
                return;
            }

            int id = spriteAsset.GetInstanceID();
            if (visited.Contains(id))
            {
                return;
            }

            visited.Add(id);

            if (spriteAsset.spriteCharacterTable != null)
            {
                int count = spriteAsset.spriteCharacterTable.Count;
                for (int i = 0; i < count; i++)
                {
                    var spriteCharacter = spriteAsset.spriteCharacterTable[i];
                    if (spriteCharacter == null || string.IsNullOrWhiteSpace(spriteCharacter.name))
                    {
                        continue;
                    }

                    names.Add(spriteCharacter.name.ToLowerInvariant());
                }
            }

            if (!includeFallbackAssets || spriteAsset.fallbackSpriteAssets == null)
            {
                return;
            }

            int fallbackCount = spriteAsset.fallbackSpriteAssets.Count;
            for (int i = 0; i < fallbackCount; i++)
            {
                CollectSpriteNames(spriteAsset.fallbackSpriteAssets[i], includeFallbackAssets, names, visited);
            }
        }

        private static bool TryReadEmojiSequence(
            string text,
            int startIndex,
            out int consumedChars,
            out string spriteName,
            out string unicodeText)
        {
            consumedChars = 0;
            spriteName = null;
            unicodeText = null;

            if (!TryReadCodePoint(text, startIndex, out int firstCodePoint, out int firstChars))
            {
                return false;
            }

            if (!IsPotentialEmojiStarter(firstCodePoint))
            {
                return false;
            }

            int index = startIndex;
            StringBuilder raw = new StringBuilder(16);
            List<int> codePoints = new List<int>(8);

            raw.Append(text, index, firstChars);
            index += firstChars;
            codePoints.Add(firstCodePoint);

            if (IsRegionalIndicator(firstCodePoint))
            {
                if (!TryReadCodePoint(text, index, out int secondRegional, out int secondRegionalChars) ||
                    !IsRegionalIndicator(secondRegional))
                {
                    return false;
                }

                raw.Append(text, index, secondRegionalChars);
                index += secondRegionalChars;
                codePoints.Add(secondRegional);

                consumedChars = index - startIndex;
                spriteName = BuildSpriteName(codePoints);
                unicodeText = raw.ToString();
                return true;
            }

            while (index < text.Length)
            {
                if (!TryReadCodePoint(text, index, out int nextCodePoint, out int nextChars))
                {
                    break;
                }

                if (nextCodePoint == VARIATION_SELECTOR_15 || nextCodePoint == VARIATION_SELECTOR_16)
                {
                    raw.Append(text, index, nextChars);
                    index += nextChars;
                    continue;
                }

                if (nextCodePoint == ZERO_WIDTH_JOINER)
                {
                    raw.Append(text, index, nextChars);
                    index += nextChars;

                    if (!TryReadCodePoint(text, index, out int joinedCodePoint, out int joinedChars))
                    {
                        break;
                    }

                    codePoints.Add(joinedCodePoint);
                    raw.Append(text, index, joinedChars);
                    index += joinedChars;
                    continue;
                }

                if (IsEmojiModifier(nextCodePoint) ||
                    nextCodePoint == COMBINING_KEYCAP ||
                    IsTagSpecCodePoint(nextCodePoint))
                {
                    codePoints.Add(nextCodePoint);
                    raw.Append(text, index, nextChars);
                    index += nextChars;
                    continue;
                }

                break;
            }

            if (IsKeycapBase(firstCodePoint) && !codePoints.Contains(COMBINING_KEYCAP))
            {
                return false;
            }

            consumedChars = index - startIndex;
            spriteName = BuildSpriteName(codePoints);
            unicodeText = raw.ToString();
            return true;
        }

        private static bool TryReadCodePoint(string text, int index, out int codePoint, out int consumedChars)
        {
            codePoint = 0;
            consumedChars = 0;

            if (string.IsNullOrEmpty(text) || index < 0 || index >= text.Length)
            {
                return false;
            }

            char c = text[index];
            if (char.IsHighSurrogate(c))
            {
                if (index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
                {
                    codePoint = char.ConvertToUtf32(c, text[index + 1]);
                    consumedChars = 2;
                    return true;
                }

                return false;
            }

            codePoint = c;
            consumedChars = 1;
            return true;
        }

        private static bool IsPotentialEmojiStarter(int codePoint)
        {
            if (IsRegionalIndicator(codePoint))
            {
                return true;
            }

            if (IsKeycapBase(codePoint))
            {
                return true;
            }

            if (codePoint == 0x00A9 || codePoint == 0x00AE)
            {
                return true;
            }

            return
                (codePoint >= 0x1F000 && codePoint <= 0x1FAFF) ||
                (codePoint >= 0x2600 && codePoint <= 0x27BF) ||
                (codePoint >= 0x2300 && codePoint <= 0x23FF) ||
                (codePoint >= 0x2B00 && codePoint <= 0x2BFF) ||
                codePoint == 0x2764;
        }

        private static bool IsEmojiModifier(int codePoint)
        {
            return codePoint >= 0x1F3FB && codePoint <= 0x1F3FF;
        }

        private static bool IsRegionalIndicator(int codePoint)
        {
            return codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF;
        }

        private static bool IsTagSpecCodePoint(int codePoint)
        {
            return codePoint >= TAG_SEQUENCE_START && codePoint <= TAG_SEQUENCE_END;
        }

        private static bool IsKeycapBase(int codePoint)
        {
            return codePoint == 0x23 || codePoint == 0x2A || (codePoint >= 0x30 && codePoint <= 0x39);
        }

        private static List<int> ParseCodePoints(string text)
        {
            List<int> codePoints = new List<int>(8);
            if (string.IsNullOrEmpty(text))
            {
                return codePoints;
            }

            int index = 0;
            while (index < text.Length)
            {
                if (!TryReadCodePoint(text, index, out int codePoint, out int consumedChars))
                {
                    break;
                }

                codePoints.Add(codePoint);
                index += consumedChars;
            }

            return codePoints;
        }

        private static string BuildSpriteName(List<int> codePoints, bool includeVariationSelectors = false, bool includeZwJ = false)
        {
            StringBuilder sb = new StringBuilder(24);
            bool hasAny = false;

            for (int i = 0; i < codePoints.Count; i++)
            {
                int codePoint = codePoints[i];
                if (!includeVariationSelectors && (codePoint == VARIATION_SELECTOR_15 || codePoint == VARIATION_SELECTOR_16))
                {
                    continue;
                }

                if (!includeZwJ && codePoint == ZERO_WIDTH_JOINER)
                {
                    continue;
                }

                if (i > 0)
                {
                    if (hasAny)
                    {
                        sb.Append('-');
                    }
                }

                sb.Append(codePoint.ToString("x"));
                hasAny = true;
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// TMP extension methods for IntelliVerseX emoji-safe text assignment.
    /// </summary>
    public static class IVXEmojiTextExtensions
    {
        /// <summary>
        /// Sets TMP text after converting Unicode emojis to TMP sprite tags.
        /// </summary>
        /// <param name="tmpText">Target TMP text component.</param>
        /// <param name="text">Raw input text.</param>
        /// <param name="spriteAssetOverride">Optional sprite asset used to keep only supported emojis as sprite tags.</param>
        public static void SetTextWithEmojiSprites(this TMP_Text tmpText, string text, TMP_SpriteAsset spriteAssetOverride = null)
        {
            if (tmpText == null)
            {
                return;
            }

            string converted = spriteAssetOverride == null
                ? IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(text)
                : IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(text, spriteAssetOverride);

            tmpText.text = converted;
        }

        /// <summary>
        /// Sets TMP input text after converting Unicode emojis to TMP sprite tags.
        /// </summary>
        /// <param name="inputField">Target TMP input field.</param>
        /// <param name="text">Raw input text.</param>
        /// <param name="spriteAssetOverride">Optional sprite asset used to keep only supported emojis as sprite tags.</param>
        public static void SetTextWithEmojiSprites(this TMP_InputField inputField, string text, TMP_SpriteAsset spriteAssetOverride = null)
        {
            if (inputField == null)
            {
                return;
            }

            string converted = spriteAssetOverride == null
                ? IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(text)
                : IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(text, spriteAssetOverride);

            inputField.text = converted;
        }

        /// <summary>
        /// Sets TMP text while stripping variation selectors only.
        /// Useful when you want Unicode emojis to remain as Unicode but avoid selector mismatch warnings.
        /// </summary>
        /// <param name="tmpText">Target TMP text component.</param>
        /// <param name="text">Raw input text.</param>
        public static void SetTextEmojiSafe(this TMP_Text tmpText, string text)
        {
            if (tmpText == null)
            {
                return;
            }

            tmpText.text = IVXEmojiTextUtility.StripVariationSelectors(text);
        }
    }
}

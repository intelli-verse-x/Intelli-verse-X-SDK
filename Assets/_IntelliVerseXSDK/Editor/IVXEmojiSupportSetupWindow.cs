#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IntelliVerseX.Core;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Provides setup and validation tools for production-ready TMP emoji rendering.
    /// </summary>
    public sealed class IVXEmojiSupportSetupWindow : EditorWindow
    {
        private const string WindowTitle = "IVX Emoji Support";
        private const string AutoFixPrefKey = "IVX_EmojiAutoFixTextureImport";
        private const int DefaultMaxTextureSize = 4096;
        private const string DefaultEmojiOneAssetPath = "Assets/TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset";
        private const string ImportedEmojiRootPath = "Assets/IntelliVerseX/Generated/Emoji";
        private const string QuizVerseAssetsRootPath = "C:/Office/Unity/intelliverse-x-games-platform-2/games/quiz-verse/Assets";
        private const string DefaultValidationSampleText = "Hello 😋 😍 😁";

        [SerializeField] private TMP_SpriteAsset _primarySpriteAsset;
        [SerializeField] private bool _includeFallbackAssets = true;
        [SerializeField] private bool _setAsDefaultSpriteAsset = true;
        [SerializeField] private bool _enableTMPEmojiSupport = true;
        [SerializeField] private bool _autoFixTextureImport = true;
        [SerializeField] private int _maxTextureSize = DefaultMaxTextureSize;
        [SerializeField] private string _sampleText = "Hello 😀 ❤️ 👨‍👩‍👧‍👦 1️⃣ 🇺🇸 🚀";

        private string _statusMessage = "Run validation to inspect current setup.";
        private MessageType _statusType = MessageType.Info;
        private string _conversionPreview = string.Empty;
        private Vector2 _scrollPosition;

        /// <summary>
        /// Opens the emoji support setup and validation window.
        /// </summary>
        [MenuItem("IntelliVerse-X SDK/Tools/Emoji/Setup & Validate", priority = 65)]
        public static void ShowWindow()
        {
            var window = GetWindow<IVXEmojiSupportSetupWindow>(WindowTitle);
            window.minSize = new Vector2(640f, 500f);
            window.Show();
        }

        /// <summary>
        /// Applies production-safe emoji setup using the built-in TMP EmojiOne sprite asset.
        /// </summary>
        [MenuItem("IntelliVerse-X SDK/Tools/Emoji/Apply Production Defaults", priority = 66)]
        public static void ApplyProductionDefaultsMenu()
        {
            TMP_SpriteAsset defaultAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(DefaultEmojiOneAssetPath);
            if (defaultAsset == null)
            {
                Debug.LogError($"[IVXEmojiSupportSetupWindow] Could not find default emoji sprite asset: {DefaultEmojiOneAssetPath}");
                return;
            }

            int updatedImporters = ApplyProductionSettingsForAsset(defaultAsset, includeFallbackAssets: true, maxTextureSize: DefaultMaxTextureSize);
            Debug.Log($"[IVXEmojiSupportSetupWindow] Applied emoji production defaults. Updated importers: {updatedImporters}.");
        }

        /// <summary>
        /// Validates emoji setup using the built-in TMP EmojiOne sprite asset.
        /// </summary>
        [MenuItem("IntelliVerse-X SDK/Tools/Emoji/Validate Production Defaults", priority = 67)]
        public static void ValidateProductionDefaultsMenu()
        {
            TMP_SpriteAsset defaultAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(DefaultEmojiOneAssetPath);
            if (defaultAsset == null)
            {
                Debug.LogError($"[IVXEmojiSupportSetupWindow] Could not find default emoji sprite asset: {DefaultEmojiOneAssetPath}");
                return;
            }

            List<string> issues = ValidateSetupForAsset(defaultAsset, includeFallbackAssets: true, sampleText: string.Empty);
            if (issues.Count == 0)
            {
                Debug.Log("[IVXEmojiSupportSetupWindow] Emoji production default validation passed.");
                return;
            }

            Debug.LogWarning("[IVXEmojiSupportSetupWindow] Emoji validation found issues: " + string.Join(" | ", issues));
        }

        /// <summary>
        /// Imports Twemoji/SidMoji sprite assets from the known quiz-verse path and wires fallback chain.
        /// </summary>
        // [MenuItem("IntelliVerse-X SDK/Tools/Emoji/Import From QuizVerse Assets", priority = 68)] // Disabled - QuizVerse specific
        public static void ImportFromQuizVerseAssetsMenu()
        {
            Directory.CreateDirectory(ImportedEmojiRootPath);

            string[] relativeFiles =
            {
                "_QuizVerse/Fonts/Twemoji/TwemojiAtlas_0.png",
                "_QuizVerse/Fonts/Twemoji/TwemojiAtlas_1.png",
                "_QuizVerse/Fonts/Twemoji/TwemojiCommon.png",
                "_QuizVerse/Fonts/Twemoji/TwemojiAtlas_0.png.meta",
                "_QuizVerse/Fonts/Twemoji/TwemojiAtlas_1.png.meta",
                "_QuizVerse/Fonts/Twemoji/TwemojiCommon.png.meta",
                "TextMesh Pro/Resources/Sprite Assets/Twemoji.asset",
                "TextMesh Pro/Resources/Sprite Assets/TwemojiCommon.asset",
                "TextMesh Pro/Resources/Sprite Assets/Twemoji_1.asset",
                "TextMesh Pro/Resources/Sprite Assets/Twemoji.asset.meta",
                "TextMesh Pro/Resources/Sprite Assets/TwemojiCommon.asset.meta",
                "TextMesh Pro/Resources/Sprite Assets/Twemoji_1.asset.meta",
                "_QuizVerse/Fonts/SidMoji/SidMojiAtlas_0.png",
                "_QuizVerse/Fonts/SidMoji/SidMojiAtlas_1.png",
                "_QuizVerse/Fonts/SidMoji/SidMojiCommon.png",
                "_QuizVerse/Fonts/SidMoji/SidMojiAtlas_0.png.meta",
                "_QuizVerse/Fonts/SidMoji/SidMojiAtlas_1.png.meta",
                "_QuizVerse/Fonts/SidMoji/SidMojiCommon.png.meta"
            };

            List<string> copied = new List<string>();
            List<string> missing = new List<string>();

            for (int i = 0; i < relativeFiles.Length; i++)
            {
                string sourcePath = Path.Combine(QuizVerseAssetsRootPath, relativeFiles[i]).Replace('\\', '/');
                string destinationPath = Path.Combine(ImportedEmojiRootPath, Path.GetFileName(relativeFiles[i])).Replace('\\', '/');
                CopyFileIfExists(sourcePath, destinationPath, copied, missing);
            }

            AssetDatabase.Refresh();

            TMP_SpriteAsset primary = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(DefaultEmojiOneAssetPath);
            List<TMP_SpriteAsset> importedFallbackAssets = FindImportedFallbackSpriteAssets();

            if (primary != null)
            {
                ConfigureFallbackChain(primary, importedFallbackAssets);
                ApplyProductionSettingsForAsset(primary, includeFallbackAssets: true, maxTextureSize: DefaultMaxTextureSize);
            }

            if (missing.Count > 0)
            {
                Debug.LogWarning("[IVXEmojiSupportSetupWindow] QuizVerse import completed with missing files: " + string.Join(" | ", missing));
            }

            Debug.Log(
                $"[IVXEmojiSupportSetupWindow] QuizVerse import complete. Copied: {copied.Count}, Missing: {missing.Count}, Fallbacks Added: {importedFallbackAssets.Count}.");
        }

        private void OnEnable()
        {
            _autoFixTextureImport = EditorPrefs.GetBool(AutoFixPrefKey, true);
            IVXEmojiTextureImportPostprocessor.SetAutoFixEnabled(_autoFixTextureImport);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("IntelliVerseX Emoji Support", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool validates and applies production-safe emoji settings for TextMeshPro sprite assets and atlas textures.",
                MessageType.Info);

            EditorGUILayout.Space(6f);
            _primarySpriteAsset = (TMP_SpriteAsset)EditorGUILayout.ObjectField(
                "Primary Sprite Asset",
                _primarySpriteAsset,
                typeof(TMP_SpriteAsset),
                false);

            _includeFallbackAssets = EditorGUILayout.Toggle(
                new GUIContent("Include Fallback Sprite Assets", "Recursively validates textures from fallback sprite assets too."),
                _includeFallbackAssets);

            _setAsDefaultSpriteAsset = EditorGUILayout.Toggle(
                new GUIContent("Set As TMP Default Sprite Asset", "Updates TMP Settings default sprite asset."),
                _setAsDefaultSpriteAsset);

            _enableTMPEmojiSupport = EditorGUILayout.Toggle(
                new GUIContent("Enable TMP Emoji Support", "Updates TMP Settings emoji support flag."),
                _enableTMPEmojiSupport);

            _maxTextureSize = EditorGUILayout.IntPopup(
                "Max Texture Size",
                _maxTextureSize,
                new[] { "2048", "4096", "8192" },
                new[] { 2048, 4096, 8192 });

            bool newAutoFix = EditorGUILayout.Toggle(
                new GUIContent("Auto-Fix Emoji Atlas Imports", "Keeps known emoji atlas textures uncompressed and non-mipmapped on import."),
                _autoFixTextureImport);
            if (newAutoFix != _autoFixTextureImport)
            {
                _autoFixTextureImport = newAutoFix;
                EditorPrefs.SetBool(AutoFixPrefKey, _autoFixTextureImport);
                IVXEmojiTextureImportPostprocessor.SetAutoFixEnabled(_autoFixTextureImport);
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate Setup", GUILayout.Height(30f)))
            {
                ValidateSetup();
            }

            if (GUILayout.Button("Apply Production Settings", GUILayout.Height(30f)))
            {
                ApplyProductionSettings();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Sample Conversion", EditorStyles.boldLabel);
            _sampleText = EditorGUILayout.TextField("Input Text", _sampleText);
            if (GUILayout.Button("Convert Sample Text", GUILayout.Height(24f)))
            {
                ConvertSampleText();
            }

            if (!string.IsNullOrEmpty(_conversionPreview))
            {
                EditorGUILayout.TextArea(_conversionPreview, GUILayout.MinHeight(48f));
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.HelpBox(_statusMessage, _statusType);

            EditorGUILayout.EndScrollView();
        }

        private void ValidateSetup()
        {
            if (_primarySpriteAsset == null)
            {
                _statusType = MessageType.Warning;
                _statusMessage = "Assign a Primary Sprite Asset before validation.";
                return;
            }

            List<string> issues = ValidateSetupForAsset(_primarySpriteAsset, _includeFallbackAssets, _sampleText);
            List<TMP_SpriteAsset> spriteAssets = CollectSpriteAssets(_primarySpriteAsset, _includeFallbackAssets);
            List<string> texturePaths = CollectTexturePaths(spriteAssets);

            if (issues.Count == 0)
            {
                _statusType = MessageType.Info;
                _statusMessage =
                    $"Validation passed. Checked {spriteAssets.Count} sprite assets and {texturePaths.Count} textures with no blocking issues.";
            }
            else
            {
                _statusType = MessageType.Warning;
                _statusMessage = $"Validation found {issues.Count} issue(s):\n- " + string.Join("\n- ", issues);
            }
        }

        private void ApplyProductionSettings()
        {
            if (_primarySpriteAsset == null)
            {
                _statusType = MessageType.Warning;
                _statusMessage = "Assign a Primary Sprite Asset before applying settings.";
                return;
            }

            int updatedImporters = ApplyProductionSettingsForAsset(_primarySpriteAsset, _includeFallbackAssets, _maxTextureSize);
            List<TMP_SpriteAsset> spriteAssets = CollectSpriteAssets(_primarySpriteAsset, _includeFallbackAssets);
            List<string> texturePaths = CollectTexturePaths(spriteAssets);

            _statusType = MessageType.Info;
            _statusMessage =
                $"Applied production settings. Updated {updatedImporters} texture importers across {texturePaths.Count} emoji textures.";
        }

        private void ConvertSampleText()
        {
            if (string.IsNullOrEmpty(_sampleText))
            {
                _conversionPreview = string.Empty;
                return;
            }

            _conversionPreview = _primarySpriteAsset == null
                ? IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(_sampleText)
                : IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(_sampleText, _primarySpriteAsset, _includeFallbackAssets);
        }

        private static int ApplyProductionSettingsForAsset(TMP_SpriteAsset spriteAsset, bool includeFallbackAssets, int maxTextureSize)
        {
            int updatedImporters = 0;
            List<TMP_SpriteAsset> spriteAssets = CollectSpriteAssets(spriteAsset, includeFallbackAssets);
            List<string> texturePaths = CollectTexturePaths(spriteAssets);

            for (int i = 0; i < texturePaths.Count; i++)
            {
                string path = texturePaths[i];
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                bool changed = IVXEmojiTextureImportSettings.Apply(importer, maxTextureSize);
                if (changed)
                {
                    importer.SaveAndReimport();
                    updatedImporters++;
                }
            }

            TMP_Settings tmpSettings = TMP_Settings.instance;
            if (tmpSettings != null)
            {
                SerializedObject settingsObject = new SerializedObject(tmpSettings);
                bool changed = false;

                SerializedProperty defaultSprite = settingsObject.FindProperty("m_defaultSpriteAsset");
                if (defaultSprite != null && defaultSprite.objectReferenceValue != spriteAsset)
                {
                    defaultSprite.objectReferenceValue = spriteAsset;
                    changed = true;
                }

                SerializedProperty emojiSupport = settingsObject.FindProperty("m_EnableEmojiSupport");
                if (emojiSupport != null && !emojiSupport.boolValue)
                {
                    emojiSupport.boolValue = true;
                    changed = true;
                }

                if (changed)
                {
                    settingsObject.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(tmpSettings);
                }
            }

            ConfigureFallbackChain(spriteAsset, FindImportedFallbackSpriteAssets());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return updatedImporters;
        }

        private static List<string> ValidateSetupForAsset(TMP_SpriteAsset spriteAsset, bool includeFallbackAssets, string sampleText)
        {
            List<string> issues = new List<string>();
            List<TMP_SpriteAsset> spriteAssets = CollectSpriteAssets(spriteAsset, includeFallbackAssets);
            List<string> texturePaths = CollectTexturePaths(spriteAssets);

            if (texturePaths.Count == 0)
            {
                issues.Add("No sprite sheet textures were found from the selected sprite assets.");
            }

            for (int i = 0; i < texturePaths.Count; i++)
            {
                string texturePath = texturePaths[i];
                TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer == null)
                {
                    issues.Add($"Texture importer missing: {texturePath}");
                    continue;
                }

                List<string> importerIssues = IVXEmojiTextureImportSettings.ValidateImporter(importer);
                for (int j = 0; j < importerIssues.Count; j++)
                {
                    issues.Add($"{texturePath}: {importerIssues[j]}");
                }
            }

            TMP_Settings tmpSettings = TMP_Settings.instance;
            if (tmpSettings == null)
            {
                issues.Add("TMP Settings asset was not found.");
            }
            else
            {
                SerializedObject settingsObject = new SerializedObject(tmpSettings);
                SerializedProperty defaultSprite = settingsObject.FindProperty("m_defaultSpriteAsset");
                SerializedProperty emojiSupport = settingsObject.FindProperty("m_EnableEmojiSupport");

                if (defaultSprite != null && defaultSprite.objectReferenceValue != spriteAsset)
                {
                    issues.Add("TMP default sprite asset is not set to the selected sprite asset.");
                }

                if (emojiSupport != null && !emojiSupport.boolValue)
                {
                    issues.Add("TMP emoji support is disabled in TMP Settings.");
                }
            }

            List<string> missing = IVXEmojiTextUtility.GetMissingSpriteNames(sampleText, spriteAsset, includeFallbackAssets);
            if (missing.Count > 0)
            {
                issues.Add($"Sample text has unsupported emoji sprite names: {string.Join(", ", missing)}");
            }

            return issues;
        }

        private static void ConfigureFallbackChain(TMP_SpriteAsset primary, List<TMP_SpriteAsset> fallbackCandidates)
        {
            if (primary == null || fallbackCandidates == null || fallbackCandidates.Count == 0)
            {
                return;
            }

            bool changed = false;
            if (primary.fallbackSpriteAssets == null)
            {
                primary.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
                changed = true;
            }

            for (int i = 0; i < fallbackCandidates.Count; i++)
            {
                TMP_SpriteAsset candidate = fallbackCandidates[i];
                if (candidate == null || candidate == primary)
                {
                    continue;
                }

                if (!primary.fallbackSpriteAssets.Contains(candidate))
                {
                    primary.fallbackSpriteAssets.Add(candidate);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(primary);
            }
        }

        private static List<TMP_SpriteAsset> FindImportedFallbackSpriteAssets()
        {
            List<TMP_SpriteAsset> results = new List<TMP_SpriteAsset>();
            string[] guids = AssetDatabase.FindAssets("t:TMP_SpriteAsset", new[] { ImportedEmojiRootPath });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TMP_SpriteAsset asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(path);
                if (asset != null)
                {
                    results.Add(asset);
                }
            }

            return results
                .OrderBy(a => a.name.Contains("Common", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(a => a.name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void CopyFileIfExists(string sourcePath, string destinationPath, List<string> copied, List<string> missing)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    missing.Add(sourcePath);
                    return;
                }

                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(sourcePath, destinationPath, overwrite: true);
                copied.Add(Path.GetFileName(destinationPath));
            }
            catch (Exception ex)
            {
                missing.Add($"{sourcePath} ({ex.Message})");
            }
        }

        private static List<TMP_SpriteAsset> CollectSpriteAssets(TMP_SpriteAsset primary, bool includeFallbacks)
        {
            List<TMP_SpriteAsset> assets = new List<TMP_SpriteAsset>();
            if (primary == null)
            {
                return assets;
            }

            HashSet<int> visited = new HashSet<int>();
            CollectSpriteAssetsRecursive(primary, includeFallbacks, visited, assets);
            return assets;
        }

        private static void CollectSpriteAssetsRecursive(
            TMP_SpriteAsset current,
            bool includeFallbacks,
            HashSet<int> visited,
            List<TMP_SpriteAsset> result)
        {
            if (current == null)
            {
                return;
            }

            int id = current.GetInstanceID();
            if (visited.Contains(id))
            {
                return;
            }

            visited.Add(id);
            result.Add(current);

            if (!includeFallbacks || current.fallbackSpriteAssets == null)
            {
                return;
            }

            for (int i = 0; i < current.fallbackSpriteAssets.Count; i++)
            {
                CollectSpriteAssetsRecursive(current.fallbackSpriteAssets[i], includeFallbacks, visited, result);
            }
        }

        private static List<string> CollectTexturePaths(List<TMP_SpriteAsset> spriteAssets)
        {
            List<string> paths = new List<string>();
            HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < spriteAssets.Count; i++)
            {
                TMP_SpriteAsset asset = spriteAssets[i];
                if (asset == null || asset.spriteSheet == null)
                {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(asset.spriteSheet);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                if (unique.Add(path))
                {
                    paths.Add(path);
                }
            }

            return paths;
        }
    }

    internal static class IVXEmojiTextureImportSettings
    {
        private static readonly string[] Platforms = { "Android", "iPhone", "WebGL", "Standalone" };

        public static bool Apply(TextureImporter importer, int maxTextureSize)
        {
            if (importer == null)
            {
                return false;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.isReadable)
            {
                importer.isReadable = false;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (importer.maxTextureSize != maxTextureSize)
            {
                importer.maxTextureSize = maxTextureSize;
                changed = true;
            }

            if (!importer.sRGBTexture)
            {
                importer.sRGBTexture = true;
                changed = true;
            }

            for (int i = 0; i < Platforms.Length; i++)
            {
                TextureImporterPlatformSettings platform = importer.GetPlatformTextureSettings(Platforms[i]);
                bool platformChanged = false;
                if (!platform.overridden)
                {
                    platform.overridden = true;
                    platformChanged = true;
                }

                if (platform.maxTextureSize != maxTextureSize)
                {
                    platform.maxTextureSize = maxTextureSize;
                    platformChanged = true;
                }

                if (platform.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    platform.textureCompression = TextureImporterCompression.Uncompressed;
                    platformChanged = true;
                }

                if (platform.format != TextureImporterFormat.RGBA32)
                {
                    platform.format = TextureImporterFormat.RGBA32;
                    platformChanged = true;
                }

                if (platform.compressionQuality != 100)
                {
                    platform.compressionQuality = 100;
                    platformChanged = true;
                }

                if (platformChanged)
                {
                    importer.SetPlatformTextureSettings(platform);
                    changed = true;
                }
            }

            return changed;
        }

        public static List<string> ValidateImporter(TextureImporter importer)
        {
            List<string> issues = new List<string>();
            if (importer == null)
            {
                issues.Add("Missing texture importer.");
                return issues;
            }

            if (importer.textureType != TextureImporterType.Sprite)
            {
                issues.Add("textureType must be Sprite.");
            }

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                issues.Add("spriteImportMode must be Multiple.");
            }

            if (importer.mipmapEnabled)
            {
                issues.Add("mipmapEnabled should be disabled for emoji atlases.");
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                issues.Add("textureCompression should be Uncompressed.");
            }

            if (importer.filterMode != FilterMode.Bilinear)
            {
                issues.Add("filterMode should be Bilinear.");
            }

            for (int i = 0; i < Platforms.Length; i++)
            {
                TextureImporterPlatformSettings platform = importer.GetPlatformTextureSettings(Platforms[i]);
                if (!platform.overridden)
                {
                    issues.Add($"{Platforms[i]} override is disabled.");
                    continue;
                }

                if (platform.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    issues.Add($"{Platforms[i]} compression should be Uncompressed.");
                }
            }

            return issues;
        }

    }

    /// <summary>
    /// Enforces production-safe texture import settings for known IVX emoji atlases.
    /// </summary>
    internal sealed class IVXEmojiTextureImportPostprocessor : AssetPostprocessor
    {
        private const string AutoFixPrefKey = "IVX_EmojiAutoFixTextureImport";
        private const int AutoFixTextureSize = 4096;

        public static void SetAutoFixEnabled(bool enabled)
        {
            EditorPrefs.SetBool(AutoFixPrefKey, enabled);
        }

        private static bool IsAutoFixEnabled()
        {
            return EditorPrefs.GetBool(AutoFixPrefKey, true);
        }

        private void OnPreprocessTexture()
        {
            if (!IsAutoFixEnabled())
            {
                return;
            }

            string normalizedPath = assetPath.Replace('\\', '/').ToLowerInvariant();
            if (!IsKnownEmojiAtlasPath(normalizedPath))
            {
                return;
            }

            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = IVXEmojiTextureImportSettings.Apply(importer, AutoFixTextureSize);
            if (changed)
            {
                Debug.Log($"[IVXEmojiTextureImportPostprocessor] Applied emoji-safe import settings to: {assetPath}");
            }
        }

        private static bool IsKnownEmojiAtlasPath(string path)
        {
            string fileName = Path.GetFileName(path);
            if (fileName == null)
            {
                return false;
            }

            bool isKnownFileName =
                fileName.StartsWith("ivxemojiatlas_") ||
                fileName.StartsWith("ivxemojicommon") ||
                fileName.StartsWith("sidmojiatlas_") ||
                fileName.StartsWith("sidmojicommon");

            if (isKnownFileName)
            {
                return true;
            }

            return
                path.Contains("/intelliversex/emojiatlases/") ||
                path.Contains("/fonts/sidmoji/") ||
                path.Contains("/fonts/twemoji/");
        }
    }
}
#endif

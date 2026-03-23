// File: IVXLocalizedText.cs
// Purpose: Auto-updating TextMeshPro component for localized strings
// Package: IntelliVerseX.Localization
// Dependencies: TextMeshPro

using UnityEngine;
using TMPro;

namespace IntelliVerseX.Localization
{
    /// <summary>
    /// MonoBehaviour component that automatically updates TextMeshPro text when language changes.
    /// Attach to any TextMeshProUGUI component that needs localization.
    /// 
    /// Usage:
    ///   1. Attach to GameObject with TMP_Text component
    ///   2. Set localization key in Inspector
    ///   3. Text auto-updates when language changes
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("IntelliVerse/Localization/Localized Text")]
    public class IVXLocalizedText : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Localization Settings")]
        [Tooltip("Localization key for this text (e.g., 'Button_Play')")]
        [SerializeField] private string localizationKey;

        [Tooltip("Default text if localization key not found")]
        [SerializeField] private string fallbackText = "";

        [Tooltip("Use formatted string with arguments (e.g., 'Score: {0}')")]
        [SerializeField] private bool useFormatting = false;

        [Tooltip("Format arguments (only used if useFormatting is true)")]
        [SerializeField] private string[] formatArgs = new string[0];

        [Header("Auto-Update Settings")]
        [Tooltip("Update text on Start()")]
        [SerializeField] private bool updateOnStart = true;

        [Tooltip("Update text when language changes")]
        [SerializeField] private bool updateOnLanguageChange = true;

        #endregion

        #region Private Fields

        private TMP_Text _textComponent;
        private string _currentKey;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();

            if (_textComponent == null)
            {
                Debug.LogError($"[IVXLocalizedText] TMP_Text component not found on {gameObject.name}");
                enabled = false;
                return;
            }

            // Use current text as fallback if not set
            if (string.IsNullOrEmpty(fallbackText))
            {
                fallbackText = _textComponent.text;
            }
        }

        private void Start()
        {
            if (updateOnStart)
            {
                UpdateText();
            }
        }

        private void OnEnable()
        {
            if (updateOnLanguageChange)
            {
                IVXLocalizationService.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void OnDisable()
        {
            if (updateOnLanguageChange)
            {
                IVXLocalizationService.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        #endregion

        #region Event Handlers

        private void OnLanguageChanged(string newLanguage)
        {
            UpdateText();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set localization key and update text
        /// </summary>
        public void SetKey(string key)
        {
            localizationKey = key;
            _currentKey = key;
            UpdateText();
        }

        /// <summary>
        /// Set localization key with format arguments
        /// </summary>
        public void SetKeyFormatted(string key, params object[] args)
        {
            localizationKey = key;
            _currentKey = key;
            useFormatting = true;

            // Convert args to string array
            formatArgs = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                formatArgs[i] = args[i]?.ToString() ?? "";
            }

            UpdateText();
        }

        /// <summary>
        /// Update text from current localization key
        /// </summary>
        public void UpdateText()
        {
            if (_textComponent == null)
            {
                Debug.LogError($"[IVXLocalizedText] TMP_Text component is null on {gameObject.name}");
                return;
            }

            if (string.IsNullOrEmpty(localizationKey))
            {
                Debug.LogWarning($"[IVXLocalizedText] Localization key is empty on {gameObject.name}");
                _textComponent.text = fallbackText;
                return;
            }

            // Get localized string
            string localizedText = GetLocalizedString();

            // Apply to text component
            _textComponent.text = localizedText;
            _currentKey = localizationKey;
        }

        /// <summary>
        /// Refresh text (useful for format args changes)
        /// </summary>
        public void Refresh()
        {
            UpdateText();
        }

        #endregion

        #region Private Methods

        private string GetLocalizedString()
        {
            if (IVXLocalizationService.Instance == null)
            {
                Debug.LogWarning($"[IVXLocalizedText] IVXLocalizationService not initialized, using fallback for key: {localizationKey}");
                return fallbackText;
            }

            if (useFormatting && formatArgs != null && formatArgs.Length > 0)
            {
                // Get formatted string
                return IVXLocalizationService.Instance.GetStringFormatted(localizationKey, formatArgs);
            }
            else
            {
                // Get simple string
                return IVXLocalizationService.Instance.GetString(localizationKey, fallbackText);
            }
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Validate localization key in editor
        /// </summary>
        private void OnValidate()
        {
            // Auto-update text in editor when key changes
            if (!string.IsNullOrEmpty(localizationKey) && localizationKey != _currentKey && Application.isPlaying)
            {
                UpdateText();
            }
        }
#endif

        #endregion
    }
}

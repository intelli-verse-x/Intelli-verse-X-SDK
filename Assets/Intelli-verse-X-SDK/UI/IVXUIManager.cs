// IVXUIManager.cs
// IntelliVerseX SDK UI Manager Base Class
// Generic UI management system for all games in the platform
// Handles panel transitions, popups, loading screens, and navigation

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Base class for game UI management
    /// Provides panel management, popup system, navigation history, and event handling
    /// 
    /// Usage:
    /// 1. Create game-specific class: public class QuizVerseUIManager : IVXUIManager<QuizVerseUIManager>
    /// 2. Register panels in Inspector or code
    /// 3. Use ShowPanel(name) / HidePanel(name) for navigation
    /// 4. Use ShowPopup(name) / HidePopup() for modal dialogs
    /// 5. Subscribe to OnPanelChanged / OnPopupOpened for tracking
    /// 
    /// Generic Pattern:
    /// The generic <T> allows for type-safe singleton access:
    /// QuizVerseUIManager.Instance instead of casting IVXUIManager.Instance
    /// </summary>
    public abstract class IVXUIManager<T> : MonoBehaviour where T : IVXUIManager<T>
    {
        #region Singleton Pattern
        
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<T>();
#else
                    _instance = FindObjectOfType<T>();
#endif
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("UI Configuration")]
        [SerializeField] protected string defaultPanelName = "Home";
        [SerializeField] protected bool trackNavigationHistory = true;
        [SerializeField] protected int maxHistorySize = 10;
        
        /// <summary>
        /// Override for custom log prefix
        /// </summary>
        protected virtual string GetLogPrefix() => "[IVX-UI]";
        
        #endregion
        
        #region State
        
        /// <summary>
        /// Navigation history for back button functionality
        /// </summary>
        protected Stack<string> navigationHistory = new Stack<string>();
        
        /// <summary>
        /// Currently active panel
        /// </summary>
        protected GameObject currentPanel;
        
        /// <summary>
        /// Currently active popup
        /// </summary>
        protected GameObject currentPopup;
        
        /// <summary>
        /// Panel registry (name -> GameObject)
        /// Override RegisterPanels() to populate this
        /// </summary>
        protected Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        
        /// <summary>
        /// Popup registry (name -> GameObject)
        /// Override RegisterPopups() to populate this
        /// </summary>
        protected Dictionary<string, GameObject> popups = new Dictionary<string, GameObject>();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Fired when panel changes (previousPanel, newPanel)
        /// </summary>
        public event Action<string, string> OnPanelChanged;
        
        /// <summary>
        /// Fired when popup opens
        /// </summary>
        public event Action<string> OnPopupOpened;
        
        /// <summary>
        /// Fired when popup closes
        /// </summary>
        public event Action<string> OnPopupClosed;
        
        /// <summary>
        /// Fired when navigating back in history
        /// </summary>
        public event Action<string> OnNavigatedBack;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Name of currently active panel
        /// </summary>
        public string CurrentPanelName { get; protected set; }
        
        /// <summary>
        /// Is a popup currently showing?
        /// </summary>
        public bool IsPopupActive => currentPopup != null && currentPopup.activeSelf;
        
        /// <summary>
        /// Can navigate back in history?
        /// </summary>
        public bool CanGoBack => navigationHistory.Count > 0;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            // Register panels and popups
            RegisterPanels();
            RegisterPopups();
        }
        
        protected virtual void Start()
        {
            // Show default panel
            if (!string.IsNullOrEmpty(defaultPanelName))
            {
                ShowPanel(defaultPanelName);
            }
        }
        
        #endregion
        
        #region Registration (Override in Child Classes)
        
        /// <summary>
        /// Override to register game panels
        /// Example: panels["Home"] = homePanel;
        /// </summary>
        protected abstract void RegisterPanels();
        
        /// <summary>
        /// Override to register game popups
        /// Example: popups["Settings"] = settingsPopup;
        /// </summary>
        protected virtual void RegisterPopups() { }
        
        #endregion
        
        #region Panel Management
        
        /// <summary>
        /// Show panel by name, hide current panel
        /// </summary>
        public virtual bool ShowPanel(string panelName, bool addToHistory = true)
        {
            if (!panels.TryGetValue(panelName, out GameObject panel))
            {
                Debug.LogError($"{GetLogPrefix()} Panel '{panelName}' not found!");
                return false;
            }
            
            // Track navigation history
            if (trackNavigationHistory && addToHistory && currentPanel != null)
            {
                navigationHistory.Push(CurrentPanelName);
                
                // Limit history size
                if (navigationHistory.Count > maxHistorySize)
                {
                    var temp = new Stack<string>();
                    for (int i = 0; i < maxHistorySize; i++)
                    {
                        if (navigationHistory.Count > 0)
                            temp.Push(navigationHistory.Pop());
                    }
                    navigationHistory.Clear();
                    while (temp.Count > 0)
                    {
                        navigationHistory.Push(temp.Pop());
                    }
                }
            }
            
            // Hide current panel
            string previousPanelName = CurrentPanelName;
            if (currentPanel != null)
            {
                currentPanel.SetActive(false);
            }
            
            // Show new panel
            currentPanel = panel;
            CurrentPanelName = panelName;
            currentPanel.SetActive(true);
            
            // Fire event
            OnPanelChanged?.Invoke(previousPanelName, panelName);
            
            Debug.Log($"{GetLogPrefix()} Panel changed: {previousPanelName} → {panelName}");
            return true;
        }
        
        /// <summary>
        /// Hide panel by name
        /// </summary>
        public virtual bool HidePanel(string panelName)
        {
            if (!panels.TryGetValue(panelName, out GameObject panel))
            {
                Debug.LogError($"{GetLogPrefix()} Panel '{panelName}' not found!");
                return false;
            }
            
            panel.SetActive(false);
            
            if (currentPanel == panel)
            {
                currentPanel = null;
                CurrentPanelName = null;
            }
            
            return true;
        }
        
        /// <summary>
        /// Hide current panel
        /// </summary>
        public virtual void HideCurrentPanel()
        {
            if (currentPanel != null)
            {
                currentPanel.SetActive(false);
                currentPanel = null;
                CurrentPanelName = null;
            }
        }
        
        #endregion
        
        #region Popup Management
        
        /// <summary>
        /// Show popup by name (modal dialog)
        /// </summary>
        public virtual bool ShowPopup(string popupName)
        {
            if (!popups.TryGetValue(popupName, out GameObject popup))
            {
                Debug.LogError($"{GetLogPrefix()} Popup '{popupName}' not found!");
                return false;
            }
            
            // Hide current popup if any
            HidePopup();
            
            // Show new popup
            currentPopup = popup;
            currentPopup.SetActive(true);
            
            // Fire event
            OnPopupOpened?.Invoke(popupName);
            
            Debug.Log($"{GetLogPrefix()} Popup opened: {popupName}");
            return true;
        }
        
        /// <summary>
        /// Hide current popup
        /// </summary>
        public virtual void HidePopup()
        {
            if (currentPopup != null)
            {
                string popupName = GetPopupName(currentPopup);
                currentPopup.SetActive(false);
                currentPopup = null;
                
                // Fire event
                OnPopupClosed?.Invoke(popupName);
                
                Debug.Log($"{GetLogPrefix()} Popup closed: {popupName}");
            }
        }
        
        private string GetPopupName(GameObject popup)
        {
            foreach (var kvp in popups)
            {
                if (kvp.Value == popup)
                    return kvp.Key;
            }
            return "Unknown";
        }
        
        #endregion
        
        #region Navigation
        
        /// <summary>
        /// Navigate back to previous panel in history
        /// </summary>
        public virtual bool GoBack()
        {
            if (!CanGoBack)
            {
                Debug.LogWarning($"{GetLogPrefix()} No navigation history");
                return false;
            }
            
            string previousPanel = navigationHistory.Pop();
            ShowPanel(previousPanel, addToHistory: false);
            
            OnNavigatedBack?.Invoke(previousPanel);
            
            Debug.Log($"{GetLogPrefix()} Navigated back to: {previousPanel}");
            return true;
        }
        
        /// <summary>
        /// Clear navigation history
        /// </summary>
        public virtual void ClearHistory()
        {
            navigationHistory.Clear();
            Debug.Log($"{GetLogPrefix()} Navigation history cleared");
        }
        
        /// <summary>
        /// Go to home/default panel and clear history
        /// </summary>
        public virtual void GoHome()
        {
            ClearHistory();
            ShowPanel(defaultPanelName, addToHistory: false);
        }
        
        #endregion
        
        #region Helpers
        
        /// <summary>
        /// Check if panel exists
        /// </summary>
        public bool HasPanel(string panelName)
        {
            return panels.ContainsKey(panelName);
        }
        
        /// <summary>
        /// Check if popup exists
        /// </summary>
        public bool HasPopup(string popupName)
        {
            return popups.ContainsKey(popupName);
        }
        
        /// <summary>
        /// Get panel GameObject by name
        /// </summary>
        public GameObject GetPanel(string panelName)
        {
            return panels.TryGetValue(panelName, out GameObject panel) ? panel : null;
        }
        
        /// <summary>
        /// Get popup GameObject by name
        /// </summary>
        public GameObject GetPopup(string popupName)
        {
            return popups.TryGetValue(popupName, out GameObject popup) ? popup : null;
        }
        
        #endregion
        
        #region Debug Utilities
        
        /// <summary>
        /// Log registered panels
        /// </summary>
        [ContextMenu("Log Registered Panels")]
        public void LogRegisteredPanels()
        {
            Debug.Log($"{GetLogPrefix()} Registered Panels ({panels.Count}):");
            foreach (var kvp in panels)
            {
                Debug.Log($"{GetLogPrefix()}   {kvp.Key} → {kvp.Value?.name}");
            }
        }
        
        /// <summary>
        /// Log registered popups
        /// </summary>
        [ContextMenu("Log Registered Popups")]
        public void LogRegisteredPopups()
        {
            Debug.Log($"{GetLogPrefix()} Registered Popups ({popups.Count}):");
            foreach (var kvp in popups)
            {
                Debug.Log($"{GetLogPrefix()}   {kvp.Key} → {kvp.Value?.name}");
            }
        }
        
        /// <summary>
        /// Log navigation history
        /// </summary>
        [ContextMenu("Log Navigation History")]
        public void LogNavigationHistory()
        {
            Debug.Log($"{GetLogPrefix()} Navigation History ({navigationHistory.Count} entries):");
            int i = navigationHistory.Count;
            foreach (var panelName in navigationHistory)
            {
                Debug.Log($"{GetLogPrefix()}   [{i--}] {panelName}");
            }
        }
        
        #endregion
    }
}

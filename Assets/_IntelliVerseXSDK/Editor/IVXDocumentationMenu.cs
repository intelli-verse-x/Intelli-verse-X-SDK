using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Provides menu items for accessing IntelliVerseX SDK documentation.
    /// </summary>
    public static class IVXDocumentationMenu
    {
        #region Constants
        
        private const string DOCS_URL = "https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/";
        private const string QUICKSTART_URL = DOCS_URL + "getting-started/quickstart/";
        private const string API_REFERENCE_URL = DOCS_URL + "api/core/";
        private const string TROUBLESHOOTING_URL = DOCS_URL + "troubleshooting/faq/";
        private const string CHANGELOG_URL = DOCS_URL + "changelog/";
        private const string GITHUB_URL = "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK";
        private const string ISSUES_URL = GITHUB_URL + "/issues";
        
        #endregion

        #region Menu Items
        
        /// <summary>
        /// Opens the main documentation website.
        /// </summary>
        [MenuItem("IntelliVerseX/📖 Open Documentation", false, 0)]
        public static void OpenDocumentation()
        {
            Application.OpenURL(DOCS_URL);
            Debug.Log("[IVX] Opened documentation: " + DOCS_URL);
        }
        
        /// <summary>
        /// Opens the Quick Start guide.
        /// </summary>
        [MenuItem("IntelliVerseX/Documentation/Quick Start Guide", false, 100)]
        public static void OpenQuickStart()
        {
            Application.OpenURL(QUICKSTART_URL);
        }
        
        /// <summary>
        /// Opens the API Reference documentation.
        /// </summary>
        [MenuItem("IntelliVerseX/Documentation/API Reference", false, 101)]
        public static void OpenAPIReference()
        {
            Application.OpenURL(API_REFERENCE_URL);
        }
        
        /// <summary>
        /// Opens the Troubleshooting guide.
        /// </summary>
        [MenuItem("IntelliVerseX/Documentation/Troubleshooting", false, 102)]
        public static void OpenTroubleshooting()
        {
            Application.OpenURL(TROUBLESHOOTING_URL);
        }
        
        /// <summary>
        /// Opens the Changelog.
        /// </summary>
        [MenuItem("IntelliVerseX/Documentation/Changelog", false, 103)]
        public static void OpenChangelog()
        {
            Application.OpenURL(CHANGELOG_URL);
        }
        
        [MenuItem("IntelliVerseX/Documentation/", false, 199)]
        public static void Separator() { }
        
        /// <summary>
        /// Opens the GitHub repository.
        /// </summary>
        [MenuItem("IntelliVerseX/GitHub Repository", false, 200)]
        public static void OpenGitHub()
        {
            Application.OpenURL(GITHUB_URL);
        }
        
        /// <summary>
        /// Opens the GitHub Issues page for reporting bugs.
        /// </summary>
        [MenuItem("IntelliVerseX/Report Issue", false, 201)]
        public static void ReportIssue()
        {
            Application.OpenURL(ISSUES_URL);
        }
        
        #endregion
        
        #region Help Menu Integration
        
        /// <summary>
        /// Also adds documentation to Help menu for discoverability.
        /// </summary>
        [MenuItem("Help/IntelliVerseX Documentation", false, 1000)]
        public static void OpenDocumentationFromHelp()
        {
            OpenDocumentation();
        }
        
        #endregion
    }
}

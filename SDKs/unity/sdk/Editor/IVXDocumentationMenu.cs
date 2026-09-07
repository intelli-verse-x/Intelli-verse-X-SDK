using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Maintainer documentation shortcuts (revamp P3 — not first-run).
    /// </summary>
    public static class IVXDocumentationMenu
    {
        private const string DocsUrl = "https://intelli-verse-x.github.io/Intelli-verse-X-SDK/";
        private const string QuickstartUrl = DocsUrl + "getting-started/quickstart/";
        private const string ApiReferenceUrl = DocsUrl + "api/core/";
        private const string TroubleshootingUrl = DocsUrl + "troubleshooting/faq/";
        private const string ChangelogUrl = DocsUrl + "changelog/";
        private const string GithubUrl = "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK";
        private const string IssuesUrl = GithubUrl + "/issues";

        [MenuItem("IntelliVerseX/Maintainers/Documentation/Open Site", false, 520)]
        public static void OpenDocumentation()
        {
            Application.OpenURL(DocsUrl);
        }

        [MenuItem("IntelliVerseX/Maintainers/Documentation/Quick Start Guide", false, 521)]
        public static void OpenQuickStart()
        {
            Application.OpenURL(QuickstartUrl);
        }

        [MenuItem("IntelliVerseX/Maintainers/Documentation/API Reference", false, 522)]
        public static void OpenAPIReference()
        {
            Application.OpenURL(ApiReferenceUrl);
        }

        [MenuItem("IntelliVerseX/Maintainers/Documentation/Troubleshooting", false, 523)]
        public static void OpenTroubleshooting()
        {
            Application.OpenURL(TroubleshootingUrl);
        }

        [MenuItem("IntelliVerseX/Maintainers/Documentation/Changelog", false, 524)]
        public static void OpenChangelog()
        {
            Application.OpenURL(ChangelogUrl);
        }

        [MenuItem("IntelliVerseX/Maintainers/Documentation/GitHub Repository", false, 530)]
        public static void OpenGitHub()
        {
            Application.OpenURL(GithubUrl);
        }

        [MenuItem("IntelliVerseX/Maintainers/Documentation/Report Issue", false, 531)]
        public static void ReportIssue()
        {
            Application.OpenURL(IssuesUrl);
        }

        [MenuItem("Help/IntelliVerseX Documentation", false, 1000)]
        public static void OpenDocumentationFromHelp()
        {
            OpenDocumentation();
        }
    }
}

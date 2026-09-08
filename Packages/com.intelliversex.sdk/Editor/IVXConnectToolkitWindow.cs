using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// UI Toolkit companion for Connect onboarding. The IMGUI Control Center remains the
    /// primary window; this surface mirrors the same Auth V2 → unique App ID story with UITK.
    /// </summary>
    public sealed class IVXConnectToolkitWindow : EditorWindow
    {
        private const string WindowTitle = "IVX Connect";

        [MenuItem("IntelliVerseX/Connect (UI Toolkit)", false, -9)]
        public static void ShowWindow()
        {
            var window = GetWindow<IVXConnectToolkitWindow>(WindowTitle);
            window.minSize = new Vector2(420, 520);
            window.Show();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 14;
            root.style.paddingRight = 14;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;

            var title = new Label("IntelliVerseX Connect");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16;
            title.style.marginBottom = 4;
            root.Add(title);

            var subtitle = new Label(
                "Sign in with Auth V2, create a unique App ID, and write it to Bootstrap Config. " +
                "Prefer Control Center for the full Check → Connect → Play path.");
            subtitle.style.whiteSpace = WhiteSpace.Normal;
            subtitle.style.marginBottom = 12;
            root.Add(subtitle);

            var steps = new VisualElement();
            steps.style.flexDirection = FlexDirection.Column;
            steps.style.marginBottom = 12;
            steps.Add(MakeStep("1", "Open Control Center (recommended) or continue here"));
            steps.Add(MakeStep("2", "Sign in with your IntelliVerse email / password"));
            steps.Add(MakeStep("3", "Enter a game name → create unique App ID"));
            steps.Add(MakeStep("4", "Press Play — Traffic tab shows live RPCs"));
            root.Add(steps);

            var openCc = new Button(() => IVXControlCenter.ShowWindowFocusConnect())
            {
                text = "Open Control Center → Connect"
            };
            openCc.style.height = 34;
            openCc.style.marginBottom = 8;
            root.Add(openCc);

            var developers = new Button(() => Application.OpenURL(IVXConnectWizardValidation.DevelopersUrl))
            {
                text = "Developers portal"
            };
            developers.style.height = 28;
            developers.style.marginBottom = 8;
            root.Add(developers);

            var signup = new Button(() => Application.OpenURL(IVXConnectWizardValidation.SignupUrl))
            {
                text = "Create account"
            };
            signup.style.height = 28;
            signup.style.marginBottom = 8;
            root.Add(signup);

            var forgot = new Button(() => Application.OpenURL(IVXConnectWizardValidation.ForgotPasswordUrl))
            {
                text = "Forgot password"
            };
            forgot.style.height = 28;
            forgot.style.marginBottom = 12;
            root.Add(forgot);

            var note = new HelpBox(
                "Cloud “list my games” and org switcher APIs are not published yet. " +
                "Control Center keeps recent Game IDs locally and shows Cognito roles from your JWT.",
                HelpBoxMessageType.Info);
            root.Add(note);
        }

        private static VisualElement MakeStep(string index, string text)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 6;
            var badge = new Label(index);
            badge.style.width = 22;
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            var label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.flexGrow = 1;
            row.Add(badge);
            row.Add(label);
            return row;
        }
    }
}

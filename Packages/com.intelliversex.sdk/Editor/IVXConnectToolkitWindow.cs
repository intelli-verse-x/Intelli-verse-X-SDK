using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Legacy menu entry — opens the full UI Toolkit Control Center on Connect.
    /// </summary>
    public sealed class IVXConnectToolkitWindow : EditorWindow
    {
        [MenuItem("IntelliVerseX/Connect (UI Toolkit)", false, -9)]
        public static void ShowWindow()
        {
            IVXControlCenter.ShowWindowFocusConnect();
        }

        public void CreateGUI()
        {
            // If an old docked instance is restored, bounce to Control Center.
            rootVisualElement.Clear();
            var note = new Label("Redirecting to IntelliVerseX Control Center…");
            note.style.marginTop = 16;
            note.style.marginLeft = 12;
            note.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(note);
            EditorApplication.delayCall += () =>
            {
                Close();
                IVXControlCenter.ShowWindowFocusConnect();
            };
        }
    }
}

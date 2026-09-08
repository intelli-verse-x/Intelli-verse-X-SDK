using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Legacy docked window type. Menu removed — use Control Center only.
    /// Restored instances bounce to <see cref="IVXControlCenter"/>.
    /// </summary>
    public sealed class IVXConnectToolkitWindow : EditorWindow
    {
        // Hide stale menu until domain reload forgets the old item.
        [MenuItem("IntelliVerseX/Connect (UI Toolkit)", true)]
        private static bool ShowWindowValidate() => false;

        [MenuItem("IntelliVerseX/Connect (UI Toolkit)", false, -9)]
        private static void ShowWindowHidden()
        {
            IVXControlCenter.ShowWindowFocusConnect();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var note = new Label("This window was replaced by IntelliVerseX → Control Center.");
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

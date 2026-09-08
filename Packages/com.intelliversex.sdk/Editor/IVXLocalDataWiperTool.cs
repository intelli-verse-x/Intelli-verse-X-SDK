#if UNITY_EDITOR
using System;
using IntelliVerseX.Storage;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Editor-only tool to wipe SDK local data (secure session, remember-me, Nakama tokens, identity mirrors).
    /// Hotkey: Ctrl + Shift + K (Cmd + Shift + K on macOS).
    /// </summary>
    public static class IVXLocalDataWiperTool
    {
        [MenuItem("IntelliVerseX/Maintainers/Wipe Local SDK Data %#k", false, 540)]
        public static void WipeLocalSdkData()
        {
            if (!EditorUtility.DisplayDialog(
                "Wipe IntelliVerseX Local Data",
                "This clears IVX secure session storage, remember-me prefs, Nakama tokens, and identity mirrors.\n\n" +
                "It does NOT call PlayerPrefs.DeleteAll() (other plugins' prefs are preserved).",
                "Wipe IVX data", "Cancel"))
            {
                return;
            }

            string sessionPath = TryGetSessionPath();

            // Canonical wipe path — UserSessionManager + IVXLocalData key catalog.
            if (!TryInvokeStatic("UserSessionManager", "ClearAllLocalData")
                && !TryInvokeStatic("IntelliVerseX.Identity.IVXUserSession", "ClearAllLocalData"))
            {
                IVXLocalData.ClearAllRegisteredKeys();
                IVXLocalData.DeleteLegacySessionFile(sessionPath);
            }

            TryInvokeStatic("IntelliVerseX.Core.IntelliVerseXIdentity", "ClearUserData");

            Debug.Log($"[IVXLocalDataWiperTool] IVX local data wiped. LegacySessionPath={sessionPath}");

            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("IVX local data wiped"));
        }

        private static string TryGetSessionPath()
        {
            Type type = FindType("IntelliVerseX.Identity.IVXUserSession")
                        ?? FindType("UserSessionManager");
            if (type == null)
                return "<unknown>";

            var prop = type.GetProperty("SessionPath",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (prop == null)
                return "<unknown>";

            object value = prop.GetValue(null, null);
            return value?.ToString() ?? "<unknown>";
        }

        private static bool TryInvokeStatic(string typeName, string methodName)
        {
            Type type = FindType(typeName);
            if (type == null)
                return false;

            var method = type.GetMethod(methodName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
                return false;

            method.Invoke(null, null);
            return true;
        }

        private static Type FindType(string fullTypeName)
        {
            Type type = Type.GetType(fullTypeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}
#endif

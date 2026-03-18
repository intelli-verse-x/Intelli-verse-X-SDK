#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Editor-only tool to wipe local SDK data quickly during testing.
    /// Hotkey: Ctrl + Shift + K (Cmd + Shift + K on macOS).
    /// </summary>
    public static class IVXLocalDataWiperTool
    {
        [MenuItem("IntelliVerse-X SDK/Tools/Wipe Local SDK Data %#k", false, 350)]
        public static void WipeLocalSdkData()
        {
            string sessionPath = TryGetSessionPath();

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Clear in-memory and persisted session state if available.
            TryInvokeStatic("IntelliVerseX.Identity.IVXUserSession", "Clear");
            TryInvokeStatic("UserSessionManager", "Clear");
            TryInvokeStatic("IntelliVerseX.Identity.IntelliVerseXUserIdentity", "Clear");
            TryInvokeStatic("IntelliVerseX.Core.IntelliVerseXIdentity", "Clear");

            Debug.Log($"[IVXLocalDataWiperTool] Local SDK data wiped. SessionPath={sessionPath}");

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("IVX local data wiped"));
            }
        }

        private static string TryGetSessionPath()
        {
            Type type = FindType("IntelliVerseX.Identity.IVXUserSession");
            if (type == null)
            {
                return "<unknown>";
            }

            var prop = type.GetProperty("SessionPath", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (prop == null)
            {
                return "<unknown>";
            }

            object value = prop.GetValue(null, null);
            return value?.ToString() ?? "<unknown>";
        }

        private static bool TryInvokeStatic(string typeName, string methodName)
        {
            Type type = FindType(typeName);
            if (type == null)
            {
                return false;
            }

            var method = type.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
            {
                return false;
            }

            method.Invoke(null, null);
            return true;
        }

        private static Type FindType(string fullTypeName)
        {
            Type type = Type.GetType(fullTypeName);
            if (type != null)
            {
                return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullTypeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
#endif

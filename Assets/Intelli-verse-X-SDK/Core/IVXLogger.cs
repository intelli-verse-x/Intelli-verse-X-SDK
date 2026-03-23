// File: Assets/_IntelliVerseXSDK/Core/IVXLogger.cs
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Centralized logging utility for IntelliVerseX SDK.
    /// Supports log levels and conditional logging.
    /// </summary>
    public static class IVXLogger
    {
        public static bool EnableDebugLogs = true;

        public static void Log(string message, Object context = null)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[IVX] {message}", context);
            }
        }

        public static void LogWarning(string message, Object context = null)
        {
            Debug.LogWarning($"[IVX] {message}", context);
        }

        public static void LogError(string message, Object context = null)
        {
            Debug.LogError($"[IVX] {message}", context);
        }

        public static void LogException(System.Exception exception, Object context = null)
        {
            Debug.LogException(exception, context);
        }
    }
}

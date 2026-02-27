using System.IO;
using ApplixirSDK.Runtime;
using UnityEditor;
using UnityEngine;

namespace ApplixirSDK.Editor.CustomEditor
{
    [UnityEditor.CustomEditor(typeof(AppLixirAdsConfig))]
    public class AppLixirAdsConfigEditor : UnityEditor.Editor
    {
        private string _versionData;

        [MenuItem("Tools/ApplixirSDK/Applixir Ads Config")]
        private static void CreateApplixirAdsConfig()
        {
            ScriptableObjectUtility.CreateAssetAtPath<AppLixirAdsConfig>(
                "ApplixirSDK/Resources/ApplixirAdsConfig.asset");
        }

        public override void OnInspectorGUI()
        {
            AppLixirAdsConfig cfg = (AppLixirAdsConfig)target;
            RenderSdkData();
            EditorGUILayout.Separator();
            RenderSdkSettings(cfg);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            RenderDebugData(cfg);

        }

        private void RenderDebugData(AppLixirAdsConfig cfg)
        {
            GUILayout.Label("Debug Data", EditorStyles.boldLabel);

            cfg.debugPlayVideoResponse = (PlayVideoResult)EditorGUILayout.EnumPopup(
                new GUIContent("Debug Play Video Result", "Editor-only Debug response to play video call."),
                cfg.debugPlayVideoResponse);
        }

        private void RenderSdkData()
        {
            if (string.IsNullOrEmpty(_versionData))
            {
                _versionData = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ApplixirSDK/Editor/res/Version.txt")
                    ?.text;
            }

            GUILayout.Label(_versionData, EditorStyles.boldLabel);
        }

        private static void RenderSdkSettings(AppLixirAdsConfig cfg)
        {
            GUILayout.Label("Ads configuration", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            
            cfg.apiKey = EditorGUILayout.TextField(
                new GUIContent("API Key", "ApiKey from your applixir.com site settings in your account"),
                cfg.apiKey);

            cfg.logLevel = (LogLevel)EditorGUILayout.EnumPopup(
                new GUIContent("Log Level", "level of logs to use for the SDK. Remember to set to None before " +
                                            "release"),
                cfg.logLevel);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(cfg); // Mark the object as dirty
                AssetDatabase.SaveAssets();  // Save changes to disk
            }
        }
    }

    public static class ScriptableObjectUtility
    {
        /// <summary>
        /// Creates and saves a new ScriptableObject of the specified type at a given path.
        /// </summary>
        public static T CreateAssetAtPath<T>(string relativePath) where T : ScriptableObject
        {
            T asset;
            var path = Path.Combine(Application.dataPath, relativePath);
            Debug.Log(relativePath);
            if (!File.Exists(path))
            {
                asset = ScriptableObject.CreateInstance<T>();
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Debug.Log($"Assets/{relativePath}");
                AssetDatabase.CreateAsset(asset, $"Assets/{relativePath}");
                AssetDatabase.SaveAssets();
            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath<T>($"Assets/{relativePath}");
            }

            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return asset;
        }
    }
}
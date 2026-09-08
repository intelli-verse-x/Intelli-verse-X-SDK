using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Bootstrap.Editor
{
    /// <summary>
    /// Inspector for <see cref="IVXBootstrapConfig"/>.
    /// Game identity is always visible; Nakama host/key stay collapsed and the key is masked.
    /// </summary>
    [CustomEditor(typeof(IVXBootstrapConfig))]
    public sealed class IVXBootstrapConfigEditor : UnityEditor.Editor
    {
        private const string PrefShowBackend = "IVX.BootstrapConfig.ShowBackend";
        private const string PrefRevealKey = "IVX.BootstrapConfig.RevealServerKey";

        private SerializedProperty _gameId;
        private SerializedProperty _gameName;
        private SerializedProperty _serverHost;
        private SerializedProperty _serverPort;
        private SerializedProperty _serverKey;
        private SerializedProperty _useSSL;
        private SerializedProperty _autoDeviceAuth;
        private SerializedProperty _persistSession;
        private SerializedProperty _enableDebugLogs;

        private bool _showBackend;
        private bool _revealKey;

        private void OnEnable()
        {
            _gameId = serializedObject.FindProperty("_gameId");
            _gameName = serializedObject.FindProperty("_gameName");
            _serverHost = serializedObject.FindProperty("_serverHost");
            _serverPort = serializedObject.FindProperty("_serverPort");
            _serverKey = serializedObject.FindProperty("_serverKey");
            _useSSL = serializedObject.FindProperty("_useSSL");
            _autoDeviceAuth = serializedObject.FindProperty("_autoDeviceAuth");
            _persistSession = serializedObject.FindProperty("_persistSession");
            _enableDebugLogs = serializedObject.FindProperty("_debugLogging");
            _showBackend = EditorPrefs.GetBool(PrefShowBackend, false);
            _revealKey = EditorPrefs.GetBool(PrefRevealKey, false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Game Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_gameId, new GUIContent("Game ID"));
            EditorGUILayout.PropertyField(_gameName, new GUIContent("Game Name"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Session", EditorStyles.boldLabel);
            if (_autoDeviceAuth != null)
                EditorGUILayout.PropertyField(_autoDeviceAuth);
            if (_persistSession != null)
                EditorGUILayout.PropertyField(_persistSession);
            if (_enableDebugLogs != null)
                EditorGUILayout.PropertyField(_enableDebugLogs);

            EditorGUILayout.Space(12);
            bool show = EditorGUILayout.BeginFoldoutHeaderGroup(
                _showBackend,
                "Backend (Nakama) — maintainers only");
            if (show != _showBackend)
            {
                _showBackend = show;
                EditorPrefs.SetBool(PrefShowBackend, _showBackend);
            }

            if (_showBackend)
            {
                EditorGUILayout.HelpBox(
                    "Host, port, and server key are sensitive. Do not commit production keys to public repos. " +
                    "Most games use the default IntelliVerseX cloud backend — leave these alone unless you self-host.",
                    MessageType.Warning);

                EditorGUILayout.PropertyField(_serverHost, new GUIContent("Server host"));
                EditorGUILayout.PropertyField(_serverPort, new GUIContent("Server port"));

                EditorGUILayout.BeginHorizontal();
                if (_revealKey)
                    EditorGUILayout.PropertyField(_serverKey, new GUIContent("Server key"));
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField("Server key", MaskKey(_serverKey?.stringValue));
                    }
                }

                bool reveal = GUILayout.Toggle(_revealKey, "Reveal", "Button", GUILayout.Width(64));
                if (reveal != _revealKey)
                {
                    _revealKey = reveal;
                    EditorPrefs.SetBool(PrefRevealKey, _revealKey);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(_useSSL, new GUIContent("Use SSL"));
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private static string MaskKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "(empty)";
            if (key.Length <= 4)
                return new string('•', key.Length);
            return new string('•', Mathf.Max(8, key.Length - 4)) + key.Substring(key.Length - 4);
        }
    }
}

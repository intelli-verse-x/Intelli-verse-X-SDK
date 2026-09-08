using IntelliVerseX.Bootstrap;
using IntelliVerseX.Bootstrap.Editor;
using IntelliVerseX.Core;
using System;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Single advanced editor surface (revamp P3). First-run stays on <see cref="IVXControlCenter"/>.
    /// Dependencies, project checks, and optional extras — no parallel wizards.
    /// </summary>
    public sealed class IVXAdvancedSetup : EditorWindow
    {
        private const string WindowTitle = "Advanced Setup";
        private Vector2 _scroll;
        private int _tab;
        private string _backendPing;
        private MessageType _backendPingType = MessageType.None;
        private static readonly string[] TabNames = { "Dependencies", "Project", "Backend", "Extras" };

        [MenuItem("IntelliVerseX/Advanced Setup", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<IVXAdvancedSetup>(WindowTitle);
            window.minSize = new Vector2(520, 560);
            window.Show();
        }

        private void OnFocus()
        {
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Advanced Setup", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Optional modules and project hardening. Day-one path: Control Center → Game ID → Play.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);

            _tab = GUILayout.Toolbar(_tab, TabNames, GUILayout.Height(28));
            EditorGUILayout.Space(10);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case 0:
                    DrawDependencies();
                    break;
                case 1:
                    DrawProject();
                    break;
                case 2:
                    DrawBackend();
                    break;
                case 3:
                    DrawExtras();
                    break;
                default:
                    DrawDependencies();
                    break;
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Control Center", GUILayout.Height(28)))
                IVXControlCenter.ShowWindow();
            if (GUILayout.Button("Refresh", GUILayout.Height(28)))
                Repaint();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDependencies()
        {
            var status = IVXDependencies.GetStatus();

            EditorGUILayout.LabelField("Core", EditorStyles.boldLabel);
            DrawRow("Newtonsoft.Json", status.Newtonsoft);
            DrawRow("TextMeshPro", status.TextMeshPro);
            DrawRow("Nakama", status.Nakama);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
            DrawRow("Native Share", status.NativeShare);
            DrawRow("Unity Purchasing (IAP)", status.Purchasing);
            DrawRow("LevelPlay ads", status.LevelPlay);

            EditorGUILayout.Space(12);
            if (!status.AllCoreReady)
            {
                EditorGUILayout.HelpBox(
                    "Core dependencies incomplete. Install required UPM packages, then add Nakama from the Asset Store or GitHub if still missing.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("Core dependencies look ready.", MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Install required UPM", GUILayout.Height(32)))
                IVXDependencies.ForceRerunSetup();
            if (GUILayout.Button("Install optional UPM", GUILayout.Height(32)))
                IVXDependencies.InstallOptionalPackages();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Asset Store packages", GUILayout.Height(28)))
                IVXDependencies.OpenAssetStoreLinks();
            if (GUILayout.Button("Log full validation", GUILayout.Height(28)))
                IVXDependencies.ValidateAssembliesToConsole();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProject()
        {
            EditorGUILayout.LabelField("Project settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Runs the same checks Control Center uses (IL2CPP, networking, defines).",
                MessageType.None);

            var validation = IVXProjectSetup.RunValidation();
            int failed = 0;
            int warnings = 0;
            for (int i = 0; i < validation.Count; i++)
            {
                var item = validation[i];
                if (!item.Passed && !item.IsWarning)
                    failed++;
                else if (!item.Passed && item.IsWarning)
                    warnings++;
            }

            DrawRow("Hard failures", failed == 0);
            EditorGUILayout.LabelField("Warnings: " + warnings, EditorStyles.miniLabel);

            EditorGUILayout.Space(8);
            for (int i = 0; i < validation.Count; i++)
            {
                var item = validation[i];
                if (item.Passed)
                    continue;
                EditorGUILayout.HelpBox(
                    item.Name + ": " + item.Message,
                    item.IsWarning ? MessageType.Warning : MessageType.Error);
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Fix project settings", GUILayout.Height(32)))
            {
                IVXProjectSetup.QuickValidate();
                Repaint();
            }

            if (GUILayout.Button("Reapply define symbols", GUILayout.Height(28)))
            {
                IVXDefineSymbolManager.ForceReapplyDefines();
            }
        }

        private void DrawBackend()
        {
            EditorGUILayout.LabelField("Nakama connection (maintainers)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Hidden from Control Center on purpose. Most games use IntelliVerseX cloud defaults. " +
                "Only change these if you self-host Nakama. Never commit production server keys to public repos.",
                MessageType.Warning);

            string[] guids = AssetDatabase.FindAssets("t:IVXBootstrapConfig");
            if (guids.Length == 0)
            {
                EditorGUILayout.HelpBox("No IVXBootstrapConfig found. Create one from Control Center first.", MessageType.Info);
                return;
            }

            var cfg = AssetDatabase.LoadAssetAtPath<IVXBootstrapConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (cfg == null)
                return;

            var so = new SerializedObject(cfg);
            so.Update();

            EditorGUILayout.ObjectField("Config asset", cfg, typeof(IVXBootstrapConfig), false);
            EditorGUILayout.PropertyField(so.FindProperty("_serverHost"), new GUIContent("Server host"));
            EditorGUILayout.PropertyField(so.FindProperty("_serverPort"), new GUIContent("Server port"));

            var keyProp = so.FindProperty("_serverKey");
            string key = keyProp?.stringValue ?? "";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Server key");
            string edited = EditorGUILayout.PasswordField(key);
            if (edited != key && keyProp != null)
                keyProp.stringValue = edited;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(so.FindProperty("_useSSL"), new GUIContent("Use SSL"));

            if (so.ApplyModifiedProperties())
                EditorUtility.SetDirty(cfg);

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping server", GUILayout.Height(28)))
                PingBackend(cfg);
            if (GUILayout.Button("Reset to IntelliVerseX cloud", GUILayout.Height(28)))
            {
                so.FindProperty("_serverHost").stringValue = IVXNakamaConfig.HOST;
                so.FindProperty("_serverPort").intValue = IVXNakamaConfig.PORT;
                so.FindProperty("_serverKey").stringValue = IVXNakamaConfig.SERVER_KEY;
                so.FindProperty("_useSSL").boolValue = true;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(cfg);
                _backendPing = $"Reset to {IVXNakamaConfig.HOST}:{IVXNakamaConfig.PORT}.";
                _backendPingType = MessageType.Info;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Select config asset", GUILayout.Height(28)))
            {
                Selection.activeObject = cfg;
                EditorGUIUtility.PingObject(cfg);
            }

            if (!string.IsNullOrEmpty(_backendPing))
                EditorGUILayout.HelpBox(_backendPing, _backendPingType);

            EditorGUILayout.HelpBox(
                "Tip: In the Inspector, Backend fields stay collapsed and the key is masked until you click Reveal.",
                MessageType.None);
        }

        private void PingBackend(IVXBootstrapConfig cfg)
        {
            string host = cfg.ServerHost;
            int port = cfg.ServerPort;
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool ok = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (!ok)
                    {
                        _backendPing = $"No response from {host}:{port}. Start Nakama or check host/port.";
                        _backendPingType = MessageType.Warning;
                        return;
                    }

                    client.EndConnect(result);
                    _backendPing = $"Reached {host}:{port}.";
                    _backendPingType = MessageType.Info;
                }
            }
            catch (Exception ex)
            {
                _backendPing = $"Could not reach {host}:{port}. {ex.Message}";
                _backendPingType = MessageType.Warning;
            }
        }

        private void DrawExtras()
        {
            EditorGUILayout.LabelField("Samples & scene helpers", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Demo content is optional. Prefer Control Center → Add bootstrap for a live game scene.",
                MessageType.Info);

            if (GUILayout.Button("Install demo scenes", GUILayout.Height(32)))
                IVXConsumerAssetInstaller.InstallDemoScenesOnly();

            if (GUILayout.Button("Install prefabs only", GUILayout.Height(28)))
                IVXConsumerAssetInstaller.InstallPrefabsOnly();

            EditorGUILayout.Space(16);
            EditorGUILayout.LabelField("Local data", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Wipes PlayerPrefs and in-memory IVX session helpers. Dev/testing only.", MessageType.Warning);
            if (GUILayout.Button("Wipe local SDK data", GUILayout.Height(28)))
                IVXLocalDataWiperTool.WipeLocalSdkData();

            bool hasBootstrap = FindBootstrapInOpenScenes() != null;
            EditorGUILayout.Space(12);
            DrawRow("Bootstrap in open scenes", hasBootstrap);
            if (GUILayout.Button("Select bootstrap config", GUILayout.Height(28)))
            {
                string[] guids = AssetDatabase.FindAssets("t:IVXBootstrapConfig");
                if (guids.Length > 0)
                {
                    var cfg = AssetDatabase.LoadAssetAtPath<IVXBootstrapConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    Selection.activeObject = cfg;
                    EditorGUIUtility.PingObject(cfg);
                }
                else
                {
                    EditorUtility.DisplayDialog(WindowTitle, "No IVXBootstrapConfig found. Create one from Control Center.", "OK");
                }
            }
        }

        private static void DrawRow(string label, bool ok)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ok ? "Ready" : "Needs work", GUILayout.Width(88));
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
        }

        private static IVXBootstrap FindBootstrapInOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var found = root.GetComponentInChildren<IVXBootstrap>(true);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }
    }
}

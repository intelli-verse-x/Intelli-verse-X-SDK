using System;
using System.IO;
using System.Net.Sockets;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Canonical first-run editor window (Home / Traffic / APIs).
    /// Paste Game ID onto <see cref="IVXBootstrapConfig"/>; advanced work stays on <see cref="IVXAdvancedSetup"/>.
    /// </summary>
    public sealed class IVXControlCenter : EditorWindow
    {
        private enum Tab
        {
            Home = 0,
            Traffic = 1,
            Apis = 2
        }

        private const string WindowTitle = "IntelliVerseX";
        private const string GeneratedConfigFolder = "Assets/IntelliVerseX/Generated";
        private const string GeneratedConfigPath = GeneratedConfigFolder + "/IVXBootstrapConfig.asset";

        private static readonly string[] TabLabels = { "Home", "Traffic", "APIs" };

        private IVXBootstrapConfig _config;
        private SerializedObject _configSo;
        private Vector2 _scroll;
        private Vector2 _trafficScroll;
        private Vector2 _apiScroll;
        private string _serverPing = "";
        private MessageType _serverPingType = MessageType.None;
        private Tab _tab = Tab.Home;
        private string _apiFilter = "";
        private double _nextTrafficRepaint;

        [MenuItem("IntelliVerseX/Control Center", false, -10)]
        public static void ShowWindow()
        {
            var window = GetWindow<IVXControlCenter>(WindowTitle);
            window.minSize = new Vector2(560, 600);
            window.Show();
        }

        private void OnEnable()
        {
            FindOrLoadConfig();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnFocus()
        {
            FindOrLoadConfig();
        }

        private void OnEditorUpdate()
        {
            if (_tab != Tab.Traffic)
                return;
            if (EditorApplication.timeSinceStartup < _nextTrafficRepaint)
                return;
            _nextTrafficRepaint = EditorApplication.timeSinceStartup + 0.5d;
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            _tab = (Tab)GUILayout.Toolbar((int)_tab, TabLabels, GUILayout.Height(28));
            EditorGUILayout.Space(8);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case Tab.Home:
                    DrawHome();
                    break;
                case Tab.Traffic:
                    DrawTraffic();
                    break;
                case Tab.Apis:
                    DrawApis();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHome()
        {
            EditorGUILayout.LabelField("IntelliVerseX", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Three steps. Then press Play.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(12);

            DrawCheck();
            EditorGUILayout.Space(12);
            DrawConnect();
            EditorGUILayout.Space(12);
            DrawPlay();
            EditorGUILayout.Space(16);
            DrawFooter();
        }

        private void DrawTraffic()
        {
            EditorGUILayout.LabelField("Traffic", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Live IVXRequestBus activity (last " + IVXRequestBus.MaxTrafficHistory +
                "). In-flight: " + IVXRequestBus.GetInFlightCount() + ".",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear history", GUILayout.Height(24)))
                IVXRequestBus.ClearTrafficHistory();
            if (GUILayout.Button("Refresh", GUILayout.Height(24)))
                Repaint();
            EditorGUILayout.EndHorizontal();

            var rows = IVXRequestBus.GetRecentTraffic();
            if (rows.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No RPCs recorded yet. Press Play or call managers that use IVXRequestBus.",
                    MessageType.None);
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("UTC", GUILayout.Width(70));
            GUILayout.Label("RPC", GUILayout.Width(180));
            GUILayout.Label("#", GUILayout.Width(28));
            GUILayout.Label("Status", GUILayout.Width(70));
            GUILayout.Label("ms", GUILayout.Width(48));
            GUILayout.Label("Error / retry");
            EditorGUILayout.EndHorizontal();

            _trafficScroll = EditorGUILayout.BeginScrollView(_trafficScroll, GUILayout.MinHeight(280));
            for (int i = 0; i < rows.Length; i++)
            {
                var e = rows[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(e.Utc.ToString("HH:mm:ss"), GUILayout.Width(70));
                GUILayout.Label(e.RpcId ?? "", GUILayout.Width(180));
                GUILayout.Label(e.Attempt.ToString(), GUILayout.Width(28));
                GUILayout.Label(e.Status ?? "", GUILayout.Width(70));
                GUILayout.Label(e.LatencyMs.ToString(), GUILayout.Width(48));
                string detail = e.Error ?? "";
                if (e.RetryAfterMs.HasValue)
                    detail = (string.IsNullOrEmpty(detail) ? "" : detail + " · ") + "retry_after_ms=" + e.RetryAfterMs.Value;
                GUILayout.Label(detail);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawApis()
        {
            EditorGUILayout.LabelField("APIs", EditorStyles.boldLabel);
            var entries = IVXRpcIndex.GetEntries();
            string source = IVXRpcIndex.LoadedFrom;
            EditorGUILayout.HelpBox(
                entries.Length > 0
                    ? "Generated RPC index (" + entries.Length + " ids). Filter by name or module."
                    : "RPC index missing — run tools/generate-rpc-index.ps1 from the repo root.",
                entries.Length > 0 ? MessageType.Info : MessageType.Warning);

            if (!string.IsNullOrEmpty(source))
                EditorGUILayout.LabelField("Source", source, EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            _apiFilter = EditorGUILayout.TextField("Filter", _apiFilter);
            if (GUILayout.Button("Reload", GUILayout.Width(64)))
            {
                IVXRpcIndex.Invalidate();
                entries = IVXRpcIndex.GetEntries();
            }
            EditorGUILayout.EndHorizontal();

            _apiScroll = EditorGUILayout.BeginScrollView(_apiScroll, GUILayout.MinHeight(280));
            string filter = (_apiFilter ?? "").Trim().ToLowerInvariant();
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e == null || string.IsNullOrEmpty(e.id))
                    continue;
                if (!string.IsNullOrEmpty(filter)
                    && e.id.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0
                    && (e.module == null || e.module.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0))
                    continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(e.id, GUILayout.Height(18), GUILayout.Width(260));
                GUILayout.Label(e.module ?? "", GUILayout.Width(90));
                GUILayout.Label(e.authRequired ? "auth" : "open", GUILayout.Width(40));
                if (GUILayout.Button("Copy", GUILayout.Width(52)))
                    EditorGUIUtility.systemCopyBuffer = e.id;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Canonical public types", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Wallet → IVXNWalletManager");
            EditorGUILayout.LabelField("Leaderboard → IVXNLeaderbordManager");
            EditorGUILayout.LabelField("Optional → com.intelliversex.sdk.ai / .discord / .photon");
        }

        private void DrawCheck()
        {
            EditorGUILayout.LabelField("1. Check", EditorStyles.boldLabel);

            bool newtonsoft = TypeExists("Newtonsoft.Json.JsonConvert");
            bool nakama = TypeExists("Nakama.Client");
            var validation = IVXProjectSetup.RunValidation();
            int failed = 0;
            for (int i = 0; i < validation.Count; i++)
            {
                if (!validation[i].Passed && !validation[i].IsWarning)
                    failed++;
            }

            DrawStatusRow("JSON (Newtonsoft)", newtonsoft);
            DrawStatusRow("Nakama client", nakama);
            DrawStatusRow("Project settings", failed == 0);
            DrawStatusRow("Photon (optional)", TypeExists("Photon.Pun.PhotonNetwork"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fix project settings", GUILayout.Height(28)))
            {
                IVXProjectSetup.QuickValidate();
                EditorUtility.DisplayDialog(
                    WindowTitle,
                    failed == 0
                        ? "Project checks passed."
                        : "Some checks failed. Open Advanced Setup > Platform Validation to apply fixes.",
                    "OK");
            }

            if (GUILayout.Button("Install dependencies", GUILayout.Height(28)))
                IVXAdvancedSetup.ShowWindow();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConnect()
        {
            EditorGUILayout.LabelField("2. Connect", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Paste the Game ID and Nakama host from your dashboard. Leave host as 127.0.0.1 for a local server.",
                MessageType.Info);

            if (_config == null)
            {
                if (GUILayout.Button("Create connection file", GUILayout.Height(32)))
                    CreateConfigAsset();
                return;
            }

            if (_configSo == null)
                _configSo = new SerializedObject(_config);

            _configSo.Update();
            EditorGUILayout.PropertyField(_configSo.FindProperty("_gameId"), new GUIContent("Game ID"));
            EditorGUILayout.PropertyField(_configSo.FindProperty("_gameName"), new GUIContent("Game name"));
            EditorGUILayout.PropertyField(_configSo.FindProperty("_serverHost"), new GUIContent("Server host"));
            EditorGUILayout.PropertyField(_configSo.FindProperty("_serverPort"), new GUIContent("Server port"));
            EditorGUILayout.PropertyField(_configSo.FindProperty("_serverKey"), new GUIContent("Server key"));
            EditorGUILayout.PropertyField(_configSo.FindProperty("_useSSL"), new GUIContent("Use SSL"));
            if (_configSo.ApplyModifiedProperties())
                EditorUtility.SetDirty(_config);

            EditorGUILayout.ObjectField("Config asset", _config, typeof(IVXBootstrapConfig), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping server", GUILayout.Height(28)))
                PingServer();
            if (GUILayout.Button("Select config", GUILayout.Height(28)))
            {
                Selection.activeObject = _config;
                EditorGUIUtility.PingObject(_config);
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_serverPing))
                EditorGUILayout.HelpBox(_serverPing, _serverPingType);

            if (string.IsNullOrWhiteSpace(_config.GameId))
            {
                EditorGUILayout.HelpBox(
                    "Game ID is empty. The SDK will not initialize until you paste one.",
                    MessageType.Warning);
            }
        }

        private void DrawPlay()
        {
            EditorGUILayout.LabelField("3. Play", EditorStyles.boldLabel);

            bool hasBootstrap = FindBootstrapInOpenScenes() != null;
            DrawStatusRow("Bootstrap in this scene", hasBootstrap);

            if (GUILayout.Button("Add bootstrap to this scene", GUILayout.Height(32)))
                AddBootstrapToScene();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Install demo scenes", GUILayout.Height(28)))
                IVXConsumerAssetInstaller.InstallDemoScenesOnly();
            if (GUILayout.Button("Advanced setup", GUILayout.Height(28)))
                IVXAdvancedSetup.ShowWindow();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Press Unity Play. Traffic tab shows IVXRequestBus calls. Photon is optional and not required.",
                MessageType.None);
        }

        private void DrawFooter()
        {
            EditorGUILayout.LabelField("Unity CLI (preferred over MCP)", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel(
                "unity open .   unity test --mode EditMode   unity build --target Android",
                EditorStyles.textField,
                GUILayout.Height(18));
        }

        private static void DrawStatusRow(string label, bool ok)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ok ? "Ready" : (label.Contains("Photon") ? "Optional" : "Needs work"), GUILayout.Width(88));
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
        }

        private void FindOrLoadConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:IVXBootstrapConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _config = AssetDatabase.LoadAssetAtPath<IVXBootstrapConfig>(path);
                _configSo = _config != null ? new SerializedObject(_config) : null;
                return;
            }

            _config = null;
            _configSo = null;
        }

        private void CreateConfigAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/IntelliVerseX"))
                AssetDatabase.CreateFolder("Assets", "IntelliVerseX");

            if (!AssetDatabase.IsValidFolder(GeneratedConfigFolder))
                AssetDatabase.CreateFolder("Assets/IntelliVerseX", "Generated");

            var asset = CreateInstance<IVXBootstrapConfig>();
            AssetDatabase.CreateAsset(asset, GeneratedConfigPath);
            AssetDatabase.SaveAssets();
            _config = asset;
            _configSo = new SerializedObject(_config);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void PingServer()
        {
            if (_config == null)
            {
                _serverPing = "Create a connection file first.";
                _serverPingType = MessageType.Warning;
                return;
            }

            string host = _config.ServerHost;
            int port = _config.ServerPort;
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool ok = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (!ok)
                    {
                        _serverPing = $"No response from {host}:{port}. Start Nakama or check host/port.";
                        _serverPingType = MessageType.Warning;
                        return;
                    }

                    client.EndConnect(result);
                    _serverPing = $"Reached {host}:{port}.";
                    _serverPingType = MessageType.Info;
                }
            }
            catch (Exception ex)
            {
                _serverPing = $"Could not reach {host}:{port}. {ex.Message}";
                _serverPingType = MessageType.Warning;
            }
        }

        private static IVXBootstrap FindBootstrapInOpenScenes()
        {
            var found = FindObjectsByType<IVXBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return found != null && found.Length > 0 ? found[0] : null;
        }

        private void AddBootstrapToScene()
        {
            if (_config == null)
            {
                EditorUtility.DisplayDialog(WindowTitle, "Create a connection file in step 2 first.", "OK");
                return;
            }

            var existing = FindBootstrapInOpenScenes();
            IVXBootstrap bootstrap = existing;
            if (bootstrap == null)
            {
                var go = new GameObject("IVX Bootstrap");
                bootstrap = go.AddComponent<IVXBootstrap>();
                Undo.RegisterCreatedObjectUndo(go, "Add IVX Bootstrap");
            }

            var so = new SerializedObject(bootstrap);
            SerializedProperty configProp = so.FindProperty("_config");
            if (configProp != null)
            {
                configProp.objectReferenceValue = _config;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[IVX] Bootstrap added. Assign IVXBootstrapConfig on the component in the Inspector.");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = bootstrap.gameObject;
        }

        private static bool TypeExists(string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (assembly.GetType(fullTypeName, false) != null)
                        return true;
                }
                catch (FileNotFoundException)
                {
                }
                catch (TypeLoadException)
                {
                }
            }

            return false;
        }
    }
}

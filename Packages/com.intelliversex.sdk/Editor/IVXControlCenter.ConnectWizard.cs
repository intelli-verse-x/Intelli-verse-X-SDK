using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Identity;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// World-class Connect wizard: Auth V2 login → refresh → unique App ID → bootstrap Game ID.
    /// Passwords and bearer tokens stay in-memory only; email/recent IDs may be remembered in EditorPrefs.
    /// </summary>
    public sealed partial class IVXControlCenter
    {
        // Auth V2 + unique-appid wizard state (editor-only; never persist secrets)
        private string _loginEmail = "";
        private string _loginPassword = "";
        private string _wizardAccessToken = "";
        private string _wizardRefreshToken = "";
        private string _wizardIdpUsername = "";
        private string _wizardUserLabel = "";
        private string _wizardDisplayName = "";
        private string _wizardGroupsLabel = "";
        private long _wizardTokenExpiryEpoch;
        private string _createGameName = "";
        private bool _wizardBusy;
        private string _wizardBusyLabel = "";
        private string _wizardStatus = "";
        private MessageType _wizardStatusType = MessageType.None;
        private CancellationTokenSource _wizardCts;
        private bool _manualGameIdFoldout;
        private bool _serverFoldout;
        private string _lastCreatedAppId = "";
        private bool _focusConnectBanner;
        private int _recentGamesPopupIndex;
        private string _renameGameName = "";

        private bool IsWizardLoggedIn => !string.IsNullOrWhiteSpace(_wizardAccessToken);

        private bool IsWizardTokenExpired
        {
            get
            {
                if (_wizardTokenExpiryEpoch <= 0)
                    return false;
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return now + 30 >= _wizardTokenExpiryEpoch;
            }
        }

        private bool CanRefreshWizardToken =>
            !string.IsNullOrWhiteSpace(_wizardRefreshToken)
            && !string.IsNullOrWhiteSpace(_wizardIdpUsername);

        private void DrawConnect()
        {
            EditorGUILayout.LabelField("2. Connect", EditorStyles.boldLabel);
            DrawConnectProgress();

            if (_focusConnectBanner)
            {
                EditorGUILayout.HelpBox(
                    "First-run: your Game ID is empty. Sign in below, name the game, and create a unique App ID.",
                    MessageType.Info);
            }

            if (_config == null)
            {
                EditorGUILayout.HelpBox(
                    "Create a bootstrap connection file once. The wizard writes your Game ID into it.",
                    MessageType.Info);
                if (GUILayout.Button("Create connection file", GUILayout.Height(34)))
                    CreateConfigAsset();
                return;
            }

            if (_configSo == null)
                _configSo = new SerializedObject(_config);

            HandleConnectKeyboardShortcuts();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawConnectAccountCard();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawConnectRegisterCard();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);
            DrawRecentGamesPicker();

            if (!string.IsNullOrEmpty(_wizardStatus))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(_wizardStatus, _wizardStatusType);
            }

            if (_wizardBusy)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(BusyLabelWithPulse(_wizardBusyLabel), EditorStyles.miniLabel);
                if (GUILayout.Button("Cancel", GUILayout.Width(72), GUILayout.Height(22)))
                {
                    CancelWizardWork();
                    SetWizardStatus("Cancelled.", MessageType.Warning);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (!string.IsNullOrWhiteSpace(_lastCreatedAppId))
                DrawCreatedSuccessActions();

            EditorGUILayout.Space(10);
            DrawManualAndServerSections();
        }

        private void DrawConnectProgress()
        {
            bool hasConfig = _config != null;
            bool loggedIn = IsWizardLoggedIn && !IsWizardTokenExpired;
            bool hasGameId = _config != null && IVXConnectWizardValidation.IsValidUuid(_config.GameId);

            EditorGUILayout.BeginHorizontal();
            DrawProgressChip("1 Config", hasConfig);
            DrawProgressChip("2 Sign in", loggedIn);
            DrawProgressChip("3 Game ID", hasGameId);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Developers", EditorStyles.miniButton, GUILayout.Width(84)))
                Application.OpenURL(IVXConnectWizardValidation.DevelopersUrl);
            if (GUILayout.Button("UITK", EditorStyles.miniButton, GUILayout.Width(48)))
                IVXConnectToolkitWindow.ShowWindow();
            EditorGUILayout.EndHorizontal();

            string tip = !hasConfig
                ? "Start by creating the connection file."
                : !loggedIn
                    ? "Sign in with your IntelliVerse account, then name your game."
                    : !hasGameId
                        ? "Name your game and create a unique App ID — or pick a recent / pasted ID."
                        : "Connected. Ping Nakama if you use a local server, then continue to Play.";
            EditorGUILayout.LabelField(tip, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4);
        }

        private static void DrawProgressChip(string label, bool done)
        {
            var prev = GUI.color;
            GUI.color = done ? new Color(0.55f, 0.9f, 0.6f) : new Color(0.75f, 0.75f, 0.75f);
            GUILayout.Label(done ? "● " + label : "○ " + label, EditorStyles.miniLabel, GUILayout.Width(78));
            GUI.color = prev;
        }

        private void DrawConnectAccountCard()
        {
            EditorGUILayout.LabelField("IntelliVerse account", EditorStyles.boldLabel);

            if (IsWizardLoggedIn)
            {
                if (IsWizardTokenExpired)
                {
                    if (CanRefreshWizardToken)
                    {
                        EditorGUILayout.HelpBox(
                            "Access token expired. Refresh to continue without re-entering your password.",
                            MessageType.Warning);
                        EditorGUI.BeginDisabledGroup(_wizardBusy);
                        if (GUILayout.Button("Refresh session", GUILayout.Height(28)))
                            _ = RefreshWizardTokenAsync(force: true);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            "Your session expired. Sign in again to create or manage a Game ID.",
                            MessageType.Warning);
                        _wizardAccessToken = string.Empty;
                        _wizardTokenExpiryEpoch = 0;
                    }
                }
                else
                {
                    string who = string.IsNullOrWhiteSpace(_wizardDisplayName)
                        ? _wizardUserLabel
                        : _wizardDisplayName + " · " + _wizardUserLabel;
                    if (string.IsNullOrWhiteSpace(who))
                        who = "Signed in";

                    EditorGUILayout.HelpBox("Signed in as " + who + ".", MessageType.Info);
                    if (!string.IsNullOrWhiteSpace(_wizardGroupsLabel))
                        EditorGUILayout.LabelField("Roles: " + _wizardGroupsLabel, EditorStyles.miniLabel);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(_wizardBusy);
                    if (GUILayout.Button("Sign out", GUILayout.Height(26)))
                        SignOutWizard();
                    EditorGUI.BeginDisabledGroup(!CanRefreshWizardToken);
                    if (GUILayout.Button("Refresh token", GUILayout.Height(26)))
                        _ = RefreshWizardTokenAsync(force: true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.EndDisabledGroup();
                    GUILayout.FlexibleSpace();
                    if (_wizardTokenExpiryEpoch > 0)
                    {
                        var left = TimeSpan.FromSeconds(Math.Max(0, _wizardTokenExpiryEpoch - DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                        EditorGUILayout.LabelField(
                            "Token ~" + FormatRemaining(left),
                            EditorStyles.miniLabel,
                            GUILayout.Width(110));
                    }
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }

            EditorGUILayout.LabelField(
                "Same Auth V2 login your game uses at runtime. Password is never saved.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUI.BeginDisabledGroup(_wizardBusy);
            _loginEmail = EditorGUILayout.TextField(
                new GUIContent("Email", "IntelliVerse account email"),
                _loginEmail);
            _loginPassword = EditorGUILayout.PasswordField(
                new GUIContent("Password", "Never stored in the project or EditorPrefs"),
                _loginPassword);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_wizardBusy || !IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out _));
            GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);
            if (GUILayout.Button(_wizardBusy && _wizardBusyLabel.StartsWith("Sign", StringComparison.Ordinal)
                    ? BusyLabelWithPulse("Signing in")
                    : "Sign in",
                GUILayout.Height(32)))
            {
                _ = SignInWizardAsync();
            }
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create account", EditorStyles.miniButton))
                Application.OpenURL(IVXConnectWizardValidation.SignupUrl);
            if (GUILayout.Button("Forgot password", EditorStyles.miniButton))
                Application.OpenURL(IVXConnectWizardValidation.ForgotPasswordUrl);
            EditorGUILayout.EndHorizontal();

            if (!IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out string blockReason) && !_wizardBusy)
                EditorGUILayout.LabelField(blockReason, EditorStyles.miniLabel);
        }

        private void DrawConnectRegisterCard()
        {
            EditorGUILayout.LabelField("Create unique App ID", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Registers a new Game ID for this Unity project and writes it into your bootstrap config.",
                EditorStyles.wordWrappedMiniLabel);

            bool canCreate = IsWizardLoggedIn && !IsWizardTokenExpired && !_wizardBusy;
            EditorGUI.BeginDisabledGroup(!canCreate);
            _createGameName = EditorGUILayout.TextField(
                new GUIContent("Game name", "Shown on the IntelliVerse platform; 2–80 characters"),
                _createGameName);

            int len = (_createGameName ?? string.Empty).Trim().Length;
            EditorGUILayout.LabelField(
                len + " / " + IVXConnectWizardValidation.MaxGameNameLength + " characters",
                EditorStyles.miniLabel);

            GUI.backgroundColor = canCreate && IVXConnectWizardValidation.TryValidateGameName(_createGameName, out _)
                ? new Color(0.45f, 0.9f, 0.55f)
                : Color.white;
            string createLabel = _wizardBusy && _wizardBusyLabel.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0
                ? BusyLabelWithPulse("Creating unique App ID")
                : "Create unique App ID";
            if (GUILayout.Button(createLabel, GUILayout.Height(34)))
                _ = CreateUniqueAppIdWizardAsync();
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            if (!IsWizardLoggedIn)
            {
                EditorGUILayout.HelpBox("Sign in above first.", MessageType.Warning);
            }
            else if (!IVXConnectWizardValidation.TryValidateGameName(_createGameName, out string nameError))
            {
                EditorGUILayout.LabelField(nameError, EditorStyles.miniLabel);
            }
            else if (_config != null && !string.IsNullOrWhiteSpace(_config.GameId))
            {
                EditorGUILayout.HelpBox(
                    "A Game ID is already set. Creating a new one will ask before replacing it.",
                    MessageType.None);
            }
        }

        private void DrawRecentGamesPicker()
        {
            var recent = IVXConnectWizardValidation.LoadRecentGames();
            EditorGUILayout.LabelField("Recent Game IDs (this Editor)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "No cloud list API is published yet — recent creates/pastes on this machine are kept locally.",
                EditorStyles.wordWrappedMiniLabel);

            string[] labels = IVXConnectWizardValidation.RecentGamePopupLabels(recent);
            EditorGUI.BeginChangeCheck();
            int next = EditorGUILayout.Popup("Apply recent", _recentGamesPopupIndex, labels);
            if (EditorGUI.EndChangeCheck())
            {
                _recentGamesPopupIndex = next;
                if (recent.Length > 0 && next > 0 && next <= recent.Length)
                {
                    var entry = recent[next - 1];
                    ApplyGameIdToConfig(entry.id, entry.name, remember: true, runSmoke: true);
                    SetWizardStatus("Applied recent Game ID:\n" + entry.id, MessageType.Info);
                    _recentGamesPopupIndex = 0;
                }
            }
        }

        private void DrawCreatedSuccessActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Latest unique App ID", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(_lastCreatedAppId, EditorStyles.textField, GUILayout.Height(18));

            EditorGUILayout.LabelField("Display name (local bootstrap)", EditorStyles.miniBoldLabel);
            _renameGameName = EditorGUILayout.TextField(_renameGameName);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save display name", GUILayout.Height(26)))
            {
                if (IVXConnectWizardValidation.TryValidateGameName(_renameGameName, out string err))
                {
                    ApplyGameNameOnly(_renameGameName.Trim());
                    IVXConnectWizardValidation.RememberRecentGame(_lastCreatedAppId, _renameGameName.Trim());
                    SetWizardStatus(
                        "Local display name saved. Platform title changes are managed in the Developers portal.",
                        MessageType.Info);
                }
                else
                {
                    SetWizardStatus(err, MessageType.Error);
                }
            }
            if (GUILayout.Button("Copy Game ID", GUILayout.Height(26)))
            {
                EditorGUIUtility.systemCopyBuffer = _lastCreatedAppId;
                SetWizardStatus("Game ID copied to clipboard.", MessageType.Info);
            }
            if (GUILayout.Button("Select config", GUILayout.Height(26)))
            {
                Selection.activeObject = _config;
                EditorGUIUtility.PingObject(_config);
            }
            if (GUILayout.Button("Edit on web", GUILayout.Height(26)))
                Application.OpenURL(IVXConnectWizardValidation.DevelopersUrl);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawManualAndServerSections()
        {
            _configSo.Update();

            bool manual = EditorGUILayout.BeginFoldoutHeaderGroup(
                _manualGameIdFoldout,
                "I already have a Game ID");
            if (manual != _manualGameIdFoldout)
            {
                _manualGameIdFoldout = manual;
                EditorPrefs.SetBool(IVXConnectWizardValidation.PrefManualFoldout, _manualGameIdFoldout);
            }

            if (_manualGameIdFoldout)
            {
                EditorGUILayout.PropertyField(
                    _configSo.FindProperty("_gameId"),
                    new GUIContent("Game ID", "Paste a UUID from the dashboard or a previous create."));
                EditorGUILayout.PropertyField(_configSo.FindProperty("_gameName"), new GUIContent("Game name"));

                string gid = _configSo.FindProperty("_gameId")?.stringValue ?? "";
                if (!string.IsNullOrWhiteSpace(gid) && !IVXConnectWizardValidation.IsValidUuid(gid))
                {
                    EditorGUILayout.HelpBox(
                        "Game ID should look like a UUID (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).",
                        MessageType.Warning);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(gid));
                if (GUILayout.Button("Copy", GUILayout.Width(64)))
                    EditorGUIUtility.systemCopyBuffer = gid.Trim();
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Paste from clipboard", GUILayout.Width(150)))
                {
                    string clip = (EditorGUIUtility.systemCopyBuffer ?? "").Trim();
                    if (IVXConnectWizardValidation.IsValidUuid(clip))
                    {
                        string name = _configSo.FindProperty("_gameName")?.stringValue ?? "";
                        ApplyGameIdToConfig(clip, name, remember: true, runSmoke: true);
                        SetWizardStatus("Pasted Game ID from clipboard.", MessageType.Info);
                    }
                    else
                    {
                        SetWizardStatus("Clipboard does not contain a UUID Game ID.", MessageType.Warning);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            bool server = EditorGUILayout.BeginFoldoutHeaderGroup(
                _serverFoldout,
                "Nakama server");
            if (server != _serverFoldout)
            {
                _serverFoldout = server;
                EditorPrefs.SetBool(IVXConnectWizardValidation.PrefServerFoldout, _serverFoldout);
            }

            if (_serverFoldout)
            {
                EditorGUILayout.PropertyField(_configSo.FindProperty("_serverHost"), new GUIContent("Server host"));
                EditorGUILayout.PropertyField(_configSo.FindProperty("_serverPort"), new GUIContent("Server port"));
                EditorGUILayout.PropertyField(_configSo.FindProperty("_serverKey"), new GUIContent("Server key"));
                EditorGUILayout.PropertyField(_configSo.FindProperty("_useSSL"), new GUIContent("Use SSL"));
                EditorGUILayout.ObjectField("Config asset", _config, typeof(IVXBootstrapConfig), false);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Ping server", GUILayout.Height(26)))
                    PingServer();
                if (GUILayout.Button("Select config", GUILayout.Height(26)))
                {
                    Selection.activeObject = _config;
                    EditorGUIUtility.PingObject(_config);
                }
                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(_serverPing))
                    EditorGUILayout.HelpBox(_serverPing, _serverPingType);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (_configSo.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_config);
                if (_config != null && IVXConnectWizardValidation.IsValidUuid(_config.GameId))
                    ApplyGameIdToRuntime(_config.GameId);
            }

            if (string.IsNullOrWhiteSpace(_config.GameId))
            {
                EditorGUILayout.HelpBox(
                    "No Game ID yet — sign in and create one, or expand “I already have a Game ID”.",
                    MessageType.Warning);
            }
            else if (IVXConnectWizardValidation.IsValidUuid(_config.GameId))
            {
                DrawStatusRow("Game ID", true);
            }
        }

        private void HandleConnectKeyboardShortcuts()
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown)
                return;
            if (e.keyCode != KeyCode.Return && e.keyCode != KeyCode.KeypadEnter)
                return;
            if (_wizardBusy)
                return;

            if (!IsWizardLoggedIn)
            {
                if (IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out _))
                {
                    _ = SignInWizardAsync();
                    e.Use();
                }
            }
            else if (IVXConnectWizardValidation.TryValidateGameName(_createGameName, out _))
            {
                _ = CreateUniqueAppIdWizardAsync();
                e.Use();
            }
        }

        private void LoadConnectWizardPrefs()
        {
            if (string.IsNullOrWhiteSpace(_loginEmail))
                _loginEmail = EditorPrefs.GetString(IVXConnectWizardValidation.PrefEmail, string.Empty) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_createGameName))
                _createGameName = EditorPrefs.GetString(IVXConnectWizardValidation.PrefGameName, string.Empty) ?? string.Empty;
            _manualGameIdFoldout = EditorPrefs.GetBool(IVXConnectWizardValidation.PrefManualFoldout, false);
            _serverFoldout = EditorPrefs.GetBool(IVXConnectWizardValidation.PrefServerFoldout, false);
            _focusConnectBanner = EditorPrefs.GetBool(IVXConnectWizardValidation.PrefFocusConnect, false);
            if (_focusConnectBanner)
                EditorPrefs.DeleteKey(IVXConnectWizardValidation.PrefFocusConnect);
        }

        private void PrefillCreateGameNameIfEmpty()
        {
            if (!string.IsNullOrWhiteSpace(_createGameName))
                return;
            if (_config != null && !string.IsNullOrWhiteSpace(_config.GameName))
            {
                _createGameName = _config.GameName.Trim();
                return;
            }

            string product = Application.productName;
            if (!string.IsNullOrWhiteSpace(product)
                && !string.Equals(product, "My project", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(product, "My Project", StringComparison.OrdinalIgnoreCase))
            {
                _createGameName = product.Trim();
            }
        }

        private void TryHydrateWizardSessionFromMemory()
        {
            if (!string.IsNullOrWhiteSpace(_wizardAccessToken))
                return;
            if (!UserSessionManager.HasSession)
                return;

            _wizardAccessToken = UserSessionManager.AccessToken;
            _wizardTokenExpiryEpoch = UserSessionManager.Current?.accessTokenExpiryEpoch ?? 0;
            _wizardRefreshToken = UserSessionManager.Current?.refreshToken ?? "";
            _wizardIdpUsername = UserSessionManager.Current?.idpUsername ?? "";
            var session = UserSessionManager.Current;
            if (session == null)
                return;

            _wizardUserLabel = !string.IsNullOrWhiteSpace(session.email)
                ? session.email
                : session.userId;
            _wizardDisplayName = BuildDisplayName(session.firstName, session.lastName, session.userName);
            _wizardGroupsLabel = TryReadJwtGroups(_wizardAccessToken);
            if (!string.IsNullOrWhiteSpace(session.email))
                _loginEmail = session.email;

            if (CanRefreshWizardToken)
            {
                ConfigureWizardUserAuth();
            }
        }

        private void CancelWizardWork()
        {
            try { _wizardCts?.Cancel(); } catch { /* ignore */ }
            try { _wizardCts?.Dispose(); } catch { /* ignore */ }
            _wizardCts = null;
            _wizardBusy = false;
            _wizardBusyLabel = string.Empty;
        }

        private void SignOutWizard()
        {
            CancelWizardWork();
            _wizardAccessToken = string.Empty;
            _wizardRefreshToken = string.Empty;
            _wizardIdpUsername = string.Empty;
            _wizardUserLabel = string.Empty;
            _wizardDisplayName = string.Empty;
            _wizardGroupsLabel = string.Empty;
            _wizardTokenExpiryEpoch = 0;
            _loginPassword = string.Empty;
            _lastCreatedAppId = string.Empty;
            try { APIManager.ClearUserAuth(); } catch { /* optional */ }
            SetWizardStatus("Signed out. Password and tokens cleared from memory.", MessageType.Info);
            Repaint();
        }

        private void SetWizardStatus(string message, MessageType type)
        {
            _wizardStatus = message ?? string.Empty;
            _wizardStatusType = type;
        }

        private void ConfigureWizardUserAuth()
        {
            if (!CanRefreshWizardToken || string.IsNullOrWhiteSpace(_wizardAccessToken))
                return;

            APIManager.ConfigureUserAuth(
                useUserAuthToken: true,
                idpUsername: _wizardIdpUsername,
                refreshToken: _wizardRefreshToken,
                initialAccessToken: _wizardAccessToken,
                accessTokenExpiresInEpoch: _wizardTokenExpiryEpoch > 0
                    ? _wizardTokenExpiryEpoch
                    : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1800);
        }

        private async Task SignInWizardAsync()
        {
            if (_wizardBusy)
                return;

            if (!IVXConnectWizardValidation.TryValidateSignIn(_loginEmail, _loginPassword, out string block))
            {
                SetWizardStatus(block, MessageType.Error);
                Repaint();
                return;
            }

            string email = _loginEmail.Trim();
            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Signing in";
            SetWizardStatus("Contacting Auth V2…", MessageType.Info);
            Repaint();

            try
            {
                string routeGameId = _config != null && IVXConnectWizardValidation.IsValidUuid(_config.GameId)
                    ? _config.GameId.Trim()
                    : IVXURLs.DefaultGameId;

                var request = new APIManager.LoginRequest
                {
                    email = email,
                    password = _loginPassword,
                    fromDevice = "unity",
                    gameId = routeGameId
                };

                // Configure user-auth so Editor refresh works; do not persist to disk.
                var response = await APIManager.LoginAsync(
                    request,
                    configureUserAuthOnSuccess: true,
                    persistSession: false,
                    ct: ct);

                if (response == null || !response.status || response.data == null)
                {
                    SetWizardStatus(HumanizeApiMessage(response?.message, "Sign-in failed. Check email and password."), MessageType.Error);
                    return;
                }

                string access = !string.IsNullOrWhiteSpace(response.data.accessToken)
                    ? response.data.accessToken
                    : response.data.token;

                if (string.IsNullOrWhiteSpace(access))
                {
                    SetWizardStatus("Sign-in succeeded but no access token came back. Try again or contact support.", MessageType.Error);
                    return;
                }

                _wizardAccessToken = access.Trim();
                _wizardRefreshToken = response.data.refreshToken?.Trim() ?? "";
                _wizardIdpUsername = response.data.user?.idpUsername?.Trim() ?? "";
                _wizardTokenExpiryEpoch = ComputeTokenExpiryEpoch(response.data.expiresIn, _wizardAccessToken);
                _wizardGroupsLabel = TryReadJwtGroups(_wizardAccessToken);

                if (response.data.user != null)
                {
                    _wizardUserLabel = !string.IsNullOrWhiteSpace(response.data.user.email)
                        ? response.data.user.email
                        : response.data.user.userName;
                    _wizardDisplayName = BuildDisplayName(
                        response.data.user.firstName,
                        response.data.user.lastName,
                        response.data.user.userName);
                }
                else
                {
                    _wizardUserLabel = email;
                    _wizardDisplayName = string.Empty;
                }

                _loginPassword = string.Empty;
                EditorPrefs.SetString(IVXConnectWizardValidation.PrefEmail, email);
                PrefillCreateGameNameIfEmpty();
                _focusConnectBanner = false;
                SetWizardStatus("Signed in. Name your game and create a unique App ID.", MessageType.Info);
            }
            catch (OperationCanceledException)
            {
                SetWizardStatus("Sign-in cancelled.", MessageType.Warning);
            }
            catch (Exception ex)
            {
                SetWizardStatus(HumanizeException(ex, "sign-in"), MessageType.Error);
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                Repaint();
            }
        }

        private async Task RefreshWizardTokenAsync(bool force)
        {
            if (_wizardBusy)
                return;
            if (!CanRefreshWizardToken)
            {
                SetWizardStatus("No refresh credentials. Sign in again.", MessageType.Error);
                Repaint();
                return;
            }

            if (!force && !IsWizardTokenExpired)
                return;

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Refreshing session";
            SetWizardStatus("Refreshing access token…", MessageType.Info);
            Repaint();

            try
            {
                ConfigureWizardUserAuth();
                await APIManager.RefreshUserTokenAsync(ct);
                string access = await APIManager.GetUserAccessTokenAsync(ct);
                if (string.IsNullOrWhiteSpace(access))
                    throw new Exception("Refresh returned an empty access token.");

                _wizardAccessToken = access.Trim();
                _wizardTokenExpiryEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1800;
                _wizardGroupsLabel = TryReadJwtGroups(_wizardAccessToken);
                if (TryReadJwtExp(_wizardAccessToken, out long exp) && exp > 0)
                    _wizardTokenExpiryEpoch = exp;

                SetWizardStatus("Session refreshed.", MessageType.Info);
            }
            catch (OperationCanceledException)
            {
                SetWizardStatus("Refresh cancelled.", MessageType.Warning);
            }
            catch (Exception ex)
            {
                _wizardAccessToken = string.Empty;
                SetWizardStatus(HumanizeException(ex, "token refresh") + " Sign in again.", MessageType.Error);
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                Repaint();
            }
        }

        private async Task CreateUniqueAppIdWizardAsync()
        {
            if (_wizardBusy)
                return;

            if (IsWizardTokenExpired && CanRefreshWizardToken)
            {
                await RefreshWizardTokenAsync(force: true);
                if (!IsWizardLoggedIn || IsWizardTokenExpired)
                    return;
            }

            if (!IsWizardLoggedIn || IsWizardTokenExpired)
            {
                if (IsWizardTokenExpired)
                    _wizardAccessToken = string.Empty;
                SetWizardStatus("Sign in first — then create a unique App ID.", MessageType.Error);
                Repaint();
                return;
            }

            if (!IVXConnectWizardValidation.TryValidateGameName(_createGameName, out string nameError))
            {
                SetWizardStatus(nameError, MessageType.Error);
                Repaint();
                return;
            }

            if (_config == null)
            {
                SetWizardStatus("Create a connection file first.", MessageType.Error);
                Repaint();
                return;
            }

            string gameName = _createGameName.Trim();

            if (!string.IsNullOrWhiteSpace(_config.GameId))
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Replace Game ID?",
                    "Bootstrap already has:\n" + _config.GameId +
                    "\n\nCreating a new unique App ID will replace it in this project’s config.\n" +
                    "The previous ID is not deleted on the server.",
                    "Replace",
                    "Cancel");
                if (!replace)
                    return;
            }

            CancelWizardWork();
            _wizardCts = new CancellationTokenSource();
            CancellationToken ct = _wizardCts.Token;
            _wizardBusy = true;
            _wizardBusyLabel = "Creating unique App ID";
            SetWizardStatus("Registering \"" + gameName + "\"…", MessageType.Info);
            Repaint();

            try
            {
                var response = await APIManager.CreateUniqueAppIdAsync(gameName, _wizardAccessToken, ct);
                string uniqueId = response.data.uniqueAppId.Trim();
                if (!IVXConnectWizardValidation.IsValidUuid(uniqueId))
                {
                    SetWizardStatus("Server returned an unexpected App ID format. Nothing was saved.", MessageType.Error);
                    return;
                }

                ApplyGameIdToConfig(uniqueId, gameName, remember: true, runSmoke: true);
                _lastCreatedAppId = uniqueId;
                _renameGameName = gameName;
                EditorPrefs.SetString(IVXConnectWizardValidation.PrefGameName, gameName);
                _manualGameIdFoldout = true;
                EditorPrefs.SetBool(IVXConnectWizardValidation.PrefManualFoldout, true);
                _focusConnectBanner = false;

                SetWizardStatus(
                    "Unique App ID created, saved, and verified locally.\n" + uniqueId,
                    MessageType.Info);
            }
            catch (OperationCanceledException)
            {
                SetWizardStatus("Create cancelled.", MessageType.Warning);
            }
            catch (Exception ex)
            {
                string msg = HumanizeException(ex, "unique App ID");
                if (IsUnauthorizedMessage(msg))
                {
                    if (CanRefreshWizardToken)
                    {
                        SetWizardStatus("Unauthorized — trying token refresh…", MessageType.Warning);
                        _wizardBusy = false;
                        await RefreshWizardTokenAsync(force: true);
                        if (IsWizardLoggedIn && !IsWizardTokenExpired)
                        {
                            SetWizardStatus("Session refreshed. Click Create again.", MessageType.Info);
                            return;
                        }
                    }

                    _wizardAccessToken = string.Empty;
                    _wizardTokenExpiryEpoch = 0;
                    msg = "Session expired or unauthorized. Sign in again, then retry.";
                }

                SetWizardStatus(msg, MessageType.Error);
            }
            finally
            {
                _wizardBusy = false;
                _wizardBusyLabel = string.Empty;
                Repaint();
            }
        }

        private void ApplyGameIdToConfig(string gameId, string gameName, bool remember, bool runSmoke)
        {
            if (_config == null || !IVXConnectWizardValidation.IsValidUuid(gameId))
                return;

            if (_configSo == null)
                _configSo = new SerializedObject(_config);

            _configSo.Update();
            SerializedProperty gameIdProp = _configSo.FindProperty("_gameId");
            SerializedProperty gameNameProp = _configSo.FindProperty("_gameName");
            if (gameIdProp != null)
                gameIdProp.stringValue = gameId.Trim();
            if (gameNameProp != null && !string.IsNullOrWhiteSpace(gameName))
                gameNameProp.stringValue = gameName.Trim();
            _configSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();

            ApplyGameIdToRuntime(gameId);
            if (remember)
                IVXConnectWizardValidation.RememberRecentGame(gameId, gameName);

            if (runSmoke)
                RunPostCreateSmokeCheck(gameId);
        }

        private void ApplyGameNameOnly(string gameName)
        {
            if (_config == null)
                return;
            if (_configSo == null)
                _configSo = new SerializedObject(_config);
            _configSo.Update();
            SerializedProperty gameNameProp = _configSo.FindProperty("_gameName");
            if (gameNameProp != null)
                gameNameProp.stringValue = gameName;
            _configSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }

        private static void ApplyGameIdToRuntime(string gameId)
        {
            if (!IVXConnectWizardValidation.IsValidUuid(gameId))
                return;
            IVXURLs.GameId = gameId.Trim();
        }

        private void RunPostCreateSmokeCheck(string gameId)
        {
            var messages = new StringBuilder();
            bool ok = true;

            if (!IVXConnectWizardValidation.IsValidUuid(gameId))
            {
                ok = false;
                messages.AppendLine("• Game ID is not a valid UUID.");
            }

            if (_config == null || !string.Equals(_config.GameId?.Trim(), gameId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ok = false;
                messages.AppendLine("• Bootstrap config was not updated.");
            }

            if (!string.Equals(IVXURLs.GameId, gameId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ok = false;
                messages.AppendLine("• Runtime IVXURLs.GameId was not applied.");
            }
            else
            {
                messages.AppendLine("• Runtime Game ID applied for Editor API calls.");
            }

            if (_config != null && _config.Validate())
                messages.AppendLine("• Bootstrap Validate() passed.");
            else
            {
                ok = false;
                messages.AppendLine("• Bootstrap Validate() failed.");
            }

            SetWizardStatus(
                (ok ? "Smoke check passed.\n" : "Smoke check found issues.\n") + messages,
                ok ? MessageType.Info : MessageType.Warning);
        }

        private static string BusyLabelWithPulse(string baseLabel)
        {
            int ticks = (int)(EditorApplication.timeSinceStartup * 4) % 4;
            return baseLabel + new string('.', ticks + 1);
        }

        private static string FormatRemaining(TimeSpan span)
        {
            if (span.TotalHours >= 1)
                return ((int)span.TotalHours) + "h " + span.Minutes + "m left";
            if (span.TotalMinutes >= 1)
                return span.Minutes + "m left";
            return Math.Max(0, (int)span.TotalSeconds) + "s left";
        }

        private static string BuildDisplayName(string first, string last, string fallback)
        {
            string f = (first ?? string.Empty).Trim();
            string l = (last ?? string.Empty).Trim();
            if (f.Length == 0 && l.Length == 0)
                return (fallback ?? string.Empty).Trim();
            return (f + " " + l).Trim();
        }

        private static long ComputeTokenExpiryEpoch(int expiresInSeconds, string jwt)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (expiresInSeconds > 0)
                return now + expiresInSeconds;

            if (TryReadJwtExp(jwt, out long exp) && exp > now)
                return exp;

            return now + 1800;
        }

        private static bool TryReadJwtExp(string jwt, out long exp)
        {
            exp = 0;
            if (string.IsNullOrWhiteSpace(jwt))
                return false;

            string[] parts = jwt.Split('.');
            if (parts.Length < 2)
                return false;

            try
            {
                string payload = parts[1]
                    .Replace('-', '+')
                    .Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                Match m = Regex.Match(json, "\"exp\"\\s*:\\s*(\\d+)");
                if (!m.Success)
                    return false;
                return long.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out exp);
            }
            catch
            {
                return false;
            }
        }

        private static string TryReadJwtGroups(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
                return string.Empty;

            string[] parts = jwt.Split('.');
            if (parts.Length < 2)
                return string.Empty;

            try
            {
                string payload = parts[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                // cognito:groups may be an array
                Match array = Regex.Match(json, "\"cognito:groups\"\\s*:\\s*\\[([^\\]]*)\\]");
                if (!array.Success)
                    return string.Empty;

                var matches = Regex.Matches(array.Groups[1].Value, "\"([^\"]+)\"");
                if (matches.Count == 0)
                    return string.Empty;

                var sb = new StringBuilder();
                for (int i = 0; i < matches.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(matches[i].Groups[1].Value);
                }

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsUnauthorizedMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return false;
            return msg.IndexOf("401", StringComparison.Ordinal) >= 0
                   || msg.IndexOf("Unauthorized", StringComparison.OrdinalIgnoreCase) >= 0
                   || msg.IndexOf("invalid token", StringComparison.OrdinalIgnoreCase) >= 0
                   || msg.IndexOf("token expired", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string HumanizeApiMessage(string message, string fallback)
        {
            if (string.IsNullOrWhiteSpace(message))
                return fallback;
            string m = message.Trim();
            if (m.Length > 280)
                m = m.Substring(0, 280) + "…";
            return m;
        }

        private static string HumanizeException(Exception ex, string action)
        {
            if (ex == null)
                return "Unknown error during " + action + ".";

            string raw = ex.Message ?? string.Empty;

            Match jsonMsg = Regex.Match(raw, "\"message\"\\s*:\\s*\"([^\"]+)\"");
            if (jsonMsg.Success)
                return HumanizeApiMessage(jsonMsg.Groups[1].Value, raw);

            if (raw.IndexOf("HTTP 401", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Unauthorized (401). Sign in again.";
            if (raw.IndexOf("HTTP 403", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Forbidden (403). This account may lack permission to create games.";
            if (raw.IndexOf("HTTP 409", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Conflict (409). Try a different game name.";
            if (raw.IndexOf("HTTP 429", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Too many requests. Wait a moment and try again.";
            if (raw.IndexOf("HTTP 5", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Server error. Try again in a minute.";
            if (raw.IndexOf("Cannot resolve", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("ConnectionError", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("No connection", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Network problem during " + action + ". Check internet connectivity.";

            return HumanizeApiMessage(raw, "Something went wrong during " + action + ".");
        }
    }
}
